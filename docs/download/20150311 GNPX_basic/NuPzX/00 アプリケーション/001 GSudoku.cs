using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

using GIDOO_space;

namespace GNPZ_sdk{

    //▼▼▼ToDo 問題管理クラスを作る▼▼▼

    public class GNumPzl{
        public NuPz_Win         pGNP00win;

        static public bool[]    SlvMtdCList = new bool[60];
        static public bool      MltSolOn;
        static public Dictionary<string,string> GMthdOption;
        static public Dictionary<string,Color> ColorDic=new Dictionary<string,Color>();  

        public int              cellSize = 36;
        public int              cellSizeP;
        public int              lineWidth = 1;
        public GFont            gsFont = new GFont( "Times New Roman", 22 );
        public bool             developMode = false;      //開発モード

        public SDK_Ctrl         SDKCntrl;         //問題生成の制御
        public GNPX_Engin       GNPX_Eng;         //カレント問題の解析エンジン
   
        public string           SDK_MethodsFileName = "SDK_Methods.txt";

        public List<UProblem>   SDKProbLst = new List<UProblem>();  //数独問題のリスト

        public UProblem         pGP{ get{ return GNPX_Eng.GP; } }//現問題の実体は GNPX_Eng にある

        private int             _CurrentPrbNo;
        public int              CurrentPrbNo{
            get{ return _CurrentPrbNo; }
            set{ 
                int nn=value;
                if( nn==99999 ) nn=SDKProbLst.Count-1;
                if( nn<0 ) nn=0;
                if( nn>=SDKProbLst.Count ) nn=SDKProbLst.Count-1;
                _CurrentPrbNo=nn;
                GNPX_Eng.SetGP( SDKProbLst[nn] );
            }
        }
        
        public void SDK_Save( UProblem UP ){
            UP.ID=SDKProbLst.Count;
            SDKProbLst.Add(UP);
        }
        public void SDK_Save_EngGP(){
            SDK_Save(GNPX_Eng.GP);
        }   

        public void SDK_Save_ifNotContain(){
            UProblem pGP=GNPX_Eng.GP;
            if( !Contain(pGP) )  SDK_Save_EngGP();
        }
        public void SDK_Remove(){
            UProblem pGP=GNPX_Eng.GP;
            int PnoMemo=CurrentPrbNo;
            if( PnoMemo==SDKProbLst.Count-1 ) PnoMemo--;
            if( Contain(pGP) ) SDKProbLst.Remove(pGP);
            int id=0;
            SDKProbLst.ForEach(P=>P.ID=(id++));
            CurrentPrbNo=PnoMemo;
        }

        public bool Contain( UProblem UP ){
            return (SDKProbLst.Find(P=>P.HTicks==UP.HTicks)!=null);
        }

        public void CESetGP( UProblem UP ){
            GNPX_Eng.SetGP(UP);
        }

        public void CreateNewPrb( UProblem UP=null ){
            if( UP==null ) UP = new UProblem("新問題");
            UP.ID=SDKProbLst.Count;
            GNPX_Eng.SetGP(UP);
            SDK_Save(UP);
            CurrentPrbNo=99999;
        }

    //▼▼▼　公開版では削除
        static public string    NiceLoopMethod;  
        static public bool      NewAlgorithm;
    //▲▲▲

        static public  int      NiceLoopMax{ get{ return GMthdOption["NiceLoopMax"].ToInt(); } }
        static public int       ALSSizeMax{  get{ return GMthdOption["ALSSizeMax"].ToInt(); } }

        public GNPZ_Graphics    SDKGrp;           //盤面表示ビットマップの作成

  
        //***** 制御
        public string           GSmode = "問題作成"; //"","問題作成","数字変更","解析"
        public int              SelectedIndexPre = 0;
        public string           AnalyzerMode;
        public List<UItemCheck> GMthdLst;

        public bool             MltSolSave=false;

