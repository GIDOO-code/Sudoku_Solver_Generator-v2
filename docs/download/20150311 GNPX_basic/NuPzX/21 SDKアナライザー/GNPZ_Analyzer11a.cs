using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GNPZ_Analyzer{     
        public bool GNP00_XYZwingALS( ){
            ALSMan.ALS_Search(1);
            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=2 ) return false;

            for( int sz=3; sz<8; sz++ ){
                if( _GNP00_XYZwingALSSub(sz) ) return true;
            }
            return false;
        }

        public bool _GNP00_XYZwingALSSub( int wsz ){ //simple UVWXYZwing
            List<UCell> FBCX = pBDL.FindAll(p=>p.FreeBC==wsz);
            if( FBCX.Count==0 ) return false;

            foreach( var P0 in FBCX ){  //着目セル
                int b0=P0.b;            //着目ブロック

                for( int no=0; no<9; no++ ){
                    int noB=1<<no;

                    Bit81 P0con = (new Bit81(pBDL,noB)) & ConnectedCells[P0.rc];
                    Bit81 Pin   = P0con&HouseCells[18+b0];

                    for( int dir=0; dir<2; dir++ ){ //dir 0:row 1:col
                        int rcDir = (dir==0)? P0.r: (9+P0.c);
                        Bit81 Pin2 = Pin-HouseCells[rcDir];　//ブロック内ALS存在候補位置
                        if( Pin2.IsZero() ) continue;

                        Bit81 Pout = (P0con&HouseCells[rcDir])-HouseCells[18+P0.b];//ブロック外ALS存在候補位置
                        foreach( var ALSout in ALSMan.IEGet(1,noB,Pout,rcDir) ){     //ブロック外ALS
                            int FreeBOut2 = ALSout.FreeB.DifSet(noB);
                            Bit81 EOut=new Bit81();　    //ブロック外ALSの#no存在位置
                            foreach( var P in ALSout.UCellLst.Where(p=>(p.FreeB&noB)>0) ) EOut.BPSet(P.rc);

                            foreach( var ALSin in ALSMan.IEGet(1,noB,Pin2,18+b0) ){
                                int FreeBin2 = ALSin.FreeB.DifSet(noB);

                                Bit81 Ein=new Bit81();   //ブロック内ALSの#no存在位置
                                foreach( var P in ALSin.UCellLst.Where(p=>(p.FreeB&noB)>0) ) Ein.BPSet(P.rc);

                                int Cover=P0.FreeB.DifSet(ALSout.FreeB|ALSin.FreeB);
                                if( Cover!=0 ) continue; //内ALSと外ALSの数字が軸セルの数字をカバーしている
                                
                                Bit81 Epat=EOut|Ein; //除外セルがカバーすべき全セル
                                if( Epat.IsZero() ) continue;
                                bool SolFond=false;
                                string msg3="";

                                int FreeBin3 = P0.FreeB.DifSet(FreeBOut2|FreeBin2); 
                                foreach( var E in pBDL.Where(p=>(p.FreeB&noB)>0) ){
                                    if( E.rc==P0.rc || Pout.IsHit(E.rc) || Pin2.IsHit(E.rc) )  continue;
                                    if( !(Epat-ConnectedCells[E.rc]).IsZero() )  continue;
                                    if( FreeBin3>0 && !ConnectedCells[E.rc].IsHit(P0.rc) )  continue;
                                    E.CancelB=noB; SolFond=true;
                                    msg3 += " "+E.rc.ToRCString();
                                }

                                if( SolFond ){
                                    SolCode=2;     
                                    string[] xyzWingName={ "XYZ-Wing","WXYZ-Wing","VWXYZ-Wing","UVWXYZ-Wing"};
                                    Result = xyzWingName[wsz-3]+"(ALS)";

                                    if( SolInfoDsp ){
                                        P0.SetNoBBgColor(P0.FreeB,AttCr,SolBkCr2);
                                        foreach( var P in ALSin.UCellLst  ) P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr);
                                        foreach( var P in ALSout.UCellLst ) P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr);

                                        string msg0=" Pivot: "+P0.rc.ToRCString();
                                        string st=""; foreach( var P in ALSin.UCellLst ) st+=" "+P.rc.ToRCString();
                                        string msg1 = " in: "+st.ToString_SameHouseComp();
                                        st="";  foreach( var P in ALSout.UCellLst ) st+=" "+P.rc.ToRCString();
                                        string msg2 = " out: "+st.ToString_SameHouseComp();
                                        st=""; foreach( var rc in Pin2.IEGet_rc() ) st+=" "+rc.ToRCString();
        
                                        ResultLong = Result+"\r"+msg0+ "\r   "+msg1+ "\r  "+msg2+ "\r Eliminated: "+msg3.ToString_SameHouseComp();
                                        Result += msg0+msg1+msg2;      
                                    }
                                    if( !SnapSaveGP(true) )  return true;
                                    foreach( var E in pBDL.Where(p=>(p.FreeB&noB)>0) ) E.CancelB=0;
                                    SolFond=false;
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