using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using static System.Diagnostics.Debug;
using System.Threading;

using GIDOO_space;

namespace GNPXcore{
    public partial class GroupedLinkGen: AnalyzerBaseV2{

/*
    All Force-based algorithms use the current links.
    The chain is assembled using this links, and the truth or false of the cell candidate is logically derived.

    The Force algorithm is based on the following logic.

     (1) Set X has one element true and the rest false. Which is true is undecided.
     (2) In a chain starting with true element, the value of the derived element is determined to be true or false.
     (3) In a chain starting with false element, the value of the derived element is uncertain (it can be true or false).
     (4) For each chain that starts assuming each element of set Ⅹ as true, 
         the authenticity of element A is determined when the true/false values ​​of element A leading by all chains match.
     (5) In the chain that starts assuming that one element B of set X is true, 
         when the true/false values ​​of the element C guided by multiple routes do not match,
         the starting element B is determined to be false. 

     https://gidoo-code.github.io/Sudoku_Solver_Generator/
     https://github.com/GIDOO-code/Sudoku_Solver_Generator/tree/master/docs/page56.html
 */ 
        public bool ForceChain_CellEx( ){
            if(GNPXApp000.GMthdOption["ForceChainCellHouseOn"]!="1")  return false;

            GroupedLink._IDsetB=false;  //ID set for debug

			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
            string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
              
            Bit81[] sPass=new Bit81[9];
            for(int k=0; k<9; k++ ) sPass[k]=new Bit81();

            bool solvedA=false;
			foreach( var P0 in pBDL.Where(p=>(p.FreeB>0)) ){            //origin cell
				Bit81[] sTrue=new Bit81[9];
				for(int k=0; k<9; k++ ) sTrue[k]=new Bit81(all_1:true);

                bool solvedB=false;
				foreach( var no0 in P0.FreeB.IEGet_BtoNo() ){
					int noB=(1<<no0);
					USuperLink USLK=pSprLKsMan.get_L2SprLKEx(P0.rc,no0,FullSearchB:true,DevelopB:false);
					if( USLK==null || !USLK.SolFound )  goto nextSearch;                   
                    for(int k=0; k<9; k++ ){
                        sTrue[k] &= (USLK.Qtrue[k] - USLK.Qfalse[k]);
                    }                  
                }

				for(int nox=0; nox<9; nox++){
					sTrue[nox].BPReset(P0.rc);
					if(!sTrue[nox].IsZero()) solvedB=true;
				}

                if(solvedB){                    
                    solvedA=true;
                    _ForceChainCellDispEx(sPass,sTrue,P0.rc); //q
                    if(!SDK_Ctrl.MltAnsSearch && dspOpt!="ForceL2") break;                    
                }

			  nextSearch:
                continue;
            }
            if(solvedA && !SDK_Ctrl.MltAnsSearch && dspOpt=="ForceL2") _ForceChainCellDispEx(sPass,null,-1);

            return (SolCode>0);
        }

        private  bool _ForceChainCellDispEx( Bit81[] sPass, Bit81[] sTrue, int rc0 ){   //q
            string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
            if(rc0<0){   //dspOpt:"ForceL2" 
                Result = ResultLong = "ForceChain_Cell";
                if(__SimpleAnalyzerB__)  return true;
				pAnMan.SnapSaveGP(true);
                return (SolCode>0);
            }

			UCell P0=pBDL[rc0];
			string st0="", st2="";
            for(int nox=0; nox<9; nox++ ){
				if(sTrue[nox].IsZero())  continue;

				foreach( var rc in sTrue[nox].IEGet_rc() ){
                    if(sPass[nox].IsHit(rc)) continue;
                    sPass[nox].BPSet(rc);

					UCell Q=pBDL[rc];
					Q.FixedNo=nox+1;
					int elm=Q.FreeB.DifSet(1<<nox);
					Q.CancelB = elm;
					SolCode=1;

					if(SolInfoB){   //SolInfoB:Flag whether to generate solution information
						if(dspOpt!="ForceL2") P0.SetNoBBgColor(P0.FreeB,Colors.Green,Colors.Yellow);
						Q.SetNoBBgColor(1<<nox, Colors.Red , Colors.LightGreen );
						Q.SetNoBColorRev(elm,Colors.Red );

                        st0 = $"ForceChain_Cell r{(Q.r+1)}c{(Q.c+1)}/{(nox+1)} is true";    //st0:Title of each solution

                        string st1="";
						foreach( var no in pBDL[rc0].FreeB.IEGet_BtoNo() ){
							USuperLink USLK = pSprLKsMan.get_L2SprLKEx(rc0,no,FullSearchB:false,DevelopB:false);
							st1 += "\r"+pSprLKsMan._GenMessage2true(USLK,Q,nox);            //st1:Exact GroupedLink path
						}
						st2 = st0+st1;                                                      //st2:Description of each solution
                        Result = ResultLong = st0; 
                        extRes += "\r"+st2;                                                 //(Description of each solution)
                        extRes = extRes.TrimStart();

						if(dspOpt=="ForceL0"){
                            if(__SimpleAnalyzerB__)  return true;
							if(!pAnMan.SnapSaveGP(false))  return true;
                            extRes=""; st2="";
						}
					}
                }
				if(SolInfoB && dspOpt=="ForceL1" && st2!=""){
                    Result = ResultLong = $"ForceChain_Cell (#{nox+1})";

                    if(__SimpleAnalyzerB__)  return true;
					if(!pAnMan.SnapSaveGP(false))  return true;
					st2="";
                    extRes="";
				}
			}
			return (SolCode>0);
        }

        private string GenMessage2FakeProposition(int rc0, Bit81[] fakeP, USuperLink USLK, UCell Q ){   //q
            string st="";
            foreach( var no in pBDL[rc0].FreeB.IEGet_BtoNo() ){
                if(fakeP[no].IsHit(rc0)){
                    st += $"ForceChain_Cell r{(rc0/9+1)}c{(rc0%9)+1}/{(no+1)} is false(contradition)";

                    st += "\r"+ pSprLKsMan._GenMessage2true(USLK,Q,no);
                    st += "\r"+ pSprLKsMan._GenMessage2false(USLK,Q,no);
                }
            }
            return st;
        }
    }  
}
