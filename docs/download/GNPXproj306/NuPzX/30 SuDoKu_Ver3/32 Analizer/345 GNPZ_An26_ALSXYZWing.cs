using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class ALSTechGenEx: AnalyzerBaseV3{
		private int GStageMemo;

        public ALSTechGenEx( GNPX_AnalyzerManEx pAnMan ): base(pAnMan){
            this.pAnMan=pAnMan;
        }

		private void Prepare(){
			if(pAnMan.GStage!=GStageMemo) {
				GStageMemo=pAnMan.GStage;
				ALSMan.Initialize();
				ALSMan.PrepareALSLinkMan(1);
			}      
		}

        public bool XYZwingALS( ){
			Prepare();
            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=2 ) return false;

            for( int sz=3; sz<8; sz++ ){
                if( _XYZwingALSSub(sz) ) return true;
            }
            return false;
        }

        private bool _XYZwingALSSub( int wsz ){ //simple UVWXYZwing
            List<UCellEx> FBCX = pBDL.FindAll(p=>p.FreeBC==wsz);
            if( FBCX.Count==0 ) return false;

            foreach( var P0 in FBCX ){  //Forcused Cell
                int b0=P0.b;            //Forcused Block

                for( int no=0; no<9; no++ ){
                    int noB=1<<no;

                    Bit81 P0con = (new Bit81(pBDL,noB)) & ConnectedCells[P0.rc];
                    Bit81 Pin   = P0con&HouseCells[18+b0];

                    for( int dir=0; dir<2; dir++ ){ //dir 0:row 1:col
                        int rcDir = (dir==0)? P0.r: (9+P0.c);
                        Bit81 Pin2 = Pin-HouseCells[rcDir];　//ALS candidate position in the block
                        if( Pin2.IsZero() ) continue;

                        Bit81 Pout = (P0con&HouseCells[rcDir])-HouseCells[18+P0.b];//ALS candidate position outside the block
                        foreach( var ALSout in ALSMan.IEGetCellInHouse(1,noB,Pout,rcDir) ){ //ALS out of Forcused Block
                            int FreeBOut2 = ALSout.FreeB.DifSet(noB);
                            Bit81 EOut=new Bit81();　    //#no existence position(outer-ALS)
                            foreach( var P in ALSout.UCellLst.Where(p=>(p.FreeB&noB)>0) ) EOut.BPSet(P.rc);

                            foreach( var ALSin in ALSMan.IEGetCellInHouse(1,noB,Pin2,18+b0) ){
                                int FreeBin2 = ALSin.FreeB.DifSet(noB);

                                Bit81 Ein=new Bit81();   //#no existence position(inner-ALS)
                                foreach( var P in ALSin.UCellLst.Where(p=>(p.FreeB&noB)>0) ) Ein.BPSet(P.rc);

                                int Cover= P0.FreeB.DifSet(ALSout.FreeB|ALSin.FreeB);
                                if( Cover!=0 ) continue; //Numbers in inner-ALS and outer-ALS cover numbers in the Forcused cell
                                
                                Bit81 Epat= EOut|Ein; //Cells covered by excluded Cells&Number
                                if( Epat.IsZero() ) continue;
                                bool SolFond=false;
                                string msg3="";

                                int FreeBin3 = P0.FreeB.DifSet(FreeBOut2|FreeBin2); 
                                foreach( var E in pBDL.Where(p=>(p.FreeB&noB)>0) ){
                                    if( E.rc==P0.rc || Pout.IsHit(E.rc) || Pin2.IsHit(E.rc) )  continue;
                                    if( !(Epat-ConnectedCells[E.rc]).IsZero() )  continue;
                                    if( FreeBin3>0 && !ConnectedCells[E.rc].IsHit(P0.rc) )  continue;
                                    E.CancelB=noB; 
                                    SolFond=true;
                                    msg3 += " "+E.rc.ToRCString();
                                }

                                if(SolFond){
                                    SolCode=2;     
                                    string[] xyzWingName={ "XYZ-Wing","WXYZ-Wing","VWXYZ-Wing","UVWXYZ-Wing"};
                                    string SolMsg = xyzWingName[wsz-3]+"(ALS)";

                                    string msg0=" Pivot: "+P0.rc.ToRCString();
                                    string st=""; foreach( var P in ALSin.UCellLst ) st+=" "+P.rc.ToRCString();
                                    string msg1 = " in: "+st.ToString_SameHouseComp();
                                    st="";  foreach( var P in ALSout.UCellLst ) st+=" "+P.rc.ToRCString();
                                    string msg2 = " out: "+st.ToString_SameHouseComp();
                                    st=""; foreach( var rc in Pin2.IEGet_rc() ) st+=" "+rc.ToRCString();       
                                    Result = SolMsg+msg0+msg1+msg2;  
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}