using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

using static System.Math;
using static System.Console;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

using Microsoft.Win32;

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

using GIDOOCV;

using GIDOO_space;

namespace GNPZ_sdk{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win{
    #region analysis
      //[Note] task,ProgressChanged,Completed,Canceled threadSafe（Prohibition of control operation）  
      #region analysis[Step] 
        private int  OnWork = 0;
        private bool ErrorStopB;
        private bool SolverBusy=false;

        private void btnSolve_Click( object sender, RoutedEventArgs e ){
            if( OnWork==2 ) return;
            if(SolverBusy)  return;

            GNP00.AnalyzerMode = "Solve";
            if( SDK_Ctrl.UGPMan==null ) SDK_Ctrl.UGPMan=new UPuzzleMan(1);
            else if(SDK_Ctrl.UGPMan.CreateNextStage()) return;  //solveup
           
            SolverBusy=true;
            SuDoKuSolver();
            MAnalizeBtnSet();
            SolverBusy=false;
        }

        private void SuDoKuSolver(){
            try{
                lblUnderAnalysis.Foreground = Brushes.LightGreen;
                Lbl_onAnalyzerM.Foreground  = Brushes.LightGreen;
                if( (string)btnSolve.Content!=pRes.msgSuspend){
                    int mc=GNP00.GNPX_Eng.Set_MethodLst_Run( );
                    if(mc<=0) GNP00.ResetMethodList();
                    lblUnderAnalysis.Visibility = Visibility.Visible;
                    Lbl_onAnalyzerM.Visibility = Visibility.Visible;

                    //GNPZ_Engin.SolInfoB = false;
                    if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC=0;

                    SDK_Ctrl.MltProblem = 1;    //single
                    SDK_Ctrl.lvlLow = 0;
                    SDK_Ctrl.lvlHgh = 999;
                    GNP00.SDKCntrl.CbxDspNumRandmize=false;
                    GNP00.SDKCntrl.GenLStyp = 1;

                    GNPXApp000.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
                    GNPZ_Engin.SolInfoB = true;
                    AnalyzerLap.Reset();

                    if(GNP00.AnalyzerMode=="Solve" || GNP00.AnalyzerMode=="MultiSolve"){
                        if(GNP00.pGP.SolCode<0)  GNP00.pGP.SolCode=0;
                        ErrorStopB = !_cellFixSub();

                        List<UCell> pBDL = pGP.BDL;
                        if(pBDL.Count(p=>p.No==0)==0){ //analysis completed
                            _SetScreenProblem();
                            goto AnalyzerEnd;
                        }

                        OnWork = 1;
                        txbStepCC.Text   = (++AnalyzerCC).ToString();
                        btnSolve.Content = pRes.msgSuspend;
                        lblUnderAnalysis.Content= pRes.lblUnderAnalysis;

                        txbStepMCC.Text  = txbStepCC.Text;
                        btnMultiSolve.Content= btnSolve.Content;
                        Lbl_onAnalyzerM.Content = lblUnderAnalysis.Content;

                        lblUnderAnalysis.Foreground=Brushes.Orange;
                        Lbl_onAnalyzerM.Foreground=Brushes.Orange;
                        Lbl_onAnalyzerTS.Content = "";
                        Lbl_onAnalyzerTSM.Content = "";
                        this.Cursor = Cursors.Wait;

                        if(!ErrorStopB){
                            __DispMode="";                
                            AnalyzerLap.Start();
                            //==============================================================
                            tokSrc = new CancellationTokenSource();　//for Cancellation 
                            taskSDK = new Task( ()=> GNP00.SDKCntrl.AnalyzerReal(tokSrc.Token), tokSrc.Token );
                            taskSDK.ContinueWith( t=> task_SDKsolver_Completed() ); //procedures used on completion
                            taskSDK.Start();
                        }
                        else{
                            __DispMode="Complated"; 
                        }

                        if(GNP00.AnalyzerMode!="MultiSolve") displayTimer.Start(); // <- To avoid unresolved operation trouble.
                    //  displayTimer.Start();
                        //--------------------------------------------------------------         
                    }
                    else{
                        try{
                            tokSrc.Cancel();
                            taskSDK.Wait();
                            btnSolve.Content=pRes.btnSolve;
                        }
                        catch(AggregateException e2){
                            WriteLine(e2.Message);
                            __DispMode="Canceled";
                        }
                    }
 
                AnalyzerEnd:
                    return;
                }
            }
            catch( Exception ex ){
                WriteLine( ex.Message );
                WriteLine( ex.StackTrace );
            }
        } 

