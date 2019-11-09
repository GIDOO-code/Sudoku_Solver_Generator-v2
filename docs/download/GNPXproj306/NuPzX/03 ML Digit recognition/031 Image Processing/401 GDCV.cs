using System;
using System.Collections.Generic;
using System.Linq;

using static System.Console;
using static System.Math;

//using MathNet.Numerics.LinearAlgebra;
//http://numerics.mathdotnet.com/api/MathNet.Numerics.LinearAlgebra.Double/DenseMatrix.htm
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using OpenCvSharp;
using GIDOO_Lib;

namespace GIDOOCV{
    public partial class GDCV{
        static GDCV(){ }
    
        //image processing
        static public Mat SubMat( Mat src, Rect rct ){ //Partial segmentation
            Mat des = new Mat( rct.Size, MatType.CV_8UC1 );
            int W=src.Width;
            unsafe{
                byte *S=src.DataPointer;
                byte *D=des.DataPointer;
                for( int y=0; y<rct.Height; y++ ){
                    int ys=rct.Top+y;
                    for( int x=0; x<rct.Width; x++ ){
                        int xs=rct.Left+x;
                        *D++ = S[ys*W+xs];
                    }
                }
            }
            return des;
        }
        static public void BuiltInMat( Mat MChk, Mat src, Rect rct ){ //Partial embedding
            int W=MChk.Width;
            unsafe{
                byte *S=src.DataPointer;
                byte *D=MChk.DataPointer;
                for( int y=0; y<rct.Height; y++ ){
                    int ys=rct.Top+y;
                    for( int x=0; x<rct.Width; x++ ){
                        int xs=rct.Left+x;
                        D[ys*W+xs] = *S++;
                    }
                }
            }
        }

        static public int[] Histogram( Mat src, bool DispB=false ){
            int[] hist=new int[256];
            Size sz=src.Size();
            int ln = sz.Width*sz.Height;

            unsafe{
                byte *S=src.DataPointer;
                for(int k=0; k<ln; k++ ) hist[*S++]++;
            }
                        //----- histogram Image -----
                        double hMax = hist.Max()/100.0;
                        Mat hMat = new Mat(300,400,MatType.CV_8UC3,Scalar.DarkGray);
                        for( int k=0; k<256; k++){
                            Scalar scr= (k%16==0)? Scalar.Red: Scalar.Blue;
                            hMat.Line(new Point(k+10,200-hist[k]/hMax+10), new Point(k+10,210),scr);
                        }
                        if(DispB)  new Window("Histgram",WindowMode.KeepRatio,hMat);
            return hist;
        }
        static public int[] Histogram( Mat src, Rect rct, bool DispB=false ){
            int[] hist=new int[256];
            int W=src.Width;
            unsafe{
                byte *S=src.DataPointer;
                for( int y=rct.Top; y<rct.Bottom; y++ ){
                    for( int x=rct.Left; x<rct.Right; x++ ){
                        hist[ S[y*W+x] ]++;
                    }
                }
            }
                        //----- histogram Image -----
                        double hMax = hist.Max()/100.0;
                        Mat hMat = new Mat(300,400,MatType.CV_8UC3,Scalar.DarkGray);
                        for( int k=0; k<256; k++){
                            Scalar scr= (k%16==0)? Scalar.Red: Scalar.Blue;
                            hMat.Line(new Point(k+10,200-hist[k]/hMax+10), new Point(k+10,210),scr);
                        }
                        if(DispB)  new Window("Histgram_Rect",WindowMode.KeepRatio,hMat);
            return hist;
        }

        static public byte GetPerValue( int[] H, double per){
            double U=H.Sum()*per, V=0.0;
            for(byte k=0; k<H.Length; k++) if((V+=H[k])>U) return k;
            return  (byte)(H.Length-1);
        }

