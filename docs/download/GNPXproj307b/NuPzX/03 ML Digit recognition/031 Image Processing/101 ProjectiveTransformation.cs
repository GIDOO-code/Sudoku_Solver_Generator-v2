using System;
using System.Collections.Generic;
using System.Linq;

using static System.Console;

//using MathNet.Numerics.LinearAlgebra;
//http://numerics.mathdotnet.com/api/MathNet.Numerics.LinearAlgebra.Double/DenseMatrix.htm
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;

namespace GIDOOCV{

    //射影変換 ProjectiveTransformation
    public class ProjectiveTrans{
        private DenseMatrix M;       //Matrix<double>
        private DenseVector V;       //Vector<double>
        private DenseMatrix Minv;
        private DenseVector Para;
        private DenseVector Pinv;

        public ProjectiveTrans(){ }

        public void SetPoint( Point2d[] realXY, Point2d[] imageXY ){
            M = new DenseMatrix(8,8);
            V = new DenseVector(8);

            for(int k=0; k<4; k++ ){
                double ix=imageXY[k].X, iy=imageXY[k].Y;
		        double rx=realXY[k].X,  ry=realXY[k].Y;
		
		        int m=k;
		        M[m,0]=ix; M[m,1]=iy; M[m,2]=1.0;
                M[m,3]=M[m,4]=M[m,5]=0.0;
		        M[m,6]= -ix*rx; M[m,7]= -iy*rx;
		        V[m] = rx;

		        m = k+4;
		        M[m,0]=M[m,1]=M[m,2]=0.0;
		        M[m,3]=ix; M[m,4]=iy; M[m,5]=1.0;
		        M[m,6]= -ix*ry; M[m,7]= -iy*ry;
		        V[m] = ry;
            }
        }
        public void SetPoint( Point[] realXY, Point[] imageXY ){
            M = new DenseMatrix(8,8);
            V = new DenseVector(8);

            for(int k=0; k<4; k++ ){
                double ix=imageXY[k].X, iy=imageXY[k].Y;
		        double rx=realXY[k].X,  ry=realXY[k].Y;
		
		        int m=k;
		        M[m,0]=ix; M[m,1]=iy; M[m,2]=1.0;
                M[m,3]=M[m,4]=M[m,5]=0.0;
		        M[m,6]= -ix*rx; M[m,7]= -iy*rx;
		        V[m] = rx;

		        m = k+4;
		        M[m,0]=M[m,1]=M[m,2]=0.0;
		        M[m,3]=ix; M[m,4]=iy; M[m,5]=1.0;
		        M[m,6]= -ix*ry; M[m,7]= -iy*ry;
		        V[m] = ry;
            }
        }

        private double a0, b0, c0, d0, e0, f0, g0, h0;
        private double a1, b1, c1, d1, e1, f1, g1, h1;
        public bool ProjectionSolver( ){
            try{
                Minv = (DenseMatrix)M.Inverse();
                Para = (DenseVector)Minv.Multiply(V);

                double a=Para[0], b=Para[1], c=Para[2], d=Para[3], e=Para[4], f=Para[5], g=Para[6], h=Para[7];
                Pinv = new DenseVector(8);
                Pinv[0]=e-f*h; Pinv[1]=c*h-b;   Pinv[2]=b*f-c*e; Pinv[3]=f*g-d;
                Pinv[4]=a-c*g; Pinv[5]=c*d-a*f; Pinv[6]=d*h-e*g; Pinv[7]=b*g-a*h;
                double w=a*e-b*d;
                Pinv = Pinv/w;                         
                               // WriteLine( "Matrix\r"+M.ToString(8,8) );
                               // WriteLine( "Vector\r"+V.ToString() );
                               // WriteLine( "Minv\r"+Minv.ToString(8,8) );
                               // WriteLine( "Para\r"+Para.ToString() );
                a0=Para[0]; b0=Para[1]; c0=Para[2]; d0=Para[3]; e0=Para[4]; f0=Para[5]; g0=Para[6]; h0=Para[7];
                a1=Pinv[0]; b1=Pinv[1]; c1=Pinv[2]; d1=Pinv[3]; e1=Pinv[4]; f1=Pinv[5]; g1=Pinv[6]; h1=Pinv[7];
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message+"\r"+e.StackTrace);
            }
            return false;
        }

