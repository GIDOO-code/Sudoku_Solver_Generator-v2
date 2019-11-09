using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using static System.Math;

using static System.Console;

using GIDOO_Lib;

using MathNet.Numerics.LinearAlgebra.Double;

namespace GIDOOCV{   
    #region MLUnit3
    public class MLUnit3{
        static  public  int     ID0;
        static  private Random  rnd;

        public  int         ID;
        public  MLUnit3     preMLU;
        public  MLUnit3     nxtMLU;

        public  int         NA;
        public  int         NB;

        public  DenseMatrix W_lst;
        public  DenseVector U_lst;
        public  DenseVector Z_lst;

        public  DenseMatrix dW_lst;
        public  DenseVector D_lst;

        public  DenseMatrix pdW_lst;
        public  double      gamma=1.0;
        public  bool        DropOutEnable{ get{ return gamma<1.0; } }
        public  bool[]      DropoutLst;
        public  Func<double,double> ActFunc;   //ActivationFunction:sigmoidF, Tanh, ReLu(max(0,x))
        public  Func<double,double> BackFunc;  //Backpropagation;

        static MLUnit3(){
            ID0=0;
            int iniVal=11;
            rnd=new Random(iniVal);  //Random number seed fixed(11)
        }
        public MLUnit3( int NA, int NB, MLUnit3 preM, Func<double,double> ActFunc, Func<double,double> BackFunc,
            double gamma ){
            ID=ID0++;
            if(preM!=null){ preM.nxtMLU=this; this.preMLU=preM; }
            this.ActFunc=ActFunc; this.BackFunc=BackFunc;
            this.NA=NA; this.NB=NB;

            U_lst = new DenseVector(NA);
            Z_lst = new DenseVector(NA);
            D_lst = new DenseVector(NA);
            
            this.gamma=gamma;
            if(NB>0){
                DropoutLst = new bool[NA];         
                W_lst      = new DenseMatrix(NB,NA);
                dW_lst     = new DenseMatrix(NB,NA);
                pdW_lst    = new DenseMatrix(NB,NA);
                Learning_Init();
            }
            WriteLine($"MLUnit3 ID:{ID} NA:{NA} NB:{NB} DropOutEnable:{DropOutEnable}");
        }
        public void Learning_Init(){
            if(W_lst==null)  return;
            for( int r=0; r<NB; r++ ){
                for( int c=0; c<NA; c++ ) W_lst[r,c] = rnd.NextDouble()*2.0-1.0;
            }
        }
    }
    #endregion

    #region MachineLearningLN3
    public partial class MachineLearningLN3{
        static public string[] LCoeffTypes = { "SGD", "Momentum", "adaGrad" };
        static public string fNamePara;
        private bool dropoutB=false;
        private int NLayer{ get{ return NSizeLst.Length; } }
        private int NLayerM1{ get{ return NLayer-1; } }
        public int[] NSizeLst;
        public int NX(int nx){ return NSizeLst[nx]; }
        public int Nout{ get{ return NSizeLst.Last(); } }
        public double errorT;
        public DenseVector SEDVresult;

        private List<ULeData>  UDataLst;
        public  List<MLUnit3>  MLUs;

        private int loop=0;
        public MachineLearningLN3( string fName, int[] NSizeLst, double gammaC ){
            loop=0;            
            SetArray_Weight( NSizeLst, gammaC );
            FileRead_MLparameter(fName,ref loop ); //Learned parameter input
        }

        private void SetArray_Weight( int[] NSizeLst, double gammaC=9999.0 ){
            this.NSizeLst=NSizeLst;
            MLUnit3 preM=null, Q;
            MLUnit3.ID0=0;

            MLUs = new List<MLUnit3>();
            for( int n=0; n<NLayer; n++ ){
                int NA=NSizeLst[n];
                int NB=(n<NLayerM1)? NSizeLst[n+1]: 0;
                double gamma=gammaC;
                if(n==NLayerM1) gamma=1.0;
                MLUs.Add(Q=new MLUnit3(NA,NB,preM,sigmoidF,sigmoidFDash,gamma) );
                preM=Q;
            }
        }

