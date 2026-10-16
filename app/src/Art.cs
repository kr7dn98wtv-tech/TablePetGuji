using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
namespace GujiApp {
 public static class Art {
  static readonly Dictionary<string,BitmapSource> Cache=new Dictionary<string,BitmapSource>();
  public static BitmapSource Load(string name){BitmapSource b;if(Cache.TryGetValue(name,out b))return b;var i=new BitmapImage();i.BeginInit();i.CacheOption=BitmapCacheOption.OnLoad;i.UriSource=new Uri(Path.Combine(Store.Root,"assets",name));i.EndInit();i.Freeze();Cache[name]=i;return i;}
  public static BitmapSource Tile(string name,int cols,int rows,int index){string key=name+":"+index;BitmapSource b;if(Cache.TryGetValue(key,out b))return b;var atlas=Load(name);int x=index%cols*atlas.PixelWidth/cols,y=index/cols*atlas.PixelHeight/rows;int right=(index%cols+1)*atlas.PixelWidth/cols,bottom=(index/cols+1)*atlas.PixelHeight/rows;if(name=="heads.png"){int[] bands={0,285,550,802,1141};y=bands[index/cols]*atlas.PixelHeight/1141;bottom=bands[index/cols+1]*atlas.PixelHeight/1141;}if(name=="wings-bags.png"){int[] bands={0,390,712,1076};y=bands[index/cols]*atlas.PixelHeight/1076;bottom=bands[index/cols+1]*atlas.PixelHeight/1076;}if(name=="clothes-front.png"){int[] bands={0,337,641,1024};y=bands[index/cols]*atlas.PixelHeight/1024;bottom=bands[index/cols+1]*atlas.PixelHeight/1024;}if(name=="heads.png"&&index%4==3)x+=42;if(name=="props.png"){int[] bands={0,285,550,805,1076};y=bands[index/cols]*atlas.PixelHeight/1076;bottom=bands[index/cols+1]*atlas.PixelHeight/1076;if(index==14)right-=24;}if(name=="foods-v06.png"){int[] bands={0,240,420,604,790,992};y=bands[index/cols]*atlas.PixelHeight/992;bottom=bands[index/cols+1]*atlas.PixelHeight/992;}if(name=="wardrobe-v06.png"){int[] bands={0,318,570,806,1102};y=bands[index/cols]*atlas.PixelHeight/1102;bottom=bands[index/cols+1]*atlas.PixelHeight/1102;}var tile=new CroppedBitmap(atlas,new Int32Rect(x,y,right-x,bottom-y));
   var rgba=new FormatConvertedBitmap(tile,PixelFormats.Bgra32,null,0);int w=tile.PixelWidth,h=tile.PixelHeight;byte[] px=new byte[w*h*4];rgba.CopyPixels(px,w*4,0);if(name=="wardrobe-v06.png"||name=="clothes-v07.png"||name=="foods-v06.png"||name=="props.png"&&index==11){px=MainComponent(px,w,h);tile=new CroppedBitmap(BitmapSource.Create(w,h,96,96,PixelFormats.Bgra32,null,px,w*4),new Int32Rect(0,0,w,h));}int minx=w,miny=h,maxx=0,maxy=0;for(int yy=0;yy<h;yy++)for(int xx=0;xx<w;xx++)if(px[(yy*w+xx)*4+3]>80){minx=Math.Min(minx,xx);miny=Math.Min(miny,yy);maxx=Math.Max(maxx,xx);maxy=Math.Max(maxy,yy);}
   b=minx<maxx?new CroppedBitmap(tile,new Int32Rect(Math.Max(0,minx-2),Math.Max(0,miny-2),Math.Min(w,maxx+3)-Math.Max(0,minx-2),Math.Min(h,maxy+3)-Math.Max(0,miny-2))):(BitmapSource)tile;b.Freeze();Cache[key]=b;return b;
  }
  // A generated atlas can contain a loose fragment from the neighbouring cell.
  // Keep the item's largest connected alpha island, with its antialiased edge.
  static byte[] MainComponent(byte[] pixels,int w,int h){
   int n=w*h;var seen=new bool[n];var queue=new int[n];int[] best=new int[0];
   for(int s=0;s<n;s++){if(seen[s]||pixels[s*4+3]<40)continue;int head=0,tail=1;queue[0]=s;seen[s]=true;
    while(head<tail){int p=queue[head++],x=p%w,y=p/w;for(int k=0;k<4;k++){int q=k==0?p-1:k==1?p+1:k==2?p-w:p+w;if(k==0&&x==0||k==1&&x==w-1||k==2&&y==0||k==3&&y==h-1)continue;if(!seen[q]&&pixels[q*4+3]>=40){seen[q]=true;queue[tail++]=q;}}}
    if(tail>best.Length){best=new int[tail];Array.Copy(queue,best,tail);}
   }
   var keep=new bool[n];foreach(int p in best){int x=p%w,y=p/w;for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){int xx=x+dx,yy=y+dy;if(xx>=0&&xx<w&&yy>=0&&yy<h)keep[yy*w+xx]=true;}}
   for(int p=0;p<n;p++)if(!keep[p]){pixels[p*4]=pixels[p*4+1]=pixels[p*4+2]=pixels[p*4+3]=0;}return pixels;
  }
  public static BitmapSource Head(int i){return i>=16?Tile("wardrobe-v06.png",4,4,i-16):Tile("heads.png",4,4,i==12||i==13?9:i);} public static BitmapSource Cloth(int i){return i>=9?Tile("clothes-v07.png",4,2,i-9):Tile("clothes-front.png",3,3,i==6||i==7?3:i);}public static BitmapSource Wing(int i){return Tile("wings-bags.png",4,3,i);}public static BitmapSource Bag(int i){return Tile("wings-bags.png",4,3,i+4);}public static BitmapSource Prop(int i){return Tile("props.png",4,4,i);}
  public static BitmapSource Food(int i){return Tile("foods-v06.png",8,5,Math.Max(0,Math.Min(37,i)));}
  static readonly Dictionary<BitmapSource,byte[]> AlphaCache=new Dictionary<BitmapSource,byte[]>();
  public static bool PixelHit(BitmapSource image,Rect box,Point p,bool fit){if(fit){double ratio=image.Width/image.Height,w=box.Width,h=w/ratio;if(h>box.Height){h=box.Height;w=h*ratio;}box=new Rect(box.X+(box.Width-w)/2,box.Y+(box.Height-h)/2,w,h);}if(!box.Contains(p))return false;byte[] px;if(!AlphaCache.TryGetValue(image,out px)){var rgba=new FormatConvertedBitmap(image,PixelFormats.Bgra32,null,0);px=new byte[image.PixelWidth*image.PixelHeight*4];rgba.CopyPixels(px,image.PixelWidth*4,0);AlphaCache[image]=px;}int x=Math.Max(0,Math.Min(image.PixelWidth-1,(int)((p.X-box.X)/box.Width*image.PixelWidth))),y=Math.Max(0,Math.Min(image.PixelHeight-1,(int)((p.Y-box.Y)/box.Height*image.PixelHeight)));return px[(y*image.PixelWidth+x)*4+3]>40;}
  public static BitmapSource WingHalf(int i,bool right){string key="winghalf:"+i+right;BitmapSource half;if(Cache.TryGetValue(key,out half))return half;var full=Wing(i);int mid=full.PixelWidth/2;half=new CroppedBitmap(full,new Int32Rect(right?mid:0,0,right?full.PixelWidth-mid:mid,full.PixelHeight));half.Freeze();Cache[key]=half;return half;}
  public static void Fit(DrawingContext dc,ImageSource image,Rect box){double ratio=image.Width/image.Height,w=box.Width,h=w/ratio;if(h>box.Height){h=box.Height;w=h*ratio;}dc.DrawImage(image,new Rect(box.X+(box.Width-w)/2,box.Y+(box.Height-h)/2,w,h));}
  public static Brush Brush(string hex){return (Brush)new BrushConverter().ConvertFromString(hex);}
  public static void Text(DrawingContext dc,string text,double size,Brush color,Point p){dc.DrawText(new FormattedText(text,System.Globalization.CultureInfo.GetCultureInfo("zh-CN"),FlowDirection.LeftToRight,new Typeface("Microsoft YaHei UI"),size,color),p);}
 }
 public class PetVisual : FrameworkElement {
  public Settings Config;public StickyDrag Drag=new StickyDrag();public double BedProgress=1,LampBrightness=1,Waking;public double Time,Bounce,Happy,ActionAge,Poke;public Point PokeAt;public string Action="";public bool Sleeping,InBed,LampOn=true,Rice,Preview,Intro;public double IntroProgress=1; public int Candy;public double CandyAge;
  public PetVisual(Settings s){Config=s;Width=360;Height=400;}
  double BubbleBounce {get{return Bounce*(1-Math.Min(1,Drag.Pull.Length/50));}}
  public Point Position(Point p){Point q=Drag.Map(p);double bounce=BubbleBounce;double sy=1+bounce;double sx=(1-bounce*.48)*(Turn>0?.65+.35*Math.Abs(Math.Cos((1-Turn)*Math.PI*2)):1);double idle=Sleeping?Math.Sin(Time*1.2)*.004:Math.Sin(Time*2.2)*.008;sy+=idle;sx-=idle*.4;
   q=new Point(180+(q.X-180)*sx,350+(q.Y-350)*sy);
   if(Poke>0){Vector v=q-PokeAt;double d=v.Length;double f=Math.Exp(-d*d/1400)*Poke*8;q+=new Vector(0,f);}
   if(InBed){double t=Math.Max(0,Math.Min(1,(BedProgress-.2)/.5));t=t*t*(3-2*t);double scale=1-.28*t;q=new Point(180+(q.X-180)*scale,350-20*t+(q.Y-350)*scale);}
   if(Waking>0&&!InBed){double scale=1-.28*Waking;q=new Point(180+(q.X-180)*scale,350-20*Waking+(q.Y-350)*scale);}
   if(Intro)q=new Point(q.X,350+(q.Y-350)*Math.Max(.08,IntroProgress));return q;
  }
  protected override void OnRender(DrawingContext dc){base.OnRender(dc);
   if(!InBed&&Waking>0){dc.PushOpacity(Waking);Art.Fit(dc,Art.Prop(9),new Rect(30+(1-Waking)*70,245,285,145));dc.Pop();}
   if(InBed){double t=Math.Min(1,BedProgress*4);dc.PushOpacity(t);Art.Fit(dc,Art.Prop(9),new Rect(30+(1-t)*70,245,285,145));DrawLamp(dc);dc.Pop();}
   else dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(20*Math.Max(0,1-Drag.Hang)),45,95,50)),null,new Point(180,355),85,5);
   Outfit o=Config.Wear;
   if(o.Wings>=0&&!InBed){Point wp=Position(new Point(180,240));double flutter=Happy>0?Math.Sin(Time*14)*5:Math.Sin(Time*2)*1.5;var left=Art.WingHalf(o.Wings,false);var right=Art.WingHalf(o.Wings,true);Art.Fit(dc,left,new Rect(wp.X-165,wp.Y-70,105,150+flutter));Art.Fit(dc,right,new Rect(wp.X+60,wp.Y-70,105,150+flutter));}
   dc.PushOpacity(Config.Clarity);DrawBody(dc);dc.Pop();
   if(o.Clothes>=0&&!InBed)Accessory(dc,Art.Cloth(o.Clothes),ClothRect(o.Clothes),false);
   if(o.Head>=0){bool flower=o.Head==0||o.Head>=3&&o.Head<=8||o.Head==15;Rect head=HeadRect(o.Head);Accessory(dc,Art.Head(o.Head),head,true);}
   DrawFace(dc);
   if(Candy>0&&!InBed){for(int i=0;i<Candy;i++){Point p=Position(new Point(180+(i-1)*28,309+Math.Sin(Time*3+i)*3));dc.PushOpacity(Math.Min(1,Math.Max(0,(18-CandyAge)/3)));Art.Fit(dc,Art.Prop(13),new Rect(p.X-12,p.Y-12,24,24));dc.Pop();}}
   if(o.Bag>=0&&!InBed){Point start=Position(new Point(112,205)),bend=Position(new Point(83,280)),end=Position(new Point(268,321));var strap=new StreamGeometry();using(var g=strap.Open()){g.BeginFigure(start,false,false);g.QuadraticBezierTo(Position(new Point(64,245)),bend,true,false);g.QuadraticBezierTo(Position(new Point(176,310)),end,true,false);}dc.DrawGeometry(null,new Pen(new SolidColorBrush(Color.FromRgb(219,183,102)),2.4),strap);Accessory(dc,Art.Bag(o.Bag),new Rect(243,296,50,50),true);}
   if(InBed){double covered=Math.Max(0,Math.Min(1,(BedProgress-.62)/.35));dc.PushOpacity(covered);Art.Fit(dc,Art.Prop(10),new Rect(42,310+(1-covered)*23,268,81));dc.Pop();if(Sleeping){Art.Text(dc,"z",15,Art.Brush("#9BA68D"),new Point(270,202+Math.Sin(Time)*3));Art.Text(dc,"·",18,Art.Brush("#D7C170"),new Point(285,188));}}
   if(Action=="water"||Reminding=="water"){Point p=new Point(180,308-Math.Min(1,ActionAge)*12);Art.Fit(dc,Art.Prop(Config.Cup),new Rect(p.X-36,p.Y-24,72,70));}
   if(Action=="offerfood")Art.Fit(dc,Art.Food(FoodIndex),new Rect(FoodPosition.X-44,FoodPosition.Y-30,88,66));
   if(Action=="food"||Reminding=="breakfast"||Reminding=="lunch"||Reminding=="dinner"){Art.Fit(dc,Art.Food(FoodIndex),new Rect(133,306,94,69));}
   if(Rice&&!InBed){Point p=Position(new Point(141,280));Art.Fit(dc,Art.Prop(12),new Rect(p.X-7,p.Y-6,14,12));}
   if(Happy>0&&!Sleeping){Point p=Position(new Point(288,190));Art.Text(dc,"♥",19,Art.Brush("#E5A1AE"),new Point(p.X,p.Y-Math.Sin(Time*4)*4));}
   if(!Preview&&!InBed){dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(160,232,244,221)),new Pen(Art.Brush("#C3D7B1"),1),new Point(312,348),14,14);Art.Text(dc,"⌂",18,Art.Brush("#537047"),new Point(303,335));}
  }
  public Point FoodPosition=new Point(93,320);public int FoodIndex=0;public Vector Look;public double Turn;public string Reminding="";
  public bool Hit(Point p){
   if(Action=="offerfood"&&Art.PixelHit(Art.Food(FoodIndex),new Rect(FoodPosition.X-44,FoodPosition.Y-30,88,66),p,true))return true;
   if(InBed)return Art.PixelHit(Art.Prop(9),new Rect(30,245,285,145),p,true)||Art.PixelHit(Art.Prop(10),new Rect(42,310,268,81),p,true)||Art.PixelHit(Art.Prop(11),new Rect(288,236,64,139),p,true)||BodyHit(p);
   if((p-new Point(312,348)).Length<=15)return true;
   Outfit o=Config.Wear;
   if(o.Head>=0){bool flower=o.Head==0||o.Head>=3&&o.Head<=8||o.Head==15;Rect r=HeadRect(o.Head);if(RigidHit(Art.Head(o.Head),r,p))return true;}
   if(o.Bag>=0&&RigidHit(Art.Bag(o.Bag),new Rect(243,296,50,50),p))return true;
   if(o.Wings>=0){Point wp=Position(new Point(180,240));double flutter=Happy>0?Math.Sin(Time*14)*5:Math.Sin(Time*2)*1.5;if(Art.PixelHit(Art.WingHalf(o.Wings,false),new Rect(wp.X-165,wp.Y-70,105,150+flutter),p,true)||Art.PixelHit(Art.WingHalf(o.Wings,true),new Rect(wp.X+60,wp.Y-70,105,150+flutter),p,true))return true;}
   Point q=Unmap(p);if(o.Clothes>=0&&Art.PixelHit(Art.Cloth(o.Clothes),ClothRect(o.Clothes),q,false))return true;return BodyHit(p);
  }
  static Rect HeadRect(int h){
   if(h==0||h>=3&&h<=8||h==15)return new Rect(215,141,68,66);
   if(h>=16)return new Rect(150,100,158,112);

   if(h>=9&&h<=14)return new Rect(153,99,153,112);
   return new Rect(177,114,123,90);
  }
  static Rect ClothRect(int c){
   if(c>=9)return new Rect(43,264,274,97);if(c==3)return new Rect(43,265,274,94);
   if(c==4)return new Rect(42,267,276,92);
   if(c==5)return new Rect(43,265,274,96);
   if(c==6)return new Rect(42,263,276,98);
   if(c==7)return new Rect(43,267,274,92);
   if(c==8)return new Rect(43,264,274,97);
   return new Rect(62,278,236,80);
  }
  public bool SecretBagHit(Point p){return Config.Wear.Bag==0&&RigidHit(Art.Bag(0),new Rect(243,296,50,50),p);}
  bool RigidHit(BitmapSource image,Rect r,Point p){Point c=Position(new Point(r.X+r.Width/2,r.Y+r.Height/2));return Art.PixelHit(image,new Rect(c.X-r.Width/2,c.Y-r.Height/2,r.Width,r.Height),p,true);}
  bool BodyHit(Point p){return Art.PixelHit(Art.Load("body.png"),new Rect(50,95,260,260),Unmap(p),false);}
  public Point Unmap(Point p){Point q=p;for(int i=0;i<7;i++){Point f=Position(q);Vector error=f-p;if(error.Length<.03)break;Point fx=Position(new Point(q.X+.5,q.Y)),fy=Position(new Point(q.X,q.Y+.5));double a=(fx.X-f.X)*2,b=(fy.X-f.X)*2,c=(fx.Y-f.Y)*2,d=(fy.Y-f.Y)*2,det=a*d-b*c;if(Math.Abs(det)<.00001)break;q.X-=(d*error.X-b*error.Y)/det;q.Y-=(-c*error.X+a*error.Y)/det;}return q;}
  void DrawLamp(DrawingContext dc){
   double level=Config.LampLevel,lit=LampBrightness;
   Art.Fit(dc,Art.Prop(11),new Rect(284,226,72,149));
   if(lit>.01){double strength=Math.Min(1,level)*lit;var halo=new RadialGradientBrush();halo.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(210*strength),255,244,180),0));halo.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(125*strength),255,220,100),.38));halo.GradientStops.Add(new GradientStop(Colors.Transparent,1));dc.DrawEllipse(halo,null,new Point(319,291),20+12*level,26+15*level);dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(245*strength),255,253,222)),null,new Point(319,293),3+level,6+2*level);}
   else{dc.DrawEllipse(Art.Brush("#665E42"),null,new Point(319,293),4,7);}
  }
  void Accessory(DrawingContext dc,BitmapSource image,Rect rect,bool rigid){if(!rigid){DrawLayer(dc,image,rect,10,4);return;}Point c=Position(new Point(rect.X+rect.Width/2,rect.Y+rect.Height/2));double angle=rigid?Math.Max(-10,Math.Min(10,Drag.Velocity.X*.025))+Math.Sin(Time*4)*(Happy>0?3:.5):0;dc.PushTransform(new RotateTransform(angle,c.X,c.Y));if(rigid)Art.Fit(dc,image,new Rect(c.X-rect.Width/2,c.Y-rect.Height/2,rect.Width,rect.Height));else dc.DrawImage(image,new Rect(c.X-rect.Width/2,c.Y-rect.Height/2,rect.Width,rect.Height));dc.Pop();}
  void DrawFace(DrawingContext dc){Brush eye=Art.Brush("#24432B");bool close=Sleeping||(Happy>0&&Drag.Phase==DragPhase.Rest)||Time%5.5>5.34;Point center=Position(new Point(180,268));
   foreach(double x in new[]{147.0,213.0}){Point p=Position(new Point(x+(Drag.Phase==DragPhase.Rest?Look.X:Math.Max(-2.5,Math.Min(2.5,(Drag.Grab.X-x)/40))),255+Look.Y));if(close){var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(new Point(p.X-5,p.Y+1),false,false);c.QuadraticBezierTo(new Point(p.X,p.Y-5),new Point(p.X+5,p.Y+1),true,false);}dc.DrawGeometry(null,new Pen(eye,2.5),g);}else{dc.DrawEllipse(eye,null,p,4.8,6.8);dc.DrawEllipse(Brushes.White,null,new Point(p.X-1,p.Y-2),1.3,1.5);}}
   foreach(double x in new[]{129.0,231.0}){Point p=Position(new Point(x,275));dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(80,246,165,133)),null,p,9,4);}
   Point mouth=Position(new Point(180,278));if(Action=="food"||Action=="water"||Action=="offerfood"){dc.DrawEllipse(eye,null,mouth,4.5,4+Math.Sin(Time*8));}else{var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(new Point(mouth.X-6,mouth.Y),false,false);c.QuadraticBezierTo(new Point(mouth.X,mouth.Y+6),new Point(mouth.X+6,mouth.Y),true,false);}dc.DrawGeometry(null,new Pen(eye,2.1),g);}
  }
  void DrawBody(DrawingContext dc){DrawLayer(dc,Art.Load("body.png"),new Rect(50,95,260,260),14,18);}
  void DrawLayer(DrawingContext dc,BitmapSource image,Rect rect,int cols,int rows){
   if(Drag.Pull.Length<.05&&Drag.Hang<.001&&Poke<=0){Point a=Position(rect.TopLeft),b=Position(rect.BottomRight);dc.DrawImage(image,new Rect(a,b));return;}
   var group=new DrawingGroup();RenderOptions.SetEdgeMode(group,EdgeMode.Aliased);
   using(var draw=group.Open()){for(int y=0;y<rows;y++)for(int x=0;x<cols;x++){
    Point a=new Point(rect.X+x*rect.Width/cols,rect.Y+y*rect.Height/rows),b=new Point(rect.X+(x+1)*rect.Width/cols,a.Y),c=new Point(a.X,rect.Y+(y+1)*rect.Height/rows),d=new Point(b.X,c.Y);
    Triangle(draw,image,rect,a,b,c,Position(a),Position(b),Position(c));Triangle(draw,image,rect,b,d,c,Position(b),Position(d),Position(c));}}
   dc.DrawDrawing(group);
  }
  static void Triangle(DrawingContext dc,BitmapSource image,Rect imageRect,Point a,Point b,Point c,Point aa,Point bb,Point cc){var source=new Matrix(b.X-a.X,b.Y-a.Y,c.X-a.X,c.Y-a.Y,a.X,a.Y);if(!source.HasInverse)return;source.Invert();source.Append(new Matrix(bb.X-aa.X,bb.Y-aa.Y,cc.X-aa.X,cc.Y-aa.Y,aa.X,aa.Y));var clip=new StreamGeometry();using(var ctx=clip.Open()){ctx.BeginFigure(a,true,true);ctx.LineTo(b,true,false);ctx.LineTo(c,true,false);}dc.PushTransform(new MatrixTransform(source));dc.PushClip(clip);dc.DrawImage(image,imageRect);dc.Pop();dc.Pop();}
  public void Save(string path,int width,int height){Measure(new Size(360,400));Arrange(new Rect(0,0,360,400));UpdateLayout();var r=new RenderTargetBitmap(width,height,96*width/360.0,96*height/400.0,PixelFormats.Pbgra32);r.Render(this);var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(r));using(var f=File.Create(path))e.Save(f);}
 }
}
