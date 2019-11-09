using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPZ_sdk{
    public partial class ALSTechGen: AnalyzerBaseV2{

        public bool ALS_XY_Wing( ){
			Prepare();

            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=2 ) return false;

            for( int szT=4; szT<15; szT++ ){    //Search in descending order of the total size of 3 ALS
                if( _ALSXYWingSub(szT) )  return true;
            }
            return false;
        }

        private bool _ALSXYWingSub( int szT ){

            //(ALS sorted by size)    
            foreach( var UC in ALSMan.ALSLst.Where(p=>p.Size<=szT-2) ){
                if( !UC.singly ) continue;
                int szS=szT-UC.Size;

                UALS UA, UB, UApre=null;
                int nxt=0, RccAC=-1, RccBC=-1;
                var cmb = new Combination(ALSMan.ALSLst.Count,2);       
                while( cmb.Successor(nxt) ){
                    nxt=0;
                    UA = ALSMan.ALSLst[cmb.Cmb[0]];
                    if( !UA.singly || UA==UC || UA.Size>szS-1 ) continue;
                    if( UA!=UApre ){
                        RccAC = ALSMan.Get_AlsAlsRcc(UA,UC); //RCC
                        if( RccAC.BitCount()!=1 ) continue;
                        UApre=UA;
                    }

                    UB = ALSMan.ALSLst[cmb.Cmb[1]];
                    if( !UB.singly || UB.Size>(szS-UA.Size) )  continue; //Skip using "Sort by size"

                    nxt=1;                        
                    if( UB==UC || UB.Size!=(szS-UA.Size) ) continue;
                    if( !(UA.B81&UB.B81).IsZero() )    continue; //Overlap
                    RccBC = ALSMan.Get_AlsAlsRcc(UB,UC);         //RCC
                    if( RccBC.BitCount()!=1 ) continue;
                    if( RccAC==RccBC ) continue;

                    int EFrB = (UA.FreeB&UB.FreeB).DifSet(RccAC|RccBC);
                    if( EFrB==0 ) continue;
                    foreach( var no in EFrB.IEGet_BtoNo() ){
                        int noB=(1<<no);
                        Bit81 UE = new Bit81();
                        foreach( var P in UA.UCellLst.Where(p=>(p.FreeB&noB)>0)) UE.BPSet(P.rc);
                        foreach( var P in UB.UCellLst.Where(p=>(p.FreeB&noB)>0)) UE.BPSet(P.rc);
                    
                        Bit81 TBD = ( new Bit81(pBDL,noB)) - (UA.B81|UB.B81|UC.B81);
                        foreach( var rc in TBD.IEGet_rc() ){
                            if( !(UE-ConnectedCells[rc]).IsZero() ) continue;
                            pBDL[rc].CancelB=noB; SolCode=2;
                        }
                    
                        if(SolCode>0){ //===== ALS XY-Wing found =====
                            ALSXYWing_SolResult(UA,UB,UC, RccAC, RccBC);
                            if( !pAnMan.SnapSaveGP(true) )  return true;
                        }
                    }
                }
            }
            return false;
        }  
        private void ALSXYWing_SolResult( UALS UA, UALS UB, UALS UC, int RccAC, int RccBC ){
            string st = "ALS XY-Wing ";            
            if( SolInfoB ){            
                foreach( var P in UA.UCellLst ) P.SetNoBBgColor(RccAC,AttCr,SolBkCr);
                foreach( var P in UB.UCellLst ) P.SetNoBBgColor(RccBC,AttCr,SolBkCr2);
                foreach( var P in UC.UCellLst ) P.SetNoBBgColor(RccAC|RccBC,AttCr,SolBkCr3);

                st += "\r ALS A: "+UA.ToStringRCN();
                st += "\r ALS B: "+UB.ToStringRCN();
                st += "\r ALS C: "+UC.ToStringRCN();
                st += "\r RCC AC: #"+RccAC.ToBitStringN(9);
                st += "\r RCC BC: #"+RccBC.ToBitStringN(9);
                ResultLong=st;
            }
            Result = "ALS XY-Wing";
        }
    }
}