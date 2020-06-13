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

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

using GIDOOCV;
using GIDOO_space;

namespace GNPXcore{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win{
        private void randomSeed_TextChanged( object sender, TextChangedEventArgs e ){
            int rv=randomSeed.Text.ToInt();
            GNP00.SDKCntrl.randomSeedVal = rv;
            GNP00.SDKCntrl.SetRandomSeed(rv);
        }

    #region SuDoKu Pattern Setting
        private void btnPatternAutoGen_Click( object sender, RoutedEventArgs e){
            GNP00.SDKCntrl.CellNumMax = (int)CellNumMax.Value;
            _GeneratePatternl(true);
            SDK_Ctrl.rxCTRL = 0;     //Initialize Puzzle candidate generater
        }
        private void btnPatternClear_Click( object sender, RoutedEventArgs e ){
            GNP00.SDKCntrl.PatGen.GPat = new int[9,9];
            _SetBitmap_PB_pattern();  
        }     
        private void PB_pattern_MouseDown( object sender, MouseButtonEventArgs e ){
            _GeneratePatternl(false);
        }
        private void btnPatternCapture_Click( object sender, RoutedEventArgs e ){
            int nn=GNP00.SDKCntrl.PatGen.patternImport( pGP );
            labelPattern.Content = pRes.lblNoOfCells + nn;
            _SetBitmap_PB_pattern();
            SDK_Ctrl.rxCTRL = 0;     //Initialize Puzzle Generater
        }

        private void _GeneratePatternl( bool ModeAuto ){      
            int patSel = patSelLst.Find(p=>(bool)p.IsChecked).Name.Substring(6,2).ToInt(); //パターン形
            int nn=0;
            if(ModeAuto) nn=GNP00.SDKCntrl.PatGen.patternAutoMaker(patSel);
            else{
                sysWin.Point pt=Mouse.GetPosition(PB_pattern);
                int row=0, col=0;
                if(__GetRCPositionFromPattern(pt,ref row,ref col)){
                    nn=GNP00.SDKCntrl.PatGen.symmetryPattern(patSel,row,col,false);
                }
            }
            SDK_Ctrl.rxCTRL = 0;     //Initialize Puzzle Generater
            _SetBitmap_PB_pattern();
            labelPattern.Content = pRes.lblNoOfCells+nn;
        }    
        private bool __GetRCPositionFromPattern( sysWin.Point pt, ref int row, ref int col ){
            int selSizeHf = GNP00.cellSizeP/2 + 1;

            row=col=-1;
            int rn = (int)(pt.Y-GNP00.lineWidth);
            rn = rn-rn/(selSizeHf*3)*2;
            row = (rn/selSizeHf);

            int cn = (int)(pt.X-GNP00.lineWidth);
            cn = cn-cn/(selSizeHf*3)*2;
            col = cn/selSizeHf;

            if( row<0 || row>=9 || col<0 || col>=9 ) return false;
            return true;
        }
        private void _SetBitmap_PB_pattern( ){
            SDKGrp.GBPatternPaint( PB_pattern, GNP00.SDKCntrl.PatGen.GPat );
        }
    #endregion SuDoKu Pattern Setting

    #region Puzzle Creater[Manual]
        private void btnBoardClear_Click( object sender, RoutedEventArgs e ){
            for(int rc=0; rc<81; rc++ ){ pGP.BDL[rc] = new UCell(rc); }
            _SetScreenProblem();　      //Show free numbers
        }
        private void btnNewProblem_Click( object sender, RoutedEventArgs e ){
            if( pGP.BDL.All(P=>P.No==0) ) return;
            GNP00.SDK_Save_ifNotContain();
            GNP00.CreateNewPrb();       //reserve space for new problems
            _SetScreenProblem();　      //Show free numbers
        }
        private void btnDeleteProblem_Click( object sender, RoutedEventArgs e ){
            GNP00.SDK_Remove();
            _SetScreenProblem();　     //Show free numbers
        }
        private void btnCopyProblem_Click( object sender, RoutedEventArgs e ){
            UPuzzle UPcpy= pGP.Copy(0,0);
            UPcpy.Name="copy";
            GNP00.CreateNewPrb(UPcpy);//reserve space for new problems
            _SetScreenProblem();　    //Show free numbers
        }