        private void task_SDKsolver_ProgressChanged( object sender, SDKEventArgs e ){ _ProgressPer=e.ProgressPer; }
        private void task_SDKsolver_Completed(){
            __DispMode = "Complated";
            displayTimer.Start();
            taskSDK=null;
        }
      #endregion  analysis[Step] 
    
      #region analysis[All] 
        private void task_SDKsolverAuto_ProgressChanged( object sender, ProgressChangedEventArgs e ){
            lblNoOfTrials.Content = pRes.lblNoOfTrials + GNP00.SDKCntrl.LoopCC;
            txbBasicPattern.Text  = GNP00.SDKCntrl.PatternCC.ToString();
            btnSolveUp.Content = pRes.btnSolveUp;
            OnWork=0;
            SolverBusy=false;
        }
        private void task_SDKsolverAuto_Completed( ){ 
            __DispMode="Complated";
            displayTimer.Start();
        }

        private void btnSDKAnalyzerAuto_Click( object sender, RoutedEventArgs e ){
            if( OnWork==1 ) return;
            GNP00.AnalyzerMode = "SolveUp";
            GNP00.GNPX_Eng.MethodLst_Run.ForEach(P=>P.UsedCC=0);
            if( (string)btnSolveUp.Content==pRes.msgSuspend ){
                tokSrc.Cancel();
                try{ taskSDK.Wait(); }
                catch(AggregateException){ __DispMode="Canceled"; }
                displayTimer.Start();
                OnWork = 0;
            }
            else{
                List<UCell> pBDL = pGP.BDL;
                if( pBDL.Count(p=>p.No==0)==0 ){             //complate
                    _SetScreenProblem();
                    goto AnalyzerEnd;
                }
                if( pBDL.Any(p=>(p.No==0 && p.FreeB==0)) ){ //No Solution
                    lblAnalyzerResult.Text = pRes.msgNoSolution;
                    goto AnalyzerEnd;
                }

                OnWork = 2;
                btnSolveUp.Content = null;
                btnSolveUp.Content = pRes.msgSuspend;          
                lblUnderAnalysis.Content  = pRes.lblUnderAnalysis;
                lblUnderAnalysis.Foreground = Brushes.Orange;               

                int mc=GNP00.GNPX_Eng.Set_MethodLst_Run( );
                if( mc<=0 ) GNP00.ResetMethodList(); 
                
                _ResetAnalizer(true); //Clear Analysis Result
                GNP00.GNPX_Eng.AnalyzerCounterReset(); 

                GNPZ_Engin.SolInfoB = false;
                SDK_Ctrl.lvlLow = 0;
                SDK_Ctrl.lvlHgh = 999;

                //==============================================================
                tokSrc = new CancellationTokenSource();
                CancellationToken ct = tokSrc.Token;   
                taskSDK = new Task( ()=> GNP00.SDKCntrl.AnalyzerRealAuto(ct), ct );
                taskSDK.ContinueWith( t=> task_SDKsolverAuto_Completed() );
                AnalyzerLap.Reset(); 
                taskSDK.Start();
                //-------------------------------------------------------------- 
                this.Cursor = Cursors.Wait;

                AnalyzerLap.Start();
                __DispMode="";     
                
              AnalyzerEnd:
                displayTimer.Start();                
                return;
            }
        }
        private void btnAnalyzerResetAll_Click( object sender, RoutedEventArgs e ){
            Thickness X=PB_GBoard.Margin;   //◆
            PB_GBoard.Margin=new Thickness(X.Left+2,X.Top+2,X.Right,X.Bottom);
            _ResetAnalizer(true);
            bruMoveTimer.Start();
            UPP.Clear();
            GNP00.GNPX_Eng.MethodLst_Run.ForEach(P=>P.UsedCC=0);
            //q            SDK_Ctrl.UGPMan.MltUProbLst=new List<UPuzzle>();
            MAnalizeBtnSet();
        }
        private void _ResetAnalizer( bool AllF=true ){
            if(OnWork>0) return;
            AnalyzerCC = 0;

            txbStepCC.Text  = "0";
            txbStepMCC.Text = "0";
            btnSolveUp.Content  = pRes.btnSolveUp;
            lblAnalyzerResult.Text    = "";
            lblUnderAnalysis.Content  = "";         
            Lbl_onAnalyzerTS.Content  = "";

            Lbl_onAnalyzerM.Content   = "";
            Lbl_onAnalyzerTSM.Content = "";
            lblAnalyzerResultM.Text   = "";

            btnMultiSolve.IsEnabled   = true;
            UPP.Clear();
            LstBxMltAns.ItemsSource   = null;
//q            SDK_Ctrl.UGPMan.MltUProbLst=new List<UPuzzle>();
            txbEpapsedTimeTS3.Text    = "";

            GNP00.GNPX_Eng.AnMan.ResetAnalysisResult(AllF);
            GNP00.GNPX_Eng.AnalyzerCounterReset();
            SDK_Ctrl.UGPMan=null;           //Initialize Step/Multiple Solution search 

            displayTimer.Stop();
            _SetScreenProblem();
        }
      #endregion analysis[All] 