        static GNumPzl( ){
            GMthdOption = new Dictionary<string,string>();
            GMthdOption["NiceLoopMax"] = "15";
            GMthdOption["ALSSizeMax"] = "5";

            GMthdOption["MltSearchLvMax"] = "10";
            GMthdOption["MltSearchNoMax"] = "50";
            GMthdOption["MltSearchTmMax"] = "10";
            
            GMthdOption["Cell"] = "1";
            GMthdOption["GroupedCells"]  = "0";
            GMthdOption["ALS"]  = "0";

            ColorDic=new Dictionary<string,Color>();
            ColorDic["Board"]        = Color.FromArgb(255,220,220,220);
            ColorDic["BoardLine"]    = Colors.Navy;

            ColorDic["CellForeNo"]   = Colors.Navy;
            ColorDic["CellBkgdPNo"]  = Color.FromArgb(255,160,160,160);
            ColorDic["CellBkgdMNo"]  = Color.FromArgb(255,190,190,200);
            ColorDic["CellBkgdZNo"]  = Colors.White;
            ColorDic["CellBkgdZNo2"] = Color.FromArgb(255,150,150,250);

            ColorDic["CellBkgdFix"]  = Colors.LightGreen;
            ColorDic["CellFixed"]    = Colors.Red;
        }

        public GNumPzl( NuPz_Win pGNP00win ){
            this.pGNP00win = pGNP00win;
            cellSizeP  = cellSize+lineWidth;
            SDKCntrl   = new SDK_Ctrl(this,0);
            GNPX_Eng  = new GNPX_Engin(new UProblem());
            UPrbMltMan.pGNPX_Eng = GNPX_Eng;

            GMthdLst = new List<UItemCheck>();
            GNPX_Engin.pGMthdLst = GMthdLst;
        }

        public UProblem GetCurrentProble( ){
            UProblem P=null;
            if( CurrentPrbNo>=0 && CurrentPrbNo<=SDKProbLst.Count-1 ){
                P = SDKProbLst[CurrentPrbNo];
            }
            return P;
        }

    #region ファイルIO
        public int SDK_FileInput( string fName, bool prbIniFlag ){
            char[] sep=new Char[]{ ' ', ',', '\t' };        
            string LRecord, pName="";

            using( StreamReader SDKfile=new StreamReader(fName) ){
                SDKProbLst.Clear();

                while( (LRecord=SDKfile.ReadLine()) !=null ){
                    if( LRecord=="" ) continue;
                    string[] eLst = LRecord.SplitEx(sep);
                   
                    if( LRecord[0]=='#' ){ pName=LRecord.Substring(1); continue; }
                    
                    int nc = eLst.Length;
                    if( eLst[0]=="sPos" ) continue;

                    string name="";
                    int difLvl=1;
                    if( eLst[0].Length>=81 ){
                        try{
                            if( nc>=2 && eLst[1]!=null ) name=eLst[2];
                            else name=pName;
                            if( nc>=3 && eLst[2]!=null ) difLvl=eLst[3].ToInt();
                            if( difLvl<=0 || difLvl>30 )  difLvl=1;

                            string st = eLst[0].Replace(".", "0").Replace(" ", "");
                            List<UCell> BDLa = new List<UCell>();
                            for( int rc=0; rc<81; rc++ ) BDLa.Add(new UCell(rc,st[rc].ToInt()));
                            int ID=SDKProbLst.Count; 
                            SDKProbLst.Add(new UProblem(ID,BDLa,name,difLvl));  
                        }
                        catch{ continue; }
                    }
                    else if( nc>=2 ){
                        try{
                            name = (eLst.Length>=2)? eLst[1]: "";
                            difLvl =  (eLst.Length>=3)? eLst[2].ToInt(): 0;
                            List<UCell> BDLa = new List<UCell>();
                            for( int r=0; r<9; r++ ){
                                LRecord = SDKfile.ReadLine();
                                eLst = LRecord.SplitEx(sep);
                                for( int c=0; c<9; c++ ){
                                    int n = Convert.ToInt32(eLst[c]);
                                    n = (n<0 && prbIniFlag)? 0: n;
                                    BDLa.Add(new UCell(r*9+c,n));
                                }
                            }
                            int ID=SDKProbLst.Count;
                            SDKProbLst.Add(new UProblem(ID,BDLa,name,difLvl));
                        }
                        catch{ continue; }
                    }
                }

                CurrentPrbNo=0;
                return CurrentPrbNo;
                
            }   
        }
        public void SDK_StringInput( string st ){
            st = st.Replace(".", "0");
            List<UCell> BDLa = new List<UCell>();
            for( int rc=0; rc<81; rc++ ) BDLa.Add(new UCell(rc,st[rc].ToInt()));
            SDKProbLst.Add(new UProblem(999,BDLa,"",0));
            int ID=0;
            SDKProbLst.ForEach(P=>P.ID=(ID++));
            CurrentPrbNo=(ID-1);
        }
          