      #region Auxiliary routine
        public DenseVector eProduct( DenseVector P, DenseVector Q ){//Hadamard product
            Debug.Assert(P.Count==Q.Count);
            DenseVector R=new DenseVector(P.Count);
            for( int k=0; k<P.Count; k++ ) R[k]=P[k]*Q[k];
            return R;
        }
        public DenseMatrix vvProduct( DenseVector P, DenseVector Qt ){
            DenseMatrix R=new DenseMatrix(P.Count,Qt.Count);
            for( int r=0; r<P.Count; r++ ){
                double Pw = P[r];
                for( int c=0; c<Qt.Count; c++ ) R[r,c] = Pw*Qt[c];
            }
            return R;
        }  
        public double sigmoidF(double x){ return 1.0/(1.0+Exp(-x)); }
        public double sigmoidFDash( double x ){ double u=sigmoidF(x); return u*(1.0-u); }
        public DenseVector sigmoidFDash( DenseVector P ){
            DenseVector Q=new DenseVector(P.Count);
            double u;
            for( int k=0; k<P.Count; k++ ) Q[k]=(u=sigmoidF(P[k]))*(1.0-u);
            return Q;
        }

        public bool FileRead_MLparameter( string fName, ref int loop ){ 
            if(!File.Exists(fName)){ MLUs.ForEach(P=>P.Learning_Init( )); }
            else{
                WriteLine("\r parameter input fName:"+fName);
                try{
                    using( var fp=new StreamReader(fName) ){
                      //===== Metadata =====
                        _GetMetaData(fp);

                      //===== Weights =====
                        for(;;){
                            int kx=int.MaxValue;
                            int paraNo=(int)_GetData(fp,ref kx);
                            if(paraNo>=99)  break;

                            int NA=NSizeLst[paraNo];
                            int NB=NSizeLst[paraNo+1];       
                            var Wght = MLUs[paraNo].W_lst;
                            
                            WriteLine("paraNo:{0}  NA:{1}  NB:{2} Wght.Values.Length:{3}", paraNo, NA, NB, Wght.Values.Length);

                            kx=int.MaxValue;
                            for( int k=0; k<NA*NB; k++ ) Wght.Values[k]=_GetData(fp,ref kx);
                        }
                    }
                }
                catch( Exception e ){
                    WriteLine( e.Message+"\r"+e.StackTrace );
                }
            }

            return false;
        }
        public void FileOutput_Wght( string fName, int loop ){
            using(var fp=new StreamWriter(fName,false,Encoding.Unicode,1024) ){
                fp.WriteLine( $"loop {loop}");
                fp.WriteLine( $"fName {fName}");
                fp.Write("layerSize ");
                for( int k=0; k<NLayer; k++ )  fp.Write( $" {NSizeLst[k]}" );
                fp.WriteLine();
                fp.WriteLine("dropout "+dropoutB);

                ULeData.eVarNameLst.ForEach(p=> fp.WriteLine("/"+p) );
                fp.WriteLine("/end");

                foreach( var MU in MLUs.Where(p=>p.nxtMLU!=null) ){
                    int NA=MU.NA, NB=MU.NB;
                    fp.WriteLine(  $" {MU.ID} {MU.ID+1} {MU.DropOutEnable} {MU.gamma}" );
                    var WghtN = MU.W_lst;
                    int n=0;
                    foreach( var P in WghtN.Values ){
                        fp.Write( $" {P:0.000000}" );
                        if( ((++n)%16)==0 ) fp.WriteLine(); 
                    }
                    if((n%16)!=0 ) fp.WriteLine(); 
                }
                fp.WriteLine( $" 99 99" );
            }
        }
        static private string[] _stL_=null;
        private void _GetMetaData( StreamReader fp ){
            string st="";
            do{
                _stL_= (st=fp.ReadLine()).Split(G._sep,StringSplitOptions.RemoveEmptyEntries);
                WriteLine($"_GetMetaData:{st}");
                int nn=_stL_.Length-1;
                switch(_stL_[0]){
                    case "fName":   fNamePara=_stL_[1]; break;
                    case "loop":    loop=int.Parse(_stL_[1]);   break;
                    case "dropout": dropoutB=(_stL_[1]=="True"); break;
                    case "layerSize": 
                        NSizeLst=new int[nn];
                        for( int k=0; k<nn; k++ ) NSizeLst[k]=int.Parse(_stL_[k+1]);
                        SetArray_Weight(NSizeLst);  //## redefinition
                        break;
                    case "/end": return;
                }
            }while(true);
        }
        private double _GetData( StreamReader fp, ref int kx ){
            if( _stL_==null || kx>_stL_.Length-1){
                _stL_= fp.ReadLine().Split(G._sep,StringSplitOptions.RemoveEmptyEntries); kx=0;
            }
            return double.Parse(_stL_[kx++]);
        }
      #endregion
 