      #region MultiAnalysis        
        static public  int selXUPP;
        static public  List<UProbS> __UPPPPP=new List<UProbS>(); //__UPrbSLst
        static public  List<UProbS> UPP{ get{return __UPPPPP; } set{__UPPPPP=value; } } //__UPPPPP
        static public  List<UPuzzle> MltAnsLs;
        static public  int[,]       Sol99sta=new int[9,9];

        private void btnMultiSolve_Click( object sender, RoutedEventArgs e ){
#if !DEBUG
            int GL=GNPXApp000.GMthdOption["GeneralLogicOn"].ToInt();
            if(GL>0){
                shortMessage("GeneralLogic is unenable.", new sysWin.Point(750,60), Colors.Red,3000);
                return;
            }
#endif
            GNP00.AnalyzerMode = "MultiSolve";

            if( SDK_Ctrl.UGPMan==null ) SDK_Ctrl.UGPMan=new UPuzzleMan(1);
            else if(SDK_Ctrl.UGPMan.CreateNextStage())  return;  //solveup
            __UPPPPP.Clear();

            lblAnalyzerResultM.Text="";

            SDK_Ctrl.MltAnsOption["MaxLevel"]    = (int)MltAnsOpt0.Value;
            SDK_Ctrl.MltAnsOption["OneMethod"]   = (int)MltAnsOpt1.Value;
            SDK_Ctrl.MltAnsOption["AllMethod"]   = (int)MltAnsOpt2.Value;
            SDK_Ctrl.MltAnsOption["MaxTime"]     = (int)MltAnsOpt3.Value;
            SDK_Ctrl.MltAnsOption["StrtTime"]    = DateTime.Now;
            SDK_Ctrl.MltAnsOption["abortResult"]  = "";

            UPP.Clear();
            selXUPP=0;

            SuDoKuSolver();
            MAnalizeBtnSet();
        }