        static public Mat EqualizeHist( Mat src, Mat des, double pL, double pH, bool DispB=false ){
            int[] hist=Histogram(src,DispB:false);
            double  perL=GetPerValue(hist,pL);
            double  perH=GetPerValue(hist,pH);

            if( (perH-perL)>1.0 ) _alpha=255.0/(perH-perL);
            else _alpha=1.0;
            _beta=perL;

            int W=src.Width, H=src.Height;
            unsafe{
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;
                for( int k=0; k<W*H; k++ ) (*D++) = _func((*S++));
            }
            if(DispB){
                new Window("EqualizeHist src",WindowMode.KeepRatio,src);                
                new Window("EqualizeHist des",WindowMode.KeepRatio,des);
            }
            return des;
        }

        static private byte b0=0, b255=255;
        static private double _alpha;
        static private double _beta;
        static private byte _func( double x ){
            double p=_alpha*(x-_beta);
            if(p<=0.0)  return b0;
            else if(p>=255) return b255;
            return (byte)p;
        }
        static public Mat Binarize( Mat src, byte thValue=128, bool DispB=false ){
            int H=src.Height, W=src.Width, WH=H*W;
            byte b255=(byte)255;
            Mat des = new Mat(src.Size(),MatType.CV_8UC1);
            unsafe{
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;
                for( int xy=0; xy<WH; xy++ ) (*D++) = ((*S++)<thValue)? b0: b255;  //S[xy];
            }
                    if(DispB)  new Window("Binarize",WindowMode.KeepRatio,des);
            return des;
        }

        static private int[] _SobelOpe={ -1,0,1, -2,0,2, -1,0,1 };
        static public Mat Sobel_1( Mat src, bool Lateral, bool DispB=false ){ //Lateral:T
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height;
            Mat des=new Mat(sz,MatType.CV_8UC1); //,Scalar.White);

            unsafe{
                int k, xb, yb, ss, V;
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;

                for( int x=0; x<W; x++ ){
                    for( int y=0; y<H; y++ ){
                        k=0; V=0;
                        if(Lateral){
                            for( int ya=-1; ya<=1; ya++ ){
                                if((yb=y+ya)<0 || yb>=H)       continue;
                                for( int xa=-1; xa<=1; xa++ ){    //Lateral direction
                                    if((xb=x+xa)<0 || xb>=W)   continue;
                                    if((ss=_SobelOpe[k++])==0) continue;  
                                    V += S[yb*W+xb]*ss;
                                } 
                            }
                        }
                        else{
                            for( int xa=-1; xa<=1; xa++ ){
                                if((xb=x+xa)<0 || xb>=W)       continue;
                                for( int ya=-1; ya<=1; ya++ ){    //Longitudinal direction
                                    if((yb=y+ya)<0 || yb>=H)   continue;
                                    if((ss=_SobelOpe[k++])==0) continue;  
                                    V += S[yb*W+xb]*ss;
                                } 
                            }
                        }
                        D[y*W+x] = (byte)((V>0)? V: 0);
                    }
                }
            }
                    if(DispB){ new Window("Sobel_1",WindowMode.KeepRatio,des); }
            return des;
        }
        static public Mat Sobel_2( Mat src, bool type4, bool DispB=false ){ //type4:T 4directions F:8directions
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height;
            Mat  des=new Mat(sz,MatType.CV_8UC1); //,Scalar.White);

            unsafe{
                int xx, yy, V, n;
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;

                for( int y=0; y<H; y++ ){
                    for( int x=0; x<W; x++ ){
                        V=0; n=0;
                        for( int ys=-1; ys<=1; ys++ ){
                            int yb=(ys==0)? 0: 1;
                            if((yy=y+ys)<0 || yy>=H)       continue;
                            for( int xs=-1; xs<=1; xs++ ){
                                int xb=(xs==0)? 0: 1;
                                if((xx=x+xs)<0 || xx>=W)   continue;
                                if(type4){ if((yb^xb)==0) continue; }
                                else{      if( ys==0 && xs==0 ) continue; }
                                V += S[yy*W+xx]; n++;
                            } 
                        }
                        int Q = V - S[y*W+x]*n;
                        if(Q<0)   Q=0;
                        if(Q>255) Q=255;
                        D[y*W+x] = (byte)Q;
                    }
                }
            }
                    if(DispB){ new Window("Sobel_2",WindowMode.KeepRatio,des); }
            return des;
        }

