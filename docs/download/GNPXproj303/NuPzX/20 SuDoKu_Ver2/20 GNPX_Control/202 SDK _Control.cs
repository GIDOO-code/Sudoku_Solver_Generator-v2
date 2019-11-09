using System;
using System.Collections.Generic;
using static System.Console;
using static System.Math;
using System.Threading;

namespace GNPZ_sdk{
    public partial class SDK_Ctrl{    
        static public event   SDKEventHandler Send_Progress; 
        static public Random  GRandom= new Random();
        static public int     TLoopCC = 0;
        static public int     lvlLow;
        static public int     lvlHgh;
        static public bool    FilePut_GenPrb;
        static public GNPZ_Engin   pGNPX_Eng;

        private GNPXApp000  pGNP00;
        private NuPz_Win    pGNP00win{ get{ return pGNP00.pGNP00win; } } 
      
      //===== Multiple Solution Analysis ====================
        static public bool   MltAnsSearch;
        static public int    MltProblem;
        static public bool   GenLS_turbo;
        static public Dictionary<string,object> MltAnsOption;
        static public UProbMan UGPMan=null;
      //---------------------------------------------------

        public int          retNZ; 
        
        public int          CellNumMax;
        public int          LoopCC=0;
        public int          PatternCC=0;

        public int          ProgressPer;
        public bool         CanceledFlag;
        public bool         CbxDspNumRandmize;  //Randomization of numbers
        public int          GenLStyp;
        public bool         CbxNextLSpattern;   //Change Latin-Square Pattern on Success
    
        public patternGenerator PatGen;         //Puzzle Pattern

        public int          randumSeedVal=0;
        public bool         threadRet;

        private bool        _DEBUGmode_= false; //false; //true;// 

        static SDK_Ctrl(){
            MltAnsOption=new Dictionary<string,object>();
            MltAnsOption["MaxLevel"]     = 10;
            MltAnsOption["OneMethod"]    = 5;
            MltAnsOption["AllMethod"]    = 50;
            MltAnsOption["MaxTime"]      = 15;
            MltAnsOption["StrtTime"]     = DateTime.Now;
            MltAnsOption["abortResult"]  = "";
        }

        public SDK_Ctrl( GNPXApp000 pGNP00, int FirstCellNum ){
            this.pGNP00 = pGNP00;
            Send_Progress += new SDKEventHandler(pGNP00win.BWGenPrb_ProgressChanged);     
            
            CellNumMax = FirstCellNum; 

            PatGen = new patternGenerator( this );
            LSP    = new LatinSqureGen( );
        }

        static public void MovePre(){
            if(UGPMan==null)  return;
            UGPMan = UGPMan.GPMpre;
            if(UGPMan==null)  return;
            pGNPX_Eng.pGP = UGPMan.pGPsel;
        }
         
        static  public void MoveNxt(){
            if(UGPMan==null)  return;
            UGPMan = UGPMan.GPMnxt;
            if(UGPMan==null)  return;
            pGNPX_Eng.pGP = UGPMan.pGPsel;
        }

        private void _ApplyPattern( int[] X ){
            for( int rc=0; rc<81; rc++ ){
                if(PatGen.GPat[rc/9,rc%9]==0) X[rc]=0;
            }
            //if( _DEBUGmode_ )  __DBUGprint2(X, "_ApplyPattern");
        }
        private void _ApplyPattern( int[,] X2 ){
            for( int rc=0; rc<81; rc++ ) if(PatGen.GPat[rc/9,rc%9]==0) X2[rc/9,rc%9]=0;
            //if( _DEBUGmode_ )  __DBUGprint2(X, "_ApplyPattern");
        }
        
        public void SetRandumSeed( int rs ){
#if DEBUG
            randumSeedVal = rs;
#else
            if(rs==0){
                int nn=Environment.TickCount&Int32.MaxValue;
                randumSeedVal=nn;
            }
#endif
            GRandom=null; 
            GRandom=new Random(randumSeedVal);
        }

    #region Generate Puzzle Candidate
        internal int[,] ASDKsol = new int[9,9];
        private int[] prKeeper = new int[9];
        private Random rnd = new Random();

