using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPXcore{

//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*====*==*==*==*
//  GeneralLogic is in development now.
//  (Completeness is about 30%.)
//  A lot of development code remains.
//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

    public class GeneralLogicGen: AnalyzerBaseV2{
        static public int GLtrialCC=0;
        static public int  ChkBas0=0, ChkBas1=0, ChkBas2=0, ChkBas3=0,  ChkBas4=0;
        static public int ChkCov1=0, ChkCov2=0;
        static public int ChkBas3A=0, ChkBas3B=0;
        private int GStageMemo;
        private UGLinkMan UGLMan;
        private int GLMaxSize;
        private int GLMaxRank;

        public GeneralLogicGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        public bool GeneralLogicExnm( ){                                //### GeneralLogic controler
            if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
                GLMaxSize = GNPXApp000.GMthdOption["GenLogMaxSize"].ToInt();
                GLMaxRank = GNPXApp000.GMthdOption["GenLogMaxRank"].ToInt();
                UGLMan=new UGLinkMan(this);
                if(SDK_Ctrl.UGPMan==null)  SDK_Ctrl.UGPMan=new UPuzzleMan(1);
                UGLMan.PrepareUGLinkMan( printB:false );    //SDK_Ctrl.UGPMan.stageNo==12 ); //false); 
			}
            
            WriteLine( $"--- GeneralLogic --- trial:{++GLtrialCC}" );
            for(int sz=1; sz<=GLMaxSize; sz++ ){
                for(int rnk=0; rnk<=GLMaxRank; rnk++ ){ 
                    if(rnk>=sz) continue;
                    ChkBas1=0; ChkBas2=0; ChkBas3=0; ChkBas4=0;
                    ChkBas3A=0; ChkBas3B=0;
                    ChkCov1=0; ChkCov2=0;

                    bool solB = GeneralLogicEx(sz,rnk);

                    string st = solB? "++": "  ";
                    WriteLine($" {sz} {rnk} {st} Bas:({ChkBas1},{ChkBas2},{ChkBas3},{ChkBas4})/{ChkBas0} " +
                              $" Cov:{ChkCov2}/{ChkCov1}  interNum({ChkBas3A}/{ChkBas3B})");

                    if(solB) return true;
                }
            }
            return false;
        }
        
        private bool GeneralLogicEx( int sz, int rnk ){                 //### GeneralLogic main routine
            if(sz>GLMaxSize || rnk>GLMaxRank)  return false;

            foreach( var UBC in UGLMan.IEGet_BaseSet(sz,rnk) ){         //### BaseSet generator
                if( pAnMan.CheckTimeOut() ) return false;

                var bas=UBC.HB981;
                foreach( var UBCc in UGLMan.IEGet_CoverSet(UBC,rnk) ){  //### CoverSet generator

                    for(int no=0; no<9; no++ ){
                        if( !(UBCc.HC981._BQ[no]-UBCc.HB981._BQ[no]).IsZero() )  goto SolFound;
                    }
                    continue;

                  SolFound:
                    foreach( int n in UBCc.Can981.noBit.IEGet_BtoNo() ){
                        foreach( int rc in UBCc.Can981._BQ[n].IEGetRC() ){
                            pBDL[rc].CancelB |= (1<<n);
                        }
                    }

                    if(SolInfoB)  _generalLogicResult(UBCc);
                    if(__SimpleAnalizerB__)  return true;
                    if(!pAnMan.SnapSaveGP(false)) return true;                 
                }
            }
            return false;
        }

        private void _generalLogicResult( UBasCov UBCc ){
            try{
                Bit81 Q=new Bit81();
                foreach( var P in UBCc.covUGLs )  Q |= P.rcnBit.CompressToHitCells();
                foreach( var UC in Q.IEGetRC().Select(rc=>pBDL[rc]))   UC.SetCellBgColor(SolBkCr2);
           
                for(int rc=0; rc<81; rc++ ){
                    int noB=UBCc.HB981.IsHit(rc);
                    if(noB>0) pBDL[rc].SetNoBBgColor(noB,AttCr,SolBkCr);
                }

                string msg = "\r     BaseSet: ";
                string msgB="";
                foreach( var P in UBCc.basUGLs ){
                    if(P.rcBit81 is Bit81) msgB += P.tfx.tfxToString()+$"#{(P.rcBit81.no+1)} ";
                    else msgB += P.UC.rc.ToRCString()+" ";
                }
                msg += ToString_SameHouseComp1(msgB);
               
                msg += "\r    CoverSet: ";
                string msgC="";
                foreach( var P in UBCc.covUGLs ){
                    if(P.rcBit81 is Bit81) msgC += P.tfx.tfxToString()+$"#{(P.rcBit81.no+1)} ";
                    else msgC +=P.UC.rc.ToRCString()+" ";
                }
                msg += ToString_SameHouseComp1(msgC);

                string st=$"GeneralLogic N:{UBCc.sz} rank:{UBCc.rnk}";
                Result = st;
                msg += $"\r\r ChkBas:{ChkBas4}/{ChkBas1}  ChkCov:{ChkCov2}/{ChkCov1}";
                ResultLong = st+msg;  
            }
            catch( Exception ex ){
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);
            }
        }

        private string ToString_SameHouseComp1( string st ){
            char[] sep=new Char[]{ ' ', ',', '\t' };
            List<string> T=st.Trim().Split(sep).ToList();
            if(T.Count<=1) return st;
            List<_ClassNSS> URCBCell=new List<_ClassNSS>();
            T.ForEach(P=> URCBCell.Add(new _ClassNSS(P)));
            return ToString_SameHouseComp2(URCBCell);
        }
        private string ToString_SameHouseComp2( List<_ClassNSS> URCBCell){
            string st;
            bool cmpF=true;
            do{
                cmpF=false;
                foreach( var P in URCBCell ){
                    List<_ClassNSS> Qlst=URCBCell.FindAll(Q=>(Q.stRCB==P.stRCB && Q.stNum[0]==P.stNum[0]));
                    if(Qlst.Count>=2){
                        st=Qlst[0].stNum[0].ToString();
                        Qlst.ForEach(Q=>st+=Q.stNum.Substring(1,Q.stNum.Length-1));
                        _ClassNSS R=URCBCell.Find(Q=>(Q.stRCB==P.stRCB));
                        R.stNum = st;
                        foreach(var T in Qlst.Skip(1)) URCBCell.Remove(T);
                        cmpF=true;
                        break;
                    }

                    Qlst=URCBCell.FindAll(Q=>(Q.stNum==P.stNum && Q.stRCB[0]==P.stRCB[0]));
                    if(Qlst.Count>=2){
                        st=Qlst[0].stRCB[0].ToString();
                        Qlst.ForEach(Q=>st+=Q.stRCB.Substring(1,Q.stRCB.Length-1));
                        _ClassNSS R=URCBCell.Find(Q=>(Q.stNum==P.stNum));
                        R.stRCB = st;
                        foreach(var T in Qlst.Skip(1)) URCBCell.Remove(T);
                        cmpF=true;
                        break;
                    }
                }

            }while(cmpF);
            st="";
            URCBCell.ForEach(P=> st+=(P.stRCB+P.stNum+" ") );
            return st;
        }
        private class _ClassNSS{
            public int sz;
            public string stRCB;
            public string stNum="  ";
            public _ClassNSS( int sz, string stRCB, string stNum ){
                this.sz=sz; this.stRCB=stRCB; this.stNum=stNum;
            }
            public _ClassNSS( string st ){
                try{
                    sz=1; 
                    if(st.Length>=2) stRCB=st.Substring(0,2);
                    if(st.Length>=4) stNum=st.Substring(2,2);
                }
                catch(Exception){ }
            }
        }
    }  

    public class UBasCov{
        public Bit324       usedLK;
        public List<UGLink> basUGLs; //
        public List<UGLink> covUGLs; //
        public Bit981 HB981;
        public Bit981 HC981;
        public Bit981 Can981;
        public int    rcCan;
        public int    noCan;
        public int    sz;
        public int    rnk;

        public UBasCov( List<UGLink> basUGLs, Bit981 HB981, int sz, Bit324 usedLK ){
            this.basUGLs=basUGLs; this.HB981=HB981; this.sz=sz; this.usedLK=usedLK;
        }
 
        public void addCoverSet( List<UGLink> covUGLs, Bit981 HC981, Bit981 Can981, int rnk ){
            this.covUGLs=covUGLs; this.HC981=HC981; this.Can981=Can981; this.rnk=rnk;
        }
        public override string ToString(){
            string st="";
            foreach( var UGL in basUGLs){
                if(UGL.rcBit81 is Bit81){   // RCB
                    int no=UGL.rcBit81.no;
                    st += string.Format("Bit81: no:{0}  {1}\r", no, UGL.rcBit81 );
                }
                else{   // Cell
                    UCell UC=UGL.UC;
                    st += string.Format("UCell: {0}\r", UC );
                }
            }
            return st;
        }
    }

}

