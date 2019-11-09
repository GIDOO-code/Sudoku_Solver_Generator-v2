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

using GIDOO_space;

//http://msdn.microsoft.com/en-us/library/ms750559.aspx

//WPF
//http://www.atmarkit.co.jp/fdotnet/chushin/introwpf_index/

//Obfuscation
//http://d.hatena.ne.jp/wwwcfe/20100513/obfuscator

//Routing event
//http://msdn.microsoft.com/ja-jp/library/ms742550.aspx

//Internationalization of applications
//http://grabacr.net/archives/1647
//http://yujiro15.net/YKSoftware/download/150602_Multilingual.pdf

namespace GNPZ_sdk{
    using pRes=Properties.Resources;

    public partial class NuPz_Win: Window{
        private Point           _WinPosMemo;
        public  GNPXApp000      GNP00;
        public  GNPZ_Graphics   SDKGrp; //board surface display bitmap
        public  CultureInfo     culture{ get{ return pRes.Culture; } }

        private int             WOpacityCC=0;
        private Stopwatch       AnalyzerLap;
        private DispatcherTimer startingTimer;
        private DispatcherTimer endingTimer;
        private DispatcherTimer displayTimer;   
        private DispatcherTimer bruMoveTimer;
        private RenderTargetBitmap bmpGZero;
        private DevelopWin      devWin;
		private ExtendResultWin ExtResultWin;

        private UProblem        pGP{ get{ return GNP00.GNPX_Eng.pGP; } }

        //=============
        private List<RadioButton> patSelLst;
/*
        private System.Drawing.Printing.PrintDocument SDKprintDocument;
        private System.Windows.Forms.PageSetupDialog SDKpageSetupDialog;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog1;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.PrintPreviewDialog SDKprintPreviewDialog;
*/
        public SDKAlarm SDKAlarmObj = new SDKAlarm();

        //WPF CHeckedListBox
        //http://www.codeproject.com/KB/WPF/WPFProblemSolving.
        //http://www.codeproject.com/KB/WPF/WPFProblemSolving.aspx
        //*************

        private DispatcherTimer     timerShortMessage;

        string CopyrightJP, CopyrightEN;
        private void MultiLangage_JP_Click(Object sender,RoutedEventArgs e) {
            ResourceService.Current.ChangeCulture("ja-JP");
            txtCopyrightDisclaimer.Text = CopyrightJP;
        }
        private void btnMultiLangage_Click(object sender, RoutedEventArgs e){
            ResourceService.Current.ChangeCulture("en");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            txtCopyrightDisclaimer.Text = CopyrightEN;
        }
        private void cmbLanguageLst_SelectionChanged(Object sender,SelectionChangedEventArgs e){
            string lng=(string)cmbLanguageLst.SelectedValue;
            ResourceService.Current.ChangeCulture(lng);
        }
        public object Culture{ get{ return pRes.Culture; } }

        private void GNPXGNPX_MouseDoubleClick( object sender, MouseButtonEventArgs e ){
            if(devWin==null) devWin=new DevelopWin(this);
            devWin.Show();
            devWin.Set_dev_GBoard(GNP00.pGP.BDL);
        }

    #region TimerEvent
    //http://msdn.microsoft.com/ja-jp/library/system.eventargs.aspx

        public class SDKAlarm{
	        public delegate void FireEventHandler(object sender, SDKEventArgs fe );
	        public event FireEventHandler FireEvent;	
	        public void  ActivateSDKAlarm( string eName, int eCode ){
		        SDKEventArgs SDKArgs = new SDKEventArgs(eName, eCode);
		        FireEvent( this, SDKArgs ); 
	        }
        }
     
        private void appExit_Click( object sender, RoutedEventArgs e ){
            GNP00.MethodListOutPut();

            WOpacityCC=0;
            endingTimer.IsEnabled = true;
            endingTimer.Start();
        }

        private void startingTimer_Tick( object sender, EventArgs e){
            WOpacityCC++;
            if( WOpacityCC >= 25 ){ this.Opacity=1.0; startingTimer.Stop(); }
            else this.Opacity=WOpacityCC/25.0;
        }
        private void endingTimer_Tick( object sender, EventArgs e){
            if( (++WOpacityCC)>10 )  Environment.Exit(0);   //Application.Exit();
            double dt = 1.0-WOpacityCC/12.0;
            this.Opacity = dt*dt;
        }
       
        private void bruMoveTimer_Tick( object sender, EventArgs e){
            Thickness X=PB_GBoard.Margin;
            PB_GBoard.Margin=new Thickness(X.Left-2,X.Top-2,X.Right,X.Bottom);
            bruMoveTimer.Stop();
        }
        
    #endregion TimerEvent