        public Point2d Convert_ItoR( Point2d P ){
            double x=P.X, y=P.Y;
            double den=g0*x+h0*y+1; //denominator 
            double X=(a0*x+b0*y+c0)/den, Y=(d0*x+e0*y+f0)/den;
            return new Point2d(X,Y);
        }
        public Point Convert_RtoI( Point2d P ){
            double x=P.X, y=P.Y;
            double den=g1*x+h1*y+1; //denominator 
            double X=(a1*x+b1*y+c1)/den, Y=(d1*x+e1*y+f1)/den;
            return new Point(X,Y);
        } 
        public  List<Line2d> EvalLines( List<LineSegmentPoint>  segProb, int ReqPsNo, int ReqHit=5 ){
            List<Line2d> SegLst = segProb.ConvertAll(Q=> new Line2d(Q.P1,Q.P2));
            List<Point2d> PLst = new List<Point2d>();
            segProb.ForEach( P => PLst.Add(P.P1) );
            segProb.ForEach( P => PLst.Add(P.P2) );

            SegLst.ForEach(Q => Q.TestOverLap(PLst,ReqPsNo) );
            SegLst = SegLst.FindAll(Q => Q.HicCount>ReqHit );
            return SegLst;
        }

        public Point   Convert_ItoR( Point P ){
            double x=P.X, y=P.Y;
            double den=g0*x+h0*y+1; //denominator 
            double X=(a0*x+b0*y+c0)/den, Y=(d0*x+e0*y+f0)/den;
            return new Point( (int)(X+0.5), (int)(Y+0.5) );
        }

        public Point Convert_RtoI( Point P ){
            double x=P.X, y=P.Y;
            double den=g1*x+h1*y+1; //denominator 
            double X=(a1*x+b1*y+c1)/den, Y=(d1*x+e1*y+f1)/den;
            return new Point( (int)(X+0.5), (int)(Y+0.5) );
        }

      #region How to use
        public void sample_Point2d(){
            Point2d[] reaXY={ new Point2d(10,10), new Point2d(40,15), new Point2d(12,50), new Point2d(60,60) };
            Point2d[] imgXY={ new Point2d(1,1), new Point2d(4,1), new Point2d(1,4), new Point2d(4,4) };

            SetPoint(reaXY,imgXY);
            if( ProjectionSolver() ){
                WriteLine( "Matrix\r"+M.ToString(8,8) );
                WriteLine( "Vector\r"+V.ToString() );

                WriteLine( "Inverse\r"+Minv.ToString(8,8) );
                WriteLine( "M*Minv\r"+(M.Multiply(Minv)).ToString(8,8) );

                WriteLine( "Parameter\r"+Para.ToString() );
                WriteLine( "Parameter\r"+Pinv.ToString() );

                Point2d P=new Point2d(2,4);
                Point2d Pa=Convert_ItoR(P);
                Point2d Pb=Convert_RtoI(Pa);

                WriteLine(P.ToString()+" "+Pa.ToString()+" "+Pb.ToString()+" ");
            }
            else WriteLine("Error");
        }
        public void sample_Point(){
            Point[] reaXY={ new Point(10,10), new Point(40,15), new Point(12,50), new Point(60,60) };
            Point[] imgXY={ new Point(1,1), new Point(4,1), new Point(1,4), new Point(4,4) };

            SetPoint(reaXY,imgXY);
            if( ProjectionSolver() ){
                WriteLine( "Matrix\r"+M.ToString(8,8) );
                WriteLine( "Vector\r"+V.ToString() );

                WriteLine( "Inverse\r"+Minv.ToString(8,8) );
                WriteLine( "M*Minv\r"+(M.Multiply(Minv)).ToString(8,8) );

                WriteLine( "Parameter\r"+Para.ToString() );
                WriteLine( "Parameter\r"+Pinv.ToString() );

                Point P=new Point(2,4);
                Point Pa=Convert_ItoR(P);
                Point Pb=Convert_RtoI(Pa);

                WriteLine(P.ToString()+" "+Pa.ToString()+" "+Pb.ToString()+" ");
            }
            else WriteLine("Error");
        }

        public void ProjectionTest(){    
            Point2d[] ptI = new Point2d[5];
            ptI[0] = new Point2d(1,1);
            ptI[1] = new Point2d(4,1);
            ptI[2] = new Point2d(1,4);
            ptI[3] = new Point2d(4,4);

            Point2d[] ptR = new Point2d[5];
            ptR[0] = new Point2d(10,10);
            ptR[1] = new Point2d(40,15);
            ptR[2] = new Point2d(12,50);
            ptR[3] = new Point2d(60,60);

            SetPoint(ptR,ptI);
            ProjectionSolver();

            WriteLine( "Matrix\r"+M.ToString(8,8) );
            WriteLine( "Vector\r"+V.ToString() );
            WriteLine( "Minv\r"+Minv.ToString(8,8) );
            WriteLine( "Para\r"+Para.ToString() );
        }
      #endregion How to use
    }
}