using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Math;
using static System.Console;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class SDK_Ctrl{
        static public bool? paraSearching;

        public void SDK_ProblemMakerRealExPara2( CancellationToken ct ){    
            int cpu=Environment.ProcessorCount;
            int ccEx=0;
            if(!CheckPattern()) return;
            paraSearching=true;
            do{
                if( ct.IsCancellationRequested ){ ct.ThrowIfCancellationRequested(); return; }

                List<UProblem> SDKp=null;
                var SDKpLst = new List< List<UProblem> >();
                var tasks = new List<Task>();
                for( int k=0; k<cpu; k++ ){
                    var tsk = Task.Run(() => (SDKp=SDK_ProblemMakerRealEx()) );
                    tasks.Add(tsk); // を、Listにまとめる
                    SDKpLst.Add(SDKp);
                }
                Task.WhenAll(tasks);

                SDKpLst.ForEach(Q=>{
                    if(Q!=null){
                        ccEx += Q.Count;
                        Q.ForEach(P=>PSave(P));
                        pGNP00.CurrentPrbNo=999999999;
                    }
                } );
            }while(ccEx<MltProblem);
            paraSearching=false;
        }
        public void SDK_ProblemMakerRealExPara( CancellationToken ct ){       
            int ccEx=0;
            if(!CheckPattern()) return;
            paraSearching=true;
            int mlt = MltProblem;
            do{
                if( ct.IsCancellationRequested ){ ct.ThrowIfCancellationRequested(); return; }
                var SDKp = SDK_ProblemMakerRealEx();
                if(SDKp!=null){
                    ccEx += SDKp.Count;
                    SDKp.ForEach(P=>PSave(P));
                    pGNP00.CurrentPrbNo=999999999;

                    mlt -= SDKp.Count;
                    SDKEventArgs se = new SDKEventArgs(ProgressPer:(mlt));
                    Send_Progress(this,se);  //(can send information in the same way as LoopCC.)
                }
            }while(ccEx<MltProblem);
            paraSearching=false;
        }

        private void PSave( UProblem P ){
            P.ID=pGNP00.SDKProbLst.Count; 
            pGNP00.SDKProbLst.Add(P);
        }

        public List<UProblem> SDK_ProblemMakerRealEx( ){
            try{
                GNPZ_EnginEx pGNPX_EngEx = new GNPZ_EnginEx(pGNP00);
                pGNPX_EngEx.Set_MethodLst_Run(); //設定は一度でよい。位置を移す

                var LSEx = GenSolPatternsListEx(RandF:CbxDspNumRandmize);

                List<UProblem> SDKPs=null;
                                Stopwatch sw=new Stopwatch();
                sw.Start();
                int ppCnt=0;
                UProblem P0;
                foreach( var BDL in LSEx ){ //Problem candidate generation
                    UProblemEx P = new UProblemEx(BDL);
                    pGNPX_EngEx.SetGP(P);
                    pGNPX_EngEx.sudokuSolver_AutoEx();           
                    if( pGNPX_EngEx.retCode==0 ){
                                __ret000=true;  //##########
                        int DifLevel=P.DifLevel;
                        if( DifLevel<lvlLow || lvlHgh<DifLevel ) continue; //Difficulty check
                        if(SDKPs==null) SDKPs=new List<UProblem>();
                        SDKPs.Add( P0=new UProblem(P) );
                        ppCnt++;
                                __ret001=true;  //##########  
                        SDK_Temporarily_output("TemporarilySave.txt",P0); //###
                    }
                }
               
                int n=LSEx.Count;
                LoopCC+=n; TLoopCC+=n;
                                sw.Stop();
                                var  lap=sw.Elapsed.Milliseconds/1000.0;
                                Write(" ppCnt:"+ppCnt+"  lap:"+lap.ToString("#0.000")+"sec");
                return SDKPs;
            }
            catch( Exception ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); }
            return null;
        }

        public void SDK_Temporarily_output( string fName, UProblem P ){
            using( var fpW=new StreamWriter(fName,append:false,encoding:Encoding.UTF8) ){
                string st= "";
                P.BDL.ForEach( q =>{ st += Max(q.No,0).ToString(); } );
                st=st.Replace("0",".");
                st += " " + (P.ID+1) + " " + P.DifLevel.ToString()+ " \"" + P.Name+"\"";
                st += " ";
                //st += " \""+SetSolution(pGP,SolSet2:true,SolAll:true)+" \"";//解を出力
                st += " \"" + DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss")+"\"";
                fpW.WriteLine(st);
            }
        }

        public List< List<UCellEx> > GenSolPatternsListEx( bool RandF ){  //for Parallel
            var LSlstEx=new List<_LSpattern>();              
            do{
                int __cc=0;
                foreach( var P in GenerateLatinSqure0A(RandF:RandF) ){
                    __cc++;
                    var Q=LSlstEx.Find(x=>(x.has==P.has));
                    if(Q==null){ 
                        LSlstEx.Add(P); Q=P;
#if DEBUG
                         Q.Sol99lst=new List<int[,]>();
#endif
                    }
#if DEBUG
                    int[,] S99=new int[9,9];
                    for(int k=0; k<81; k++ ) S99[k/9,k%9] = P.Sol99[k/9,k%9];
                    Q.Sol99lst.Add(S99);
#endif
                    Q.cnt++;
                }
#if DEBUG                     
                double per = LSlstEx.Count*100.0/__cc;
                Write( "\n========== LSpattern = "+ LSlstEx.Count+"/"+__cc + "("+per.ToString("0.00")+"%)" );
#endif
                if(GenLS_turbo) LSlstEx = LSlstEx.FindAll(p=>(p.cnt==1));
#if DEBUG
                if(GenLS_turbo){
                        per = LSlstEx.Count*100.0/__cc;
                        Write( "  =>(turbo) "+ LSlstEx.Count+"/"+__cc + "("+per.ToString("0.00")+"%)" );
                }
            //  WriteLine();
#endif

            }while(LSlstEx.Count<=0);

            var L_Ex=new List<List<UCellEx>>();
            LSlstEx.ForEach( P => {
                var Q = new List<UCellEx>();
                for( int rc=0; rc<81; rc++ )  Q.Add(new UCellEx(rc,P.SolX[rc]));
                L_Ex.Add(Q);
            } );

            return L_Ex;
        }
    }

    public class UCellEx{ //Basic Cell Class
        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;

        public int      ErrorState; //0:-  1:Fixed 　8:Violation  9:No solution
        public int      No;         //>0:Problem  =0:Open  <0:Solution
        public int      FreeB;
        public int      FreeBC{ get{ return FreeB.BitCount(); } }

        public int      FixedNo;  
        public int      CancelB;
        
        public bool     Selected;
        public int      nx;

        public UCellEx( ){}
        public UCellEx( int rc, int No=0, int FreeB=0 ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = FreeB;
        }

        public UCellEx Copy( ){
            UCellEx UCcpy=(UCellEx)this.MemberwiseClone();
            return UCcpy;
        }
        public override string ToString(){
            string po = " UCell rc:"+rc+"["+((r+1)*10+(c+1)) +"]  no:"+No;
            po +=" FreeB:" + FreeB.ToBitString(9);
            po +=" CancelB:" + CancelB.ToBitString(9);
            return po;
        }
    }
}