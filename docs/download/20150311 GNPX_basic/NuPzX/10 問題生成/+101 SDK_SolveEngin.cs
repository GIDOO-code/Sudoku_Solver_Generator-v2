using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Diagnostics;

//【UCell：セル】>>>【BDL：9x9盤】  
//               >>>【UProblem：数独問題】

//               >>>【GNPX_Engin：数独解法】/GNPZ_Analyzer
//               >>>【SDK_Ctrl:問題生成・テスト・報告】

namespace GNPZ_sdk{
    public delegate bool PAnalizer();
    public class TagAnalyzer{
        public string     MethodName;
        public bool       Enabled;
        public int        difLevel; 
        public int        UCount;
        public PAnalizer  Method;

        public TagAnalyzer( string MethodName, int difLevel, PAnalizer Method ){
            this.MethodName = MethodName;
            this.difLevel   = difLevel;
            this.Method     = Method;
            this.UCount     = 0;
            this.Enabled    = true; 
        }
        public override string ToString() {
            return (MethodName.PadRight(30)+"["+difLevel+"]"+(Enabled? "T": "F")+" "+UCount);               ;
        }
    }

    //与えられた数独問題を解く
    public class GNPX_Engin{
        //===== 解法の条件・パラメータ(共通) =====GNumPzl.MltSolOn
        static public int       cCode;
        static public int       retCode;
        static public bool      SolInfoDsp;

        static public string    GNPZ_AnalyzerMessage;
        static public List<string> SDK_AnlzST = new List<string>();
        static public TimeSpan  SdkExecTime;

        static public List<TagAnalyzer>  SDK_Methods0 = new List<TagAnalyzer>();
        static public List<TagAnalyzer>  SDK_MethodsRun = new List<TagAnalyzer>();
        static public List<UItemCheck> pGMthdLst;

        //===================================
        public UProblem         GP=null;    //解読データ
        public GNPZ_Analyzer    SDA;        //解法
            
        public int              cCodePre = 0;
        public int              SolvingLoopCC;     

        static public bool      ChkPrint=false;

        public GNPX_Engin( UProblem GPx ){
            GP  = GPx;
            SDA = new GNPZ_Analyzer( this );
            SDA.SetProblem(this);
            SolvingLoopCC = 0;

            SDK_Methods0   = new List<TagAnalyzer>();

            //===== singles =====
            SDK_Methods0.Add( new TagAnalyzer( "LastDigit",        1, SDA.GNP00_LastDigit ) );

            SDK_Methods0.Add( new TagAnalyzer( "NakedSingle",      1, SDA.GNP00_NakedSingle ) );
            SDK_Methods0.Add( new TagAnalyzer( "HiddenSingle",     1, SDA.GNP00_HiddenSingle ) );
         
            //===== Intersection =====
            SDK_Methods0.Add( new TagAnalyzer( "LockedCandidate",  2, SDA.GNP00_LockedCandidate ) );

            //===== LockedSet =====
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(2cells)",       2, SDA.GNP00_LockedSet2 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(2cells)Hidden", 2, SDA.GNP00_LockedSet2Hidden ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(3cells)",       3, SDA.GNP00_LockedSet3 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(3cells)Hidden", 3, SDA.GNP00_LockedSet3Hidden ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(4cells)",       4, SDA.GNP00_LockedSet4 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(4cells)Hidden", 4, SDA.GNP00_LockedSet4Hidden ) );

            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(5cells)",       -5, SDA.GNP00_LockedSet5 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(5cells)Hidden", -5, SDA.GNP00_LockedSet5Hidden ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(6cells)",       -5, SDA.GNP00_LockedSet6 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(6cells)Hidden", -5, SDA.GNP00_LockedSet6Hidden ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(7cells)",       -5, SDA.GNP00_LockedSet7 ) );
            SDK_Methods0.Add( new TagAnalyzer( "LockedSet(7cells)Hidden", -5, SDA.GNP00_LockedSet7Hidden ) );

            //===== X-Wing(Fish) =====
            SDK_Methods0.Add( new TagAnalyzer( "X-Wing",                 3, SDA.GNP00_XWing ) );
            SDK_Methods0.Add( new TagAnalyzer( "SwordFish",              4, SDA.GNP00_SwordFish ) );
            SDK_Methods0.Add( new TagAnalyzer( "JellyFish",              5, SDA.GNP00_JellyFish ) );
            SDK_Methods0.Add( new TagAnalyzer( "Squirmbag",             -5, SDA.GNP00_Squirmbag ) );
            SDK_Methods0.Add( new TagAnalyzer( "Whale",                 -5, SDA.GNP00_Whale ) );
            SDK_Methods0.Add( new TagAnalyzer( "Leviathan",             -5, SDA.GNP00_Leviathan ) );  

