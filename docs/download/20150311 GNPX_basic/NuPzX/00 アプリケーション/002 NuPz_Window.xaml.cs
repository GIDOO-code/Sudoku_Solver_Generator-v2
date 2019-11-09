using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
using VisualPrint;

using SUDOKUX;

//http://msdn.microsoft.com/en-us/library/ms750559.aspx

//WPF入門
//http://www.atmarkit.co.jp/fdotnet/chushin/introwpf_index/

//難読化 ▼▼▼　公開時は削除
//http://d.hatena.ne.jp/wwwcfe/20100513/obfuscator

//方法 : ルーティング イベントを処理する
//http://msdn.microsoft.com/ja-jp/library/ms742550.aspx

namespace GNPZ_sdk{
    public delegate void SDKEventHandler( object sender, SDKEventArgs args );
 
    public class SDKEventArgs: EventArgs{
	    public string eName;
	    public int    eCode;
        public int    ProgressPer;
        public bool   Cancelled;

	    public SDKEventArgs( string eName=null, int eCode=-1, int ProgressPer=-1
            , bool Cancelled=false ){
            try{
		        this.eName = eName;
		        this.eCode = eCode;
                this.ProgressPer = ProgressPer;
                this.Cancelled = Cancelled;
            }
            catch(Exception e ){
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
	    }
    }

    public partial class NuPz_Win: Window{
        public  GNumPzl         GNP00;
        public  GNPZ_Graphics   SDKGrp;           //盤面表示ビットマップの作成

        private int             WOpacityCC=0;
        private Stopwatch       AnalyzerLap;
        private DispatcherTimer startingTimer;
        private DispatcherTimer endingTimer;
        private DispatcherTimer displayTimer;
        private RenderTargetBitmap bmpGZero;

        private GNP00_PrePrint    GPrePrnt;

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

    #region イベント
    //http://msdn.microsoft.com/ja-jp/library/system.eventargs.aspx

    public class SDKAlarm{
	    public delegate void FireEventHandler(object sender, SDKEventArgs fe );
	    public event FireEventHandler FireEvent;	
	    public void ActivateSDKAlarm( string eName, int eCode ){
		    SDKEventArgs SDKArgs = new SDKEventArgs(eName, eCode);
		    FireEvent( this, SDKArgs ); 
	    }
    }

    #endregion イベント

