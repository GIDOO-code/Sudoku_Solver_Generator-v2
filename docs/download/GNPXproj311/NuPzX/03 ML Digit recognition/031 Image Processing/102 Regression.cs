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

namespace GIDOOCV{
    public class Regression{
        public  int         nSize;
        private DenseMatrix M;       //Matrix<double>
        private DenseVector V;       //Vector<double>
        private double      Y2;
        private DenseMatrix Minv;
        public  DenseVector Para;    //regression coefficient
        private DenseVector ParaD;   //regression coefficient(Differential)
        private DenseVector ParaI;   //regression coefficient(Integral)
        private DenseVector Vdata;

        private bool        CalculusB;
        public  int         sampleNo;
        public  double[]    stddevCoef;
        public  double[]    tCoef;
        public  double[]    statLst;
        public  double      CorrelCoeff;

        public Regression( int nSize ){ 
            this.nSize=nSize;
            M = new DenseMatrix(nSize+1,nSize+1);
            V = new DenseVector(nSize+1);
            Vdata = new DenseVector(nSize+1);
        }

        public void SetPoint_NthFunc( List<double> XLst,List<double> YLst ){
            SetPoint_NthFunc( XLst.ToArray(), YLst.ToArray() );
        }
        public void SetPoint_NthFunc( double[] XLst, double[] YLst ){
            for(int n=0; n<XLst.Count(); n++ ) SetPoint_NthFunc(XLst[n],YLst[n]);
        }
        public void SetPoint_NthFunc( double X, double Y ){
            double P=1.0;
            for(int k=0; k<=nSize; k++ ){ Vdata[k]=P; P*=X; }
            M += VecVecToMatrix(Vdata);
            V += Vdata*Y;
            Y2 += Y*Y;
        }
        public void SetData_Vector( double[] X, double Y ){
            for(int k=0; k<=nSize; k++ ){ Vdata[k]=X[k]; }
            M += VecVecToMatrix(Vdata);
            V += Vdata*Y;
            Y2 += Y*Y;
        }

        private DenseMatrix VecVecToMatrix( DenseVector V ){
            DenseMatrix M=new DenseMatrix(nSize+1,nSize+1);
            for(int r=0; r<=nSize; r++ ){
                double X=V[r];
                for(int c=0; c<=nSize; c++ ) M[r,c] = X*V[c];
            }
            return M;
        }

        public DenseVector RegressionSolver( bool CalculusB=false, bool dispB=false ){
            this.CalculusB=CalculusB;
            try{
                CorrelCoeff = -9.0;
                Minv = (DenseMatrix)M.Inverse();
                Para = (DenseVector)Minv.Multiply(V);
                if(CalculusB){    //integral/differential parameter
                    ParaD=new DenseVector(nSize+1);
                    ParaI=new DenseVector(nSize+1);
                    for(int k=0; k<nSize; k++ ){
                        ParaD[k]=Para[k]*k;     //ParaD[0]=0
                        ParaI[k]=Para[k]/(k+1);
                    }
                }

              #region statistics
                //*** Dispersion (Overall: sr, regression: sy, residual: se) ***
                int ns =(int)M[0,0];
	            double yav = V[0]/ns;
	            double sy  = Y2 -(double)ns * yav * yav;
                double bxy = Para.DotProduct(V);
	            double sr  = bxy-(double)ns * yav * yav;
	            double se  = sy-sr;
                double sig2= se/(double)(ns-nSize-1);

                //*** Multiple correlation coefficient, F value ***
	            double r2 = sr/sy;
                CorrelCoeff = Sqrt(r2);

	            double rr2 = 1.0-(double)(ns-1)/(double)(ns-nSize-1) * (1.0-r2);
                double F = sr/(double)nSize/(se/(double)(ns-nSize-1));
                double prb = FValue(F,nSize,ns-nSize-1); //Hypothesis test by F value(Risk factor)
       
                //*** AIC ***
	            double theta = se/(double)(ns);
	            double AIC = ns*Log(theta) + 2.0*(nSize+1.0);

                //*** Standard deviation of regression coefficient ***
                stddevCoef=new double[nSize+1];
	            for(int k=0; k<=nSize; k++ ) stddevCoef[k] = Sqrt(sig2*Minv[k,k]);

                //*** T value of coefficient ***
                tCoef=new double[nSize+1];
                double   chkV=0.0;//Test for coefficient=0
	            for(int k=0; k<=nSize; k++ ) tCoef[k] = (Para[k]-chkV)/stddevCoef[k];

                sampleNo=ns;
                //r2 = Max(r2,0.0);
                //rr2 = Max(rr2,0.0);
                statLst=new double[]{ CorrelCoeff, r2, Sqrt(rr2), rr2, sig2, sy, sr, se, F, prb, AIC };
                if(dispB) regressionResult();

              #endregion
                return Para;
            }
            catch(Exception e){
                Console.WriteLine(e.Message+"\r"+e.StackTrace);
            }
            return null;
        }
        public void regressionResult(){
            WriteLine("M\r"+M);
            WriteLine("V\r"+V);
            WriteLine("Minv\r"+Minv);
            WriteLine("<*Minv\r"+(M*Minv));

            WriteLine("ns={0}\nr={1}  r2={2}\nr*={3:0.0000}  r*2={4:0.0000}\n", sampleNo, statLst[0],statLst[1], statLst[2], statLst[3]);
		    WriteLine("sig2={0:0.0000}  sy={1:0.0000}  sr={2:0.0000}  se={3:0.0000}\n", statLst[4], statLst[5], statLst[6], statLst[7] );
		    WriteLine("f={0:0.0000}  prb={1:0.0000}  aic={2:0.0000}\n", statLst[8], statLst[9], statLst[10] );
		    WriteLine("No.     para   stdDev  t-value");
            for(int k=0; k<=nSize; k++ ){
			    WriteLine("  {0}   {1:0.0000}   {2:0.0000}   {3:0.0000}", k, Para[k], stddevCoef[k], tCoef[k] );
		    }
        }
        public string ToStringParaT(){
            string st="";
            for(int k=0; k<=nSize; k++ )  st += string.Format(" {0:0.00000}({1:0.0})", Para[k], tCoef[k] );
            return st;
        }