            //===== Finned X-Wing(Finned Fish) =====
            SDK_Methods0.Add( new TagAnalyzer( "Finned X-Wing",    5, SDA.GNP00_FinnedXWing ) );
            SDK_Methods0.Add( new TagAnalyzer( "Finned SwordFish", 6, SDA.GNP00_FinnedSwordFish ) );
            SDK_Methods0.Add( new TagAnalyzer( "Finned JellyFish", 6, SDA.GNP00_FinnedJellyFish ) );
            SDK_Methods0.Add( new TagAnalyzer( "Finned Squirmbag", 7, SDA.GNP00_FinnedSquirmbag ) );
            SDK_Methods0.Add( new TagAnalyzer( "Finned Whale",     7, SDA.GNP00_FinnedWhale ) );
            SDK_Methods0.Add( new TagAnalyzer( "Finned Leviathan", 7, SDA.GNP00_FinnedLeviathan ) );

            //===== SueDeCoq =====
            SDK_Methods0.Add( new TagAnalyzer( "SueDeCoq",         5, SDA.GNP00_SueDeCoq ) );

            //===== Skyscraper =====
            SDK_Methods0.Add( new TagAnalyzer( "Skyscraper",       4, SDA.GNP00_Skyscraper ) );    //+

            //===== EmptyRectangle =====    //(Skyscraper を先に適用する)
            SDK_Methods0.Add( new TagAnalyzer( "Empty Rectangle",  4, SDA.GNP00_EmptyRectangle ) ); 

            //===== XY-Wing =====
            SDK_Methods0.Add( new TagAnalyzer( "XY-Wing",          5, SDA.GNP00_XYwing ) );
            SDK_Methods0.Add( new TagAnalyzer( "W-Wing",           6, SDA.GNP00_Wwing ) );

            SDK_Methods0.Add( new TagAnalyzer( "XYZ-Wing",         5, SDA.GNP00_XYZwing ) );
            SDK_Methods0.Add( new TagAnalyzer( "WXYZ-Wing",        5, SDA.GNP00_WXYZwing ) );
            SDK_Methods0.Add( new TagAnalyzer( "VWXYZ-Wing",       6, SDA.GNP00_VWXYZwing ) );
            SDK_Methods0.Add( new TagAnalyzer( "UVWXYZ-Wing",      6, SDA.GNP00_UVWXYZwing ) );

            SDK_Methods0.Add( new TagAnalyzer( "WXYZwing(ALS)",    7, SDA.GNP00_XYZwingALS ) );
  
            //===== Coloring =====
            SDK_Methods0.Add( new TagAnalyzer( "Coloring Trap",    5, SDA.GNP00_Color_Trap ) );
            SDK_Methods0.Add( new TagAnalyzer( "Coloring Wrap",    5, SDA.GNP00_Color_Wrap ) );
            SDK_Methods0.Add( new TagAnalyzer( "MultiColoring Type-1", 6, SDA.GNP00_MultiColor_Type1 ) );
            SDK_Methods0.Add( new TagAnalyzer( "MultiColoring Type-2", 6, SDA.GNP00_MultiColor_Type2 ) );
      
            //===== Chains and Loops =====
            SDK_Methods0.Add( new TagAnalyzer( "Remote Pair",      5, SDA.GNP00_RemotePair ) );
            SDK_Methods0.Add( new TagAnalyzer( "X-Chain",          6, SDA.GNP00_XChain ) );
            SDK_Methods0.Add( new TagAnalyzer( "XY-Chain",         6, SDA.GNP00_XYChain ) );

            //===== ALS-XZ XY-Wing =====
            //次バージョン公開予定
   
            //===== (Finned)Franken/Mutant Fish, Advanced Franken/Mutant Fish =====
            SDK_Methods0.Add( new TagAnalyzer( "Franken/Mutant Fish",          7, SDA.GNP00_FrankenMutantFish ) );  
            SDK_Methods0.Add( new TagAnalyzer( "Finned Franken/Mutant Fish",   7, SDA.GNP00_FinnedFrankenMutantFish ) );           

            //===== Nice Loops =====
            //次バージョン公開予定    

