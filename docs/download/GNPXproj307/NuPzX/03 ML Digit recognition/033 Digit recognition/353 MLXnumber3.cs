using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Console;

using GIDOO_Lib;
using GIDOO_space;
using OpenCvSharp;

namespace GIDOOCV{
    public partial class MLXnumber3{
        public  ProjectiveTrans     PT = new ProjectiveTrans();

        //----- Data -----
        public  LearningDataNumber  LeaDaNum;
        public  List<ULeData>       pUDLstA;   //{ get{ return LeaDaNum.UDataLst; } }    

        //----- MachineLearning -----
        public  MachineLearningLN3  MLXA3;

        public  int[]               LayerNumLst; // 中間のサイズは自由に設定できる。＠ここで設定するのみ！！！
        public  string              fName;

        private string              sdkImg="0123456789Arial.jpg";
        private string              AddedImage="AddedImage";
        private Mat                 DigitImg;
        private int                 dSize;
        private Random rdm = new Random(314);

        public MLXnumber3(){
            //----- Image input -----   
            DigitImg = new Mat(sdkImg,ImreadModes.GrayScale); //Gray
            dSize=(DigitImg.Height+2)/(DigitImg.Width/10);
        }

        public void Set_LayerData(string fName,int MLtype,int MidLSize,double gammaC,bool DispB=false){
            this.fName=fName;           
            LeaDaNum = new LearningDataNumber(fName);
            pUDLstA = LeaDaNum.UDataLst;
            
            LayerNumLst=new int[MLtype];                            //Size definition of each hierarchy
            for(int k=1; k<MLtype-1; k++ ) LayerNumLst[k]=MidLSize;
            LayerNumLst[0]= (new ULeData(null,-1)).pSize;           //input
            LayerNumLst[MLtype-1]=11;
            MLXA3=new MachineLearningLN3(fName,LayerNumLst,gammaC);
        }
        public void Set_LayerData(List<ULeData> pUDLstA,string fName,int MLtype,int MidLSize,double gammaC,bool DispB=false){
            this.pUDLstA=pUDLstA; 
            this.fName=fName;   
            LayerNumLst=new int[MLtype];                            //Size definition of each hierarchy
            for(int k=1; k<MLtype-1; k++ ) LayerNumLst[k]=MidLSize;
            LayerNumLst[0]= (new ULeData(null,-1)).pSize;           //Input
            LayerNumLst[MLtype-1]=11;
            MLXA3=new MachineLearningLN3(fName,LayerNumLst,gammaC);
        }
        public ULeData GetULeData( int nx ){
            if(nx<pUDLstA.Count)  return pUDLstA[nx];
            else return null;
        }

        public void CreateTeacher_Img_Feature_All( double AdditionRandomNoise=0.0, bool autoDNB=false ){
            if(pUDLstA!=null)  return;
            int ID=0;
            foreach( var P in CreateTeacherImg(AdditionRandomNoise,autoDNB) ){
                P.CalcFeature(DispB:false);
                P.ID=(ID++);
            }
        }
        