        public void  MultiSolved( object sender, SDKSolutionEventArgs e ){
            UPP.Add(e.UPB);
        }
        private void LstBxMltAns_SelectionChanged(object sender,SelectionChangedEventArgs e){
            try{
                if( SDK_Ctrl.UGPMan==null )   return;
                var Q=(UProbS)LstBxMltAns.SelectedItem;
                if(Q==null)  return;
                selXUPP=Q.IDmp1-1;
                if(selXUPP<0)  return;
                var U=UPP[selXUPP];
                lblAnalyzerResultM.Text= "["+(Q.IDmp1)+"] "+Q.Sol_ResultLong; 

                List<UPuzzle> pMltUProbLst=SDK_Ctrl.UGPMan.MltUProbLst;
                if(pMltUProbLst==null || pMltUProbLst.Count<=selXUPP)  return;
                UPuzzle pGPx=pMltUProbLst[selXUPP];
                SDK_Ctrl.UGPMan.pGPsel=pGPx;
                if( pGP.IDm!=selXUPP) SDK_Ctrl.UGPMan.GPMnxt=null;
                GNP00.GNPX_Eng.pGP = pGPx;
            }
            catch(Exception e2){ WriteLine($"{e2.Message}\r{e2.StackTrace}"); }
        }
    
        private void MAnalizeBtnSet(){
            if(SDK_Ctrl.UGPMan==null){
                btnMTop.IsEnabled=false;
                btnMPre.IsEnabled=false;
            }
            else{
                btnMTop.IsEnabled=true;
                btnMPre.IsEnabled=(SDK_Ctrl.UGPMan.GPMpre!=null);
            }
        }
        private void btnMPre_Click( object sender, RoutedEventArgs e ){
            LstBxMltAns.ItemsSource=null;
            if(SDK_Ctrl.UGPMan==null){ MAnalizeBtnSet(); return; }

            SDK_Ctrl.MovePre(); 
            if( SDK_Ctrl.UGPMan==null ){ _ResetAnalizer(true); return; }

            List<UPuzzle> pMltUProbLst=SDK_Ctrl.UGPMan.MltUProbLst;
            if( !GNPXApp000.chbConfirmMultipleCells || pMltUProbLst==null ) return;

            AnalyzerCC=SDK_Ctrl.UGPMan.stageNo;
            GNPZ_Engin.GNPX_AnalyzerMessage = SDK_Ctrl.UGPMan.pGPsel.Sol_ResultLong;
            txbStepCC.Text  = AnalyzerCC.ToString();
            txbStepMCC.Text = txbStepCC.Text;
            lblAnalyzerResult.Text= SDK_Ctrl.UGPMan.pGPsel.Sol_ResultLong;

            selXUPP=SDK_Ctrl.UGPMan.pGPsel.IDm;
            if(selXUPP<0)  return;
            UPP = pMltUProbLst.ConvertAll(P=>new UProbS(P));
            LstBxMltAns.ItemsSource=UPP;
            LstBxMltAns.SelectedIndex = selXUPP;

            if(selXUPP<UPP.Count){
                LstBxMltAns.ScrollIntoView(UPP[selXUPP]);
            }
            _Set_DGViewMethodCounter(); //Counter of applied algorithm
        }
        
        private void LstBxMltAns_MouseWheel(object sender,MouseWheelEventArgs e){ /*dummy*/ }
      #endregion MultiAnalysis    

