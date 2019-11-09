using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using OpenCvSharp.XFeatures2D;
using System.IO;

using OpenCvSharp;
using GIDOO_Lib;
using GIDOO_space;

namespace GIDOOCV{
    public partial class sdkFrameRecgV3{  //  static public class CvExtensions{ 
        static string  folderName=null;//"FailureImage";
        static public  int regSize=32;         
        static public  int[] link16=new int[16];
        static private int[] drctn={-1,+1,-4,+4 };
        
        public MLXnumber3 MLn3;  
        public ProjectiveTrans PT=new ProjectiveTrans( );
        private double gammaC;
        private GDCV GDCV=new GDCV();

        public int diagonal, diag2;
        public List<int>    dglTbl;
        private Point2d Center;

        static sdkFrameRecgV3(){
            //Connected node of 4×4 lattice
            for( int n=0; n<16; n++ ){
                int r0=n/4, c0=n%4, d=0;
                for( int k=0; k<4; k++ ){
                    int n1=n+drctn[k], r1=n1/4, c1=n1%4;
                    if(n1<0 || n1>=16 )  continue;
                    if( (r0==r1 && Math.Abs(c0-c1)==1) || (c0==c1 && Math.Abs(r0-r1)==1) ) d |= (1<<n1);
                }
                link16[n]=d;
                //WriteLine($" n:{n} -> d{d.ToBitStringN(16)}");
            }
        }

        public sdkFrameRecgV3( string fName, int MLtype=6, int MidLSize=64, double gammaC=0.7 ){
            this.gammaC=gammaC;
            MLn3 = new MLXnumber3();
            MLn3.Set_LayerData(fName,MLtype,MidLSize,gammaC,DispB:true); //Machine learning  
        }

      #region SuDoku frame recognition
        public Mat stdImg=null;
        public int[] sdkFrameRecg_SolverV3( Mat imgWhiteB, byte thrVal, bool DispB=false ){
                //___PrintTimeSpan("start");

            Mat imgBlackW = _01_ReverseBW(imgWhiteB);
            Point2d[] Q4=null;
            
            stdImg = _01_DetectGrid4(imgWhiteB,imgBlackW,thrVal,out Q4, DispB:false);  //Binarization: background white, object black         
                //___PrintTimeSpan("_01_DetectGrid4");

            if(Q4==null) return null;
            if(DispB)  using( new Window("_0A_StandardFrame",WindowMode.KeepRatio,stdImg) ){ Cv2.WaitKey(0); }

            Mat stdImg2 = GetHoughLines(stdImg);
                //___PrintTimeSpan("GetHoughLines");       
            Point2d[] Q16 = _0_DetectGrid16(stdImg2,Q4,DispB:false); //Frame distortion adjustment
                //___PrintTimeSpan("_0_DetectGrid16");
            Point2d[] Q100 =  _0_DetectGrid100Ex(stdImg2,Q16,DispB:false);
                //___PrintTimeSpan("_0_DetectGrid100Ex");
              
            int[] SDK8=_0_digitRecognition100(stdImg,Q100,DispB:false);//true);  //Result display
                //___PrintTimeSpan("_0_digitRecognition100");
  
            return SDK8;    //stdImg;
        } 
        private DateTime _startTm, _currentTm, _Tm0;
        private void ___PrintTimeSpan( string st ){
            if(st=="start"){ _startTm=_currentTm=DateTime.Now; WriteLine("-----Timer start"); }
            else{ 
                _Tm0=DateTime.Now;
                Write( "######"+st.PadRight(25) +" "+ (_Tm0-_currentTm).Milliseconds + "ms");
                WriteLine(" / {0:0.0} ms", (_Tm0-_startTm).TotalMilliseconds);
                _currentTm=_Tm0;
            }
        }
        
