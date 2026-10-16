using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Forms=System.Windows.Forms;
namespace GujiApp {
 public class PetWindow:Window {
  public Settings Config;public PetVisual View;public ReminderEngine Reminders;public Shell Panels;
  public bool IsTest,Busy;bool feeding,releasingCapture,feedingMoved;Point feedingStart;public int FrameCount;public string ReminderKind="";public Window ReminderWindow;
  readonly DispatcherTimer clock=new DispatcherTimer(),animation=new DispatcherTimer();readonly Stopwatch elapsed=Stopwatch.StartNew();double lastFrame,activeUntil,actionEnd,walkEnd,walkDirection,fallLeft,fallSpeed,bounceVelocity,pokes;WanderSchedule wandering;Point previousHover;double hoverAt;bool waitingClick;double pressTime;Point pressLocal;
  readonly Random rng=new Random();Forms.NotifyIcon tray;Outfit beforeSleep;double originalLeft,originalTop;double playAge=-1,playOriginLeft,playSign;
  public PetWindow(Settings settings,bool test){IsTest=test;Config=settings;beforeSleep=settings.DayWear;View=new PetVisual(settings);View.Sleeping=settings.Sleeping;View.InBed=settings.Sleeping||settings.BedPrepared;View.LampOn=!settings.Sleeping;Title=settings.PetName+" · 桌面小伙伴";WindowStyle=WindowStyle.None;AllowsTransparency=true;Background=null;ResizeMode=ResizeMode.NoResize;ShowInTaskbar=false;Topmost=settings.Topmost;Width=360*settings.Size;Height=400*settings.Size;
   var box=new Viewbox{Child=View,Stretch=System.Windows.Media.Stretch.Fill};Content=box;
   var area=SystemParameters.WorkArea;Left=double.IsNaN(settings.Left)?area.Right-Width-20:settings.Left;Top=double.IsNaN(settings.Top)?area.Bottom-Height:settings.Top;
   Reminders=new ReminderEngine(DateTime.Now,Config);Panels=new Shell(this);
   SourceInitialized+=(s,e)=>{Clamp();var source=HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);source.AddHook(HitHook);if(!test)CreateTray();};
   Loaded+=(s,e)=>{if(!test){clock.Start();animation.Start();if(!settings.Onboarded)Dispatcher.BeginInvoke(new Action(()=>Panels.Intro()));else Greet();}};
   MouseLeftButtonDown+=Press;MouseMove+=Move;MouseLeftButtonUp+=Release;
   LostMouseCapture+=(s,e)=>{if(!releasingCapture)CancelCapture();};
   MouseRightButtonUp+=(s,e)=>{Panels.Context().IsOpen=true;e.Handled=true;};
   KeyDown+=(s,e)=>{if(e.Key==Key.Escape){if(Panels.Current!=null)Panels.Current.Close();else Close();}};
   clock.Interval=TimeSpan.FromSeconds(1);clock.Tick+=(s,e)=>Tick(DateTime.Now);
   animation.Interval=TimeSpan.FromMilliseconds(33);animation.Tick+=(s,e)=>Frame();
   wandering=new WanderSchedule(DateTime.Now,rng);if(!test){Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;Microsoft.Win32.SystemEvents.PowerModeChanged+=PowerChanged;}Closed+=(s,e)=>{clock.Stop();animation.Stop();if(!test){Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;Microsoft.Win32.SystemEvents.PowerModeChanged-=PowerChanged;}if(ReminderWindow!=null)ReminderWindow.Close();if(Panels.Current!=null)Panels.Current.Close();if(tray!=null)tray.Dispose();Config.Left=Left;Config.Top=Top;Config.Sleeping=View.Sleeping;Config.BedPrepared=View.InBed;Config.LastSeen=DateTime.Now;if(!test)Persist();};
  }
  public void RefreshIdentity(){Title=Config.PetName+" · 桌面小伙伴";if(tray!=null)tray.Text=Config.PetName+" · 右键打开小窝";}
  public void Persist(){if(IsTest)return;if(!Store.Save(Config)&&!IsTest)MessageBox.Show("设置暂时未能保存，请检查程序所在文件夹是否可写。\n"+Store.LastError,"保存设置");}
  void DisplayChanged(object sender,EventArgs e){Dispatcher.BeginInvoke(new Action(Clamp));}
  void PowerChanged(object sender,Microsoft.Win32.PowerModeChangedEventArgs e){if(e.Mode==Microsoft.Win32.PowerModes.Resume)Dispatcher.BeginInvoke(new Action(()=>{Reminders.Reset(DateTime.Now,Config);wandering.Reset(DateTime.Now,Config.Wander);walkEnd=0;playAge=-1;}));}
  void Greet(){DateTime now=DateTime.Now;if(now.Date!=Config.LastSeen.Date){if(now.Hour>=5&&now.Hour<22)Wake(false);if(now.Hour>=5&&now.Hour<12){Wake(false);Say(Config.OwnerName+"，早呀！今天也一起加油。",5);}else Say(now.Hour<18?"下午好呀，今天也陪着你。":"晚上好呀，见到你真开心。",5);if(now.Date>Config.Adoption.Date&&now.Month==Config.Adoption.Month&&now.Day==Config.Adoption.Day)Say("今天是我们认识的纪念日，最喜欢你啦！",8);}else Say("你回来啦，我一直在这里。",4);Config.LastSeen=now;}
  public void Say(string text,double seconds){if(IsTest)return;Panels.Speech(text,seconds);}
  public void ActivateMotion(double seconds){activeUntil=Math.Max(activeUntil,elapsed.Elapsed.TotalSeconds+seconds);animation.Interval=TimeSpan.FromMilliseconds(33);}
  Point Local(MouseEventArgs e){Point p=e.GetPosition(this);return new Point(p.X/Config.Size,p.Y/Config.Size);}
  Point CursorPoint(){Native.POINT p;Native.GetCursorPos(out p);var src=PresentationSource.FromVisual(this);return src==null?new Point(p.X,p.Y):src.CompositionTarget.TransformFromDevice.Transform(new Point(p.X,p.Y));}
  void Press(object sender,MouseButtonEventArgs e){Point p=Local(e);if(View.Action=="offerfood"&&new Rect(View.FoodPosition.X-45,View.FoodPosition.Y-35,90,70).Contains(p)){feeding=true;feedingMoved=false;feedingStart=p;CaptureMouse();e.Handled=true;return;}if(View.InBed&&(p.X>280||View.Sleeping)){ToggleLamp();e.Handled=true;return;}
   if((p-new Point(312,348)).Length<=15){Panels.Home();e.Handled=true;return;}
   if(e.ClickCount==2){View.Happy=2.8;View.Bounce=.12;ActivateMotion(3);Say("抱抱！最喜欢你啦。",3);return;}
   if(Busy)return;walkEnd=0;playAge=-1;pressLocal=p;waitingClick=true;pressTime=elapsed.Elapsed.TotalSeconds;View.Drag.Begin(CursorPoint(),p,Config.Size);fallLeft=0;originalLeft=Left;originalTop=Top;CaptureMouse();ActivateMotion(2);e.Handled=true;
  }
  void Move(object sender,MouseEventArgs e){Point p=Local(e);if(feeding){feedingMoved|=(p-feedingStart).Length>3;View.FoodPosition=p;ActivateMotion(1);View.InvalidateVisual();if((p-View.Position(new Point(180,278))).Length<35){feeding=false;ReleaseOwnedCapture();Eat(View.FoodIndex);}return;}if(View.Drag.Phase!=DragPhase.Rest){Vector shift=View.Drag.Move(CursorPoint());Left+=shift.X;Top+=shift.Y;ActivateMotion(1);View.InvalidateVisual();}else if(IsMouseOver&&!View.Sleeping){walkEnd=0;double time=elapsed.Elapsed.TotalSeconds;if((p-previousHover).Length>5&&time-hoverAt>.18){previousHover=p;hoverAt=time;View.Happy=.6;ActivateMotion(1);}}}
  void Release(object sender,MouseButtonEventArgs e){if(feeding){bool click=!feedingMoved;feeding=false;View.FoodPosition=new Point(93,320);ReleaseOwnedCapture();if(click)Eat(View.FoodIndex);else View.InvalidateVisual();return;}if(View.Drag.Phase==DragPhase.Rest)return;bool moved=View.Drag.Moved;EndDrag();ReleaseOwnedCapture();if(waitingClick&&!moved){waitingClick=false;Point p=Local(e);if(View.SecretBagHit(p)){Panels.Note();return;}if(View.Rice&&new Rect(123,265,40,30).Contains(p)){View.Rice=false;View.Happy=1;Say("谢谢你帮我擦干净，嘿嘿。",3);}else{pokes++;View.Poke=1;View.PokeAt=p;View.Bounce=-.06;ActivateMotion(2);if(pokes>=4){pokes=0;View.Happy=.8;Say("有点痒痒的呀～",2);} }} }
  void ReleaseOwnedCapture(){releasingCapture=true;try{base.ReleaseMouseCapture();}finally{releasingCapture=false;}}
  public void CancelCapture(){if(feeding){feeding=false;View.FoodPosition=new Point(93,320);View.InvalidateVisual();}if(View.Drag.Phase!=DragPhase.Rest)EndDrag();waitingClick=false;}
  void EndDrag(){bool lifted=View.Drag.Phase==DragPhase.Lifted;View.Drag.End();if(lifted){fallLeft=24*Config.Size;fallSpeed=0;}ActivateMotion(3);Clamp();}
  public void SetSize(double v){double bottom=Top+Height,center=Left+Width/2;Config.Size=v;Width=360*v;Height=400*v;Left=center-Width/2;Top=bottom-Height;Clamp();View.InvalidateVisual();Persist();}
  public void Apply(Outfit o){Config.Wear=o.Copy();if(Config.CupWithOutfit)Config.Cup=Wardrobe.MatchingCup(o);View.Happy=2;View.Turn=1;View.Bounce=.1;ActivateMotion(3);View.InvalidateVisual();Persist();}
  public void FeedCandy(){Wake(false);if(View.Candy>=3){Say("肚子里装满星星啦，等一会儿嘛。",3);return;}View.Candy++;View.CandyAge=0;View.Happy=2;View.Bounce=.08;ActivateMotion(19);Say("啊呜！你也要闪闪发光呀。",3);}
  public void Drink(){Wake(false);View.Action="water";View.ActionAge=0;actionEnd=elapsed.Elapsed.TotalSeconds+4;View.Happy=4;ActivateMotion(5);Reminders.NextWater=DateTime.Now.AddMinutes(Config.WaterMinutes);Say("咕嘟咕嘟，你也喝一口嘛。",4);}
  public void OfferFood(int index){Wake(false);View.FoodIndex=index;View.FoodPosition=new Point(93,320);View.Action="offerfood";View.Rice=false;ActivateMotion(3);Say("把食物拖到嘴边就能喂我，也可以点一下。",5);View.InvalidateVisual();}
  public void Eat(int index){Wake(false);View.FoodIndex=index;View.Action="food";View.ActionAge=0;View.Rice=false;actionEnd=elapsed.Elapsed.TotalSeconds+4;View.Happy=4;ActivateMotion(5);Say("啊呜！我们一起好好吃饭呀。",4);}
  public void Bed(){if(View.InBed)return;beforeSleep=Config.Wear.Copy();Config.DayWear=beforeSleep.Copy();Config.Wear=Config.SleepWear.Copy();Config.BedPrepared=true;View.InBed=true;View.BedProgress=0;View.LampOn=true;View.Sleeping=false;View.Action="";View.Happy=1;walkEnd=0;ActivateMotion(4);Say("被子铺好啦，点一下小灯就睡觉哦。",5);View.InvalidateVisual();}
  public void ToggleLamp(){if(!View.InBed){Bed();return;}if(View.Sleeping){Wake(true);return;}View.BedProgress=1;View.LampOn=false;ActivateMotion(1);View.Sleeping=true;Config.Sleeping=true;View.Happy=0;View.InvalidateVisual();CloseReminder();Panels.CloseSpeech();Persist();}
  public void Wake(bool speak){if(!View.InBed)return;View.Sleeping=false;Config.Sleeping=false;Config.BedPrepared=false;View.InBed=false;View.Waking=1;View.LampOn=true;if(beforeSleep!=null)Config.Wear=beforeSleep.Copy();Config.DayWear=null;View.Happy=1;ActivateMotion(2);if(speak)Say("唔……醒啦！陪你玩一会儿。",3);View.InvalidateVisual();Persist();}
  public void Stretch(){View.Bounce=.20;View.Happy=2;ActivateMotion(4);Say("一、二、三，伸个懒腰！",4);}
  public void ShowReminder(string kind){CloseReminder();ReminderKind=kind;View.Reminding=kind;ActivateMotion(2);ReminderWindow=Panels.Reminder(kind);}
  public void CloseReminder(){if(ReminderWindow!=null){var w=ReminderWindow;ReminderWindow=null;w.Close();}ReminderKind="";View.Reminding="";View.InvalidateVisual();}
  public void ResetWander(){wandering.Reset(DateTime.Now,Config.Wander);}
  public void Quieten(){walkEnd=0;playAge=-1;if(Config.Focus||Config.Paused){CloseReminder();Panels.CloseSpeech();}}
  public void Recover(){if(!IsVisible)Show();var a=SystemParameters.WorkArea;Left=a.Right-Width-20;Top=a.Bottom-Height-5;Clamp();Persist();}
  public Rect WorkArea(){var src=PresentationSource.FromVisual(this);var from=src==null?Matrix.Identity:src.CompositionTarget.TransformFromDevice;var to=src==null?Matrix.Identity:src.CompositionTarget.TransformToDevice;Point center=to.Transform(new Point(Left+Width/2,Top+Height/2));var r=Forms.Screen.FromPoint(new System.Drawing.Point((int)center.X,(int)center.Y)).WorkingArea;return new Rect(from.Transform(new Point(r.Left,r.Top)),from.Transform(new Point(r.Right,r.Bottom)));}
  public void Clamp(){if(double.IsNaN(Left)||double.IsNaN(Top))return;var src=PresentationSource.FromVisual(this);var matrix=src==null?Matrix.Identity:src.CompositionTarget.TransformFromDevice;var pixel=src==null?Matrix.Identity:src.CompositionTarget.TransformToDevice;Point c=pixel.Transform(new Point(Left+Width/2,Top+Height/2));var sc=Forms.Screen.FromPoint(new System.Drawing.Point((int)c.X,(int)c.Y));var r=sc.WorkingArea;Point a=matrix.Transform(new Point(r.Left,r.Top)),b=matrix.Transform(new Point(r.Right,r.Bottom));Left=Math.Max(a.X,Math.Min(b.X-Width,Left));Top=Math.Max(a.Y,Math.Min(b.Y-Height,Top));}
  void Tick(DateTime now){bool fullscreen=Native.Fullscreen();if(fullscreen||Config.Focus||Busy||!IsVisible){walkEnd=0;playAge=-1;}bool suppress=fullscreen||Busy||!IsVisible||ReminderWindow!=null;string kind=Reminders.Poll(now,Config,suppress);if(kind!=null)ShowReminder(kind);
   int wander=wandering.Poll(now,Config.Wander,!Config.Focus&&!View.Sleeping&&!Busy&&!fullscreen&&IsVisible&&View.Drag.Phase==DragPhase.Rest&&!IsMouseOver&&View.Action==""&&ReminderWindow==null);
   if(wander>0){Rect area=WorkArea();walkDirection=Left<=area.Left+30?1:Left+Width>=area.Right-30?-1:rng.Next(2)==0?-1:1;if(wander==2)Pounce();else walkEnd=elapsed.Elapsed.TotalSeconds+wandering.DurationSeconds;ActivateMotion(10);}
   bool active=elapsed.Elapsed.TotalSeconds<activeUntil||View.Drag.Phase!=DragPhase.Rest||View.Drag.Pull.Length>.05||fallLeft>0;
   animation.Interval=TimeSpan.FromMilliseconds(active?33:View.Sleeping||Config.Focus||fullscreen?800:180);
  }
  void Frame(){FrameCount++;double now=elapsed.Elapsed.TotalSeconds,dt=lastFrame==0?1.0/30:Math.Max(.001,now-lastFrame);lastFrame=now;View.Time=now;View.Waking=Math.Max(0,View.Waking-dt/.7);if(View.InBed)View.BedProgress=Math.Min(1,View.BedProgress+dt/2.8);double lampDelta=(View.LampOn?1.0:0)-View.LampBrightness;View.LampBrightness+=Math.Sign(lampDelta)*Math.Min(Math.Abs(lampDelta),dt*3);double physics=Math.Min(.3,dt);while(physics>0){double h=Math.Min(physics,1.0/120);View.Drag.Step(h);bounceVelocity+=(-View.Bounce*110-bounceVelocity*13)*h;View.Bounce+=bounceVelocity*h;physics-=h;}View.Happy=Math.Max(0,View.Happy-dt);View.Poke=Math.Max(0,View.Poke-dt*3);View.Turn=Math.Max(0,View.Turn-dt*.8);if(!IsTest){Point look=CursorPoint();View.Look=new Vector(Math.Max(-2.5,Math.Min(2.5,(look.X-Left-Width/2)/90)),Math.Max(-1.5,Math.Min(1.5,(look.Y-Top-Height*.65)/120)));}
   if(playAge>=0){playAge+=dt;if(playAge<.35)View.Bounce=-.12*Math.Sin(playAge/.35*Math.PI/2);else if(playAge<1.15){double phase=(playAge-.35)/.8;Left=playOriginLeft+playSign*36*Config.Size*phase;View.Bounce=Math.Sin(phase*Math.PI)*.14;Clamp();}else if(playAge<1.7)View.Bounce=-.25*Math.Sin((playAge-1.15)/.55*Math.PI);else if(playAge>=2.4){playAge=-1;View.Happy=1;}}
   if(Math.Abs(View.Bounce)<.001&&Math.Abs(bounceVelocity)<.005)View.Bounce=0;
   if(fallLeft>0){fallSpeed+=1700*dt;double amount=Math.Min(fallLeft,fallSpeed*dt);Top+=amount;Clamp();fallLeft-=amount;if(fallLeft<=0){View.Bounce=-.17;bounceVelocity=0;}}
   if(View.Candy>0){View.CandyAge+=dt;if(View.CandyAge>=18)View.Candy=0;}
   if(View.Action!=""&&View.Action!="offerfood"){View.ActionAge+=dt;if(now>=actionEnd){if(View.Action=="food"){View.Rice=Foods.Rice(View.FoodIndex);View.Bounce=.10;}View.Action="";}}
   if(walkEnd>now&&!IsMouseOver&&!Config.Focus&&!View.Sleeping){Left+=walkDirection*10*Math.Min(.1,dt);View.Bounce=Math.Sin(now*6)*.035;Clamp();}else walkEnd=0;
   View.InvalidateVisual();
  }
    IntPtr HitHook(IntPtr hwnd,int msg,IntPtr wp,IntPtr lp,ref bool handled){if(msg!=0x84||View.Drag.Phase!=DragPhase.Rest)return IntPtr.Zero;int packed=unchecked((int)lp.ToInt64());Point p=PointFromScreen(new Point((short)(packed&0xffff),(short)((packed>>16)&0xffff)));p=new Point(p.X/Config.Size,p.Y/Config.Size);if(!View.Hit(p)){handled=true;return new IntPtr(-1);}return IntPtr.Zero;}
  public void StartRuntimeForTest(){clock.Start();animation.Start();ActivateMotion(4);}
  public void Pounce(){if(View.Sleeping||Config.Focus)return;playAge=0;playOriginLeft=Left;playSign=CursorPoint().X>Left+Width/2?1:-1;ActivateMotion(4);}
  public void TestTray(){CreateTray();tray.Visible=false;tray.Dispose();tray=null;}
  void CreateTray(){tray=new Forms.NotifyIcon();using(var bitmap=new System.Drawing.Bitmap(System.IO.Path.Combine(Store.Root,"assets","body.png")))using(var small=new System.Drawing.Bitmap(bitmap,new System.Drawing.Size(32,32))){IntPtr h=small.GetHicon();tray.Icon=(System.Drawing.Icon)System.Drawing.Icon.FromHandle(h).Clone();Native.DestroyIcon(h);}tray.Text=Config.PetName+" · 右键打开小窝";tray.Visible=true;tray.MouseClick+=(s,e)=>{if(e.Button==Forms.MouseButtons.Left){if(!IsVisible)Show();Panels.Home();}else if(e.Button==Forms.MouseButtons.Right){var menu=Panels.Context();menu.IsOpen=true;}};}
 }
 public static class Native {
  [StructLayout(LayoutKind.Sequential)]public struct POINT{public int X,Y;}
  [StructLayout(LayoutKind.Sequential)]public struct RECT{public int Left,Top,Right,Bottom;}
  [DllImport("user32.dll")]public static extern bool DestroyIcon(IntPtr h);
  [DllImport("user32.dll")]public static extern bool GetCursorPos(out POINT p);
  [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out RECT r);
  [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
  public static bool Fullscreen(){IntPtr h=GetForegroundWindow();uint id;GetWindowThreadProcessId(h,out id);if(id==(uint)Process.GetCurrentProcess().Id)return false;RECT r;if(!GetWindowRect(h,out r))return false;var screen=Forms.Screen.FromHandle(h).Bounds;return r.Left<=screen.Left&&r.Top<=screen.Top&&r.Right>=screen.Right&&r.Bottom>=screen.Bottom;}
 }
}
