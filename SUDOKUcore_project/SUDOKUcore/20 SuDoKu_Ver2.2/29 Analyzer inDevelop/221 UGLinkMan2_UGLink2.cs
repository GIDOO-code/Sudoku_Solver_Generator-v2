using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using Microsoft.VisualBasic.CompilerServices;
using System.Linq.Expressions;

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
    public partial class UGLinkMan2{
        static public int         _jkc_=0;               //for DEBUG
   
        public List<UCell>          pBDL;             
        public Bit81[]              pHouseCells;

        public Bit981               _BDL_B9;
        private List<UGLink_unit>     GLK_UList_All;
        private List<UGLink_unit>     GLK_UList_Marked;
        private List<UGLink_pair>[,]  GLK_connection;
        private Bit81[]             _noPat=new Bit81[9];


        private BaseSet_Status2      BSstatus2=null;   

        private Bit324              BasCovCandidates;       //BaseSet & CoverSet candidates (B,C common)
        private int[,]              rcnGenNo; 

        private Queue<int>          QueueGlk=new Queue<int>();

        private int stageNo{ get{ return SDK_Ctrl.UGPMan.stageNo; } }

        static private bool printSW=true;
        static private bool _chkX1=false;
        static private bool _chkX2=false;
        //Copy below line and paste on 9x9 grid of GNPX. 
        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  

        public UGLinkMan2( AnalyzerBaseV2 AnB ){
            this.pBDL = AnB.pBDL;
            this.pHouseCells                = AnalyzerBaseV2.HouseCells;
            BaseSet_Status2.pHouseCells     = AnalyzerBaseV2.HouseCells;
            BaseSet_Status2.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
            UGLink_unit.pConnectedCells     = AnalyzerBaseV2.ConnectedCells;
        }

        public int PrepareUGLinkMan( bool printB=false ){                   //Initialization for stage
            GLK_connection    = new List<UGLink_pair>[81,9];
            GLK_UList_All     = new List<UGLink_unit>();
            GLK_UList_Marked  = new List<UGLink_unit>();

            _LinkSearch(printSW:false);

            for(int no=0; no<9; no++)  _noPat[no] = new Bit81(pBDL,(1<<no)); //Bit expression of cell with number n

            return 0;
        }

        public void Initialize(){       //Initialization for each trial
            BSstatus2=null;
            QueueGlk.Clear();
            _jkc_=0;
        }    

        public IEnumerable<UBasCov2> IEGet_BaseSet( int sz, int rnk ){      //### BaseSet generator
            if(GLK_UList_All.Count<sz*2+rnk) yield break;

            BSstatus2 = new BaseSet_Status2(sz,rnk);
            
            //Marking method
            // #1:Focus on the link(LK0) and follow the connecting links. A link of size(sz) is a candidate for BaseSet/CoverSet.
            //    (This alone won't make it faster.)
            // #2:Select BaseSet from the candidates and check suitability.
            //    Excludes generation G1 links.
            // #3:Also select the CoverSet from the candidates. At least common elements with baseset are required.
            //    Light selection and aptitude testing.
            //    Efficient evaluation of aptitude of baseSet is important.
            // #4:If there is no solution for the candidate starting from LK0, record LK0 in the invalid list.
            // #2':Select BaseSet from the candidates, exclude links in the invalid list.

            Bit324 invalidLink = new Bit324();
            foreach( var LK0 in GLK_UList_All){     //#1 LK0:First link
                _chkX1=false; _chkX2=false;

            //    if( SDK_Ctrl.UGPMan.stageNo>=12 && sz>=3 ){//&& rnk==1){  
            //        _chkX1=false; _chkX2=true;
            //        if(LK0.ToAppearance()=="r4#2"){
            //            WriteLine($"--->{LK0.ToAppearance()}");
            //            _chkX1=false; _chkX2=true;
            //        }
            //    }
                    if(_chkX1) WriteLine($"\r-1- baseset sz:{sz} rnk:{rnk}  first:{LK0} -");

                GLK_UList_Marked.Clear();
                QueueGlk.Clear();
                rcnGenNo = new int[9,81];               //### gen ### 
                BasCovCandidates = new Bit324();

              #region ----- marking for LK0 -------------------------     
                BasCovCandidates.BPSet(LK0.IDsq);                           //1)BC_Set 候補のリスト  ## 初のリンク(LK0)の登録
                LK0._gen=1;
                foreach( var NDx in LK0.ConnectLinks){                      //LK0の他端ノードNDx　に着目
                    var (rc,no) = (NDx>>4,NDx&0xF);                         // (rc,no) <-- NDx                        
                    rcnGenNo[no,rc] = 1;                //### gen ###    　 //NDXの世代<-1    set rcnGenNo
                    QueueGlk.Enqueue(NDx);              //Enqueue(ND1)
                        if(_chkX1) WriteLine($"@{_jkc_++}      -1.5- Enqueue {rc.ToRCString()}#{no+1}");
                }

                //==== sz>=2 ====
                while(QueueGlk.Count>0){
                    var ND2 = QueueGlk.Dequeue();
                    var (rc,no) = (ND2>>4,ND2&0xF);
                    int gen=rcnGenNo[no,rc];            //### gen ###  
                            if(_chkX1) WriteLine($"@{_jkc_++}  -2- Dequeue {rc.ToRCString()}#{no+1}-G{gen}");                      

                    if(gen>sz) continue;
                    foreach( var LKpair in GLK_connection[rc,no] ){
                        var (no2,rc2) = (LKpair.no2,LKpair.rc2);
                        var gen2 = rcnGenNo[no2,rc2];   //### gen ###   
                        UGLink_unit LKx=LKpair.objUGL;
                        LKx._gen=gen;
                        bool hit = BasCovCandidates.IsHit(LKx.IDsq);
                        BasCovCandidates.BPSet(LKx.IDsq);   //1)BC_Set 候補のリスト　ビット表現で重複を回避
                                if(_chkX1){
                                    string st3 = $"@{_jkc_++}      -2.5- Link set {LKx} {(hit? "---": "New")}";
                                    WriteLine(st3);
                                }

                        if(gen2==0){
                            rcnGenNo[no2,rc2] = gen+1;  //### gen ###                   
                            if(gen<sz){
                                QueueGlk.Enqueue(LKpair.rcno);
                                if(_chkX1){
                                    string st3 = $"@{_jkc_++}         -3- Enqueue {rc2.ToRCString()}#{no2+1}-G{gen+1}";
                                    st3 += "   "+LKx.ToString();
                                    WriteLine(st3);
                                }
                            }
                        }

                    }
                }
                                     
                GLK_UList_Marked.Add(LK0);                  //First is LK0.
                BasCovCandidates.BPReset(LK0.IDsq);         //reset LK0.
                foreach( var LX in BasCovCandidates.IEGet_Index().Select(kx => GLK_UList_All[kx])) GLK_UList_Marked.Add(LX);
                    if(_chkX2){
                        string stBC = $"\r@{_jkc_++} BasCovCandidates({GLK_UList_Marked.Count}) :";
                        GLK_UList_Marked.ForEach( LK => stBC+=$" {LK.ToAppearance(Gen:true)}" );   //#### debug print
                        WriteLine(stBC);
                    }
                if(GLK_UList_Marked.Count<sz) continue;    
              #endregion ----- marking -------------------------

              #region ----- selecte -------------------------
                var GLK_UList_Sel=new List<UGLink_unit>(); 
                Combination cmbBas = new Combination(GLK_UList_Marked.Count,sz);
                string  st, stw="";
                int  nxt=int.MaxValue;                      //(skip function)              
                string stT="";  
                while(cmbBas.Successor(nxt)){
                    nxt=int.MaxValue;
                    if(cmbBas.Index[0]!=0)  continue;           //"The first link is fixed at 0!"
                    GeneralLogicGen2.ChkBas0++;      //*****
              
                    var   usedLKIDsq = new Bit324();
                    var   BaseSetLst = new List<UGLink_unit>();                 
                    var   HB981      = new Bit981(); //BaseSet bitPattern 
                    long  RCBN_frameA=0;
                    int[] RCB_frameB9 = new int[9];
                    foreach( var (kx,elementX) in cmbBas.IEGetIndex2() ){
                        nxt = kx;
                        var UGL = GLK_UList_Marked[elementX];
                        if(kx>=1 && UGL._gen==1) goto LnxtCombination;              // (#2) excludes generation G1 links.
                        if( invalidLink.IsHit(UGL.IDsq) )  goto LnxtCombination;    // (#2')exclude links in the invalid list.

                        usedLKIDsq.BPSet(UGL.IDsq);
                        BaseSetLst.Add(UGL);
                        foreach( var P in UGL.ConnectLinks){
                            int no2=P&0xF, rc2=P>>4;
                            if(HB981.IsHit(no2,rc2)) goto LnxtCombination;          // BaseSet links do not overlap.
                            HB981.BPSet(no2,rc2);
                        }
                        long RCBN_frameB = UGL.RCBN_frameB; //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]
                        int freebX = (int)(RCBN_frameB>>27);
                        foreach(var no in freebX.IEGet_BtoNo()) RCB_frameB9[no] |= (int)(RCBN_frameB&0x7FFFFFF);
                        RCBN_frameA |= UGL.RCBN_frameB;    //bit expression of rcb 
                        if(kx>0){
                            if(!Check_rcbnCondition(sz,rnk,elementX,RCBN_frameA)) goto LnxtCombination;   //(### extremely efficient) ???? 
                        }
                    }
                    if(!Check_rcbnCondition(sz,rnk,0,RCBN_frameA)) goto LnxtCombination;   //Evaluate BaseSet pattern. Extremely efficient.
                    if(sz>=2){ 
                        BSstatus2.Prepare( usedLKIDsq, BaseSetLst, HB981, RCB_frameB9);

                    //check 421 52.1sec
                    //check 124 53.6sec

                        if( !BSstatus2.Check_4() ) goto LnxtCombination;    //Each element of LK0 is associated with the rest of the links.
                        if( !BSstatus2.Check_2() ) goto LnxtCombination;    //Each cell(A) is in a position to link with other cells.

                        if( !BSstatus2.Check_1() ) goto LnxtCombination;    //A and B are not linked by other link(C).   

                    //  if( !BSstatus2.Check_3() ) goto LnxtCombination;  //There is a limit to the number of cells that have no links other than BaseSet.                  
                    //      Check_1 and _3 are almost the same evaluation. Check_3 is a heavy process.

                    //  if( !BSstatus2.Check_5() ) goto LnxtCombination;  //XXX Almost no effect. Check_1-4 has been determined.
                                     
                    }
                    var BasCov = new UBasCov2( usedLKIDsq, BaseSetLst, HB981, sz );
                            if(_chkX2){
                                string st2=$"\r{_jkc_++} BaseSet:";
                                BaseSetLst.ForEach(P => st2+=" "+P.ToAppearance());
                             //     WriteLine(st2);
                                WriteLine($"Check_5---->{st2}");    //################### 
                            }
                    yield return BasCov;

                  LnxtCombination:
                    continue;
                }

              //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                invalidLink.BPSet(LK0.IDsq); //#4:If there is no solution for the candidate starting from LK0, record LK0 in the invalid list.
              //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

              #endregion ----- selecte -------------------------

            }
            yield break;
        }
        private void  _QueueCheck( int ix ){
            string st = QueueGlk.Aggregate($"Queue:{ix}",(Q,P)=> Q+$" {P.ToString()}");
            WriteLine(st);
        }
        private string  _UGLink_unit_ToRCBString( List<UGLink_unit> UGLunitLst, bool printB=false){
            string st = "";
            //foreach(var P in UGLunitLst){ st += " "+P.ToAppearance();
            st = UGLunitLst.Aggregate("",(p,q)=>p+=$" {q.ToAppearance()}");
            if(printB) WriteLine(st);
            return st;
        }
        //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  

        public IEnumerable<UBasCov2> IEGet_CoverSet( UBasCov2 BasCov, int rnk ){         //### CoverSet generator
            if(GLK_UList_All==null)  yield break;
            int  sz         = BasCov.sz;
            
            if(GLK_UList_Marked.Count<sz) yield break;

            var  BaseSetLst = BasCov.BaseSetLst;
            var  usedLKIDsq = BasCov.usedLKIDsq;        //used links 
            var  HB981      = BasCov.HB981;             //BaseSet bit expression
            int  noBit      = HB981.noBit;              //bit expression of digits containing cell elements

            var UGLCovCandidates=new List<UGLink_unit>();  // UGLCovCandidates:candidate link

          #region Preparation(Create UGLCovCandidates)
            //Preparation: select candidates for CoverSet.(this process is extremely effective!)
            // 1)First select BaseSet, then select CoverSet to cover.
            // 2)CoverSet requirements:
            //  .Exclude links that do not contain BaseSet elements
            //  .Rank=0 includes two or more elements of BaseSet
            //  .Rank>0 contains one or more elements of BaseSet

            Bit81 Bcmp = HB981.CompressToHitCells();
//x            long  RCBN_frameBex = BaseSetLst.Aggregate((long)0,(p,q)=>p|q.RCBN_frameBex);
 
            foreach( var P in GLK_UList_Marked ){  //Exclude Baseset links
//x                if((RCBN_frameBex&P.RCBN_frameB) == 0) continue; 
                if( BaseSetLst.Contains(P) )  continue;
                if(P.type==0){         // 0:link
                    if((noBit&(1<<P.no))==0) continue;                      //Exclude links with digits not included in BaseSet.           
                    int Bcount = (HB981._BQ[P.no] & P.rcBit81).BitCount();  //Bcount : Number of cells in common with BaseSet.
                    if(Bcount==0)  continue;                                //Link without common parts is excluded from candidate links.

                    if(rnk==0 && Bcount<2) continue;                        //if rank=0, the CoverSet has two or more common items
                    UGLCovCandidates.Add(P);
                }

                else{                  // 1:cell
                    if(noBit.BitCount()<=1) continue;                       //For links within a cell, the Coverset digits must be 2 or more.
                    if( (noBit&P.FreeB)==0 )  continue;                     //Exclude links with digits not included in BaseSet.
                    int B=noBit&P.FreeB;                                    //Common digits of BaseSet and link
                    if(B==0)  continue;                                     //Link without common parts are excluded from candidate links.
                    int rc=P.rc;                                            //focused cell
                    
                    int kcc=0, kccLim=(rnk==0)? 2:1;
                    foreach( var no in B.IEGet_BtoNo() ){                   //no:Candidate digit of forcused cell.
                        if( !HB981._BQ[no].IsHit(rc) )  continue;           //do not check if BaseSet does not include no.
                        if(++kcc>=kccLim){ UGLCovCandidates.Add(P); break; }//number of digits contained in BaseSet satisfies the condition(kccLim).
                    }
                } 
            }
            if(_chkX2){ 
                    string st2=$"{_jkc_++} UGLCovCandidates for CoverSet:";
                    st2 = UGLCovCandidates.Aggregate(st2,(p,q)=>p+" "+q.ToAppearance());//#### debug print
                    
                    WriteLine(st2);
            }
          #endregion Preparation(Create UGLCovCandidates)

          #region CoverSet generator
            if(UGLCovCandidates.Count<sz+rnk)  yield break;
            Bit981 HC981 =new Bit981();           //CoverSet
            Bit981 Can981=new Bit981();           //Items that break "Locked"(excludable candidates)

            Combination cmbCvr=new Combination(UGLCovCandidates.Count,sz+rnk);
            int nxt=int.MaxValue;
            while( cmbCvr.Successor(nxt) ){                                             //Combination one-element generator
                                    ++GeneralLogicGen2.ChkCov1;
                HC981.Clear();
                Array.ForEach( cmbCvr.Index, m=> HC981 |= UGLCovCandidates[m].rcnBit ); //CoverSet bit expression

                if( !(HB981-HC981).IsZero() ) goto LNextSet;                            //BaseSet is covered?
                Bit981 CsubB = HC981-HB981;                                             //CsubB:excludable candidates
                if( CsubB.IsZero() ) goto LNextSet;                                     // is exist?

                var  CoverSetLst=new List<UGLink_unit>();
                Array.ForEach( cmbCvr.Index, m=> CoverSetLst.Add(UGLCovCandidates[m]) );//CoverSet List

                if(rnk==0){ Can981=CsubB; }  //(excludable candidates)                  
                else{   //if(rnk>0){

            //  rank=k
            //    Consider the case of covering n-BaseSet with (n+k)-CoverSet.
            //    In order to be an analysis algorithm, the following conditions must be satisfied.
            //        1:(n+k)-CoverSet completely covers n-BaseSet.
            //        2:(k+1) of the (n+k) links of CoverSet have elements in common which are not included in the n-BaseSet.
            //    When these conditions are satisfied, the elements of the intersection of condition 2 are not true.          
                    bool SolFound=false; 
                    Can981.Clear();
                    foreach( int n in CsubB.noBit.IEGet_BtoNo() ){
                        foreach( int rc in CsubB._BQ[n].IEGetRC() ){
                            int kc = CoverSetLst.Count(Q=>Q.rcnBit.IsHit(n,rc));
                            if(kc==rnk+1){
                                Can981.BPSet(n,rc);
                                SolFound=true;
                            }
                        }
                    }

                    if(!SolFound) continue;     
                }
                                    ++GeneralLogicGen2.ChkCov2;   //*****

                BasCov.addCoverSet( CoverSetLst, HC981, Can981, rnk );
                    if(_chkX2){ 
                        string st=$"{_jkc_++}      ◆CoverSet:";  
                        foreach( var P in CoverSetLst ) st += $" {P.ToAppearance()}";     //#### debug print
                        WriteLine(st);
                    }
                yield return BasCov;

              LNextSet:
                continue;
            }
            yield break;
          #endregion CoverSet generator
        }
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
                               
      #region link_search
        private void _LinkSearch( bool printSW=false ){

            int IDx=-1;
            // *==* cell-cell link
            for(int no=0; no<9; no++){
                for(int tfx=0; tfx<27; tfx++ ){
                    int noB = 1<<no;
                    List<UCell> PLst = pBDL.IEGetCellInHouse(tfx,noB).ToList();
                    int szL=PLst.Count;
                    if(szL<=1) continue;
                    int SW = (szL==2)? 0: 1;
                    IDx++;
                    var LK1 =new UGLink_unit(IDx,SW,tfx,no,PLst);
                    GLK_UList_All.Add(LK1);
                    Combination cmb=new Combination(szL,2);
                    while( cmb.Successor() ){
                        UCell UC1=PLst[cmb.Index[0]], UC2=PLst[cmb.Index[1]];
                        __SetGLinkLstSet(LK1,SW,tfx,no,UC1,UC2);
                    }
                }
            }

            // *==* in-cell link
            foreach( var P in pBDL.Where(p => p.No==0) ){
                int rc=P.rc;
                int[] noLst=P.FreeB.IEGet_BtoNo().ToArray();
                int szL= noLst.Length;
                if(szL<=1) continue;
                int SW = (szL==2)? 0: 1;
                IDx++;
                var LK2 =new UGLink_unit(IDx,SW,rc,P.FreeB);
                GLK_UList_All.Add(LK2);

                Combination cmb=new Combination(szL,2);
                while( cmb.Successor() ){
                    int no1=noLst[cmb.Index[0]], no2=noLst[cmb.Index[1]];
                    __SetGLinkLstSet(LK2,SW,rc,no1,no2);
                }
            }

            if(printSW){
                WriteLine($"============================ stage:{stageNo} GLK_UList_All");
                foreach( var P in GLK_UList_All) WriteLine(P);
                WriteLine($"============================ stage:{stageNo} GLK_connection");
                foreach( var rc in Enumerable.Range(0,81) ){
                    foreach( var no in Enumerable.Range(0,9)){
                        if(GLK_connection[rc,no]!=null){
                            GLK_connection[rc,no].ForEach(P=>WriteLine(P.ToString_rcno(rc,no)));
                        }
                    }
                }
            }
        }

        private void __SetGLinkLstSet(UGLink_unit LKx, int SW, int tfx, int no, UCell UC1, UCell UC2 ){     // cell-cell link
            int rc1=UC1.rc, rc2=UC2.rc;

            UGLink_pair LK1 =new UGLink_pair(LKx,SW,rc2,no);
            if(GLK_connection[rc1,no]==null)  GLK_connection[rc1,no] = new List<UGLink_pair>(); 
            GLK_connection[rc1,no].Add(LK1);

            UGLink_pair LK2 =new UGLink_pair(LKx,SW,rc1,no);
            if(GLK_connection[rc2,no]==null)  GLK_connection[rc2,no] = new List<UGLink_pair>(); 
            GLK_connection[rc2,no].Add(LK2);
        }

        private void __SetGLinkLstSet(UGLink_unit LKx, int SW, int rc, int no1, int no2 ){           // in-cell link
            UGLink_pair LK1 =new UGLink_pair(LKx,SW,rc,no2);
            if(GLK_connection[rc,no1]==null)  GLK_connection[rc,no1] = new List<UGLink_pair>(); 
            GLK_connection[rc,no1].Add(LK1);
            UGLink_pair LK2 =new UGLink_pair(LKx,SW,rc,no1);
            if(GLK_connection[rc,no2]==null)  GLK_connection[rc,no2] = new List<UGLink_pair>(); 
            GLK_connection[rc,no2].Add(LK2);
        }
      #endregion link_search

      #region class BaseSet_Status2
        public class BaseSet_Status2{
            static public Bit81[] pHouseCells;
            static public Bit81[] pConnectedCells;

            
            public Bit324             usedLKIDsq;   //usedLink(by serial number) 
            public List<UGLink_unit>  BaseSetLst;   //BaseSet List
            public Bit981             HB981;        //BaseSet bitPattern
            public int[]              RCB_frameB;   //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]

//            public int            _cellC;         // Number of cell_link in BaseSet
//            public int[]          _frame_0;
//            public int[]          _frame_1;
//            public Bit81          _frame_81;
//            public Bit81          _frame_T;

            private int          noBit{ get => HB981.noBit; }
            private int          noBitC{ get => noBit.BitCount(); }
            public int           sz;
            public int           rnk;
            public long[]   rcbnFrame9;      //bit expression of [ UC.FreeB<<27 | 1<<(UC.b+18)) | 1<<(UC.c+9) | (1<<UC.r) ]

            public BaseSet_Status2( int sz, int rnk ): base(){
                this.sz=sz; this.rnk=rnk;
            }

            public void Prepare( Bit324 usedLKIDsq, List<UGLink_unit> BaseSetLst, Bit981 HB981,  int[] RCB_frameB){ 
                this.usedLKIDsq = usedLKIDsq;
                this.BaseSetLst = BaseSetLst;
                this.HB981      = HB981;
                this.RCB_frameB = RCB_frameB;

                rcbnFrame9 = new long[9];
                foreach( var UGL in BaseSetLst){
                    if(UGL.type==0){        //--- type:link ---
                        rcbnFrame9[UGL.rcBit81.no] |= (long)UGL.RCBN_frameB;
                    } 
                    else{                   //--- type:Cell ---
                        int rc=UGL.rc;
                        foreach(var no in UGL.FreeB.IEGet_BtoNo()) rcbnFrame9[no] |= (long) rc.ToRCBitPat();
                    }
                }
            }

            public bool Check_1( ){                //##################################### Check_1
                            //There is a link(C) between the link(A) and the other links(B).
                            // 1)Divide BaseSet into link(A) and other links(B)
                            // 2)A and B are linked by other link(C)?
                        //BaseSetのリンク群を、１リンク(A)と残りのリンク群(B)に分ける。
                        //AとBは、何らかのリンク(C)で連結している。
                        //最も簡単なテスト。
                        //Bと、Aの"連結"　に共通部分がないときはエラー
                int szX = (sz==2)? 1: sz;
                for( int k=0; k<szX; k++ ){
                    UGLink_unit A = BaseSetLst[k];

                    Bit981 B = new Bit981();
                    for(int m=0; m<sz; m++ ){
                        if(m==k)  continue;
                        B |= BaseSetLst[m].rcnBit;
                    }
                    if( (B&A.rcnConnected).Count<=0 ) return false;  //A and B are not linked by other link(C).
                }

                GeneralLogicGen2.ChkBas1++;   //*****
                return true;    //A and B are linked by other link(C)?
            }

            public bool Check_2( ){ //for sz>=2     //##################################### Check_2    
                        //2つ以上の数字を含むBaseSetが対象。
                        // １つの数字に着目し、
                        // 　1)その平面にあるBaseSetセルが１つならエラー
                        //　 2)平面内の全セルが互いに"連結"している。
                bool niceB=false;
                if(noBitC>1){
                            //Focus on one digit.
                            // Each cell(A) is in a position to link with other cells.
                    foreach( var no in noBit.IEGet_BtoNo() ){
                        Bit81 Qno = HB981._BQ[no];  
                        if( Qno.BitCount()==1 ){ goto Lreturn; }    //...1)
                        foreach( var rc in Qno.IEGetRC()){
                            Bit81 R=Qno&pConnectedCells[rc];        //pConnectedCells: position to link with other cells.
                            if(R.BitCount()<1){  goto Lreturn; }    //...2)            not include cell(A).
                        }
                    }
                }
                niceB=true;

            Lreturn:
                if(niceB)  GeneralLogicGen2.ChkBas2++;   //*****
                return niceB;
            }
 
            public bool Check_3( ){                //##################################### Check_3
                bool niceB=false;            
                if( noBitC<2 ){ niceB=true; goto Lreturn; }     //not applicable for 1 digit.  
               
                int noNotSingle=0;
                { //There is a limit to the number of cells that have no links other than BaseSet.                   
                    foreach(var UGL in BaseSetLst){
                        if(UGL.type==0){
                            int no=UGL.no;
                            noNotSingle |= (1<<UGL.no);
                            RCB_frameB[no] |= ((int)(UGL.RCBN_frameB_Remove_tfx)) & 0x7FFFFFF;
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
                            if( BaseSetLst.FindIndex(P=> P.IDunit==tfxn)>=0 )  continue;
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
                                        GeneralLogicGen2.ChkBas3B++;   //*****
                    foreach( var n1 in Q9.noBit.IEGet_BtoNo() ){
                        foreach( var rc in Q9._BQ[n1].IEGetRC() ){
                            foreach( var n2 in Q9.noBit.IEGet_BtoNo().Where(nx=>nx<n1) ){       //[rc]#n1 <-(indirectly)-> [rc']#n2
                                if( (Q9._BQ[n2] & pConnectedCells[rc]).BitCount()==0 ) continue;
                                //There may be links indirectly connecting cells. Proceed to the next test.
                                        GeneralLogicGen2.ChkBas3A++;   //*****
                                niceB=true; goto Lreturn;
                            }
                        }
                    }
                }

              Lreturn:
                if(niceB)  GeneralLogicGen2.ChkBas3++;   //*****
                return niceB;
            }

            public bool Check_4( ){                //##################################### Check_4
                //bas981A:最初のリンク(LK0)以外のリンクの表現 bas981A
                //rcnBitLst:LK0の要素セル(rcn)のリスト
                //(rc,no) in rcnBitLst について
                //　bas981Aと　(rc,no)の"連結"　はしているか。連結していないのはrnkまでは許容。
                //Each element of LK0 is associated with the rest of the links.
                bool niceB=false;
                if( sz==1 ){ niceB=true; goto Lreturn; }
                Bit981 bas981A = new Bit981();
                foreach( var LDK in BaseSetLst.Skip(1)) bas981A |= LDK.rcnBit;  //Aggregate. However, LK0 is excluded.
                var rcnBitLst = BaseSetLst[0].rcnElementLst;
                
                int noHit=0; 
                foreach( var (rc,no) in rcnBitLst.Select(p => (p>>4,p&0xF)) ){
                    if( (pConnectedCells[rc] & bas981A._BQ[no]).IsZero() ){     //(Link type)Is it related?
                        foreach( var no2 in bas981A.noBit.IEGet_BtoNo()){
                            if( bas981A._BQ[no2].IsHit(rc) ) goto LinCellHit;   //(Cell type)Is it related?
                        }  
                        if( (++noHit)>rnk ) goto Lreturn;                       //Unrelated elements may be up to rnk.                   
                    }

                  LinCellHit:
                    continue;
                }
                niceB=true;

               Lreturn:              
                if(niceB) GeneralLogicGen2.ChkBas4++;   //*****
                return niceB;
            }

            public bool Check_5( ){                //##################################### Check_5
                //Almost no effect. Check 1-4 has been determined.
                bool niceB=false;            
                if( noBitC<2 ){ niceB=true; goto Lreturn; }     //not applicable for 1 digit.  
 
                int conC = BaseSetLst.Count(p=>p.type==1);      //Cell type
                if(conC>=noBitC){ niceB=true; goto Lreturn; }
    
                Bit981 Q9 = new Bit981(HB981);
                Bit81  Q1 = new Bit81();        //
                foreach( var no in HB981.noBit.IEGet_BtoNo()) Q1 |= HB981._BQ[no];  //convolve the n-plane
                foreach( var UGL in BaseSetLst.Where(p=>p.type==1) ){               
                    int rc = UGL.rc;
                    foreach( var no in UGL.FreeB.IEGet_BtoNo()) Q1.BPReset(rc);     //exclude cell_link part
                }

                var cmbT5 = new Combination_int9(noBitC,2,HB981.noBit);
                int nxt=int.MaxValue;
                while(cmbT5.Successor(nxt)){
                    var A = Q9._BQ[cmbT5.index2[0]];
                    var B = Q9._BQ[cmbT5.index2[1]];
                    Q1 -= (A&B);
                }
                if(Q1.IsZero()){ niceB=true; goto Lreturn; }

                foreach( int rcX in Q1.IEGetRC()){
                    if( !BaseSetLst.Any(P=>P.existAnotherLink(rcX)) )  goto Lreturn;
                }
                niceB=true;

              Lreturn:
                if(niceB)  GeneralLogicGen2.ChkBas5++;   //*****
                return niceB;
            }

#if false
            public bool Check_5X( int nxt, ref string st ){                //##################################### Check_4
                //****
                bool niceB=false;
                if( sz<=1 ){ niceB=true; goto Lreturn; }

                long frameX=0;
                st="";
                for( int szM=2; szM<=sz;szM++){
                    st += $"   [{szM}]";
                    for( int k=0; k<szM; k++) frameX |= BaseSetLst[k].RCBN_frameB;
                    int rC = ((int)(frameX&0x1FF)).BitCount();
                    int cC = ((int)(frameX>>9)&0x1FF).BitCount();
                    int bC = ((int)(frameX>>18)&0x1FF).BitCount();
                    int nC = ((int)(frameX>>27)&0x1FF).BitCount();
                    List<int> _S = new List<int>();
                    _S.Add(rC); _S.Add(cC); _S.Add(bC);   //aa  _S.Add(nC);
                    _S.Sort();

                    st += string.Join("-",_S)+"@ "+frameX.ToBitString();
                }
                niceB=true;

              Lreturn:              
                if(niceB) GeneralLogicGen2.ChkBas4++;   //*****
                return niceB;
            }
#endif
#if false
            public bool Check_5x( ){                //##################################### Check_4
                //Each element of LK0 is associated with the rest of the links.
                bool niceB=false;
                if( sz==1 ){ niceB=true; goto Lreturn; }

                Bit981 bas981A = new Bit981();
                foreach( var LDK in BaseSetLst.Skip(1)) bas981A |= LDK.rcnBit;  //Aggregate. However, LK0 is excluded.
                var rcnBitLst = BaseSetLst[0].rcnElementLst;
                
                int noHit=0; 
                foreach( var (rc,no) in rcnBitLst.Select(p => (p>>4,p&0xF)) ){
                    if( (pConnectedCells[rc] & bas981A._BQ[no]).IsZero() ){     //(Link type)Is it related?
                        foreach( var no2 in bas981A.noBit.IEGet_BtoNo()){
                            if( bas981A._BQ[no2].IsHit(rc) ) goto LinCellHit;   //(Cell type)Is it related?
                        }  
                        if( (++noHit)>rnk ) goto Lreturn;                       //Unrelated elements may be up to rnk.                   
                    }

                  LinCellHit:
                    continue;
                }
                niceB=true;

               Lreturn:              
                if(niceB) GeneralLogicGen2.ChkBas4++;   //*****
                return niceB;
            }
#endif
            //1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  
            public bool Check_X( ){                //##################################### Check_4 
//                論理再構成中
                bool niceB=false;
//!                int noBP=HB981.noBit;
                if( noBitC<=1){ niceB=true; goto LNextSet; }
                else{   //(noCC<=2)
                    //Divide the BaseSet into two groups(A,B). 
                    //Test for the intersection of A and B.
                    foreach( var no in noBit.IEGet_BtoNo()){            //classified as #no and others.
                        Bit81 A=new Bit81(), B=new Bit81();
                        foreach(var P in BaseSetLst){
                            if(P.type==0){              //--- type:link ---
                                if(P.rcBit81.no==no) A |= P.rcBit81;
                                else                 B |= P.rcBit81;
                            }
                            else{                       //Cell type
                                int rc=P.rc; 
                                foreach(var n in P.FreeB.IEGet_BtoNo()){
                                    if(n==no) A.BPSet(rc);
                                    else      B.BPSet(rc);
                                }
                            }
                        }

                        int nOL=(B&A).Count;
                        if(rnk==0 && nOL<2)  goto LNextSet;
                        if(rnk>0  && nOL<1)  goto LNextSet;                           
                    //  WriteLine($"---------- no:{no} sz:{sz} rnk:{rnk} nOL:{nOL} (A-B):{(A-B).Count} (B-A):{(B-A).Count}");
                        if(noBitC==2) break;
                    }                 
                    niceB=true;
                }

            LNextSet:              
                if(niceB) GeneralLogicGen2.ChkBas6++;   //*****
                return niceB;
            }

        }
    #endregion class BaseSet_Status2

    }
}