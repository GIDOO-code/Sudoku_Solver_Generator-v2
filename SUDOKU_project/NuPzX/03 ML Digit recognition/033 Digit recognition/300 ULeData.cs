using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Math;

using GIDOO_Lib;

using OpenCvSharp;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GIDOOCV{
    public partial class ULeData: ULDataBase{
        static public  List<string> eVarNameLst=null;
        static private Random rdm = new Random(314);
        public DenseVector  SEest;
        public double[]     gPara0=null;

        public ULeData( Mat Udata, int ans ): base(Udata,ans){
            //===== weight =====
            EvalValDic["BlankSpace"] = _GenInitialEvalVal("BlankSpace",1);
            EvalValDic["Pixel1024"]  = _GenInitialEvalVal("Pixel1024",1024);

            //----------------------------------------------------------------
            if( eVarNameLst==null ){
                eVarNameLst = new List<string>();
                foreach( var P in EvalValDic ) eVarNameLst.Add(P.Key+" "+P.Value.Size );
            }
        }  

        public void CalcFeature( bool DispB=false ){
            Calc1024( thValue:128 );     
            CopyParameter_DicToArray();
        }

        private void Calc1024( int thValue ){
            Size sz=pUData.Size();
            int W=sz.Width , H=sz.Height;

            int kx=0, cc=0;
            unsafe{
                byte *S = pUData.DataPointer;
                for(int y=0; y<H; y++ ){
                    for(int x=0; x<W; x++ ){
                        double p = (S[y*W+x]>thValue)? 0.0: 1.0;
                        _SeteVal( "Pixel1024", kx++, p );
                        if(p>0.0) cc++;
                    }
                }
                _SeteVal( "BlankSpace", 0, ((cc<10)? 1.0: 0.0) );
            }
        }
        public void AddDenoise(double AdditionRandomNoise){
            if(gPara0==null){
                gPara0=new double[gPara.Length];
                for(int k=0; k<gPara.Length; k++) gPara0[k]=gPara[k];
            }
            int kAdDN = (int)(gPara.Length*AdditionRandomNoise);
            for(int k=0; k<kAdDN; k++){
                int n=rdm.Next(1023);
                gPara[n] = 1.0-gPara0[n];
            }
        }
    }
}