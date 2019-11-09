using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

using OpenCvSharp;

namespace GIDOOCV{
    public class Line2d{
        public Point2d Pa;
        public Point2d Pb;
        public double  a,b,c;
        public double  Length;
        public int     HicCount;

        public Line2d( Point2d Pax, Point2d Pbx ){
            Pa=Pax; Pb=Pbx;
            if(Pa.X>Pb.X)  __Swap(Pa,Pb);
            if( Pa.X==Pb.X && Pa.Y==Pb.Y )  return;
            double A=Pb.Y-Pa.Y, B=Pa.X-Pb.X, C=(Pa.Y-Pb.Y)*Pa.X+(Pb.X-Pa.X)*Pa.Y;
            double S=Math.Sqrt(A*A+B*B);
            a = A/S; b=B/S; c=C/S;
            Length = Math.Sqrt(A*A+B*B);
        }
        public Line2d( Point Paa, Point Pbb ): this( new Point2d(Paa.X,Paa.Y), new Point2d(Pbb.X,Pbb.Y) ){  }

        public double Dist_PtoL( Point2d P ){
            return Math.Abs(a*P.X+b*P.Y+c);
        }

        public Point2d footPoint( Point2d P ){ //垂線の足
            Point2d VP = P-(new Point2d(a,b))*(a*P.X+b*P.Y+c);
            return VP;
        }
        public bool InnerPoint_LineSeg( Point2d P ){
            Point2d Pf=footPoint(P);
            double Dab=Pa.DistanceTo(Pb);
            double Daf=Pa.DistanceTo(Pf);
            double Dbf=Pb.DistanceTo(Pf);
            return (Math.Min(Daf,Dbf)<Dab);
        }
        public double Dist_PtoLend( Point2d P ){
            double d2 = Math.Min( Pa.DistanceTo(P), Pb.DistanceTo(P) );
            return d2;
        }

        private  bool __test1( Point2d P, double keqPsNo=5 ){
            double DL=Dist_PtoL(P);
            if( InnerPoint_LineSeg(P) )  return (DL<keqPsNo);
            else{
                double Dpab = Math.Min( Pa.DistanceTo(P), Pa.DistanceTo(P) );
                return ( Dpab<keqPsNo );
            }
        }
        public void TestOverLap( List<Point2d>PLst, double keqPsNo=5 ){
            HicCount = PLst.Count( Q => __test1(Q,keqPsNo) );
           // WriteLine( $"Pa:{Pa} Pb:{Pb} HitCount:{HicCount}" );
        }

        private void __Swap( Point2d P, Point2d Q ){
            Point2d X=P; P=Q; Q=X;
        }
    }
}