        public List<UCell> GeneratePuzzleCandidate( ){ //Generate puzzle candidate
            int[]  P=GenSolPatternsListA(CbxDspNumRandmize,GenLStyp); //*****

            List<UCell> BDLa = new List<UCell>();
            for( int rc=0; rc<81; rc++ )  BDLa.Add(new UCell(rc,P[rc]));
            if( _DEBUGmode_ ) __DBUGprint(BDLa);
            return BDLa;
        }    
        private void __DBUGprint( List<UCell> BDL ){
            string po;
            WriteLine();
            for( int r=0; r<9; r++ ){
                po = r.ToString("            ##0:");
                for( int c=0; c<9; c++ ){
                    int No = BDL[r*9+c].No;
                    if( No==0 ) po += " .";
                    else po += No.ToString(" #");
                }
                WriteLine(po);
            }
        }
    #endregion Generate puzzle candidate

    #region Create Puzzle
        //====================================================================================
        public void SDK_ProblemMakerReal( CancellationToken ct ){ //Creating problems[Automatic]
            try{
                int mlt = MltProblem;
                pGNPX_Eng.Set_MethodLst_Run();

                do{
                    if( ct.IsCancellationRequested ){ ct.ThrowIfCancellationRequested(); return; }

                    LoopCC++; TLoopCC++;
                    List<UCell>   BDL = GeneratePuzzleCandidate( );  //Problem candidate generation
                    UProblem P = new UProblem(BDL);
                    pGNPX_Eng.SetGP(P);

                    pGNPX_Eng.AnalyzerCounterReset();
                    pGNPX_Eng.sudokAnalyzerAuto(ct);
            
                    if( GNPZ_Engin.retCode==0 ){

                        __ret000=true;  //##########

                        string prbMessage;
                        int DifLevel = pGNPX_Eng.GetDifficultyLevel(out prbMessage);
                        if( DifLevel<lvlLow || lvlHgh<DifLevel ) continue; //Difficulty check

                        __ret001=true;  //##########
           
                        P.DifLevel = DifLevel;
                        P.Name = prbMessage;
                        P.TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        P.solMessage = pGNPX_Eng.DGViewMethodCounterToString();
                        pGNP00.SDK_ProblemListSet(P);
                    
                        SDKEventArgs se = new SDKEventArgs(ProgressPer:(--mlt));
                        Send_Progress(this,se);  //(can send information in the same way as LoopCC.)
                        if( CbxNextLSpattern ) rxCTRL=0; //Change LS pattern at next problem generation
                    }
                }while(mlt>0);
            }
            catch( Exception ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); }
        }
    #endregion Create Puzzle

    #region Analize
        public void AnalyzerReal( CancellationToken ct ){      //Analysis[step]
            int ret2=0;
            retNZ=-1; LoopCC++; TLoopCC++;
            pGNPX_Eng.Set_MethodLst_Run(false); 
            pGNPX_Eng.AnalyzerControl( ct, ref ret2, true );
            SDKEventArgs se = new SDKEventArgs(ProgressPer:(retNZ));
            Send_Progress(this,se);  
        }
        public void AnalyzerRealAuto( CancellationToken ct ){   //Analysis[solveUp]
            LoopCC++; TLoopCC++;
            bool chbConfirmMultipleCells = GNPXApp000.chbConfirmMultipleCells;
            pGNPX_Eng.Set_MethodLst_Run(false);
            pGNPX_Eng.sudokAnalyzerAuto(ct);
            SDKEventArgs se = new SDKEventArgs(ProgressPer:(GNPZ_Engin.retCode));
            Send_Progress(this,se);
        }
    #endregion Analize
 
        private void __DBUGprint2( int[,] pSol99, string st="" ){
            string po;
            WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int wk=pSol99[r,c];
                    if(wk==0) po += " .";
                    else po += wk.ToString(" #");
                }
                WriteLine(po);
            }
        }


        private void __DBUGprint2( int[] X, bool sqF, string st="" ){
            string po, p2="";
            if(sqF) WriteLine();
            for( int r=0; r<9; r++ ){
                po = "";
                for( int c=0; c<9; c++ ){
                    int wk=Abs(X[r*9+c]);
                    if(wk==0) po += " .";
                    else po += wk.ToString(" #");
                }
                if(sqF) WriteLine(st+r.ToString("##0:")+po);
                p2 += " "+po.Replace(" ","");
            }
            WriteLine(st+" "+p2);
        }
    }
}
    