#if false
1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2
--- GeneralLogic --- trial:1
 1 0 ++ Bas:(83,0,0,0)/83  Cov:1/70  interNum(0/0)
--- GeneralLogic --- trial:2
 1 0    Bas:(103,0,0,0)/226  Cov:0/80  interNum(0/0)
 2 0 ++ Bas:(302,224,59,12)/3894  Cov:1/337  interNum(0/0)
--- GeneralLogic --- trial:3
 1 0    Bas:(103,0,0,0)/4037  Cov:0/72  interNum(0/0)
 2 0 ++ Bas:(495,369,106,24)/10078  Cov:1/496  interNum(0/0)
--- GeneralLogic --- trial:4
 1 0    Bas:(103,0,0,0)/10221  Cov:0/64  interNum(0/0)
 2 0    Bas:(601,414,116,46)/18738  Cov:0/511  interNum(0/0)
 2 1 ++ Bas:(276,221,141,50)/21270  Cov:1/26826  interNum(17/62)
--- GeneralLogic --- trial:5
 1 0    Bas:(103,0,0,0)/21413  Cov:0/62  interNum(0/0)
 2 0    Bas:(595,409,115,46)/29930  Cov:0/503  interNum(0/0)
 2 1    Bas:(995,719,515,206)/39963  Cov:0/102852  interNum(44/171)
 3 0 ++ Bas:(2615,2350,488,14)/232225  Cov:1/15031  interNum(0/0)
