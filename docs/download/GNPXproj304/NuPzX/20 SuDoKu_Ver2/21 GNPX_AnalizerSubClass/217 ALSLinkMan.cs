using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Math;

using GIDOO_space;

namespace GNPZ_sdk {
    public class ALSLinkMan{
        private GNPX_AnalyzerMan pAnMan;
        private List<UCell>   pBDL{ get{ return pAnMan.pBDL; } }
        private Bit81[]       pHouseCells=AnalyzerBaseV2.HouseCells;
        private Bit81[]       pConnectedCells=AnalyzerBaseV2.ConnectedCells;

        public List<UALS>     ALSLst;

        public List<ALSLink>        AlsInnerLink;   //innerLink
        public List<LinkCellAls>[]  LinkCeAlsLst;   //Cell->ALS
        public bool                 ALS2ALS_Link;   //ALS ->ALS

        public ALSLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan = pAnMan;
		}

		public void Initialize() {
            ALSLst    =null;
            AlsInnerLink=null;
            LinkCeAlsLst=null;
            ALS2ALS_Link=false;
        }
        public int PrepareALSLinkMan( int nPls ){ //QALS_Search
            if( ALSLst!=null ) return ALSLst.Count();
            int ALSSizeMax = GNPXApp000.ALSSizeMax;

            int mx=0; //tentative ID, reset later
            ALSLst = new List<UALS>();
            List<int> singlyMan = new List<int>();
            for( int nn=1; nn<=nPls; nn++ ){
                for( int tf=0; tf<27; tf++ ){
                    List<UCell> Pcells=pBDL.IEGetCellInHouse(tf,0x1FF).ToList();
                    if( Pcells.Count<1 ) continue;
                    int szMax = Min(Pcells.Count,8-nn);
                    szMax = Min(szMax,ALSSizeMax);  //ALS size maximum value
                    for( int sz=1; sz<=szMax; sz++ ){
                        Combination cmb = new Combination(Pcells.Count,sz);
                        while( cmb.Successor() ){                        
                            int FreeB=0;
                            Array.ForEach(cmb.Cmb, q=> FreeB|=Pcells[q].FreeB );
                            if( FreeB.BitCount()!=(sz+nn) ) continue;
                            List<UCell> Q=new List<UCell>();
                            Array.ForEach(cmb.Cmb, q=> Q.Add(Pcells[q]) );
                        
                            //Check for existence of ALS with the same configuration
                            UALS UA=new UALS(mx++,sz,tf,FreeB,Q);
                            if( !UA.IsPureALS() ) continue;
                            int hs= UA.GetHashCode();
                            if( singlyMan.Any(p=>p==hs) )  UA.singly=false;
                            else singlyMan.Add(hs);

                            ALSLst.Add(UA);
                        }
                    }
                }
            }

            ALSLst.Sort();
            int ID=0;
            ALSLst.ForEach(P=> P.ID=ID++ );
            // ALSLst.ForEach(P=>WriteLine(P));
            return ALSLst.Count();  
        }
             
        public IEnumerable<UALS> IEGetCellInHouse( int lvl, int noB, Bit81 Area, int tfx=-1 ){
            foreach( var P in ALSLst.Where(p=>p.Level==lvl) ){
                if( (P.FreeB&noB)==0 )       continue;
                if( tfx>0 && P.tfx!=tfx )    continue;
                if( !(P.B81-Area).IsZero() ) continue;
                yield return P;
            }
        }
          
        //RCC
        public int  Get_AlsAlsRcc( UALS UA, UALS UB ){           
            if( (UA.FreeB&UB.FreeB)==0 )       return 0;   //no common digit
            if( !(UA.B81&UB.B81).IsZero() )    return 0;   //overlaps　
            if( (UA.rcbFilter&UB.B81).IsZero() ) return 0; //no contact 

            int RCC=0, Dir=UA.rcbDir&UB.rcbDir;
            //[definition] rcbDir |= ( (1<<(P.b+18)) | (1<<(P.c+9)) | (1<<(P.r)) );
            foreach( int tfx in Dir.IEGet_BtoNo(27) ){
                Bit81 ComH = pHouseCells[tfx];
                int FrAi=0, FrAo=0, FrBi=0, FrBo=0;
                UA.UCellLst.ForEach(P=>{
                    if( ComH.IsHit(P.rc) ) FrAi |= P.FreeB;
                    else                   FrAo |= P.FreeB;
                } );
                UB.UCellLst.ForEach(P=>{
                    if( ComH.IsHit(P.rc) ) FrBi |= P.FreeB;
                    else                   FrBo |= P.FreeB;
                } );
                RCC |= (FrAi.DifSet(FrAo)) & (FrBi.DifSet(FrBo));    //RCC
            }
            return RCC;
        }

