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
    public class UGLinkMan{
        public List<UCell>     pBDL;             
        public Bit81[]         pHouseCells;

        public Bit981          _BDL_B9;
        private List<Bit81>    HBL=new List<Bit81>();
        private List<UGLink>   UGLLst=null;
        private BaseSet_Status BSstatus=null;   

      //static public bool printSW=false;
        private int          _jkc_=0;               //for DEBUG

        private int stageNo{ get{ return SDK_Ctrl.UGPMan.stageNo; } }

        //..7.2..4546.5....9.95.....7.....8.3.9...6...1.7.2...987......8384...1.5253..8.9.. 
        //83..76..2....85.....1...7...8...3....67...13....7...4...2...3.....24....9..63..25 
        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2 
        //6..2....1.8.9.4.2...9.1.3...9.8.1.35..3...7..45.3.7.6...7.4.2...6.7.9.1.9....2..3
        public UGLinkMan( AnalyzerBaseV2 AnB ){
            this.pBDL = AnB.pBDL;
            this.pHouseCells       = AnalyzerBaseV2.HouseCells;
            BaseSet_Status.pHouseCells = AnalyzerBaseV2.HouseCells;
            UGLink.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
            BaseSet_Status.pConnectedCells   = AnalyzerBaseV2.ConnectedCells;

            UGLink.pUGLM=this;
        }

        public int PrepareUGLinkMan( bool printB=false ){

            UGLLst=new List<UGLink>();
            UGLink.IDnum0=0;
            
            //In UGLLst, rcb-Links is first, and cell-Links is second. This order is used in the algorithm.
            // 1)rcb-Links
            for(int no=0; no<9; no++){          //no:digit
                Bit81 BPnoB=new Bit81(pBDL,(1<<no));
                for(int tfx=0; tfx<27; tfx++){  //tfx:house
                    Bit81 Q = pHouseCells[tfx]&BPnoB;   
                    if(Q.IsZero()) continue;
                    Q.no=no;
                    Q.ID=(tfx<<4)|no;                             //ID is the usage within this algorithm(UGLinkMan).
                    if( UGLLst.All(P=>(P.rcBit81.no!=no || P.rcBit81!=Q)) ){ UGLLst.Add(new UGLink(Q)); }  //Q is unique?
//                  else{ usedLKIgnrLst.Add(Q.ID); }              //House is different but cells pattern is the same.
                }
            }

            // 2)cell-Links
            foreach( var UC in pBDL.Where(p=>p.No==0) )  UGLLst.Add(new UGLink(UC));    // cell elements

            if(printB){
                UGLLst.ForEach(P=>WriteLine(P.ToString("prepare")));
//             _usedLKLst_ToRCBString("### usedLKIgnrLst",usedLKIgnrLst);
            }

            _BDL_B9 = new Bit981();
            foreach( var P in pBDL.Where(p=>(p.FreeB)>0) ){
                foreach( var no in P.FreeB.IEGet_BtoNo()) _BDL_B9._BQ[no].BPSet(P.rc);
            }
            for( int n=0; n<9; n++ ) if( _BDL_B9._BQ[n].Count==0 ) _BDL_B9._BQ[n]=null;

            return UGLLst.Count;
        }

        public void Initialize(){ BSstatus=null; }

        public IEnumerable<UBasCov> IEGet_BaseSet( int sz, int rnk ){ 
            if(UGLLst==null)  yield break;
           
            BSstatus  = new BaseSet_Status(sz,rnk); 

            List<UGLink>   basUGLs   = BSstatus.basUGLs;                                    //BaseSet List
            Bit981         HB981     = BSstatus.HB981;                                      //BaseSet bitPattern
            Bit324         usedLK    = BSstatus.usedLK;                                     //usedLink(by serial number)
            List<int>      usedLKLst = BSstatus.usedLKLst;

            long RCBN_frameA=0;

            _jkc_=0;
            var  cmbBas=new Combination(UGLLst.Count,sz);
            int  nxt=int.MaxValue;   //(skip function)
            while(cmbBas.Successor(nxt)) {
                GeneralLogicGen.ChkBas0++;   //*****

                _jkc_++;
            // sz=1
                if(sz==1){
                    UGLink UGL = UGLLst[cmbBas.Index[0]];
                    if(UGL.UC is UCell) goto LNextSet;                                      //only row/column/block link.

                    HB981.Clear(); HB981.BPSet(UGL.rcBit81.no,UGL.rcBit81,tfbSet: false);   //accumulate rcbn info. in HB981.
                    basUGLs.Clear(); basUGLs.Add(UGL);                                      //set UGL in BaseSet

                    GeneralLogicGen.ChkBas1++;   //*****
                    goto LBSFound;   //possibility of solution
                }
            //===================================================================================================================

            // sz>=2         
                HB981.Clear();
                basUGLs.Clear();
                usedLKLst.Clear();

                RCBN_frameA = 0;
                int[] _RCB_frameB = new int[9];
                for( int k=0; k<sz; k++ ){                                                  //(r:row c:column b:block n:digit => rcbn)
                    nxt = k;
                    UGLink UGL = UGLLst[cmbBas.Index[k]];
                    RCBN_frameA |= UGL.RCBN_frameB;                                         //bit expression of rcbn

                    if(!Check_rcbnCondition(sz,rnk,k,RCBN_frameA)) goto LNextSet;           //### extremely efficient

                    if(UGL.rcBit81 is Bit81) {                           // ........................ rcb link  ........................
                        int no = UGL.rcBit81.no;
                        if(k>0 && HB981.IsHit(no,UGL.rcBit81)) goto LNextSet;               //elements already included in HB981
                        HB981.BPSet(no,UGL.rcBit81,tfbSet: true);                           //accumulate rcbn info. in HB981.
                        usedLKLst.Add(UGL.rcBit81.ID);      //(ID=tfx<<4 | no)              //[rcb_link type]register ID to used list.
                        _RCB_frameB[no] |= (int)UGL.RCBN_frameB & 0x3FFFFFF;
                    }
                    else {                                               // ....................... Cell link ........................
                        UCell UC = UGL.UC;
                        int rc = UC.rc;
                        //In UGLLst, rcb-Links is first, and cell-Links is second.
                        //Even in combination, this order is maintained.
                        //Therefore, the condition "cell-links have common parts with rcb-Links?" is satisfied.
                        foreach(var no in UC.FreeB.IEGet_BtoNo(9)) {
                            if(k>0 && HB981.IsHit(no,rc)) goto LNextSet;                    //Not BaseSet as it has no intersection.
                            HB981.BPSet(no,rc,tfbSet: true);                                //accumulate rcbn info. in HB981.
                            _RCB_frameB[no] |= rc.ToRCBitPat();
                        }
                        int IDrc = rc<<4 | 0xF; //( 0xF:Cell type identification flag )
                        usedLKLst.Add(IDrc);               //(ID=rc<<4| no)               //[cell type]register ID to used list.

                    }
                    basUGLs.Add(UGL);                                                       //set UGL in BaseSet
                                                                                            // ...........................................................
                }
                BSstatus.RCB_frameB = _RCB_frameB;
                __UsedLinkToFrame( HB981, usedLKLst, BSstatus);

//                if(SDK_Ctrl.UGPMan.stageNo==20 && _usedLKLst_ToRCBString("",usedLKLst)==" r3#2 r4#2 r3#8" ){                
//                    WriteLine( _usedLKLst_ToRCBString($"usedLKLst:{_jkc_}",usedLKLst,addFreeBB:true) ); 
//                    Board_Check();
//                }
//                if(SDK_Ctrl.UGPMan.stageNo==20 && _usedLKLst_ToRCBString("",usedLKLst)==" r3#2 r4#2 r3#8" ){                
//                    WriteLine( _usedLKLst_ToRCBString($"usedLKLst:{_jkc_}",usedLKLst,addFreeBB:true) ); 
//                    Board_Check();
//                } 

                if( !BSstatus.Check_1() ) goto LNextSet;      //A and B are not linked by other link(C).
                if( !BSstatus.Check_2() ) goto LNextSet;      //Each cell(A) is in a position to link with other cells.
                if( !BSstatus.Check_3() ) goto LNextSet;      //There is a limit to the number of cells that have no links other than BaseSet.

                LBSFound:
                usedLK.Clear();
                basUGLs.ForEach(P => usedLK.BPSet(P.IDnum)); //IDrc: rc<<17 | 1<<16
                UBasCov UBC = new UBasCov(basUGLs,HB981,sz,usedLK);
                yield return UBC;

            //---------------------------------------------------------------------------------------  
            LNextSet:
                continue;
            }
            yield break;

            void __UsedLinkToFrame(Bit981 HB981,List<int> usedLKLst,BaseSet_Status BSstatus) {
                int[] _frame_0 = new int[9];
                int[] _frame_1 = new int[9];
                Bit81 _frame_T = new Bit81();
                int frm, _cellC=0;
                foreach(var no in HB981.noBit.IEGet_BtoNo()) {
                    _frame_0[no] = frm = HB981._BQ[no].IEGetRC().Aggregate(0,(Q,rc) => Q | rc.ToRCBitPat());
                    _frame_1[no] = ___frame_ResetUsed(frm,no,usedLKLst);  
                    _frame_T   |= HB981._BQ[no];
                }
                Bit81 _frame_81 = new Bit81(_frame_T);
                usedLKLst.ForEach(P => { if((P&0xF)==0xF){ _frame_81.BPReset(P>>4); _cellC++; } });

                BSstatus._frame_0 = _frame_0;
                BSstatus._frame_1 = _frame_1;
                BSstatus._frame_T = _frame_T;
                BSstatus._frame_81 = _frame_81;
                BSstatus._cellC    = _cellC;        //(Number of cell links in BaseSet)
            }
        }

    #region for Algorithm Research & Debug 
        public void Board_Check( string msg="" ){
            if( BSstatus==null || BSstatus.sz<=1 )  return;

            Bit981         HB981     = BSstatus.HB981;                                      //BaseSet bitPattern
            List<int>      usedLKLst = BSstatus.usedLKLst;

            if(msg!="") Write($"=>{msg}");

            string st = _usedLKLst_ToRCBString($"\rBaseSet usedLKLst:{_jkc_} -->",usedLKLst);
            usedLKLst.ForEach(P=> st+=$" {P}");
            WriteLine(st);

            WriteLine(  ___frame_ToRCBString_2(BSstatus) );

            List<int> MLst=new List<int>();
            foreach(var nx in HB981.noBit.IEGet_BtoNo()) MLst.Add(nx);
            for( int r=0; r<9;r++){
                st = $"  {r+1} ";
                MLst.ForEach(nx =>{
                    string M=$" {nx+1}";
                    var A=HB981._BQ[nx].Get_RowBitPatten(r);
                    for( int c=0; c<9;c++)  st += ((A&(1<<c))>0)? M: " .";
                    st += "    ";
                } );
                for( int c=0; c<9; c++){ 
                    if( BSstatus._frame_T.IsHit(r*9+c)){
                        st += BSstatus._frame_81.IsHit(r*9+c)? " X": " B"; 
                    }
                    else{ st += " .";}

                }
                if(r==1)  st+= " B is a BaseSet";
                if(r==2)  st+= " X is not used.";

                WriteLine(st);
            }
        }  
        private string  _usedLKLst_ToRCBString( string AName, List<int> usedLKLst, bool addFreeBB=false, bool printB=false){
            string st = AName;
            foreach(var P in usedLKLst){
                if((P&0xF)!=0xF){ st+=" "+(P>>4).tfxToString($"#{((P&0xF)+1)}"); }
                else{ 
                    st+=" "+(P>>4).ToRCString();
                    if(addFreeBB) st+="#"+pBDL[P>>4].FreeB.ToBitStringN(9);
                }
            }
            if(printB) WriteLine(st);
            return st;
        }
        
        private int ___frame_ResetUsed( int frame, int no, List<int> usedLKLst){
            int frameX=frame;
            foreach( var P in usedLKLst){
                if((P&0xF)==0xF){ /* cell-Link */ }
                else if((P&0xF)==no){ frameX &= (1<<(P>>4))^0x7FFFFFFF; }
            }
            //WriteLine($"___frame_ResetUsed frame:{frame.ToBitString27()} frameX:{frameX.ToBitString27()} ");
            return frameX;
        } 

        private string ___frame_ToRCBString_2( BaseSet_Status BSstatus ){
            Bit981 HB981 = BSstatus.HB981;  

            string st="";
            foreach(var no in HB981.noBit.IEGet_BtoNo()){
                if(st!="") st +="\r";
                int frm  = BSstatus._frame_0[no];  
                int frm1 = BSstatus._frame_1[no];
                st += $"#{no+1}[{___frame_ToRCBString(frm1)}] <- [{___frame_ToRCBString(frm)}]";
            }          
            return st;
        }

        private string ___frame_ToRCBString( int frame){
            string st="";
            st += $"r{(frame&0x1FF).ToBitString(9)} ";
            st += $"c{((frame>>9)&0x1FF).ToBitString(9)} ";
            st += $"b{((frame>>18)&0x1FF).ToBitString(9)}";
            return st;
        }
    #endregion for Algorithm Research & Debug 

        private bool Check_rcbnCondition(int sz,int rnk, int kx, long RCBN_frameA, bool printB=false ){
            // kx is a cycle no 
            //Extremely efficient method by consideration
            int rC = ((int)(RCBN_frameA&0x1FF)).BitCount();
            int cC = ((int)(RCBN_frameA>>9)&0x1FF).BitCount();
            int bC = ((int)(RCBN_frameA>>18)&0x1FF).BitCount();
            int nC = ((int)(RCBN_frameA>>27)&0x1FF).BitCount();
            List<int> _S = new List<int>();
            _S.Add(rC); _S.Add(cC); _S.Add(bC);   //aa  _S.Add(nC);
            _S.Sort();

            int rcbC = Min(rC,cC);
            rcbC = Min(rcbC,bC);
            
            if(sz>=2){
                if(printB){
                    Write( $"  rC:{rC} cC:{cC} b:{bC} nC:{nC}  ->" );
                    _S.ForEach(p=>Write($" {p}"));
                    WriteLine("\r");
                }

                if(nC==1)  return (_S[1]<=sz+rnk);
                else if(nC==2) return (_S[0]<=sz+rnk);
            }

            return ( (rcbC+nC-1) <= sz+rnk );
        }

        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2            
        public IEnumerable<UBasCov> IEGet_CoverSet( UBasCov UBC, int rnk ){         //### CoverSet generator
            if(UGLLst==null)  yield break;

            List<UGLink> basUGLs=UBC.basUGLs;
            Bit981 HB981  = UBC.HB981;      //BaseSet
            Bit324 usedLK = UBC.usedLK;     //used links 
            int    noBit  = HB981.noBit;    //bit expression of digits containing cell elements
            int    sz=UBC.sz;

            List<UGLink> UGLCovLst=new List<UGLink>();  // UGLCovLst:candidate link

          #region Preparation(Create UGLCovLst)
            //Preparation: select candidates for CoverSet.(this process is extremely effective!)
            // 1)First select BaseSet, then select CoverSet to cover.
            // 2)CoverSet requirements:
            //  .Exclude links that do not contain BaseSet elements
            //  .Rank=0 includes two or more elements of BaseSet
            //  .Rank>0 contains one or more elements of BaseSet

            Bit81 Bcmp = HB981.CompressToHitCells();
            foreach( var P in UGLLst.Where(q=>!usedLK.IsHit(q.IDnum)) ){            //Exclude Baseset links

                if(P.rcBit81 is Bit81){         // P is a "row/column/block link" case
                    if( (noBit&(1<<P.rcBit81.no))==0 ) continue;                    //Exclude links with digits not included in BaseSet.           
                    int Bcount = (HB981._BQ[P.rcBit81.no] & P.rcBit81).BitCount();  //Bcount : Number of cells in common with BaseSet.
                    if(Bcount==0)  continue;                                        //Link without common parts is excluded from candidate links.

                    if( rnk==0 && Bcount<2 ) continue;                              //if rank=0, the CoverSet has two or more common items
                    UGLCovLst.Add(P);
                }

                else{                           // P is a "cell" case
                    if(noBit.BitCount()<=1) continue;                               //For links within a cell, the Coverset digits must be 2 or more.
                    if( (noBit&P.UC.FreeB)==0 )  continue;                          //Exclude links with digits not included in BaseSet.
                    int B=noBit&P.UC.FreeB;                                         //Common digits of BaseSet and link
                    if(B==0)  continue;                                             //Link without common parts are excluded from candidate links.
                    int rc=P.UC.rc;                                                 //focused cell
                    
                    int kcc=0, kccLim=(rnk==0)? 2:1;
                    foreach( var no in B.IEGet_BtoNo() ){                           //no:Candidate digit of forcused cell.
                        if( !HB981._BQ[no].IsHit(rc) )  continue;                   //do not check if BaseSet does not include no.
                        if(++kcc>=kccLim){ UGLCovLst.Add(P); break; }               //number of digits contained in BaseSet satisfies the condition(kccLim).
                    }
                } 
            }
          #endregion Preparation(Create UGLCovLst)

          #region CoverSet generator
            if(UGLCovLst.Count<sz+rnk)  yield break;
            Bit981 HC981=new Bit981();            //CoverSet
            Bit981 Can981=new Bit981();           //Items that break "Locked"(excludable candidates)

            Combination cmbCvr=new Combination(UGLCovLst.Count,sz+rnk);
            int nxt=int.MaxValue;
            while( cmbCvr.Successor(nxt) ){                                         //Combination one-element generator
                                    ++GeneralLogicGen.ChkCov1;
            
                HC981.Clear();
                Array.ForEach( cmbCvr.Index, m=> HC981 |= UGLCovLst[m].rcnBit );    //CoverSet bit expression

                if( !(HB981-HC981).IsZero() ) goto LNextSet;                        //BaseSet is covered?
                Bit981 CsubB = HC981-HB981;                                         //CsubB:excludable candidates
                if( CsubB.IsZero() ) goto LNextSet;                                 // is exist?

                List<UGLink>  covUGLs=new List<UGLink>();
                Array.ForEach( cmbCvr.Index, m=> covUGLs.Add(UGLCovLst[m]) );       //CoverSet List expression

                if(rnk==0){ Can981=CsubB; }  //(excludable candidates)                  
                else{   //if(rnk>0){

            /*  rank=k
                Consider the case of covering n-BaseSet with (n+k)-CoverSet.
                In order to be an analysis algorithm, the following conditions must be satisfied.
                    1:(n+k)-CoverSet completely covers n-BaseSet.
                    2:(k+1) of the (n+k) links of CoverSet have elements in common which are not included in the n-BaseSet.
                When these conditions are satisfied, the elements of the intersection of condition 2 are not true.
            */
                    bool SolFound=false; 
                    foreach( int n in CsubB.noBit.IEGet_BtoNo() ){
                        foreach( int rc in CsubB._BQ[n].IEGetRC() ){
                            int kc = covUGLs.Count(Q=>Q.IsHit(n,rc));
                            if(kc==rnk+1){
                                Can981.BPSet(n,rc);
                                SolFound=true;
                            }
                        }
                    }

                    if(!SolFound) continue;     
                }
                                    ++GeneralLogicGen.ChkCov2;   //*****

                UBC.addCoverSet( covUGLs, HC981, Can981, rnk );
                yield return UBC;

              LNextSet:
                continue;
            }
            yield break;
          #endregion CoverSet generator
        }


    #region class BaseSet_Status
        public class BaseSet_Status{
            static public Bit81[] pHouseCells;
            static public Bit81[] pConnectedCells;

            public List<UGLink>   basUGLs   = new List<UGLink>();                               //BaseSet List
            public Bit981         HB981     = new Bit981();                                     //BaseSet bitPattern
            public Bit324         usedLK    = new Bit324();                                     //usedLink(by serial number)
            public List<int>      usedLKLst = new List<int>();

            public int[]          RCB_frameB;
            public int            _cellC;           // Number of cell_link in BaseSet
            public int[]          _frame_0;
            public int[]          _frame_1;
            public Bit81          _frame_81;
            public Bit81          _frame_T;

            private int          noBit{ get => HB981.noBit; }
            private int          noBitC{ get => noBit.BitCount(); }
            public int           sz;
            public int           rnk;
            public long[]  rcbnFrame9;      //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]

            public BaseSet_Status( int sz, int rnk ){
                this.sz=sz; this.rnk=rnk;

                rcbnFrame9 = new long[9];
                foreach( var UGL in basUGLs){
                    if(UGL.rcBit81 is Bit81){ rcbnFrame9[UGL.rcBit81.no] |= (long)UGL.RCBN_frameB; }
                    else{
                        int rc=UGL.UC.rc;
                        foreach(var no in UGL.UC.FreeB.IEGet_BtoNo()) rcbnFrame9[no] |= (long) rc.ToRCBitPat();
                    }
                }
            }

            public bool Check_1( ){                //##################################### Check_1
                            //There is a link(C) between the link(A) and the other links(B).
                            // 1)Divide BaseSet into link(A) and other links(B)
                            // 2)A and B are linked by other link(C)?
                int szX = (sz==2)? 1: sz;
                for( int k=0; k<szX; k++ ){
                    UGLink A = basUGLs[k];

                    Bit981 B = new Bit981();
                    for(int m=0; m<sz; m++ ){
                        if(m==k)  continue;
                        B |= basUGLs[m].rcnBit;
                    }
                    if( (B&A.rcnConnected).Count<=0 ) return false;  //A and B are not linked by other link(C).
                }

                GeneralLogicGen.ChkBas1++;   //*****
                return true;    //A and B are linked by other link(C)?
            }

            public bool Check_2( ){ //for sz>=2     //##################################### Check_2     
                bool niceB=false;
                if(noBitC>1){
                            //Focus on one digit.
                            // Each cell(A) is in a position to link with other cells.
                    foreach( var no in noBit.IEGet_BtoNo() ){
                        Bit81 Qno = HB981._BQ[no];  
                        if( Qno.BitCount()==1 ){ goto Lreturn; }
                        foreach( var rc in Qno.IEGetRC()){
                            Bit81 R=Qno&pConnectedCells[rc];            //pConnectedCells: position to link with other cells.
                            if(R.BitCount()<1){  goto Lreturn; }        //                 not include cell(A).
                        }
                    }
                }
                niceB=true;

            Lreturn:
                if(niceB)  GeneralLogicGen.ChkBas2++;   //*****
                return niceB;
            }
 
            public bool Check_3( ){                //##################################### Check_3
                bool niceB=false;            
                if( noBitC<2 ){ niceB=true; goto Lreturn; }       //not applicable for 1 digit.  
               
                int noNotSingle=0;
                { //There is a limit to the number of cells that have no links other than BaseSet.                   
                    foreach(var P in usedLKLst){
                        int no = P&0xF;
                        if(no<9){
                            noNotSingle |= 1<<no;
                            int tfx = (int)(P>>4);  // get house(P=tfx<<4 | no)
                            RCB_frameB[no] &= (1<<tfx) ^ 0x7FFFFFF;             //Exclude BaseSe link from candidates.
                                //if(SDK_Ctrl.UGPMan.stageNo==10) WriteLine($"+++ tfx:{tfx} RCB_frameB[{no}]:{RCB_frameB[no].ToBitString27()}");

                        }
                    }
                }

                Bit981 Q9 = HB981.Copy();
                { //Excludes the cell with the same rc position from the BaseSet_copy(Q9).
                    Bit81  Q  = HB981.noBit.IEGet_BtoNo().Aggregate(new Bit81(), (X,n)=> X|HB981._BQ[n]);

                    noNotSingle ^= 0x1FF;
                    foreach(var n in noNotSingle.IEGet_BtoNo()) Q9._BQ[n].Clear();
                    foreach( var rc in Q.IEGetRC() ){
                        if( HB981.GetBitPattern_rcN(rc).BitCount()>=2 ){        //Q9 reset when there is a link between digits in cell[rc].
                            foreach( var no in noBit.IEGet_BtoNo() ) Q9._BQ[no].BPReset(rc);
                        }
                    }
                    foreach( var no in Q9.noBit.IEGet_BtoNo() ){
                        foreach( int tfx in RCB_frameB[no].IEGet_BtoNo(27)){ 
                            int tfxn = tfx<<4 | noBit;
                            if( usedLKLst.Contains(tfxn) )  continue;
                            var R = HB981._BQ[no]&pHouseCells[tfx];
                            if(R.Count>=2){
                                foreach( var rc in R.IEGetRC() ) Q9._BQ[no].BPReset(rc);
                            }
                        }
                    }
                }

                int Q9cc = Q9.BitCount();
                if(Q9cc <= rnk){ niceB=true; goto Lreturn; }
                if(Q9cc <= rnk*2){  //Rank>0 connects indirectly to Finmed Link.
                                        GeneralLogicGen.ChkBas3B++;   //*****
                    foreach( var n1 in Q9.noBit.IEGet_BtoNo() ){
                        foreach( var rc in Q9._BQ[n1].IEGetRC() ){
                            foreach( var n2 in Q9.noBit.IEGet_BtoNo().Where(nx=>nx<n1) ){       //[rc]#n1 <-(indirectly)-> [rc']#n2
                                if( (Q9._BQ[n2] & pConnectedCells[rc]).BitCount()==0 ) continue;
                                //There may be links indirectly connecting cells. Proceed to the next test.
                                        GeneralLogicGen.ChkBas3A++;   //*****
                                niceB=true; goto Lreturn;
                            }
                        }
                    }
                }

              Lreturn:
                if(niceB)  GeneralLogicGen.ChkBas3++;   //*****
                return niceB;
            }

        }

    #endregion class BaseSet_Status

    }

    public class UGLink{
        static public UGLinkMan pUGLM;
        static public Bit81[]   pConnectedCells;
        static public int       IDnum0;

        public int      sz;             //size
        public int      IDnum;          //generation order number

        //There are two types of UGLink, rcb_link and cell_link
        public Bit81    rcBit81=null;   //rcb_link and used to identify rcb_link_type(not null).
        public UCell    UC=null;        //cell_link and used to identify cell_link_type(not null)

        //Representing two types as common data
        public Bit981   rcnBit;         //bit expression of rc[n] of GLink elements.
        public Bit981   rcnConnected;   //bit expression of rc[n] of Connected Cells of GLink elements.
        public long     RCBN_frameB;    //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]

        public int tfx{ get{ return( (rcBit81 is Bit81)? (rcBit81.ID>>4): -1); } }

        public UGLink( Bit81 rcBit81 ){ //### rcb_link type ###
            this.IDnum=IDnum0++; 
            this.rcBit81=rcBit81; this.sz=rcBit81.BitCount();
            rcnBit=new Bit981(rcBit81);

            rcnConnected=new Bit981();   
            var _conn=new Bit81();
            foreach(var rc in rcBit81.IEGet_rc()){
                _conn |= pConnectedCells[rc];
                for(int n=0; n<9; n++ ) rcnConnected.BPSet(n,rc);
            }
            int no=rcBit81.no;
            rcnConnected._BQ[no] = _conn-rcBit81;
            RCBN_frameB = (long)rcBit81.Get_RowColumnBlock() | ((long)1<<(no+27));   //¡ RCBN_frameB
        }
        public UGLink( UCell UC ){  //### Cell type ###
            this.IDnum=IDnum0++; this.UC=UC;
            rcnBit=new Bit981();
            rcnConnected=new Bit981();
            foreach(var n in UC.FreeB.IEGet_BtoNo() )  rcnBit.BPSet(n,UC.rc);
            for(int n=0; n<9; n++) rcnConnected._BQ[n] |= pConnectedCells[UC.rc];

            // bit expression of rcbn. [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]
            int _rcbFrame = (1<<UC.r | 1<<(UC.c+9) | 1<<(UC.b+18));
            RCBN_frameB = (long)_rcbFrame | ((long)UC.FreeB)<<27;                   //¡ RCBN_frameB
        }
        public bool IsHit( int no, int rc ){
            if( rcBit81 is Bit81 ){ if( this.rcBit81.no==no && rcBit81.IsHit(rc) ) return true; }
            else{ if( UC.rc==rc && (UC.FreeB&(1<<no))>0 ) return true; }
            return false;
        }

        public string ToString( string ttl="" ){
            string st = ttl+" UGLink IDnum:"+IDnum+" tfx:"+ tfx.tfxToString()+"("+tfx+")";
            if(UC!=null) st+="UCell "+ UC.ToString();
            else         st+="ULink no:"+ (rcBit81.no) + " Bit81 "+rcBit81.ToString();
            return st;
        }
    }

}