--- GeneralLogic --- trial:6
 1 0    Bas:(103,0,0,0)/232368  Cov:0/58  interNum(0/0)
 2 0    Bas:(598,400,109,46)/240896  Cov:0/496  interNum(0/0)
 2 1    Bas:(1008,713,505,211)/250929  Cov:0/95975  interNum(43/166)
 3 0 ++ Bas:(1732,1539,251,10)/379859  Cov:1/5287  interNum(0/0)
--- GeneralLogic --- trial:7
 1 0    Bas:(103,0,0,0)/380002  Cov:0/56  interNum(0/0)
 2 0    Bas:(639,428,116,46)/389008  Cov:0/512  interNum(0/0)
 2 1    Bas:(984,689,506,219)/399106  Cov:0/80559  interNum(46/160)
 3 0 ++ Bas:(2846,2495,458,15)/612945  Cov:1/10197  interNum(0/0)
--- GeneralLogic --- trial:8
 1 0    Bas:(103,0,0,0)/613088  Cov:0/52  interNum(0/0)
 2 0 ++ Bas:(643,421,108,46)/621676  Cov:1/438  interNum(0/0)
--- GeneralLogic --- trial:9
 1 0    Bas:(103,0,0,0)/621819  Cov:0/48  interNum(0/0)
 2 0    Bas:(633,399,108,46)/630999  Cov:0/364  interNum(0/0)
 2 1 ++ Bas:(469,361,251,100)/635484  Cov:1/37825  interNum(22/84)
--- GeneralLogic --- trial:10
 1 0    Bas:(103,0,0,0)/635627  Cov:0/44  interNum(0/0)
 2 0    Bas:(646,397,108,46)/644814  Cov:0/346  interNum(0/0)
 2 1    Bas:(993,651,480,223)/654912  Cov:0/61890  interNum(42/155)
 3 0    Bas:(5160,4295,682,36)/1028704  Cov:0/8550  interNum(0/0)
 3 1 ++ Bas:(1769,1232,609,62)/1091945  Cov:1/1292152  interNum(54/370)
--- GeneralLogic --- trial:11
 1 0 ++ Bas:(17,0,0,0)/1091962  Cov:1/11  interNum(0/0)
--- GeneralLogic --- trial:12
 1 0    Bas:(102,0,0,0)/1092104  Cov:0/40  interNum(0/0)
 2 0    Bas:(659,393,104,49)/1101333  Cov:0/257  interNum(0/0)
 2 1    Bas:(979,619,457,215)/1111289  Cov:0/53027  interNum(42/154)
 3 0    Bas:(4955,4097,621,37)/1481104  Cov:0/5782  interNum(0/0)
 3 1 ++ Bas:(3186,2142,1038,87)/1610525  Cov:1/2393747  interNum(119/698)
--- GeneralLogic --- trial:13
 1 0    Bas:(99,0,0,0)/1610663  Cov:0/30  interNum(0/0)
 2 0    Bas:(645,374,95,47)/1619582  Cov:0/175  interNum(0/0)
 2 1    Bas:(930,570,428,211)/1629010  Cov:0/37957  interNum(41/146)
 3 0    Bas:(4520,3676,550,41)/1970843  Cov:0/2553  interNum(0/0)
 3 1 ++ Bas:(3157,2040,1020,90)/2097474  Cov:1/1840876  interNum(97/649)

Execution time: 38.5seconds.
#endif