    #region G9 開始・終了
        public NuPz_Win(){
            GNP00 = new GNumPzl(this);
            SDKGrp = new GNPZ_Graphics(GNP00);

            InitializeComponent();     
          //this.MouseLeftButtonDown += (sender, e) => this.DragMove(); //ここでは別の方法を採用

            this.Opacity = 0;

            GNPXGNPX.Content = "GNPX "+DateTime.Now.Year;

            possibleTecs.ItemsSource=possibleTecsLst;
           
            //パターン形式の RadioButton Controls Collection
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
            displayTimer.Interval = TimeSpan.FromMilliseconds(20);//50
            displayTimer.Tick += new EventHandler(displayTimer_Tick);
          #endregion Timer

            bmpGZero = new RenderTargetBitmap((int)PB_GBoard.Width,(int)PB_GBoard.Height, 96,96, PixelFormats.Default);
            SDKGrp.GBoardPaint( bmpGZero, new UProblem(), /*GNP00.crList,*/ "問題作成" );
            PB_GBoard.Source = bmpGZero;

            string endl = "\r";
            string st  = "===== 著作権・免責 =====" + endl;
            st += "【著作権】" + endl;
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
            st += "・第三者に対して本ソフトウエアを販売すること、" + endl;
            st += "  販売を目的とした宣伝・営業・複製を行うこと" + endl;
            st += "・第三者に対して本ソフトウエアの使用権を譲渡・再承諾すること。" + endl;
            st += "・本ソフトウエアに対してリバースエンジニアリングを行うこと" + endl;
            st += "・本承諾書、付属文書、本ソフトウエアの一部または全部を改変・除去すること" + endl + endl;

            st += "【免責事項】" + endl;
            st += "作者は、本ソフトウエアの使用または使用不能から生じるコンピュータの故障、情報の喪失、";
            st += "その他あらゆる直接的及び間接的被害に対して一切の責任を負いません。" + endl;
            txt著作権免責.Text = st;

            tabCtrlMode.Focus();
            PB_GBoard.Focus();
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

        private void Window_Loaded( object sender, RoutedEventArgs e ){
            _Display_GB_GBoard( );       //メインボード設定
            _SetBitmap_PB_pattern();     //パターンスペース設定

            Lbl_onAnalyzer.Content   = "";
            Lbl_onAnalyzerM.Content  = "";
            Lbl_onAnalyzerTS.Content = "";
            Lbl_onAnalyzerTSM.Content = "";
            
            //===== 解法リスト設定 =====           
            GMethod00A.ItemsSource = GNP00.GetMethodListFromFile();
            NiceLoopMax.Value = GNumPzl.GMthdOption["NiceLoopMax"].ToInt();
            ALSSizeMax.Value  = GNumPzl.GMthdOption["ALSSizeMax"].ToInt();
            method_NLCell.IsChecked   = (GNumPzl.GMthdOption["Cell"]!="0");
            method_NLGCells.IsChecked = (GNumPzl.GMthdOption["GroupedCells"]!="0");
            method_NLALS.IsChecked    = (GNumPzl.GMthdOption["ALS"]!="0");

            WOpacityCC=0;
            startingTimer.Start();
        }
        private void appExit_Click( object sender, RoutedEventArgs e ){
            _Get_GNPXOptionPara();
            GNP00.MethodListOutPut();

            WOpacityCC=0;
            endingTimer.IsEnabled = true;
            endingTimer.Start();
        }
        private void Window_MouseDown( object sender, MouseButtonEventArgs e ){

/* ▼▼▼▼▼▼▼▼▼▼　公開版では削除　▼▼▼▼▼▼▼▼▼▼
            //===== 有効期限チェック =====
            if( !GNPZExtender.fileYMDCheckNew(new DateTime(2015,1,1)) ){
                WOpacityCC = 0;
                endingTimer.Start();
            }
*/

            if( e.Inner(PB_GBoard) )    return; 
            if( e.Inner(tabCtrlMode) )  return;
            this.DragMove();
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
    #endregion 開始・終了

    #region G9 手法選択
        private void GMethod02_Click( object sender, RoutedEventArgs e ){
            GMethod00A.ItemsSource = null;
            GMethod00A.ItemsSource = GNP00.ResetMethodList();
        }
        private void GMethod01U_Click( object sender, RoutedEventArgs e ){
            int nx = GMethod00A.SelectedIndex;
            if( nx< 0 || nx==0 )  return;
            GMethod00A.ItemsSource = null;
            GMethod00A.ItemsSource = GNP00.ChangeMethodList(nx,-1);
            GMethod00A.SelectedIndex = nx-1;
            GNP00.MethodListOutPut();
        }
        private void GMethod01D_Click( object sender, RoutedEventArgs e ){
            int nx = GMethod00A.SelectedIndex;
            if( nx<0 || nx==GMethod00A.Items.Count-1 )  return;
            GMethod00A.ItemsSource = null;
            GMethod00A.ItemsSource = GNP00.ChangeMethodList(nx,1);
            GMethod00A.SelectedIndex = nx+1;
            GNP00.MethodListOutPut();
        }
    #endregion 手法選択
 
    #region ファイルIO
        private string    fNameSDK;
        private void btnFileInputQ_Click( object sender, RoutedEventArgs e ){
            var OpenFDlog = new OpenFileDialog();
            OpenFDlog.Multiselect = false;
            OpenFDlog.Title  = "問題ファイル";
            OpenFDlog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if( (bool)OpenFDlog.ShowDialog() ){
                fNameSDK = OpenFDlog.FileName;
                GNP00.SDK_FileInput( fNameSDK, (bool)cbxProbInitialSet.IsChecked );
                txtFileName.Text = fNameSDK;

                _SetScreenProblem();
                GNP00._SDK_Ctrl_Initialize();

                btnProbPre.IsEnabled = (GNP00.CurrentPrbNo>=1);
                btnProbNxt.IsEnabled = (GNP00.CurrentPrbNo<GNP00.SDKProbLst.Count-1);
            }
        }
        private void btnFileOutputQ_Click( object sender, RoutedEventArgs e ){
            var SaveFDlog = new SaveFileDialog();
            SaveFDlog.Title = "問題ファイルのセーブ";
            SaveFDlog.FileName = fNameSDK;
            SaveFDlog.Filter = "テキストファイル(*.txt)|*.txt|全てのファイル(*.*)|*.*";
            
            GNumPzl.SlvMtdCList[0] = true;
            if( !(bool)SaveFDlog.ShowDialog() ) return;
            fNameSDK = SaveFDlog.FileName;
            bool append  = (bool)cbxAppend.IsChecked;
            bool fType81 = (bool)cbxFile81Nocsv.IsChecked;
            bool SolSort = (bool)cbxSolutionSort.IsChecked;
            bool SolSet  = (bool)cbxProbSolSetOutput.IsChecked;
            bool SolSet2 = (bool)cbxProbSolSetOutput2.IsChecked;

            if( GNP00.SDKProbLst.Count==0 ){
                if( GNP00.GNPX_Eng.GP.BDL.All(p=>p.No==0) ) return;
                GNP00.GNPX_Eng.GP.ID = GNP00.SDKProbLst.Count;
                GNP00.SDKProbLst.Add(GNP00.GNPX_Eng.GP);
                GNP00.CurrentPrbNo = 0;
                _SetScreenProblem();
            }
            GNP00.GNPX_Eng.Set_GNPZMethodList(true);  //true:全手法を用いる  
            GNP00.SDK_FileOutput( fNameSDK, append, fType81, SolSort, SolSet, SolSet2 );
        }
        private void btnFavfileOutputQ_Click( object sender, RoutedEventArgs e ){
            GNP00.btnFavfileOutput(true);
        }


        private void cbxProbSolSetOutput_Checked( object sender, RoutedEventArgs e ){
            cbxProbSolSetOutput2.IsEnabled = (bool)cbxProbSolSetOutput.IsChecked;
            Color cr = cbxProbSolSetOutput2.IsEnabled? Colors.White: Colors.Gray;
            cbxProbSolSetOutput2.Foreground = new SolidColorBrush(cr); 
        }

       //問題のcopy/paste (board<-->clipboard)
        private void Grid_PreviewKeyDown( object sender, KeyEventArgs e ){
            bool KeySft  = (Keyboard.Modifiers&ModifierKeys.Shift)>0;
            bool KeyCtrl = (Keyboard.Modifiers&ModifierKeys.Control)>0;

            if( e.Key==Key.C && KeyCtrl ){
                string st=GNP00.GNPX_Eng.GP.ToLineString();
                Clipboard.SetData(DataFormats.Text, st);
            }
            else if( e.Key==Key.F && KeyCtrl ){
                string st=GNP00.GNPX_Eng.GP.ToGridString(KeySft);           
                Clipboard.SetData(DataFormats.Text, st);
            }
            else if( e.Key==Key.V && KeyCtrl ){
                string st=(string)Clipboard.GetData(DataFormats.Text);
                if( st==null || st.Length<81 ) return ;
                var UP=GNP00.SDK_ToUProblem(st,saveF:true); 
                GNP00.CurrentPrbNo=99999;
                _SetScreenProblem();
                btnAnalyzerResetAll_Click( sender, e ); //解析結果クリア 
            }
        }
    #endregion ファイルIO
        
    #region  動作モード 
        private void tabCtrlMode_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            if( (TabControl)sender!=tabCtrlMode ) return;
            TabItem tb=tabCtrlMode.SelectedItem as TabItem;
            if( tb.Name.Substring(0,4)!="tabA" )  return;
            GNP00.GSmode = (string)tb.Header;    //Tab名 -> 動作モード

            switch(GNP00.GSmode){
                case "解析": sNoAssist = (bool)chbAnalyze00.IsChecked; break;
                case "問題作成":
                    TabItem tb2=tabAutoManual.SelectedItem as TabItem;
                    if( tb2==null )  return ;
                    if( (string)tb2.Header=="自動" )  sNoAssist=false;
                    else sNoAssist = (bool)chbAssist01.IsChecked;
                    break;

                case "オプション":
                    bool sAssist=true;
                    chbOpsAssist.IsChecked=sAssist;
                    sNoAssist=sAssist;
                    break;

                default: sNoAssist=false; break;
            }

            _Display_GB_GBoard();   
            tabSolver_SelectionChanged(sender,e);
        }
        
        private string _PreSelTabName="";
        private void tabSolver_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            TabItem tabItm = tabSolver.SelectedItem as TabItem;
            _Get_GNPXOptionPara();
            if( tabItm!=null ){
                if( tabItm.Name!="" &&  _PreSelTabName=="tabSelMethodSel" ){     
                    GNP00.MethodListOutPut();
                }
                _PreSelTabName = tabItm.Name;  
                SDK_Ctrl.MltAnsSearch = (tabItm.Name=="TC_SDK_MltiAnalyzer"); //複数解解析"
            }    
        }     
        
    #endregion  動作モード Tab

    #region 問題の選択、メインボード設定
        private void btnProbPre_Click( object sender, RoutedEventArgs e ){ _Get_PreNxtPrg(-1); }
        private void btnProbNxt_Click( object sender, RoutedEventArgs e ){ _Get_PreNxtPrg(+1); }
        private void _Get_PreNxtPrg( int pm ){
            int nn=GNP00.CurrentPrbNo +pm;
            if( nn<0 || nn>GNP00.SDKProbLst.Count-1 ) return;

            GNP00.CurrentPrbNo = nn;
            GNP00.GNPX_Eng.SDA.gNoPzAnalyzerReset(0,false);  //解析結果のみクリア
            GNP00.GNPX_Eng.SDA.SetBoardFreeB();
            GNP00.GNPX_Eng.AnalyzerCounterReset();
            _DGViewMethodCounterSet();
            _SetScreenProblem();
        
            AnalyzerCC = 0;
            btnAnalyzerCC.Content = "";
            lblAnalyzerResult.Text = "";

            Lbl_onAnalyzerTS.Content = "";
            Lbl_onAnalyzerTSM.Content = "";

            btnAnalyzerCCM.Content = "";
            lblAnalyzerResultM.Text = "";
            possibleTecsLst.Clear();
            possibleTecs.ItemsSource=null;
        }

        private void _SetScreenProblem( ){
            UProblem P = GNP00.GetCurrentProble( );
            _Display_GB_GBoard();
            if( P!=null ){
                txtProbNo.Text = (P.ID+1).ToString();
                txtProbName.Text = P.Name;
                nUDDifficultyLevel.Text = P.DifLevelT.ToString();
            
                int nP=0, nZ=0, nM=0, nn=P.ID;
                __Set_CellsPZMCount( ref nP, ref nZ, ref nM );

                btnProbPre.IsEnabled = (nn>0);
                btnProbNxt.IsEnabled = (nn<GNP00.SDKProbLst.Count-1);
                       
                _DGViewMethodCounterSet();
            }
        }     
    #endregion 問題の選択

