using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk {
    public partial class GNPZ_Analyzer{
        public class CellLinkMan{   //GNPZ_Analyzerの内部クラスである必要性はない。
            private int           S=1, W=2;
            private GNPZ_Analyzer pSA;
            public  int           SWCtrl;
            public  List<UCellLink>[] CeLK81;//セルチエインの制御

            public List<LinkCellAls>[]   LinkCeAlsLst;

            public CellLinkMan( GNPZ_Analyzer pSA ){
                this.pSA  = pSA;
                UCellLink.pSA =pSA;
                LinkCeAlsLst = new List<LinkCellAls>[81];
                SWCtrl=0;
            }
             
            public void PrepareCellLink( int swSW ){
                if( (swSW.DifSet(SWCtrl))==0 )  return;

                if( SWCtrl==0 ) CeLK81=new List<UCellLink>[81];
                _SrtongLinkSearch(true); //strong link
                _WeakLinkSearch( );      //weak link
                SWCtrl |= swSW;

                foreach( var P in CeLK81 ) if(P!=null) P.Sort();
            } 
          
            public void  ResetLoopFlag(){
                foreach( var P in CeLK81.Where(p=>p!=null) ){
                    P.ForEach(Q=>Q.LoopFlag=false);
                }
            }
            private void _SrtongLinkSearch( bool weakOn ){
                List<UCell>   qBDL=pSA.pGP.BDL;
                for( int tfx=0; tfx<27; tfx++ ){
                    for( int no=0; no<9; no++){
                        int noB = 1<<no;
                        List<UCell> PLst = qBDL.IEGet(tfx,noB).ToList();
                        if( PLst.Count!=2 ) continue;
                        int rc1=PLst[0].rc, rc2=PLst[1].rc;

                        SetLinkList(tfx,1,no,rc1,rc2);
                        SetLinkList(tfx,1,no,rc2,rc1);
                      
                        if( weakOn ){
                            SetLinkList(tfx,2,no,rc1,rc2);
                            SetLinkList(tfx,2,no,rc2,rc1);
                        }
                    }  
                }               
            #region Debug Print
            //    foreach( var P81 in CeLK81.Where(p=>p!=null) ) P81.Sort();
            //    __NLPrint( CeLK81 );
            #endregion Debug Print
            }
            private void _WeakLinkSearch( ){
                List<UCell>   qBDL=pSA.pGP.BDL;
                for( int tfx=0; tfx<27; tfx++ ){
                        for( int no=0; no<9; no++){
                        int noB = 1<<no;
                        List<UCell> PLst = qBDL.IEGet(tfx,noB).ToList();
                        if( PLst.Count<=2 ) continue;

                        bool SFlag=(PLst.Count==2);
                        for( int n=0; n<PLst.Count-1; n++){
                            int rc1 = PLst[n].rc;
                            for( int m=n+1; m<PLst.Count; m++ ){
                                int rc2 = PLst[m].rc;
                                SetLinkList(tfx,2,no,rc1,rc2,SFlag);
                                SetLinkList(tfx,2,no,rc2,rc1,SFlag);
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
                Console.WriteLine();
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
                        Console.WriteLine(st);
                    }
                }
                // Console.WriteLine( "Capacity:" + CellLinkList.Capacity );
            }   

            public  void SetLinkList( int tfx, int type, int no, int rc1, int rc2, bool SFlag=false ){
                var LK =new UCellLink(tfx,type,no,rc1,rc2,SFlag);
                if( CeLK81[rc1]==null ) CeLK81[rc1]=new List<UCellLink>();
                if( !CeLK81[rc1].Contains(LK) )  CeLK81[rc1].Add(LK);
            } 

            public bool ContainsLink( UCellLink LK ){
                List<UCellLink> P=CeLK81[LK.rc1];
                return (P!=null && P.Contains(LK));
            }
            public IEnumerable<UCellLink> IEGet( int typB ){
                foreach( var P in CeLK81.Where(p=>p!=null) ){
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
                if( P==null ) yield break;
                foreach( var LK in P.Where(p=> ((p.no==no)&&(p.type&typB)>0)) ){
                    yield return LK;
                }
                yield break;
            }
            public IEnumerable<UCellLink> IEGet_CeCeSeq( UCellLink LKpre ){
                var P=CeLK81[LKpre.rc2];
                if( P==null ) yield break;
                foreach( var LKnxt in P ){
                    if( Check_CellCellSequence(LKpre,LKnxt) ) yield return LKnxt;
                }
                yield break;
            }
            public IEnumerable<UCellLink> IEGetRcNoBTypB( int rc, int noB, int typB ){
                var P=CeLK81[rc];
                if( P==null ) yield break;
                foreach( var LK in P ){
                    if( ((1<<LK.no)&noB)>0 && ((LK.type&typB)>0) ) yield return LK;
                }
                yield break;
            }

            public bool Check_CellCellSequence( UCellLink LKpre, UCellLink LKnxt ){ 
                int noP=LKpre.no, noN=LKnxt.no;
                List<UCell>   qBDL=pSA.pGP.BDL;
                UCell UCX=qBDL[LKpre.rc2];
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

            public void Create_Cell2ALS_Link( ALSLinkMan ALSman ){
                if( LinkCeAlsLst!=null ) return ;
                LinkCeAlsLst = new List<LinkCellAls>[81];
                if( ALSman.ALSLst==null || ALSman.ALSLst.Count<2 )  return;

                List<UCell>  qBDL=pSA.pGP.BDL;
                foreach( var PA in ALSman.ALSLst.Where(P=>P.singly) ){
                    foreach( var no in PA.FreeB.IEGet_BtoNo() ){
                        int noB=(1<<no);
                        int rcbDir=0;
                        foreach( var P in PA.UCellLst.Where(q=>(q.FreeB&noB)>0) ){
                            rcbDir |= ( (1<<(P.b+18)) | (1<<(P.c+9)) | (1<<(P.r)) );
                        }

                        for( int tx=0; tx<27; tx+=9 ){
                            int d = rcbDir&(0x1FF<<tx);
                            if( d.BitCount()!=1 ) continue;
                            int tfx=d.BitToNum(27);

                            foreach( var P in qBDL.IEGet(tfx,noB) ){
                                if( PA.B81.IsHit(P.rc) ) continue;

                                var Q = new LinkCellAls(P,PA,no);
                                if( LinkCeAlsLst[P.rc]==null ){
                                    LinkCeAlsLst[P.rc]=new List<LinkCellAls>();
                                }
                                else if( LinkCeAlsLst[P.rc].Contains(Q) ) continue;
                                LinkCeAlsLst[P.rc].Add(Q);
                            }
                        }
                    }
                }
                for( int rc=0; rc<81; rc++ ) if( LinkCeAlsLst[rc]!=null ) LinkCeAlsLst[rc].Sort();
            }
        }   
        public class UCellLink: IComparable{
            static public GNPZ_Analyzer pSA;
            static private PAnalyzer PA=new PAnalyzer();
            public List<UCell>       pBDL{ get{ return pSA.pBDL; } }
            public int               ID;    //初期値はtfx 外部で再設定する
            public int               tfx;
            public int               type;
            public bool              SFlag; //T:Strong
            public bool              LoopFlag; //ループ形成の最後のリンク
            public readonly int      no;
            public readonly int      rc1;
            public readonly int      rc2;
            public UCell             UCe1{ get{ return pBDL[rc1]; } }
            public UCell             UCe2{ get{ return pBDL[rc2]; } }

            public UCellLink(){}
            public UCellLink( int tfx, int type, int no, int rc1, int rc2, bool SFlag=false ){
                this.tfx=tfx; this.type=type; this.SFlag=SFlag; 
                this.no=no; this.rc1=rc1; this.rc2=rc2; this.ID=tfx;
            }

            public UCellLink Reverse(){
                UCellLink ULK=new UCellLink(tfx,type,no,rc2,rc1,SFlag);
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
        }         
        public class LinkCellAls: IComparable{
            public readonly UCell UC;
            public readonly UALS  ALS;
            public readonly int   nRCC=-1; //no:0...8 (doubly の場合は個別にリンクを作る)
            public LinkCellAls( UCell UC, UALS ALS, int nRCC ){
                this.UC=UC; this.ALS=ALS; this.nRCC=nRCC;
            }
            public  override bool Equals( object obj ){
                var A = obj as LinkCellAls;
                return (this.ALS.ID==A.ALS.ID);
            }
            public int CompareTo( object obj ){
                LinkCellAls A = obj as LinkCellAls;
                return (this.ALS.ID-A.ALS.ID);
            }
        }
    }
}