       //========== Advanced Technique ==========
            //===== Cannibalistic Fish =====
            //公開時期　未定

            //===== Grouped Nice Loop/Chain =====
            //公開時期　未定

            //===== Kraken Fish =====
            //公開時期　未定

        }

        public void SetGP( UProblem GPx ){
            GP=GPx;
            SDA.SetProblem(this);
            SDA.SetBoardFreeB();    //問題の初期化
        }

        public int Set_GNPZMethodList( bool AllMthd=false ){
            SDK_MethodsRun.Clear();
            //int dx=0;
            foreach( var mt in pGMthdLst ){
                if( !AllMthd && !mt.IsChecked )  continue;
                var an = SDK_Methods0.Find( m => m.MethodName==mt.Name );
                SDK_MethodsRun.Add(an);
                //an.difLevel = an.difLevel*100+(++dx); //難易度同一時の順序保存を確認
            }
            //SDK_MethodsRun.Sort((a,b)=>(Math.Abs(a.difLevel)-Math.Abs(b.difLevel)));//難易度順にソート

            return SDK_MethodsRun.Count;
        }
        public void AnalyzerCounterReset(){
            SDK_MethodsRun.ForEach( AX => AX.UCount=0 );
        }
        private Random rnd = new Random();
        private void DspNumRandmize( UProblem P ){
            List<int> ranNum = new List<int>();

            for( int r=0; r<9; r++ )  ranNum.Add( rnd.Next(0,9)*10+r );
            ranNum.Sort( (x,y) => (x-y) );
            for( int r=0; r<9; r++) ranNum[r] %= 10;

            int n;
            P.BDL.ForEach(q =>{
                if( (n=q.No)>0 ) q.No = ranNum[n-1]+1;
            } );
        }
        public void sudokAnalyzerAuto( CancellationToken ct ){
//          Set_GNPZMethodList();
            SDA.SetBoardFreeB();
            Stopwatch AnalyzerLap = new Stopwatch();
            AnalyzerLap.Start();
            while(true){
                if( ct.IsCancellationRequested ){ return; }
                int ret2=0;     
                bool ret = AnalyzerControl(ct,ref ret2,false );
                if( GP.Insoluble==true ) retCode=-999;   //候補数字なしのセルを発見
                if( ret2==-999888777 )   retCode=ret2;
                if( ret==false )  break;
                if( !SDA.GNPZ_NumberFix( ) )  retCode=-998;
                SdkExecTime = AnalyzerLap.Elapsed;
                if( retCode<0 )  return;
            }
            AnalyzerLap.Stop();        
            int  nP=0, nZ=0, nM=0;
            SDA.cellPZMCounter(ref nP,ref nZ,ref nM);
            retCode=nZ;
        }

        public bool AnalyzerControl( CancellationToken ct, ref int ret2, bool SolInfoDsp ){
            List<UCell> XYchainList = new List<UCell>();
            Stopwatch AnalyzerLap = new Stopwatch();

            bool ret=false;
            try{
                GP.GNPZ_ResultLong = "";
                int   lvlLow = SDK_Ctrl.lvlLow;
                int   lvlHgh = SDK_Ctrl.lvlHgh;
                SDA.SetProblem( this );

   #region 解法適用
                int  mCC=0;            
                do{    //ダミーのブロック
                    ret = SDA.gNoPzAnalyzerRouleCheck();
                    if( GP.SolCode==99999 ) break;
                    if( ret==false ){
                        if( SolInfoDsp ) GP.GNPZ_ResultLong = "00 解なし";
                        ret2 = -999888777;
                        return false;
                    }
                    ret=false;
                    //-------------------------------------------
                LblRestart:    
                    AnalyzerLap.Start();

                    DateTime MltAnsTimer=DateTime.Now;
                    UProblem GPpre=null;
                    if( SDK_Ctrl.MltAnsSearch ) GPpre=GP.Copy();
                    try{
                        //SDA.AnalyzerLog = "";
                        GP.SolCode=-1;
                        SDA.Analyzer_Initialize();
                        //SDK_MethodsRun.ForEach( P=>Console.WriteLine(P) );

                        bool L1SolFond=false;
                        foreach( var P in SDK_MethodsRun ){
                            //if( !P.Enabled )  continue;
                            int lvl=P.difLevel; 
                            int lvlAbs = Math.Abs(lvl);

                            if( SDK_Ctrl.MltAnsSearch ){
                                if( L1SolFond && lvlAbs>=2 )  break;
                                if( lvlAbs>SDK_Ctrl.MltAnsOption[0] ) break;
                                
                                if( SDK_Ctrl.MltAnsOption[4]==1 ){
                                    GNPZ_AnalyzerMessage = "探索数制限で打切り";
                                    break;
                                }
                                TimeSpan tmspn=DateTime.Now-MltAnsTimer;
                                if( tmspn.TotalSeconds>=SDK_Ctrl.MltAnsOption[2] ||
                                    SDK_Ctrl.MltAnsOption[4]==2 ){
                                    GNPZ_AnalyzerMessage = "探索時間制限で打切り";
                                    break;
                                }

                                if( SDK_Ctrl.GPMX==null || SDK_Ctrl.GPMX.MltUProbLst==null ) goto LblCont;
                                if( SDK_Ctrl.GPMX.MltUProbLst.Count>=SDK_Ctrl.MltAnsOption[1] ) break;
                            }
                            else{
                                if( lvl<0 ) continue; //負難易度手法は複数解解析のみ
                            }
                          LblCont:
                            
                            if( !SDK_Ctrl.MltAnsSearch && lvl<0 ) continue; //負難易度手法は複数解解析のみ
                            if( SDK_Ctrl.MltAnsSearch && L1SolFond && Math.Abs(lvl)>=2 )  continue;
                            if( lvl>lvlHgh )  continue;
                            if( ChkPrint ) Console.WriteLine("---> method{0} :{1}", (mCC++), P.MethodName);
                            GNPZ_AnalyzerMessage = P.MethodName;
                            if( ct!=null && ct.IsCancellationRequested ){ /*ct.ThrowIfCancellationRequested();*/ return false; }
                            GP.difLevel=P.difLevel;

                            if( (ret=P.Method()) ){
                                if( SDK_Ctrl.GPMX!=null &&
                                    SDK_Ctrl.GPMX.MltUProbLst.Any(q=>q.SolCode==1) ) L1SolFond=true;
                                P.UCount++;
                                if( ChkPrint ) Console.WriteLine( "========================> solved {0}", P.MethodName );
                                if( !SDK_Ctrl.MltAnsSearch )  goto succeedBreak;
                            }
                        }
                        if( SDK_Ctrl.MltAnsSearch && SDK_Ctrl.GPMX.MltUProbLst!=null ){
                            GP=SDK_Ctrl.GPMX.MltUProbLst.First();
                            ret=true;
                            goto succeedBreak;
                        }

                        if( ChkPrint ) Console.WriteLine( "========================> 解けない");
                        if( SolInfoDsp ) GP.GNPZ_ResultLong = "解けない";
                        ret2 = -999888777;
                        return false;
                    }
                    catch( Exception e ){
                        Console.WriteLine( e.Message );
                        Console.WriteLine( e.StackTrace );
                        goto LblRestart;
                    }
                    finally{ AnalyzerLap.Stop(); }
                }while(false);
            }
            catch( ThreadAbortException ex ){
                Console.WriteLine( ex.Message );
                Console.WriteLine( ex.StackTrace );
            }
          succeedBreak:  //Fond
            SdkExecTime = AnalyzerLap.Elapsed;
#endregion 解法適用
            return ret;  
        }
        public int solutionConditionCode( ){
            int rc=0, n, cCode=0;
            GP.BDL.ForEach( q =>{
                if( (n=q.No)!=0 )  cCode ^= ((rc++)*7 + n*97);
                else               cCode ^= q.FreeB*317 + (rc*13);
            } );
            return cCode;
        }
        public int DifficultyLevelChecker( out string prbMessage, out int lvlBase ){
            int    lvlMax=0;
            prbMessage = "";
            foreach( var Mthd in SDK_MethodsRun ){
                if( Mthd.UCount<=0 ) continue;
                int nlv = Math.Abs(Mthd.difLevel);
                if( lvlMax<=nlv ){  //（等号は、"同じレベルでは後の名前に置換える"ため）
                    lvlMax = nlv;
                    prbMessage = Mthd.MethodName;//
                } 
            }
            lvlBase = lvlMax;
            return lvlMax;
        }
               
        public string DGViewMethodCounterToString(){ //手法の集計
            string solMessage="";
            foreach( var q in SDK_MethodsRun.Where(p=>p.UCount>0) ){
                solMessage += " "+q.MethodName+"["+q.UCount+"]";
            }
            return solMessage;
        }
    }
}