    #region パターンの選択、パターン設定
        private void btnPatternAutoGen_Click( object sender, RoutedEventArgs e){
            GNP00.SDKCntrl.CellNumMax = (int)CellNumMax.Value;
            _GeneratePatternl(true);
            SDK_Ctrl.rxCTRL = 0;           //問題候補生成の初期化
        }
        private void btnPatClear_Click( object sender, RoutedEventArgs e ){
            GNP00.SDKCntrl.PatGen.GPat = new int[9,9];
            _SetBitmap_PB_pattern();  
        }     
        private void PB_pattern_MouseDown( object sender, MouseButtonEventArgs e ){
            _GeneratePatternl(false);
        }
        private void btnPatternImport_Click( object sender, RoutedEventArgs e ){
            int nn=GNP00.SDKCntrl.PatGen.patternImport( GNP00.GNPX_Eng.GP );
            labelPattern.Content = "問題セル数：" + nn.ToString();
            _SetBitmap_PB_pattern();
            SDK_Ctrl.rxCTRL = 0;           //問題候補生成の初期化
        }

        private void _GeneratePatternl( bool ModeAuto ){  /*G9*/        
            int patSel = patSelLst.Find(p=>(bool)p.IsChecked).Name.Substring(6,2).ToInt(); //パターン形
            int nn=0;
            if( ModeAuto ) nn=GNP00.SDKCntrl.PatGen.patternAutoMaker(patSel);
            else{
                Point pt=Mouse.GetPosition(PB_pattern);
                int row=0, col=0;
                if( __GetRCPositionFromPattern( pt,ref row,ref col) ){
                    nn=GNP00.SDKCntrl.PatGen.symmetricPattern(patSel,row,col,false);
                }
            }
            SDK_Ctrl.rxCTRL = 0;           //問題候補生成の初期化
            _SetBitmap_PB_pattern();
            labelPattern.Content = "問題セル数："+nn;
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
        private void _SetBitmap_PB_pattern( ){  //パターンスペース設定
            SDKGrp.GBPatternPaint( PB_pattern, GNP00.SDKCntrl.PatGen.GPat );
        }
    #endregion パターンの選択

    #region 表示
        private bool sNoAssist=false;
        private void _Display_GB_GBoard( ){  //メインボード設定
            UProblem P = GNP00.GNPX_Eng.GP;
            Lbl_onAnalyzer.Visibility = (GNP00.GSmode=="解析")? Visibility.Visible: Visibility.Hidden; 
            Lbl_onAnalyzerM.Visibility = Visibility.Visible; 
      
            SDKGrp.GBoardPaint( bmpGZero, P, GNP00.GSmode, sNoAssist );
            PB_GBoard.Source = bmpGZero;

            int nP=0, nZ=0, nM=0;
            __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
            txtProbNo.Text = (P.ID+1).ToString();
            txtProbName.Text = P.Name;
        }
        private void chbOpsAssist_Checked( object sender, RoutedEventArgs e ) {
            sNoAssist = (bool)chbOpsAssist.IsChecked;

            int nP=0, nZ=0, nM=0;
            if( __Set_CellsPZMCount(ref nP,ref nZ,ref nM) ) _Display_GB_GBoard( );
        }

        private bool __Set_CellsPZMCount( ref int nP, ref int nZ, ref int nM ){
            nP=nZ=nM=0;
            if( GNP00.GNPX_Eng==null )  return false;
            bool sAssist=GNP00.GNPX_Eng.SDA.cellPZMCounter(ref nP, ref nZ, ref nM);
            if( nP+nZ+nM>0 ){
                lblStepCounter.Content = "セル数 問:" + nP.ToString() +
                    "  解:" + nM.ToString("0#") + "  残:" + nZ.ToString("0#");
            }
            return ((nP+nM>0)&sAssist);
        }
        private void chbAnalyze00_Checked( object sender, RoutedEventArgs e ){
            if( bmpGZero==null )  return;
            sNoAssist = (bool)chbAnalyze00.IsChecked;
            _SetScreenProblem();
        }
        private void chbAssist01_Checked( object sender, RoutedEventArgs e ){
            sNoAssist = (bool)chbAssist01.IsChecked;
            _Display_GB_GBoard();　//(空き数字を表示)
        }

        private int    __GCCounter__=0;
        private int    _ProgressPer;
        private string __DispMode=null;

        private void displayTimer_Tick( object sender, EventArgs e ){
            _Display_GB_GBoard();

            //Console.WriteLine("displayTimer_Tick");
            
            switch(GNP00.GSmode){
                case "問題作成": _Display_CreateProblem(); break;

                case "複数解解析":
                case "解析":     _Display_AnalyzeProb(); break;
            }

            ResMemory.Content = "Memory:" + GC.GetTotalMemory( true ).ToString();            
            if( ((++__GCCounter__)%1000)==0 ){ GC.Collect(); __GCCounter__=0; }
        }
        private void _Display_CreateProblem( ){
            Mlttrial.Content = "試行回数：" + GNP00.SDKCntrl.LoopCC;
            MlttrialT.Content = "(累積:" + SDK_Ctrl.TLoopCC + ")";
            LSPattern.Content = "基本パターン：" + GNP00.SDKCntrl.PatternCC;
            gamGen05A.Content = "残り" + (_ProgressPer.ToString().PadLeft( 2 )) + " 問";

            UProblem pGP=GNP00.pGP;
            if( pGP!=null ){
                int nn=GNP00.SDKProbLst.Count;
                if( nn>0 ){
                    txtProbNo.Text = nn.ToString();
                    txtProbName.Text = GNP00.SDKProbLst.Last().Name;
                    nUDDifficultyLevel.Text = pGP.DifLevelT.ToString();
                }
            }

            TimeSpan ts = AnalyzerLap.Elapsed;
            string st = "";
            if( ts.TotalSeconds>1.0 ) st += ts.TotalSeconds.ToString( " 0.0" ) + " sec";
            else                      st += ts.TotalMilliseconds.ToString( " 0.0" ) + " msec";

            Lbl_onAnalyzerTS.Content = st;
            Lbl_onAnalyzerTSM.Content = st;
            Lbl_onAnalyzerTS3.Content = "経過時間：" + st;

            if( __DispMode!=null && __DispMode!="" ){
                _SetScreenProblem();
                if( __DispMode=="Canceled" )  Mlttrial.Content += " キャンセル";
                displayTimer.Stop();
                AnalyzerLap.Stop();
                btnP13MltStart.Content = "問題作成";
            }
            __DispMode="";
        }
        private void _Display_AnalyzeProb(){
            bool SearcEnd=false;
            if( __DispMode=="Canceled" ){
                Lbl_onAnalyzer.Foreground = Brushes.LightCoral; 
                Lbl_onAnalyzerM.Foreground = Brushes.LightCoral; 
                GNP00.MltSolSave = false;
                displayTimer.Stop();
                __DispMode="";
            }
            
            else if( __DispMode=="Complated" ){
                Lbl_onAnalyzer.Content = "解析完了";
                    SearcEnd=true;
                    if( SDK_Ctrl.MltAnsOption[4]==0 ) Lbl_onAnalyzerM.Content = "解析完了";
                    if( SDK_Ctrl.MltAnsOption[4]==1 ) Lbl_onAnalyzerM.Content = "探索数上限打切り";
                    if( SDK_Ctrl.MltAnsOption[4]==2 ) Lbl_onAnalyzerM.Content = "探索時間上限打切り";
                    if( SDK_Ctrl.MltAnsOption[4]>0  ) Lbl_onAnalyzerM.Foreground = Brushes.Orange;
                    else Lbl_onAnalyzerM.Foreground = Brushes.LightBlue;  

                    btnMltAnlzSearch.Content = "複数解探索";
                    btnMltAnlzSearch.IsEnabled = true;
                Lbl_onAnalyzer.Foreground = Brushes.LightBlue;   
 
                _DGViewMethodCounterSet();  //手法の集計
                string msgST = GNP00.GNPX_Eng.GP.GNPZ_ResultLong;
                lblAnalyzerResult.Text = msgST;
                if( msgST.LastIndexOf("ルール違反")>=0 || msgST.LastIndexOf("解析不能")>=0 ){ }
                displayTimer.Stop();
                __DispMode="";
            }
            else{
                lblAnalyzerResult.Text = GNPX_Engin.GNPZ_AnalyzerMessage;
                Lbl_onAnalyzerM.Content = "解析中："+GNPX_Engin.GNPZ_AnalyzerMessage;
            }        

            lblAnalyzerResultM.Text=GNP00.GNPX_Eng.GP.GNPZ_ResultLong;        
            possibleTecs.ItemsSource=null;

            if( SDK_Ctrl.MltAnsSearch && SDK_Ctrl.GPMX!=null && SDK_Ctrl.GPMX.MltUProbLst!=null ){
                List<UProblem> pMltUProbLst=SDK_Ctrl.GPMX.MltUProbLst;

                try{
                    if( pMltUProbLst!=null && pMltUProbLst.Count>0 ){
                        possibleTecsLst.Clear();
 
                        int sq=0;
                        pMltUProbLst.ForEach(P=> possibleTecsLst.Add(new MltTec(P,++sq)) );
                        possibleTecs.ItemsSource = possibleTecsLst;
                        if(!SearcEnd) possibleTecs.ScrollIntoView(possibleTecsLst.Last());
                        else          possibleTecs.ScrollIntoView(possibleTecsLst.First());
                        int selX=SDK_Ctrl.GPMX.selX;
                        if( selX>=0 && selX<pMltUProbLst.Count ){
                            lblAnalyzerResultM.Text = pMltUProbLst[selX].GNPZ_ResultLong; 
                        }
                    }
                }
                catch( Exception e ){
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            string st="";   
            TimeSpan ts2 = GNPX_Engin.SdkExecTime;
            TimeSpan ts = AnalyzerLap.Elapsed;
            if( ts.TotalSeconds>1.0 )  st=ts.TotalSeconds.ToString("0.000")+" sec";
            else                       st=ts.TotalMilliseconds.ToString("0.000")+" msec";

            Lbl_onAnalyzerTS.Content  = st;
            Lbl_onAnalyzerTSM.Content  = st;
            Lbl_onAnalyzerTS3.Content = "経過時間："+st;
                        
            btnSDKAnalyzer.Content     = "解　析";
            btnMltAnlzSearch.Content   = "複数解探索";
            btnSDKAnalyzerAuto.Content = "自動解析";

            if( GNPX_Engin.GNPZ_AnalyzerMessage.Contains("sys") ){
                lblAnalyzerResultM.Text = GNPX_Engin.GNPZ_AnalyzerMessage;
            }

            this.Cursor = Cursors.Arrow;

            _DGViewMethodCounterSet();
            _SetScreenProblem();
 
            OnWork = 0;
        }
    #endregion 表示

    #region マウスＩＦ
        //***** 制御変数
        private int     noPChg = -1;
        private int[]   noPChgList = new int[9];
        private int     rowMemo; 
        private int     colMemo;
        private int     noPMemo;
        private bool    mouseFlag = false;

        private void PB_GBoard_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ){  
            if( mouseFlag ) return;
            if( GNP00.GSmode!="問題作成" && GNP00.GSmode!="数字変更" )  return;

            int r, c;
            int noP = _Get_PB_GBoardRCNum( out r, out c );
            if( noP<=0 ){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }
            rowMemo=r; colMemo=c;       
            mouseFlag = true;
            if( GNP00.GSmode=="数字変更" ) return;

            if( GNP00.GSmode!="問題作成" ){
                if( GNP00.GNPX_Eng.GP.BDL[r*9+c].No > 0 ) return;
            }

            rowMemo=r; colMemo=c; noPMemo=noP;
            _GNumericPadManager( r, c, noP );  
        }
        private void PB_GBoard_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ){
            if( !mouseFlag ) return;
            mouseFlag = false;

            int noP=0;
            if( GNP00.GSmode=="数字変更" ){ _Change_PB_GBoardNum( ref noP ); return; }
        }

        private void _GNumericPadManager( int r, int c, int noP ){
            noPMemo = noP;
            int FreeB=0x1FF;
            if( GNP00.GSmode=="問題作成"  ){
                FreeB = GNP00.GNPX_Eng.GP.BDL[r*9+c].FreeB;   //1:選択可能な数字
            }

            GnumericPad.Source = SDKGrp.CreateCellImageLight( GNP00.GNPX_Eng.GP.BDL[r*9+c], noP );
 
            int PosX = (int)PB_GBoard.Margin.Left + 2 + 37*c + (int)c/3;
            int PosY = (int)PB_GBoard.Margin.Top  + 2 + 37*r + (int)r/3;        
            GnumericPad.Margin = new Thickness(PosX, PosY, 0,0 );        
            GnumericPad.Visibility = Visibility.Visible;
            
        }       
        private void GnumericPad_MouseMove( object sender, MouseEventArgs e ){
            if( !mouseFlag ) return;
            int r, c;
             
            if( GNP00.GSmode=="数字変更" ) return;
            int noP  = _Get_PB_GBoardRCNum( out r, out c );
            if( noP<=0 || r!=rowMemo || c!=colMemo ){
                GnumericPad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }

            if( GNP00.GSmode!="問題作成" ){
                if( GNP00.GNPX_Eng.GP.BDL[r*9+c].No > 0) return;
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
            if( GNP00.GSmode=="数字変更" ){ _Change_PB_GBoardNum( ref noP ); return; }
            //if( noP <= 0 ){ justNum = -1;  return; }
            */
            UCell BDX = GNP00.GNPX_Eng.GP.BDL[rowMemo*9+colMemo];

            int numAbs = Math.Abs(BDX.No);
            if( numAbs==noP ){ BDX.No=0; goto MouseUpFinary; }

            int FreeB = BDX.FreeB;   //1:選択可能な数字
            if( GNP00.GSmode=="問題作成" ){
                BDX.No=0;
                GNP00.GNPX_Eng.SDA.SetBoardFreeB();
                FreeB = BDX.FreeB;
                if( ((FreeB>>(noP-1))&1)==0 ) goto MouseUpFinary;
                BDX.No=noP;
            }
          
          MouseUpFinary:
            GNP00.GNPX_Eng.SDA.SetBoardFreeB();
            _SetScreenProblem();
            GnumericPad.Visibility = Visibility.Hidden;
            rowMemo=-1; colMemo=-1;

            int nP=0, nZ=0, nM=0;
            __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
        }
        private void cellPZMCounterForm( ref int nP, ref int nZ, ref int nM ){
            GNP00.GNPX_Eng.SDA.cellPZMCounter( ref nP, ref nZ, ref nM);
            if( nP+nZ+nM > 0 ){
                lblStepCounter.Content = "セル数 問:" + nP.ToString() +
                    "  解:" + nM.ToString("0#") + "  残:" + nZ.ToString("0#");
            }
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
    #endregion マウスＩＦ 

    #region 問題作成
      #region 問題作成【マニュアル】
        private void btnBoardClear_Click( object sender, RoutedEventArgs e ){
            for( int rc=0; rc<81; rc++ ){ GNP00.GNPX_Eng.GP.BDL[rc] = new UCell(rc); }
            _SetScreenProblem();　//(空き数字を表示)
        }
        private void btnNewProblem_Click( object sender, RoutedEventArgs e ){
            UProblem pGP= GNP00.GNPX_Eng.GP;
            if( pGP.BDL.All(P=>P.No==0) ) return;
            GNP00.SDK_Save_ifNotContain();
            GNP00.CreateNewPrb();//新しい問題用の領域を確保する
            _SetScreenProblem();　//(空き数字を表示)
        }
        private void btnDeleteProblem_Click( object sender, RoutedEventArgs e ){
            GNP00.SDK_Remove();
            _SetScreenProblem();　//(空き数字を表示)
        }
        
        private void btnCopyProblem_Click(object sender, RoutedEventArgs e) {
            UProblem UPcpy= GNP00.GNPX_Eng.GP.Copy();
            UPcpy.Name="コピー";
            GNP00.CreateNewPrb(UPcpy);//新しい問題用の領域を確保する
            _SetScreenProblem();　//(空き数字を表示)
        }

        #region 数字変更
        private void btnNumChange_Click( object sender, RoutedEventArgs e ){
             if( GNP00.GSmode!="数字変更" ){
                GNP00.GSmode = "数字変更";
                grpManual.Visibility = Visibility.Hidden;
                txNumChange.Text = "1";
                txNumChange.Visibility = Visibility.Visible;
                btnNumChangeFix.Visibility = Visibility.Visible;
                noPChg = 1;
                for( int k=0; k<9; k++ ) noPChgList[k] = k+1;
                mouseFlag = false;
                PB_GBoard.IsEnabled = true;
            }
        }
        private void btnNumChangeFix_Click( object sender, RoutedEventArgs e ){
            GNP00.GSmode = "問題作成";
            grpManual.Visibility = Visibility.Visible;
            txNumChange.Visibility = Visibility.Hidden;
            btnNumChangeFix.Visibility = Visibility.Hidden;
            noPChg = -1;
        }
        private void _Change_PB_GBoardNum( ref int noP ){
            int nm, nmAbs;
            if( rowMemo<0 || rowMemo>8 || colMemo<0 || colMemo>8) return;

            noP = Math.Abs( GNP00.GNPX_Eng.GP.BDL[rowMemo*9+colMemo].No );
            if( noP==0 )  return;
            if( noP!=noPChg ){

                foreach( var q in GNP00.GNPX_Eng.GP.BDL ){
                    nm = q.No;
                    if( nm==0 )  continue;
                    nmAbs = Math.Abs( nm );
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
                GNP00.GSmode = "問題作成";
                grpManual.Visibility = Visibility.Visible;
                txNumChange.Visibility = Visibility.Hidden;
                btnNumChangeFix.Visibility = Visibility.Hidden;
                noPChg = -1;
            }
            else GNP00.GSmode = "数字変更";
            mouseFlag = false;
            return;
        }
        private void _SetGSBoad_rc_num( int r, int c, int noP ){
            if( r<0 || r>=9 ) return;
            if( c<0 || c>=9 ) return;
            int numAbs = Math.Abs(noP);
            if( numAbs==0 || numAbs>=10) return;
            GNP00.GNPX_Eng.GP.BDL[r*9+c].No=noP;
        }
        #endregion 数字変更  
 
        private void btnTrans_Click( object sender, RoutedEventArgs e ) {
            Button btn = sender as Button;
            GNP00.SDK_TransProb(btn.Name);
            _SetScreenProblem();
        }

      #endregion 問題作成【マニュアル】

      #region 問題作成【自動】
        //開始
        private Task taskSDK;
        private CancellationTokenSource tokSrc;
        private void btnP13Start_Click( object sender, RoutedEventArgs e ){
        //    int mc=GNP00.SDKCntrl.GNPX_Eng.Set_GNPZMethodList( );
        //    if( mc<=0 ) GNP00.ResetMethodList();

            if( (string)btnP13MltStart.Content=="問題作成" ){
                __DispMode=null;
                GNP00.SDKCntrl.LoopCC = 0;
                btnP13MltStart.Content  = "中  断";

                GNPX_Engin.SolInfoDsp = false;
                if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC = 0;
                GNP00.SDKCntrl.CbxDspNumRandmize = (bool)cbxDspNumRandmize.IsChecked;//数字の乱数化
                GNP00.SDKCntrl.GenLStyp = int.Parse(GenLStyp.Text);
                GNP00.SDKCntrl.CbxNextLSpattern  = (bool)ChbNextLSpattern.IsChecked;
                
                SDK_Ctrl.lvlLow = (int)gamGen01.Value;
                SDK_Ctrl.lvlHgh = (int)gamGen02.Value;

                int n=gamGen05.Text.ToInt();
                n = Math.Max(Math.Min(n,1000),0); 
                SDK_Ctrl.MltProblem = _ProgressPer = n;
                GNP00.MltSolSave = true;

                displayTimer.Start();
                AnalyzerLap.Start();

                tokSrc = new CancellationTokenSource();　//中断時の手続き用  
                taskSDK = new Task( ()=> GNP00.SDKCntrl.SDK_ProblemMakerReal(tokSrc.Token), tokSrc.Token );
                taskSDK.ContinueWith( t=> btnP13Start2Complated() ); //完了時の手続きを登録
                taskSDK.Start();
            }
            else{   //"中断"
                try{
                    tokSrc.Cancel();
                    taskSDK.Wait();
                }
                catch(AggregateException){
                    __DispMode="Canceled"; 
                }
            }
            return;
        }  
        //プログレス表示
        public void BWGenPrb_ProgressChanged( object sender, SDKEventArgs e ){ _ProgressPer=e.ProgressPer; }
        //完了
        private void btnP13Start2Complated( ){ __DispMode="Complated"; }

        private void gamGen01_ValueChanged( object sender, RoutedPropertyChangedEventArgs<object> e ){
            if( gamGen02==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Lval>Uval ) gamGen02.Value=Lval;
        }
        private void gamGen02_ValueChanged( object sender, RoutedPropertyChangedEventArgs<object> e ){
            if( gamGen01==null )  return;
            int Lval=(int)gamGen01.Value, Uval=(int)gamGen02.Value;
            if( Uval<Lval ) gamGen01.Value=Uval;
        }
      #endregion 問題作成【自動】

    #endregion 問題作成 

    #region 解析
      //【注意】task,ProgressChanged,Completed,CanceledのthreadSafeに注意（control操作禁止）
      #region 解析【ステップ】複数解解析
        private int OnWork = 0;

        private void btnSDKAnalyzer_Click( object sender, RoutedEventArgs e ){
            if( OnWork==2 ) return;

            if( GNP00.AnalyzerMode==null ) GNP00.AnalyzerMode = "解析";
            if( !SDK_Ctrl.MltAnsSearch )  SDK_Ctrl.GPMX=null;

            try{
                Lbl_onAnalyzer.Foreground = Brushes.LightGreen;
                Lbl_onAnalyzerM.Foreground = Brushes.LightGreen;
                if( (string)btnSDKAnalyzer.Content!="中　断" ){
                    int mc=GNP00.GNPX_Eng.Set_GNPZMethodList( );
                    if( mc<=0 ) GNP00.ResetMethodList();
                    Lbl_onAnalyzer.Visibility = Visibility.Visible;
                    Lbl_onAnalyzerM.Visibility = Visibility.Visible;

//                  GNPX_Engin.SolInfoDsp = false;
                    if( GNP00.SDKCntrl.retNZ==0 )  GNP00.SDKCntrl.LoopCC=0;

                    SDK_Ctrl.MltProblem = 1;    //単独
                    SDK_Ctrl.lvlLow = 0;
                    SDK_Ctrl.lvlHgh = 999;
                    GNP00.SDKCntrl.CbxDspNumRandmize=false;
                    GNP00.SDKCntrl.GenLStyp = 1;

                    GNumPzl.MltSolOn = (bool)MltSolOn.IsChecked;
                    GNPX_Engin.SolInfoDsp = true;
                    AnalyzerLap.Reset();

                    if( GNP00.AnalyzerMode=="解析" || GNP00.AnalyzerMode=="複数解解析" ){
                        _cellFixSub( );
                        List<UCell> pBDL = GNP00.GNPX_Eng.GP.BDL;
                        if( pBDL.Count(p=>p.No==0)==0 ){            //解析完了
                            _SetScreenProblem();
                            goto AnalyzerEnd;
                        }
                        if( pBDL.Any(p=>(p.No==0 && p.FreeB==0)) ){ //解なし
                            lblAnalyzerResult.Text = "候補数字がなくなったセルがある\r作成問題の場合はバグ";
                            goto AnalyzerEnd;
                        }

                        OnWork = 1;    
                        btnAnalyzerCC.Content= "ステップ "+(++AnalyzerCC).ToString();
                        btnAnalyzerCCM.Content= btnAnalyzerCC.Content;
                        GNPZ_Analyzer.pAnalyzerCC = AnalyzerCC;
                        btnSDKAnalyzer.Content = "中　断";
                        btnMltAnlzSearch.Content = "中　断";
                        Lbl_onAnalyzer.Content = "解析中";
                        Lbl_onAnalyzerM.Content = "解析中";
                        Lbl_onAnalyzer.Foreground=Brushes.Orange;
                        Lbl_onAnalyzerM.Foreground=Brushes.Orange;
                        Lbl_onAnalyzerTS.Content = "";
                        Lbl_onAnalyzerTSM.Content = "";
                        this.Cursor = Cursors.Wait;

                        __DispMode="";
                        displayTimer.Start();
                        AnalyzerLap.Start();
                        //==============================================================
                        tokSrc = new CancellationTokenSource();　//中断時の手続き用  
                        taskSDK = new Task( ()=> GNP00.SDKCntrl.AnalyzerReal(tokSrc.Token), tokSrc.Token );
                        taskSDK.ContinueWith( t=> task_SDKsolver_Completed() ); //完了時の手続きを登録
                        taskSDK.Start();
                        //--------------------------------------------------------------         
                    }
                    else{   //"中断"
                        try{
                            tokSrc.Cancel();
                            taskSDK.Wait(); 
                        }
                        catch(AggregateException e2){
                            Console.WriteLine(e2.Message);
                            __DispMode="Canceled";
                        }
                    }
 
                AnalyzerEnd:
                    //GNPX_Engin.ChkPrint = false;    //*****
                    return;
                }

            }
            catch( Exception ex ){
                Console.WriteLine( ex.Message );
                Console.WriteLine( ex.StackTrace );
            }

        } 
            
        public  class MltTec{
            public int    ID{ get; set; }
            public int    difL{ get; set; }
            public string tech{ get; set; }
            public MltTec(UProblem P, int ID ){ difL=P.difLevel; tech=P.GNPZ_Result; this.ID=ID; }
        }
        private List<MltTec> possibleTecsLst=new List<MltTec>();

        private void task_SDKsolver_ProgressChanged( object sender, SDKEventArgs e ){ _ProgressPer=e.ProgressPer; }
        private void task_SDKsolver_Completed(){
            __DispMode = "Complated";
        }
        private void btnAnalyzerReset_Click( object sender, RoutedEventArgs e ){
            if( OnWork>0 ) return;
            AnalyzerCC = AnalyzerCCMemo;
            btnAnalyzerCC.Content= "ステップ "+(AnalyzerCC).ToString();
            btnAnalyzerCCM.Content=btnAnalyzerCC.Content;
            GNP00.GNPX_Eng.SDA.gNoPzAnalyzerReset( 1, (bool)MltSolOn.IsChecked );    //解析結果のみクリア
            btnSDKAnalyzer.Content = "解  析";
            btnMltAnlzSearch.Content = "複数解探索";

            Lbl_onAnalyzer.Content = "";
            lblAnalyzerResult.Text = "";
            lblAnalyzerResultM.Text = "";
            Lbl_onAnalyzerTS.Content  = "";
            Lbl_onAnalyzerTSM.Content  = "";
            Lbl_onAnalyzerTS3.Content = "経過時間：";
            GNP00.GNPX_Eng.cCodePre = 0;
            GNP00.GNPX_Eng.SDA.SetBoardFreeB();

            GNP00.GNPX_Eng.AnalyzerCounterReset();
            _DGViewMethodCounterSet();
            
            displayTimer.Stop();
            _SetScreenProblem();
        }

        private void btnMltAnlzSearch_Click( object sender, RoutedEventArgs e ){
            if( SDK_Ctrl.GPMX==null ) SDK_Ctrl.GPMX=new UPrbMltMan();
            else{
                SDK_Ctrl.GPMX.selX = possibleTecs.SelectedIndex;
                SDK_Ctrl.GPMX.Create();
            }
            possibleTecsLst.Clear();
            lblAnalyzerResultM.Text="";

            SDK_Ctrl.MltAnsOption[0] = (int)MltAnsOpt0.Value; //レベル
            SDK_Ctrl.MltAnsOption[1] = (int)MltAnsOpt1.Value; //複数解上限
            SDK_Ctrl.MltAnsOption[2] = (int)MltAnsOpt2.Value; //探索時間上限
            SDK_Ctrl.MltAnsOption[3] = (int)(DateTime.Now-(DateTime.Today.AddHours(-1))).TotalSeconds;
            SDK_Ctrl.MltAnsOption[4] = 0;

            btnMltAnlzSearch.IsEnabled = false;
            btnSDKAnalyzer_Click(sender,e);
        }
        private void possibleTecs_SelectionChanged( object sender, SelectionChangedEventArgs e ){
            if( SDK_Ctrl.GPMX==null )  return;
            List<UProblem> pMltUProbLst=SDK_Ctrl.GPMX.MltUProbLst;
            if( !GNumPzl.MltSolOn || pMltUProbLst==null ) return;
            if( pMltUProbLst.Count<=0 )  return;                                                                                                                                       
            int selX=possibleTecs.SelectedIndex;
            if( selX<0 || selX>=pMltUProbLst.Count )  return;

            SDK_Ctrl.GPMX.selX = selX;
            GNP00.GNPX_Eng.GP = pMltUProbLst[selX];
            lblAnalyzerResultM.Text = pMltUProbLst[selX].GNPZ_ResultLong;
        }
     
        private void btnMPre_Click( object sender, RoutedEventArgs e ){
            if( SDK_Ctrl.GPMX==null )  return;
            SDK_Ctrl.GPMX.MovePre();
            if( SDK_Ctrl.GPMX==null )  return;
            List<UProblem> pMltUProbLst=SDK_Ctrl.GPMX.MltUProbLst;
            if( !GNumPzl.MltSolOn || pMltUProbLst==null ) return;
            possibleTecs.SelectedIndex = SDK_Ctrl.GPMX.selX;
            _Display_AnalyzeProb();
        }

        private void btnMNxt_Click( object sender, RoutedEventArgs e ){
            if( SDK_Ctrl.GPMX==null )  return ;
            SDK_Ctrl.GPMX.MoveNxt();
            if( SDK_Ctrl.GPMX==null )  return;
            List<UProblem> pMltUProbLst=SDK_Ctrl.GPMX.MltUProbLst;
            if( !GNumPzl.MltSolOn || pMltUProbLst==null ) return;
            possibleTecs.SelectedIndex = SDK_Ctrl.GPMX.selX;
            _Display_AnalyzeProb();
        }

     #endregion  解析【ステップ】複数解解析       
    
      #region 解析【全て】
        private void task_SDKsolverAuto_ProgressChanged( object sender, ProgressChangedEventArgs e ){
            Mlttrial.Content = "試行回数：" + GNP00.SDKCntrl.LoopCC;
            LSPattern.Content = "基本パターン：" + GNP00.SDKCntrl.PatternCC;
            btnSDKAnalyzerAuto.Content = "自動解析";
            OnWork=0;
        }
        private void task_SDKsolverAuto_Completed( ){ 
            displayTimer.Start(); __DispMode="Complated";
        }

        private void btnSDKAnalyzerAuto_Click( object sender, RoutedEventArgs e ){
            if( OnWork==1 ) return;

            if( (string)btnSDKAnalyzerAuto.Content=="中　断" ){
                tokSrc.Cancel();
                try{ taskSDK.Wait(); }
                catch(AggregateException){ __DispMode="Canceled"; }
                displayTimer.Start();
                OnWork = 0;
            }
            else{
                List<UCell> pBDL = GNP00.GNPX_Eng.GP.BDL;
                if( pBDL.Count(p=>p.No==0)==0 ){             //解析完了
                    _SetScreenProblem();
                    goto AnalyzerEnd;
                }
                if( pBDL.Any(p=>(p.No==0 && p.FreeB==0)) ){ //解なし
                    lblAnalyzerResult.Text = "候補数字がなくなったセルがある";
                    goto AnalyzerEnd;
                }

                OnWork = 2;
                btnSDKAnalyzerAuto.Content = null;
                btnSDKAnalyzerAuto.Content = "中　断";          
                Lbl_onAnalyzer.Content  = "解析中";
                Lbl_onAnalyzer.Foreground = Brushes.Orange;               

                int mc=GNP00.GNPX_Eng.Set_GNPZMethodList( );
                if( mc<=0 ) GNP00.ResetMethodList(); 
                
                btnAnalyzerResetAll_Click( sender, e ); //解析結果クリア 
                GNP00.GNPX_Eng.AnalyzerCounterReset(); 

                GNPX_Engin.SolInfoDsp = false;
                SDK_Ctrl.lvlLow = 0;
                SDK_Ctrl.lvlHgh = 999;
//              GNPX_Engin.SolInfoDsp = true;

                //==============================================================
                tokSrc = new CancellationTokenSource();　//中断時の手続き用
                CancellationToken ct = tokSrc.Token;   
                taskSDK = new Task( ()=> GNP00.SDKCntrl.AnalyzerRealAuto(ct), ct );
                taskSDK.ContinueWith( t=> task_SDKsolverAuto_Completed() ); //完了時の手続きを登録
                AnalyzerLap.Reset(); 
                taskSDK.Start();
                //--------------------------------------------------------------
   
                this.Cursor = Cursors.Wait;

                AnalyzerLap.Start();
                displayTimer.Start();
          //      if( (bool)AutoSolDisp.IsChecked ) displayTimer.Start();

              AnalyzerEnd:
                //GNPX_Engin.ChkPrint = false;    //*****
                return;

            }
        }
        private void btnAnalyzerResetAll_Click( object sender, RoutedEventArgs e ){
            if( OnWork>0 ) return;
            AnalyzerCC = 0;
            btnAnalyzerCC.Content= "";
            GNP00.GNPX_Eng.SDA.gNoPzAnalyzerReset( 0, false );    //解析結果のみクリア
            btnSDKAnalyzerAuto.Content  = "自動解析";
            lblAnalyzerResult.Text    = "";
            Lbl_onAnalyzer.Content    = "";         
            Lbl_onAnalyzerTS.Content  = "";

            btnAnalyzerCCM.Content    = "";
            Lbl_onAnalyzerM.Content   = "";
            Lbl_onAnalyzerTSM.Content = "";
            lblAnalyzerResultM.Text   = "";

            btnMltAnlzSearch.IsEnabled=true;

            possibleTecsLst.Clear();
            possibleTecs.ItemsSource = null;

            Lbl_onAnalyzerTS3.Content = "経過時間：";
            GNP00.GNPX_Eng.cCodePre   = 0;
            GNP00.GNPX_Eng.SDA.SetBoardFreeB();

            GNP00.GNPX_Eng.AnalyzerCounterReset();
            SDK_Ctrl.GPMX=null;

            _DGViewMethodCounterSet();

            displayTimer.Stop();
            _SetScreenProblem();
        }
      #endregion 解析【全て】

      #region 解析【手法集計】
        public int AnalyzerCC=0;
        private int AnalyzerCCMemo=0;
        private int AnalyzerMMemo=0;   
        private void _cellFixSub(  ){
            if( GNP00.pGP.SolCode<= 0) return;
            GNP00.GNPX_Eng.SDA.GNPZ_NumberFix( );

            if( GNP00.pGP.SolCode==-999 ){
                lblAnalyzerResult.Text = "手法制御のエラー";
                GNP00.pGP.SolCode = -1;
            }

            int nP=0, nZ=0, nM=0;
            cellPZMCounterForm( ref nP, ref nZ, ref nM );
            if( nM!=AnalyzerMMemo ){
                AnalyzerCCMemo = AnalyzerCC;
                AnalyzerMMemo = nM;
            }

            if( nZ==0 && (bool)cbxFileDifficultyLevel.IsChecked ){
                string prbMessage;
                int lvlBase;
                int DifLevelT = GNP00.GNPX_Eng.DifficultyLevelChecker( out prbMessage, out lvlBase );
                GNP00.GNPX_Eng.GP.DifLevelT = DifLevelT;
                nUDDifficultyLevel.Text = DifLevelT.ToString();
            }
        }
        private List<MethodCounter> MCList;
        private void _DGViewMethodCounterSet(){ //手法の集計
            MCList = new List<MethodCounter>();

            foreach( var P in GNPX_Engin.SDK_MethodsRun ){
                if( P.UCount <= 0 )  continue;
                MCList.Add( new MethodCounter( P.MethodName, P.UCount ) );
            }
            DGViewMethodCounter.ItemsSource = MCList;
            if( MCList.Count>0 )  DGViewMethodCounter.SelectedIndex=-1;

            if( GNP00.GSmode=="解析" && MCList.Count>0 && DGViewMethodCounter.Columns.Count>1 ){
                //http://msdn.microsoft.com/ja-jp/library/ms745683.aspx
                //http://social.msdn.microsoft.com/Forums/ja/csharpgeneralja/thread/6a3160d7-8ce0-461b-89e2-9b20e1ff31d6
                //【Att】HorizontalAlignment.Centerとすると、不要な縦線が現れる 現象・原因・対処方法不明
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

      #endregion 解析【手法集計】 
    #endregion 解析

    #region 印刷
        private int SDKpage=0;
        private int SDKpageMax;
        private void btnPageSetup_Click( object sender, RoutedEventArgs e ){ }
#if false //▼
        private void btnPrint_Click( object sender, RoutedEventArgs e ){
            SDK_ProblemPrinter PP = new SDK_ProblemPrinter();
            int mLow  = (int)prnt02A.Value;
            int mHigh = (int)prnt02B.Value;
            int mStart= (int)prnt04A.Value;
            int mEnd  = (int)prnt04B.Value;
            if( (bool)prnt01.IsChecked ){  mLow=0; mHigh=999; }
            if( (bool)prnt03.IsChecked ){  mStart=0; mEnd=9999; }
            PP.SDK_PrintDocument( GNP00, mLow, mHigh, mStart, mEnd, (bool)prnt05.IsChecked );
        }
#endif
/*
        private void btnPageSetup_Click( object sender, RoutedEventArgs e ){
            SDKpageSetupDialog.PageSettings = new System.Drawing.Printing.PageSettings();
            SDKpageSetupDialog.ShowDialog();
        }
*/
/*
        private void btnPrintPreview_Click( object sender, RoutedEventArgs e ){
            int mLow  = (int)prnt02A.Value;
            int mHigh = (int)prnt02B.Value;
            int mStart= (int)prnt04A.Value;
            int mEnd  = (int)prnt04B.Value;
            int lvl;

            SDKpage = 0;
            int n = 0;
            foreach( UProblem gpx in GNP00.SDKProbLst ){
                n++;
                lvl = gpx.DifLevelT;
                if( (bool)prnt02.IsChecked && (lvl<mLow || lvl>mHigh)) continue;
                if( (bool)prnt04.IsChecked && (n<mStart || n>mEnd)) continue;
                SDKpage++;
            }
            SDKpageMax = SDKpage;

            SDKpage = 1;
            SDKprintPreviewDialog.ShowDialog();
        }
*/
/*
        private void btnPrint_Click( object sender, RoutedEventArgs e ){
//            printDialog1.PrinterSettings = new System.Drawing.Printing.PrinterSettings();
//            printDialog1.AllowCurrentPage = true;
//            var ret = printDialog1.ShowDialog();
            int mLow  = (int)prnt02A.Value;
            int mHigh = (int)prnt02B.Value;
            int mStart= (int)prnt04A.Value;
            int mEnd  = (int)prnt04B.Value;
            int lvl;

            SDKpage = 0;
            int n = 0;
            foreach( UProblem gpx in GNP00.SDKProbLst ){ 
                n++;
                lvl = gpx.DifLevelT;
                if( (bool)prnt02.IsChecked && (lvl<mLow || lvl>mHigh)) continue;
                if( (bool)prnt04.IsChecked && (n<mStart || n>mEnd))    continue;
                SDKpage++;
            }
            SDKpageMax = SDKpage;

            SDKpage = 1;

            var Vis = SDK_PrintDocument();
            VisualPrintDialog printDlg = new VisualPrintDialog( Vis );
            printDlg.ShowDialog();
//            }
        }
*/
/*              
    //    private void SDKprintDocument_PrintPage( object sender, PrintPageEventArgs e ){
        private Visual SDK_PrintDocument( ){
            Point  pt0=new Point(0,0);
            Point  pt1;
            string po;

            GFont gFnt12 = new GFont("ＭＳ　ゴシック",12,FontWeights.Medium,FontStyles.Normal);
            GFont gFnt16 = new GFont("ＭＳ　ゴシック",16,FontWeights.Medium,FontStyles.Normal);
            GFont gFnt20 = new GFont("ＭＳ　ゴシック",20,FontWeights.Medium,FontStyles.Normal);
            GFormattedText GF12 = new GFormattedText( gFnt12 );
            GFormattedText GF16 = new GFormattedText( gFnt16 );
            GFormattedText GF20 = new GFormattedText( gFnt20 );

            var brsh = new SolidColorBrush(Colors.Black);    //フリー(DarkBlue)

            int mLow  = (int)prnt02A.Value;
            int mHigh = (int)prnt02B.Value;
            int mStart= (int)prnt04A.Value;
            int mEnd  = (int)prnt04B.Value;
            int lvl;

            int prbCC = 0;
            int SDKpageP = SDKpage+3;

            List<UProblem> SDKPList = new List<UProblem>();
            foreach( var p in GNP00.SDKProbLst ){
                lvl = p.DifLevelT;
                if( (bool)prnt02.IsChecked && (lvl<mLow || lvl>mHigh)) continue;
                int n = p.pNumber;
                if( (bool)prnt04.IsChecked && (n<mStart || n>mEnd))    continue;
                SDKPList.Add(p);
            }
            if( (bool)prnt05.IsChecked ) SDKPList.Sort( (pa,pb)=>(pa.DifLevelT-pb.DifLevelT) );
            
            var drwVis = new DrawingVisual();

            DrawingContext DC=drawVisual.RenderOpen();
            foreach( UProblem px in SDKPList ){
                prbCC++;
              {
                    
                    switch( prbCC%4 ){
                        case 1: pt0 = new Point(60,0); break;
                        case 2: pt0 = new Point(440,0); break;
                        case 3: pt0 = new Point(60,500); break;
                        case 0: pt0 = new Point(440,500); break;
                    }
                    
                    SDKGrp.GBoardPPrint( drwVis, px, crList );
                    if( (prbCC%4)==1 ){
                        po = "GSD light v.0.9";    
                        DC.DrawText( GF20.GFText(po,brsh), new Point(20,10) );   
                    }
                    po = px.pNumber.ToString() + " " + px.name;    
                    pt1 = new Point(pt0.X,pt0.Y+145); 
                    DC.DrawText( GF20.GFText(po,brsh), pt1 );   

                    po = "難易度："+px.DifLevelT;   
                    pt1 = new Point(pt0.X+270,pt0.Y+175); 
                    DC.DrawText( GF20.GFText(po,brsh), pt1 );  
                }                

                //*** 20120311
                //==== page Control =====
                if( prbCC==SDKpageP ){
                    if( prbCC < SDKpageMax ){ SDKpage += 4; }
                    else{ SDKpage += 4; break; }
                }
                
                //-----------------------
            }
            DC.Close();
            return drawVisual;
        }
*/
    #endregion 印刷


        private void Window_Unloaded( object sender, RoutedEventArgs e ){
            Environment.Exit(0);
        }
        private void randumSeed_TextChanged( object sender, TextChangedEventArgs e ){
            int rv=randumSeed.Text.ToInt();
            GNP00.SDKCntrl.randumSeedVal = rv;
            GNP00.SDKCntrl.SetRandumSeed(rv);
        }
//▲▲▲

        private void NiceLoopMax_ValueChanged( object sender, RoutedPropertyChangedEventArgs<object> e ) {
            if( NiceLoopMax==null )  return;
            GNumPzl.GMthdOption["NiceLoopMax"]  = NiceLoopMax.Value.ToString();
        }

        private void ALSSizeMax_ValueChanged( object sender, RoutedPropertyChangedEventArgs<object> e ) {
            if( ALSSizeMax==null )  return;
            GNumPzl.GMthdOption["ALSSizeMax"]  = ALSSizeMax.Value.ToString();
        }

        private void _Get_GNPXOptionPara(){
            GNumPzl.GMthdOption["Cell"]         = ((bool)method_NLCell.IsChecked)? "1": "0";
            GNumPzl.GMthdOption["GroupedCells"] = ((bool)method_NLGCells.IsChecked)? "1": "0";
            GNumPzl.GMthdOption["ALS"]          = ((bool)method_NLALS.IsChecked)? "1": "0";

     ///    GNumPzl.GMthdOption["AFish"]        = ((bool)method_NLALS.IsChecked)? "1": "0"; //次期開発

        }
//▼▼▼公開時は削除 ???
   
        private void PutBitMap_Click( object sender, RoutedEventArgs e ){
            Clipboard.SetData(DataFormats.Bitmap,bmpGZero);
        }

        private void SaveBitMap_Click( object sender, RoutedEventArgs e ){
            Clipboard.SetData(DataFormats.Bitmap,bmpGZero);

            BitmapEncoder enc = new PngBitmapEncoder(); // JpegBitmapEncoder(); BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmpGZero));

            if( !Directory.Exists("画像蔵") ){ Directory.CreateDirectory("画像蔵"); }
            string fName=DateTime.Now.ToString("yyyyMMdd HHmmss")+".png";
            using( Stream stream = File.Create("画像蔵/"+fName) ){
                enc.Save(stream);
            }         
        }

        private void btnHomePage_Click( object sender, RoutedEventArgs e ){
            Process.Start("http://csdenp.web.fc2.com");
        }

        private void txtProbName_KeyUp( object sender, KeyEventArgs e ) {
            if( e.Key==Key.Return )  GNP00.GNPX_Eng.GP.Name=txtProbName.Text;
        }




//▲▲▲
    }

}