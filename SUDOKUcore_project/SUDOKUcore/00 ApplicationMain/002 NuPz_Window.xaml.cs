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
using System.Runtime.InteropServices;

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

//using OpenCvSharp;
//using OpenCvSharp.Extensions;

using GIDOOCV;

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

namespace GNPXcore{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win: sysWin.Window{

        [DllImport("USER32.dll",CallingConvention=CallingConvention.StdCall)]
        static private extern void SetCursorPos(int X,int Y); //Move the mouse cursor to Control

        private sysWin.Point    _WinPosMemo;
        public  GNPXApp000      GNP00;
        public  GNPZ_Graphics   SDKGrp; //board surface display bitmap
        public  CultureInfo     culture{ get{ return pRes.Culture; } }

        private int             WOpacityCC=0;
        private Stopwatch       AnalyzerLap;
        private string          AnalyzerLapElaped{
            get{
                TimeSpan ts = AnalyzerLap.Elapsed;
                string st = "";
                if( ts.TotalSeconds>1.0 ) st += ts.TotalSeconds.ToString("0.0") + " sec";
                else                      st += ts.TotalMilliseconds.ToString("0.0") + " msec";
                return st;
            }
        }
        private DispatcherTimer startingTimer;
        private DispatcherTimer endingTimer;
        private DispatcherTimer displayTimer;   
        private DispatcherTimer bruMoveTimer;
        private RenderTargetBitmap bmpGZero;
        private DevelopWin      devWin;
		private ExtendResultWin ExtResultWin;

        private UPuzzle        pGP{ get{ return GNP00.GNPX_Eng.pGP; } }

        //=============
        private List<RadioButton> patSelLst;
        private List<RadioButton> rdbVideoCameraLst;

        private DispatcherTimer     timerShortMessage;

