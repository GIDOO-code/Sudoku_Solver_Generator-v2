using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;

//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*====*==*==*==*
//  Add description to code GeneralLogic and its auxiliary routein.
//  "GeneralLogic" completeness is about 40%.
//    Currently,it takes a few seconds to solve a size 3 problem.
//    As an expectation, I would like to solve a size 5 problem in a few seconds.
//    Probably need a new theory.
//
//  The following contains a lot of development code.
//
//*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

namespace GNPXcore{
    
        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  
    public class UGLink_unit{
        static public Bit81[]   pConnectedCells;
        private int  UGL;
        public  int  IDsq;
        public  List<int>  ConnectLinks=new List<int>();     //Links
        public  List<int>  rcnElementLst=new List<int>();
        public long    RCBN_frameB;             //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]
        public long    RCBN_frameB_Remove_tfx;  //Remove tfx from RCBN_frameB

        public  Bit81  rcBit81=new Bit81();         //rcb_link and used to identify rcb_link_type(not null)     
        public  Bit981 rcnBit =new Bit981();        //bit expression of rc[n] of GLink elements.
        public  Bit981 rcnConnected=new Bit981();   //bit expression of rc[n] of Connected Cells of GLink elements.  (used only in Check_1)##
        public  int    FreeB;
        public  int    _gen;

        public  int  IDunit{ get=> UGL&0xFFF; }
        public  int  SW{ set=>UGL|=(value<<16); get=>((UGL>>16)&1); }        //0:strong 1:weak
        public  int  type{ set=> UGL|=(value<<17); get=>((UGL>>17)&1); }     //0:link   1:cell
        public  int  tfx{  set=> UGL|=(value<<4); get=>(UGL>>4)&0x1F; }
                
        public  int  no{ set=> UGL|=value; get=>(UGL&0xF); }
        public  int  rc{ set=> UGL|=(value<<4); get=>(UGL>>4)&0xFF; } 
        public  UGLink_unit(){ }
        public  UGLink_unit( int IDx, int SW, int  tfx, int no, List<UCell> PLst ){    //link
            this.IDsq=IDx; this.SW=(SW&1); this.type=0; this.tfx=tfx; this.no=no;
            var _conn=new Bit81();
            foreach( var UC in PLst){
                ConnectLinks.Add(UC.rc<<4|no);
                rcnBit.BPSet(no,UC.rc); 
                rcBit81.BPSet(UC.rc);
                rcnElementLst.Add((UC.rc<<4|no));
                           
                //bit expression of rcbn. [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]
                int _rcbFrame = (1<<UC.r | 1<<(UC.c+9) | 1<<(UC.b+18));
                RCBN_frameB |= (long)_rcbFrame | ((long)1<<(no+27));      //bit expression of frame
                RCBN_frameB_Remove_tfx |= RCBN_frameB;
                _conn |= pConnectedCells[UC.rc];
                foreach( var no2 in Enumerable.Range(0,9)) rcnConnected.BPSet(no2,UC.rc);
            }
            RCBN_frameB_Remove_tfx = RCBN_frameB_Remove_tfx.DifSet(tfx);     //Remove tfx from RCBN_frameB
            rcnConnected._BQ[no] = _conn-rcBit81;
        }
        public  UGLink_unit( int IDx, int SW, int rc, int FreeB ){                     //Cell
            this.IDsq=IDx; this.SW=(SW&1); this.type=1; this.rc=rc; no=0xF; this.FreeB=FreeB;
            foreach(var no in FreeB.IEGet_BtoNo()){ 
                ConnectLinks.Add(rc<<4|no);
                rcnBit.BPSet(no,rc); 
                rcBit81.BPSet(rc);
                rcnElementLst.Add((rc<<4|no));
            }
            //bit expression of rcbn. [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]
            int _rcbFrame = (1<<rc/9 | 1<<(rc%9+9) | 1<<(rc.ToBlock()+18));
            RCBN_frameB = (long)_rcbFrame | ((long)FreeB)<<27;                   //bit expression of frame
            RCBN_frameB_Remove_tfx  = (long)_rcbFrame | (long)FreeB<<27;   //Remove tfx from RCBN_frameB
            foreach( var no2 in Enumerable.Range(0,9)) rcnConnected._BQ[no2] |= pConnectedCells[rc];
        }

        public bool GetGroup_true(Bit981B groop){
            return ConnectLinks.Any(P=> groop.IsTrue(P&0xF,P>>4));
        }
        public bool GetGroup_false(Bit981B groop){
            return ConnectLinks.Any(P=> groop.IsFalse(P&0xF,P>>4));
        }

