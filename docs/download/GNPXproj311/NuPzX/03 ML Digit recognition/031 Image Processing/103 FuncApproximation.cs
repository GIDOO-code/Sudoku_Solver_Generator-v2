using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
//using System.Windows;
using static System.Console;

//using MathNet.Numerics.LinearAlgebra;
//http://numerics.mathdotnet.com/api/MathNet.Numerics.LinearAlgebra.Double/DenseMatrix.htm
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
//http://www.maroon.dti.ne.jp/jyaku9/koneta/koneta7/koneta7-1.html

using OpenCvSharp;
using GIDOO_space;

using GNPZ_sdk;
namespace GIDOOCV{
    //Function approximation by regression
    public class FuncApproximation{
        private int         nSize;
        public  Regression  RegXY;
        public  Regression  RegXY1;
        private Regression  RegLX;
        public List<double> XLst;
        public List<double> YLst;
        public  bool        XYmode;

        public FuncApproximation( int nSize, List<PointEx> PTda, bool CalculusB=false, bool DispB=false ){ 
            this.nSize=nSize; 
            this.XLst = PTda.ConvertAll(P=> P.Pt.X);
            this.YLst = PTda.ConvertAll(P=> P.Pt.Y);
            double Xw=XLst.Max()-XLst.Min();
            double Yw=YLst.Max()-YLst.Min();

            XYmode = (Xw>Yw);
            if(!XYmode)  Swap<List<double>>(ref XLst,ref YLst );

            //======================================================
            double X1=XLst.Min(), X2=XLst.Max();
            double Y1=YLst.Min(), Y2=YLst.Max();
            
            Regression  RegXYmax=null;
            for(int n=1; n<=4; n++ ){ 
                RegXY = new Regression(n);
                if(n==1)  RegXY1=RegXY;
                RegXY.SetPoint_NthFunc(XLst,YLst);               
                RegXY.RegressionSolver(CalculusB:CalculusB);
                if(DispB) WriteLine("n:{0} R:{1:0.0000} {2}", n, RegXY.CorrelCoeff, RegXY.ToStringParaT() );
                double R=RegXY.CorrelCoeff;
                if(R<-0.5||R>1.0) continue;
                if(RegXYmax==null || R>RegXYmax.CorrelCoeff)  RegXYmax=RegXY;
                if(R>0.9999) break;
            }
            RegXY=RegXYmax;
            if(DispB){
                Write("=============== nSize:{0} R:{1:0.0000}", RegXY.nSize, RegXY.CorrelCoeff );
                WriteLine( RegXY.ToStringParaT() );
            }
            //------------------------------------------------------
        }
        public void Swap<T>( ref T a, ref T b ){ T w=b; b=a; a=w; }
        public double[] CreateFunction( Point2d PT0, double Pstart, double Pend, bool CalculusB=false ){ 
            double X1=PT0.X, Y1=PT0.Y;       
            List<double> Yest=new List<double>();
            foreach( var X in XLst ) Yest.Add(RegXY.Estimate(X));   //XLst -> Yest

            List<double> LenLst=new List<double>();
            LenLst.Add(0.0);

            double Xpre=X1, Ypre=Y1, L=0.0;
            for(int k=1; k<XLst.Count(); k++ ){
                double x=XLst[k], y=Yest[k];
                double xd=x-Xpre, yd=y-Ypre;
                L += Sqrt(xd*xd+yd*yd);
                LenLst.Add(L);
                Xpre=x; Ypre=y;
            }
            double per=(Pend-Pstart)/LenLst.Last();
            for(int k=0; k<XLst.Count(); k++ ) LenLst[k] = LenLst[k]*per+Pstart;

            RegLX = new Regression(nSize);
            RegLX.SetPoint_NthFunc(LenLst,XLst);
            RegLX.RegressionSolver(CalculusB:CalculusB);   //L -> Xest;
            return LenLst.ToArray();
        }
        
