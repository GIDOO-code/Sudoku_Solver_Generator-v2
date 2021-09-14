using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPXcore{
    public partial class GroupedLinkGen: AnalyzerBaseV2{
        public bool ForceChain_ContradictionEx( ){
            GroupedLink._IDsetB=false;  //ID set for debug
			string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
			string st0="", st2="";

			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			Bit81[] GLC=new Bit81[9];
			for(int k=0; k<9; k++ ) GLC[k]=new Bit81();  

			foreach( var P0 in pBDL.Where(p=>p.No==0) ){
				foreach( var no in P0.FreeB.IEGet_BtoNo() ){
					int noB=(1<<no);
                    USuperLink USLK=pSprLKsMan.get_L2SprLKEx(P0.rc,no,FullSearchB:false,DevelopB:false);
					if(USLK!=null){
                        Bit81 sContradict = new Bit81();

                        for(int k=0; k<9; k++ ){
                            sContradict = USLK.Qtrue[k] & USLK.Qfalse[k];
                            if(sContradict.IsZero()) continue;

                            foreach( var Q in sContradict.IEGet_rc().Select(rc=>pBDL[rc])){
						        P0.CancelB |= noB;
						        P0.SetCellBgColor(Colors.LightGreen);	
						        int E=P0.FreeB.DifSet(P0.CancelB);
						        SolCode = (E.BitCount()==1)? 1: 2;

						        if(SolInfoB){
                                    if(dspOpt!="ForceL2") Q.SetNoBBgColor(Q.FreeB,Colors.Green,Colors.Yellow);
							        P0.SetNoBColorRev(noB,Colors.Red );
							        if(E.BitCount()==1)  P0.SetNoBColor(E,Colors.Red);												
							        GLC[no].BPSet(P0.rc);
						            st0 = $"ForceChain_Contradiction r{(P0.r+1)}c{(P0.c+1)}/#{(no+1)} is false";
                                    Result = ResultLong = st0;
                          
                                    string stX = pSprLKsMan._GenMessage2true(USLK,Q,k);
                                    stX += "\r"+ pSprLKsMan._GenMessage2false(USLK,Q,k);

                                    if(st2!="")  st2+="\r";
                                    st2 += (st0+ "\r" + stX);
                                    st2 = st2.Trim();
							        extRes = st2;
							        if(dspOpt=="ForceL0"){
                                        if(__SimpleAnalyzerB__)  return true;
								        if(!pAnMan.SnapSaveGP(false))  return true;
								        st2="";
							        }
						        }
                                goto LNextSearch;      //One contradictory cell is enough
                            }
					    }
                    }

                  LNextSearch:
				    if(SolInfoB && dspOpt=="ForceL1" && st2!=""){	
					    Result = ResultLong = $"ForceChain_Contradiction {P0.rc.ToRCString()}";
					    extRes = st2;
                        if(__SimpleAnalyzerB__)  return true;
					    if(!pAnMan.SnapSaveGP(false))  return true;
					    st2="";

                    }
                } 
            }
			if( SolInfoB && dspOpt=="ForceL2" && st2!="" ){	
				Result = ResultLong = "ForceChain_Contradiction";
				extRes = st2;
                if(__SimpleAnalyzerB__)  return true;
				if(!pAnMan.SnapSaveGP(false))  return true;
			}
			_developDisp2Ex( GLC );						//37		
            return (SolCode>0);
        }

        private  void _developDisp2Ex( Bit81[] GLC ){
			List<UCell> qBDL=new List<UCell>();
            pBDL.ForEach(p=>qBDL.Add(p.Copy()));
			foreach( var P in qBDL.Where(q=>q.FreeB>0) ){
				int E=0;
				for(int n=0; n<9; n++ ){ if( GLC[n].IsHit(P.rc) ) E|=(1<<n); }
				if(E>0){
					UCell Q=pBDL[P.rc];
					P.SetNoBBgColor(E, Colors.White , Colors.PowderBlue );
					P.SetNoBColorRev(E,Colors.Blue );

					Q.SetNoBBgColor(E, Colors.White , Colors.PowderBlue );
					Q.SetNoBColorRev(E,Colors.Red );
					int sb = Q.FreeB.DifSet(E);
					Q.CancelB=E;
					if(sb.BitCount()==1){
						Q.FixedNo=sb.BitToNum()+1;
						SolCode=1;
					}
					else if(sb.BitCount()==0){ Q.SetCellBgColor(Colors.Violet); }
				}
            }
            devWin.Set_dev_GBoard( qBDL );
        }
    }
}