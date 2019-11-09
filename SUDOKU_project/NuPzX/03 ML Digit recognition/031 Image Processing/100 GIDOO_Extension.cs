using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static System.Console;

using OpenCvSharp;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GIDOO_Lib{
    static public class IEnumerableExtensions{      
        static public void Apply<T>( this IList<T> list, Func<T,int,T> func ){ //Apply the specified function to all list elements
            for(int k=0; k<list.Count; k++ )  list[k] = func(list[k],k);
        }
        static public void Apply<T>( this IList<T> list, Func<T,T> func ){    //Apply the specified function to all list elements
            for(int k=0; k<list.Count; k++ )  list[k] = func(list[k]);
        }
        static public void Apply<T>( this IList<T> lst, IList<T> resultLst, Func<T,T> func ){ //Apply the specified function to all list elements
            for(int k=0; k<lst.Count; k++ )  resultLst[k] = func(lst[k]);
        }
        static private double[] xF={ 1.0, -1.0, -1.0, 1.0 };
        static private double[] yF={ 1.0, 1.0, -1.0, -1.0 };
        static public Point[] RotatedRectToRect( this RotatedRect R ){
            Point[] PL=new Point[4];
            double x0=R.Center.X, y0=R.Center.Y, H=R.Size.Height/2.0, W=R.Size.Width/2.0;
            double a0=R.Angle/180.0*Math.PI, xs, ys, SN=Math.Sin(a0), CN=Math.Cos(a0);
            for(int k=0; k<4; k++ ){
                xs= W*xF[k]; ys=H*yF[k];
                PL[k] = new Point( x0+xs*CN-ys*SN, y0+xs*SN+ys*CN );
            }
            return PL;
        }

        static public DenseMatrix VecVecToMatrix( this DenseVector P, DenseVector Q ){
            DenseMatrix R = new DenseMatrix(P.Count,Q.Count);
            for(int r=0; r<P.Count; r++ ){
                for(int c=0; c<Q.Count; c++ )  R[r,c]=P[r]*Q[c];
            }
            return R;
        }

        // Minimum value element(list.FindMin(c=> func(c))
        static public TSource FindMin<TSource, TResult>( this IEnumerable<TSource> self, Func<TSource, TResult> selector ){
            return self.First(c=> selector(c).Equals(self.Min(selector)));
        }
    
        // Maximum value element(list.FindMax(c=> func(c))
        static public TSource FindMax<TSource, TResult>( this IEnumerable<TSource> self, Func<TSource, TResult> selector ){
            return self.First(c=> selector(c).Equals(self.Max(selector)));
        }

        static public Point ToPoint( this Point2d P){ return new Point( (int)(P.X+0.5), (int)(P.Y+0.5) ); }
        static public Point2d ToPoint( this Point P){ return new Point2d(P.X, P.Y); }

        static public string ToBitString( this int num, int ncc ){
            int numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? (k+1).ToString(): ".";
                numW >>= 1;
            }
            return st;
        }
        static public string ToBitStringN( this int num, int ncc ){
            int numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                if( (numW&1)!=0 ) st += " "+k.ToString();
                numW >>= 1;
            }
            if( st=="" )  st = "-";

            return st;
        }
        static public IEnumerable<int> IEGet_BtoNo( this int noBin, int sz ){
            for(int no=0; no<sz; no++ ){
                if( (noBin&(1<<no))>0 ) yield return no;
            }
            yield break;
        }

        static public int[] sdk_CalcHist( this Mat src ){
            int[] hist=new int[256];
            int HW = src.Height*src.Width;
            unsafe{
                byte *S=src.DataPointer;
                for(int k=0; k<HW; k++ )  hist[*S++]++;
            }
            return hist;
        }

        //(practice)
        static public Mat Displace( this Mat Uimg, int sftX, int sftY, bool DispB ){ //位置移動
            Size sz=Uimg.Size();
            int W=sz.Width , H=sz.Height;
            Mat UimgD = new Mat(sz,MatType.CV_8UC1,Scalar.All(255));

            {
                using( MatOfByte imgMBS=new MatOfByte(Uimg) )
                using( MatOfByte imgMBD=new MatOfByte(UimgD) ){
                    var IdxS = imgMBS.GetIndexer();
                    var IdxD = imgMBD.GetIndexer();
                    for(int y=0; y<H; y++ ){
                        int yy = y+sftY;
                        if( yy<0 || yy>=H ) continue;
                        for(int x=0; x<W; x++ ){
                            int xx = x+sftX;
                            if( xx<0 || xx>=W-1 )  continue;
                            IdxD[yy,xx] = IdxS[y,x];
                        }
                    }
                }
            }

            if(DispB){
            //    using(new Window("origin",WindowMode.Normal,Uimg))
                using(new Window("Displaced",WindowMode.Normal,UimgD)){ Cv2.WaitKey(0); }
            }
            return UimgD;
        }

        //(practice)
        static public Mat Enlarge( this Mat Uimg, double alphaX, double alphaY ){
            Size sz=Uimg.Size();
            int W=sz.Width, H=sz.Height, h2=H/2;
            Mat UimgD = new Mat(sz,MatType.CV_8UC1,Scalar.All(255));
            unsafe{
                byte *S = Uimg.DataPointer;
                byte *D = UimgD.DataPointer;
                for(int y=0; y<H; y++ ){
                    int yy = (int)((y-h2)/alphaY+0.5+h2);
                    if( yy<0 || yy>=H ) continue;
                    for(int x=0; x<W; x++ ){
                        int xx = (int)(x/alphaX+0.5);
                        if( xx<0 || xx>=W-1 )  continue;
                        D[y*W+x] = S[yy*W+xx];
                    }
                }
            }

            return UimgD;
        }

        //Set the dot to white at random(probability P)
        static private Random rndRW=new Random(11);
        static public Mat RandomWhite( this Mat Uimg, double rndP, bool DispB){           
            Size sz=Uimg.Size();
            int W=sz.Width , H=sz.Height, h2=H/2;
            Mat UimgD = Uimg.Clone(); // new Mat(sz,MatType.CV_8UC1,Scalar.All(255));

            {
                using( MatOfByte imgMBS=new MatOfByte(Uimg) )
                using( MatOfByte imgMBD=new MatOfByte(UimgD) ){
                    var IdxS = imgMBS.GetIndexer();
                    var IdxD = imgMBD.GetIndexer();
                    for(int y=0; y<H; y++ ){
                        for(int x=0; x<W; x++ ){
                            if(IdxS[y,x]==255)  continue;
                            if(rndRW.NextDouble()<rndP) IdxD[y,x]=255;
                        }
                    }
                }
            }

            if(DispB){
            //    using(new Window("origin",WindowMode.Normal,Uimg))
                using(new Window("RandomWhite",WindowMode.Normal,UimgD)){ Cv2.WaitKey(0); }
            }

            return UimgD;
        }
    }
}