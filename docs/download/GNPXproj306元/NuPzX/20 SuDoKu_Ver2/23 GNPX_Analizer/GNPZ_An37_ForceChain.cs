using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using static System.Console;
using System.Threading;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GroupedLinkGen: AnalyzerBaseV2{
		public static DevelopWin devWin; //## development

		public bool ForceChain_Cell( ){
			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			Bit81[] GLC=new Bit81[9];
			for( int k=0; k<9; k++ ) GLC[k]=new Bit81(all:true);

			foreach( var P0 in pBDL.Where(p=>(p.FreeB>0)) ){
				Bit81[] sTrue=new Bit81[9];
				for( int k=0; k<9; k++ ) sTrue[k]=new Bit81(all:true);

				foreach( var noH in P0.FreeB.IEGet_BtoNo() ){
					int noB=(1<<noH);
					USuperLink USLK=pSprLKsMan.get_L2SprLK(P0.rc,noH,CorrectF:false,DevelopB:false);
					if(!USLK.SolFond)  goto nextSearch;
					for( int k=0; k<9; k++ ) sTrue[k] &= USLK.Qtrue[k];
				}

				bool solved=false;
				for( int nox=0; nox<9; nox++ ){
					sTrue[nox].BPReset(P0.rc);
                    //Thread.Sleep(1);
					if( !sTrue[nox].IsZero() )  solved=true;
				}

				if(solved) _ForceChainCellDisp(sTrue,P0.rc);

			  nextSearch:
				continue;
            }
            return (SolCode>0);
        }
        private  bool _ForceChainCellDisp( Bit81[] sTrue, int mX ){
			UCell P0=pBDL[mX];
			string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
			string st0="", st2="";

			for( int nox=0; nox<9; nox++ ){
				if( sTrue[nox].IsZero() )  continue;

				foreach( var rc in sTrue[nox].IEGet_rc() ){
					UCell Q=pBDL[rc];
					Q.FixedNo=nox+1;
					int elm=Q.FreeB.DifSet(1<<nox);
					Q.CancelB = elm;

					SolCode=1;
					st0 = "ForceChain_Cell r"+(Q.r+1)+"c"+(Q.c+1)+"/+"+(nox+1)+" is true";
					Result = st0;

					if(SolInfoB){
						P0.SetNoBBgColor(P0.FreeB,Colors.Green,Colors.Yellow);
						Q.SetNoBBgColor(1<<nox, Colors.Red , Colors.LightGreen );
						Q.SetNoBColorRev(elm,Colors.Red );

						string st1="";
						foreach( var no in pBDL[mX].FreeB.IEGet_BtoNo() ){
							USuperLink USLK = pSprLKsMan.get_L2SprLK(mX,no,CorrectF:true,DevelopB:false); //Accurate path
							st1 += "\r"+pSprLKsMan._GenMessage2true(USLK,Q,nox);
						}

						if(st2!="") st2+="\r";
						st2 += st0+st1;
						ResultLong = st0;
						extRes = st2;

						if( dspOpt=="ForceL0" ){
							if( !pAnMan.SnapSaveGP(false) )  return true;
							st2="";
						}
					}
				}
				if( SolInfoB && dspOpt=="ForceL1" && st2!="" ){
					st0 = "ForceChain_Cell";
					Result = st0;
					ResultLong = st0;
					extRes = st2;
					if( !pAnMan.SnapSaveGP(false) )  return true;
					st2="";
				}
			}
			if( SolInfoB && dspOpt=="ForceL2" && st2!="" ){	
				st0 = "ForceChain_Cell";
				Result = st0;
				ResultLong = st0;
				extRes = st2;
				if( !pAnMan.SnapSaveGP(false) )  return true;
			}
			return (SolCode>0);
        }
		
		public bool ForceChain_House( ){
            //int chkID=0;
			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			Bit81[] GLC=new Bit81[9];
			for( int k=0; k<9; k++ ) GLC[k]=new Bit81(all:true);

			for( int hs=0; hs<27; hs++ ){
				int noBs=pBDL.IEGetCellInHouse(hs).Aggregate(0,(Q,P)=>Q|(P.FreeB));

				foreach( var noH in noBs.IEGet_BtoNo() ){
					int noB=(1<<noH);
					Bit81[] sTrue=new Bit81[9];
					for( int k=0; k<9; k++ ) sTrue[k]=new Bit81(all:true);

					foreach( var P in pBDL.IEGetCellInHouse(hs,noB) ){
						USuperLink USLK=pSprLKsMan.get_L2SprLK(P.rc,noH,CorrectF:false,DevelopB:false); //#######false
						if(!USLK.SolFond)  goto nextSearch;
						for( int k=0; k<9; k++ ) sTrue[k] &= USLK.Qtrue[k];
					}

					bool solved=false;
					for( int nox=0; nox<9; nox++ ){
						sTrue[nox] -= HouseCells[hs];
						if( !sTrue[nox].IsZero() ){
                            solved=true;
                            //Thread.Sleep(1);
                            //WriteLine("ForceChain_House:"+(chkID++));
                        }
					}

					if(solved)  _ForceChainHouseDisp(sTrue,hs,noH);
				}
			  nextSearch:
				continue;
            }
            return (SolCode>0);
        }	
		private  bool _ForceChainHouseDisp( Bit81[] sTrue, int hs, int noH ){
			string dspOpt = GNPXApp000.GMthdOption["ForceLx"];

			string st0="", st2="";
			UCell P0=pBDL[hs];
			for( int nox=0; nox<9; nox++ ){
				if( sTrue[nox].IsZero() )  continue;

				foreach( var rc in sTrue[nox].IEGet_rc() ){
					UCell Q=pBDL[rc];
					Q.FixedNo=nox+1;
					int elm=Q.FreeB.DifSet(1<<nox);
					Q.CancelB = elm;
					SolCode=1;
					st0 = "ForceChain_House("+HouseToString(hs)+"/#"+(noH+1)+") r"+(Q.r+1)+"c"+(Q.c+1)+"/+"+(nox+1)+" is true";
					Result = st0;

					if(SolInfoB){
						Q.SetNoBBgColor(1<<nox, Colors.Red , Colors.LightGreen );
						Q.SetNoBColorRev(elm,Colors.Red );

						string st1="";
						foreach( var P in pBDL.IEGetCellInHouse(hs,1<<noH) ){
							USuperLink USLK = pSprLKsMan.get_L2SprLK(P.rc,noH,CorrectF:true,DevelopB:false); //Accurate path
							st1 += "\r"+pSprLKsMan._GenMessage2true(USLK,Q,nox);
							P.SetNoBBgColor(1<<noH, Colors.Green , Colors.Yellow );
						}

						if(st2!="") st2+="\r";
						st2 += st0+st1;
						ResultLong = st0;
						extRes = st2;

						if( dspOpt=="ForceL0" ){
							if( !pAnMan.SnapSaveGP(false) )  return true;
							st2="";
						}
					}
				}
				if( SolInfoB && dspOpt=="ForceL1" && st2!="" ){	
					st0 = "ForceChain_House("+HouseToString(hs)+"/#"+(noH+1)+")";
					Result = st0;
					ResultLong = st0;
					extRes = st2;
					if( !pAnMan.SnapSaveGP(false) )  return true;
					st2="";
				}
			}
			if( SolInfoB && dspOpt=="ForceL2" && st2!="" ){	
				st0 = "ForceChain_House("+HouseToString(hs)+")";
				Result = st0;
				ResultLong = st0;
				extRes = st2;
				if( !pAnMan.SnapSaveGP(false) )  return true;
			}
			return (SolCode>0);
        }
    }  
}