        private void PB_GBoard_MouseEnter(object sender,MouseEventArgs e) {
            if(!tabManual.IsSelected) return;
                    //WriteLine($"Enter...{tabManual.IsSelected} ");
            PB_GBoard.MouseDown += new MouseButtonEventHandler(Create_PB_GBoard_MouseLeftButtonDown);
        }
        private void PB_GBoard_MouseLeave(object sender,MouseEventArgs e) {
                    //WriteLine($"Leave...{tabManual.IsSelected}");
            PB_GBoard.MouseDown -= new MouseButtonEventHandler(Create_PB_GBoard_MouseLeftButtonDown);
        }
        private void Create_PB_GBoard_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ){  
                   //WriteLine("PB_GBoard_MouseLeftButtonDown");
            int r=-1, c=-1;
            int noP=_Get_PB_GBoardRCNumSmall(ref r,ref c);
            rowMemo=r; colMemo=c; noPMemo=noP;
            _GNumericPadManager(r,c,noP); 
        }
        
      #region Mouse operation
        //***** Control variable
        private int     rowMemo; 
        private int     colMemo;
        private int     noPMemo;

        private void _GNumericPadManager( int r, int c, int noP ){              //Draw GNumericPad
            noPMemo = noP;
            GnumericPad.Source = SDKGrp.CreateCellImageLight( pGP.BDL[r*9+c], noP );
            int PosX = (int)PB_GBoard.Margin.Left +2 +37*c +(int)c/3;
            int PosY = (int)PB_GBoard.Margin.Top  +2 +37*r +(int)r/3;        
            GnumericPad.Margin = new Thickness(PosX, PosY, 0,0 );        
            GnumericPad.Visibility = Visibility.Visible;           
        }       
        private void GnumericPad_MouseMove( object sender, MouseEventArgs e ){  //Redraw GNumericPad when selection changes
                  //  Write("GnumericPad_MouseMove >> ");                      
            int r=-1, c=-1;
            int noP=_Get_PB_GBoardRCNumSmall(ref r, ref c);
            if(noP!=noPMemo){ noPMemo=noP; _GNumericPadManager(r,c,noP); }
                 //   WriteLine($"---4 GnumericPad_MouseMove noP:{noP} r{r} c{c}");
        }  
        private void GnumericPad_MouseLeftButtonUp(object sender, MouseButtonEventArgs e){
        //##### Operation is wrong when changing numbers with mouse moving selection?!
            int r=-1, c=-1;
            int noP=_Get_PB_GBoardRCNumSmall(ref r,ref c);
                 //   WriteLine($"GnumericPad_MouseLeftButtonUp noP:{noP} r{r} c{c}");

            if(noP<=0){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }
            if( r!=rowMemo || c!=colMemo ) return;

            UCell BDX = pGP.BDL[rowMemo*9+colMemo];
            if(Abs(BDX.No)==noP) BDX.No=0;
            else{
                BDX.No=0; 
                GNP00.GNPX_Eng.AnMan.Set_CellFreeB();
                if( ((BDX.FreeB>>(noP-1))&1)!=0) BDX.No=noP;
            }
          
            GNP00.GNPX_Eng.AnMan.Set_CellFreeB();
            _SetScreenProblem();
            GnumericPad.Visibility = Visibility.Hidden;
            rowMemo=-1; colMemo=-1;

            __Set_CellsPZMCount( );
        }
        private void GnumericPad_MouseLeave(object sender,MouseEventArgs e){
                    //WriteLine("GnumericPad_MouseLeave");        
            GnumericPad.Visibility = Visibility.Hidden;
            rowMemo=-1; colMemo=-1;
            return;
        }
        private int  _Get_PB_GBoardRCNumSmall(ref int boadRow,ref int boadCol ){
            int cSz=GNP00.cellSize, LWid=GNP00.lineWidth;

            boadRow = boadCol =-1;
            sysWin.Point pt = Mouse.GetPosition(PB_GBoard);
            int cn=(int)pt.X-2, rn=(int)pt.Y-2;

            cn = cn - cn/cSz - cn/(cSz*3+LWid)*LWid;
            boadCol = cn/cSz;
            cn=(cn%cSz)/12;
            if(cn<0 || cn>=3 || boadCol<0 || boadCol>=9){ boadRow=boadCol=-1; return -1; }
            
            rn = rn - rn/cSz - rn/(cSz*3+LWid)*LWid;
            boadRow = rn/cSz;
            rn=(rn%cSz)/12;
            if(rn<0 || rn>=3 || boadRow<0 || boadRow>=9){ boadRow=boadCol=-1; return -1; }
            return (rn*3+cn+1);
        }
      #endregion Mouse operation
    #endregion  Puzzle Creater[Manual]

