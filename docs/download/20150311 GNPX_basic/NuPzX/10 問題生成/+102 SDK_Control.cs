using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UPrbMltMan{
        static public GNPX_Engin pGNPX_Eng;
        public List<UProblem> MltUProbLst=null;
        public int        selX=-1;     
        public UPrbMltMan GPMpre=null;
        public UPrbMltMan GPMnxt=null;

        public UPrbMltMan( ){
            if( SDK_Ctrl.GPMX!=null )  SDK_Ctrl.GPMX.GPMnxt=this;
            this.GPMpre=SDK_Ctrl.GPMX;
        }

        public void　Create( ){ //新規作成
            UPrbMltMan GPMXnew = new UPrbMltMan();
            SDK_Ctrl.GPMX = GPMXnew;
            pGNPX_Eng.GP  = pGNPX_Eng.GP.Copy();
        //    return GPMXnew;
        }

        public void MovePre(){
            if( SDK_Ctrl.GPMX==null )  return;
            SDK_Ctrl.GPMX = SDK_Ctrl.GPMX.GPMpre;
            if( SDK_Ctrl.GPMX==null )  return;
            int selX     = SDK_Ctrl.GPMX.selX;
            if( selX<0 || selX>=SDK_Ctrl.GPMX.MltUProbLst.Count )  return;
            pGNPX_Eng.GP = SDK_Ctrl.GPMX.MltUProbLst[selX];
        }
         
        public void MoveNxt(){
            if( SDK_Ctrl.GPMX==null )  return;
            SDK_Ctrl.GPMX = SDK_Ctrl.GPMX.GPMnxt;
            if( SDK_Ctrl.GPMX==null )  return;
            int selX     = SDK_Ctrl.GPMX.selX;
            if( selX<0 || selX>=SDK_Ctrl.GPMX.MltUProbLst.Count )  return;
            pGNPX_Eng.GP = SDK_Ctrl.GPMX.MltUProbLst[selX];
        }
    }

    public partial class SDK_Ctrl{    
        static public event   SDKEventHandler Send_Progress; 
        static public Random  GRandom= new Random();
        static public int     TLoopCC = 0;
        static public int     lvlLow;
        static public int     lvlHgh;

      //===== 複数解解析拡張 20140730- ====================
        static public bool   MltAnsSearch;
        static public int    MltProblem;
        static public int[]  MltAnsOption=new int[10];
        static public UPrbMltMan GPMX=null;
      //---------------------------------------------------

        public NuPz_Win     pGNP00win;
        private GNumPzl     pGNP00;
        public GNPX_Engin   pGNPX_Eng{ get{ return pGNP00.GNPX_Eng; } }

        public int          retNZ; 
        
        public int          CellNumMax;
        public int          LoopCC = 0;
        public int          PatternCC = 0;

        public int          ProgressPer;
        public bool         CanceledFlag;
        public bool         CbxDspNumRandmize;  //数字の乱数化 
        public int          GenLStyp;
        public bool         CbxNextLSpattern;   //問題成功時にLSパターンを変更
    
        public patternGenerator PatGen; //露出パターン

        public int          randumSeedVal=0;
        public bool         threadRet;

        private bool        _DEBUGmode_= false; //false; //true;// 

        public SDK_Ctrl( GNumPzl pGNP00, int FirstCellNum ){
            this.pGNP00 = pGNP00;
            this.pGNP00win = pGNP00.pGNP00win;
            Send_Progress += new SDKEventHandler(pGNP00win.BWGenPrb_ProgressChanged);     
            
            CellNumMax = FirstCellNum; 

            PatGen  = new patternGenerator( this );
            LSP     = new LatinSqureGen( ); 　//▼仮の設定
        }

        public void SetRandumSeed( int rs ){
#if DEBUG
            randumSeedVal = rs;
#else
            if( rs==0 ){
                int nn = Environment.TickCount&Int32.MaxValue;
                randumSeedVal = nn;
            }
#endif
            GRandom = null; 
            GRandom = new Random(randumSeedVal);
        }

    #region 問題候補生成
        //パターン生成コントロール　
        internal int[,] ASDKsol = new int[9,9];
        private int[] prKeeper = new int[9];
        private Random rnd = new Random();

        public List<UCell> GenerateSolCandidate( ){ //問題候補生成
            int[] P=GenSolPatternsList(CbxDspNumRandmize,GenLStyp);
            List<UCell> BDLa = new List<UCell>();
            for( int rc=0; rc<81; rc++ )  BDLa.Add(new UCell(rc,P[rc]));
            if( _DEBUGmode_ ) __DBUGprint(BDLa);
            return BDLa;
        }    
        private void __DBUGprint( List<UCell> BDL ){
            string po;
            Console.WriteLine();
            for( int r=0; r<9; r++ ){
                po = r.ToString("            ##0:");
                for( int c=0; c<9; c++ ){
                    int No = BDL[r*9+c].No;
                    if( No==0 ) po += " .";
                    else po += No.ToString(" #");
                }
                Console.WriteLine(po);
            }
        }
    #endregion 問題候補生成

    #region 問題作成
        //====================================================================================
        public void SDK_ProblemMakerReal( CancellationToken ct ){ //問題作成【自動】L1067
            try{
                int mlt = MltProblem;
                pGNPX_Eng.Set_GNPZMethodList();

                do{
                    if( ct.IsCancellationRequested ){ ct.ThrowIfCancellationRequested(); return; }

                    LoopCC++; TLoopCC++;
                    List<UCell>   BDL = GenerateSolCandidate( );  //候補問題生成
                    UProblem P = new UProblem(BDL);
                    pGNPX_Eng.GP = P;
//q                    pGNPX_Eng.SDA.SetProblem(GNPX_Eng);

                    pGNPX_Eng.AnalyzerCounterReset();
                    pGNPX_Eng.sudokAnalyzerAuto(ct);
            
                    if( GNPX_Engin.retCode==0 ){
                        string prbMessage;
                        int lvlBase;
                        int DifLevelT = pGNPX_Eng.DifficultyLevelChecker( out prbMessage, out lvlBase );
                        if( lvlBase<lvlLow || lvlHgh<lvlBase ) continue; //難易度チェック
           
                        P.DifLevelT = DifLevelT;
                        P.Name = prbMessage;
                        P.TimeStamp = DateTime.Now.ToShortDateString();
                        P.solMessage = pGNPX_Eng.DGViewMethodCounterToString();
                        pGNP00.SDK_ProblemListSet(P);
                    
                        SDKEventArgs se = new SDKEventArgs(ProgressPer:(--mlt));
                        Send_Progress(this,se);  //LoopCCと同じ方法でも情報を送れる。
                        if( CbxNextLSpattern ) rxCTRL=0; //次の問題生成時にLSパターンを変更
                    }
                }while(mlt>0);
            }
            catch( Exception ex ){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    #endregion 問題作成

    #region 解析
        public void AnalyzerReal( CancellationToken ct ){      //解析【ステップ】
            int ret2=0;
            retNZ=-1; LoopCC++; TLoopCC++;
            pGNPX_Eng.Set_GNPZMethodList(false); 
            pGNPX_Eng.AnalyzerControl( ct, ref ret2, true );
            SDKEventArgs se = new SDKEventArgs(ProgressPer:(retNZ));
            Send_Progress(this,se);  //LoopCCと同じ方法でも情報を送れる。
        }
        public void AnalyzerRealAuto( CancellationToken ct ){   //解析【全て】
            LoopCC++; TLoopCC++;
            bool MltSolOn = GNumPzl.MltSolOn;
            pGNPX_Eng.Set_GNPZMethodList(false);
            pGNPX_Eng.sudokAnalyzerAuto(ct);
            SDKEventArgs se = new SDKEventArgs(ProgressPer:(GNPX_Engin.retCode));
            Send_Progress(this,se);  //LoopCCと同じ方法でも情報を送れる。
        }
    #endregion 解析
  
        private const int ParaSolsNo=65536;   //10; //■forDevelop■ //65535;
        public LatinSqureGen LSP;
        private int s1=0, s2=0;
        static public int rxCTRL=0;      //=0:新たなパターンで問題候補生成 

        private int[,] Sol99=new int[9,9];

        public int[] GenSolPatternsList( bool RandF, int GenLSTyp ){
         
            switch(GenLSTyp){
                case 0: GenerateLatinSqure(ref rxCTRL,Sol99); break;
                case 1: GenerateLatinSqure2(ref rxCTRL,Sol99); break;
            }

            int[] SolX = new int[81];
            for( int rc=0; rc<81; rc++ ) SolX[rc] = Sol99[rc/9,rc%9];
            if( RandF ) _DspNumRandmize(SolX); //表示数字のランダム変更
            _ApplyPattern(SolX);

            string st="";
            for( int k=0; k<27; k++ ) st += Sol99[k/9,k%9];
            for( int r=3; r<9; r++ ){
                for( int c=0; c<3; c++ )  st += Sol99[r,c];
                st += ",";
            }

            st += ",";
            for( int r=3; r<9; r++ ){
                for( int c=3; c<9; c++ )  if( SolX[r*9+c]>0 ) st += SolX[r*9+c];
            }

            return SolX;
        }
        
        public List<int[]> GenSolPatternsList( bool RandF,  int GenLSTyp, int SolNo ){  //並列処理用の問題生成
            var ASDKList = new List<int[]>();
            int nc = Math.Max( SolNo, ParaSolsNo );
            for( int k=0; k<nc; k++ ){
                switch(GenLSTyp){
                    case 0: GenerateLatinSqure(ref rxCTRL,Sol99); break;
                    case 1: GenerateLatinSqure2(ref rxCTRL,Sol99); break;
                }

                int[] SolX = new int[81];
                Sol99.CopyTo(SolX,0);
                if( RandF ) _DspNumRandmize(SolX); //表示数字のランダム変更
                _ApplyPattern(SolX);//◆
                ASDKList.Add(SolX);
            }
            return ASDKList;
        }
        private Permutation[] prmLst=new Permutation[9];
        private int[] URow;
        private int[] UCol;

        private int[,] PatOn = new int[9,7];

        public bool GenerateLatinSqure( ref int RX, int[,] LS ){
            if( RX<3 ){
                PatternCC++;
                LSP.GeneratePara( ref Sol99, s1, s2 );
                URow=new int[9]; UCol=new int[9];
                for( int r=0; r<3; r++ ){
                    for( int c=3; c<9; c++ ){
                        UCol[c] |= (1<<LS[r,c]);
                        URow[c] |= (1<<LS[c,r]); //r,c:逆に用いる
                    }
                }
                RX=3; prmLst[RX] = null;
            }

            do{
              LNxtLevel:
                Permutation prm=prmLst[RX];
                if( prm==null ) prmLst[RX]=prm=new Permutation(9,6);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=LS[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }

                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    for( int cx=3; cx<9; cx++ ){
                        nxtX=cx-3;
                        int no=prm.Pnum[nxtX]+1;
                        int noB = 1<<(no);
                        if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;
                        if( (URow[RX]&noB)>0 ) goto LNxtPrm;
                        if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm;
                        LS[RX,cx] = no;
                    }
                    if( RX==8 ){
                        if( _DEBUGmode_ )  __DBUGprint2(LS, "    ");
                        return true;//◆
                    }
                    prmLst[++RX]=null;
                    goto LNxtLevel;

                  LNxtPrm:
                    continue;
                }
            }while((--RX)>=3);

            return false;
        }
        public bool GenerateLatinSqure2( ref int RX, int[,] LS ){
            //露出数字に対する変動部分生成。
            if( RX<3 ){
                PatternCC++;
                LSP.GeneratePara( ref Sol99, s1, s2 );
                URow=new int[9]; UCol=new int[9];
                for( int r=0; r<3; r++ ){
                    for( int c=3; c<9; c++ ){
                        UCol[c] |= (1<<LS[r,c]);
                        URow[c] |= (1<<LS[c,r]); //r,c:逆に用いる
                    }
                }
                RX=3; prmLst[RX] = null;

                for( int r=3; r<9; r++ ){
                    int nc=0;
                    for( int c=3; c<9; c++ ){
                        if( PatGen.GPat[r,c]>0 ) PatOn[r,nc++]=c;
                    }
                    PatOn[r,6]=nc;

                //    Console.Write("\r##  {0}:", r);
                //    for( int c=3; c<7; c++ )  Console.Write(" "+PatOn[r,c] );
                }
            }

            if( RX==8 )  while(PatOn[RX,6]<=0) RX--;//変動部分が1行空白のとき

            do{
              LNxtLevel:
                Permutation prm=prmLst[RX];
                if( prm==null ) prmLst[RX]=prm=new Permutation(9,PatOn[RX,6]);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=LS[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }
                int nc=PatOn[RX,6];
                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    for( int cx2=0; cx2<nc; cx2++ ){
                        nxtX=cx2;
                        int cx=PatOn[RX,cx2];
                        int no=prm.Pnum[nxtX]+1;
                        int noB = 1<<(no);
                        if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;
                        if( (URow[RX]&noB)>0 ) goto LNxtPrm;
                        if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm;
                        LS[RX,cx] = no;
                    }
                    if( RX==8 ){
                        if( _DEBUGmode_ )  __DBUGprint2(LS, "    ");
                        return true;//◆
                    }
                    prmLst[++RX]=null;
                    goto LNxtLevel;

                  LNxtPrm:
                    continue;
                }
                while( (--RX)>=3 && PatOn[RX,6]<=0 );
            }while(RX>=3);

            return false;
        }   

        private void _DspNumRandmize( int[] P ){
            List<int> ranNum = new List<int>();
            for( int r=0; r<9; r++ )  ranNum.Add( rnd.Next(0,9)*10 + r );
            ranNum.Sort( (x,y) => (x-y) );
            for( int r=0; r<9; r++) ranNum[r] %= 10;

            for( int rc=0; rc<81; rc++ ){
                int n=P[rc];
                if( n>0 ) P[rc] = ranNum[n-1]+1;
            }
        } 

        private void _ApplyPattern( int[] X ){
            for( int rc=0; rc<81; rc++ ){
                if( PatGen.GPat[rc/9,rc%9]==0 ) X[rc]=0;
            }
            if( _DEBUGmode_ )  __DBUGprint2(X, "    ");
       }

       private void __DBUGprint2( int[,] pSol99, string st="" ){
            string po;
            Console.WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int wk = pSol99[r,c];
                    if( wk==0 ) po += " .";
                    else po += wk.ToString(" #");
                }
                Console.WriteLine(po);
            }
        }
        private void __DBUGprint2( int[] X, string st="" ){
            string po;
            Console.WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int wk = X[r*9+c];
                    if( wk==0 ) po += " .";
                    else po += wk.ToString(" #");
                }
                Console.WriteLine(po);
            }
        }
    }
}