      #region analysis[Method aggregation]
        public int  AnalyzerCC=0;
        private int AnalyzerCCMemo=0;
        private int AnalyzerMMemo=0;   
        private int[] eNChk;
        private bool _cellFixSub(  ){
            if( GNP00.pGP.SolCode<0) return false;
            bool retB=GNP00.GNPX_Eng.AnMan.FixOrEliminate_SuDoKu( ref eNChk );
            if( !retB && GNP00.GNPX_Eng.AnMan.SolCode==-9119 ){
                string st="";
                for(int h=0; h<27; h++ ){
                    if(eNChk[h]!=0x1FF){
                        st+= "Candidate #"+(eNChk[h]^0x1ff).ToBitStringNZ(9)+" disappeared in "+_ToHouseName(h)+"\r";
                        GNP00.GNPX_Eng.AnMan.SetBG_OnError(h);
                    }
                }

                lblAnalyzerResult.Text=st;
                GNP00.pGP.SolCode = GNP00.GNPX_Eng.AnMan.SolCode;
                return false;
            }

            if( GNP00.pGP.SolCode==-999 ){
                lblAnalyzerResult.Text = "Method control error";
                GNP00.pGP.SolCode = -1;
            }

            int nP=0, nZ=0, nM=0;
            __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
            if(nZ==0){ GNP00.GNPX_Eng.AnMan.SolCode=0; return true; }
            if(nM!=AnalyzerMMemo ){
                AnalyzerCCMemo = AnalyzerCC;
                AnalyzerMMemo  = nM;
            }

            if(nZ==0 && (bool)chbSetDifficulty.IsChecked){
                string prbMessage;
                int DifLevel = GNP00.GNPX_Eng.GetDifficultyLevel( out prbMessage );
                pGP.DifLevel = DifLevel;
                nUDDifficultyLevel.Text = DifLevel.ToString();
                lblCurrentnDifficultyLevel.Content = $"Difficulty: {GNP00.pGP.pMethod.DifLevel}"; //CurrentLevel
                if(lblAnalyzerResult.Text!="") lblCurrentnDifficultyLevel.Visibility=Visibility.Visible;
                else                           lblCurrentnDifficultyLevel.Visibility=Visibility.Hidden;
            }
            return true;
        }
        private string _ToHouseName( int h ){
            string st="";
            switch(h/9){
                case 0: st="row";    break;
                case 1: st="Column"; break;
                case 2: st="block";  break;
            }
            st += ((h%9)+1).ToString();
            return st;
        }
        private List<MethodCounter> MCList;
        private void _Set_DGViewMethodCounter(){
            MCList = new List<MethodCounter>();

            if(GNP00.AnalyzerMode!="SolveUp"){
                var Q=SDK_Ctrl.UGPMan;
                if(Q!=null){
                    if(Q.pGPsel!=null){
                        GNP00.GNPX_Eng.MethodLst_Run.ForEach(P=>P.UsedCC=0);
                        try{
                            while(Q!=null){ Q.pGPsel.pMethod.UsedCC++; Q =Q.GPMpre; }
                        }
                        catch(Exception e){ WriteLine(e.Message+"\r"+e.StackTrace); }
                    }
                }
            }
            foreach(var P in GNP00.GNPX_Eng.MethodLst_Run){
                if(P.UsedCC<=0)  continue;
                MCList.Add( new MethodCounter(P.MethodName,P.UsedCC) );
            }

            DGViewMethodCounter.ItemsSource = MCList;
            if(MCList.Count>0)  DGViewMethodCounter.SelectedIndex=-1;

            if(GNP00.GSmode=="tabASolve" && MCList.Count>0 && DGViewMethodCounter.Columns.Count>1){
                Style style = new Style(typeof(DataGridCell));
                style.Setters.Add(new Setter(DataGrid.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                DGViewMethodCounter.Columns[1].CellStyle = style;
            }
        }
        private class MethodCounter{
            public string methodName{ get; set; }
            public string count{ get; set; }
            public MethodCounter( string nm, int cc ){
                methodName = " "+nm;//.PadRight(30);
                count = cc.ToString()+" ";
            }
        }

      #endregion analysis[Method aggregation]
    #endregion analysis

    #region Method selection
        private void btnDefaultSettings_Click( object sender, RoutedEventArgs e ){
            int nx=GMethod00A.SelectedIndex;  //****
            GMethod00A.ItemsSource = null; //GSel*
            GMethod00A.ItemsSource = GNP00.ResetMethodList();
            GMethod00A.SelectedIndex = nx;    //****
            GNP00.MethodListOutPut();
            GNP00.GNPX_Eng.Set_MethodLst_Run(false);
            string po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            GeneralLogicOnChbx.IsChecked=(po.ToInt()==1);
        }

        private void GMethod01U_Click( object sender, RoutedEventArgs e ){
            int nx = GMethod00A.SelectedIndex;
            if( nx<=3 || nx==0 )  return;
            GMethod00A.ItemsSource = null; //GSel*
            GMethod00A.ItemsSource = GNP00.ChangeMethodList(nx,-1);
            GMethod00A.SelectedIndex = nx-1;
            GNP00.MethodListOutPut();
            GNP00.GNPX_Eng.Set_MethodLst_Run(false);
        }
        private void GMethod01D_Click( object sender, RoutedEventArgs e ){
            int nx = GMethod00A.SelectedIndex;
            if( nx<0 || nx==GMethod00A.Items.Count-1 )  return;
            GMethod00A.ItemsSource = null; //GSel*
            GMethod00A.ItemsSource = GNP00.ChangeMethodList(nx,1);
            GMethod00A.SelectedIndex = nx+1;
            GNP00.MethodListOutPut();
            GNP00.GNPX_Eng.Set_MethodLst_Run(false);
        }

        private void btnMethodCheck_Click(Object sender,RoutedEventArgs e){
            bool B=((Button)sender==btnMethodCheck);
            GNP00.SolverLst1.ForEach(P=>P.IsChecked=B);

            GNP00.SolverLst1.Find(x=>x.MethodName.Contains("LastDigit")).IsChecked=true;
            GNP00.SolverLst1.Find(x=>x.MethodName.Contains("NakedSingle")).IsChecked=true;
            GNP00.SolverLst1.Find(x=>x.MethodName.Contains("HiddenSingle")).IsChecked=true;

            UAlgMethod Q=GNP00.SolverLst1.Find(x=>x.MethodName.Contains("GeneralLogic"));
            string po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            Q.IsChecked=(po.ToInt()==1);

            _MethodSelectionMan();
        }

        private void ALSSizeMax_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if(ALSSizeMax==null)  return;
            GNPXApp000.GMthdOption["ALSSizeMax"] = ALSSizeMax.Value.ToString();
            _MethodSelectionMan();
        }

        private void NiceLoopMax_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if(NiceLoopMax==null)  return;
            GNPXApp000.GMthdOption["NiceLoopMax"] = NiceLoopMax.Value.ToString();
            _MethodSelectionMan();
        }

