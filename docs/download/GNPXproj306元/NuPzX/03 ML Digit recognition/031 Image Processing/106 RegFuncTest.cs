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

using OpenCvSharp;
using GIDOO_space;

namespace GIDOOCV{
    //Function approximation by regression
    public class RegFuncTest{
        public RegFuncTest(){ }

        public void Test(){
            List<PointEx> LL=new List<PointEx>();
            for( int k=0;  k<10; k++ ){
                double x=k+1;
                Point2d P=new Point2d( x, FuncG(x) );
                PointEx Q = new PointEx( P, new Point2d(1,1)  );
                LL.Add( Q );
                WriteLine("{0}, {1}", P.X, P.Y ); 
            }

            var FRegTop = new FuncApproximation(3,LL,CalculusB:true);

            for( int k=0;  k<10; k++ ){
                double X=k;
                double Y=LL[k].Pt.Y;
                double Ye=FRegTop.RegXY.Estimate(X);
                double Yd=FRegTop.RegXY.DiffEstimate(X);
                WriteLine("X:{0}  Y;{1}  Ye:{2}  Yd:{3}", X, Y, Ye, Yd );
            }
        }

        private double FuncG( double X ){
            double Y = 10 + 0.2*X + 0.03*X*X + Math.Sin(X)/100.0;
            return Y;
        }
    }
}