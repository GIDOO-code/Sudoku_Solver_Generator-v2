using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using static System.Console;
using System.Threading;

using GIDOO_space;

namespace GNPXcore{
    public partial class GroupedLinkGen: AnalyzerBaseV2{
		public static DevelopWin devWin; //## development

        // *=*=* Updated to radiation search *=*=*
        public bool GroupedNiceLoopEx( ){
			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );

            bool DevelopB=false;
			foreach( var P0 in pBDL.Where(p=>(p.FreeB>0)) ){
				foreach( var noH in P0.FreeB.IEGet_BtoNo() ){
					int noB=(1<<noH);
                    foreach( var GLKH in pSprLKsMan.IEGet_SuperLinkFirst(P0,noH)){
                        UCell P000=GLKH.UGCellsA[0];    //P000=P0
					    USuperLink GNL_Result = pSprLKsMan.GNL_EvalSuperLinkChain(GLKH,P000.rc,DevelopB:DevelopB);
                        if(GNL_Result!=null){       //***** Solved   
                            string st3="";
                            string st=_chainToStringGNL(GNL_Result,ref st3);
                            if(DevelopB)  WriteLine($"***** solved:{st}");
                                                               
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP(true))  return true;
                            
                            //return true;
                        }
                    }
				}
            }
            return false;
        }

        public  string _chainToStringGNL(USuperLink GNL_Result, ref string st3){
            string st="";
            if(GNL_Result==null)  return st;

            GroupedLink GLKnxt = GNL_Result.resultGLK;
            //pSprLKsMan.Debug_ChainPrint(GLKnxt);
            var SolLst = pSprLKsMan.Convert_ChainToList_GNL(GNL_Result);
            GroupedLink GLKorg=SolLst[0];

            {//===================== cells coloring ===========================
                foreach( var LK in SolLst ){
                    bool bALK = LK is ALSLink;
                    int type = (LK is ALSLink)? S: LK.type;//ALSLink, in ALS, is S
                    foreach( var P1 in LK.UGCellsA.Select(p=>pBDL[p.rc])){
                        //WriteLine($"---------- {P1}");
                        int noB=(1<<LK.no);
                        if(!bALK)    P1.SetCellBgColor(SolBkCr);
                        if(type==S){ P1.SetNoBColor(noB,AttCr2);  }
                        else{        P1.SetNoBColor(noB,AttCr3); }
                    }

                    if(type==W){
                        foreach( var P2 in LK.UGCellsB.Select(p=>pBDL[p.rc])){
                            int noB2=(1<<LK.no);
                            if(!bALK)  P2.SetCellBgColor(SolBkCr);
                            P2.SetNoBColor(noB2,AttCr);
                        }
                    }
                }

                int cx=2;
                foreach( var LK in SolLst ){    // ALS
                    ALSLink ALK = LK as ALSLink;
                    if(ALK==null)  continue;
                    Color crG=_ColorsLst[cx++];
                    foreach( var P in ALK.ALSbase.B81.IEGet_rc().Select(rc=>pBDL[rc]) ){
                        P.SetCellBgColor(crG);
                    }
                }
            }

            {//===================== result report ===========================
                st3="";
                int SolType = GNL_Result.contDiscontF;
                if(SolType==1) st = "Nice Loop(Cont.)";  //<>continuous
                else{                                              //<>discontinuous
                    int rc=GLKorg.UGCellsA[0].rc;
                    var P=pBDL[rc];
                    st = "Nice Loop(Discont.) r"+(rc/9+1)+"c"+(rc%9+1);
                    int dcTyp= GLKorg.type*10+GLKnxt.type; 
                    switch(dcTyp){
                        case 11: st+=$" is {(GLKorg.no+1)}";     P.SetCellBgColor(SolBkCr2); break;
                        case 12: st+=$" is not {(GLKnxt.no+1)}"; P.CancelB=1<<GLKnxt.no; break;
                        case 21: st+=$" is not {(GLKorg.no+1)}"; P.CancelB=1<<GLKorg.no; break;
                        case 22: st+=$" is not {(GLKorg.no+1)}"; P.CancelB=1<<GLKorg.no; break;
                    }
                }

                string st2=__chainToStringGNLsub(SolLst, ref st3 );
                st = st3+st;
                Result = st;
                ResultLong = st+"\r"+st2;
            }
            return st;
        }

        public  string __chainToStringGNLsub(List<GroupedLink> SolLst, ref string st3){
            string po = $"[{SolLst[0].UGCellsA}]";
            foreach( var LK in SolLst ){
                string ST_LinkNo="";
                ALSLink ALK=LK as ALSLink;
                if(ALK!=null){
                    ST_LinkNo = $"-#{(ALK.no+1)}ALS<{ALK.ALSbase.ToStringRC()}>#{(ALK.no2+1)}-";
                }
                else{
                    string mk = (LK.type==1)? "=": "-";
                    ST_LinkNo = mk+(LK.no2+1)+mk;
                }
                po += $"{ST_LinkNo}[{LK.UGCellsB}]";
            }
            
            if(po.Contains("ALS") || po.Contains("[<")) st3="Grouped ";
            return po;
        }
    }  
}