    #region Application start/end
        public NuPz_Win(){
            try{
                GNP00  = new GNPXApp000(this);
                SDKGrp = new GNPZ_Graphics(GNP00);
           
                devWin = new DevelopWin(this);
			    GroupedLinkGen.devWin = devWin;

                InitializeComponent();
                cmbLanguageLst.ItemsSource = GNP00.LanguageLst;  

                GNPX_AnalyzerMan.Send_Solved += MultiSolved;

              //this.MouseLeftButtonDown += (sender, e) => this.DragMove();

                GNPXGNPX.Content = "GNPX "+DateTime.Now.Year;
           
              //RadioButton Controls Collection
                patSelLst = GNPZExtender.GetControlsCollection<RadioButton>(this);
                patSelLst = patSelLst.FindAll(p=>p.Name.Contains("patSel"));

              #region Timer
                AnalyzerLap = new Stopwatch();

                timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
                timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
                timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);

                startingTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
                startingTimer.Interval = TimeSpan.FromMilliseconds(70);
                startingTimer.Tick += new EventHandler(startingTimer_Tick);
                this.Opacity=0.0;
                startingTimer.Start();

                endingTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
                endingTimer.Interval = TimeSpan.FromMilliseconds(70);
                endingTimer.Tick += new EventHandler(endingTimer_Tick);

                displayTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
                displayTimer.Interval = TimeSpan.FromMilliseconds(100);//50
                displayTimer.Tick += new EventHandler(displayTimer_Tick);

                bruMoveTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
                bruMoveTimer.Interval = TimeSpan.FromMilliseconds(20);
                bruMoveTimer.Tick += new EventHandler(bruMoveTimer_Tick);
              #endregion Timer

                bmpGZero = new RenderTargetBitmap((int)PB_GBoard.Width,(int)PB_GBoard.Height, 96,96, PixelFormats.Default);
                SDKGrp.GBoardPaint( bmpGZero, (new UProblem()).BDL, "tabACreate" );
                PB_GBoard.Source = bmpGZero;

              #region Copyright
                string endl = "\r";
                string st = "【著作権】" + endl;
                st += "本ソフトウエアと付属文書に関する著作権は、作者GNPX に帰属します。" + endl;
                st += "本ソフトウエアは著作権法及び国際著作権条約により保護されています。" + endl;
                st += "使用ユーザは本ソフトウエアに付された権利表示を除去、改変してはいけません" + endl + endl;

                st += "【配布】" + endl;
                st += "インターネット上での二次配布、紹介等は事前の承諾なしで行ってかまいません。";
                st += "バージョンアップした場合等には、情報の更新をお願いします。" + endl;
                st += "雑誌・書籍等に収録・頒布する場合には、事前に作者の承諾が必要です。" + endl + endl;
                   
                st += "【禁止事項】" + endl;
                st += "以下のことは禁止します。" + endl;
                st += "・オリジナル以外の形で、他の人に配布すること" + endl;
                st += "・第三者に対して本ソフトウエアを販売すること" + endl;
                st += "・販売を目的とした宣伝・営業・複製を行うこと" + endl;
                st += "・第三者に対して本ソフトウエアの使用権を譲渡・再承諾すること" + endl;
                st += "・本ソフトウエアに対してリバースエンジニアリングを行うこと" + endl;
                st += "・本承諾書、付属文書、本ソフトウエアの一部または全部を改変・除去すること" + endl + endl;

                st += "【免責事項】" + endl;
                st += "作者は、本ソフトウエアの使用または使用不能から生じるコンピュータの故障、情報の喪失、";
                st += "その他あらゆる直接的及び間接的被害に対して一切の責任を負いません。" + endl;
                CopyrightJP=st;

                st="===== CopyrightDisclaimer =====" + endl;
                st += "Copyright" + endl;
                st += "The copyright on this software and attached document belongs to the author GNPX" + endl;
                st += "This software is protected by copyright law and international copyright treaty." + endl;
                st += "Users should not remove or alter the rights indication attached to this software." + endl + endl;

                st += "distribution" + endl;
                st += "Secondary distribution on the Internet, introduction etc. can be done without prior consent.";
                st += "Please update the information when upgrading etc etc." + endl;
                st += "In the case of recording / distributing in magazines · books, etc., consent of the author is necessary beforehand." + endl + endl;
                   
                st += "Prohibited matter" + endl;
                st += "The following things are forbidden." + endl;
                st += "Distribute it to other people in forms other than the original." + endl;
                st += "Selling this software to a third party." + endl;
                st += "Promotion, sales and reproduction for sale." + endl;
                st += "Transfer and re-accept the right to use this software to a third party." + endl;
                st += "Modification / removal of this consent form and attached document" + endl + endl;

                st += "Disclaimer" + endl;
                st += "The author assumes no responsibility for damage to computers, loss of information or any other direct or indirect damage resulting from the use or inability of the Software." + endl;
                CopyrightEN=st;
                txtCopyrightDisclaimer.Text = CopyrightEN;
              #endregion Copyright

                tabCtrlMode.Focus();
                PB_GBoard.Focus();

//q                MltAnsLst = SDK_Ctrl.UGPMan.MltUProbLst;
            }
            catch(Exception e){
                WriteLine(e.Message+"\r"+e.StackTrace);
            }
        }
        private void Window_Loaded( object sender, RoutedEventArgs e ){
            _Display_GB_GBoard( );       //board setting
            _SetBitmap_PB_pattern();     //Pattern setting

            lblUnderAnalysis.Content   = "";
            Lbl_onAnalyzerM.Content  = "";
            Lbl_onAnalyzerTS.Content = "";
            Lbl_onAnalyzerTSM.Content = "";
            
            //===== solution list setting =====           
            GMethod00A.ItemsSource = GNP00.GetMethodListFromFile();
            NiceLoopMax.Value = GNPXApp000.GMthdOption["NiceLoopMax"].ToInt();
            ALSSizeMax.Value  = GNPXApp000.GMthdOption["ALSSizeMax"].ToInt();
            method_NLCell.IsChecked   = (GNPXApp000.GMthdOption["Cell"]!="0");
            method_NLGCells.IsChecked = (GNPXApp000.GMthdOption["GroupedCells"]!="0");
            method_NLALS.IsChecked    = (GNPXApp000.GMthdOption["ALS"]!="0");

			string po=(string)GNPXApp000.GMthdOption["ForceLx"];
			switch(po){
				case "ForceL0": ForceL0.IsChecked=true; break;
				case "ForceL1": ForceL1.IsChecked=true; break;
				case "ForceL2": ForceL2.IsChecked=true; break;
			}

            po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            GeneralLogicOnChbx.IsChecked=(po.ToInt()==1);

            po =(string)GNPXApp000.GMthdOption["GenLogMaxSize"];
            int GLMaxSize=po.ToInt();
            if( GLMaxSize>0 && GLMaxSize<=(int)GenLogMaxSize.MaxValue ) GenLogMaxSize.Value=GLMaxSize;

            po=(string)GNPXApp000.GMthdOption["GenLogMaxRank"];
            int GLMaxRank=po.ToInt();
            if( GLMaxRank>0 && GLMaxRank<=(int)GenLogMaxRank.MaxValue ) GenLogMaxRank.Value=GLMaxRank;

            WOpacityCC =0;
            startingTimer.Start();

            _WinPosMemo = new Point(this.Left,this.Top+this.Height);
        }
        private void Window_Unloaded( object sender, RoutedEventArgs e ){
            Environment.Exit(0);
        }
    #endregion Application start/end 

    #region Location
		private void GNPXwin_LocationChanged( object sender,EventArgs e ){ //Synchronously move open window
            foreach( Window w in Application.Current.Windows ){
                if( w.Owner!=this && w!=this ){ w.Topmost=true; __GNPXwin_LocationChanged(w); }
            }
            _WinPosMemo = new Point(this.Left,this.Top);
		}
		private void __GNPXwin_LocationChanged( Window _win ){
            if( _win==null )  return;
            _win.Left = this.Left-_WinPosMemo.X+_win.Left;
            _win.Top  = this.Top -_WinPosMemo.Y+_win.Top ;
        }	
    #endregion Location

        private void Window_MouseDown( object sender, MouseButtonEventArgs e ){
            if( e.Inner(PB_GBoard) )    return; 
            if( e.Inner(tabCtrlMode) )  return;
            this.DragMove();
        }
        private void btnHomePage_Click( object sender, RoutedEventArgs e ){
            string cul=Thread.CurrentThread.CurrentCulture.Name;
            Console.WriteLine("The current culture is {0}", cul);
            if(cul=="ja-JP") Process.Start("http://csdenp.web.fc2.com");
            else             Process.Start("http://csdenpe.web.fc2.com"); 
        }               
    #region ShortMessage
        public void shortMessage(string st, Point pt, Color cr, int tm ){
            LbShortMes.Content = st;
            LbShortMes.Foreground = new SolidColorBrush(cr);

            if( tm==9999 ) timerShortMessage.Interval = TimeSpan.FromSeconds(5);
            else           timerShortMessage.Interval = TimeSpan.FromMilliseconds(tm);            
            timerShortMessage.Start();
            LbShortMes.Visibility = Visibility.Visible;
        }
        private void timerShortMessage_Tick( object sender, EventArgs e ){
            LbShortMes.Visibility = Visibility.Hidden;
            timerShortMessage.Stop();
        }
    #endregion ShortMessage

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

        private void GeneralLogicOnChbx_Checked(Object sender,RoutedEventArgs e){
            if( GeneralLogicOnChbx==null )  return;
            int k=(bool)GeneralLogicOnChbx.IsChecked? 1: 0;
            GNPXApp000.GMthdOption["GeneralLogicOn"] = k.ToString();
            _MethodSelectionMan();
        }
        private void GenLogMaxSize_NumUDValueChanged(Object sender,GIDOOEventArgs args) {
            if( GenLogMaxSize==null )  return;
            GNPXApp000.GMthdOption["GenLogMaxSize"] = GenLogMaxSize.Value.ToString();
            _MethodSelectionMan();
        }