      #region CreateTeacherImg
      //##############################################################################
        public IEnumerable<ULeData> CreateTeacherImg( double AdditionRandomNoise=0.0, bool autoDNB=false ){
            pUDLstA = new List<ULeData>();
            foreach( var Q in CreateTeacherImg_Blank(20) )  yield return Q;             // Blank image           
            foreach( var Q in CreateTeacherImg_fromFile(ctrlWDMax:4) ) yield return Q;  // Generate digits data from file
            foreach( var Q in CreateTeacherImg_fromAddFile() ) yield return Q;          // Generate digits data from the folder"AddedImage"
            foreach( var Q in CreateTeacherImg_wideFont(0.9,1.2) ) yield return Q;      // Wide font generation
            foreach( var Q in CreateTeacherImg_addNoise(AdditionRandomNoise,autoDNB) ) yield return Q;
        }
        public IEnumerable<ULeData> CreateTeacherImg_Blank(int nn=20){ // Blank image      
            for(int k=0; k<20; k++){
                var mg0=new Mat(new Size(32,32),MatType.CV_8UC1,Scalar.White);
                ULeData Q=new ULeData(mg0,10);  //## Blank image
                pUDLstA.Add(Q); 
                yield return Q;
            }
            yield break;
        }
        public IEnumerable<ULeData> CreateTeacherImg_fromFile( int ctrlWDMax=4 ){
            int Nrow=10;
            dSize=(DigitImg.Height+2)/(DigitImg.Width/10);               
            for(int nx=0; ; nx++ ){ 
                int ky=nx/Nrow, kx=nx%Nrow;
                if(ky>=(dSize-1))  break;
            //  if(kx==0)  continue;

                int ys=ky*33; Range rgY=new Range(ys,ys+32);
                int xs=kx*33; Range rgX=new Range(xs,xs+32);
                Mat mg09=new Mat(); 
                DigitImg.SubMat(rgY,rgX).CopyTo(mg09);
                for(int ctrlWD=0; ctrlWD<ctrlWDMax; ctrlWD++ ){    
                    Mat mg09Nml = _PTNormalize(mg09,ctrlWD,200);    //## Pattern normalization
                    var Q = new ULeData(mg09Nml,kx);
                    pUDLstA.Add(Q);
                    yield return Q;
                }
            }
            yield break;           
        }
        public IEnumerable<ULeData> CreateTeacherImg_fromAddFile( ){
            if(!Directory.Exists(AddedImage)) yield break; 
            foreach( string fx in Directory.GetFiles(AddedImage) ){
                string st=Path.GetFileName(fx).Substring(0,1);
                if(!st.IsNumeric()) continue;
                Mat mg = new Mat(fx, ImreadModes.GrayScale);
                    //using( new Window("AddedImage",WindowMode.KeepRatio,mg) ){ Cv2.WaitKey(0); }                                
                int kx=int.Parse(st);
                var Q = new ULeData(mg,kx);

                pUDLstA.Add(Q);
                yield return Q;
            }
            yield break; 
        }
        public IEnumerable<ULeData> CreateTeacherImg_wideFont( double alp1, double alp2 ){
            int nSz=pUDLstA.Count();
            for(int n=0; n<nSz; n++){
                ULeData UM=pUDLstA[n];
                //if(UM.ans==0) continue;

                for(int k=0; k<2; k++){
                    double alp=(k==0)? alp1: alp2;
                    Mat UMEnL    = UM.pUData.Enlarge(alp,alp);
                    Mat UMEnLNml = _PTNormalize(UMEnL,0,200);    //◆パターン正規化
                        //using( new Window("幅広字体生成 UMEnL",WindowMode.KeepRatio,UMEnL) )
                        //using( new Window("幅広字体生成 UMEnLNml",WindowMode.KeepRatio,UMEnLNml) ){ Cv2.WaitKey(0); }  
                    var Q=new ULeData(UMEnLNml,UM.ans);
                    pUDLstA.Add(Q);
                    yield return Q;
                }
            }
            yield break; 
        }
        public IEnumerable<ULeData> CreateTeacherImg_addNoise( double AdditionRandomNoise=0.0, bool autoDNB=false ){
            int nSz=pUDLstA.Count();
            for(int n=0; n<nSz; n++){
                ULeData P=pUDLstA[n];
                //if(P.ans==0) continue;
                if(AdditionRandomNoise==0.0){
                    var Q=_AddRandomPoint(P,0.05,0.05,DispB:false);  
                    pUDLstA.Add(Q);
                    yield return Q;
                    }
                else if(!autoDNB){//denoising
                    var Q=_AddRandomPoint2(P,AdditionRandomNoise);
                    pUDLstA.Add(Q);
                    yield return Q;
                }
            }
            yield break; 
        }

        private Size szP = new Size(32,32);
        private ULeData _AddRandomPoint(ULeData P, double r0, double r255, bool DispB=false ){
            ULeData Q=new ULeData(P.pUData,P.ans);
            Q.pUData = P.pUData.Clone();
            Mat Qimg = Q.pUData;
            List<int> blackLst=new List<int>();
            //if(P.ans>0){
                int KK = (int)(szP.Height*szP.Width*r255);
                unsafe{
                    byte *S=Qimg.DataPointer;
                    for(int k=0; k<KK; k++ ) S[rdm.Next(1023)]=0;
                    byte *B=S;
                    for(int k=0; k<1024; k++ ) if(*B++<128) blackLst.Add(k);
                    int cnt=blackLst.Count;
                    KK=(int)(cnt*r0);
                    for(int k=0; k<KK; k++ ) S[ blackLst[rdm.Next(cnt)] ]=(byte)255;
                }
                if(DispB){
                    using( new Window("_AddRandomPoint P",WindowMode.KeepRatio,P.pUData) )
                    using( new Window("_AddRandomPoint Q",WindowMode.KeepRatio,Qimg) ){ Cv2.WaitKey(0); }
                }
            //}

            return Q;
        }
        private ULeData _AddRandomPoint2(ULeData P, double rX, bool DispB=false ){
            ULeData Q=new ULeData(P.pUData,P.ans);
            Q.pUData =P.pUData.Clone();
            Mat Pimg = Q.pUData;
            List<int> blackLst=new List<int>();
            int KK = (int)(szP.Height*szP.Width*rX);
            byte b255=(byte)255;
            if(P.ans>0){
                unsafe{
                    byte *S=Pimg.DataPointer;
                    for(int k=0; k<KK; k++ ){
                        int r=rdm.Next(1023);
                        S[r] = (byte)(b255-S[r]);
                    }
                }
                if(DispB){
                    new Window("AddRandomPoint2 Source",WindowMode.KeepRatio,P.pUData);
                    new Window("AddRandomPoint2 Result",WindowMode.KeepRatio,Pimg);
                    Cv2.WaitKey(0);
                }
            }

            return Q;
        }
        
