using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;

using GIDOO_space;

namespace GNPZ_sdk {

    public partial class GNPZ_Analyzer{ 

        public class GroupedLinkMan{
            private const int     S=1, W=2;
            private GNPZ_Analyzer pSA;
            private List<UCell>   pBDL;
            public  int           SWCtrl;
            private Bit81[]       pHouseCells;
            public List<GroupedLink> GrpCeLKLst;
            private int[]         bitRC=new int[9];

            public GroupedLinkMan( GNPZ_Analyzer pSA ){
                this.pSA  = pSA;               
                SWCtrl=0;

                for( int k=0; k<9; k++ ){
                    int r0=k/3, c0=k%3, bit=0;
                    for( int m=0; m<9; m++ ){
                        if( m/3!=r0 && m%3!=c0 ) bit |= (1<<m);
                    }
                    bitRC[k]=bit;
                }
            }
             
            public void PrepareGroupedLinkMan( ){
                if( SWCtrl==0 ) GrpCeLKLst=new List<GroupedLink>();
                this.pBDL = pSA.pBDL;
                SearchGroupedLink();
                GrpCeLKLst.Sort();

           //     Console.WriteLine("GrpCeLKLst.Count:"+GrpCeLKLst.Count);
           //     int cc=0;
           //     GrpCeLKLst.ForEach(P=>{
           //         Console.WriteLine( "{0}:{1}", (cc++).ToString().PadLeft(3), P.ToString());
           //     } );

                foreach( var P in GrpCeLKLst ){
                    if( P.no!=P.no2 )  Console.WriteLine(P);
                }
            }   
        