        public UProblem SDK_ToUProblem( string st, string name="", int difLvl=0, bool saveF=false ){
            st = st.Replace(".", "0").Replace(" ", "");
            List<UCell> B = new List<UCell>();
            for( int rc=0; rc<81; rc++ ) B.Add(new UCell(rc,st[rc].ToInt()));
            var UP=new UProblem(999,B,name,difLvl);
            if(saveF) SDK_Save(UP);
            return UP; 
        }
        public void SDK_TransProb( string ctrl ){
            int ID=CurrentPrbNo;
            UCell[] P = new UCell[81];
            pGP.BDL.CopyTo(P);
            List<UCell> R=new List<UCell>();
            switch(ctrl){
                case "btnTransRt":
                    for( int rc=0; rc<81; rc++ ){
                        UCell Q = P[(8-rc%9)*9+rc/9];
                        R.Add(new UCell(rc,Q.No,Q.FreeB));
                    }
                    break;
                case "btnTransRL":
                    for( int rc=0; rc<81; rc++ ){
                        UCell Q = P[rc+8-(rc%9)*2];
                        R.Add(new UCell(rc,Q.No,Q.FreeB));
                    }
                    break;
                case "btnTransUD":
                    for( int rc=0; rc<81; rc++ ){
                        UCell Q = P[72-rc/9*9+(rc%9)];
                        R.Add(new UCell(rc,Q.No,Q.FreeB));
                    }
                    break;
            }
            SDKProbLst[ID] = new UProblem(ID,R);
            CurrentPrbNo=ID;
        }