        private void GenLogMaxRank_NumUDValueChanged(Object sender,GIDOOEventArgs args) {
            if( GenLogMaxRank==null )  return;
            GNPXApp000.GMthdOption["GenLogMaxRank"] = GenLogMaxRank.Value.ToString();
            _MethodSelectionMan();
        }
    #endregion Method selection
 
    #region File I/O, keyDown 
        private string    fNameSDK;
        private void btnOpenPuzzleFile_Click( object sender, RoutedEventArgs e ){
            var OpenFDlog = new OpenFileDialog();
            OpenFDlog.Multiselect = false;

            OpenFDlog.Title  = pRes.filePuzzleFile;
            OpenFDlog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if( (bool)OpenFDlog.ShowDialog() ){
                fNameSDK = OpenFDlog.FileName;
                GNP00.SDK_FileInput( fNameSDK, (bool)chbInitialState.IsChecked );
                txtFileName.Text = fNameSDK;

                _SetScreenProblem();
                GNP00._SDK_Ctrl_Initialize();

                btnProbPre.IsEnabled = (GNP00.CurrentPrbNo>=1);
                btnProbNxt.IsEnabled = (GNP00.CurrentPrbNo<GNP00.SDKProbLst.Count-1);
            }
        }
        private void btnSavePuzzle_Click( object sender, RoutedEventArgs e ){
            var SaveFDlog = new SaveFileDialog();
            SaveFDlog.Title  =  pRes.filePuzzleFile;
            SaveFDlog.FileName = fNameSDK;
            SaveFDlog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            
            GNPXApp000.SlvMtdCList[0] = true;
            if( !(bool)SaveFDlog.ShowDialog() ) return;
            fNameSDK = SaveFDlog.FileName;
            bool append  = (bool)chbAdditionalSave.IsChecked;
            bool fType81 = (bool)chbFile81Nocsv.IsChecked;
            bool SolSort = (bool)chbSolutionSort.IsChecked;
            bool SolSet  = (bool)cbxProbSolSetOutput.IsChecked;
            bool SolSet2 = (bool)chbAddAlgorithmList.IsChecked;

            if( GNP00.SDKProbLst.Count==0 ){
                if( pGP.BDL.All(p=>p.No==0) ) return;
                pGP.ID = GNP00.SDKProbLst.Count;
                GNP00.SDKProbLst.Add(pGP);
                GNP00.CurrentPrbNo=0;
                _SetScreenProblem();
            }
            GNP00.GNPX_Eng.Set_MethodLst_Run(true);  //true:All Method 
            GNP00.SDK_FileOutput( fNameSDK, append, fType81, SolSort, SolSet, SolSet2 );
        }
        private void btnSaveToFavorites_Click( object sender, RoutedEventArgs e ){
            GNP00.btnFavfileOutput(true,SolSet:true,SolSet2:true);
        }
        private void cbxProbSolSetOutput_Checked( object sender, RoutedEventArgs e ){
            chbAddAlgorithmList.IsEnabled = (bool)cbxProbSolSetOutput.IsChecked;
            Color cr = chbAddAlgorithmList.IsEnabled? Colors.White: Colors.Gray;
            chbAddAlgorithmList.Foreground = new SolidColorBrush(cr); 
        }

       //Copy/Paste Puzzle(board<-->clipboard)
        private void Grid_PreviewKeyDown( object sender, KeyEventArgs e ){
            bool KeySft  = (Keyboard.Modifiers&ModifierKeys.Shift)>0;
            bool KeyCtrl = (Keyboard.Modifiers&ModifierKeys.Control)>0;

            if( e.Key==Key.C && KeyCtrl ){
                string st=pGP.CopyToBuffer();
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }
            else if( e.Key==Key.F && KeyCtrl ){
                string st=pGP.ToGridString(KeySft);   
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }
            else if( e.Key==Key.V && KeyCtrl ){
                string st=(string)Clipboard.GetData(DataFormats.Text);
                Clipboard.Clear();
                if( st==null || st.Length<81 ) return ;
                var UP=GNP00.SDK_ToUProblem(st,saveF:true); 
                if( UP==null) return;
                GNP00.CurrentPrbNo=999999999;//GNP00.SDKProbLst.Count-1
                _SetScreenProblem();
                _ResetAnalizer(false); //Clear analysis result

            }

        }
    #endregion File I/O, keyDown 
        
    #region　operation mode
        private void tabCtrlMode_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            if( (TabControl)sender!=tabCtrlMode ) return;
            TabItem tb=tabCtrlMode.SelectedItem as TabItem;
            if( tb.Name.Substring(0,4)!="tabA" )  return;
            GNP00.GSmode = (string)tb.Name;    //Tab Name -> State mode

            switch(GNP00.GSmode){
                case "tabASolve": sNoAssist = (bool)chbDisplayCandidate.IsChecked; break;
                case "tabACreate":
                    TabItem tb2=tabAutoManual.SelectedItem as TabItem;
                    if( tb2==null )  return ;
                    if( (string)tb2.Name=="tabBAuto" )  sNoAssist=false;
                    else sNoAssist = (bool)chbShowNousedDigits.IsChecked;
                    gridExhaustiveSearch.Visibility=
                        (int.Parse(GenLStyp.Text)==2)? Visibility.Visible: Visibility.Hidden;
                    break;

                case "tabAOption":
                    bool sAssist=true;
                    chbSetAnswer.IsChecked=sAssist;
                    sNoAssist=sAssist;
                    break;

                default: sNoAssist=false; break;
            }