            private void SearchGroupedLink(){
                try{
                    this.pHouseCells = pSA.HouseCells;
                    UGrCells[] LQ=new UGrCells[3];
                    for( int no=0; no<9; no++ ){
                        int noB=1<<no;
                        Bit81 BPnoB = new Bit81(pBDL,noB);
            
                        //------------------------------------------
                        for( int tfx=0; tfx<18; tfx++ ){
                            Bit81 BPnoB2 = BPnoB&pHouseCells[tfx];

                            List<Bit81> houseLst=new List<Bit81>();
                            List<int>   tfxLst=new List<int>();
                            for( int k=0; k<9; k++ ){
                                int hx = (tfx<9)? (k+9): k;
                                Bit81 BX = BPnoB2&pHouseCells[hx];
                                if( BX.IsZero() ) continue;
                                houseLst.Add(BX);
                                tfxLst.Add(hx);
                            }
                            for( int k=0; k<3; k++ ){
                                int hx = (tfx<9)? (tfx/3*3+k): ((tfx-9)/3+k*3);
                                hx += 18;
                                Bit81 BX = BPnoB2&pHouseCells[hx];
                                if( BX.IsZero() ) continue;
                                houseLst.Add(BX);
                                tfxLst.Add(hx);
                            }

                            if( houseLst.Count<2 ) continue;
                            Permutation prm=new Permutation(houseLst.Count,2);
                            while( prm.Successor() ){
                                int na=prm.Pnum[0], nb=prm.Pnum[1];
                                Bit81 HA=houseLst[na];
                                Bit81 HB=houseLst[nb];
                                if( !(HA&HB).IsZero() ) continue;
                                UGrCells LA=new UGrCells(tfxLst[na],no);
                                UGrCells LB=new UGrCells(tfxLst[nb],no);
                                foreach( var P in HA.IEGetUCeNoB(pBDL,0x1FF) ) LA.Add(P);
                                foreach( var P in HB.IEGetUCeNoB(pBDL,0x1FF) ) LB.Add(P);
                                SetGroupedLink(tfxLst[nb],W,no,LA,LB);
                                if( !(BPnoB2-(HA|HB)).IsZero() ) continue;
                                SetGroupedLink(tfxLst[nb],S,no,LA,LB);
                            }
                        }
           
                        //------------------------------------------
                        for( int tfx=18; tfx<27; tfx++ ){
                            int bx = tfx-18;
                            int b0 = (bx/3*27+(bx%3)*3); //ブロックの左上のセル番号

                            Bit81 BPnoB2 = BPnoB&pHouseCells[tfx];
                            List<Bit81> houseLst=new List<Bit81>();
                            List<int>   tfxLst=new List<int>();
                            for( int k=0; k<3; k++ ){
                                int r0=(b0/9+k);
                                Bit81 BX = BPnoB2&pHouseCells[r0];
                                if( BX.IsZero() ) continue;
                                houseLst.Add(BX);
                                tfxLst.Add(r0);
                            }
                            for( int k=0; k<3; k++ ){
                                int c0=(b0%9)+k;
                                Bit81 BX = BPnoB2&pHouseCells[c0+9];
                                if( BX.IsZero() ) continue;
                                houseLst.Add(BX);
                                tfxLst.Add(c0+9);
                            }

                            if( houseLst.Count>=2 ){
                                Permutation prm=new Permutation(houseLst.Count,2);
                                while( prm.Successor() ){
                                    int na=prm.Pnum[0], nb=prm.Pnum[1];
                                    Bit81 HA=houseLst[na];
                                    Bit81 HB=houseLst[nb]-HA;
                                    if( HB.IsZero() ) continue;
                                    UGrCells LA=new UGrCells(tfxLst[na],no);
                                    UGrCells LB=new UGrCells(tfxLst[nb],no);
                                    foreach( var P in HA.IEGetUCeNoB(pBDL,0x1FF) ) LA.Add(P);
                                    foreach( var P in HB.IEGetUCeNoB(pBDL,0x1FF) ) LB.Add(P);
                                    SetGroupedLink(tfxLst[nb],W,no,LA,LB);
                                    if( !(BPnoB2-(HA|HB)).IsZero() ) continue;
                                    SetGroupedLink(tfxLst[nb],S,no,LA,LB);
                                }
                            }
                            if( houseLst.Count>=1 ){
                                for( int na=0; na<houseLst.Count; na++ ){
                                    Bit81 HA=houseLst[na];
                                    UGrCells LA=new UGrCells(tfxLst[na],no);
                                    foreach( var P in HA.IEGetUCeNoB(pBDL,0x1FF) ) LA.Add(P);
                                    foreach( var rc in BPnoB2.IEGet_rc() ){
                                        if( HA.IsHit(rc) ) continue;
                                        UGrCells LB=new UGrCells(-9,no,pBDL[rc]);
                                        SetGroupedLink(tfxLst[na],W,no,LA,LB);
                                        SetGroupedLink(-9,W,no,LB,LA);
                                    }
                                }
                            }
                        }

                    }
                }
                catch( Exception ex ){
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            private  void SetGroupedLink( int tfx, int type, int no, UGrCells LA, UGrCells LB, bool Print=false ){
                if( LA.Count==0 || LB.Count==0 ) return;
                if( LA.Count==1 && LB.Count==1 ) return ;
                GroupedLink GrpLK = new GroupedLink(LA,LB,tfx,type);
                int ix = GrpCeLKLst.FindIndex(P=>(P.Equals(GrpLK)));
                if( ix>=0 ) return;
                GrpCeLKLst.Add(GrpLK);

                if( Print ){
                    Console.WriteLine("\r*LA tfx:{0} type:{1} no:{2}", tfx, type, (no+1) );
                    LA.ForEach(P=>Console.WriteLine(P));
                    Console.WriteLine("*LB");
                    LB.ForEach(P=>Console.WriteLine(P));
                }
            } 

            public IEnumerable<GroupedLink> IEGet_GlkNoType( UGrCells GLK, int no, int typB ){
                foreach( var P in GrpCeLKLst ){
                    if( P.UGCellsA==GLK )  yield return  P;
                }
                yield break;
            }
        }         
        public class GroupedLink: IComparable{
            public UCellLink        UCelLK=null;

            public UGrCells         UGCellsA;
            public UGrCells         UGCellsB;
            public int no{  get{ return UGCellsA.no; } }
            public int no2{ get{ return UGCellsB.no; } }

            public int              type;
            public int              tfx;   //house番号
            public readonly int     FreeB; //UGCellsの要素数字
            public bool             LoopFlag; //ループ形成の最後のリンク
            public Bit81            UsedCs;

            public GroupedLink(){}

            public GroupedLink( UGrCells UGCellsA, UGrCells UGCellsB, int tfx, int type ){
                this.UGCellsA = UGCellsA; this.UGCellsB = UGCellsB; this.tfx=tfx;
                this.type=type;

                FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
                FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
            }
            public GroupedLink(UGrCells UGCellsA, UGrCells UGCellsB, int type ){
                this.UGCellsA=UGCellsA; this.UGCellsB=UGCellsB;
                this.type=type;
            }

            public GroupedLink( UCellLink LK ){
                UCelLK=LK;
                UGCellsA=new UGrCells(LK.tfx,LK.no,LK.UCe1);
                UGCellsB=new UGrCells(LK.tfx,LK.no,LK.UCe2);
                this.type=LK.type;
                this.tfx =LK.tfx;

                FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
                FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
            }

            public override bool Equals( object obj ){
                GroupedLink Q = obj as GroupedLink;
                if( Q==null )  return true;
                if( this.type!=Q.type )  return false;
                if( this.tfx!=Q.tfx )    return false;
                if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
                if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
                return true;
            }

            public int CompareTo( object obj ){
                GroupedLink Q = obj as GroupedLink;
                int ret= this.UGCellsA.CompareTo(Q.UGCellsA);
                if( ret!=0 )  return ret;
                return this.UGCellsB.CompareTo(Q.UGCellsB);
            }

            public override string ToString(){
                string st= "tfx:"+tfx.ToString().PadLeft(2) +" no:"+no+" type:"+type+" ";
                st += UGCellsA.ToString()+ " -> "+UGCellsB.ToString();
                return st;
            }
        }

        public class ALSLinkMan{
            private GNPZ_Analyzer pSA;
            private List<UCell>   pBDL;
            public List<UALS>     ALSLst;
            public List<ALSLink>  ALSLinkLst;
            private Bit81[]       pHouseCells;

            public ALSLinkMan( GNPZ_Analyzer pSA ){
                this.pSA = pSA;
                UALS.pSA = pSA;             
            }
            public void Reset(){
                ALSLst    =null;
                ALSLinkLst=null;
            }
            public int ALS_Search( int nPls ){
                if( ALSLst!=null ) return ALSLst.Count();
                this.pBDL = pSA.pBDL;
                UALS.pBDL = pSA.pBDL;
                int ALSSizeMax = GNumPzl.ALSSizeMax;
                this.pHouseCells = pSA.HouseCells;

                int mx=0; //仮のID、後に再設定
                ALSLst = new List<UALS>();
                List<int> singlyMan = new List<int>();
                for( int nn=1; nn<=nPls; nn++ ){
                    for( int tf=0; tf<27; tf++ ){
                        List<UCell> P=pBDL.IEGet(tf,0x1FF).ToList();
                        if( P.Count<1 ) continue;
                        int szMax = Math.Min(P.Count,8-nn);
                        szMax = Math.Min(szMax,ALSSizeMax);       //【変更】ALSサイズ最大値を制限
                        for( int sz=1; sz<=szMax; sz++ ){
                            Combination cmb = new Combination(P.Count,sz);
                            while( cmb.Successor() ){                        
                                int FreeB=0;
                                Array.ForEach(cmb.Cmb, q=> FreeB|=P[q].FreeB );
                                if( FreeB.BitCount()!=(sz+nn) ) continue;
                                List<UCell> Q=new List<UCell>();
                                Array.ForEach(cmb.Cmb, q=> Q.Add(P[q]) );
                        
                                //同じ構成のALSの存在チェック⇒A.singly=true:最初 false;2つ目以降
                                UALS UA=new UALS(mx,sz,tf,FreeB,Q);
                                if( !UA.IsALS() ) continue;
                                int hs= UA.GetHashCode();
                                if( singlyMan.Count>0 && singlyMan.Contains(hs) ) UA.singly=false;
                                singlyMan.Add(hs);

                                mx++;
                                ALSLst.Add(UA);
                            }
                        }
                    }
                }

                ALSLst.Sort();
                int ID=0;
                ALSLst.ForEach(P=> P.ID=ID++ );
                // ALSLst.ForEach(P=>Console.WriteLine(P));
                return ALSLst.Count();  
            }
             
            public IEnumerable<UALS> IEGet( int lvl, int noB, Bit81 Area, int tfx=-1 ){
                foreach( var P in ALSLst.Where(p=>p.Level==lvl) ){
                    if( (P.FreeB&noB)==0 ) continue;
                    if( tfx>0 && P.tfx!=tfx ) continue;
                    if( !(P.B81-Area).IsZero() ) continue;
                    yield return P;
                }
            }
                   
            public int  GetALSRCC( UALS UA, UALS UB ){           
                if( (UA.FreeB&UB.FreeB)==0 )       return 0; //共通数字なし
                if( !(UA.B81&UB.B81).IsZero() )    return 0; //範囲が重なる
                if( (UA.rcbFilter&UB.B81).IsZero() ) return 0; //House接触なし 

                int RCC=0, Dir=UA.rcbDir&UB.rcbDir;
                //rcbDir |= ( (1<<(P.b+18)) | (1<<(P.c+9)) | (1<<(P.r)) );
                foreach( int tfx in Dir.IEGet_BtoNo(27) ){
                    Bit81 ComH = pSA.HouseCells[tfx];
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
            public void SearchALSLink(){
                if( ALSLst==null ) ALS_Search(1);
                if( ALSLst.Count==0 )  return;
                if( ALSLinkLst!=null )  return;

                foreach( var P in ALSLst.Where(p=>(p.Size>=2&&p.singly)) ){
                    List<int> noLst=P.FreeB.IEGet_BtoNo().ToList();
                    Permutation prm=new Permutation(noLst.Count,2);
                    while( prm.Successor(2) ){
                        int noS = noLst[prm.Pnum[0]];
                        int noD = noLst[prm.Pnum[1]];
                        UGrCells GS = P.SelectNoCells(noS);
                        UGrCells GD = P.SelectNoCells(noD);
     //*****            if( GS.Count>P.Size || GD.Count>P.Size )  continue;
                        Bit81 B81D = new Bit81();
                        GD.ForEach(q=>B81D.BPSet(q.rc));
                        for( int tfx=0; tfx<27; tfx++ ){
                            if( !(B81D-pHouseCells[tfx]).IsZero() )  continue;  
                            SetGroupedLink(P,GS,GD,tfx);
                        }
                    }
                }
                if( ALSLinkLst==null || ALSLinkLst.Count<=0 ) return;
                ALSLinkLst.Sort();
                int ID=0;
                ALSLinkLst.ForEach(P=>P.ID=(ID++));
            //    ALSLinkLst.ForEach(P=>{
            //    //    if( P.ALSbase.Size==2 && P.tfx==18 ){
            //            Console.WriteLine("ALSLink {0} -> tfx:{1} {2}", 
            //                P.UGCellsA.GCToString(), P.tfx, P.UGCellsB.GCToString() );
            //   //    }
            //    } );
            }
            
            private  void SetGroupedLink( UALS P, UGrCells GS, UGrCells GD, int tfx ){
                ALSLink ALSLK= new ALSLink(P,GS,GD,tfx);
                if( ALSLinkLst==null ) ALSLinkLst=new List<ALSLink>();
                if( ALSLinkLst.Count>0 ){
                    int ix = ALSLinkLst.FindIndex(Q=>(Q.Equals(ALSLK)));
                    if( ix>=0 ) return;
                }
                ALSLinkLst.Add(ALSLK);

                //Console.WriteLine("ALSLink {0} -> tfx:{1} {2}", GS.GCToString(), tfx, GD.GCToString() );
            }

            public void Create_ALS2ALS_Link( bool doubly ){
                var cmb = new Combination( ALSLst.Count, 2 );
                while (cmb.Successor()) {
                    UALS UA = ALSLst[cmb.Cmb[0]];
                    UALS UB = ALSLst[cmb.Cmb[1]];

                    int RCC = GetALSRCC( UA, UB );
                    if( RCC==0 ) continue;
                    if( !doubly && RCC.BitCount()!=1 ) continue;

                    if( UA.ConnLst==null )  UA.ConnLst=new List<UALSPair>();
                    if( UB.ConnLst==null )  UB.ConnLst=new List<UALSPair>();
                    foreach( var no in RCC.IEGet_BtoNo() ){ //RCCの数だけ登録
                        UALSPair LKX=new UALSPair(UA,UB,RCC,no);
                        if( !UA.ConnLst.Contains(LKX) ) UA.ConnLst.Add(LKX);
                        LKX=new UALSPair(UB,UA,RCC,no);
                        if( !UB.ConnLst.Contains(LKX) ) UB.ConnLst.Add(LKX);
                    }
                }
            }   
        } 
        public class ALSLink: GroupedLink, IComparable{
            private const int          S=1, W=2;
            public int                 ID;
            public UALS                ALSbase=null;

            public ALSLink( UALS ALSbase, UGrCells UGCellsA, UGrCells UGCellsB, int tfx ){
                this.ALSbase=ALSbase;
                this.UGCellsA=UGCellsA;
                this.UGCellsB=UGCellsB;
                this.tfx=tfx;
                this.type=S;
            }

            public int CompareTo( object obj ){
                ALSLink Q = obj as ALSLink;
                int ret= this.UGCellsA.CompareTo(Q.UGCellsA);
                if( ret!=0 )  return ret;
                return this.UGCellsB.CompareTo(Q.UGCellsB);
            }     
       
            public override bool Equals( object obj ){
                ALSLink Q = obj as ALSLink;
                if( Q==null )  return true;
                if( this.tfx!=Q.tfx )  return false;
                if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
                if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
                return true;
            }

            public override string ToString(){
                string st= "tfx:"+tfx.ToString().PadLeft(2) +" type:"+type+" ";
                st += UGCellsA.ToString()+"/"+UGCellsA.no + " -> "+UGCellsB.ToString()+"/"+UGCellsB.no;
                return st;
            }

        }
    }
}
