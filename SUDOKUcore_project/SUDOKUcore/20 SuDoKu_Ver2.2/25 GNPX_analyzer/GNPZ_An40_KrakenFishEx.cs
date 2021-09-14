using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using static System.Diagnostics.Debug;

namespace GNPXcore{

//in development
    public partial class GroupedLinkGen: AnalyzerBaseV2{

		private string _krfMsg2;
        public bool KrakenFishEx( ){
			Prepare();
			for(int sz=2; sz<5; sz++ ){
				for(int no=0; no<9; no++ ){
					if( ExtKrFishSubEx(sz,no,27,FinnedF:false) ) return true;
				}
            }
            return false;
        }
        public bool KrakenFinnedFishEx( ){
			Prepare();
			for(int sz=2; sz<7; sz++ ){
				for(int no=0; no<9; no++ ){
					if(ExtKrFishSubEx(sz,no,27,FinnedF:true) )  return true;
				}
            }
            return false;
        }
        public bool ExtKrFishSubEx( int sz, int no, int FMSize, bool FinnedF ){  
			int BaseSel=0x7FFFFFF, CoverSel=0x7FFFFFF;
            int noB=(1<<no);

			string krfSolMsg="";
			FishMan FMan=new FishMan(this,FMSize,no,sz,(sz>=3));
            foreach( var Bas in FMan.IEGet_BaseSet(BaseSel,FinnedF:FinnedF) ){    //Generate BaseSet
                foreach( var Cov in FMan.IEGet_CoverSet(Bas,CoverSel,FinnedF) ){  //Generate CoverSet  
                    Bit81 FinB81 = Cov.FinB81;
							//dbCC++;//##############
                    if( FinnedF!=FinB81.IsZero() ){
							//WriteLine( $"dbCC:{dbCC} \rbas:{Bas.BaseB81}\rCov:{Cov.CoverB81}" );//######
							//WriteLine( $"Bas.HouseB:{Bas.HouseB.ToBitString27()}");
							//WriteLine( $"Cov.HouseB:{Cov.HouseC.ToBitString27()}");
							
						Bit81 UsedB = Bas.BaseB81;								
						foreach( var Hb in Bas.HouseB.IEGet_BtoNo(27) ){
							Bit81 E = Bas.BaseB81&HouseCells[Hb];
							if(FinnedF) E|=FinB81;	//Finned
							if( E.IsZero() )  continue;

							foreach( var P in pBDL.Where(p=> !UsedB.IsHit(p.rc)) ){
								foreach( var noZ in P.FreeB.IEGet_BtoNo() ){
									int noZb=(1<<noZ);
                                    USuperLink USLK=pSprLKsMan.get_L2SprLKEx(P.rc,no,FullSearchB:false,DevelopB:false);
                                    //WriteLine( $" USuperLink rc:{P.rc} no:{noZ+1}" );
                                    if( !USLK.SolFound ) continue;
									Bit81 Ef = E - USLK.Qfalse[no];
									if( !Ef.IsZero() )  continue;

									//Accurate analysis
                                    USLK=pSprLKsMan.get_L2SprLKEx(P.rc,noZ,FullSearchB:true,DevelopB:false);//#####
									Ef = E - USLK.Qfalse[no];
									if( !Ef.IsZero() )  continue;
									P.CancelB|=noZb;
									SolCode=2;

									if(SolInfoB){
										_KrFish_FishResultEx(no,sz,Bas,Cov);
										krfSolMsg += $"\r{_krfMsg2}  r{(P.r+1)}c{(P.c+1)}/{(noZ+1)} is false";
										foreach( var rc in E.IEGet_rc() ){
											krfSolMsg += "\r"+pSprLKsMan._GenMessage2false(USLK,pBDL[rc],no);
										}
									}
									//goto LSolFound;
								}
							}

					//	LSolFound:
							if(SolCode>0){
								if(SolInfoB) extRes = krfSolMsg;
                                if(__SimpleAnalyzerB__)  return true;
								if(!pAnMan.SnapSaveGP(false)) return true; 
							}
						}
                    }
                }
            }
            return false;
        }

        private void _KrFish_FishResultEx( int no, int sz, UFish Bas, UFish Cov ){
            int   HB=Bas.HouseB, HC=Cov.HouseC;
            Bit81 PB=Bas.BaseB81, PFin=Cov.FinB81; 
            Bit81 EndoFin=Bas.EndoFin, CnaaFin=Cov.CannFin;
            string[] FishNames={ "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
    
            PFin-=EndoFin;
            try{
                int noB=(1<<no);                 
                foreach( var P in PB.IEGet_rc().Select(p=>pBDL[p]) )   P.SetNoBBgColor(noB,AttCr,SolBkCr);
                foreach( var P in PFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr2);
                foreach( var P in EndoFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr3);
                foreach( var P in CnaaFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr3);

                string msg = "\r     Digit: " + (no+1);                 
                msg += "\r   BaseSet: " + HB.HouseToString();
                msg += "\r  CoverSet: " + HC.HouseToString();;
                string msg2=$" #{(no+1)} {HB.HouseToString().Replace(" ","")}/{HC.HouseToString().Replace(" ","")}";
 
                string FinmsgH="", FinmsgT="";
                if(PFin.Count>0){
                    FinmsgH = "Finned ";
                    string st="";
                    foreach( var rc in PFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r    FinSet: "+st.ToString_SameHouseComp();
                
                }
                 
                if(!EndoFin.IsZero()){
                    FinmsgT = " with Endo Fin";
                    string st="";
                    foreach( var rc in EndoFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Endo Fin: "+st.ToString_SameHouseComp();
                }

                if(!CnaaFin.IsZero()){
                    FinmsgH = "Cannibalistic ";
                    if( PFin.Count>0 ) FinmsgH = "Finned Cannibalistic ";
                    string st="";
                    foreach( var rc in CnaaFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Cannibalistic: "+st.ToString_SameHouseComp();
                }

                string Fsh = FishNames[sz-2];
				int bf=0, cf=0;
				for(int k=0; k<3; k++ ){
					if( ((Bas.HouseB>>(k*9))&0x1FF)>0 ) bf|=(1<<k);
					if( ((Cov.HouseC>>(k*9))&0x1FF)>0 ) cf|=(1<<k);
				}
                if((bf+cf)>3) Fsh = "Franken/Mutant "+Fsh;
                Fsh = "Kraken "+FinmsgH+Fsh+FinmsgT;
                ResultLong = Fsh+msg;  
                _krfMsg2=Fsh.Replace("Franken/Mutant","F/M")+msg2;
				Result=_krfMsg2;
            }
            catch(Exception ex){
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);
            }
        }

        private void __Dev_PutDBLEx( string dir, string fName, bool append ){
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using( var fp=new StreamWriter(dir+@"\"+fName,append:append,encoding:Encoding.UTF8) ){  
                string st=pBDL.ConvertAll(P=>P.No).Connect("").Replace("-","+").Replace("0",".");
                fp.WriteLine(st);
            }
        }
    }  
}