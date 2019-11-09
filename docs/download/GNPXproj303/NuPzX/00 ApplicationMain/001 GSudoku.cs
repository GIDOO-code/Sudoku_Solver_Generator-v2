using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using static System.Math;
using static System.Console;

using System.Windows.Media;
using System.Threading;

using GIDOO_space;

namespace GNPZ_sdk{
    public class GNPXApp000{
        public NuPz_Win         pGNP00win;

        static public bool[]    SlvMtdCList = new bool[60];
        static public bool      chbConfirmMultipleCells;
        static public Dictionary<string,string> GMthdOption;
        static public Dictionary<string,Color> ColorDic=new Dictionary<string,Color>();  

        public int              cellSize = 36;
        public int              cellSizeP;
        public int              lineWidth = 1;
        public GFont            gsFont = new GFont( "Times New Romaon", 22 );                  
        public bool             developMode = false;

        public SDK_Ctrl         SDKCntrl;           //Problem Generator
        public GNPZ_Engin       GNPX_Eng;           //Analysis Engine
   
        public string           SDK_MethodsFileName = "SDK_Methods_V2.txt";

        public List<UProblem>   SDKProbLst = new List<UProblem>();  //SUDOKU proble List

        public UProblem         pGP{ get{ return GNPX_Eng.pGP; } }  //current problem is in GNPX_Eng

        private int             _CurrentPrbNo;
        public int              CurrentPrbNo{
            get{ return _CurrentPrbNo; }
            set{ 
                int nn=value;
                if(nn==999999999) nn=SDKProbLst.Count-1;
                if(nn<0) nn=0;
                if(SDKProbLst.Count>0){
                    if(nn>=SDKProbLst.Count ) nn=SDKProbLst.Count-1;
                    _CurrentPrbNo=nn;
                    GNPX_Eng.SetGP( SDKProbLst[nn] );
                }
            }
        }

        static public int       NiceLoopMax{ get{ return GMthdOption["NiceLoopMax"].ToInt(); } }
        static public int       ALSSizeMax{  get{ return GMthdOption["ALSSizeMax"].ToInt(); } }
        static public int       GeneralLogicOn{ get{ return GMthdOption["GeneralLogicOn"].ToInt(); } }

        public GNPZ_Graphics    SDKGrp;             //board bitmap
  
        public string           GSmode = "tabACreate";
        public int              SelectedIndexPre = 0;
        public string           AnalyzerMode;

        public List<UAlgMethod> SolverLst1 = new List<UAlgMethod>();
        public List<UMthdChked> SolverLst2 = new List<UMthdChked>(); //valid analysis routines List
        public List<string>     LanguageLst;
   
        public PuzzleTrans      PTrans;             //Transform 