        static public Mat Blur( Mat src, int size, bool DispB=false ){ 
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height, mp=size/2;
            Mat  des=new Mat(sz,MatType.CV_8UC1); //,Scalar.White);

            unsafe{
                int xx, yy, V, n;
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;

                for( int y=0; y<H; y++ ){
                    for( int x=0; x<W; x++ ){
                        V=0; n=0;
                        for( int ys=-mp; ys<=mp; ys++ ){
                            int yb=(ys==0)? 0: 1;
                            if((yy=y+ys)<0 || yy>=H)       continue;
                            for( int xs=-mp; xs<=mp; xs++ ){
                                int xb=(xs==0)? 0: 1;
                                if((xx=x+xs)<0 || xx>=W)   continue;
                                V += S[yy*W+xx]; n++;
                            } 
                        }
                        int Q = (int)((V+0.5)/n);
                        if(Q<0)   Q=0;
                        if(Q>255) Q=255;
                        D[y*W+x] = (byte)Q;
                    }
                }
            }
                    if(DispB){ new Window("Blur-"+size,WindowMode.KeepRatio,des); }
            return des;
        }

        //GaussianBlur
        static private double[] _GaussianPar={ 2,4,5,4,2, 4,9,12,9,4, 5,12,15,12,5, 4,9,12,9,4, 2,4,5,4,2 };
        static public Mat GaussianBlur( Mat src, bool DispB=false ){ 
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height, mp=2;
            Mat  des=new Mat(sz,MatType.CV_8UC1);

            unsafe{
                int xx, yy;
                double V, n, gp;
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;

                for( int y=0; y<H; y++ ){
                    for( int x=0; x<W; x++ ){
                        V=0; n=0;
                        for( int ys=-mp; ys<=mp; ys++ ){
                            int yb=(ys==0)? 0: 1;
                            if((yy=y+ys)<0 || yy>=H)       continue;
                            for( int xs=-mp; xs<=mp; xs++ ){
                                int xb=(xs==0)? 0: 1;
                                if((xx=x+xs)<0 || xx>=W)   continue;
                                gp = _GaussianPar[(mp+ys)*5+(mp+xs)];
                                V += S[yy*W+xx]*gp; n+=gp;
                            } 
                        }
                        int Q = (int)(V/n);
                        if(Q<0)   Q=0;
                        if(Q>255) Q=255;
                        D[y*W+x] = (byte)Q;
                    }
                }
            }
                    if(DispB){ new Window("GaussianBlur",WindowMode.KeepRatio,des); }
            return des;
        }