        private void GdNiceLoop_CGA_Checked(Object sender,RoutedEventArgs e){
            if(method_NLCell==null || method_NLGCells==null || method_NLALS==null)  return;
            GNPXApp000.GMthdOption["Cell"]           = ((bool)method_NLCell.IsChecked)? "1": "0";
            GNPXApp000.GMthdOption["GroupedCells"]   = ((bool)method_NLGCells.IsChecked)? "1": "0";
            GNPXApp000.GMthdOption["ALS"]            = ((bool)method_NLALS.IsChecked)? "1": "0";
            _MethodSelectionMan();
        }
   		private void ForceL0L1L2_Checked( object sender,RoutedEventArgs e ){
            if(ALSSizeMax==null)  return;
			GNPXApp000.GMthdOption["ForceLx"] = ((RadioButton)sender).Name;
            _MethodSelectionMan();
		}
   		private void ForceL0L1L2_copy_Checked( object sender,RoutedEventArgs e ){
            if(ALSSizeMax==null)  return;
			GNPXApp000.GMthdOption["ForceLx"] = (((RadioButton)sender).Name).Replace("_copy","");
            _MethodSelectionMan();
		}
        private void GeneralLogicOnChbx_Checked(Object sender,RoutedEventArgs e){
            if( GeneralLogicOnChbx==null )  return;
            int k=(bool)GeneralLogicOnChbx.IsChecked? 1: 0;
            GNPXApp000.GMthdOption["GeneralLogicOn"] = k.ToString();
            _MethodSelectionMan();
        }
        private void GenLogMaxSize_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if( GenLogMaxSize==null )  return;
            GNPXApp000.GMthdOption["GenLogMaxSize"] = GenLogMaxSize.Value.ToString();
            _MethodSelectionMan();
        }

        private void GenLogMaxRank_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if( GenLogMaxRank==null )  return;
            GNPXApp000.GMthdOption["GenLogMaxRank"] = GenLogMaxRank.Value.ToString();
            _MethodSelectionMan();
        }

        private void ForceChainCellHouse_Checked(object sender, RoutedEventArgs e){
            if(ForceChainCellHouse==null) return;
            int k = (bool)ForceChainCellHouse.IsChecked? 1 : 0;
            GNPXApp000.GMthdOption["ForceChainCellHouseOn"] = k.ToString();
            _MethodSelectionMan();
        }
        #endregion Method selection
    }
}