    #region Application start/end
        public NuPz_Win(){
            try{
                GNPXApp000.pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                GNP00  = new GNPXApp000(this);
                SDKGrp = new GNPZ_Graphics(GNP00);
           
                devWin = new DevelopWin(this);
			    GroupedLinkGen.devWin = devWin;

                InitializeComponent();

                LbShortMes.Visibility = Visibility.Hidden;
                cmbLanguageLst.ItemsSource = GNP00.LanguageLst;  
                lblCurrentnDifficultyLevel.Visibility=Visibility.Hidden;

                GNPX_AnalyzerMan.Send_Solved += MultiSolved;

                GNPXGNPX.Content = "GNPXcore "+DateTime.Now.Year;
           
              //RadioButton Controls Collection
                var rdbLst = GNPZExtender.GetControlsCollection<RadioButton>(this);
                patSelLst = rdbLst.FindAll(p=>p.Name.Contains("patSel"));
                rdbVideoCameraLst = rdbLst.FindAll(p=>p.Name.Contains("rdbCam"));

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
                SDKGrp.GBoardPaint( bmpGZero, (new UPuzzle()).BDL, "tabACreate" );
                PB_GBoard.Source = bmpGZero;    //◆Initial setting

                lblProcessorCount.Content = "ProcessorCount:"+Environment.ProcessorCount;

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
//                NuPz_Win_camera();
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

            //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
        #if !DEBUG
            GNPXApp000.GMthdOption["GeneralLogicOn"]="0";   //GeneralLogic Off
        #endif
            //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

            po=(string)GNPXApp000.GMthdOption["GeneralLogicOn"];
            GeneralLogicOnChbx.IsChecked=(po.ToInt()==1);

            po =(string)GNPXApp000.GMthdOption["GenLogMaxSize"];
            int GLMaxSize=po.ToInt();
            if( GLMaxSize>0 && GLMaxSize<=(int)GenLogMaxSize.MaxValue ) GenLogMaxSize.Value=GLMaxSize;

            po=(string)GNPXApp000.GMthdOption["GenLogMaxRank"];
            int GLMaxRank=po.ToInt();
            if( GLMaxRank>=0 && GLMaxRank<=(int)GenLogMaxRank.MaxValue ) GenLogMaxRank.Value=GLMaxRank;

            WOpacityCC =0;
            startingTimer.Start();

            _WinPosMemo = new sysWin.Point(this.Left,this.Top+this.Height);
            
            { //Move the mouse cursor to Button:btnOpenPuzzleFile                 
                var btnQ=btnOpenPuzzleFile;                    
                var ptM=new Point(btnQ.Margin.Left+btnQ.Width/2,btnQ.Margin.Top+btnQ.Height/2);//Center coordinates
                var pt = grdFileIO.PointToScreen(ptM);  //Grid relative coordinates to screen coordinates.
                SetCursorPos((int)pt.X,(int)pt.Y);      //Move the mouse cursor
            }

            GNPXApp000._Loading_ = false;
        }
        private void Window_Unloaded( object sender, RoutedEventArgs e ){
            Environment.Exit(0);
        }
    #endregion Application start/end 

        string CopyrightJP, CopyrightEN;
        private void  MultiLangage_JP_Click(Object sender,RoutedEventArgs e){
            ResourceService.Current.ChangeCulture("ja-JP");
            txtCopyrightDisclaimer.Text = CopyrightJP;
            _MethodSelectionMan();
            bruMoveTimer.Start();
        }
        private void  btnMultiLangage_Click(object sender, RoutedEventArgs e){
            ResourceService.Current.ChangeCulture("en");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            txtCopyrightDisclaimer.Text = CopyrightEN;
            _MethodSelectionMan();
            bruMoveTimer.Start();
        }
        private void  cmbLanguageLst_SelectionChanged(Object sender,SelectionChangedEventArgs e){
            string lng=(string)cmbLanguageLst.SelectedValue;
            ResourceService.Current.ChangeCulture(lng);
            bruMoveTimer.Start();
        }
        public object Culture{ get{ return pRes.Culture; } }

        private void  GNPXGNPX_MouseDoubleClick( object sender, MouseButtonEventArgs e ){
            if(devWin==null) devWin=new DevelopWin(this);
            devWin.Show();
            devWin.Set_dev_GBoard(GNP00.pGP.BDL);
        }

    #region Start/end Timer    
        private void appExit_Click( object sender, RoutedEventArgs e ){
            GNP00.MethodListOutPut();

            WOpacityCC=0;
            endingTimer.IsEnabled = true;
            endingTimer.Start();
//            if(capture!=null) capture.Release();
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
            Thickness X=PB_GBoard.Margin;   //◆
            PB_GBoard.Margin=new Thickness(X.Left-2,X.Top-2,X.Right,X.Bottom);
            bruMoveTimer.Stop();
        }       
    #endregion TimerEvent

    #region Location
		private void GNPXwin_LocationChanged( object sender,EventArgs e ){ //Synchronously move open window
            foreach( sysWin.Window w in Application.Current.Windows )  __GNPXwin_LocationChanged(w);
            _WinPosMemo = new sysWin.Point(this.Left,this.Top);
		}
		private void __GNPXwin_LocationChanged( sysWin.Window w ){
            if( w==null || w.Owner==this || w==this )  return;
            w.Left = this.Left-_WinPosMemo.X+w.Left;
            w.Top  = this.Top -_WinPosMemo.Y+w.Top ;
            w.Topmost=true;
        }	
    #endregion Location

        private void Window_MouseDown( object sender, MouseButtonEventArgs e ){
            if(e.Inner(PB_GBoard))    return; //◆
            if(e.Inner(tabCtrlMode))  return;
            this.DragMove();
        }
        private void btnHomePage_Click( object sender, RoutedEventArgs e ){
            string cul=Thread.CurrentThread.CurrentCulture.Name;
            Console.WriteLine("The current culture is {0}", cul);
            if(cul=="ja-JP") Process.Start("http://csdenp.web.fc2.com");
            else             Process.Start("http://csdenpe.web.fc2.com"); 
        }  
        
        private void btnHomePageGitHub_Click( object sender, RoutedEventArgs e ){
            string cul=Thread.CurrentThread.CurrentCulture.Name;
            Console.WriteLine("The current culture is {0}", cul);
            if(cul=="ja-JP") Process.Start("https://gidoo-code.github.io/Sudoku_Solver_Generator_jp");
            else             Process.Start("https://gidoo-code.github.io/Sudoku_Solver_Generator"); 
        }  
   
    #region ShortMessage
        public void shortMessage(string st, sysWin.Point pt, Color cr, int tm ){
            LbShortMes.Content = st;
            LbShortMes.Foreground = new SolidColorBrush(cr);
            LbShortMes.Margin = new Thickness(pt.X,pt.Y,0,0);

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

    #region　operation mode
        private void tabCtrlMode_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            if( (TabControl)sender!=tabCtrlMode ) return;
            TabItem tb=tabCtrlMode.SelectedItem as TabItem;
            if(tb==null)  return;
            if( tb.Name.Substring(0,4)!="tabA" )  return;
            GNP00.GSmode = (string)tb.Name;    //Tab Name -> State mode

            switch(GNP00.GSmode){
                case "tabASolve": sNoAssist = (bool)chbShowCandidate.IsChecked; break;
                case "tabACreate":
                    TabItem tb2=tabAutoManual.SelectedItem as TabItem;
                    if( tb2==null )  return ;
                    if( (string)tb2.Name=="tabBAuto" )  sNoAssist=false;
                    else sNoAssist = (bool)chbShowNoUsedDigits.IsChecked;
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
            string cul=Thread.CurrentThread.CurrentCulture.Name;
            string st;
            if(cul=="ja-JP") st= B? "有効": "無効";
            else             st= (B? "":"not ") + "available";
            LblGeneralLogic.Content = "GeneralLogic :"+ st;
            LblGeneralLogic.Foreground = (B)? Brushes.LightBlue: Brushes.Yellow;

            po = (string)GNPXApp000.GMthdOption["ForceChainCellHouseOn"];
            B= (po.ToInt()==1);
            if(cul == "ja-JP") st = B ? "有効" : "無効";
            else st = (B? "" : "not ") + "available";
            LblForceChain_CellHouse.Content = "ForceChain_Cell/House :" + st;
            LblForceChain_CellHouse.Foreground = (B) ? Brushes.LightBlue : Brushes.Yellow;
        }

        private void tabSolver_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            TabItem tabItm = tabSolver.SelectedItem as TabItem;
            if( tabItm!=null ) SDK_Ctrl.MltAnsSearch = (tabItm.Name=="tabBMultiSolve");  
        }
        #endregion operation mode
    }
}