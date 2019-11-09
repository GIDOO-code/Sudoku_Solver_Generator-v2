using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GroupedLinkGen: AnalyzerBaseV2{

        public bool ForceChain_Contradiction( ){
			string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
			string st0="", st2="";

			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			Bit81[] GLC=new Bit81[9];
			for( int k=0; k<9; k++ ) GLC[k]=new Bit81();  

			foreach( var P in pBDL.Where(p=>p.No==0) ){
				foreach( var no in P.FreeB.IEGet_BtoNo() ){
					int noB=(1<<no);
                    USuperLink USLK=pSprLKsMan.Eval_SuperLinkChain(P.rc,no,Gbreak:true, DevelopB:false);
					if( !USLK.SolFond ){
						P.CancelB |= noB;
						P.SetCellBgColor(Colors.LightGreen);	
						int E=P.FreeB.DifSet(P.CancelB);
						SolCode = (E.BitCount()==1)? 1: 2;

						st0 = "ForceChain_Contradiction r"+(P.r+1)+"c"+(P.c+1)+"/#"+(no+1)+" is false";
						Result = st0;

						if(SolInfoB){
							P.SetNoBColorRev(noB,Colors.Red );
							if(E.BitCount()==1 )  P.SetNoBColor(E,Colors.Red);												

							GLC[no].BPSet(P.rc);						
							st2 += (st0+"\r"+USLK.stMsg);
							ResultLong = st0;
							extRes = st2+" ";
							if( dspOpt=="ForceL0" ){
								if( !pAnMan.SnapSaveGP(false) )  return true;
								st2="";
							}
						}
					}
                }
				if( SolInfoB && dspOpt=="ForceL1" && st2!="" ){	
					st0 = "ForceChain_Contradiction";
					Result = st0;
					ResultLong = st0;
					extRes = st2;
					if( !pAnMan.SnapSaveGP(false) )  return true;
					st2="";
				}
            }
			if( SolInfoB && dspOpt=="ForceL2" && st2!="" ){	
				st0 = "ForceChain_Contradiction";
				Result = st0;
				ResultLong = st0;
				extRes = st2;
				if( !pAnMan.SnapSaveGP(false) )  return true;
			}
			_developDisp2( GLC );						//37		

            return (SolCode>0);
        }
        private  void _developDisp2( Bit81[] GLC ){
			List<UCell> qBDL=new List<UCell>();
            pBDL.ForEach(p=>qBDL.Add(p.Copy()));
			foreach( var P in qBDL.Where(q=>q.FreeB>0) ){
				int E=0;
				for( int n=0; n<9; n++ ){ if( GLC[n].IsHit(P.rc) ) E|=(1<<n); }
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
		public bool ForceChainNetDevelop( ){
			Prepare();
			string SolMsg="";
			pSprLKsMan.Initialize();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			Bit81[] GLC=new Bit81[9];
			for( int k=0; k<9; k++ ) GLC[k]=new Bit81();  
			foreach( var P in pBDL.Where(p=>p.No==0) ){
				foreach( var no in P.FreeB.IEGet_BtoNo() ){
					int noB=(1<<no);
                    USuperLink USLK=pSprLKsMan.Eval_SuperLinkChain(P.rc,no,Gbreak:false, DevelopB:false);
					if( USLK.SolFond ){
						GLC[no].BPSet(P.rc);
						pSprLKsMan.developDisp( P.rc, no, USLK, DevelopB:true );
						SolMsg = USLK.stMsg;
						Result = "ForceChainNetDevelop";
						ResultLong = "ForceChainNetDevelop";
						SolCode=2;
						goto LBreak;
					}
                }
            }
		LBreak:
			extRes = SolMsg;
            return (SolCode>0);
		}

		private string HouseToString(int hs){
			string st;
			if(hs<9)  st="row"+(hs+1);
			else if(hs<18) st="col"+(hs-8);
			else st="blk"+(hs-17);
			return st;
		}
        private void _Dev_PutDBL( string dir, string fName, bool append ){
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using( var fp=new StreamWriter(dir+@"\"+fName,append) ){  
                string st=pBDL.ConvertAll(P=>P.No).Connect("").Replace("-","+").Replace("0",".");
                fp.WriteLine(st);
            }
        }
    }  
}