        public override string ToString(){
            string st=$"UGLink_unit ID:{IDsq} SW:{SW} type:{type} ";
            string st2="";
            if(type==0){ st += $"tfx:{tfx} no:{no+1} <link>"; st2+=$" {tfx.tfxToString()}#{no+1}"; }       //0:link
            else{        st += $"rc:{rc}  <cell>"; }                 //1:cell

            st += st2;
            return st;
        }
        public string ToAppearance( bool Gen=false){
            string st;
            if(type==0){ st = $"{tfx.tfxToString()}#{no+1}"; }       //0:link
            else{        st = $"{rc.ToRCString()}"; }
            if(Gen)  st += $"G{_gen}";
            return st;
        }

        public bool existAnotherLink( int rcX ){
            if(this.type==1) return false;
            var Pindex = rcnElementLst.FindIndex(P=>((P>>4)==rcX));
            if(Pindex<0) return false;

            foreach( var rc in ConnectLinks.Select(p=>(p>>4))){
                if(rc==rcX) continue;
                if( pConnectedCells[rc].IsHit(rcX) )  return true;
            }

            return false;
;        }
    }
    public class UGLink_pair{
        private int  UGL;       //
//        public  int  IDsq;      //UGLink_unit reference index
        public  UGLink_unit objUGL;

        public int rcno{ get => (UGL&0xFFF); }
        public int SW{ set=> UGL|=(value<<16); get=>((UGL>>16)&1); }     //T:strong F:weak
                
        public int  rc2{ set=> UGL|=(value<<4); get=>((UGL>>4)&0xFF); }    
        public int  no2{ set=> UGL|=value; get=>(UGL&0xF); }

        public UGLink_pair(){ }

        public UGLink_pair( UGLink_unit objUGL, int SW, int rc2, int no2){
            this.objUGL=objUGL; this.SW=SW; this.rc2=rc2; this.no2=no2;
        }

        public string ToString_rcno(int rc1, int no1){
            string st = $"UGLink_pair objUGL:{objUGL} SW:{SW}  rc#no:{rc1}#{no1+1} --> {rc2}#{no2+1}";
            return st;
        }
        public string ToString_rcnoSimple(int rc1, int no1 ){
            string st;;
            if(rc1==rc2){ st = $"{rc1.ToRCString()} #{no1+1} --> #{no2+1}"; }
            else{         st = $"#{no1+1} {rc1.ToRCString()} --> {rc2.ToRCString()}"; }
            //string st = $"{rc1.ToRCString()}#{no1+1} --> {rc2.ToRCString()}#{no2+1}";
            return st;
        }
        public override string ToString(){
            string st=$"UGLink_pair objUGL:{objUGL} SW:{SW}  --> rc#no:{rc2.ToRCString()}#{no2+1}";
            return st;
        }
    
        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  
    }          

    public class UBasCov2{ 
        public Bit324             usedLKIDsq; 
//        public List<UGLink_unit> GLK_UList_Sel;    // for CoverSet by chain_search 
        
        public List<UGLink_unit> BaseSetLst;    // BaseSet list
        public List<UGLink_unit> CoverSetLst;   // CoverSet list

        public Bit981 HB981;
        public Bit981 HC981;
        public Bit981 Can981;
        public int    rcCan;
        public int    noCan;
        public int    sz;
        public int    rnk;

        public UBasCov2( Bit324 usedLKIDsq, List<UGLink_unit> BaseSetLst, Bit981 HB981, int sz ){
            this.usedLKIDsq=usedLKIDsq; this.BaseSetLst=BaseSetLst; this.HB981=HB981; this.sz=sz; 
        }
 
        public void addCoverSet( List<UGLink_unit> CoverSetLst, Bit981 HC981, Bit981 Can981, int rnk ){
            this.CoverSetLst=CoverSetLst; this.HC981=HC981; this.Can981=Can981; this.rnk=rnk;
        }
        public override string ToString(){
            string msg = "\r     BaseSet: ";
            string msgB = BaseSetLst.Aggregate("",(Q,P)=>Q+$" {P.ToAppearance()}");
            msg += msgB.ToString_SameHouseComp1();
               
            msg += "\r    CoverSet: ";
            string msgC = CoverSetLst.Aggregate("",(Q,P)=>Q+$" {P.ToAppearance()}");
            msg += msgC.ToString_SameHouseComp1();
            return msg;
        }
    }

    public class Combination_int9: Combination{
        private int noBit;
        private int[] _noBitLst;
        public int[] index2;

        public Combination_int9( int N, int R, int noBit ): base(N,R){
            this.noBit=noBit;
            _noBitLst = noBit.IEGet_BtoNo(9).ToArray();
            index2 = new int[R];
        }

        public new bool Successor(int skip=int.MaxValue){
            if(base.Successor(skip)){ 
                for(int k=0; k<R; k++) index2[k]=_noBitLst[base.Index[k]];
                return true;
            }
            return false;
        }
    }
}