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
using System.Text;

using Microsoft.Win32;

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

//using OpenCvSharp;
//using OpenCvSharp.Extensions;
using GIDOOCV;

using GIDOO_space;

namespace GNPZ_sdk{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win{
    #region display
        private bool sNoAssist=false;
        private int  solLevelCC=0;
        private int  solLevelMax=0;
        private void _Display_GB_GBoard( UPuzzle GPML=null, bool DevelopB=false ){
            if( GNP00.AnalyzerMode=="MultiSolve" && __DispMode!="Complated" )  return;
            try{
                UPuzzle currentP = GPML?? pGP;

                lblUnderAnalysis.Visibility = (GNP00.GSmode=="tabASolve")? Visibility.Visible: Visibility.Hidden; 
                Lbl_onAnalyzerM.Visibility = Visibility.Visible; 
      
                SDKGrp.GBoardPaint(bmpGZero, currentP.BDL, GNP00.GSmode, sNoAssist);
                PB_GBoard.Source = bmpGZero;    //◆currentP.BDL set

                __Set_CellsPZMCount();
                txtProbNo.Text = (currentP.ID+1).ToString();
                txtProbName.Text = currentP.Name;
                nUDDifficultyLevel.Text = currentP.DifLevel.ToString();
    //The following code "pMethod" is rewritten to another thread. 
    //This may cause an access violation.
    //here Try with try{...} catch(Exception){...}. 
                int DiffL=(GNP00.pGP.pMethod==null)? 0: GNP00.pGP.pMethod.DifLevel; //
                lblCurrentnDifficultyLevel.Content = $"Difficulty: {DiffL}"; //CurrentLevel

                if(DevelopB) _Display_Develop();
			    if(GNP00.GSmode=="tabASolve")  _Display_ExtResultWin();	
            }
            catch(Exception e){
                WriteLine( e.Message+"\r"+e.StackTrace );
#if DEBUG
                using(var fpW=new StreamWriter("Exception_002e.txt",true,Encoding.UTF8)){
                    fpW.WriteLine($"---{DateTime.Now} {e.Message} \r{e.StackTrace}");
                }
#endif
            }
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
			if(ExtResultWin==null){
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
            if( __Set_CellsPZMCount() ) _Display_GB_GBoard( );
        }

        private bool __Set_CellsPZMCount( ){
            int nP=0, nZ=0, nM=0;
            return  __Set_CellsPZMCount( ref nP, ref nZ, ref nM );
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

                if(GNP00.GSmode=="tabACreate"){
                    int solLvl = Min( SDK_Ctrl.solLevelNN, 81-nP);
                    lblSolutionLevel.Content = $"Probability without solution: {solLvl}/{81-nP}";
                    lblSolutionLevel.Visibility = Visibility.Visible;
                }
                else{ lblSolutionLevel.Visibility=Visibility.Hidden; }
            }
            return ((nP+nM>0)&sAssist);
        }

        private void chbAnalyze00_Checked( object sender, RoutedEventArgs e ){
            if( bmpGZero==null )  return;
            sNoAssist = (bool)chbDisplayCandidate.IsChecked;//chbShowNoUsedDigits
            _SetScreenProblem();
        }
        private void chbAssist01_Checked( object sender, RoutedEventArgs e ){
            sNoAssist = (bool)chbShowNoUsedDigits.IsChecked;
            _Display_GB_GBoard();　//(Show free numbers)
        }

        private int    __GCCounter__=0;
        private int    _ProgressPer;
        private string __DispMode=null;

        private void displayTimer_Tick( object sender, EventArgs e ){
            _Display_GB_GBoard();   //******************
            
            UPuzzle GPML=null;
            if(GNP00.GSmode=="DigRecogCmp" || GNP00.GSmode=="DigRecogCancel"){
                if(GNP00.SDK81!=null){
                    GNP00.CurrentPrbNo=999999999;
                    GPML=GNP00.SDK_ToUPuzzle(GNP00.SDK81,saveF:true);
                    GNP00.CurrentPrbNo=GPML.ID;     //20180731
                }
                displayTimer.Stop();
            /*
                btnRecog.Content="Input";
                bdbtnRecog.BorderBrush=Brushes.Blue;
                cameraMessageBox.Content = (GNP00.GSmode=="DigRecogCmp")? "Fixed": "Canceled";
                cameraMessageBox.Foreground = Brushes.LightBlue;
            */
                _SetScreenProblem();
                GNP00.GSmode = "tabACreate";
            }
/*
            bool? B=SDK_Ctrl.paraSearching;
            if(B!=null && !(bool)B){
                displayTimer.Stop();
                _SetScreenProblem( );
                SDK_Ctrl.paraSearching=null;
            }
*/
            switch(GNP00.GSmode){
                case "DigRecogTry":
                case "tabACreate": _Display_CreateProblem(); break;

                case "tabBMultiSolve":
                case "tabASolve":  _Display_AnalyzeProb(); break;
            }    

            lblResourceMemory.Content = "Memory: " + GC.GetTotalMemory(true).ToString("N0");            
            if( ((++__GCCounter__)%1000)==0 ){ GC.Collect(); __GCCounter__=0; }
        }

        private UPuzzle CreateDigitToUProblem(int[] SDK81){
            string st="";
            for(int rc=0; rc<81; rc++ ){
                int nn=SDK81[rc];
                if(nn>9) nn=0;
                st += st.ToString();
            }
            UPuzzle UP=GNP00.SDK_ToUPuzzle(st,saveF:true); 
            return UP;
        }
        private RenderTargetBitmap bmpPD = new RenderTargetBitmap(176,176, 96,96, PixelFormats.Default);//176=18*9+2*4+1*6        
        private void _Display_CreateProblem(){
            txbNoOfTrials.Text    = GNP00.SDKCntrl.LoopCC.ToString();
            txbNoOfTrialsCum.Text = SDK_Ctrl.TLoopCC.ToString();
            txbBasicPattern.Text  = GNP00.SDKCntrl.PatternCC.ToString();
            int n=gamGen05.Text.ToInt();
            lblNoOfProblems1.Content = (n-_ProgressPer).ToString();

            UPuzzle pGP = GNP00.pGP;
            if(pGP!=null){
                int nn=GNP00.SDKProbLst.Count;
                if(nn>0){
                    txtProbNo.Text = nn.ToString();
                    txtProbName.Text = GNP00.SDKProbLst.Last().Name;
                    nUDDifficultyLevel.Text = pGP.DifLevel.ToString();
                }
            }

            string st = AnalyzerLapElaped;
            Lbl_onAnalyzerTS.Content  = st;
            Lbl_onAnalyzerTSM.Content = st;
            txbEpapsedTimeTS3.Text    = st;

            if(__DispMode!=null && __DispMode!=""){
                _SetScreenProblem();
                displayTimer.Stop();
                AnalyzerLap.Stop();
                btnCreateProblemMlt.Content = pRes.btnCreateProblemMlt;
            }
            __DispMode="";

            if((bool)chbCreateProblemEx2.IsChecked){
                SDKGrp.GBPatternDigit( bmpPD, Sol99sta );
            }
            else bmpPD.Clear();
            PB_BasePatDig.Source=bmpPD; 
        }

        private void _Display_AnalyzeProb(){
            //WriteLine("----------------"+__DispMode);
            if( __DispMode=="Canceled" ){
                shortMessage("cancellation accepted",new sysWin.Point(120,188),Colors.Red,3000);
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

            if(UPP!=null && UPP.Count>0){
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

            string st=AnalyzerLapElaped;
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
    }
}