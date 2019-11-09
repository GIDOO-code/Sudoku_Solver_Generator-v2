using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk {

  #region 制御系
    public partial class GNPZ_Analyzer{
        static public List<MethodInfo> methInfoList = new List<MethodInfo>(); 
        static private bool  SolInfoDsp;
        static public int    pAnalyzerCC;

        private int          SolCode{
            get{ return pENGN.GP.SolCode; }
            set{ pENGN.GP.SolCode=value; }
        }

        private GNPX_Engin   pENGN;
        private UProblem     pGP{ get{ return pENGN.GP; } }
        private List<UCell>  pBDL{ get{ return pGP.BDL; } }
        private bool         MltSolOn{ get{ return GNumPzl.MltSolOn; } }
        private int          NiceLoopMax{ get{ return GNumPzl.GMthdOption["NiceLoopMax"].ToInt(); } }
        private int          ALSSizeMax{ get{ return GNumPzl.GMthdOption["ALSSizeMax"].ToInt(); } }
        public bool          NewAlgorithm;

        public CellLinkMan   CeLKMan;
        public SuperLinkMan  SprLKMan;

        public ALSLinkMan    ALSMan;
//開発  public ALSLinkMan    ALS2s; //+1と+2のALS

        private int          cCode;

//      public  string       AnalyzerLog;
        public  string       Result{ get{ return pGP.GNPZ_Result; } set{ pGP.GNPZ_Result=value; } }
        public  string       ResultLong{ get{ return pGP.GNPZ_ResultLong; } set{ pGP.GNPZ_ResultLong=value; } }

        public  GNPZ_Analyzer( GNPX_Engin SDKS ){
            pENGN = SDKS;

            int mC = methInfoList.Count;
            mthdCCList = new methodCouter[mC];
            for( int m=0; m<mC; m++ ) mthdCCList[m] = new methodCouter();

            CeLKMan = new CellLinkMan(this);
            ALSMan = new ALSLinkMan(this);
//開発      ALS2s = new ALSLinkMan(this);

            SprLKMan = new SuperLinkMan(this);

            SetConnectedCells(); //house参照用の定数の生成
        }

        public void SetProblem( GNPX_Engin pENGN ){
            this.pENGN = pENGN;

            NewAlgorithm  = GNumPzl.NewAlgorithm;
            SolInfoDsp    = GNPX_Engin.SolInfoDsp;
            cCode         = GNPX_Engin.cCode;
            Result        = pGP.GNPZ_Result;
            ResultLong    = pGP.GNPZ_ResultLong;
        }

        public void Analyzer_Initialize(){
            CeLKMan.SWCtrl=0;
            ALSMan.Reset();
//開発      ALS2s.ALSLst=null;
            CeLKMan.LinkCeAlsLst=null;
            pBDL.ForEach(P=>P.ECrLst=null);

            SprLKMan.Reset();
//          UCellLink.pBDL=pBDL;
        }

        private bool SnapSaveGP( bool saveAll=false ){
            if( !SDK_Ctrl.MltAnsSearch )  return false;

            if( SDK_Ctrl.GPMX.MltUProbLst==null )  SDK_Ctrl.GPMX.MltUProbLst=new List<UProblem>();
            else if( SDK_Ctrl.GPMX.MltUProbLst.Count>=SDK_Ctrl.MltAnsOption[1] ){
                SDK_Ctrl.MltAnsOption[4]=1;
                return false;
            }

            if( saveAll || SDK_Ctrl.GPMX.MltUProbLst.All(P=>(P.GNPZ_Result!=pGP.GNPZ_Result)) ){
                UProblem GPX=pGP.Copy();
                SDK_Ctrl.GPMX.MltUProbLst.Add(GPX);
            }

            pBDL.ForEach(p=>p.ResetAnalysisResult());
            pGP.SolCode=-1;
            return true;
        }

        private bool CheckTimeOut(){ //true:TimeOut
            //時間のかかる処理のみに組込む
            if( !SDK_Ctrl.MltAnsSearch )  return false;
            int ts = (int)(DateTime.Now-(DateTime.Today.AddHours(-1))).TotalSeconds - SDK_Ctrl.MltAnsOption[3];
            bool tmout = (ts>=SDK_Ctrl.MltAnsOption[2]);
            if( tmout )  SDK_Ctrl.MltAnsOption[4]=2;
            return tmout;
        }
    }
  #endregion 制御系

  #region 関連性フィルター
    public partial class GNPZ_Analyzer{　//関連チェック用
        public Bit81[] ConnectedCells;    //軸セル(rc)に関連するセル
        public Bit81[] ConnectedCellsRev; //軸セル(rc)に関連しないセル     //▼▼▼旧・次期開発をリストラしたときに廃止▼▼▼
        public Bit81[] HouseCells;        //行列ブロック(0-26)の関連セル
 
        private void SetConnectedCells(){
            if( ConnectedCells!=null )  return;
            ConnectedCells    = new Bit81[81];
            ConnectedCellsRev = new Bit81[81];
            int rb=0, cb=0, tp, fx;
            for( int r=0; r<9; r++ ){
                for ( int c=0; c<9; c++ ){
                    Bit81 BS = new Bit81();
                    for( int k=0; k<9; k++ ) if( k!=c ) BS.BPSet(r*9+k);
                    for( int k=0; k<9; k++ ) if( k!=r ) BS.BPSet(k*9+c);
                    fx = r/3*3 + c/3;
                    for( int k=0; k<9; k++){
                        GetTupleRC( 2, fx, k, ref rb, ref cb );
                        if( rb!=r && cb!=c )  BS.BPSet( rb*9+cb );
                    }
                    ConnectedCells[r*9+c]    = BS;
                    ConnectedCellsRev[r*9+c] = BS ^ 0x7FFFFFF;
                }
            }

            HouseCells = new Bit81[27];
            for( int k=0; k<27; k++ ){
                tp=k/9;
                fx=k%9;
                Bit81 tmp=new Bit81();
                for( int nx=0; nx<9; nx++ ){ 
                    tmp.BPSet( GetTupleRC(tp,fx,nx) );
                }
                HouseCells[k] = tmp;
            }
        }
    }
  #endregion 関連性フィルター

    #region 共通関数
    public partial class GNPZ_Analyzer{
        private string[]    rcbStr = new string[]{ "r", "c", "b" };
        private int[,,]     TTbitCheck = new int[10, 10, 9];
        public methodCouter[] mthdCCList = new methodCouter[100];

        //===== Result ====================
        // カラーネーム一覧表 http://www.coara.or.jp/~ynakamra/iro/colorname.html //▼削除

        private Color[] _ColorsLst = new Color[]{
            Colors.LightGreen, Colors.Yellow,
            Colors.Aqua, Colors.MediumSpringGreen, Colors.Moccasin, Colors.YellowGreen, 
            Colors.Pink, Colors.ForestGreen, Colors.Aquamarine, Colors.Beige,
            Colors.Lavender, Colors.Magenta, Colors.Olive, Colors.SlateBlue };


        private Color       AttCr    = Colors.Red;
        private Color       AttCr2   = Colors.Blue;
        private Color       AttCr3   = Colors.Green;
        private Color       SolBkCr  = Colors.Yellow;
        private Color       SolBkCr2 = Colors.LightGreen;//Aqua;//SpringGreen//Colors.CornflowerBlue;  //FIn
        private Color       SolBkCr3 = Colors.Aqua;　　//Colors.CornflowerBlue;
        private Color       SolBkCr4 = Colors.CornflowerBlue;
        public int          BDCode;
        public int          SA_sq;

        public int BDCodeGet( ){
            int BDC=1, bin20=104729;    //10000番目の素数
            int nn;

            int rc=0;
            pBDL.ForEach( P =>{
                rc++;
                nn = P.No;
                if( nn<=0 ) nn=-nn;;
                BDC = BDC*17 + rc*119 + nn*19;
                BDC %= bin20;
            } );
            return BDC;
        }
         
        public bool cellPZMCounter( ref int nP, ref int nZ,ref int nM ){
            int P=0, Z=0, M=0;
            pBDL.ForEach( q =>{
                if( q.No>0 )      P++;
                else if( q.No<0 ) M++;
                else              Z++;
            } );
            nP=P; nZ=Z; nM=M;
            return pBDL.Any(q=>q.FreeB>0);
        }

        public int DifficultyLevelChecker(out string prbMessage ){
            int difFlag = 0;
            int n, lvl=0, lvlMax=0, kMax=0;
            double df=0;

            prbMessage = "";
            for( int k=0; k<mthdCCList.Length; k++ ){
                if( mthdCCList[k].counter <= 0 ) continue;
                n = methInfoList[k].lvlValue;
                if( n>=3 ) df += mthdCCList[k].DifLevelT;
                if( n<30 )  difFlag |= (1<<n);
                if( lvlMax<=n ){ lvlMax=n; kMax = k; } 
            }
            prbMessage = methInfoList[kMax].methodName;
            for( int k=0; k<32; k++){
                difFlag >>= 1;
                lvl = k+1;
                if( difFlag==0 ) break;
            }

            lvl += (int)(df/2.0);    //レベル3以上の手法が2つ以上あるとき +1
            return lvl;
        }

        //*** 規則のチェック（タップル内で同じ数字が使われていたら"false"を返す
        public bool gNoPzAnalyzerRouleCheck( ){
            bool    ret=true;

            int nc=0;
            pBDL.ForEach(q =>{ if(q.No==0) nc++; } );

            if( pGP.Insoluble==true ){ SolCode=9; return false; }

            for( int tfx=0; tfx<27; tfx++ ){
                int usedB=0, errB=0, rc=0;
                for( int k=0; k<9; k++ ){
                    UCell P = GetTupleRC( pBDL, tfx/9, tfx%9, k, ref rc );
                    if( P.No==0 ) continue;
                    int no=Math.Abs(P.No);
                    if( (usedB&(1<<no))!=0 ) errB |= (1<<no); //すでに使われている
                    usedB |= (1<<no);
                }

                if( errB==0 ) continue;
                for( int k=0; k<9; k++ ){
                    UCell P = GetTupleRC( pBDL, tfx/9, tfx%9, k, ref rc );
                    if( P.No==0 ) continue;
                    int no=Math.Abs(P.No);
                    if( (errB&(1<<no))!=0 ){ P.ErrorState=8; ret=false; }//警告フラッグを設定
                }
            }
            SolCode = ret? 0: 9; //99:ルール違反
            return ret;
        }

        //*** 候補が１つ
        public void gNoPzAnalyzerReset( int md, bool MltSolOn ){ //md=0:問題も含め全て初期化　md>0:問題以外を初期化
            pBDL.ForEach( P =>{
                if( P.No <= 0){
                    P.Reset_StepInfo();
                    P.FreeB = 0x1FF;
                    if( md==0 && P.No<0 ) P.No=0;
                }
            } );
            
            Array.ForEach( mthdCCList, p =>{ p.counter=0; p.DifLevelT=0.0; } );
        }

        // 確定処理
        public bool GNPZ_NumberFix( ){
            List<UCell> BDL=pGP.BDL;
            if( BDL.Any(p=>p.FixedNo>0) ){
                foreach( var P in BDL.Where(p=>p.No==0) ){
                    int No = P.FixedNo;
                    if( No<1 && No>9 ) continue;
                    P.FixedNo = 0;
                    P.No      = -No;
                    P.CellBgCr = Colors.Black;
                  }
                
                SetBoardFreeB(false);
                foreach( var P in pBDL.Where(p=>(p.No==0 && p.FreeBC==0)) )  P.ErrorState=9;
            }
            else if( BDL.Any(p=>p.CancelB>0) ){
                foreach( var P in pBDL.Where(p=>p.CancelB>0) ){
                    int CancelB  = P.CancelB ^ 0x1FF;
                    P.FreeB   &= CancelB;
                    P.CancelB  = 0;       
                    
//                    P.AttNmB   = 0;
//                    P.AttCrB2  = 0;
//                    P.AttCrB   = 0;
                    P.CellBgCr = Colors.Black;
                }
            }
            else{
                return false;   //解なし
  //              Console.WriteLine("System error SolCode:{0}", SolCode );
  //              Environment.Exit(999);
            }
            pBDL.ForEach(P=>P.ECrLst=null);

            SolCode = -1;
            return true;
        }
        public void SetBoardFreeB( bool allFlag=true ){
            pGP.Insoluble=false;
            foreach( var P in pBDL ){
                P.Reset_StepInfo();
                int freeB=0;
                if( P.No==0 ){
                    freeB = SetCellFreeB(P.rc);
                    if( !allFlag ) freeB &= P.FreeB;
                    if( freeB==0 ){ pGP.Insoluble=true; P.ErrorState=1; }//解なし
                }
                P.FreeB = freeB;
            }
        }
        public int SetCellFreeB( int rc ){ //Cell[rc]のフリー数字を求める
            int freeB=0;
            foreach( var P in pBDL.IEGetFixed_Pivot27(rc) ) freeB |= (1<<Math.Abs(P.No));

/* 比較例として以下を用いる 
            int r0 = rc/9;
            int c0 = rc%9;
            int b0 = rc.ToBlock();
            for( int k=0; k<9; k++ ){
                int no = pBDL[r0*9+k].No;
                if( no!=0 ) freeB |= (1<<Math.Abs(no));

                no = pBDL[k*9+c0].No;
                if( no!=0 ) freeB |= (1<<Math.Abs(no));

                int rcQ = b0.BlockNToRc(k); //((b0/3)*3+k/3)*9 + ((b0%3)*3+k%3);
                no = pBDL[rcQ].No;
                if( no!=0 ) freeB |= (1<<Math.Abs(no));
            }
*/
            return (freeB>>=1)^0x1FF; //右１ビットシフトで内部表現に変換
        }

        //========== タップル内のセル取得 ==========
        private List<int> GetTupleRCList( List<UCell> pBDL, int tp, int fx, int noB ){
#if DEBUG
            if( tp<0 || 2<tp || fx<0 || 9<=fx ){ Console.WriteLine( "*** tp:"+tp + " fx:"+fx ); return null; }
#endif
            List<int> SDKrcList=new List<int>();
            int rc=-1;
            for( int nx=0; nx<9; nx++ ){
                UCell BDX = pBDL[rc=GetTupleRC(tp,fx,nx)];
                if( BDX.No!=0 )  continue;
                if( (BDX.FreeB&noB)==0 )  continue;
                SDKrcList.Add(rc);
            }

            return SDKrcList;
        }
        private List<UCell> GetTupleList( List<UCell> pBDL, int tp, int fx, int noB ){
#if DEBUG
            if( tp<0 || 2<tp || fx<0 || 9<=fx ){ Console.WriteLine( "*** tp:"+tp + " fx:"+fx ); return null; }
#endif
            List<UCell> UCellList=new List<UCell>();
            int rc=-1;
            for( int nx=0; nx<9; nx++ ){
                UCell BDX = pBDL[rc=GetTupleRC(tp,fx,nx)];
                BDX.nx = nx;
                if( BDX.No!=0 )  continue;
                if( (BDX.FreeB&noB)==0 )  continue;
                UCellList.Add(BDX);
            }
            return UCellList;
        }
        private List<UCell> GetTupleList( List<UCell> pBDL, int tp, int fx, int noB, int rcbBPIn, ref int rcbBPOut ){
#if DEBUG
            if( tp<0 || 2<tp || fx<0 || 9<=fx ){ Console.WriteLine( "*** tp:"+tp + " fx:"+fx ); return null; }
#endif
            List<UCell> UCellList=new List<UCell>();
            int rc=-1;
            rcbBPOut = 0;
            for( int nx=0; nx<9; nx++ ){
                if( (rcbBPIn&(1<<nx))==0 ) continue;
                UCell P = pBDL[rc=GetTupleRC(tp,fx,nx)];
                if( P.No!=0 )  continue;
                if( (P.FreeB&noB)==0 )  continue;
                UCellList.Add(P);
                rcbBPOut |= (1<<nx);
            }
            return UCellList;
        }
#if false    
        private int GetTupleSeq27( int tp, int fx, int r0, int c0, out int r, out int c ){
            r=c=0;
            switch(tp){
                case 0: r=r0; c=fx; break; //行   
                case 1: r=fx; c=c0; break; //列
                case 2: int b0=r0/3*3+c0/3; r=(b0/3)*3+fx/3; c=(b0%3)*3+fx%3; break;//ブロック
            }
            if( r==r0 && c==c0 ) return -1;
            return r*9+c;
        }
#endif
        static public IEnumerable<int> GetTupleSeq27( int rc ){ 
            int r=0, c=0;
            for( int kx=0; kx<27; kx++ ){
                switch(kx/9){
                    case 0: r=rc/9; c=kx%9; break; //行   
                    case 1: r=kx%9; c=rc%9; break; //列
                    case 2: int b=rc/27*3+(rc%9)/3; r=(b/3)*3+(kx%9)/3; c=(b%3)*3+kx%3; break;//ブロック
                }
                yield return r*9+c;
            }
        }

        private int GetTupleRC( int tp, int fx, int nx ){ //nx=0...8
            int r=0, c=0;
            switch(tp){
                case 0: r=fx; c=nx; break;//行
                case 1: r=nx; c=fx; break;//列
                case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
            }
            return (r*9+c);
        }   
        private void GetTupleRC( int tp, int fx, int nx, ref int r, ref int c ){
            r=c=0;
            switch(tp){
                case 0: r=fx; c=nx; break;
                case 1: r=nx; c=fx; break;
                case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;
            }
        }
        private UCell GetTupleRC( List<UCell> pBDL, int tp, int fx, int nx, ref int rc ){
            int r=0, c=0;
            switch(tp){
                case 0: r=fx; c=nx; break;
                case 1: r=nx; c=fx; break;
                case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;
            }
            rc = r*9 + c;
            return pBDL[rc];
        }
    
        static public IEnumerable<int> GetTupleRC( int tp, int fx ){ //nx=0...8
            int r=0, c=0;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//行
                    case 1: r=nx; c=fx; break;//列
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
                }
                yield return (r*9+c);
            }
        }
        static public IEnumerable<int> GetOnRC( Bit81 X81 ){
            for( int rc=0; rc<81; rc++ ){
                if( X81.IsHit(rc) ) yield return rc;
            }
        }
    }
  #endregion 共通関数
}