            _Display_GB_GBoard();   
            //tabSolver_SelectionChanged(sender,e);
        }      
        private void _MethodSelectionMan(){
            GMethod00A.ForceCursor=true;          

            if(GNP00!=null){
                GMethod00A.ItemsSource = null; //GSel*
                GMethod00A.ItemsSource = GNP00.SetMethodLis_1to2(true);
            }

            string po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            bool B=(po.ToInt()==1);
            LblGeneralLogic.Content = "GeneralLogic "+ (B? "有効":"無効");
        }
        private void tabSolver_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            TabItem tabItm = tabSolver.SelectedItem as TabItem;
            if( tabItm!=null ) SDK_Ctrl.MltAnsSearch = (tabItm.Name=="tabBMultiSolve");  
        }          
    #endregion operation mode

    #region Selection problem
        private void btnProbPre_Click( object sender, RoutedEventArgs e ){ _Get_PreNxtPrg(-1); }
        private void btnProbNxt_Click( object sender, RoutedEventArgs e ){ _Get_PreNxtPrg(+1); }
        private void _Get_PreNxtPrg( int pm ){ //498
            int nn=GNP00.CurrentPrbNo +pm;
            if( nn<0 || nn>GNP00.SDKProbLst.Count-1 ) return;

            GNP00.CurrentPrbNo = nn;
            GNP00.GNPX_Eng.AnMan.ResetAnalysisResult(false); //Clear analysis result only
            GNP00.GNPX_Eng.AnalyzerCounterReset();
            SDK_Ctrl.UGPMan=null;                            //MultiSolver Initialize
            _SetScreenProblem();
        
            AnalyzerCC = 0;
            lblAnalyzerResult.Text = "";

            Lbl_onAnalyzerTS.Content = "";
            Lbl_onAnalyzerTSM.Content = "";

            txbStepCC.Text  = "0";
            txbStepMCC.Text = "0";

            lblAnalyzerResultM.Text = "";
            LstBxMltAns.ItemsSource=null;
        }

        private void _SetScreenProblem( ){
            UProblem P = GNP00.GetCurrentProble( );
            _Display_GB_GBoard();
            if( P!=null ){
                txtProbNo.Text = (P.ID+1).ToString();
                txtProbName.Text = P.Name;
                nUDDifficultyLevel.Text = P.DifLevel.ToString();
            
                int nP=0, nZ=0, nM=0, nn=P.ID;
                __Set_CellsPZMCount( ref nP, ref nZ, ref nM );

                btnProbPre.IsEnabled = (nn>0);
                btnProbNxt.IsEnabled = (nn<GNP00.SDKProbLst.Count-1);
                       
                _Set_DGViewMethodCounter();
            }
        }
        private void txtProbName_TextChanged(Object sender,TextChangedEventArgs e) {
            if(txtProbName.IsFocused) pGP.Name=txtProbName.Text;
        }                     


    #endregion Selection problem

    #region Sudoku Pattern Setting
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
            SDK_Ctrl.rxCTRL = 0;     //Initialize Puzzle candidate generater
        }

        private void _GeneratePatternl( bool ModeAuto ){      
            int patSel = patSelLst.Find(p=>(bool)p.IsChecked).Name.Substring(6,2).ToInt(); //パターン形
            int nn=0;
            if( ModeAuto ) nn=GNP00.SDKCntrl.PatGen.patternAutoMaker(patSel);
            else{
                Point pt=Mouse.GetPosition(PB_pattern);
                int row=0, col=0;
                if( __GetRCPositionFromPattern( pt,ref row,ref col) ){
                    nn=GNP00.SDKCntrl.PatGen.symmetryPattern(patSel,row,col,false);
                }
            }
            SDK_Ctrl.rxCTRL = 0;     //Initialize Puzzle candidate generater
            _SetBitmap_PB_pattern();
            labelPattern.Content = pRes.lblNoOfCells+nn;
        }    
        private bool __GetRCPositionFromPattern( Point pt, ref int row, ref int col ){
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
    #endregion Sudoku Pattern Setting

    #region display
        private bool sNoAssist=false;
        private void _Display_GB_GBoard( bool DevelopB=false ){
            if( GNP00.AnalyzerMode=="MultiSolve" && __DispMode!="Complated" )  return;
            UProblem P = pGP;

            lblUnderAnalysis.Visibility = (GNP00.GSmode=="tabASolve")? Visibility.Visible: Visibility.Hidden; 
            Lbl_onAnalyzerM.Visibility = Visibility.Visible; 
      
            SDKGrp.GBoardPaint( bmpGZero, P.BDL, GNP00.GSmode, sNoAssist );
            PB_GBoard.Source = bmpGZero;

            int nP=0, nZ=0, nM=0;
            __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
            txtProbNo.Text = (P.ID+1).ToString();
            txtProbName.Text = P.Name;
            nUDDifficultyLevel.Text = P.DifLevel.ToString();

            if(DevelopB) _Display_Develop();
			if(GNP00.GSmode=="tabASolve")  _Display_ExtResultWin();		
        }
        private void _Display_Develop(){
            int[] TrPara=pPTrans.TrPara;
            LblRg.Content    = TrPara[0].ToString();      
            LblR123g.Content = TrPara[1].ToString();
            LblR456g.Content = TrPara[2].ToString();
            LblR789g.Content = TrPara[3].ToString();

            LblCg.Content    = TrPara[4].ToString();
            LblC123g.Content = TrPara[5].ToString();
            LblC456g.Content = TrPara[6].ToString();
            LblC789g.Content = TrPara[7].ToString(); 
            LblRC7g.Content  = TrPara[8].ToString();
        }

		private void _Display_ExtResultWin(){
			if( pGP.extRes==null || pGP.extRes.Length<5 ){
				if(ExtResultWin!=null && ExtResultWin.Visibility==Visibility.Visible ){
					ExtResultWin.Visibility=Visibility.Hidden;
				}
				return;
			}
				//WriteLine( "_Display_ExtResultWin" );
			if(ExtResultWin==null) {
				ExtResultWin=new ExtendResultWin(this);
				ExtResultWin.Width = this.Width;
				ExtResultWin.Left  = this.Left;
				ExtResultWin.Top   = this.Top+this.Height;
			}
			ExtResultWin.SetText(pGP.extRes);
			ExtResultWin.Show();
		}	

        private void chbSetAnswer_Checked( object sender, RoutedEventArgs e ){
            sNoAssist = (bool)chbSetAnswer.IsChecked;

            int nP=0, nZ=0, nM=0;
            if( __Set_CellsPZMCount(ref nP,ref nZ,ref nM) ) _Display_GB_GBoard( );
        }

        private bool __Set_CellsPZMCount( ref int nP, ref int nZ, ref int nM ){
            nP=nZ=nM=0;
            if( txbPuzzle==null || GNP00.GNPX_Eng==null )  return false;
            bool sAssist=GNP00.GNPX_Eng.AnMan.AggregateCellsPZM(ref nP, ref nZ, ref nM);
            if( nP+nZ+nM>0 ){
                txbPuzzle.Text=nP.ToString();
                txbSolved.Text=nM.ToString();
                txbUnsolved.Text=nZ.ToString();
                if(nP>0)  txbUnsolved.Background = (nZ==0)? Brushes.Navy: Brushes.Black;
            }
            return ((nP+nM>0)&sAssist);
        }

        private void chbAnalyze00_Checked( object sender, RoutedEventArgs e ){
            if( bmpGZero==null )  return;
            sNoAssist = (bool)chbDisplayCandidate.IsChecked;
            _SetScreenProblem();
        }
        private void chbAssist01_Checked( object sender, RoutedEventArgs e ){
            sNoAssist = (bool)chbShowNousedDigits.IsChecked;
            _Display_GB_GBoard();　//(Show free numbers)
        }

        private int    __GCCounter__=0;
        private int    _ProgressPer;
        private string __DispMode=null;

        private void displayTimer_Tick( object sender, EventArgs e ){
            _Display_GB_GBoard(); //******************

            switch(GNP00.GSmode){
                case "tabACreate": _Display_CreateProblem(); break;

                case "tabBMultiSolve":
                case "tabASolve":      _Display_AnalyzeProb(); break;
            }    

            lblResourceMemory.Content = "Memory:" + GC.GetTotalMemory( true ).ToString();            
            if( ((++__GCCounter__)%1000)==0 ){ GC.Collect(); __GCCounter__=0; }
        }

        private void _Display_CreateProblem( ){
            txbNoOfTrials.Text    = GNP00.SDKCntrl.LoopCC.ToString();
            txbNoOfTrialsCum.Text = SDK_Ctrl.TLoopCC.ToString();
            txbBasicPattern.Text  = GNP00.SDKCntrl.PatternCC.ToString();
            int n=gamGen05.Text.ToInt();
            lblNoOfProblems1.Content = (n-_ProgressPer).ToString();

            UProblem pGP=GNP00.pGP;
            if( pGP!=null ){
                int nn=GNP00.SDKProbLst.Count;
                if( nn>0 ){
                    txtProbNo.Text = nn.ToString();
                    txtProbName.Text = GNP00.SDKProbLst.Last().Name;
                    nUDDifficultyLevel.Text = pGP.DifLevel.ToString();
                }
            }

            TimeSpan ts = AnalyzerLap.Elapsed;
            string st = "";
            if( ts.TotalSeconds>1.0 ) st += ts.TotalSeconds.ToString( " 0.0" ) + " sec";
            else                      st += ts.TotalMilliseconds.ToString( " 0.0" ) + " msec";

            Lbl_onAnalyzerTS.Content = st;
            Lbl_onAnalyzerTSM.Content = st;
            txbEpapsedTimeTS3.Text    = st;

            if( __DispMode!=null && __DispMode!="" ){
                _SetScreenProblem();
                if( __DispMode=="Canceled" )  lblNoOfTrials.Content += " "+pRes.lblNoOfTrialsCanceled;
                displayTimer.Stop();
                AnalyzerLap.Stop();
                btnCreateProblemMlt.Content = pRes.btnCreateProblemMlt;
            }
            __DispMode="";
        }

        private void _Display_AnalyzeProb(){

            if( __DispMode=="Canceled" ){
                lblUnderAnalysis.Foreground = Brushes.LightCoral; 
                Lbl_onAnalyzerM.Foreground  = Brushes.LightCoral; 
                displayTimer.Stop();
            }
            
            else if( __DispMode=="Complated" ){
                lblUnderAnalysis.Content = pRes.msgAnalysisComplate;
                if( (string)SDK_Ctrl.MltAnsOption["abortResult"]!="" ){
                    Lbl_onAnalyzerM.Content = SDK_Ctrl.MltAnsOption["abortResult"];
                }
                else{
                    Lbl_onAnalyzerM.Content = pRes.msgAnalysisComplate;
                    Lbl_onAnalyzerM.Foreground = Brushes.LightBlue;  

					if( (bool)chbDifficultySetting.IsChecked ){
						string prbMessage;
						int DifLevel = GNP00.GNPX_Eng.GetDifficultyLevel( out prbMessage );
						pGP.DifLevel = DifLevel;
						nUDDifficultyLevel.Text = DifLevel.ToString();
					}
                }
                btnSolve.Content     = pRes.btnSolve;
                btnMultiSolve.Content= pRes.btnMultiSolve;
                btnMultiSolve.IsEnabled = true;
                lblUnderAnalysis.Foreground = Brushes.LightBlue;   
 
                _Set_DGViewMethodCounter();  //Aggregation of methods
                string msgST = pGP.Sol_ResultLong;
                if(!ErrorStopB) lblAnalyzerResult.Text = msgST;
                if( msgST.LastIndexOf("anti-rule")>=0 || msgST.LastIndexOf("Unparsable")>=0 ){ }
                displayTimer.Stop();
            }
            else{
                if(!ErrorStopB)  lblAnalyzerResult.Text = GNPZ_Engin.GNPX_AnalyzerMessage;
                Lbl_onAnalyzerM.Content = pRes.lblUnderAnalysis+" : "+GNPZ_Engin.GNPX_AnalyzerMessage;
            }        

            if( UPP!=null && UPP.Count>0 ){
                try{
                    if( __DispMode=="Complated" ) LstBxMltAns.ItemsSource=null;
                    LstBxMltAns.ItemsSource=UPP;

                    if( __DispMode=="Complated" ){
                        LstBxMltAns.ScrollIntoView(UPP.First());
                        selXUPP=0;
                        AnalyzerLap.Stop();
                    }
                    else{
                        LstBxMltAns.ScrollIntoView(UPP.Last());
                        selXUPP=UPP.Count-1;
                    }
                    LstBxMltAns.SelectedIndex=selXUPP;

                    var Q=(UProbS)LstBxMltAns.SelectedItem;
                    if(Q==null) Q=UPP[0];
                    lblAnalyzerResultM.Text= "["+(Q.IDmp1)+"] "+Q.Sol_ResultLong;
                }
                catch( Exception e ){
                    WriteLine(e.Message);
                    WriteLine(e.StackTrace);
                }
            }

            string st="";   
            TimeSpan ts2 = GNPZ_Engin.SdkExecTime;
            TimeSpan ts = AnalyzerLap.Elapsed;
            if( ts.TotalSeconds>1.0 )  st=ts.TotalSeconds.ToString("0.000")+" sec";
            else                       st=ts.TotalMilliseconds.ToString("0.000")+" msec";

            Lbl_onAnalyzerTS.Content   = st;
            Lbl_onAnalyzerTSM.Content  = st;
            txbEpapsedTimeTS3.Text     = st;
                        
            btnSolveUp.Content         = pRes.btnSolveUp;

            if( GNPZ_Engin.GNPX_AnalyzerMessage.Contains("sys") ){
                lblAnalyzerResultM.Text = GNPZ_Engin.GNPX_AnalyzerMessage;
            }

            this.Cursor = Cursors.Arrow;
            if( __DispMode=="Complated" ) _SetScreenProblem();
 
            OnWork = 0;
//            __DispMode="";
        }
 
        private void btnCopyBitMap_Click( object sender, RoutedEventArgs e ){
            try{
                Clipboard.SetData(DataFormats.Bitmap,BitmapFrame.Create(bmpGZero));
            }
            catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            //( clipboard COMException http://shen7113.blog.fc2.com/blog-entry-28.html )
        }
        private void btnSaveBitMap_Click( object sender, RoutedEventArgs e ){
            BitmapEncoder enc = new PngBitmapEncoder(); // JpegBitmapEncoder(); BmpBitmapEncoder();
            BitmapFrame bmf = BitmapFrame.Create(bmpGZero);
            enc.Frames.Add(bmf);
            try {
                Clipboard.SetData(DataFormats.Bitmap,bmf);
            }
            catch(System.Runtime.InteropServices.COMException){ /* NOP */ }

            if( !Directory.Exists(pRes.fldSuDoKuImages) ){ Directory.CreateDirectory(pRes.fldSuDoKuImages); }
            string fName=DateTime.Now.ToString("yyyyMMdd HHmmss")+".png";
            using( Stream stream = File.Create(pRes.fldSuDoKuImages+"/"+fName) ){
                enc.Save(stream);
            }    
        }
    #endregion display

    #region mouse IF
        //***** Control variable
        private int     noPChg = -1;
        private int[]   noPChgList = new int[9];
        private int     rowMemo; 
        private int     colMemo;
        private int     noPMemo;
        private bool    mouseFlag = false;

        private void PB_GBoard_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ){  
            if( mouseFlag ) return;
            if( GNP00.GSmode!="tabACreate" && GNP00.GSmode!="tabATransform" )  return;

            int r, c;
            int noP = _Get_PB_GBoardRCNum( out r, out c );
            if( noP<=0 ){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }
            rowMemo=r; colMemo=c;       
            mouseFlag = true;
            if( GNP00.GSmode=="tabATransform" ) return;

            if( GNP00.GSmode!="tabACreate" ){
                if( pGP.BDL[r*9+c].No > 0 ) return;
            }

            rowMemo=r; colMemo=c; noPMemo=noP;
            _GNumericPadManager( r, c, noP );  
        }
        private void PB_GBoard_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ){
            if( !mouseFlag ) return;
            mouseFlag = false;

            int noP=0;
            if( GNP00.GSmode=="tabATransform" ){ _Change_PB_GBoardNum( ref noP ); return; }
        }

        private void _GNumericPadManager( int r, int c, int noP ){
            noPMemo = noP;
            int FreeB=0x1FF;
            if( GNP00.GSmode=="tabACreate"  ){
                FreeB = pGP.BDL[r*9+c].FreeB;
            }

            GnumericPad.Source = SDKGrp.CreateCellImageLight( pGP.BDL[r*9+c], noP );
 
            int PosX = (int)PB_GBoard.Margin.Left + 2 + 37*c + (int)c/3;
            int PosY = (int)PB_GBoard.Margin.Top  + 2 + 37*r + (int)r/3;        
            GnumericPad.Margin = new Thickness(PosX, PosY, 0,0 );        
            GnumericPad.Visibility = Visibility.Visible;
            
        }       
        private void GnumericPad_MouseMove( object sender, MouseEventArgs e ){
            if( !mouseFlag ) return;
            int r, c;
             
            if( GNP00.GSmode=="tabATransform" ) return;
            int noP  = _Get_PB_GBoardRCNum( out r, out c );
            if( noP<=0 || r!=rowMemo || c!=colMemo ){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }

            if( GNP00.GSmode!="tabACreate" ){
                if( pGP.BDL[r*9+c].No > 0) return;
            }

            if( r!=rowMemo || c!=colMemo ){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }

            if( noP!=noPMemo ){
                rowMemo = r;
                colMemo = c;
                noPMemo = noP;
                _GNumericPadManager( r, c, noP );
            }
        }        
        private void GnumericPad_MouseUp( object sender, MouseButtonEventArgs e ){
            if( !mouseFlag ) return;
            mouseFlag = false;
            int r, c;
            int noP = _Get_PB_GBoardRCNum( out r, out c );

            if( r!=rowMemo || c!=colMemo ) return;
            /*
            if( GNP00.GSmode=="tabATransform" ){ _Change_PB_GBoardNum( ref noP ); return; }
            //if( noP <= 0 ){ justNum = -1;  return; }
            */
            UCell BDX = pGP.BDL[rowMemo*9+colMemo];

            int numAbs = Abs(BDX.No);
            if( numAbs==noP ){ BDX.No=0; goto MouseUpFinary; }

            int FreeB = BDX.FreeB;
            if( GNP00.GSmode=="tabACreate" ){
                BDX.No=0;
                GNP00.GNPX_Eng.AnMan.Set_CellFreeB();
                FreeB = BDX.FreeB;
                if( ((FreeB>>(noP-1))&1)==0 ) goto MouseUpFinary;
                BDX.No=noP;
            }
          
          MouseUpFinary:
            GNP00.GNPX_Eng.AnMan.Set_CellFreeB();
            _SetScreenProblem();
            GnumericPad.Visibility = Visibility.Hidden;
            rowMemo=-1; colMemo=-1;

            int nP=0, nZ=0, nM=0;
            __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
        }

        private int  _Get_PB_GBoardRCNum( out int boadRow, out int boadCol ){
            int cellSizeP = GNP00.cellSizeP;
            int cellSizeP3 = cellSizeP*3;
            int cellSizeP32 = cellSizeP3+2;
            int LWid = GNP00.lineWidth;

            boadRow = boadCol =-1;
            Point pt = Mouse.GetPosition(PB_GBoard);
            int rn = (int)pt.Y-2;
            if( rn/cellSizeP32 >= 9 )  return -1;
            rn = rn - rn/cellSizeP32*LWid;
            if( rn/cellSizeP >= 9 )  return -1;
            boadRow = rn / cellSizeP;
            rn = (rn%cellSizeP)/12;
            if( rn >= 3 )  return -1;

            int cn = (int)pt.X-2;
            if( cn/cellSizeP32 >= 9 )  return -1;
            cn = cn - cn/cellSizeP32*LWid;
            if( cn/cellSizeP >= 9 )  return -1;
            boadCol = cn / cellSizeP;
            cn = (cn%cellSizeP)/12;
            if( cn >= 3 )  return -1;


            if( boadRow<0 || boadRow>=9 || boadCol<0 || boadCol>=9) return -1;
            int noP = cn+rn*3+1;
            return noP;
        }
    #endregion mouse IF 

    #region Problem creation
      #region Problem creation[Manual]
        private void btnBoardClear_Click( object sender, RoutedEventArgs e ){
            for( int rc=0; rc<81; rc++ ){ pGP.BDL[rc] = new UCell(rc); }
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
            UProblem UPcpy= pGP.Copy(0,0);
            UPcpy.Name="copy";
            GNP00.CreateNewPrb(UPcpy);//reserve space for new problems
            _SetScreenProblem();　    //Show free numbers
        }

        #region Number change
        private void btnNumChange_Click( object sender, RoutedEventArgs e ){
                TransSolverA("NumChange",true); //display solution

                txNumChange.Text = "1";
                txNumChange.Visibility = Visibility.Visible;
                btnNumChangeDone.Visibility = Visibility.Visible;
                noPChg = 1;
                for( int k=0; k<9; k++ ) noPChgList[k] = k+1;
                mouseFlag = false;
                PB_GBoard.IsEnabled = true;
                _SetScreenProblem();　//Show free numbers
                _Display_GB_GBoard();
        //    }
        }
        private void btnNumChangeDone_Click( object sender, RoutedEventArgs e ){
            GNP00.GSmode = "tabACreate";
            txNumChange.Visibility = Visibility.Hidden;
            btnNumChangeDone.Visibility = Visibility.Hidden;
            noPChg = -1;
       //     TransSolverA("Checked",(bool)chbShowSolution.IsChecked);
            _Display_GB_GBoard();
        }
        private void _Change_PB_GBoardNum( ref int noP ){
            int nm, nmAbs;
            if( rowMemo<0 || rowMemo>8 || colMemo<0 || colMemo>8) return;

            noP = Abs( pGP.BDL[rowMemo*9+colMemo].No );
            if( noP==0 )  return;
            if( noP!=noPChg ){

                foreach( var q in pGP.BDL ){
                    nm = q.No;
                    if( nm==0 )  continue;
                    nmAbs = Abs( nm );
                    if( noP>=noPChg ){
                        if( nmAbs < noPChg)  continue;
                        else if( nmAbs==noP ) q.No = (nm>0)? noPChg: -noPChg;
                        else if( nmAbs<noP )  q.No = (nm>0)? nmAbs+1:   -(nmAbs+1);
                    }
                    else{
                        if( nmAbs < noP ) continue;
                        else if( nmAbs==noP)        q.No = (nm>0)? noPChg: -noPChg;
                        else if( nmAbs<=noPChg ) q.No = (nm>0)? nmAbs-1:   -(nmAbs-1);
                    }
                }
            }

            _SetScreenProblem();
            noPChg++;
            txNumChange.Text = noPChg.ToString();
            if( noPChg>9 ){
                GNP00.GSmode = "tabACreate";
                txNumChange.Visibility = Visibility.Hidden;
                btnNumChangeDone.Visibility = Visibility.Hidden;
                noPChg = -1;
            }
            else GNP00.GSmode = "tabATransform";
            mouseFlag = false;
            return;
        }
        private void _SetGSBoad_rc_num( int r, int c, int noP ){
            if( r<0 || r>=9 ) return;
            if( c<0 || c>=9 ) return;
            int numAbs = Abs(noP);
            if( numAbs==0 || numAbs>=10) return;
            pGP.BDL[r*9+c].No=noP;
        }
        #endregion Number change  
 
      #endregion  Problem creation[Manual]

      #region  Problem creation[Auto]
        //Start
        private Task taskSDK;
        private CancellationTokenSource tokSrc;
        private void btnP13Start_Click( object sender, RoutedEventArgs e ){
        //    int mc=GNP00.SDKCntrl.GNPX_Eng.Set_MethodLst_Run( );
        //    if( mc<=0 ) GNP00.ResetMethodList();

            if( (string)btnCreateProblemMlt.Content==pRes.btnCreateProblemMlt ){
                __DispMode=null;
                GNP00.SDKCntrl.LoopCC = 0;
                btnCreateProblemMlt.Content  = pRes.msgSuspend; //

                GNPZ_Engin.SolInfoB = false;
                if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC = 0;
                GNP00.SDKCntrl.CbxDspNumRandmize = (bool)chbRandomizingNumbers.IsChecked;//数字の乱数化
                GNP00.SDKCntrl.GenLStyp = int.Parse(GenLStyp.Text);
                GNP00.SDKCntrl.CbxNextLSpattern  = (bool)chbChangeBasicPattenOnSuccess.IsChecked;
                
                SDK_Ctrl.lvlLow = (int)gamGen01.Value;
                SDK_Ctrl.lvlHgh = (int)gamGen02.Value;
                SDK_Ctrl.FilePut_GenPrb = (bool)chbFileOutputOnSuccess.IsChecked;

                int n=gamGen05.Text.ToInt();
                n = Max(Min(n,1000),0); 
                SDK_Ctrl.MltProblem = _ProgressPer = n;
//                GNP00.MltSolSave = true;

                displayTimer.Start();
                AnalyzerLap.Start();

                tokSrc = new CancellationTokenSource();　//procedures for suspension 
                taskSDK = new Task( ()=> GNP00.SDKCntrl.SDK_ProblemMakerReal(tokSrc.Token), tokSrc.Token );
                taskSDK.ContinueWith( t=> btnP13Start2Complated() ); //完了時の手続きを登録
                taskSDK.Start();
            }
            else{   //"Suspend"
                try{
                    tokSrc.Cancel();
                    taskSDK.Wait();
                    GNP00.CurrentPrbNo=999999999;//GNP00.SDKProbLst.Count;
                    _SetScreenProblem( );
                    btnCreateProblemMlt.Content = pRes.btnCreateProblemMlt;
                }
                catch(AggregateException){
                    __DispMode="Canceled"; 
                }
            }
            return;
        }  
        //Progress display
        public void BWGenPrb_ProgressChanged( object sender, SDKEventArgs e ){ _ProgressPer=e.ProgressPer; }
        //Done
        private void btnP13Start2Complated( ){ __DispMode="Complated"; }

        private void gamGen01_NumUDValueChanged(Object sender,GIDOOEventArgs args) {
            if( gamGen02==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Lval>Uval ) gamGen02.Value=Lval;
        }

        private void gamGen02_NumUDValueChanged(Object sender,GIDOOEventArgs args) {
            if( gamGen01==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Uval<Lval ) gamGen01.Value=Uval;
        }

        private void btnESnxtSucc_Click( Object sender,RoutedEventArgs e ){
            int RX=(int)UP_ESRow.Value;
            GNP00.SDKCntrl.Force_NextSuccessor(RX);
            _Display_GB_GBoard();
        }
        #endregion 問題作成【自動】

        private void randumSeed_TextChanged( object sender, TextChangedEventArgs e ){
            int rv=randumSeed.Text.ToInt();
            GNP00.SDKCntrl.randumSeedVal = rv;
            GNP00.SDKCntrl.SetRandumSeed(rv);
        }

    #endregion Problem creation

    #region analysis
      //[Note] task,ProgressChanged,Completed,Canceled threadSafe（Prohibition of control operation）
    
      #region analysis[Step] 
        private int OnWork = 0;

        private bool ErrorStopB;
        private void btnSolve_Click( object sender, RoutedEventArgs e ){
            if( OnWork==2 ) return;

            GNP00.AnalyzerMode = "Solve";
            if( SDK_Ctrl.UGPMan==null ) SDK_Ctrl.UGPMan=new UProbMan(1);
            else if(SDK_Ctrl.UGPMan.CreateNextStage())  return;  //solveup

            SuDoKuSolver();
            MAnalizeBtnSet();
        }

        private void SuDoKuSolver(){
            try{
                lblUnderAnalysis.Foreground = Brushes.LightGreen;
                Lbl_onAnalyzerM.Foreground  = Brushes.LightGreen;
                if( (string)btnSolve.Content!=pRes.msgSuspend){
                    int mc=GNP00.GNPX_Eng.Set_MethodLst_Run( );
                    if( mc<=0 ) GNP00.ResetMethodList();
                    lblUnderAnalysis.Visibility = Visibility.Visible;
                    Lbl_onAnalyzerM.Visibility = Visibility.Visible;

                    //GNPZ_Engin.SolInfoB = false;
                    if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC=0;

                    SDK_Ctrl.MltProblem = 1;    //単独
                    SDK_Ctrl.lvlLow = 0;
                    SDK_Ctrl.lvlHgh = 999;
                    GNP00.SDKCntrl.CbxDspNumRandmize=false;
                    GNP00.SDKCntrl.GenLStyp = 1;

                    GNPXApp000.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
                    GNPZ_Engin.SolInfoB = true;
                    AnalyzerLap.Reset();

                    if( GNP00.AnalyzerMode=="Solve" || GNP00.AnalyzerMode=="MultiSolve" ){
                        if(GNP00.pGP.SolCode<0)  GNP00.pGP.SolCode=0;
                        ErrorStopB = !_cellFixSub();

                        List<UCell> pBDL = pGP.BDL;
                        if( pBDL.Count(p=>p.No==0)==0 ){ //analysis completed
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
        }
      #endregion  analysis[Step] 

      #region MultiAnalysis 
        
        static public  int selXUPP;
        static public  List<UProbS> __UPPPPP=new List<UProbS>(); //__UPrbSLst
        static public  List<UProbS> UPP{ get{return __UPPPPP; } set{__UPPPPP=value; } } //__UPPPPP
        static public  List<UProblem> MltAnsLs; //MAQQQQQ

        private void btnMultiSolve_Click( object sender, RoutedEventArgs e ){
            GNP00.AnalyzerMode = "MultiSolve";

            if( SDK_Ctrl.UGPMan==null ) SDK_Ctrl.UGPMan=new UProbMan(1);
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

        public void MultiSolved( object sender, SDKSolutionEventArgs e ){
            UPP.Add(e.UPB);
        }
        private void LstBxMltAns_SelectionChanged(object sender,SelectionChangedEventArgs e){
            if( SDK_Ctrl.UGPMan==null )   return;
            var Q=(UProbS)LstBxMltAns.SelectedItem;
            if(Q==null)  return;
            selXUPP=Q.IDmp1-1;
            if(selXUPP<0)  return;
            var U=UPP[selXUPP];
            lblAnalyzerResultM.Text= "["+(Q.IDmp1)+"] "+Q.Sol_ResultLong; 

            List<UProblem> pMltUProbLst=SDK_Ctrl.UGPMan.MltUProbLst;
            UProblem pGPx=pMltUProbLst[selXUPP];
            SDK_Ctrl.UGPMan.pGPsel=pGPx;
            if( pGP.IDm!=selXUPP) SDK_Ctrl.UGPMan.GPMnxt=null;
            GNP00.GNPX_Eng.pGP = pGPx;
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

            List<UProblem> pMltUProbLst=SDK_Ctrl.UGPMan.MltUProbLst;
            if( !GNPXApp000.chbConfirmMultipleCells || pMltUProbLst==null ) return;

            AnalyzerCC=SDK_Ctrl.UGPMan.stageNo;
            GNPZ_Engin.GNPX_AnalyzerMessage = SDK_Ctrl.UGPMan.pGPsel.Sol_ResultLong;
            txbStepCC.Text  = AnalyzerCC.ToString();
            txbStepMCC.Text = txbStepCC.Text;

            selXUPP=SDK_Ctrl.UGPMan.pGPsel.IDm;
            if(selXUPP<0)  return;
            UPP = pMltUProbLst.ConvertAll(P=>new UProbS(P));
            LstBxMltAns.ItemsSource=UPP;
            LstBxMltAns.SelectedIndex = selXUPP;

            if( selXUPP<UPP.Count ){
                LstBxMltAns.ScrollIntoView(UPP[selXUPP]);
            }
        }
        
        private void LstBxMltAns_MouseWheel(object sender,MouseWheelEventArgs e){ /*dummy*/ }

      #endregion MultiAnalysis    
    
      #region analysis[All] 
        private void task_SDKsolverAuto_ProgressChanged( object sender, ProgressChangedEventArgs e ){
            lblNoOfTrials.Content = pRes.lblNoOfTrials + GNP00.SDKCntrl.LoopCC;
            txbBasicPattern.Text  = GNP00.SDKCntrl.PatternCC.ToString();
            btnSolveUp.Content = pRes.btnSolveUp;
            OnWork=0;
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
                displayTimer.Start();

              AnalyzerEnd:
                return;

            }
        }
        private void btnAnalyzerResetAll_Click( object sender, RoutedEventArgs e ){
            Thickness X=PB_GBoard.Margin;
            PB_GBoard.Margin=new Thickness(X.Left+2,X.Top+2,X.Right,X.Bottom);
            _ResetAnalizer(true);
            bruMoveTimer.Start();
            UPP.Clear();
            //q            SDK_Ctrl.UGPMan.MltUProbLst=new List<UProblem>();
            MAnalizeBtnSet();
        }
        private void _ResetAnalizer( bool AllF=true ){
            if( OnWork>0 ) return;
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
//q            SDK_Ctrl.UGPMan.MltUProbLst=new List<UProblem>();
            txbEpapsedTimeTS3.Text    = "";

            GNP00.GNPX_Eng.AnMan.ResetAnalysisResult(AllF);
            GNP00.GNPX_Eng.AnalyzerCounterReset();
            SDK_Ctrl.UGPMan=null;           //Initialize Step/Multiple Solution search 

            displayTimer.Stop();
            _SetScreenProblem();
        }
      #endregion analysis[All] 

      #region analysis[Method aggregation]
        public int AnalyzerCC=0;
        private int AnalyzerCCMemo=0;
        private int AnalyzerMMemo=0;   
        private int[] eNChk;
        private bool _cellFixSub(  ){
            if( GNP00.pGP.SolCode<0) return false;
            bool retB=GNP00.GNPX_Eng.AnMan.FixOrEliminate_Sudoku( ref eNChk );
            if( !retB && GNP00.GNPX_Eng.AnMan.SolCode==-9119 ){
                string st="";
                for( int h=0; h<27; h++ ){
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
            if( nZ==0){ GNP00.GNPX_Eng.AnMan.SolCode=0; return true; }
            if( nM!=AnalyzerMMemo ){
                AnalyzerCCMemo = AnalyzerCC;
                AnalyzerMMemo = nM;
            }

            if( nZ==0 && (bool)chbDifficultySetting.IsChecked ){
                string prbMessage;
                int DifLevel = GNP00.GNPX_Eng.GetDifficultyLevel( out prbMessage );
                pGP.DifLevel = DifLevel;
                nUDDifficultyLevel.Text = DifLevel.ToString();
            }
            return true;
        }
        private string _ToHouseName( int h ){
            string st="";
            switch(h/9){
                case 0: st="row"; break;
                case 1: st="Column"; break;
                case 2: st="block"; break;
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
            foreach( var P in GNP00.GNPX_Eng.MethodLst_Run ){
                if( P.UsedCC <= 0 )  continue;
                MCList.Add( new MethodCounter( P.MethodName, P.UsedCC ) );
            }

            DGViewMethodCounter.ItemsSource = MCList;
            if( MCList.Count>0 )  DGViewMethodCounter.SelectedIndex=-1;

            if( GNP00.GSmode=="tabASolve" && MCList.Count>0 && DGViewMethodCounter.Columns.Count>1 ){
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

    #region Puzzle Transform
        private PuzzleTrans pPTrans{ get{ return GNP00.PTrans; } }
        private void btnPatCVRCg_Click( object sender, RoutedEventArgs e ){
            Button btn = sender as Button;
            TransSolverA(btn.Name,(bool)chbShowSolution.IsChecked);
            _SetScreenProblem();
            _Display_Develop();
        }
        private void TransSolverA( string Name, bool DspSol ){
            SDK_Ctrl.MltProblem = 1;
            SDK_Ctrl.lvlLow = 0;
            SDK_Ctrl.lvlHgh = 999;
            GNP00.SDKCntrl.CbxDspNumRandmize=false;
            GNP00.SDKCntrl.GenLStyp = 1;
            GNPXApp000.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
            GNPZ_Engin.SolInfoB = true;

            pPTrans.SDK_TransProbG(Name,DspSol);
        }
        private void chbShowSolution_Checked( object sender, RoutedEventArgs e ){
            if(pGP.AnsNum==null)  TransSolverA("Checked",true);
            pPTrans.SDK_TransProbG("Checked",(bool)chbShowSolution.IsChecked);

            _Display_GB_GBoard(DevelopB:true);
        }
        private void btnTransEst_Click( object sender, RoutedEventArgs e ){
            pPTrans.btnTransEst();
            _Display_GB_GBoard(DevelopB:true);
        }
        private void btnTransRes_Click(object sender, RoutedEventArgs e ){
            pPTrans.btnTransRes();
            if(!(bool)chbShowSolution.IsChecked) pGP.BDL.ForEach(P=>{P.No=Max(P.No,0);});
            _Display_GB_GBoard(DevelopB:true);
        }

        private void btnNomalize_Click( object sender, RoutedEventArgs e ){
            if(pGP.AnsNum==null)  TransSolverA("Checked",true);
            string st=pPTrans.SDK_Nomalize( (bool)chbShowSolution.IsChecked, (bool)chbNrmlNum.IsChecked );
            tbxTransReport.Text=st;
            _Display_GB_GBoard(DevelopB:true);
        }
    #endregion Puzzle Transform
    }
}