        public void _SDK_Ctrl_Initialize(){
            AnalyzerMode = "解析";
            GNPX_Eng.GP.GNPZ_ResultLong = "";
            GNPX_Eng.SDA.SetProblem(GNPX_Eng);

            GNPX_Eng.cCodePre = 0;
            GNPX_Eng.AnalyzerCounterReset( );
            GNPX_Eng.SDA.gNoPzAnalyzerReset( 0, true );      //問題の最初に戻す
            GNPX_Eng.SDA.SetBoardFreeB();
        }
        public void SDK_FileOutput( string fName, bool append, bool fType81, bool SolSort, bool SolSet, bool SolSet2 ){
            if( SDKProbLst.Count==0 )  return;

            SDK_Ctrl.MltProblem = 1;    //単独
            SDK_Ctrl.lvlLow = 0;
            SDK_Ctrl.lvlHgh = 999;

            string LRecord, solMessage="";
            GNumPzl.SlvMtdCList[0] = true;  //全手法を用いる

            var tokSrc = new CancellationTokenSource();　//中断時の手続き用

            int m=0;
            SDKProbLst.ForEach( p=>p.ID=(++m) ); //▼▼▼ToDo 問題管理クラスを作る▼▼▼
            IEnumerable<UProblem> qry;
            if( SolSort ) qry = from p in SDKProbLst orderby p.DifLevelT ascending select p;
            else qry = from p in SDKProbLst select p;

            using( StreamWriter SDKfile=new StreamWriter(fName,append,System.Text.Encoding.Default) ){
                foreach( var gp in qry ){

                    //===== 準備 =====
                    solMessage = "";
                    if( SolSet ){//解を出力
                        CurrentPrbNo = gp.ID;
                        GNPX_Eng.GP =gp;

                        string prbMessage="";
                        if( GNPX_Eng.GP.DifLevelT<=0 || GNPX_Eng.GP.Name=="" ){
                            foreach( var p in gp.BDL )  if( p.No<0 ) p.No=0;

                            GNPX_Eng.SDA.SetBoardFreeB();
                            GNPX_Eng.AnalyzerCounterReset();
                            GNPX_Eng.sudokAnalyzerAuto(tokSrc.Token);                      
                            if( GNPX_Engin.retCode<0 ){
                                GNPX_Eng.GP.DifLevelT = -999;
                                GNPX_Eng.GP.Name = "解けない";
                            }
                            else{
                                int lvlBase;
                                int difficult = GNPX_Eng.DifficultyLevelChecker( out prbMessage, out lvlBase );
                                GNPX_Eng.GP.DifLevelT = difficult;
                                GNPX_Eng.GP.Name = prbMessage;
                            }
                        }
                        solMessage = prbMessage;
                        if( SolSet2 ) solMessage += GNPX_Eng.DGViewMethodCounterToString();　//適用手法を付加
                    }
                    
                    if( fType81 ){　//1行出力(解出力なし）
                        LRecord = "";
                        gp.BDL.ForEach(q =>{
                            int no=Math.Max(q.No,0);
                            LRecord += no.ToString();
                        } );
                        LRecord=LRecord.Replace("0",".");
                        LRecord += " " + gp.ID + "  \"" + gp.Name + "\"";
                        LRecord += " " + gp.DifLevelT;
                        SDKfile.WriteLine(LRecord);
                    }
                    else{ //タイトルとマトリクス型解出力
                        LRecord = gp.ID.ToString() + ", \"" + gp.Name + "\"";
                        LRecord += ", " + gp.DifLevelT.ToString();
                        LRecord += ","+ solMessage;
                        SDKfile.WriteLine(LRecord);

                        for( int r=0; r<9; r++ ){
                            int n = gp.BDL[r*9+0].No;
                            if( !SolSet && n<0 ) n = 0;
                            LRecord = n.ToString();
                            for( int c=1; c<9; c++ ){
                                n = gp.BDL[r*9+c].No;
                                if( !SolSet && n<0 ) n = 0;
                                LRecord += ", " + n.ToString();
                            }
                            SDKfile.WriteLine(LRecord);
                        }
                    }
                }
            }
            GNumPzl.SlvMtdCList[0] = false;  //手法選択を有効に戻す

            m=0;
            SDKProbLst.ForEach( p=>p.ID=(m++) ); //▼▼▼
        }

        public void btnFavfileOutput( bool fType81=true ){
            string LRecord;
            string fNameFav = "◆お気に入り.txt";

            var tokSrc = new CancellationTokenSource();　//中断時の手続き用
            GNumPzl.SlvMtdCList[0] = true; //全手法を用いる

            UProblem pGP=GNPX_Eng.GP;
            GNPX_Eng.SDA.SetBoardFreeB();
            GNPX_Eng.sudokAnalyzerAuto(tokSrc.Token);
            string prbMessage;
            int    prbLvl;
            int difLvl = GNPX_Eng.DifficultyLevelChecker( out prbMessage, out prbLvl );

            using( var fpW=new StreamWriter(fNameFav,true) ){
                if( fType81 ){
                    LRecord = "";
                    pGP.BDL.ForEach(q => LRecord += (Math.Max(q.No,0)).ToString() );
                    LRecord=LRecord.Replace("0",".");
                }
                else{
                    LRecord = pGP.ID.ToString() + ", \"" + prbMessage + "\"";
                    LRecord += ", " + GNPX_Eng.GP.DifLevelT.ToString();
                    LRecord += ", \"" + prbLvl+  "\"";
                    fpW.WriteLine(LRecord);
                
                    for( int r=0; r<9; r++ ){
                        int n = pGP.BDL[r*9+0].No;
                        LRecord = n.ToString();
                        for( int c=1; c<9; c++ ){
                            n = pGP.BDL[r*9+c].No;
                            LRecord += ", " + n.ToString();
                        }
                        LRecord += "\r";
                    }
                }
                fpW.WriteLine(LRecord);
            }
            GNumPzl.SlvMtdCList[0] = false;//手法指定有効
        }