      #region MachineLearning
        public void    Learning( string fName, List<ULeData> UDataLst, string LCofTyp, bool dropoutB0, int mBatchN=1 ){
            this.dropoutB=dropoutB0;

            this.UDataLst = UDataLst;
            int  Nout=NSizeLst.Last();
            double alpha=0.01, Nalpha=1.0/UDataLst.Count, mu=0.5, HitPerMax=-1.0;    
                string stL="\r** start **  LayerSize:";
                foreach( int p in NSizeLst ) stL+=" "+p;
                stL+= "  Learning coefficient:"+LCofTyp+  "  dropout:"+dropoutB;
                stL+= "  mBatchN:"+mBatchN;
                stL += "  fName:"+fName+" \r";   
          
            DenseVector E=new DenseVector(Nout);
            for( ; HitPerMax<1.0; ){
              #region Convergence status evaluation and display of results
                errorT=0.0;
                UDataLst.ForEach(X=>{ errorT+=CalculateByDropOut(X,DoB:false,DispB:false); });
                int match = UDataLst.Count(X=>X.est==X.ans);
                double HitPer=(double)match*Nalpha;
                if(HitPerMax<0) HitPerMax=HitPer;

                stL += $"loop:{loop} match:{match}/{UDataLst.Count} ({HitPer*100.0:0.00}%)";
                stL += $" alpha:{alpha:0.0000} {__match10()} errT:{errorT:0.0000} {DateTime.Now}";

                WriteLine(stL);
                using( var fpL=new StreamWriter("Log_"+fName,true,Encoding.Unicode,1024) ){ fpL.WriteLine(stL); }
                stL="";

                if(HitPer>HitPerMax || (HitPer>0.99 && HitPer>=HitPerMax) ){
                    HitPerMax=HitPer;
                    FileOutput_Wght(fName,loop);
                }
                if(loop>5000)  break;
              #endregion Convergence status evaluation and display of results

                MLUs.Last().D_lst.Clear();
                            //  UDataLst.ForEach( X=>{
                foreach( var X in UDataLst ){   // for debug
//$$                    if(dropoutB) CreateDropout( );
                    var P=MLUs[0];
                    for( int k=0; k<NX(0); k++ ) P.Z_lst[k] = X.gPara[k];
                    RecursiveCalcu_Propagation( P, X, DoB:false, DispB:false );
                    UpdateWeight(LCofTyp,alpha,mu);
                }           // );
                loop++;
            }
            FileOutput_Wght(fName,loop);
        } 
        private void RecursiveCalcu_Propagation( MLUnit3 P, ULeData X, bool DoB, bool DispB ){
          //===== Forward calculation =====
//$$            ApplyDropout_____(P,DoB:P.DropOutEnable);
            P.Z_lst[0]=1.0;
            var Q=P.nxtMLU;
            Q.U_lst = P.W_lst*P.Z_lst;
            Q.U_lst.Apply( Q.Z_lst, x=>P.ActFunc(x)); //Activation Function

          //===== Backward calculation =====           
            if(Q.nxtMLU!=null){ // Intermediate layer
                RecursiveCalcu_Propagation( Q, X, DoB, DispB );// [Next layer]
            }   
            else{               // Final layer
              //DenseVector E = new DenseVector(Nout,0.0); E.Clear();
              //DenseVector E = DenseVector.Create(Nout,0.0); E[X.ans]=1.0;  
                DenseVector E = DenseVector.Create(Nout,p=>(p==X.ans)?1.0:0.0);
                Q.D_lst = E-Q.Z_lst;
            }

            DenseVector SU = sigmoidFDash(P.U_lst);
            DenseMatrix Wt = (DenseMatrix)P.W_lst.Transpose();
            DenseVector WD = Wt*Q.D_lst;
            P.D_lst = eProduct(SU,WD);  //Hadamard product //②
//$$                ApplyDropout_____(P,DoB:true);
            P.dW_lst = vvProduct(Q.D_lst,P.Z_lst);
            return;
        }
        public double  CalculateByDropOut( ULDataBase X, bool DoB, bool DispB=false ){
            for( int k=0; k<NX(0); k++ ) MLUs[0].Z_lst[k] = X.gPara[k];//Pointer copy is not good. Value copy.
            foreach( var P in MLUs.Where(p=>p.nxtMLU!=null) ){
//$$                ApplyDropout_____(P,DoB:DoB);
                P.Z_lst[0]=1.0;
                var Q=P.nxtMLU;
                Q.U_lst = P.W_lst*P.Z_lst;
                Q.U_lst.Apply( Q.Z_lst, x=>P.ActFunc(x)); //Activation Function
            }
            var Zout= MLUs.Last().Z_lst;
            double dt=Zout.Sum();
            X.est = (dt<0.7||dt>1.5)? 99: Zout.MaximumIndex();

            DenseVector E=new DenseVector(Nout);
            E.Clear();
            if(X.ans>=0 && X.ans<=9)  E[X.ans]=1.0;
            DenseVector Edif = E-Zout;
            errorT = Edif*Edif;
            
          #region check
            if(DispB && X.ans!=X.est && X.ID<15){
                        int Nout=NSizeLst.Last();
                        Write($"    {X.ID}" );
                        for( int k=0; k<Nout; k++ ) Write($" {Zout[k]:0.00000}" );
                        WriteLine($" ->{Zout.MaximumIndex()} ans:{X.ans} est:{X.est} vMax:{Zout.Max():0.00000}" );
                    }
          #endregion check
            return errorT;
        }
        private Random rnd=new Random();
        private void   CreateDropout( ){   
            foreach( var MU in MLUs.Where(p=>p.DropOutEnable) ){
                double per=MU.gamma;
                var B=MU.DropoutLst;
                for( int k=1; k<B.Length; k++ ) B[k] = (rnd.NextDouble()>per);
            }
        }
        private void   ApplyDropout_____( MLUnit3 MLU, bool DoB ){
            if(!dropoutB || !MLU.DropOutEnable)  return;
            double gamma=MLU.gamma;
            var Z=MLU.Z_lst;                             
            if(DoB){
                var B=MLU.DropoutLst;
                for( int k=1; k<MLU.NA; k++ ) if(B[k]) Z[k]=0.0;
            }
            else{ Z*=gamma; }
        }

