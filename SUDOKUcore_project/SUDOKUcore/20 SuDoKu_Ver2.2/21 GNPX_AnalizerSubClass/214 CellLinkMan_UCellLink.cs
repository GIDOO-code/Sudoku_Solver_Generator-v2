using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPXcore {
    public class CellLinkMan{
        //Cell-to-cell link
        // Strong links are true and false in both directions. Link is 2 cells.
        // Weak links are true only to the other. Link is 3 or more cells.
        // http://csdenpe.web.fc2.com/page25.html

        private GNPX_AnalyzerMan pAnMan;
        private List<UCell>   pBDL{ get{ return pAnMan.pBDL; } }      
        private Bit81[]       pHouseCells;
        public  int           SWCtrl;
        public  List<UCellLink>[] CeLK81;//cell Link

        public CellLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan = pAnMan;
            this.pHouseCells = AnalyzerBaseV2.HouseCells;
            SWCtrl=0;
        }

		public void  Initialize(){ SWCtrl=0; }
    
      #region Create Stron/WeakLink List            
        public void  PrepareCellLink( int swSW ){
            if( (swSW.DifSet(SWCtrl))==0 )  return;

            if( SWCtrl==0 ) CeLK81=new List<UCellLink>[81];
            _SrtongLinkSearch(true); //strong link
            _WeakLinkSearch( );      //weak link
            SWCtrl |= swSW; 

            int IDX=0;
            foreach( var P in CeLK81 ){
                if(P!=null){ P.Sort(); P.ForEach(Q=> Q.ID=(++IDX) ); }
            }
        }          
        public void  ResetLoopFlag(){
            foreach( var P in CeLK81.Where(p=>p!=null) ){ P.ForEach(Q=>Q.LoopFlag=false); }
        }

        private void _SrtongLinkSearch( bool weakOn ){
            for(int tfx=0; tfx<27; tfx++ ){
                for(int no=0; no<9; no++){
                    int noB = 1<<no;
                    List<UCell> PLst = pBDL.IEGetCellInHouse(tfx,noB).ToList();
                    if(PLst.Count!=2)  continue;
                    UCell UC1=PLst[0], UC2=PLst[1];

                    //The algorithm is simplified with a list that includes forward and reverse directions.
                    //For example, skyscraper.
                    SetLinkList(tfx,1,no,UC1,UC2);
                    SetLinkList(tfx,1,no,UC2,UC1);  //Generate the opposite direction

                    if(weakOn){ //Strong links are also weak links.
                        SetLinkList(tfx,2,no,UC1,UC2);
                        SetLinkList(tfx,2,no,UC2,UC1);
                    }
                }  
            }               
        #region Debug Print
        //    foreach( var P81 in CeLK81.Where(p=>p!=null) ) P81.Sort();
        //    __NLPrint( CeLK81 );
        #endregion Debug Print
        }
        private void _WeakLinkSearch( ){
            for(int tfx=0; tfx<27; tfx++ ){
                    for(int no=0; no<9; no++){
                    int noB = 1<<no;
                    List<UCell> PLst = pBDL.IEGetCellInHouse(tfx,noB).ToList();
                    if( PLst.Count<=2 ) continue;

                    bool SFlag=(PLst.Count==2);
                    for(int n=0; n<PLst.Count-1; n++){
                        UCell UC1=PLst[n];
                        for(int m=n+1; m<PLst.Count; m++ ){
                            UCell UC2=PLst[m];
                            SetLinkList(tfx,2,no,UC1,UC2,SFlag);
                            SetLinkList(tfx,2,no,UC2,UC1,SFlag);
                        }
                    }
                }  
            }     
        #region Debug Print
        //    foreach( var P81 in CeLK81.Where(p=>p!=null) ) P81.Sort();
        //    __NLPrint( CeLK81 );
        #endregion Debug Print
        }
        private void __NLPrint( List<UCellLink>[] CeLkLst ){
            WriteLine();
            int nc=0;
            foreach( var P81 in CeLkLst.Where(p=>p!=null) ){
                foreach( var P in P81 ){
                    int type = P.type;
                    int no   =  P.no;
                    int rc1  =  P.rc1;
                    int rc2  = P.rc2;

                    string st = "  No:" + (nc++).ToString().PadLeft(3);
                    st += "  type:" + type + "  no:" + (no+1);
                    if( type <= 2 ){
                        st += "  rc[" + rc1.ToString("00") + "]r" + ((rc1/9)+1);
                        st +=  "c" + ((rc1%9)+1) + "-b" + (rc1.ToBlock()+1);
                        st += " --> rc[" + rc2.ToString("00") + "]r" + ((rc2/9)+1);
                        st += "c" + ((rc2%9)+1) + "-b" + (rc2.ToBlock()+1);
                    }
                    else{
                        st += " " + ((rc1<10)? "r"+rc1: "c"+(rc1-10));
                        st += ((rc2<10)? "r"+rc2: "c"+(rc2-10));
                    }
                    WriteLine(st);
                }
            }
        }   

        public  void SetLinkList( int tfx, int type, int no, UCell UC1, UCell UC2, bool SFlag=false ){
            var LK =new UCellLink(tfx,type,no,UC1,UC2,SFlag);
            int rc1=UC1.rc;
            if(CeLK81[rc1]==null) CeLK81[rc1]=new List<UCellLink>();
            if(!CeLK81[rc1].Contains(LK))  CeLK81[rc1].Add(LK);
        }
      #endregion Create Stron/WeakLink List

        public IEnumerable<UCellLink> IEGetCellInHouse( int typB ){
            foreach(var P in CeLK81.Where(p=>p!=null) ){
                foreach( var Q in P.Where(q=>((q.type&typB)>0)) ) yield return Q;
            }
        }
        public IEnumerable<UCellLink> IEGetNoType( int no, int typB ){
            foreach( var P in CeLK81.Where(p=>p!=null) ){
                foreach( var Q in P.Where(q=>((q.no==no)&&(q.type&typB)>0)) ) yield return Q;
            }
        }

        public IEnumerable<UCellLink> IEGetRcNoType( int rc, int no, int typB  ){
            var P=CeLK81[rc];
            if(P==null) yield break;
            foreach( var LK in P.Where(p=> ((p.no==no)&&(p.type&typB)>0)) ){
                yield return LK;
            }
            yield break;
        }
        public IEnumerable<UCellLink> IEGet_CeCeSeq( UCellLink LKpre ){
            var P=CeLK81[LKpre.rc2];
            if(P==null) yield break;
            foreach( var LKnxt in P ){
                if( Check_CellCellSequence(LKpre,LKnxt) ) yield return LKnxt;
            }
            yield break;
        }
        public IEnumerable<UCellLink> IEGetRcNoBTypB( int rc, int noB, int typB ){
            var P=CeLK81[rc];
            if(P==null) yield break;
            foreach( var LK in P ){
                if( ((1<<LK.no)&noB)>0 && ((LK.type&typB)>0) ) yield return LK;
            }
            yield break;
        }

        public bool Check_CellCellSequence( UCellLink LKpre, UCellLink LKnxt ){ 
            int noP=LKpre.no, noN=LKnxt.no;
            UCell UCX=LKpre.UCe2;
            switch(LKpre.type){
                case 1:
                    switch(LKnxt.type){
                        case 1: return (noP!=noN);  //S->S
                        case 2: return (noP==noN);  //S->W
                    }
                    break;
                case 2:
                    switch(LKnxt.type){
                        case 1: return (noP==noN);  //W->S
                        case 2: return ((noP!=noN)&&(UCX.FreeBC==2)); //W->W
                    }
                    break;
            }
            return false;
        }
    }

    public class UCellLink: IComparable{
        public int               ID;
        public int               tfx;
        public int               type;
        public bool              SFlag;     //T:Strong
        public bool              LoopFlag;  //Last Link
        public bool              BVFlag;    //bivalue Link
        public readonly int      no;
        public readonly UCell    UCe1;
        public readonly UCell    UCe2;
        public int               rc1{ get{ return UCe1.rc; } }
        public int               rc2{ get{ return UCe2.rc; } }
        public readonly Bit81    B81;

        public UCellLink(){}
        public UCellLink( int tfx, int type, int no, UCell UCe1, UCell UCe2, bool SFlag=false ){
            this.tfx=tfx; this.type=type; this.no=no; this.SFlag=SFlag; 
            this.UCe1=UCe1; this.UCe2=UCe2; this.ID=tfx;
            BVFlag = UCe1.FreeBC==2 && UCe2.FreeBC==2;
            B81=new Bit81(rc1); B81.BPSet(rc2);
        }
        public UCellLink( UCell UCe1, UCell UCe2, int no, int type ){
            this.UCe1=UCe1; this.UCe2=UCe2; this.no=no; this.type=type;
        }

        public UCellLink Reverse(){
            UCellLink ULK=new UCellLink(tfx,type,no,UCe2,UCe1,SFlag);
            return ULK;
        }
                     
        public int CompareTo( object obj ){
            UCellLink Q = obj as UCellLink;
            if( this.type!=Q.type ) return (this.type-Q.type);
            if( this.no  !=Q.no   ) return (this.no-Q.no);
            if( this.rc1 !=Q.rc1  ) return (this.rc1-Q.rc1);
            if( this.rc2 !=Q.rc2  ) return (this.rc2-Q.rc2);
            return (this.ID-Q.ID);
        }
        public override bool Equals( object obj ){
            UCellLink Q = obj as UCellLink;
            if( Q==null )  return true;
            if( this.type!=Q.type || this.no!=Q.no )   return false;
            if( this.rc1!=Q.rc1   || this.rc2!=Q.rc2 ) return false;
            return true;
        }
        public override string ToString(){
            string st="ID:"+ID.ToString().PadLeft(2)+ " type:"+type +" no:"+no;
            st +=  " rc1:"+rc1.ToString().PadLeft(2)+ " rc2:"+rc2.ToString().PadLeft(2); 
            return st;
        }
        public override int GetHashCode(){ return base.GetHashCode(); }
    }         
}