        static GNPXApp000( ){
            GMthdOption = new Dictionary<string,string>();
            GMthdOption["NiceLoopMax"] = "10";
            GMthdOption["ALSSizeMax"] = "5";

            GMthdOption["MltSearchLvMax"] = "10";
            GMthdOption["MltSearchNoMax"] = "50";
            GMthdOption["MltSearchTmMax"] = "10";
            
            GMthdOption["Cell"]           = "1";
            GMthdOption["GroupedCells"]   = "0";
            GMthdOption["ALS"]            = "0";
			GMthdOption["ForceLx"]        = "ForceL0";
  
            GMthdOption["GeneralLogicOn"] = "1";          
            GMthdOption["GenLogMaxSize"]  = "3";
            GMthdOption["GenLogMaxRank"]  = "1";

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
        public GNPXApp000( NuPz_Win pGNP00win ){
            List<string> DirLst=Directory.EnumerateDirectories(".").ToList();
            LanguageLst=new List<string>();
            LanguageLst.Add("en");
            foreach( var P in DirLst ){
                var Q=P.Replace(".","").Replace("\\","");
                if(Q=="en")  continue;
                if(Q.Length==2 ||(Q[2]=='-' && Q.Length==5)) LanguageLst.Add(Q);
            }

            LanguageLst = LanguageLst.FindAll(P=>(P.Length==2 ||(P[2]=='-' && P.Length==5)));
            LanguageLst.Sort();

            this.pGNP00win = pGNP00win;
            cellSizeP  = cellSize+lineWidth;
            SDKCntrl   = new SDK_Ctrl(this,0);
            GNPX_Eng   = new GNPZ_Engin(this, new UProblem());
            UProbMan.pGNPX_Eng = GNPX_Eng;
            SDK_Ctrl.pGNPX_Eng = GNPX_Eng;
            
            PTrans = new PuzzleTrans(this);

            SolverLst2 = new List<UMthdChked>();
        }
        public void _SDK_Ctrl_Initialize(){
            AnalyzerMode = "Solve";
            GNPX_Eng.pGP.Sol_ResultLong = "";
            UProblem pGP=GetCurrentProble();
            GNPX_Eng.SetGP(pGP);

            GNPX_Eng.AnalyzerCounterReset( );
            GNPX_Eng.AnMan.ResetAnalysisResult(true);   //Return to initial state
            GNPX_Eng.AnMan.Set_CellFreeB();
            SDK_Ctrl.UGPMan=null;                       //initialize Multi_solver
			GNPX_Eng.pGP.extRes="";
        }

    #region Problem Management
        public UProblem GetCurrentProble( ){
            UProblem P=null;
            if( CurrentPrbNo>=0 && CurrentPrbNo<=SDKProbLst.Count-1 ){
                P = SDKProbLst[CurrentPrbNo];
            }
            return P;
        }
            
        public void SDK_Save( UProblem UP ){
            UP.ID=SDKProbLst.Count;
            SDKProbLst.Add(UP);
        }
        public void SDK_Save_EngGP(){
            SDK_Save(GNPX_Eng.pGP);
        }   
        public void CreateNewPrb( UProblem UP=null ){
            if( UP==null ) UP = new UProblem("New Problem");
            UP.ID=SDKProbLst.Count;
            GNPX_Eng.SetGP(UP);
            SDK_Save(UP);
            CurrentPrbNo=999999999;
        }
     
        public void SDK_Save_ifNotContain(){
            UProblem pGP=GNPX_Eng.pGP;
            if( !Contain(pGP) )  SDK_Save_EngGP();
        }
        public void SDK_Remove(){
            UProblem pGP=GNPX_Eng.pGP;
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
    #endregion Problem management

    #region file I/O
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
                    string TimeStamp="";
                    if( eLst[0].Length>=81 ){
                        try{
                            if( nc>=3 && eLst[2]!=null ) name=eLst[2];
                            else name=pName;
                            if( nc>=4 && eLst[3]!=null ) difLvl=eLst[3].ToInt();
                            if( difLvl<=0 || difLvl>30 ) difLvl=1;
                            if( nc>=5 && eLst[4]!=null ) TimeStamp=eLst[4];

                            string st = eLst[0].Replace(".", "0").Replace(" ", "");
                            List<UCell> BDLa=_stringToBDL(st);
                            int ID=SDKProbLst.Count; 
                            SDKProbLst.Add(new UProblem(ID,BDLa,name,difLvl,TimeStamp));  
                        }
                        catch{ continue; }
                    }
                    else if(nc>=2){
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
            List<UCell> BDLa=_stringToBDL(st);
            SDKProbLst.Add(new UProblem(999,BDLa,"",0));
            int ID=0;
            SDKProbLst.ForEach(P=>P.ID=(ID++));
            CurrentPrbNo=(ID-1);
        }
        public UProblem SDK_ToUProblem( string st, string name="", int difLvl=0, bool saveF=false ){
            List<UCell> B=_stringToBDL(st);
            if(B==null)  return null;
            var UP=new UProblem(999,B,name,difLvl);
            if(saveF) SDK_Save(UP);  
            return UP; 
        }          
        public List<UCell> _stringToBDL( string stOrg ){
            try{
                string st = stOrg.Replace(".", "0").Replace(" ", "");
                List<UCell> B = new List<UCell>();
                int rc=0;
                for( int k=0; rc<81; ){
                    if(st[k]=='+'){ k++; B.Add(new UCell(rc++,-(st[k++].ToInt()))); }
                    else{
                        while(!st[k].IsNumeric()) k++;
                        B.Add(new UCell(rc++,st[k++].ToInt()));
                    }
                }
                return B;
            }
            catch(Exception e){
                WriteLine("string error:"+e.Message+"\r"+e.StackTrace);
            }
            return null;
        }
                
        public string SetSolution( UProblem GP, bool SolSet2, bool SolAll=false ){
            string solMessage="";
            GNPX_Eng.pGP =GP;

            string prbMessage="";
            if( SolAll || GNPX_Eng.pGP.DifLevel<=0 || GNPX_Eng.pGP.Name=="" ){
                foreach( var p in GP.BDL )  if( p.No<0 ) p.No=0;

                GNPX_Eng.AnMan.Set_CellFreeB();
                GNPX_Eng.AnalyzerCounterReset();

                var tokSrc = new CancellationTokenSource();　        //for suspension
                GNPX_Eng.sudokAnalyzerAuto(tokSrc.Token);                      
                if( GNPZ_Engin.retCode<0 ){
                    GNPX_Eng.pGP.DifLevel = -999;
                    GNPX_Eng.pGP.Name = "unsolvable";
                }
                else{
                    int difficult = GNPX_Eng.GetDifficultyLevel( out prbMessage);
                    GNPX_Eng.pGP.DifLevel = difficult;
                    GNPX_Eng.pGP.Name = prbMessage;
                }
            }     
            solMessage = prbMessage;
            if(SolSet2) solMessage += GNPX_Eng.DGViewMethodCounterToString();　//適用手法を付加
            solMessage=solMessage.Trim();

            return solMessage;
        }

        public void SDK_FileOutput( string fName, bool append, bool fType81, bool SolSort, bool SolSet, bool SolSet2 ){
            if( SDKProbLst.Count==0 )  return;

            SDK_Ctrl.MltProblem = 1;
            SDK_Ctrl.lvlLow = 0;
            SDK_Ctrl.lvlHgh = 999;

            string LRecord, solMessage="";
            GNPXApp000.SlvMtdCList[0] = true;  //use all methods

            var tokSrc = new CancellationTokenSource();　        //for suspension

            int m=0;
            SDKProbLst.ForEach( p=>p.ID=(m++) );                 //▼▼▼ToDo Make problem management class
            IEnumerable<UProblem> qry;
            if(SolSort) qry = from p in SDKProbLst orderby p.DifLevel ascending select p;
            else qry = from p in SDKProbLst select p;

            using( StreamWriter fpW=new StreamWriter(fName,append,Encoding.UTF8) ){
                foreach( var P in qry ){

                    //===== Preparation =====
                    solMessage = "";
                    if(SolSet) solMessage = SetSolution(P,SolSet2);//output Solution
                    
                    if(fType81){　//Solution(tytpe:line)
                        LRecord = "";
                        P.BDL.ForEach( q =>{ LRecord += Max(q.No,0).ToString(); } );
                        LRecord=LRecord.Replace("0",".");

                        LRecord += " " + (P.ID+1)+ " \"" + P.Name+"\" " + P.DifLevel.ToString();
                        LRecord += " \"" + P.TimeStamp+ "\"";
                        if(SolSet&&SolSet2) LRecord += " \""+SetSolution(P,SolSet2:true,SolAll:true)+" \"";//解出力
                        fpW.WriteLine(LRecord);
                    }
                    else{ //problem_name and Solution(tytpe:matrix)
                        LRecord = (P.ID+1).ToString() + ", \"" + P.Name + "\"";
                        LRecord += ", " + P.DifLevel.ToString();
                        LRecord += ","+ solMessage;
                        fpW.WriteLine(LRecord);

                        for( int r=0; r<9; r++ ){
                            int n = P.BDL[r*9+0].No;
                            if( !SolSet && n<0 ) n=0;
                            LRecord = n.ToString();
                            for( int c=1; c<9; c++ ){
                                n = P.BDL[r*9+c].No;
                                if( !SolSet && n<0 ) n=0;
                                LRecord += ", " + n.ToString();
                            }
                            fpW.WriteLine(LRecord);
                        }
                    }
                }
            }
            GNPXApp000.SlvMtdCList[0] = false;             //restore method selection
        }
        public void btnFavfileOutput( bool fType81=true, bool SolSet=false, bool SolSet2=false ){
            string LRecord;
            string fNameFav = "SDK_Favorite.txt";

            var tokSrc = new CancellationTokenSource(); //procedures for suspension
            GNPXApp000.SlvMtdCList[0] = true;              //use all methods

            UProblem pGP=GNPX_Eng.pGP;
            GNPX_Eng.AnMan.Set_CellFreeB();
            GNPX_Eng.sudokAnalyzerAuto(tokSrc.Token);
            string prbMessage;
            int difLvl = GNPX_Eng.GetDifficultyLevel(out prbMessage);

            using( var fpW=new StreamWriter(fNameFav,true) ){
                if( fType81 ){
                    LRecord = "";
                    pGP.BDL.ForEach( q =>{ LRecord += Max(q.No,0).ToString(); } );
                    LRecord=LRecord.Replace("0",".");

                    LRecord += " " + (pGP.ID+1) + " \"" + pGP.Name+"\"";
                    if(SolSet){
                        LRecord += " " + pGP.DifLevel.ToString();
                        if(SolSet2) LRecord += " \""+SetSolution(pGP,SolSet2:true,SolAll:true)+" \"";//解を出力
                    }
                    LRecord += " \"" + pGP.TimeStamp+ "\"";
                }
                else{
                    LRecord = pGP.ID.ToString() + ", \"" + prbMessage + "\"";
                    LRecord += ", " + GNPX_Eng.pGP.DifLevel.ToString();
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
            GNPXApp000.SlvMtdCList[0] = false;//use selected methods
        }
        public void SDK_ProblemListSet( UProblem GPx ){
            GPx.ID=SDKProbLst.Count;
            SDKProbLst.Add(GPx);
            if( SDK_Ctrl.FilePut_GenPrb){
                string fName = "AutoGen" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                emergencyFilePut( "AutoGen", fName );
            }
            CurrentPrbNo = SDKProbLst.Count-1;
        }
        private  void emergencyFilePut( string dirStr, string fName ){
            if( !Directory.Exists(dirStr) ){ Directory.CreateDirectory(dirStr); }
            using( var fpW=new StreamWriter(dirStr+@"\"+fName,false) ){     
                foreach( UProblem P in SDKProbLst ){
                    string LRecord = "";
                    P.BDL.ForEach( q =>{ LRecord += Max(q.No,0).ToString(); } );
                    LRecord=LRecord.Replace("0",".");
                    LRecord += " " + (P.ID+1) + " \"" + P.Name+"\"";
                    LRecord += " " + P.DifLevel.ToString();
                    //LRecord += " \""+SetSolution(P,SolSet2:true,SolAll:true)+" \"";//解出力は省略
                    LRecord += " \"" + P.TimeStamp+ "\"";
                    fpW.WriteLine(LRecord);
                }
            }   
        }
    #endregion  file I/O

    #region SuDoKu Algorithm

        public List<UMthdChked> GetMethodListFromFile( ){
            if(SolverLst1==null) new List<UAlgMethod>();
            else SolverLst1.Clear();

            SolverLst1.AddRange(GNPX_Eng.AnMan.SolverLst0);
            SolverLst1.ForEach(P=>P.IsChecked=true);

            char[] sep=new char[]{' ',','};
   
            string st;
            int IDx=0;

            if( File.Exists(SDK_MethodsFileName) ){
                using( var fIn=new StreamReader(SDK_MethodsFileName) ){
                    while( (st=fIn.ReadLine()) !=null ){
                        bool bChk = true;

                        if( st[0]=='*' ){
                            var mLst= st.Split(sep,StringSplitOptions.RemoveEmptyEntries);
                            st=mLst[0].Substring(1);
                            GMthdOption[st] = mLst[1];
                        }
                        else{
                            if(st[0]=='-'){ bChk=false; st=st.Substring(1); }
                            UAlgMethod Q= SolverLst1.Find(x=>x.MethodName.Contains(st));
                            if(Q is UAlgMethod){ Q.ID=IDx++; Q.IsChecked=bChk; }
                        }
                    }
                }
            }
            SolverLst1.Sort( (p,q)=>(p.ID-q.ID) );
            SetMethodLis_1to2(FileOutput:false);
            return SolverLst2;
        }

        public List<UMthdChked> ResetMethodList(){
            int IDx=0;
            if(SolverLst1==null) new List<UAlgMethod>();
            else SolverLst1.Clear();

            SolverLst1.AddRange(GNPX_Eng.AnMan.SolverLst0);
            SolverLst1.ForEach(P=> { P.IsChecked=true; P.ID=IDx++; });

            SetMethodLis_1to2(true);
            return SolverLst2;
        }    

        public List<UMthdChked> SetMethodLis_1to2( bool FileOutput ){
            string po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            bool B=(po.ToInt()==1);
            UAlgMethod P=SolverLst1.Find(x=>x.MethodName.Contains("GeneralLogic"));
            if(P==null)  return null;
            P.IsChecked=B; P.IsEnabled=false;

            SolverLst1.Find(x=>x.MethodName.Contains("LastDigit")).IsEnabled=false;
            SolverLst1.Find(x=>x.MethodName.Contains("NakedSingle")).IsEnabled=false;
            SolverLst1.Find(x=>x.MethodName.Contains("HiddenSingle")).IsEnabled=false;

            SolverLst2 = SolverLst1.ConvertAll(Q=>new UMthdChked(Q));

            if(FileOutput)  MethodListOutPut();
            return SolverLst2;
        }      
        public List<UMthdChked> ChangeMethodList( int nx, int UD ){
            UAlgMethod MA=SolverLst1[nx], MB;
            if(UD<0){ MB=SolverLst1[nx-1]; SolverLst1[nx-1]=MA; SolverLst1[nx]=MB; }
            if(UD>0){ MB=SolverLst1[nx+1]; SolverLst1[nx+1]=MA; SolverLst1[nx]=MB; }
            SetMethodLis_1to2(FileOutput:true);
            return SolverLst2;
        }
        public void MethodListOutPut( ){
            using( var fOut=new StreamWriter(SDK_MethodsFileName) ){
                string po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
                bool B=(po.ToInt()==1);
                SolverLst1.Find(x=>x.MethodName.Contains("GeneralLogic")).IsChecked =B;
                SolverLst2.Find(x=>x.Name.Contains("GeneralLogic")).IsChecked =B;

                SolverLst1.ForEach(P=>{
                    string st=(P.IsChecked? "": "-")+P.MethodName.TrimStart( ' ');
                    fOut.WriteLine(st); });
                foreach( var P in GMthdOption ) fOut.WriteLine("*"+P.Key +" "+P.Value );
            }
        }
    #endregion SuDoKu Algorithm

    }

    public class UMthdChked{
        public int    ID{ get; set; }
        public string Name{ get; set; }
        public bool   IsChecked{
            get{ return UAM.IsChecked; }
            set{ UAM.IsChecked=value; }
        }
        public bool   IsEnabled{
            get{ return UAM.IsEnabled; }
            set{ UAM.IsEnabled=value; }
        }
        public UAlgMethod UAM;
        public int    __DifLevel;  //(difficulty)

        public UMthdChked(UAlgMethod P){
            this.UAM=P; this.ID=P.ID; Name=P.MethodName; IsChecked=P.IsChecked;
        }
    }
}