        private void   UpdateWeight( string LCofTyp, double alpha, double mu ){
            DenseMatrix X;
            switch(LCofTyp){
                case "SGD":
                    foreach( var MU in MLUs.Where(p=>p.nxtMLU!=null) ){
                        MU.W_lst = MU.W_lst + MU.dW_lst*alpha;  //⑤
                    }
                    break;

                case "Momentum":
                    foreach( var MU in MLUs.Where(p=>p.nxtMLU!=null) ){
                        X = MU.dW_lst*alpha - MU.pdW_lst*mu;
                        MU.pdW_lst = X;
                        MU.W_lst = MU.W_lst + X;
                    }
                    break;

                case "adaGrad":
                    //http://www.logos.t.u-tokyo.ac.jp/~hassy/deep_learning/adagrad/
                    double v, s;
                    foreach( var MU in MLUs.Where(p=>p.nxtMLU!=null) ){
                        s=0;
                        double[] W=MU.W_lst.Values;
                        double[] dW=MU.dW_lst.Values;
                        double[] pdW=MU.pdW_lst.Values;
                        for( int k=0; k<W.Length; k++ ){
                            pdW[k] += ((v=dW[k])*v);
                            s = pdW[k];
                            s = (s<=0.0)? 1.0: Sqrt(s);
                            W[k] += alpha*v/s; 
                        }
                    }
                    break;
            }
        }
        private string __match10(){
            int ans, Nout=NSizeLst.Last();
            int[]  ccAns=new int[Nout];
            int[]  ccSuc=new int[Nout];
            UDataLst.ForEach( X =>{ ccAns[ans=X.ans]++; if(X.ans==X.est)  ccSuc[ans]++; } );
            string st="  ";

            for( int n=0; n<Nout; n++ ){
                string po= "▼ ";
                double p=ccSuc[n]/ccAns[n];
                if(p==1.0) po="   ";
                else if(p>0.9) po="▽ ";
                st += n.ToString()+":"+ccSuc[n]+"/"+ccAns[n]+po;
            }
            return st;
        }    
      #endregion MachineLearning
    }
    #endregion
}