        public Point2d EstimatePt2Pt(Point2d P){
            double Xe=P.X, Ye=P.Y;
            if(XYmode) Ye=RegXY.Estimate(Xe);
            else       Xe=RegXY.Estimate(Ye);
            return (new Point2d(Xe,Ye));
        }
        public Point2d EstimateL2Pt(double L){   
            double Xe=RegLX.Estimate(L);　// L  -> Xe
            double Ye=RegXY.Estimate(Xe); // Xe -> Ye
            Point2d Pt= XYmode? (new Point2d(Xe,Ye)): (new Point2d(Ye,Xe));
            return Pt;
        }
        public double FuncPt2F(Point Pt){
            double F=0.0;
            if(XYmode) F= Pt.Y-RegXY.Estimate(Pt.X);
            else       F= Pt.X-RegXY.Estimate(Pt.Y);
            return F;
        }
        public double FuncPt2F(Point2d Pt){
            double F=0.0;
            if(XYmode) F= Pt.Y-RegXY.Estimate(Pt.X);
            else       F= Pt.X-RegXY.Estimate(Pt.Y);
            return F;
        }
        public void GetTangent( Point2d p0, ref double a, ref double b, ref double c){
            double x0=p0.X, y0=p0.Y;
            if(XYmode){
                double dx=RegXY.DiffEstimate(x0);
                y0=RegXY.Estimate(x0);
                a=dx; b=-1.0; c=dx*x0-y0;
            }
            else{
                double dy=RegXY.DiffEstimate(y0);
                x0=RegXY.Estimate(y0);
                a=1.0; b=-dy; c=x0-dy*y0;
            }
        }
        public void GetTangent1( Point2d p0, double e, ref double a, ref double b, ref double c){
            double x0=p0.X, y0=p0.Y;
            if(XYmode){
                y0=RegXY.Estimate(x0); 
                a=(RegXY.Estimate(x0+e)-y0)/e; b=-1.0; c=a*x0-y0;
            }
            else{
                x0=RegXY.Estimate(y0); 
                a=-1.0; b=(RegXY.Estimate(x0+e)-y0)/e; c=b*y0-x0;
            }
        }
        public void GetTangent( ref double a, ref double b, ref double c){
            DenseVector Para=RegXY1.Para; 
            if(XYmode){ a=-Para[1]; b=1.0; c=Para[0]; }
            else{       a=1.0; b=-Para[1]; c=Para[0]; }
        }

