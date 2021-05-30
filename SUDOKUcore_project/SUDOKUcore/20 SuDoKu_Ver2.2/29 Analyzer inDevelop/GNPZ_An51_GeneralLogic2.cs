using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPXcore{

//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*====*==*==*==*
//  Add description to code GeneralLogic and its auxiliary routein.
//  "GeneralLogic" completeness is about 30%.
//    Currently,it takes a few seconds to solve a size 3 problem.
//    As an expectation, I would like to solve a size 5 problem in a few seconds.
//    Probably need a new theory.
//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

    public partial class GeneralLogicGen2: AnalyzerBaseV2{
        static public int   GLtrialCC=0;
        static public int   ChkBas0=0, ChkBas1=0, ChkBas2=0, ChkBas3=0, ChkBas4=0, ChkBas5=0, ChkBas6=0, ChkBas7=0;
        static public int   ChkCov1=0, ChkCov2=0;
        static public int   ChkBas3A=0, ChkBas3B=0;
        private int         GStageMemo;

        private UGLinkMan2  UGLMan2;
        private int         GLMaxSize;
        private int         GLMaxRank;
        public GeneralLogicGen2( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        public bool GeneralLogic2( ){                                //### GeneralLogic controler
            if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
                GLMaxSize = GNPXApp000.GMthdOption["GenLogMaxSize"].ToInt();
                GLMaxRank = GNPXApp000.GMthdOption["GenLogMaxRank"].ToInt();
                UGLMan2   = new UGLinkMan2(this);
                if(SDK_Ctrl.UGPMan==null)  SDK_Ctrl.UGPMan=new UPuzzleMan(1);

                UGLMan2.PrepareUGLinkMan( printB:false );
			}
            
            WriteLine( $"--- GeneralLogicEx --- trial:{++GLtrialCC}" );
            for(int sz=1; sz<=GLMaxSize; sz++ ){
                for(int rnk=0; rnk<=GLMaxRank; rnk++ ){ 
                    if(rnk>=sz) continue;
                    ChkBas1=0; ChkBas2=0; ChkBas3=0; ChkBas4=0; ChkBas5=0; ChkBas6=0; ChkBas7=0;
                    ChkBas3A=0; ChkBas3B=0;
                    ChkCov1=0; ChkCov2=0;

                    bool solB = GeneralLogic2_Solver(sz,rnk);
                    string st = solB? "++": "  ";

                    WriteLine($" {sz} {rnk} {st} Bas:({ChkBas1},{ChkBas2},{ChkBas3},{ChkBas4},{ChkBas5},{ChkBas6},{ChkBas7})/{ChkBas0} " +
                              $" Cov:{ChkCov2}/{ChkCov1}  interNum({ChkBas3A}/{ChkBas3B})");
                    if(solB) return true;
                }
            }
            return false;
        }
        
        private bool GeneralLogic2_Solver( int sz, int rnk ){                 //### GeneralLogic main routine
            if(sz>GLMaxSize || rnk>GLMaxRank)  return false;

            string st="";
            UGLMan2.Initialize();
            foreach( var UBS in UGLMan2.IEGet_BaseSet(sz,rnk) ){         //UBS: BaseSet generator
                if( pAnMan.CheckTimeOut() ) return false;

                var bas=UBS.HB981;
                foreach( var UBCc in UGLMan2.IEGet_CoverSet(UBS,rnk) ){  //### CoverSet generator


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
                    if(SolInfoB){
                        st=_generalLogicResult(UBCc);
                    }

                    if(__SimpleAnalizerB__)  return true;
                    if(!pAnMan.SnapSaveGP(false)) return true;                 
                }

            }
            return false;
        }

        private string _generalLogicResult( UBasCov2 BasCov ){
            try{
                var  BaseSetLst = BasCov.BaseSetLst;        //BaseSet  list
                var  CoverSetLst = BasCov.CoverSetLst;        //CoverSet list 

                Bit81 Q=new Bit81();
                foreach( var P in CoverSetLst )  Q |= P.rcnBit.CompressToHitCells();
                foreach( var UC in Q.IEGetRC().Select(rc=>pBDL[rc]))   UC.SetCellBgColor(SolBkCr2);
           
                for(int rc=0; rc<81; rc++ ){
                    int noB=BasCov.HB981.IsHit(rc);
                    if(noB>0) pBDL[rc].SetNoBBgColor(noB,AttCr,SolBkCr);
                }

                string msg = "\r     BaseSet: ";
                string msgB = BaseSetLst.Aggregate("",(Q,P)=>Q+$" {P.ToAppearance()}");
                msg += msgB.ToString_SameHouseComp1();
               
                msg += "\r    CoverSet: ";
                string msgC = CoverSetLst.Aggregate("",(Q,P)=>Q+$" {P.ToAppearance()}");
                msg += msgC.ToString_SameHouseComp1();

                string st=$"GeneralLogic size:{BasCov.sz} rank:{BasCov.rnk}";
                Result = st;
                msg += $"\rChkBas:{ChkBas4}/{ChkBas1}  ChkCov:{ChkCov2}/{ChkCov1}";
                ResultLong = st+"\r "+msg;
                return st+"\r"+msg;
            }
            catch( Exception ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); }
            return "";
        }
    }
}