        public void SDK_ProblemListSet( UProblem GPx ){
            GPx.ID=SDKProbLst.Count;
            SDKProbLst.Add(GPx);
            if( MltSolSave ){
                string fName = "連続作成" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                emergencyFilePut( "連続作成", fName );
            }
            CurrentPrbNo = SDKProbLst.Count-1;
        }
        private  void emergencyFilePut( string dirStr, string fName ){
            if( !Directory.Exists(dirStr) ){ Directory.CreateDirectory(dirStr); }
            using( var fpW=new StreamWriter(dirStr+@"\"+fName,false) ){     
                foreach( UProblem P in SDKProbLst ){
                    string st = "";
                    P.BDL.ForEach( q =>{ st += Math.Max(q.No,0).ToString(); } );
                    st += ", " + (P.ID+1) + "  ,\"" + P.Name + "\"";
                    st += ", " + P.DifLevelT.ToString();
                    st += ", \"" + P.TimeStamp +  "\"";
                    fpW.WriteLine(st);
                }
            }   
        }
    #endregion ファイルIO

    #region 解法リスト
        public List<UItemCheck> GetMethodListFromFile( ){
            List<string> SDK_ALtemp = GNPX_Engin.SDK_Methods0.ConvertAll(P=>P.MethodName);
            char[] sep=new char[]{' ',','};
   
            string st;
            int IDx=0;
            GMthdLst.Clear();

            if( File.Exists(SDK_MethodsFileName) ){
                using( var fIn=new StreamReader(SDK_MethodsFileName) ){
                    while( (st=fIn.ReadLine()) !=null ){
                        bool bChk = true;
                        if( st[0]=='-' ){ bChk=false; st=st.Substring(1); }
                        else if( st[0]=='*' ){
                            var mLst= st.Split(sep,StringSplitOptions.RemoveEmptyEntries);
                            st=mLst[0].Substring(1);
                            GMthdOption[st] = mLst[1];
                        }

                        int nx = SDK_ALtemp.FindIndex(x=>x==st);
                        if( nx>=0 ){
                            SDK_ALtemp.RemoveAt(nx);
                            GMthdLst.Add(new UItemCheck(IDx++,st,bChk));
                        }
                    }
                }

                if( SDK_ALtemp.Count>0 ){
                    SDK_ALtemp.ForEach( mt => GMthdLst.Add(new UItemCheck(IDx++,mt,false)) );
                }
            }
            else{
                SDK_ALtemp.ForEach( mt => GMthdLst.Add(new UItemCheck(IDx++,mt,true)) );
                MethodListOutPut();
            }
            if( GMthdLst.Count==0 ) ResetMethodList();

            return GMthdLst;
        }
        public List<UItemCheck> ResetMethodList(){
            int ID=0;
            GMthdLst = GNPX_Engin.SDK_Methods0.ConvertAll(P=>(new UItemCheck(ID++,P.MethodName,true)));
            MethodListOutPut();
            return GMthdLst;
        }
        
        public List<UItemCheck> ChangeMethodList( int nx, int UD ){
            UItemCheck MA=GMthdLst[nx], MB;
            if( UD<0 ){ MB=GMthdLst[nx-1]; GMthdLst[nx-1]=MA; GMthdLst[nx]=MB; }
            if( UD>0 ){ MB=GMthdLst[nx+1]; GMthdLst[nx+1]=MA; GMthdLst[nx]=MB; }
            return GMthdLst;
        }
        public void MethodListOutPut(){
            using( var fOut=new StreamWriter(SDK_MethodsFileName) ){
             // Console.WriteLine("MethodListOutPut");
                GMthdLst.ForEach( mt => fOut.WriteLine( (mt.IsChecked? "": "-")+mt.Name ) );
                foreach( var P in GMthdOption ) fOut.WriteLine("*"+P.Key +" "+P.Value );
            }
        }

    #endregion 解法リスト

    }

    public class UItemCheck{
        public int ID{ get; set; }
        public string Name{ get; set; }
        public bool IsChecked{ get; set; }
        public UItemCheck( int ID, string NameX, bool chk ){
            this.ID=ID; Name=NameX; IsChecked=chk;
        }
    }
}