        public Mat _01_DetectGrid4( Mat imgWhiteB, Mat imgBlackW, byte thValue, out Point2d[] Q4fnd, bool DispB ){
            //Acquiring convex hull

            //Coordinate list of contour
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(imgBlackW, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple );

            Size    sz=imgWhiteB.Size();
            int     W=sz.Width, H=sz.Height, WW=(int)Math.Min(W*0.4,H*0.4);
            try{
                Mat checkImg = imgWhiteB.CvtColor(ColorConversionCodes.GRAY2BGR); //###### Gray->Color
                foreach( var PP in contours.Where(p=>p.Count()>=4) ){ 
                    double Xmin=PP.Min(p=>p.X);  
                    double Xmax=PP.Max(p=>p.X);
                    double Ymin=PP.Min(p=>p.Y);  
                    double Ymax=PP.Max(p=>p.Y);  
                    if((Xmax-Xmin)<WW || (Ymax-Ymin)<WW ) continue;          

                    //convex hull (clockwise:true counterclockwise)
                    var P=Cv2.ConvexHull(PP,clockwise:true).ToList();   
                    double area = Cv2.ContourArea(P); //Area of convex hull
                    //if(area<WW*WW) continue;

                    List<Point2d> P2 = P.ConvertAll(q => new Point2d(q.X,q.Y) );
                    Mat stdImg = CorrectDistortion(imgWhiteB,P2,out Point2d[] Q4, DispB:false);          
                    Q4fnd = Q4;
                    if( stdImg==null) return null;
                    return stdImg;
                }
            }
            catch(Exception e ){ WriteLine(e.Message+"\r"+e.StackTrace); }
            Q4fnd=null;
            return null;
        }                    
        private Mat CorrectDistortion( Mat imgWhiteB, List<Point2d> PTs, out Point2d[] Q4, bool DispB=false){
            Center = new Point2d(PTs.Average(P => P.X),PTs.Average(P => P.Y));
            List<PointEx> PTda = PTs.ToList().ConvertAll(P => new PointEx(P,Center));
            PTda.Sort((A,B) => (int)((A.Angle-B.Angle)*100000.0));
            PTda.AddRange(PTda);
                //if(DispB){ for(int k=0; k<PTda.Count(); k++) WriteLine("k:{0} PTda:{1}",k,PTda[k]); }
            List<List<PointEx>> PTda4 = _SepareteLins(PTda);    //BLTR
            if(PTda4==null || PTda4.Count<4){ Q4=null; return null; }
#if false
                if(DispB){
                    Mat resImg=imgWhiteB.CvtColor(ColorConversionCodes.GRAY2BGR); //Gray->Color変換
                    Scalar[] clrLst={ Scalar.Red, Scalar.Blue, Scalar.Yellow, Scalar.Green };
                    for( int k=0; k<4; k++ ){
                        var clr=clrLst[k];  //(k%2==0)? Scalar.Red: Scalar.Blue;
                        PTda4[k].ForEach(Q=>resImg.Circle(Q.Pt,5,clr,5));
                    }
                    using( new Window("◆1◆resImg",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                }
#endif
            int nSize=1;
            var FRegTop = new FuncApproximation(nSize,PTda4[2],CalculusB:true);
            var FRegBtm = new FuncApproximation(nSize,PTda4[0],CalculusB:true);
            var FRegLft = new FuncApproximation(nSize,PTda4[1],CalculusB:true);
            var FRegRgt = new FuncApproximation(nSize,PTda4[3],CalculusB:true);

            Point2d[] PT4=new Point2d[4];
            var FM=new RegFuncUtility(ImgCheck:imgWhiteB);
            Point2d P00=new Point2d(0,0), PGG=new Point2d(imgWhiteB.Width,imgWhiteB.Height);

            PT4[0]=FM.IntersectionPoint(FRegTop,FRegLft, P00, PGG, DispB:false);
            PT4[2]=FM.IntersectionPoint(FRegBtm,FRegLft, P00, PGG, DispB:false);
            PT4[1]=FM.IntersectionPoint(FRegTop,FRegRgt, P00, PGG, DispB:false);
            PT4[3]=FM.IntersectionPoint(FRegBtm,FRegRgt, P00, PGG, DispB:false);

                if(DispB){
                    Mat resImg=imgWhiteB.CvtColor(ColorConversionCodes.GRAY2BGR); //Gray->Color
                    Scalar[] clrLst={ Scalar.Red, Scalar.Blue, Scalar.Yellow, Scalar.Green };
                    for( int k=0; k<4; k++ ){
                        var clr=clrLst[k];  //(k%2==0)? Scalar.Red: Scalar.Blue;
                        PTda4[k].ForEach(Q=>resImg.Circle(Q.Pt,5,clr,5));
                    }
                    //using( new Window("#1#resImg",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 

                    for( int k=0; k<4; k++ ){
                        var clr=clrLst[k];
                        PTda4[k].ForEach(Q=>resImg.Circle(PT4[k],10,clr,5));
                    }                                    
                    using( new Window("#2#resImg",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                }

            FRegTop.CreateFunction(PT4[0],0,32*9);
            FRegBtm.CreateFunction(PT4[2],0,32*9);
            //FRegLft.CreateFunction(PT4[0],0,32*9);
            //FRegRgt.CreateFunction(PT4[2],0,32*9);

            Mat MatCor=new Mat(352,352,MatType.CV_8UC1,Scalar.White);
            unsafe{
                byte *S=imgWhiteB.DataPointer;
                byte *D=MatCor.DataPointer;
                int W=imgWhiteB.Width, H=imgWhiteB.Height, W2=MatCor.Width;
                double per=1.0/(32*9);
                for( int x=0; x<=32*9+3; x++){
                    double xd=x;
                    Point ptT = FRegTop.EstimateL2Pt(xd);
                    Point ptB = FRegBtm.EstimateL2Pt(xd);
                    for( int y=0; y<=32*9+3; y++){
                        Point pt=_2_Get_InterPolation(ptT,ptB,per*y);
                        if(pt.X<0 || pt.X>=W || pt.Y<0 || pt.Y>=H){
            ////            using( new Window("#test# imgWhiteB",WindowMode.KeepRatio,imgWhiteB) ){ Cv2.WaitKey(0); } 
                            Q4=null;
                            return null;
                        }
                        D[(y+32)*W2+x+32] = S[pt.Y*W+pt.X];
                    }
                }
            }
          
            Q4 = new Point2d[4];
            Q4[0] = new Point2d(32 ,32);
            Q4[1] = new Point2d(320,32);
            Q4[2] = new Point2d(32 ,320);
            Q4[3] = new Point2d(320,320);

            if(DispB) new Window("MatCor",WindowMode.KeepRatio,MatCor);
            return MatCor;
        }  
        private List<List<PointEx>> _SepareteLins( List<PointEx> PTda, bool DispB=false ){

            List<int> SepIndex = new List<int>();
            for( int k=1; k<PTda.Count-1; k++ ){
                double L0=PTda[k].Length, Lpre=PTda[k-1].Length, Lnxt=PTda[k+1].Length;
                if(Lpre<=L0 && L0>=Lnxt ){
                    SepIndex.Add(k); 
                    if(SepIndex.Count==5) break;
                }
            }
            if(SepIndex.Count<5) return null;

            List<List<PointEx>> SegLines = new List<List<PointEx>>();

            for( int k=0; k<4; k++ ){
                int na=SepIndex[k];
                SegLines.Add( PTda.GetRange(na,SepIndex[k+1]-na+1) );
            }
            SegLines[0].Reverse();
            SegLines[1].Reverse();

            if(DispB){
                using( var fpL=new StreamWriter("debug_SepareteLins.txt",true,Encoding.Unicode) ){
                    for( int k=0; k<4; k++ ){
                        WriteLine(); fpL.WriteLine(); 
                        int na=SepIndex[k];
                        var P=PTda.GetRange(na,SepIndex[k+1]-na+1);
                        P.ForEach(p=>WriteLine("{0}, {1}", p.Pt.X, p.Pt.Y));
                        P.ForEach(p=>fpL.WriteLine("{0}, {1}", p.Pt.X, p.Pt.Y));
                    }
                }
            }
            return SegLines;
        }
      
        private Mat GetHoughLines( Mat imgGrayWB, bool DispB=false ){
            Mat imgGrayWstdI = _01_ReverseBW(imgGrayWB);
            List<LineSegmentPoint> segStdLst =Cv2.HoughLinesP(imgGrayWstdI,1,Math.PI/180,20,10,10).ToList();
           // HoughLinesP(Mat image, double rho, double theta, int threshold, double minLineLength,double maxLineGap)

            Mat dstImg=new Mat(imgGrayWB.Size(),MatType.CV_8UC1,Scalar.White);
                foreach (LineSegmentPoint s in segStdLst){
                dstImg.Line(s.P1,s.P2,Scalar.Black,1,LineTypes.AntiAlias,0);
            }
                if(DispB){
                    using(new Window("SourceGray", WindowMode.KeepRatio, imgGrayWB))
                    using(new Window("DetectedFrame", WindowMode.KeepRatio, dstImg)){ Cv2.WaitKey(); }
                }
            return dstImg;
        }
        private Point2d[] _0_DetectGrid16( Mat frame2, Point2d[] Q4, bool DispB ){ //Frame warp adjustment                                                                      //輪郭抽出から隣接領域の切り出し(その3)凸包の取得( http://independence-sys.net/main/?p=1705 )
            Point2d[] Q16 = _1_Point4ToPoint16(Q4);
            Mat frameBW=frame2;
            int sfX0=20, sfMax=0;
            double ev=0;
            Point2d Px=new Point(0,0), PbDir=new Point2d();
            fnLine fn;

            double ev16, ev16Max=0;
            double[] evPar=new double[16];
            for( int loop=0; loop<5; loop++ ){//for( int Loop=0; Loop<6; Loop++ ){  //################ 3-5
                ev16=0.0;
                for( int n=0; n<16; n++ ){
                    int sfX=sfX0;
                    if(loop>=2 && evPar[n]>0.99) sfX/=loop; //If the prefitness is sufficiently high, narrow the width.
                    Point2d Pa=Q16[n], PaMax=Pa;
                    int link16n=link16[n], mm=0;
                
                    double evMax=0.0, evT=0.0, evTMax=0.0;                   
                    sfMax=-999;
                    foreach( var m in link16n.IEGet_BtoNo(16) ){    //Pa to Pb : Q16[m] direction
                        Point2d Pb=Q16[m];
                        fn=null;
                        for( int sf=sfX; ; sf=((sf<0)? -(sf+1):-sf) ){//Move by sf ->Px
                            Px=_2_MovePoint(Pa,Pa,Pb,sf,fn);
                            ev= _2_Eval_LineMatchingSel(frameBW,Q16,Px,link16n,128,ref evT);

                            if(ev>evMax){ evMax=ev; sfMax=sf; PbDir=Pb; mm=m; evTMax=evT; }  
                            if(sf==0) break;
                        }
                    }
                    fn=null;
                    if(sfMax!=-999)  Q16[n]=_2_MovePoint(Pa,Pa,PbDir,sfMax,fn); 
                    evPar[n]=evTMax;
                     //   if(DispB){
                     //       string sfMaxST=sfMax.ToString().PadLeft(3);
                     //       WriteLine( $"########_1_Detect_GridMatching sf:{sfMaxST} evMax:{evMax} n-m:{n}-{mm} Pa:{Pa} Pb:{PbDir} Px:{Px} " );
                     //   }
 
                    ev16+=evMax;
                }

                if(DispB){
                    Write(" evPar:");
                    foreach( var p in evPar) Write(" {0:0.000}",p);
                    WriteLine(" ### _0_DetectGrid16 ev:{0:0.0000} evMax:{1:0.0000}", ev16, ev16Max );
                }
                if(ev16<=ev16Max) break;
                ev16Max=ev16;
            }
            if(DispB) _DetectGridCheck(frame2,Q16);
            
            return Q16;
        }
                     
        private int[] _0_digitRecognition100(Mat imgWhiteB,Point2d[] Q100, bool DispB=false){
            int[] SDK81 = _01B_DetectDigits100(frame2:imgWhiteB, Q100:Q100, DispBres:false);
            for(int k=0; k<81; k++) if(SDK81[k]>9) SDK81[k]=11;
          #region debug print
            if(DispB){
                string st="_0_digitRecognition";
                for(int rc=0; rc<81; rc++){                           
                    int p = SDK81[rc];
                    if((rc%9)==0) st+=" ";
                    string po = "x";
                    if(p==0) po = ".";
                    else if(p<10) po = p.ToString();
                    st += po;
                }
                WriteLine(st);

                st="_0_digitRecognition";
                for(int rc=0; rc<81; rc++){
                    if(SDK81[rc]==0) st += " .";
                    else if(SDK81[rc]==11)  st += " X";
                    else st += " " + SDK81[rc];
                    if((rc%9)==8) { WriteLine(st); st = "_0_digitRecognition"; }
                }

            }      
          #endregion
            return SDK81;
        }
         
        public int[] _01B_DetectDigits100( Mat frame2, Point2d[] Q100, bool DispBres=false ){
            Point[] Q4W = new Point[4];

            for( int r=1; r<9; r++ ){
                if((r%3)==0) continue;
                int r0 =r/3*3;
                for( int c=1; c<9; c++ ){
                    if((c%3)==0) continue;
                    int c0=c/3*3;
                    Q100[r*10+c] = crossPoint(Q100[r*10+c0],Q100[r*10+c0+3], Q100[r0*10+c],Q100[(r0+3)*10+c]);
                }
            }

            int[] SB81=new int[81];
            for( int rc=0; rc<81; rc++ ){
                int r=rc/9,c=rc%9, rcA=r*10+c;
                Q4W[0]=Q100[rcA]; Q4W[1]=Q100[rcA+1]; Q4W[2]=Q100[rcA+11]; Q4W[3]=Q100[rcA+10]; 

                //Inverse projection transformation
                Mat reaRecog9 = _2B_SDK_ProjectivTransformInv(frame2,Q4W,DispB:false); //##@@2               

                //Sudoku digits recognition
                int Q = _2B_DigitRecognition(reaRecog9,rc0:rc,resDispB:true); //$$$$$$$$
                SB81[rc] = Q;
                if(folderName!=null && Q==99){
                    if(!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
                    string fName = folderName+"/"+DateTime.Now.ToString("yyMMdd_HHmmssfff")+".jpg";
                    Cv2.ImWrite(fName, reaRecog9);
                }
              /*
                if(DispBres || Q==99){
                    using( new Window("DDigit rc:"+rc,WindowMode.KeepRatio,reaRecog9) ){ Cv2.WaitKey(0); }

                }
              */
            }
                            if(DispBres){
                                string st="SDK_ImageRecogEx2   ";
                                for( int rc=0; rc<81; rc++ ){
                                    int sb=SB81[rc];
                                    if(sb==10) st+=" .";
                                    else if(sb==0 || sb>10) st+=" X";
                                    else                  st+=" "+SB81[rc];
                                    if((rc%9)==8){ WriteLine(st); st="SDK_ImageRecogEx2   "; }
                                }
                                WriteLine();
                            }
            return SB81;
        }
        private Point2d[] _0_DetectGrid100Ex(Mat stdImg,Point2d[] Q16,bool DispB){
            double a=1.0/3.0, b=2.0/3.0;
            var Q100 = new Point2d[100];

            for( int k=0; k<16; k++) Q100[(k/4*30)+(k%4)*3]=Q16[k];
            for( int r=0; r<=10; r+=3){
                for( int c=0; c<9; c+=3){
                    int rc=r*10+c;
                    Q100[rc+1] = _2_Get_InterPolation(Q100[rc],Q100[rc+3],a);
                    Q100[rc+2] = _2_Get_InterPolation(Q100[rc],Q100[rc+3],b);
                }
            }
            for( int c=0; c<=9; c+=3 ){
                for( int r=0; r<9; r+=3){
                    int rc=(r)*10+c;
                    Q100[rc+10] = _2_Get_InterPolation(Q100[rc],Q100[rc+30],a);
                    Q100[rc+20] = _2_Get_InterPolation(Q100[rc],Q100[rc+30],b);
                }
            }  
            Mat Q100Mat = stdImg.CvtColor(ColorConversionCodes.GRAY2BGR);  //####
            foreach( var P in Q100.Where(p=>p.X>0) ) Q100Mat.Circle(P,3,Scalar.Green,2);  //####

            double evMax=0.0;
            for( int loop=0; loop<10; loop++ ){  //####
                double ev=0.0;
                for( int bk=0; bk<9; bk++ ){
                    int rc0 = (bk/3)*30 + (bk%3)*3;
                    ev += AdjustGrid81(stdImg,Q100, rc0+1,rc0+0,rc0+3, rc0+31,rc0+30,rc0+33);
                    ev += AdjustGrid81(stdImg,Q100, rc0+2,rc0+0,rc0+3, rc0+32,rc0+30,rc0+33);

                    ev += AdjustGrid81(stdImg,Q100, rc0+10,rc0+0,rc0+30, rc0+13,rc0+3,rc0+33);
                    ev += AdjustGrid81(stdImg,Q100, rc0+20,rc0+0,rc0+30, rc0+23,rc0+3,rc0+33);
                }

                //WriteLine("### AdjustGrid81 ev:{0} evMax:{1}", ev, evMax );
                if(ev<=evMax) break;
                evMax=ev;
            }
                if(DispB){
                    foreach( var P in Q100.Where(p=>p.X>0) ) Q100Mat.Circle(P,5,Scalar.Red,2);  //####
                    using( new Window("_0_DetectGrid100Ex",WindowMode.KeepRatio,Q100Mat) ){ Cv2.WaitKey(0); }  //####
                }
            return Q100;
        }

        private Mat _01_ReverseBW( Mat src ){
            int HW = src.Height*src.Width;
            Mat dst= src.Clone();
            unsafe{
                byte *S=src.DataPointer;
                byte *D=dst.DataPointer;
                for( int k=0; k<HW; k++ ){
                    *D++ = (byte)(((*S++)>128)? 0: 255);
                }
            }
            return dst;
        }

      #endregion SuDoku frame recognition
  
        public Point[] _1_GetElection( Point[] RR ){ //Adjust to erect position
            int XC=(RR[0].X+RR[1].X+RR[2].X+RR[3].X)/4;
            int YC=(RR[0].Y+RR[1].Y+RR[2].Y+RR[3].Y)/4;
            int sft=0;
            for( sft=0; sft<4; sft++ ){
                if( RR[sft].X<=XC && RR[sft].Y<=YC ) break;
            }
            Point[] RRa = new Point[4];
            for( int k=0; k<4; k++ ) RRa[k]=RR[(k+sft)%4];
#if false
                WriteLine($" sft:{sft}" );
                for( int k=0; k<4; k++ )  Write( $"{RR[k]}" );
                WriteLine();
                for( int k=0; k<4; k++ )  Write( $"{RRa[k]}" );
                WriteLine();
#endif
            return RRa;
        }
        public class fnLine{
            private Point2d ptA;
            private Point2d ptB;
            private double ax;
            private double bx;
            private double ay;
            private double by;

            public fnLine( Point2d ptA, Point2d ptB ){
                this.ptA=ptA; this.ptB=ptB;
                double w=ptA.X-ptB.X;
                ax=bx=0.0;
                if(w!=0.0){ ax=(ptA.Y-ptB.Y)/w; bx=ptB.Y-ax*ptB.X; }

                w=ptA.Y-ptB.Y;
                ay=by=0.0;
                if(w!=0.0){ ay=(ptA.X-ptB.X)/w; by=ptB.X-ay*ptB.Y; }
            }
            public Point2d linePX(double X){ return (new Point2d(X,ax*X+bx)); }
            public Point2d linePY(double Y){ return (new Point2d(ay*Y+by,Y)); }
            public int linePXa(int X){ return (int)(ax*X+bx); }
            public int linePYa(int Y){ return (int)(ay*Y+by); }
        }
        public Point2d _2_MovePoint( Point2d pt0, Point2d ptA, Point2d ptB, int sft, fnLine fn ){
            //Move ptA by sft to ptB direction
            if(fn==null) fn=new fnLine(ptA,ptB);
            double dx=Math.Abs(ptA.X-ptB.X), dy=Math.Abs(ptA.Y-ptB.Y);
            if(dx>dy){
                double X=pt0.X + sft;
                return fn.linePX(X);
            }
            else{
                double Y=pt0.Y + sft;
                return fn.linePY(Y);
            }
        }
        public IEnumerable<Point> _3_GetViaPoint( Point ptA, Point ptB ){
            fnLine fn=new fnLine(ptA,ptB);
            double dx=Math.Abs(ptA.X-ptB.X), dy=Math.Abs(ptA.Y-ptB.Y);
            Point Pt=new Point();
            if(dx>dy){
                int sgn=(ptA.X>ptB.X)? -1: 1;
                for( int sft=0; ; sft+=sgn ){
                    int X=ptA.X+sft;
                    if(X==ptB.X) yield break;
                    Pt.X=X; Pt.Y=fn.linePXa(X);
                    yield return Pt; ;
                }
            }
            else{
                int sgn=(ptA.Y>ptB.Y)? -1: 1;
                for( int sft=0; ; sft+=sgn ){
                    int Y=ptA.Y+sft;
                    if(Y==ptB.Y) yield break;
                    Pt.Y=Y; Pt.X=fn.linePYa(Y);
                    yield return Pt;
                }
            }
        }

        private Mat _0A_StandardFrame( Mat frame, ref Point[] ptR, bool DispB ){ //input:gray
            Size    sz=frame.Size();
            int     W=sz.Width, H=sz.Height, szD=32*9, szDP=szD+20;
            Mat frame2 = new Mat(szDP,szDP,MatType.CV_8UC1);
            Point[] ptI=new Point[4];
            ptI[0]=new Point(10,10); ptI[1] = new Point(10+szD,10); ptI[2]=new Point(10+szD,10+szD); ptI[3] = new Point(10,10+szD); 
            PT.SetPoint(ptR,ptI);
            PT.ProjectionSolver();
            Point ptXY=new Point();
            unsafe{
                byte *S=frame.DataPointer;
                byte *D=frame2.DataPointer;
                for( int y=0; y<szDP; y++ ){
                    ptXY.Y=y;
                    for( int x=0; x<szDP; x++ ){
                        ptXY.X=x;
                        Point P=PT.Convert_ItoR(ptXY);
                        if(P.X>=0 && P.X<W && P.Y>=0 && P.Y<H )  *D= S[P.Y*W+P.X];
                        D++;
                    }
                }
            }
            ptR=ptI;
            if(DispB) new Window("_0A_StandardFrame", WindowMode.KeepRatio, frame2);
            return frame2;
        }

        //Board(81cells) recognition
        public Mat _2B_SDK_ProjectivTransformInv( Mat frame2, Point[] Q4W, bool DispB ){
            int rgN=regSize, Na=0, Nb=regSize, Nc=Na+Nb;
            Size sz = new Size(rgN,rgN);
            
            //射影変換設定
            Point[] reaXY={ new Point(Na,Na), new Point(Nc,Na), new Point(Nc,Nc), new Point(Na,Nc) };//■テンプレート枠
            
            PT.SetPoint(reaXY,Q4W);   //Projective transformation parameter set
            PT.ProjectionSolver();    //Calculation of projective transformation parameters

            byte b255=255, b0=0, thVal=128;
            Mat BlockImg = new Mat(sz,MatType.CV_8UC1);
            unsafe{
                int Hi=frame2.Height, Wi=frame2.Width, Wb=BlockImg.Width;
                byte *S=frame2.DataPointer;
                byte *D=BlockImg.DataPointer;

                Point Qi=new Point(), Qr=new Point();
                for( int y=0; y<rgN; y++ ){
                    Qr.Y=y;
                    for( int x=0; x<rgN; x++ ){
                        Qr.X=x;
                        Qi= PT.Convert_RtoI(Qr);
                        if( Qi.X<0 || Qi.X>=Wi || Qi.Y<0 || Qi.Y>=Hi ) continue;
                        D[y*Wb+x] = (S[Qi.Y*Wi+Qi.X]>thVal)? b255: b0;
                    }
                }
            }
            //    Cv2.ImWrite("BlockImg.jpg", BlockImg);

            if(DispB){
                new Window("SDK_MakeOutCel",WindowMode.KeepRatio,BlockImg); 
            }
            return BlockImg;
        }
      
        private int _2B_DigitRecognition( Mat MMcel, int rc0, bool resDispB ){
            //Borderline erase
            _ClearFrame(MMcel,sf:4,DispB:false); //sf:Width of frame to remove
            _IsAlmostEmpty(MMcel,rc0,cnt0:10);

            Mat MMnml = MLn3._PTNormalize(MMcel,0,220);    //Pattern normalization

            ULeData X=new ULeData(MMnml,-1);
            X.CalcFeature();
    //        MLn3.MLXA3.CalculateByDropOut(X,DoB:false,DispB:false); //判別
            
            bool DoB=(gammaC<1.0);
            MLn3.MLXA3.CalculateByDropOut(X,DoB:false,DispB:false); //判別

            return X.est;
        }
        private void _ClearFrame( Mat Img, int sf=4, bool DispB=false ){
            if(DispB){
                using( new Window("Before",WindowMode.KeepRatio,Img) ){ Cv2.WaitKey(0); }
            }

            int W=Img.Width, H=Img.Height;
            byte b128=(byte)128, b255=(byte)255; 
            Queue<int> Que=new Queue<int>();
            unsafe{
                byte *S0=Img.DataPointer;
                int rc;
                for( int r=0; r<H; r++ ){
                    if(r<sf || r>=H-sf){
                        for( int c=0; c<W; c++){
                            rc=r*W+c;
                            if(S0[rc]<b128){
                                S0[rc]=b255;
                                if(r>0   && S0[rc-W]<b128) Que.Enqueue(rc-W);
                                if(r<H-1 && S0[rc+W]<b128) Que.Enqueue(rc+W);
                                if(c>0   && S0[rc-1]<b128) Que.Enqueue(rc-1);
                                if(c<W-1 && S0[rc+1]<b128) Que.Enqueue(rc+1);
                            }
                        }
                    }
                    else{
                        for( int c=0; c<sf; c++){
                            rc=r*W+c;
                            if(S0[rc]<b128){
                                S0[rc]=b255;
                                if(r>0   && S0[rc-W]<b128) Que.Enqueue(rc-W);
                                if(r<H-1 && S0[rc+W]<b128) Que.Enqueue(rc+W);
                                if(c>0   && S0[rc-1]<b128) Que.Enqueue(rc-1);
                                if(c<W-1 && S0[rc+1]<b128) Que.Enqueue(rc+1);
                            }
                        }
                        for( int c=W-sf; c<W; c++){
                            rc=r*W+c;
                            if(S0[rc]<b128){
                                S0[rc]=b255;
                                if(r>0   && S0[rc-W]<b128) Que.Enqueue(rc-W);
                                if(r<H-1 && S0[rc+W]<b128) Que.Enqueue(rc+W);
                                if(c>0   && S0[rc-1]<b128) Que.Enqueue(rc-1);
                                if(c<W-1 && S0[rc+1]<b128) Que.Enqueue(rc+1);
                            }
                        }
                    }
                }

                while(Que.Count>0){
                    rc=Que.Dequeue();
                    *(S0+rc)=b255;
                    int r=rc/W, c=rc%W;
                    if(S0[rc]<b128){
                        S0[rc]=b255;
                        if(r>0   && S0[rc-W]<b128) Que.Enqueue(rc-W);
                        if(r<H-1 && S0[rc+W]<b128) Que.Enqueue(rc+W);
                        if(c>0   && S0[rc-1]<b128) Que.Enqueue(rc-1);
                        if(c<W-1 && S0[rc+1]<b128) Que.Enqueue(rc+1);
                    }
                }
                if(DispB){
                    using( new Window("After",WindowMode.KeepRatio,Img) ){ Cv2.WaitKey(0); }
                }
            }
        }
        private void _IsAlmostEmpty( Mat Img, int rc0, int cnt0=10 ){
            int W=Img.Width, H=Img.Height, WH=W*H;
            int[] pat={-W-1,-W,-W+1, -1,+1, W-1,W,W+1 };

            byte b128=(byte)128, b255=(byte)255;
            unsafe{
                byte *S=Img.DataPointer;
                //If the surrounding eight cells are W, change to W
                for( int r=0; r<H; r++ ){
                    for( int c=0; c<W; c++ ){
                        int rc=r*W+c;
                        if( *(S+rc)>b128 ) continue;
                        bool Bhit=false;
                        for( int k=0; k<pat.Length; k++ ){
                            int rc2=rc+pat[k];
                            if(rc2<0 || rc2>=WH) continue;
                            if(c==0 && rc2%W>1)  continue;
                            if(c==W-1 && rc2%W==0 ) continue;
                            if( *(S+rc2)>b128 ) continue;
                            Bhit=true;
                            break;
                        }
                        if(!Bhit) *(S+rc)=b255;//change to W
                    }
                }

                int cnt=0;
                for( int rc=0; rc<WH; rc++ ){
                    if(*S++<b128) cnt++;
                }
                if(cnt<cnt0){
                    S=Img.DataPointer;
                    for( int rc=0; rc<WH; rc++ ) *S++=b255;
                }
            }

        }

        private static void _DetectGridCheck(Mat frame2,Point2d[] Q16){
            Mat Q4Mat = frame2.CvtColor(ColorConversionCodes.GRAY2BGR);
            int cr = 0;
            for(int n=0; n<16; n++){
                Point Pa = new Point(Q16[n].X,Q16[n].Y);
                foreach(var m in link16[n].IEGet_BtoNo(16)){
                    //   if(n>m)  continue;
                    Point Pb = new Point(Q16[m].X,Q16[m].Y);
                    Scalar clr = G.colokAky[(cr++) % 6];
                    Q4Mat.Line(Pa,Pb,clr,2);
                }
            }
            using( new Window("_0_DetectGrid4",WindowMode.KeepRatio,Q4Mat) ){ Cv2.WaitKey(0); }
        }

        private Point2d[] _1_Point4ToPoint16( Point2d[] Q4 ){
            double a=1.0/3.0, b=2.0/3.0;
            var Q16 = new Point2d[16];
            Q16[0]=Q4[0];  Q16[3]=Q4[1]; Q16[12]=Q4[2]; Q16[15]=Q4[3];

            Q16[1]  = _2_Get_InterPolation(Q16[0],Q16[3],a);
            Q16[2]  = _2_Get_InterPolation(Q16[0],Q16[3],b);
            Q16[13] = _2_Get_InterPolation(Q16[12],Q16[15],a);
            Q16[14] = _2_Get_InterPolation(Q16[12],Q16[15],b);

            Q16[4]  = _2_Get_InterPolation(Q16[0],Q16[12],a);
            Q16[8]  = _2_Get_InterPolation(Q16[0],Q16[12],b);
            Q16[7]  = _2_Get_InterPolation(Q16[3],Q16[15],a);
            Q16[11] = _2_Get_InterPolation(Q16[3],Q16[15],b);

            Q16[5]  = _2_Get_InterPolation(Q16[1],Q16[13],a);
            Q16[9]  = _2_Get_InterPolation(Q16[1],Q16[13],b);
            Q16[6]  = _2_Get_InterPolation(Q16[2],Q16[14],a);
            Q16[10] = _2_Get_InterPolation(Q16[2],Q16[14],b);
            return Q16;
        }
        private double AdjustGrid81(Mat stdImg,Point2d[] Q100, int rc1, int rc1A, int rc1B, int rc2, int rc2A, int rc2B){
            int sfX=10, sfX2=5;
            double ev, evMax=0.0;
            int sf1Max=0, sf2Max=0;

            Point P1=Q100[rc1], P1A=Q100[rc1A], P1B=Q100[rc1B];
            Point P2=Q100[rc2], P2A=Q100[rc2A], P2B=Q100[rc2B];
            fnLine fn1=null, fn2=null;
            for(int sf1=sfX; sf1!=0; sf1=((sf1<0)? -(sf1+1):-sf1)){
                Point P1X = _2_MovePoint(P1,P1A,P1B,sf1,fn1);
                for(int sf2=sfX2; sf2!=0; sf2=((sf2<0)? -(sf2+1):-sf2)){
                    Point P2X = _2_MovePoint(P2,P2A,P2B,sf1+sf2,fn2);
                    ev = _2_Eval_LineMatching9(stdImg,P1X,P2X,128);
                    if(ev>evMax){ evMax=ev; sf1Max=sf1; sf2Max=sf1+sf2; }
                }
            }
            if(evMax>0){
                Q100[rc1] = _2_MovePoint(P1,P1A,P1B,sf1Max,fn1);
                Q100[rc2] = _2_MovePoint(P2,P2A,P2B,sf2Max,fn2);
            }
            return evMax;
        }
        private double _2_Eval_LineMatchingSel( Mat imgG, Point2d[] Q16, Point2d Pa, int lnk, int thVal ){
            double eval=0.0;
            Size sz=imgG.Size();
            int H=sz.Height, W=sz.Width;
            unsafe{
                byte *S=imgG.DataPointer;
                foreach( var m in lnk.IEGet_BtoNo(16) ){
                    Point Pb=Q16[m];
                    foreach( var pt in _3_GetViaPoint(Pa,Pb) ){
                        if(pt.X<0 || pt.Y<0 || pt.X>=W-1 || pt.Y>=H-1 )  continue;
                        byte B=S[pt.Y*W+pt.X];
                        eval += (B<thVal)? 1: 0; //already Binarized
                    }
                }
            }
            return eval;
        }
        private double _2_Eval_LineMatchingSel( Mat imgG, Point2d[] Q16, Point2d Pa, int lnk, int thVal, ref double evPer ){
            double eval=0.0,  evT=0.0;
            Size sz=imgG.Size();
            int H=sz.Height, W=sz.Width;
            unsafe{
                byte *S=imgG.DataPointer;
                foreach( var m in lnk.IEGet_BtoNo(16) ){
                    Point Pb=Q16[m];
                    foreach( var pt in _3_GetViaPoint(Pa,Pb) ){
                        if(pt.X<0 || pt.Y<0 || pt.X>=W-1 || pt.Y>=H-1 )  continue;
                        byte B=S[pt.Y*W+pt.X];
                        if(B<thVal) eval++; //already Binarized
                        evT++;
                    }
                }
                evPer = eval/evT;
            }
            return eval;
        }
        private double _2_Eval_LineMatching9( Mat imgG, Point2d Pa, Point2d Pb, int thVal ){
            double eval=0.0;
            Size sz=imgG.Size();
            int H=sz.Height, W=sz.Width;
            unsafe{
                byte *S=imgG.DataPointer;
                foreach( var pt in _3_GetViaPoint(Pa,Pb) ){
                    if(pt.X<0 || pt.Y<0 || pt.X>=W-1 || pt.Y>=H-1 )  continue;
                    byte B=S[pt.Y*W+pt.X];
                    eval += (B<thVal)? 1: 0; //already Binarized
                }
            }
            return eval;
        }
        private Point2d _2_Get_InterPolation( Point2d A, Point2d B, double r ){
          //return (A*(1.0-r)+B*r);
            return new Point( A.X*(1.0-r)+B.X*r+0.5, A.Y*(1.0-r)+B.Y*r+0.5);
        }
       
        private Point2d crossPoint( Point2d PT1, Point2d PT2, Point2d PT3, Point2d PT4 ){
            //Find the intersection of line segments (PT1,PT2) and straight lines (PT3,PT4).
            Point2d crossP;
	        double dev = (PT2.Y-PT1.Y)*(PT4.X-PT3.X)-(PT2.X-PT1.X)*(PT4.Y-PT3.Y);
	        if(Math.Abs(dev)<1.0e10){ crossP=new Point2d(double.MaxValue,double.MaxValue); }

	        double d1 = (PT3.Y*PT4.X-PT3.X*PT4.Y);
	        double d2 = (PT1.Y*PT2.X-PT1.X*PT2.Y);

	        crossP.X = d1*(PT2.X-PT1.X) - d2*(PT4.X-PT3.X);
	        crossP.X /= dev;
	        crossP.Y = d1*(PT2.Y-PT1.Y) - d2*(PT4.Y-PT3.Y);
	        crossP.Y /= dev;
            return crossP;
        }
    }

    public class PointEx{
        public Point2d Pt;
        public double Length;
        public double Angle;
        public PointEx( Point2d Pt, Point2d center ){
            this.Pt = Pt;
            Length = Point2d.Distance(Pt,center);
            Angle  = GDCV.Angle(Pt,center);
        }

        public override string ToString(){
            string st = string.Format("PointEx [X,Y]:[{0:0.#00},{1:0.#00}]", Pt.X, Pt.Y );
            st += string.Format("  Length:{0:0.#00}", Length );
            st += string.Format("  Angle:{0:0.#00}({1:0.#00})", Angle, Angle*180.0/Math.PI );
            return st;
        }
    }
}