    #region  Puzzle Creater[Auto]
        //Start
        private Task taskSDK;
        private CancellationTokenSource tokSrc;
        private void btnP13Start_Click( object sender, RoutedEventArgs e ){
            btnAnalyzerResetAll_Click(this,e);
          //SDK_Ctrl.UGPMan=null; //Terminate task. Included in the above processing 

#if !DEBUG
            int GL=GNPXApp000.GMthdOption["GeneralLogicOn"].ToInt();
            if(GL>0){
                shortMessage("GeneralLogic is unenable.", new sysWin.Point(450,150), Colors.Red,3000);
                return;
            }
#endif
            if( (string)btnCreateProblemMlt.Content==pRes.btnCreateProblemMlt ){
                __DispMode=null;
                GNP00.SDKCntrl.LoopCC = 0;
                btnCreateProblemMlt.Content = pRes.msgSuspend; // 

                GNPZ_Engin.SolInfoB = false;
                if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC = 0;
                GNP00.SDKCntrl.CbxDspNumRandmize = (bool)chbRandomizingNumbers.IsChecked;
                GNP00.SDKCntrl.GenLStyp = int.Parse(GenLStyp.Text);
                GNP00.SDKCntrl.CbxNextLSpattern  = (bool)chbChangeBasicPattenOnSuccess.IsChecked;
              //if((bool)chbCreateProblemEx2.IsChecked && (int)gamGen02.Value>5){ gamGen02.Value=5; }
                SDK_Ctrl.lvlLow = (int)gamGen01.Value;
                SDK_Ctrl.lvlHgh = (int)gamGen02.Value;
                SDK_Ctrl.FilePut_GenPrb = false;  //(bool)chbFileOutputOnSuccess.IsChecked;


                int n=gamGen05.Text.ToInt();
                n = Max(Min(n,100000000),0); 
                SDK_Ctrl.MltProblem = _ProgressPer = n;
                SDK_Ctrl.GenLS_turbo = (bool)GenLS_turbo.IsChecked;

                displayTimer.Start();
                AnalyzerLap.Start();

            //============================================================================================== 20181031
                AnalyzerBaseV2.__SimpleAnalizerB__ = true;//(bool)chbCreateProblemEx2.IsChecked;
                tokSrc  = new CancellationTokenSource();　//procedures for suspension 
                taskSDK = new Task( ()=> GNP00.SDKCntrl.SDK_ProblemMakerReal(tokSrc.Token), tokSrc.Token );
                taskSDK.ContinueWith( t=> btnP13Start2Complated() ); //Completion process
                taskSDK.Start();
            //---------------------------------------------------------------------------------------------- 20181031
            }
            else{   //"Suspend"
                try{
                    AnalyzerBaseV2.__SimpleAnalizerB__=false;
                    chbCreateProblemEx2.IsEnabled=true;
                    shortMessage("cancellation accepted",new sysWin.Point(120,188),Colors.Red,2000);
                    tokSrc.Cancel();

                    GNP00.CurrentPrbNo=999999999;
                    _SetScreenProblem( );
                    btnCreateProblemMlt.Content = pRes.btnCreateProblemMlt;
                }
                catch(AggregateException){ __DispMode="Canceled"; }
            }
            return;
        }
        private void chbCreateProblemEx2_Checked(object sender,RoutedEventArgs e){
            if(chbChangeBasicPattenOnSuccess==null) return;
        //  chbChangeBasicPattenOnSuccess.IsChecked=false;
        //  chbChangeBasicPattenOnSuccess.IsEnabled=false;
        //  if((int)gamGen02.Value>5) gamGen02.Value=5;
        }
        private void chbCreateProblemEx2_Unchecked(object sender,RoutedEventArgs e){
        //  chbChangeBasicPattenOnSuccess.IsEnabled=true;
            bmpPD.Clear();
            PB_BasePatDig.Source=bmpPD; 
        }