        private Point2d crossPoint( Point2d PT1, Point2d PT2, Point2d PT3, Point2d PT4 ){
            //Find the intersection of line segments(PT1,PT2), or straight lines(Infinity point)

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

  #region RegressionFuncUtility
    public class RegFuncUtilitySub{
        static double alp=0.618033988, bta=1.0-alp;
        private FuncApproximation F;
        private Point2d[] PtLst=new Point2d[4];
        private double[]  DstLst=new double[4];
        public RegFuncUtilitySub( FuncApproximation F, Point2d PtA, Point2d PtB){
            this.F=F;
            SetRange(PtA,PtB);
        }
        public void SetRange( Point2d PtA,Point2d PtB){
            PtLst[0] = PtA;
            PtLst[3] = PtB;
            PtLst[1] = PtA*alp+PtB*bta;
            PtLst[2] = PtA*bta+PtB*alp;
            for(int k=0; k<4; k++) PtLst[k] = F.EstimatePt2Pt(PtLst[k]);
        }
        public double __Solve( Point2d PtX, ref Point2d PtAns ){
            for(int k=0; k<4; k++ ) DstLst[k] = PtLst[k].DistanceTo(PtX);
            do{
                if( DstLst[1]<DstLst[2] ){
                    PtLst[3]=PtLst[2]; DstLst[3]=DstLst[2];
                    PtLst[2]=PtLst[1]; DstLst[2]=DstLst[1];
                    PtLst[1] = F.EstimatePt2Pt(PtLst[0]*bta+PtLst[2]*alp);
                    DstLst[1] = PtLst[1].DistanceTo(PtX);
                }
                else{ 
                    PtLst[0]=PtLst[1]; DstLst[0]=DstLst[1];
                    PtLst[1]=PtLst[2]; DstLst[1]=DstLst[2];
                    PtLst[2] = F.EstimatePt2Pt(PtLst[1]*alp+PtLst[3]*bta);
                    DstLst[2] = PtLst[2].DistanceTo(PtX);
                }
            }while( PtLst[2].DistanceTo(PtLst[1])>0.1);
            PtAns=(PtLst[1]+PtLst[2])*0.5;
            PtAns = F.EstimatePt2Pt(PtAns);
            return DstLst[1];
        }
        public Point2d Estimate(Point2d Pt){ return F.EstimatePt2Pt(Pt); }
    }

    public class RegFuncUtility{
        private FuncApproximation F1;
        private FuncApproximation F2;
        public  Mat ImgCheck;   //for DEBUG

        public RegFuncUtility( Mat ImgCheck=null){ this.ImgCheck=ImgCheck;}

        public Point2d IntersectionPoint( FuncApproximation F1, FuncApproximation F2, Point2d PtA, Point2d PtB, bool DispB=false ){
            this.F1=F1; this.F2=F2;
            Point Pt0 = (Point)((PtA+PtB)*0.5);
            RegFuncUtilitySub FS1=new RegFuncUtilitySub(F1,PtA,PtB);
            RegFuncUtilitySub FS2=new RegFuncUtilitySub(F2,PtA,PtB);

            Point2d PtAns=new Point2d();
            Mat resImg=null;
            try{
                if(DispB){
                    resImg=ImgCheck.CvtColor(ColorConversionCodes.GRAY2BGR); //Gray->Color変換
                    for(int x=0; x<ImgCheck.Width; x+=20 ){
                        Point P1=new Point(x,F1.RegXY.Estimate(x));
                        Point P2=P1+(new Point(4,4));
                        resImg.Rectangle(P1,P2,Scalar.Orange,3);
                    }
                    for(int y=0; y<ImgCheck.Height; y+=20 ){
                        Point P1=new Point(F2.RegXY.Estimate(y),y);
                        Point P2=P1+(new Point(4,4));
                        resImg.Rectangle(P1,P2,Scalar.Blue,3);
                     }
                    resImg.Circle(Pt0,10,Scalar.Red,5);
                    using( new Window("IntersectionPoint",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                }

                Point2d  PtAns2=F2.EstimatePt2Pt(Pt0), PtAns1=Pt0;
                int loop=5;
                while(--loop>0){
                    double vAns1 = FS1.__Solve(PtAns2,ref PtAns1);
                            if(DispB){
                                resImg.Circle((Point)PtAns2,5,Scalar.Blue,3);
                                resImg.Circle((Point)PtAns1,5,Scalar.Red,loop*2);
                                using( new Window("IntersectionPoint",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                            }
                    double vAns2 = FS2.__Solve(PtAns1,ref PtAns2);
                            if(DispB){
                                resImg.Circle((Point)PtAns2,5,Scalar.Red,3);
                                resImg.Circle((Point)PtAns1,5,Scalar.Blue,loop*2);
                                using( new Window("IntersectionPoint",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                            }
                    if(vAns1<0.01 && vAns2<0.01) break;
                    FS1.SetRange(PtA,PtB);
                    FS2.SetRange(PtA,PtB);
                }
                PtAns=FS1.Estimate(PtAns2);
            }
            catch(Exception e ){ WriteLine(e.Message+"\r"+e.StackTrace); }
            return PtAns;
        }
        public Point2d IntersectionPoint__x( FuncApproximation F1, FuncApproximation F2, Point2d PTX, Point2d PTXdummy, bool DispB ){
            this.F1=F1; this.F2=F2;
            Point2d PT=PTX, PTnxt=new Point2d(-9999,-9999);
            bool B=F1.XYmode;
            DenseMatrix M=new DenseMatrix(2,2), Minv;
            DenseVector V=new DenseVector(2);
            double a=0.0, b=0.0, c=0.0, a1=0.0, b1=0.0, c1=0.0, d=0;
            int loop;
             
            Mat resImg=null;
            if(DispB){
                    resImg=ImgCheck.CvtColor(ColorConversionCodes.GRAY2BGR); //Gray->Color変換
                    WriteLine();
            }
            for(loop=0; loop<20; loop++){ //Up to 20 times
                int r=2+loop*2;
                if(loop==0){
                    F1.GetTangent(ref a, ref b, ref c);
                    F2.GetTangent(ref a1, ref b1, ref c1);                  
                }
                else{
                    double e=Min(d*0.1,1.0);
                    F1.GetTangent1(PT,e,ref a, ref b, ref c);
                    F2.GetTangent1(PT,e,ref a1, ref b1, ref c1);
                }
                        if(DispB){                           
                            for(int x=0; x<ImgCheck.Width; x+=20 ){
                                Point P1=new Point(x,-a*x+c);
                                Point P2=P1+(new Point(r,r));
                                resImg.Rectangle(P1,P2,Scalar.Blue,3);
                            }
                            for(int y=0; y<ImgCheck.Height; y+=20 ){
                                Point P1=new Point(-b1*y+c1,y);
                                Point P2=P1+(new Point(r,r));
                                resImg.Rectangle(P1,P2,Scalar.Red,3);
                            }
                            using( new Window("IntersectionPoint",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                        }
                bool matrixB=true;
                if(Abs(a)<0.1 ){ PTnxt.Y=c/b;   PTnxt.X=F2.RegXY.Estimate(PTnxt.Y) ;matrixB=false; }
                if(Abs(a1)<0.1){ PTnxt.Y=c1/b1; PTnxt.X=F1.RegXY.Estimate(PTnxt.Y) ;matrixB=false; }
                if(Abs(b)<0.1 ){ PTnxt.X=c/a;   PTnxt.Y=F2.RegXY.Estimate(PTnxt.X) ;matrixB=false; }
                if(Abs(b1)<0.1){ PTnxt.X=c1/a1; PTnxt.Y=F1.RegXY.Estimate(PTnxt.X) ;matrixB=false; }

                if(DispB){
                    string st="IntersectionPoint loop:"+loop;
                    st += string.Format(" a:{0:0.0000} b:{1:0.0000} c:{2:0.0000}", a,b,c);
                    st += string.Format(" a1:{0:0.0000} b1:{1:0.0000} c1:{2:0.0000} ", a1,b1,c1 );
                    st += matrixB? "..": "◆";
                    st += string.Format("PT:(X:{0:0.0},Y:{1:0.0}) PTnxt:(X:{2:0.0},Y:{3:0.0})", PT.X,PT.Y, PTnxt.X,PTnxt.Y );
                    WriteLine(st);
                }
               
                if(matrixB){
                    M[0,0]=a; M[0,1]=b; V[0] =c;             
                    M[1,0]=a1; M[1,1]=b1; V[1] =c1;

                    Minv = (DenseMatrix)M.Inverse();
                    V = (DenseVector)Minv.Multiply(V);
                    PTnxt.X=V[0]; PTnxt.Y=V[1];
                }
                        if(DispB){
                            Point P1=(Point)PTnxt;
                            Point P2=P1+(new Point(r*5,r*5));
                            resImg.Rectangle(P1,P2,Scalar.Green,3);
                            using( new Window("IntersectionPoint",WindowMode.KeepRatio,resImg) ){ Cv2.WaitKey(0); } 
                        }
                d=PT.DistanceTo(PTnxt);
                if(d<1.0e-1) break;
                PT=PTnxt;
            }
            return  PT;
        }
        public Point2d IntersectionPointB( FuncApproximation F1, FuncApproximation F2, Point2d PtCenter, bool DispB ){
            Point2d PtA=new Point2d(0,0), PtB=new Point2d(PtCenter.X*2,PtCenter.Y*2);          
            Point2d[,] PTlst=new Point2d[3,3];
            while(true){
                double xMin= Min(PtA.X,PtB.X);
                double xMax= Max(PtA.X,PtB.X);
                double yMin= Min(PtA.Y,PtB.Y);
                double yMax= Max(PtA.Y,PtB.Y);
                double xSpn=(xMax-xMin)*0.5;
                double ySpn=(yMax-yMin)*0.5;
                if(xSpn<0.01 && ySpn<0.01 ) break;

                for(int r=0; r<3; r++ ){
                    for(int c=0; c<3; c++ ) PTlst[r,c] = new Point2d(xMin+xSpn*c,yMin+ySpn*r);
                }

                WriteLine();
                int k=0;
                foreach( var P in PTlst){
                    WriteLine( "{4}: {0:000.00},{1:000.00} {2:0.00} {3:0.00}",P.X,P.Y,F1.FuncPt2F((Point)P),F2.FuncPt2F((Point)P),++k);
                }

                for(int r=0; r<2; r++ ){
                    for(int c=0; c<2; c++ ){
                        PtA=PTlst[r,c]; PtB=PTlst[r+1,c+1];
                        if( F1.FuncPt2F((Point)PtA)*F1.FuncPt2F((Point)PtB)>0.0 ) continue;
                        if( F2.FuncPt2F((Point)PtA)*F2.FuncPt2F((Point)PtB)>0.0 ) continue;
                        goto LFond;
                    }
                }
                for(int r=0; r<2; r++ ){
                    for(int c=0; c<2; c++ ){
                        PtA=PTlst[r+1,c]; PtB=PTlst[r,c+1];
                        if( F1.FuncPt2F((Point)PtA)*F1.FuncPt2F((Point)PtB)>0.0 ) continue;
                        if( F2.FuncPt2F((Point)PtA)*F2.FuncPt2F((Point)PtB)>0.0 ) continue;
                        goto LFond;
                    }
                } 
               // WriteLine();
               // foreach( var P in PTlst) WriteLine("IntersectionPoint "+P );
              LFond:
                continue;
            }
            return PtB;
        }
        public Point2d IntersectionPointC( FuncApproximation F1, FuncApproximation F2, Point2d PtCenter, bool DispB ){
            Point2d PtA=new Point2d(0,0), PtB=new Point2d(PtCenter.X*2,PtCenter.Y*2);          
            Point2d[] PTlst=new Point2d[9];
            while(true){
              Lrepeat:
                double xMin= Min(PtA.X,PtB.X);
                double xMax= Max(PtA.X,PtB.X);
                double yMin= Min(PtA.Y,PtB.Y);
                double yMax= Max(PtA.Y,PtB.Y);
                double xSpn=(xMax-xMin)*0.5;
                double ySpn=(yMax-yMin)*0.5;
                if(xSpn<0.01 && ySpn<0.01 ) break;

                WriteLine(); 
                for(int rc=0; rc<9; rc++ ) PTlst[rc] = new Point2d(xMin+xSpn*(rc%3),yMin+ySpn*(rc/3));

                int k=0;
                foreach( var P in PTlst){
                    WriteLine( "{4}: {0:000},{1:000}   {2:0.00} {3:0.00}",P.X,P.Y,F1.FuncPt2F(P),F2.FuncPt2F(P),++k);
                }

                Combination cmb=new Combination(9,2);
                while(cmb.Successor()){
                    int s0=cmb.Index[0], s1=cmb.Index[1];
                    if(s0/3==s1/3 || s0%3==s1%3 || Abs((s0/3-s1/3)*(s0%3-s1%3))==4 ) continue;
                        //int s2=s1/3*3+s0%3, s3=s0/3*3+s1%3;
                        //WriteLine("   {0} {1} {2} {3}", s0, s1, s2, s3 ); 
                    PtA=PTlst[s0]; PtB=PTlst[s1];
                    if( F1.FuncPt2F(PtA)*F1.FuncPt2F(PtB)>0.0 ) continue;
                    if( F2.FuncPt2F(PtA)*F2.FuncPt2F(PtB)>0.0 ) continue;
                    
                    int s2=s1/3*3+s0%3, s3=s0/3*3+s1%3;
                    PtA=PTlst[s2]; PtB=PTlst[s3];
                    if( F1.FuncPt2F(PtA)*F1.FuncPt2F(PtB)>0.0 ) continue;
                    if( F2.FuncPt2F(PtA)*F2.FuncPt2F(PtB)>0.0 ) continue;
                    WriteLine("   {0} {1} {2} {3}", s0, s1, s2, s3 );
                    goto Lrepeat;
                } 

               // WriteLine();
               // foreach( var P in PTlst) WriteLine("IntersectionPoint "+P );
             }
            return PtB;
        }
    }
  #endregion RegressionFuncUtility
}