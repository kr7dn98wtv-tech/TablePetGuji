using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
namespace GujiApp {
 public class Outfit { public string Name=""; public int Head=0,Wings=0,Clothes=-1,Bag=0; public Outfit Copy(){return (Outfit)MemberwiseClone();} }
 public class Settings {
  public string PetName="咕叽",OwnerName="主人",SecretNote="愿你每天都有一点小开心。"; public double LampLevel=1,Size=1,Clarity=.87,Left=double.NaN,Top=double.NaN;
  public Outfit DayWear;public Outfit SleepWear=new Outfit{Name="睡衣困困",Head=2,Wings=-1,Clothes=1,Bag=-1};public bool BedPrepared=false; public Outfit Wear=new Outfit(); public List<Outfit> Favorites=new List<Outfit>(); public int Cup=0,Wander=1;
  public bool CupWithOutfit=false,Focus=false,Paused=false,Topmost=true,Sound=false,Onboarded=false,Sleeping=false;
  public bool Water=true,Meals=true,Activity=true,Bedtime=true;
  public int WaterMinutes=60,ActivityMinutes=60,SnoozeMinutes=10;
  public string Breakfast="08:00",Lunch="12:00",Dinner="18:30",Night="22:30";
  public DateTime Adoption=DateTime.Today,LastSeen=DateTime.MinValue;
 }
 public static class Store {
  public static string Root=AppDomain.CurrentDomain.BaseDirectory;
  public static string PathName {get{return Path.Combine(Root,"data","settings.xml");}}
  public static string LastError="";
  public static Settings Load(){try {if(File.Exists(PathName))using(var f=File.OpenRead(PathName)){var s=(Settings)new XmlSerializer(typeof(Settings)).Deserialize(f);Normalize(s);return s;}}catch(Exception e){LastError=e.Message;}return new Settings();}
  public static void Normalize(Settings s){s.LampLevel=Math.Max(.2,Math.Min(1.5,s.LampLevel));s.Size=Math.Max(.4,Math.Min(1.5,s.Size));s.Clarity=Math.Max(.5,Math.Min(.98,s.Clarity));s.Cup=Math.Max(0,Math.Min(5,s.Cup));s.Wander=Math.Max(0,Math.Min(2,s.Wander));s.Wear=s.Wear??new Outfit();s.Favorites=s.Favorites??new List<Outfit>();Fix(s.Wear);if(s.DayWear!=null)Fix(s.DayWear);s.SleepWear=s.SleepWear??new Outfit{Head=2,Wings=-1,Clothes=1,Bag=-1};Fix(s.SleepWear);s.Favorites.RemoveAll(o=>o==null);s.Favorites.ForEach(Fix);s.WaterMinutes=Math.Max(5,Math.Min(240,s.WaterMinutes));s.ActivityMinutes=Math.Max(10,Math.Min(240,s.ActivityMinutes));s.SnoozeMinutes=Math.Max(1,Math.Min(120,s.SnoozeMinutes));if(string.IsNullOrWhiteSpace(s.PetName))s.PetName="咕叽";if(string.IsNullOrWhiteSpace(s.OwnerName))s.OwnerName="主人";if(s.SecretNote==null)s.SecretNote="愿你每天都有一点小开心。";}
  static void Fix(Outfit o){o.Head=Math.Max(-1,Math.Min(23,o.Head));o.Wings=Math.Max(-1,Math.Min(3,o.Wings));o.Clothes=Math.Max(-1,Math.Min(16,o.Clothes));o.Bag=Math.Max(-1,Math.Min(7,o.Bag));if(o.Head==12)o.Head=9;if(o.Head==13)o.Head=10;if(o.Clothes==6)o.Clothes=3;if(o.Clothes==7)o.Clothes=4;if(o.Name=="奶油雪团"||o.Name=="莓莓围炉")o.Name="日常暖暖";}
  public static bool Save(Settings s){try{Normalize(s);Directory.CreateDirectory(Path.GetDirectoryName(PathName));string tmp=PathName+".tmp";using(var f=File.Create(tmp))new XmlSerializer(typeof(Settings)).Serialize(f,s);if(File.Exists(PathName))File.Replace(tmp,PathName,PathName+".bak");else File.Move(tmp,PathName);return true;}catch(Exception e){LastError=e.Message;return false;}}
 }
 public static class Wardrobe {
  public static readonly string[] Heads={"小雏菊","奶油贝雷帽","困困睡帽","铃兰","草莓花","樱花","四叶草","向日葵","月光小花","栗子针织帽","枫糖贝雷帽","南瓜帽","","","星夜睡帽","嫩芽","焦糖格纹帽","苔绿针织帽","燕麦毛线帽","枫叶贝雷帽","云朵睡帽","蜜桃睡帽","星星睡帽","薄荷睡帽"};
  public static readonly string[] Wings={"晨露薄绿","樱花薄粉","蜜糖晴天","星月薄紫"};
  public static readonly string[] Clothes={"蜜桃围巾","月亮披肩","奶油花边","栗子披肩","枫糖毛衣","南瓜棉披风","","","星夜斗篷","焦糖格纹披肩","苔绿毛衣","燕麦开衫","枫叶棉披风","云朵睡衣","蜜桃睡衣","星星睡衣","薄荷睡衣"};
  public static readonly string[] Bags={"秘密星星包","铃兰叶子包","草莓小包","樱花信封包","四叶幸运坠","向日葵编织包","月光小包","奶油月亮包"};
  public static readonly string[] Cups={"晨露叶子杯","雏菊马克杯","草莓牛奶杯","星月晚安杯","橡果暖暖杯","雪团保温杯"};
  public static readonly Outfit[] Sets={
   Make("花园精灵",0,0,-1,0),Make("围巾日常",1,-1,0,-1),Make("睡衣困困",2,-1,1,-1),
   Make("铃兰晨露",3,0,-1,1),Make("草莓花信",4,1,2,2),Make("樱花来信",5,1,-1,3),Make("四叶幸运",6,0,-1,4),Make("向日葵晴天",7,2,-1,5),Make("月光小花",8,3,-1,6),
   Make("栗子秋游",9,-1,3,-1),Make("枫糖学院",10,-1,4,-1),Make("南瓜暖暖",11,-1,5,-1),Make("星夜冬眠",14,-1,8,-1),Make("焦糖秋日",16,-1,9,-1),Make("苔绿秋游",17,-1,10,-1),Make("燕麦暖秋",18,-1,11,-1),Make("枫叶小憩",19,-1,12,-1),Make("云朵晚安",20,-1,13,-1),Make("蜜桃好梦",21,-1,14,-1),Make("星星酣眠",22,-1,15,-1),Make("薄荷午睡",23,-1,16,-1)};
  public static int MatchingCup(Outfit o){if(o.Head>=20||o.Head==2||o.Head==14)return 3;if(o.Head>=16&&o.Head<=19||o.Head==9||o.Head==10||o.Head==11)return 4;if(o.Head==12||o.Head==13)return 5;if(o.Head==4)return 2;if(o.Head==15||o.Head<0)return 0;return 1;}
  static Outfit Make(string n,int h,int w,int c,int b){return new Outfit{Name=n,Head=h,Wings=w,Clothes=c,Bag=b};}
 }
 public static class Foods {
  public static readonly string[] Names={"麻辣烫","小蛋糕","红烧肉","炸鸡","红烧排骨","番茄炒蛋","宫保鸡丁","鱼香肉丝","麻婆豆腐","清炒土豆丝","西兰花炒虾仁","土豆炖牛肉","糖醋里脊","黄焖鸡","红烧鱼","蒸蛋羹","馄饨","牛肉面","重庆小面","蛋炒饭","饺子","小笼包","锅贴","葱油饼","汉堡","披萨","薯条","章鱼小丸子","汤圆","粽子","玉米","烤红薯","西瓜","水果拼盘","草莓冰淇淋","红糖糍粑","蛋挞","玉米浓汤"};
  public static readonly int[][] Groups={new[]{0,2,4,5,6,7,8,9,10,11,12,13,14,15,37},new[]{16,17,18,19,20,21,22,23,28,29,30,31},new[]{3,24,25,26,27},new[]{1,32,33,34,35,36}};
  public static bool Rice(int i){return i==19||i==29;}
 }
 public enum DragPhase { Rest,Sticky,Lifted }
 public class StickyDrag {
  public const double PeelDistance=72,MaxStretch=104;
  public DragPhase Phase;public Point Start,Last,Grab;public Vector Pull,Velocity,Target;public double Hang,HangVelocity;public bool Moved;public double Scale=1;double held,distance,threshold=PeelDistance;
  public void Begin(Point screen,Point local,double scale){Phase=DragPhase.Sticky;Start=Last=screen;Grab=local;Scale=scale;Moved=false;Pull=Velocity=Target=new Vector();Hang=HangVelocity=held=distance=0;}
  public Vector Move(Point p){Vector delta=p-Last;Last=p;Vector offset=(p-Start)/Scale;distance=offset.Length;Moved|=distance>4;
   if(Phase==DragPhase.Sticky){double t=Math.Min(1,distance/PeelDistance);Target=distance<.01?new Vector():offset/distance*(MaxStretch*(.08*t+.92*Math.Pow(t,2)));
    // A deliberate short tug is enough. Sideways/downward repositioning needs less stretch.
    threshold=offset.Y<0&&Math.Abs(offset.Y)>Math.Abs(offset.X)?PeelDistance:36;
    if(distance>=threshold&&held>=.12){Phase=DragPhase.Lifted;return delta;}return new Vector();}
   if(Phase==DragPhase.Lifted){Target=new Vector(Math.Max(-25,Math.Min(25,delta.X/Scale*1.6)),Math.Max(-28,Math.Min(-6,-12-delta.Y/Scale*.6)));return delta;}return new Vector();
  }
  public void End(){Phase=DragPhase.Rest;Target=new Vector();held=distance=0;}
  public void Step(double dt){held+=dt;if(Phase==DragPhase.Sticky&&distance>=threshold&&held>=.22)Phase=DragPhase.Lifted;
   if(Phase==DragPhase.Lifted)Target+=(new Vector(0,-12)-Target)*(1-Math.Exp(-dt*4));
   HangVelocity+=((Phase==DragPhase.Lifted?1:0)-Hang)*100*dt-HangVelocity*15*dt;Hang=Math.Max(0,Math.Min(1.1,Hang+HangVelocity*dt));if(Phase!=DragPhase.Lifted&&Hang<.001&&Math.Abs(HangVelocity)<.01){Hang=0;HangVelocity=0;}
   Vector a=(Target-Pull)*(Phase==DragPhase.Rest?105:200)-Velocity*(Phase==DragPhase.Rest?13:20);Velocity+=a*dt;Pull+=Velocity*dt;if(Phase==DragPhase.Rest&&Pull.Length<.025&&Velocity.Length<.04){Pull=Velocity=new Vector();}
  }
  public Point Map(Point p){
   double height=Math.Max(0,Math.Min(1,(350-p.Y)/255));
   double baseWeight=Math.Pow(height,.45);
   double dx=(p.X-Grab.X)/160,dy=(p.Y-Grab.Y)/190;
   double local=.65+.35*Math.Exp(-(dx*dx+dy*dy));
   double weight=Math.Min(baseWeight*local,Math.Max(0,(p.Y-16)/MaxStretch*.92));Point q=p+Pull*weight;
   // A stretched jelly keeps its footprint while its middle narrows; the face follows the same mesh.
   double strain=Math.Min(1,Pull.Length/MaxStretch),waist=Math.Sin(height*Math.PI);
   q.X=180+(q.X-180)*(1-.38*strain*waist);
   double below=Math.Max(0,Math.Min(1,(p.Y-Grab.Y)/Math.Max(40,350-Grab.Y)));
   q.X=Grab.X+(q.X-Grab.X)*(1-.24*below*Hang);q.Y+=14*Math.Sin(below*Math.PI*.5)*Hang;return q;
  }
 }
 public class WanderSchedule {
  public DateTime NextCheck,LastCheck;readonly Random random;
  public int DurationSeconds;
  public WanderSchedule(DateTime now,Random rng){random=rng;Reset(now,1);}
  public void Reset(DateTime now,int mode){LastCheck=now;NextCheck=now.AddMinutes(mode==2?random.Next(8,16):random.Next(20,41));}
  public int Poll(DateTime now,int mode,bool eligible){if(now<LastCheck||(now-LastCheck).TotalHours>2){Reset(now,mode);return 0;}LastCheck=now;if(now<NextCheck)return 0;Reset(now,mode);if(mode==0||!eligible||random.Next(3)==0)return 0;DurationSeconds=random.Next(6,10);return random.Next(4)==0?2:1;}
 }
 public class ReminderEngine {
  public DateTime NextWater,NextActivity;public Dictionary<string,DateTime> Later=new Dictionary<string,DateTime>();readonly HashSet<string> Fired=new HashSet<string>();public DateTime LastTick;
  public ReminderEngine(DateTime now,Settings s){Reset(now,s);}
  public void Reset(DateTime now,Settings s){NextWater=now.AddMinutes(s.WaterMinutes);NextActivity=now.AddMinutes(s.ActivityMinutes);LastTick=now;Later.Clear();}
  public void Snooze(string kind,DateTime now,int minutes){Later[kind]=now.AddMinutes(minutes);}
  public string Poll(DateTime now,Settings s,bool suppress){
   if(now<LastTick||(now-LastTick).TotalSeconds>90){Reset(now,s);return null;}LastTick=now;
   var due=new List<string>();foreach(var kv in Later)if(kv.Value<=now)due.Add(kv.Key);foreach(var k in due)Later.Remove(k);
   if(s.Meals){Meal(now,s.Breakfast,"breakfast",due);Meal(now,s.Lunch,"lunch",due);Meal(now,s.Dinner,"dinner",due);}if(s.Bedtime)Meal(now,s.Night,"sleep",due);
   if(now>=NextWater){NextWater=now.AddMinutes(s.WaterMinutes);if(s.Water)due.Add("water");}if(now>=NextActivity){NextActivity=now.AddMinutes(s.ActivityMinutes);if(s.Activity)due.Add("activity");}
   if(suppress||s.Paused||s.Focus||s.Sleeping)return null;
   foreach(string k in new[]{"breakfast","lunch","dinner","sleep","water","activity"})if(due.Contains(k)&&Enabled(k,s))return k;return null;
  }
  static bool Enabled(string k,Settings s){return k=="water"?s.Water:k=="activity"?s.Activity:k=="sleep"?s.Bedtime:s.Meals;}
  void Meal(DateTime now,string time,string id,List<string> due){TimeSpan t;if(!TimeSpan.TryParse(time,out t))return;string key=now.ToString("yyyyMMdd")+id;if(now.TimeOfDay>=t&&now.TimeOfDay<t.Add(TimeSpan.FromMinutes(1))&&Fired.Add(key))due.Add(id);if(Fired.Count>16){string today=now.ToString("yyyyMMdd"),yesterday=now.AddDays(-1).ToString("yyyyMMdd");Fired.RemoveWhere(k=>!k.StartsWith(today)&&!k.StartsWith(yesterday));}}
 }
}
