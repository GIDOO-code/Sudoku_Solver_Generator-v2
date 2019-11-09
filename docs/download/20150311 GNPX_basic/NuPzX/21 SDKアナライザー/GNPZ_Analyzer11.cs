using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GNPZ_Analyzer{     
        public bool GNP00_XYZwing( ){    return _GNP00_UVWXYZwing(3); }  //XYZ-wing
        public bool GNP00_WXYZwing( ){   return _GNP00_UVWXYZwing(4); }  //WXYZ-wing
        public bool GNP00_VWXYZwing( ){  return _GNP00_UVWXYZwing(5); }  //VWXYZ-wing
        public bool GNP00_UVWXYZwing( ){ return _GNP00_UVWXYZwing(6); }  //UVWXYZ-wing

        public bool _GNP00_UVWXYZwing( int wsz ){ //simple UVWXYZwing
            List<UCell> FBCX = pBDL.FindAll(p=>p.FreeBC==wsz);
            if( FBCX.Count==0 ) return false;

            bool wingF=false;
            foreach( var P0 in FBCX ){  //着目セル
                int b0=P0.b;            //着目ブロック
                foreach( int no in P0.FreeB.IEGet_BtoNo() ){ //着目数字
                    int noB=1<<no;
                    Bit81 P0con = (new Bit81(pBDL,noB,FreeBC:2)) & ConnectedCells[P0.rc];
                    Bit81 Pin   = P0con&HouseCells[18+P0.b];
                 
                    Bit81 Pout=null, Pin2=null;
                    for( int dir=0; dir<2; dir++ ){ //dir 0:row 1:col
                        int rcDir = (dir==0)? P0.r: (9+P0.c);
                        Pin2 = Pin-HouseCells[rcDir];
                        if( Pin2.IsZero() ) continue;
                        Pout = (P0con&HouseCells[rcDir])-HouseCells[18+P0.b];
                        if( Pin2.Count+Pout.Count != (wsz-1) ) continue;

                        int FreeBin = Pin2.AggregateFreeB(pBDL);
                        int FreeBout = Pout.AggregateFreeB(pBDL);
                        if( (FreeBin|FreeBout)!=P0.FreeB ) continue;
                        Bit81 ELst = HouseCells[rcDir]&HouseCells[18+P0.b];
                        ELst.BPReset(P0.rc);
                        string msg3="";
                        foreach( var E in ELst.IEGet_rc().Select(p=>pBDL[p]) ){
                            if( (E.FreeB&noB)>0 ){
                                E.CancelB=noB; wingF=true; 
                                msg3 += " "+E.rc.ToRCString();
                            }
                        }
                        if( !wingF )  continue;
                        
                        //--- ...wing fond -------------
                        SolCode=2;     
                        string[] xyzWingName={ "XYZ-Wing","WXYZ-Wing","VWXYZ-Wing","UVWXYZ-Wing"};
                        Result = xyzWingName[wsz-3];

                        if( SolInfoDsp ){
                            P0.SetNoBBgColor(P0.FreeB,AttCr,SolBkCr2);
                            foreach( var P in Pin2.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr);
                            foreach( var P in Pout.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr);

                            string msg0=" Pivot: "+P0.rc.ToRCString();
                            string st=""; foreach( var rc in Pin2.IEGet_rc() ) st+=" "+rc.ToRCString();
                            string msg1 = " in: "+st.ToString_SameHouseComp();
                            st="";  foreach( var rc in Pout.IEGet_rc() ) st+=" "+rc.ToRCString();
                            string msg2 = " out: "+st.ToString_SameHouseComp();
                            st=""; foreach( var rc in Pin2.IEGet_rc() ) st+=" "+rc.ToRCString();
        
                            ResultLong = Result+"\r"+msg0+ "\r   "+msg1+ "\r  "+msg2+ "\r Eliminated: "+msg3.ToString_SameHouseComp();
                            Result += msg0+msg1+msg2;      
                        }
                        if( !SnapSaveGP(true) )  return true;
                        wingF=false;
                    }
                }
            }
            return false;
        }

    }
}