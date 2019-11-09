using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace GNPZ_sdk {
    public class GroupedLinkMan{
        static public Bit81[] pHouseCells{ get{ return AnalyzerBaseV2.HouseCells; } }
        private const int     S=1, W=2;
        private GNPX_AnalyzerMan pAnMan;
        private List<UCell>   pBDL{ get{ return pAnMan.pBDL; } }
        public  int           SWCtrl;
        public List<GroupedLink> GrpCeLKLst;
        private int[]         bitRC=new int[9];

        public GroupedLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan  = pAnMan;               
            SWCtrl=0;

            for(int k=0; k<9; k++ ){
                int r0=k/3, c0=k%3, bit=0;
                for(int m=0; m<9; m++ ){ if( m/3!=r0 && m%3!=c0 ) bit |= (1<<m); }
                bitRC[k]=bit;
            }
		}

		public void Initialize(){
			GrpCeLKLst=new List<GroupedLink>();
		}

		public void PrepareGroupedLinkMan(){
            SearchGroupedLink();
            GrpCeLKLst.Distinct();
            GrpCeLKLst.Sort();

        //     WriteLine("GrpCeLKLst.Count:"+GrpCeLKLst.Count);
        //     int cc=0;
        //     GrpCeLKLst.ForEach(P=>{
        //         WriteLine( $"{(cc++).ToString().PadLeft(3)}:{P.ToString()}" );
        //     } );

            foreach( var P in GrpCeLKLst ){
                if( P.no!=P.no2 )  WriteLine(P);
            }
        }   
        
       private void SearchGroupedLink(){
            try{
                UGrCells[] LQ=new UGrCells[3];
                for(int no=0; no<9; no++ ){
                    int noB=1<<no;
                    Bit81 BPnoB = new Bit81(pBDL,noB);
            
                    //------------------------------------------
                    for(int tfx=0; tfx<18; tfx++ ){
                        Bit81 BPnoB2 = BPnoB&pHouseCells[tfx];

                        List<Bit81> houseLst=new List<Bit81>();
                        List<int>   tfxLst=new List<int>();
                        for(int k=0; k<9; k++ ){
                            int hx = (tfx<9)? (k+9): k;
                            Bit81 BX = BPnoB2&pHouseCells[hx];
                            if( BX.IsZero() ) continue;
                            houseLst.Add(BX);
                            tfxLst.Add(hx);
                        }
                        for(int k=0; k<3; k++ ){
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
                            int na=prm.Index[0], nb=prm.Index[1];
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
                    for(int tfx=18; tfx<27; tfx++ ){
                        int bx = tfx-18;
                        int b0 = (bx/3*27+(bx%3)*3); //Cell number at the top left of the block

                        Bit81 BPnoB2 = BPnoB&pHouseCells[tfx];
                        List<Bit81> houseLst=new List<Bit81>();
                        List<int>   tfxLst=new List<int>();
                        for(int k=0; k<3; k++ ){
                            int r0=(b0/9+k);
                            Bit81 BX = BPnoB2&pHouseCells[r0];
                            if( BX.IsZero() ) continue;
                            houseLst.Add(BX);
                            tfxLst.Add(r0);
                        }
                        for(int k=0; k<3; k++ ){
                            int c0=(b0%9)+k;
                            Bit81 BX = BPnoB2&pHouseCells[c0+9];
                            if( BX.IsZero() ) continue;
                            houseLst.Add(BX);
                            tfxLst.Add(c0+9);
                        }

                        if( houseLst.Count>=2 ){
                            Permutation prm=new Permutation(houseLst.Count,2);
                            while( prm.Successor() ){
                                int na=prm.Index[0], nb=prm.Index[1];
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
                            for(int na=0; na<houseLst.Count; na++ ){
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
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);
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
                WriteLine( $"\r*LA tfx:{tfx} type:{type} no:{(no+1)}" );
                LA.ForEach(P=>WriteLine(P));
                WriteLine("*LB");
                LB.ForEach(P=>WriteLine(P));
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
        private const int     S=1, W=2;
        public UCellLink     UCelLK=null;

        public UGrCells      UGCellsA;
        public UGrCells      UGCellsB;
        public int no{  get{ return UGCellsA.no; } }
        public int no2{ get{ return UGCellsB.no; } }

		public bool			rootF; 
        public int          type;
        public int          tfx=-1;   //house number(-1:in-cell Link)
        public readonly int FreeB;    //element number of UGCells
        public bool         LoopFlag; //last link of loop
        public Bit81        UsedCs=new Bit81();
        public object       preGrpedLink;

        public GroupedLink(){}

        public GroupedLink( UGrCells UGCellsA, UGrCells UGCellsB, int tfx, int type ){
            this.UGCellsA = UGCellsA; this.UGCellsB = UGCellsB; //this.tfx=tfx;
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

		public GroupedLink( UCell UC, int no1, int no2, int type, bool rootF=false ){
			this.rootF=rootF;
			int F = (1<<no1) | (1<<no2);
			// if( no1==no2 || (UC.FreeB&(1<<no1))==0 || (UC.FreeB&(1<<no2))==0 ){
			if( (UC.FreeB&(1<<no1))==0 || (UC.FreeB&(1<<no2))==0 ){
				UGCellsA=UGCellsB=null;
				return;
			}
			
			UGCellsA=new UGrCells(-1,no1,UC);
			UGCellsB=new UGrCells(-1,no2,UC);
			UCelLK=null;
			this.type= (UC.FreeBC==2)? type: W;

			FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
		}

        public override bool Equals( object obj ){
            GroupedLink Q = obj as GroupedLink;
            if( Q==null )  return true;
            if( this.type!=Q.type )  return false;
//            if( this.tfx!=Q.tfx )    return false;
            if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
            if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
            return true;
        }
		public bool EqualNull(){ return (tfx==-1); }

        public int CompareTo( object obj ){
            GroupedLink Q = obj as GroupedLink;
            int ret= this.UGCellsA.CompareTo(Q.UGCellsA);
            if( ret!=0 )  return ret;
            return this.UGCellsB.CompareTo(Q.UGCellsB);
        }

        public override string ToString(){
            try{
                string st= "tfx:"+tfx.ToString().PadLeft(2) +" no:"+no+" type:"+type+" ";
                st += UGCellsA.ToString()+ " -> "+UGCellsB.ToString();
                return st;
            }
            catch(System.NullReferenceException ex ){
                WriteLine(ex.Message);
            }
            return "null Exception";
        }
        public string GrLKToString(){
            try{
                string P="+", M="-";
                if(type==1){ P="-"; M="+"; }
                string st= "["+ (type==1? "S": "W")+" ";
                if( this is ALSLink ){
                    ALSLink A=this as ALSLink;
                    string po="";
                    A.ALSbase.UCellLst.ForEach( p =>{  po += " r"+(p.r+1) + "c"+((p.c)+1); } );
                    st += "(ALS:"+po.ToString_SameHouseComp()+") ";
                }
                st += UGCellsA.ToString()+"/"+P+(no+1);
                st +=" -> "+UGCellsB.ToString()+"/"+M+(no2+1)+"]";
                return st;
            }
            catch(System.NullReferenceException ex ){
                WriteLine(ex.Message);
            }
            return "null Exception";
        }
        public override int GetHashCode(){ return base.GetHashCode(); }
    }
}