        public double Estimate( double X ){
            double  P=1.0;
            for(int k=0; k<=nSize; k++ ){ Vdata[k]=P; P*=X; }

            double Y = Para.DotProduct(Vdata);
            return Y;
        }
                
        //Differential estimate
        public double	DiffEstimate( double X ){
            double  P=1.0; Vdata[0]=0.0;
            for(int k=1; k<=nSize; k++ ){ Vdata[k]=P; P*=X; }	
	        double  p = ParaD.DotProduct(Vdata);
	        return  p;
        }

        //Integral  estimate
        public double	IntegralEstimate( double X ){
            double  P=X;
            for(int k=0; k<=nSize; k++ ){ Vdata[k]=P; P*=X; }
	        double  p = ParaD.DotProduct(Vdata);
	        return  p;
        }

        //F distribution F(x,m､n)
        public double FValue( double x, int m, int n ){
	        int		i, j, ka, kb;
	        double	rm, rn;
	        double	d, p, w, y, z;                                                                    
	        ka=(m-1)%2+1; kb=(n-1)%2+1;
	        rm=m; rn=n;
            w=x*rm/rn; z=1.0/(1.0+w);
	        if(ka==1){
		        if(kb==1){ p=Sqrt(w); y=1.0/PI; d=y*z/p; p=2.0*y*Atan(p); }
                else{      p=Sqrt(w*z); d=0.5*p*z/w; }
	        }else{
		        if(kb==1){ p=Sqrt(z); d=0.5*z*p; p=1.0-p;
		        }else{     d=z*z; p=w*z; }
	        }
	        y=2.0*w/z;
	        j=kb+2;
	        while(j<=n){ 
		        d=(1.0+(double)ka/(double)(j-2))*d*z;
		        if(ka==1){ p=p+d*y/(double)(j-1); }
                else{ p=(p+w)*z; }
		        j=j+2;
	        }
	
	        y=w*z; z=2.0/z;
	        kb=n-2; i=ka+2;
	        while(i<=m){
                j=i+kb;
		        d=y*d*(double)j/(double)(i-2);
		        p=p-z*d/(double)j;
		        i=i+2;
	        }
	        return (1.0-p);
        }
    }

    #region usage
#if false
    public class regressionTest{
        private double[,]	sampleData=new double[,]{
		        //	y,			x1,			x2,			x3
		        {	2.134	,	-1.401	,	1.591	,	-0.473	}	,
		        {	-2.246	,	-1.124	,	1.499	,	-0.224	}	,
		        {	1.683	,	-0.906	,	0.865	,	0.042	}	,
		        {	-3.893	,	-0.531	,	0.277	,	-1.122	}	,
		        {	2.080	,	-0.156	,	-0.131	,	0.067	}	,
		        {	8.265	,	0.167	,	-0.483	,	3.697	}	,
		        {	-3.153	,	0.468	,	-0.676	,	-0.918	}	,
		        {	3.456	,	0.895	,	-0.925	,	1.168	}	,
		        {	2.179	,	1.083	,	-1.032	,	0.181	}	,
		        {	-0.506	,	1.628	,	-0.871	,	-0.519	}	,
	        };

        public regressionTest(){
            Regression reg=new Regression(3);
            double[] Xary=new double[4];
            for(int n=0; n<sampleData.GetLength(0); n++){
                double Y=sampleData[n,0];
                for(int k=1; k<=3; k++) Xary[k]=sampleData[n,k];
                Xary[0] = 1.0;
                reg.SetData_Vector(Xary,Y);
            }

            reg.RegressionSolver();
            reg.regressionResult();
        }
    }
#endif
    #endregion

}