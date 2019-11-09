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

using OpenCvSharp;
using OpenCvSharp.Extensions;
using GIDOOCV;

using GIDOO_space;

namespace GNPZ_sdk{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win{
    #region camera / ML digit recognition
        static  public Mat      frame00=new Mat();
        static  public int[]    SDK8;
        private DispatcherTimer cameraTimer;
        private int             camID=-1;
        private VideoCapture    capture;
        private List<string>    captureTypeList=new List<string>{"3.1MP 4:3 2048x1530", "1.9MP 4:3 1600x1200", "0.3MP 4:3 640x480" };
        private WriteableBitmap w1 = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);   //PixelFormats.Gray8
        private DateTime        startTime;
        
        private void NuPz_Win_camera(){
            cameraTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
            cameraTimer.Interval = TimeSpan.FromMilliseconds(50);
            cameraTimer.Tick += new EventHandler(cameraTimer_Tick);
            
            btnRecog.Content="Input";

            captureType.ItemsSource=captureTypeList;
            captureType.SelectedIndex=1;
            _SetCaptureType();
            bdbtnRecog.BorderBrush=null;
            
            lblPara_fName.Content = "ML_parameter:"+MachineLearningLN3.fNamePara;
                            
            cameraMessageBox.Content="...";
            cameraMessageBox.Foreground=Brushes.LightBlue;

        }
        private void notDisplayFfomNextTime_Checked(object sender,RoutedEventArgs e){
            GNPXApp000.GMthdOption["FirstMessage"] = ((bool)notDisplayFfomNextTime.IsChecked)? "notDisp": "Disp";
        }
        private void btnOperation_Click(object sender,RoutedEventArgs e){
            bool btnB=(sender as Button)==btnOperation;
            msgbxFirstMessage.Visibility =btnB? Visibility.Visible: Visibility.Hidden;
        }
        private void rdbCam123_Checked(object sender,RoutedEventArgs e){
            rdbVideoCameraLst.ForEach(P=>P.Visibility=Visibility.Hidden);
            var selP=sender as RadioButton;
            selP.Visibility=Visibility.Visible;
            camID = int.Parse(selP.Name.Replace("rdbCam",""))-1;
            cameraTimer.Start();
        }
        //camera start/stop
        private void tabAutoManual_SelectionChanged(object sender,SelectionChangedEventArgs e){
            TabItem tb=tabAutoManual.SelectedItem as TabItem;
            if(tb==null || cameraTimer==null) return;
            if( tb.Name=="WebCam" ){
                cameraTimer.Start();
                if(GNPXApp000.GMthdOption.ContainsKey("FirstMessage")){
                    bool fmDsp=GNPXApp000.GMthdOption["FirstMessage"]=="Disp";
                    msgbxFirstMessage.Visibility = fmDsp? Visibility.Visible: Visibility.Hidden;
                }
                else msgbxFirstMessage.Visibility = Visibility.Visible;
            }
            else{ cameraTimer.Stop(); }
            bdbtnRecog.BorderBrush=Brushes.Blue;
            cameraMessageBox.Content = "";
        }   
        private void captureType_SelectionChanged(object sender,SelectionChangedEventArgs e){
            _SetCaptureType();
        }
        private void _SetCaptureType(){
#if false
            for(int ID=0; ID<3; ID++){
                try{
                    var cap=new VideoCapture(ID);
                    if(cap.IsOpened()){
                        rdbVideoCameraLst[ID].Visibility = Visibility.Visible;                      
                    }
                    else{ rdbVideoCameraLst[ID].Visibility = Visibility.Hidden; }
                    if(cap.IsEnabledDispose) cap.Dispose();
                }
                catch(Exception e){ WriteLine(e.Message); }
            }
#endif
            if(capture==null || cameraTimer==null)  return;
            cameraTimer.Stop();
            string st = (string)captureType.SelectedValue;
            var sp = st.Split(' ');
            var eLst = sp[2].Split('x');
            capture.Set(CaptureProperty.FrameWidth,int.Parse(eLst[0]));// 2048 / 1600 / 640
            capture.Set(CaptureProperty.FrameHeight,int.Parse(eLst[1]));// 1530 / 1200 / 480
            startTime = DateTime.Now;
            cameraTimer.Start();
        }

        //#############################################################################
        private void cameraTimer_Tick( object sender, EventArgs ex ){
            try{
                if(camID<0)  return;
                if(capture==null) capture=new VideoCapture(camID);
                if(!capture.IsOpened()){ __CameraError(); return; }

                capture.Read(frame00); 
                if(frame00.Width==0){ __CameraError(); return; }

                if((bool)chbXaxis.IsChecked) frame00 = frame00.Flip(FlipMode.X);
                if((bool)chbYaxis.IsChecked) frame00 = frame00.Flip(FlipMode.Y);
                w1 = WriteableBitmapConverter.ToWriteableBitmap(frame00,PixelFormats.Bgr24);
                Img1.Source = w1;
            }
            catch( Exception e ){
                cameraMessageBox.Content="Recognition failed";
                WriteLine( e.Message+"\r"+e.StackTrace);
            }
        }
        //#############################################################################

        private void __CameraError(){
            cameraMessageBox.Content = "Camera is not mounted";
            if(cameraTimer!=null)  cameraTimer.Stop();
            if(capture!=null) capture.Release();
        }

        private void btnRecog_Click(object sender,RoutedEventArgs e){
            if((string)btnRecog.Content=="Input") {
                btnRecog.Content="Stop";
                GNP00.GSmode = "DigRecogTry";
                GNP00.pGP_DgtRecog = new UPuzzle();
                bdbtnRecog.BorderBrush=Brushes.Orange;

                cameraMessageBox.Content="Reaading";
                tokSrc = new CancellationTokenSource();　//procedures for suspension 
                taskSDK = new Task( ()=> GNP00.SDKRecgMan.DigitRecogMlt(tokSrc.Token), tokSrc.Token );
                taskSDK.ContinueWith( t=> DigitsRecogComplated() ); //完了時の手続きを登録
                taskSDK.Start();
            }
            else{
                btnRecog.Content="Input";
                tokSrc.Cancel();
                try{ taskSDK.Wait(); }
                catch(AggregateException){ GNP00.GSmode = "DigRecogCancel"; }
            }
        }

        //The following two routines is launched from another thread(taskSDK).
        //Then can not operate windows elements.
        public void DigitsRecogReport( object sender, SDKEventArgs e ){ 
            try{ 
                if(e.SDK81==null) return;
                GNP00.SDK81=e.SDK81;
                GNP00.pGP_DgtRecog.SetNo_fromIntArray(GNP00.SDK81);
                GNP00.GNPX_Eng.pGP=GNP00.pGP_DgtRecog;
                displayTimer.Start();
            }
            catch(Exception e2){ WriteLine( e2.Message+"\r"+e2.StackTrace); }
        }
        private void DigitsRecogComplated( ){
            try{ GNP00.GSmode = "DigRecogCmp"; }
            catch(Exception e ){ WriteLine( e.Message+"\r"+e.StackTrace); }
        }
    #endregion camera
    }
}