        //Lightness adjustment
        static public Mat ContrastConditioning( Mat src, byte SMin, byte SMax, byte DMin, byte DMax, bool DispB=false ){
            double a=(double)(DMax-DMin)/(double)(SMax-SMin), b=DMin-a*SMin;

            int WH = src.Height*src.Width;
            Mat des = new Mat(src.Size(),MatType.CV_8UC1);
            unsafe{
                byte *S=src.DataPointer;
                byte *D=des.DataPointer;
                for( int k=0; k<WH; k++ ){
                    byte x=*S++;
                    byte p=DMin;
                    if(x>SMin){
                        if(x>SMax) p=DMax;
                        else p=(byte)(a*x+b);
                    }
                    *D++ = p;
                }
            }
                    if(DispB)  new Window("ContrastConditioning",WindowMode.KeepRatio,des);
            return des;
        }
        static public Mat Reverse( Mat src, bool DispB=false ){
            int H=src.Height, W=src.Width, HW=H*W;
            Mat des = new Mat(src.Size(),MatType.CV_8UC1);
            unsafe{
                byte *S=src.DataPointer;
                byte *D=des.DataPointer;
                for( int k=0; k<HW; k++ ){
                    *D++ = (byte)(255-(*S++));
                }
            }
                    if(DispB)  new Window("Reverse",WindowMode.KeepRatio,des);
            return des;
        }
        static public Mat DisplyHistgram( string title, Mat src, Scalar Clr ){
            Mat Mhist=new Mat(120,276,MatType.CV_8UC3,Clr);
            int[] hist = src.sdk_CalcHist();

            Mhist.Line(new Point(10,10), new Point (10,110),Scalar.White,1);
            Mhist.Line(new Point(10,110), new Point (265,110),Scalar.White,1);
            for( int k=0; k<=256; k+=32 ){
                int m=(k%64)==0? 6: 3;
                Mhist.Line(new Point(10+k,110), new Point (10+k,110+m),Scalar.White,1);
            }

            double hMax=100.0/hist.Max();
            for( int k=0; k<256; k++ ){
                if(hist[k]==0.0)  continue;
                int h= 110-(int)(hist[k]*hMax);
                Mhist.Line(new Point(k+10,110), new Point (k+10,h),Scalar.White,1);
            }
            new Window(title,WindowMode.KeepRatio,Mhist);
            return Mhist;
        }

        static private byte[] _kernel4={0,1,0, 1,1,1, 0,1,0}; // cross (+)
        static private byte[] _kernel8={1,1,1, 1,0,1, 1,1,1};
        static public Mat Erode( Mat src, int kernelType=8, bool DispB=false ){//Shrinking (Input is binary image）
            return _subErodeDilate( src,Erode:true,kernelType:kernelType,DispB:DispB);
        }
        static public Mat Dilate( Mat src, int kernelType=8, bool DispB=false ){//Expansion(Input is binary image）
            return _subErodeDilate( src,Erode:false,kernelType:kernelType,DispB:DispB);
        }
        static private Mat _subErodeDilate( Mat src, bool Erode=true, int kernelType=8, bool DispB=false ){
            //src:Binary image(0 or 255)

            byte ED=(byte)(Erode? 255: 0);
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height;
            byte[] kernel=(kernelType==4)? _kernel4: _kernel8;

            Mat des=new Mat(sz,MatType.CV_8UC1,Scalar.White); //,Scalar.White);
            unsafe{
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;
                for( int y=0; y<H; y++ ){
                    for( int x=0; x<W; x++){
                        byte BW=S[y*W+x];
                        if(BW!=ED){
                            int k=0;
                            for( int ky=-1; ky<2; ky++ ){
                                for( int kx=-1; kx<2; kx++ ){
                                    if(kernel[k++]==0)  continue;
                                    int yy=y+ky; if(yy<0||yy>=H) continue;
                                    int xx=x+kx; if(xx<0||xx>=W) continue;
                                    if( S[yy*W+xx]==ED ){ *D=ED; break; }
                                }
                            }
                        }
                        D++;
                    }
                }
            }
            if(DispB) new Window("Erode/Dilate",WindowMode.KeepRatio,des);
            return des;
        }