      #region Position and size formatting
        public Mat _PTNormalize( Mat Mg0, int ctrlWD, byte thValue ){
            //Position adjustment
#if false
            Mat Mg5=new Mat();
            Cv2.Resize(Mg0, Mg5, new Size(Mg0.Cols*10, Mg0.Rows*10), 0, 0, InterpolationFlags.Lanczos4);
            using(new Window("PTNormalize", Mg5)){/* Cv2.WaitKey();*/ }
#endif
            Point2d[] ptI=__GetCorner4(Mg0,thValue);
            Point2d[] ptR=new Point2d[4];
            int wd;
            if(ctrlWD==0) wd=(int)((ptI[4].X/ptI[4].Y)*28); //wdCtrl:0,1-3
            else wd=15+ctrlWD*2;
            if(wd>22) wd=22;
            int w=(32-wd)/2;
            ptR[0] = new Point2d(w,4);
            ptR[1] = new Point2d(w+wd,4);
            ptR[2] = new Point2d(w+wd,28);
            ptR[3] = new Point2d(w,28);

            PT.SetPoint(ptR,ptI);
            PT.ProjectionSolver();

            Size sz = Mg0.Size();
            int W=sz.Width;

            Mat MgR= new Mat(sz, MatType.CV_8UC1, Scalar.All(255));
            
            byte B=255, b0=0, b255=255, b128=128;   
            int cnt=0;
            unsafe{
                byte *S = Mg0.DataPointer;
                byte *D = MgR.DataPointer;

                for(int x=0; x<sz.Width; x++ ){
                    for(int y=0; y<sz.Height; y++ ){                    
                        Point Rxy = PT.Convert_RtoI( new Point(x,y) );
                        B=255;
                        if( Rxy.Y>=0 && Rxy.Y<sz.Height && Rxy.X>=0 && Rxy.X<sz.Width ){
                            B=S[Rxy.Y*W+Rxy.X];
                            B=(B>thValue)? b255: b0;
                        }
                        D[y*W+x] = B;
                        if(B<b128) cnt++;
                    }
                }
                if(cnt<32) for(int k=0; k<32; k++) D[k]=b0;
                else if(cnt<64)  for(int k=32; k<64; k++) D[k]=b0;
            }

            return MgR;
        }
        public Point2d[] __GetCorner4( Mat src, byte thresholdValue ){
            int W=src.Width , H=src.Height;
            int xMin=W, xMax=0, yMin=H, yMax=0;
            unsafe{
                byte *S = src.DataPointer;  
                byte V;
                for(int y=0; y<H; y++ ){
                    for(int x=0; x<W; x++ ){
                        V=S[y*W+x];
                        if(V>thresholdValue) continue;
                        xMin = Math.Min(x,xMin);
                        xMax = Math.Max(x,xMax);
                        yMin = Math.Min(y,yMin);
                        yMax = Math.Max(y,yMax);
                    }
                }
            }
            //double ww=Math.Max(xMax-xMin,yMax-yMin)/2.0;
            //double xc=(xMin+xMax)/2.0, yc=(yMin+yMax)/2.0;
            //xMin = (int)(xc-ww); xMax=(int)(xc+ww); yMin=(int)(yc-ww); yMax=(int)(yc+ww);

            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(xMin,yMin);
            pts[1] = new Point2d(xMax,yMin);
            pts[2] = new Point2d(xMax,yMax);
            pts[3] = new Point2d(xMin,yMax);
            pts[4] = new Point2d(xMax-xMin,yMax-yMin);
            
            return pts;
        }
      #endregion Position and size formatting

      //##############################################################################
      #endregion CreateTeacherImg
        
        public void Learning( string LCofTyp, int mBatchN, bool dropB=false ){
            MLXA3.Learning( fName, pUDLstA, LCofTyp, dropB, mBatchN); 
        }
    }
}