        //Link in ALS(when 1-number is excluded and locked, other digits are in one house）
        public void QSearch_AlsInnerLink(){
            if( ALSLst==null ) PrepareALSLinkMan(1);
            if( ALSLst.Count==0 )  return;
            if( AlsInnerLink!=null )  return;

            foreach( var Pals in ALSLst.Where(p=>(p.Size>=2&&p.singly)) ){
                List<int> noLst=Pals.FreeB.IEGet_BtoNo().ToList();
                Permutation prm=new Permutation(noLst.Count,2);//using permutation
                while( prm.Successor(2) ){
                    int noS = noLst[prm.Pnum[0]];
                    int noD = noLst[prm.Pnum[1]];

                    UGrCells GS = Pals.SelectNoCells(noS);//ALS cell group with noS
                    UGrCells GD = Pals.SelectNoCells(noD);//ALS cell group with noD

                    Bit81 B81D = new Bit81();
                    GD.ForEach(q=>B81D.BPSet(q.rc));

                    for( int tfx=0; tfx<27; tfx++ ){
                        if( !(B81D-pHouseCells[tfx]).IsZero() )  continue;  
                        _SetGroupedLink(Pals,GS,GD,tfx);
                    }
                }
            }
            if( AlsInnerLink==null || AlsInnerLink.Count<=0 ) return;
            AlsInnerLink.Sort();
            int ID=0;
            AlsInnerLink.ForEach(P=>P.ID=(ID++));
        //    AlsInnerLink.ForEach(P=>{
        //    //    if( P.ALSbase.Size==2 && P.tfx==18 ){
        //            WriteLine($"ALSLink {P.UGCellsA.GCToString()} -> tfx:{P.tfx} {P.UGCellsB.GCToString()}" );
        //   //    }
        //    } );
        }
        private  void _SetGroupedLink( UALS P, UGrCells GS, UGrCells GD, int tfx ){
            ALSLink ALSLK= new ALSLink(P,GS,GD,tfx);
            if( AlsInnerLink==null ) AlsInnerLink=new List<ALSLink>();
            if( AlsInnerLink.Count>0 ){
                int ix = AlsInnerLink.FindIndex(Q=>(Q.Equals(ALSLK)));
                if( ix>=0 ) return;
            }
            AlsInnerLink.Add(ALSLK);

            //WriteLine( $"ALSLink {GS.GCToString()} -> tfx:{tfx} {GD.GCToString()}" );
        }
          
        //Link between Cell and ALS
        public void QSearch_Cell2ALS_Link( ){
            if(ALSLst==null) PrepareALSLinkMan(1);
            if( LinkCeAlsLst!=null ) return ;
            LinkCeAlsLst = new List<LinkCellAls>[81];
            if( ALSLst==null || ALSLst.Count<2 )  return;

            foreach( var PA in ALSLst.Where(P=>P.singly) ){
                foreach( var no in PA.FreeB.IEGet_BtoNo() ){
                    int noB=(1<<no);
                    Bit81 H=new Bit81(true);
                    foreach( var P in PA.UCellLst.Where(q=>(q.FreeB&noB)>0) ){
                        H&=pConnectedCells[P.rc];
                    }
                    if(H.IsZero()) continue;
                    foreach( var P in H.IEGetUCeNoB(pBDL,noB) ){
                        var Q = new LinkCellAls(P,PA,no);
                        if( LinkCeAlsLst[P.rc]==null )  LinkCeAlsLst[P.rc]=new List<LinkCellAls>();
                        LinkCeAlsLst[P.rc].Add(Q);
                    }
                }
            }
            for( int rc=0; rc<81; rc++ ) if( LinkCeAlsLst[rc]!=null ) LinkCeAlsLst[rc].Sort();
        }

        //Link by ALS-ALS RCC
        public void QSearch_ALS2ALS_Link( bool doubly ){
            if(ALSLst==null) PrepareALSLinkMan(1);

            if(ALS2ALS_Link) return;
            ALS2ALS_Link=true;

            var cmb = new Combination( ALSLst.Count, 2 );
            while (cmb.Successor()){
                UALS UA = ALSLst[cmb.Cmb[0]];
                UALS UB = ALSLst[cmb.Cmb[1]];

                int RCC = Get_AlsAlsRcc( UA, UB );
                if( RCC==0 ) continue;
                if( !doubly && RCC.BitCount()!=1 ) continue;

                if( UA.ConnLst==null )  UA.ConnLst=new List<UALSPair>();
                if( UB.ConnLst==null )  UB.ConnLst=new List<UALSPair>();
                foreach( var no in RCC.IEGet_BtoNo() ){
                    UALSPair LKX=new UALSPair(UA,UB,RCC,no);
                    if( !UA.ConnLst.Contains(LKX) ) UA.ConnLst.Add(LKX);
                    LKX=new UALSPair(UB,UA,RCC,no);
                    if( !UB.ConnLst.Contains(LKX) ) UB.ConnLst.Add(LKX);
                }
            }
        }   
    } 

    public class ALSLink: GroupedLink, IComparable{
        private const int  S=1, W=2;
        public int         ID;
        public UALS        ALSbase=null;

        public ALSLink( UALS ALSbase, UGrCells UGCellsA, UGrCells UGCellsB, int tfx ){
            this.ALSbase=ALSbase;
            this.UGCellsA=UGCellsA;
            this.UGCellsB=UGCellsB;
            this.tfx=tfx;
            this.type=S;
        }

        public new int CompareTo( object obj ){
            ALSLink Q = obj as ALSLink;
            int ret= this.UGCellsA.CompareTo(Q.UGCellsA);
            if( ret!=0 )  return ret;
            return this.UGCellsB.CompareTo(Q.UGCellsB);
        }     
       
        public override bool Equals( object obj ){
            ALSLink Q = obj as ALSLink;
            if( Q==null )  return true;
//          if( this.tfx!=Q.tfx )  return false;
            if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
            if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
            return true;
        }

        public override string ToString(){
            string st= "tfx:"+tfx.ToString().PadLeft(2) +" type:"+type+" ";
            st += UGCellsA.ToString()+"/"+UGCellsA.no + " -> "+UGCellsB.ToString()+"/"+UGCellsB.no;
            return st;
        }
        public override int GetHashCode(){ return base.GetHashCode(); }
    }
}