        //Progress display
        public void BWGenPrb_ProgressChanged( object sender, SDKEventArgs e ){ _ProgressPer=e.ProgressPer; }
        //Done
        private void btnP13Start2Complated( ){
            __DispMode="Complated"; 
            AnalyzerBaseV2.__SimpleAnalizerB__=false;
            this.Dispatcher.Invoke(() => { chbCreateProblemEx2.IsEnabled=true; } );
            taskSDK=null;
        }

        private void gamGen01_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if( gamGen02==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Lval>Uval ) gamGen02.Value=Lval;
        }
        private void gamGen02_NumUDValueChanged(Object sender,GIDOOEventArgs args){
            if( gamGen01==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Uval<Lval ) gamGen01.Value=Uval;
        }

        private void btnESnxtSucc_Click( Object sender,RoutedEventArgs e ){
            int RX=(int)UP_ESRow.Value;
            GNP00.SDKCntrl.Force_NextSuccessor(RX);
            _Display_GB_GBoard();
        }
    #endregion  Puzzle Creater[Auto]
        
    #region Selecte problem
        private void btnProbPre_Click(object sender,RoutedEventArgs e){ _Get_PreNxtPrg(-1); }
        private void btnProbNxt_Click(object sender,RoutedEventArgs e){ _Get_PreNxtPrg(+1); }
        private void _Get_PreNxtPrg( int pm ){ //498
            int nn=GNP00.CurrentPrbNo +pm;
            if(nn<0 || nn>GNP00.SDKProbLst.Count-1) return;

            GNP00.CurrentPrbNo = nn;
            GNP00.GNPX_Eng.AnMan.ResetAnalysisResult(false); //Clear analysis result only
            GNP00.GNPX_Eng.AnalyzerCounterReset();
            SDK_Ctrl.UGPMan=null;                            //MultiSolver Initialize
            _SetScreenProblem();
        
            AnalyzerCC = 0;
            lblAnalyzerResult.Text = "";

            Lbl_onAnalyzerTS.Content  = "";
            Lbl_onAnalyzerTSM.Content = "";

            txbStepCC.Text  = "0";
            txbStepMCC.Text = "0";

            lblAnalyzerResultM.Text = "";
            LstBxMltAns.ItemsSource=null;
        }

        private UPuzzle _SetScreenProblem( ){
            UPuzzle P = GNP00.GetCurrentProble( );
            _Display_GB_GBoard();
            if(P!=null){           
                txtProbNo.Text = (P.ID+1).ToString();
                txtProbName.Text = P.Name;
                nUDDifficultyLevel.Text = P.DifLevel.ToString();
                if(GNP00.pGP.pMethod!=null){
                    int DiffL=(GNP00.pGP.pMethod==null)? 0: GNP00.pGP.pMethod.DifLevel;
                    lblCurrentnDifficultyLevel.Content = $"Difficulty: {DiffL}"; //CurrentLevel
                    if(lblAnalyzerResult.Text!="") lblCurrentnDifficultyLevel.Visibility=Visibility.Visible;
                    else                           lblCurrentnDifficultyLevel.Visibility=Visibility.Hidden;
                }
                btnProbPre.IsEnabled = (P.ID>0);
                btnProbNxt.IsEnabled = (P.ID<GNP00.SDKProbLst.Count-1);
                       
                _Set_DGViewMethodCounter();
            }
            return P;
        }
        private void txtProbName_TextChanged(Object sender,TextChangedEventArgs e){
            if(txtProbName.IsFocused) pGP.Name=txtProbName.Text;
        }                     
    #endregion Selection problem
    }
}