        static public Mat Sharp( Mat src, int k, int kernelType=8, bool DispB=false ){
            //src:Gray image(0-255)
            Size sz=src.Size();
            int  W=sz.Width, H=sz.Height;
            byte[] kernel=(kernelType==4)? _kernel4: _kernel8;
            int[]  kernelK=new int[9];
            for( int n=0; n<9; n++ ) kernelK[n] = -kernel[n]*k;
            kernelK[4] = kernelType*k+1;

            Mat des=new Mat(sz,MatType.CV_8UC1,Scalar.White);
            unsafe{
                byte *S = src.DataPointer;
                byte *D = des.DataPointer;
                for( int y=0; y<H; y++ ){
                    for( int x=0; x<W; x++){
                        int n=0, val=0, nx=0;
                        for( int ky=-1; ky<2; ky++ ){
                            for( int kx=-1; kx<2; kx++ ){
                                int kerW=kernelK[n++];
                                if(kerW==0)  continue;
                                if(ky==0 && kx==0 )  continue;
                                int yy=y+ky; if(yy<0||yy>=H) continue;
                                int xx=x+kx; if(xx<0||xx>=W) continue;
                                val += S[yy*W+xx] * kerW;
                                nx -= kerW;
                            }
                        }
                        val += (int)(S[y*W+x])*(1+nx);
                        val = Min(val,255);
                        *D++ = (byte)Max(val,0); 
                    }
                }
            }
            if(DispB) new Window("Sharp",WindowMode.KeepRatio,des);
            return des;
        }
        static public void DisplyHistgram2( Mat A, Mat B, Scalar BgClr, Scalar ClrA, Scalar ClrB ){
            int[] HA=A.sdk_CalcHist();
            int[] HB=B.sdk_CalcHist();

            Mat DA=new Mat(120,276,MatType.CV_8UC3,BgClr);

            double hMax= 100.0/HA.Max();
            int p=110-(int)(HA[0]*hMax);
            for( int k=1; k<256; k++ ){
                int h= 110-(int)(HA[k]*hMax);
                DA.Line(new Point(k+10,p), new Point (k+10,h),ClrA,1);
                p=h;
            }
            hMax= 100.0/HB.Max();
            p=110-(int)(HB[0]*hMax);
            for( int k=1; k<256; k++ ){
                int h= 110-(int)(HB[k]*hMax);
                DA.Line(new Point(k+10,p), new Point (k+10,h),ClrB,1);
                p=h;
            }

            DA.Line(new Point(10,10), new Point (10,110),Scalar.White,1);
            DA.Line(new Point(10,110), new Point (265,110),Scalar.White,1);
            for( int k=0; k<=256; k+=32 ){
                int m=(k%64)==0? 6: 3;
                DA.Line(new Point(10+k,110), new Point (10+k,110+m),Scalar.White,1);
            }

            new Window("Histgram",WindowMode.KeepRatio,DA);
        }
        static public int CheckHistgram(double[] Hst){
            int lng=Hst.Length;
            double T=Hst.Sum(), T50=T/2.0, P50=0.0, Q=0.0;
            int    k50=0;
            for( int k=0; k<lng/2; k++ ) P50+=Hst[k];
            if(P50>T*0.8)  return 1;
            for( int k=0; k<lng; k++ ){ if((Q+=Hst[k])>T50){ k50=k; break; } }
            if(k50<150)  return 2;
            return 10;
        }
             
        static public double Distance( Point2d pt0, Point2d pt) {
            double dx=pt0.X-pt.X, dy=pt0.Y-pt.Y;
            return (Math.Sqrt(dx*dx+dy*dy));
        } 

        static public double Angle(  Point2d P1, Point2d P2 ){ return (Angle((Point2d)(P1-P2))); }
        static public double Angle(  Point2d P ){
            double PX=P.X, PY=P.Y;
            double t;
            if(Math.Abs(PX)>0.1){
                t = Math.Atan(PY/PX);
                if(PX<0.0) t+=Math.PI;
            }
            else{
                t = Math.Atan(PX/PY);
                if(PY<0.0) t+=Math.PI;
                t = Math.PI/2-t;
            }
            if(t<0.0) t += Math.PI*2.0;
            return t;
        }
        static public Point  Center(  Point[] PTs ){
            Point Center = new Point(PTs.Average(P=>P.X),PTs.Average(P=>P.Y));
            return Center;
        }
    }
}