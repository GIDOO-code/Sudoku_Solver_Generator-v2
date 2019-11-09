using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using OpenCvSharp.XFeatures2D;
using System.IO;
using System.Threading;

using GNPZ_sdk;

using OpenCvSharp;
using GIDOO_Lib;
using GIDOO_space;

namespace GIDOOCV{
    public partial class sdkFrameRecgV3Man{

        static public event   SDKEventHandler Send_DigitsRecog; 
        private NuPz_Win        pGNP00win;

        private Mat             frameWB=new Mat();
        private sdkFrameRecgV3  sdkFRec;   
        private string          fName;  //MachinLearning
        private uint[]          SDK64;
  
        static sdkFrameRecgV3Man(){ }

        public sdkFrameRecgV3Man(  GNPXApp000 pGNP00, string fName, int MLtype=6, int MidLSize=64, double gammaC=0.7 ){
            pGNP00win = pGNP00.pGNP00win;
            this.fName=fName;
            Send_DigitsRecog += new SDKEventHandler(pGNP00win.DigitsRecogReport);  

            if(sdkFRec==null)  sdkFRec=new sdkFrameRecgV3(fName:fName);
            SDK64=Enumerable.Repeat(0xFFFFFFFF,81).ToArray();   
        }

        private Task taskSDK;
        private CancellationTokenSource tokSrc;
        public void DigitRecogMlt( CancellationToken ct ){
            SDK64=Enumerable.Repeat(0xFFFFFFFF,81).ToArray(); 
            bool succB=false;
            int[] SDK81;
            do{
                if( ct.IsCancellationRequested ){ ct.ThrowIfCancellationRequested(); return; }
                succB = DigitRecog(out SDK81, dispB:true);  //true);
                if(SDK81!=null){
                    SDKEventArgs se = new SDKEventArgs(SDK81:SDK81);
                    Send_DigitsRecog(this,se);  //(can send information in the same way as LoopCC.)
                }
            }while(!succB);
        }

        private bool DigitRecog(out int[] SDK8, bool dispB=false){
            Mat _frame = NuPz_Win.frame00.CvtColor(ColorConversionCodes.BGR2GRAY);
            Cv2.AdaptiveThreshold(_frame, frameWB, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 51, 5);  //c=10              

            if(sdkFRec==null)  sdkFRec=new sdkFrameRecgV3(fName:fName);
            SDK8 = sdkFRec.sdkFrameRecg_SolverV3(imgWhiteB:frameWB, thrVal:128, DispB:false);
            if(SDK8==null)  return false;
            for( int k=0; k<81; k++) SDK64[k] = SDK64[k]<<4 | (uint)(SDK8[k]&0xF); 

            bool succB=false;
            SDK8 = DigitRecog_MajorityVote(ref succB);

                        if(dispB){
                            string st="";
                            int eCC=0;
                            foreach( var p in SDK8 ){ 
                                if(p==0 || p>10){st += "#"; eCC++; }
                                else st += (p<=9)? p.ToString(): ".";
                            }
                            WriteLine("DigitRecog:"+st);
                        }
            return succB;
        }
        private int[] DigitRecog_MajorityVote(ref bool succB ){
            int[] SDK8M=new int[81];
            succB=true;

            for( int rc=0; rc<81; rc++ ){
                int[] _cnt=new int[11];
                ulong sdk=SDK64[rc];
                int nTotal=0;
                for( int n=0; n<8; n++ ){
                    int P=(int)(sdk&0xF);
                    if(P==0xF) break;
                    nTotal++;
                    if(P>=10) P=10;
                    _cnt[P]++;
                    sdk>>=4;
                }
                List<int> _cntLst=_cnt.ToList();
                int NmaxCnt=_cntLst.Max();
                SDK8M[rc]=_cntLst.FindIndex(p=>p==NmaxCnt);
                double per=NmaxCnt/(double)nTotal;
                if(NmaxCnt<=3 && !(nTotal>3 && per>=0.7) ) succB=false;
            }
            return SDK8M;
        }
    }
}