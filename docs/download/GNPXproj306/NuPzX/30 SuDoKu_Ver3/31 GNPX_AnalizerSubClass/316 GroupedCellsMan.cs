using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace GNPZ_sdk {
    public class GroupedLinkManEx{
        static public Bit81[] pHouseCells{ get{ return AnalyzerBaseV2.HouseCells; } }
        private const int     S=1, W=2;
        private GNPX_AnalyzerManEx pAnMan;
        private List<UCellEx>   pBDL{ get{ return pAnMan.pBDL; } }
        public  int           SWCtrl;
        public List<GroupedLinkEx> GrpCeLKLst;
        private int[]         bitRC=new int[9];

        public GroupedLinkManEx( GNPX_AnalyzerManEx pAnMan ){
            this.pAnMan  = pAnMan;               
            SWCtrl=0;

            for( int k=0; k<9; k++ ){
                int r0=k/3, c0=k%3, bit=0;
                for( int m=0; m<9; m++ ){
                    if( m/3!=r0 && m%3!=c0 ) bit |= (1<<m);
                }
                bitRC[k]=bit;
            }
		}

		public void Initialize(){
			GrpCeLKLst=new List<GroupedLinkEx>();
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
                UGrCellsEx[] LQ=new UGrCellsEx[3];
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
                            int na=prm.Index[0], nb=prm.Index[1];
                            Bit81 HA=houseLst[na];
                            Bit81 HB=houseLst[nb];
                            if( !(HA&HB).IsZero() ) continue;
                            UGrCellsEx LA=new UGrCellsEx(tfxLst[na],no);
                            UGrCellsEx LB=new UGrCellsEx(tfxLst[nb],no);
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
                        int b0 = (bx/3*27+(bx%3)*3); //Cell number at the top left of the block

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
                                int na=prm.Index[0], nb=prm.Index[1];
                                Bit81 HA=houseLst[na];
                                Bit81 HB=houseLst[nb]-HA;
                                if( HB.IsZero() ) continue;
                                UGrCellsEx LA=new UGrCellsEx(tfxLst[na],no);
                                UGrCellsEx LB=new UGrCellsEx(tfxLst[nb],no);
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
                                UGrCellsEx LA=new UGrCellsEx(tfxLst[na],no);
                                foreach( var P in HA.IEGetUCeNoB(pBDL,0x1FF) ) LA.Add(P);
                                foreach( var rc in BPnoB2.IEGet_rc() ){
                                    if( HA.IsHit(rc) ) continue;
                                    UGrCellsEx LB=new UGrCellsEx(-9,no,pBDL[rc]);
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

        private  void SetGroupedLink( int tfx, int type, int no, UGrCellsEx LA, UGrCellsEx LB, bool Print=false ){
            if( LA.Count==0 || LB.Count==0 ) return;
            if( LA.Count==1 && LB.Count==1 ) return ;
            GroupedLinkEx GrpLK = new GroupedLinkEx(LA,LB,tfx,type);
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

        public IEnumerable<GroupedLinkEx> IEGet_GlkNoType( UGrCellsEx GLK, int no, int typB ){
            foreach( var P in GrpCeLKLst ){
                if( P.UGCellsA==GLK )  yield return  P;
            }
            yield break;
        }
    }         
    public class GroupedLinkEx: IComparable{
        private const int     S=1, W=2;
        public UCellLinkEx     UCelLK=null;

        public UGrCellsEx      UGCellsA;
        public UGrCellsEx      UGCellsB;
        public int no{  get{ return UGCellsA.no; } }
        public int no2{ get{ return UGCellsB.no; } }

		public bool			rootF; 
        public int          type;
        public int          tfx=-1;   //house number(-1:in-cell Link)
        public readonly int FreeB;    //element number of UGCells
        public bool         LoopFlag; //last link of loop
        public Bit81        UsedCs=new Bit81();
        public object       preGrpedLink;

        public GroupedLinkEx(){}

        public GroupedLinkEx( UGrCellsEx UGCellsA, UGrCellsEx UGCellsB, int tfx, int type ){
            this.UGCellsA = UGCellsA; this.UGCellsB = UGCellsB; //this.tfx=tfx;
            this.type=type;

            FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
        }
        public GroupedLinkEx(UGrCellsEx UGCellsA, UGrCellsEx UGCellsB, int type ){
            this.UGCellsA=UGCellsA; this.UGCellsB=UGCellsB;
            this.type=type;
        }

        public GroupedLinkEx( UCellLinkEx LK ){
            UCelLK=LK;
            UGCellsA=new UGrCellsEx(LK.tfx,LK.no,LK.UCe1);
            UGCellsB=new UGrCellsEx(LK.tfx,LK.no,LK.UCe2);
            this.type=LK.type;
            this.tfx =LK.tfx;

            FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
        }

		public GroupedLinkEx( UCellEx UC, int no1, int no2, int type, bool rootF=false ){
			this.rootF=rootF;
			int F = (1<<no1) | (1<<no2);
			// if( no1==no2 || (UC.FreeB&(1<<no1))==0 || (UC.FreeB&(1<<no2))==0 ){
			if( (UC.FreeB&(1<<no1))==0 || (UC.FreeB&(1<<no2))==0 ){
				UGCellsA=UGCellsB=null;
				return;
			}
			
			UGCellsA=new UGrCellsEx(-1,no1,UC);
			UGCellsB=new UGrCellsEx(-1,no2,UC);
			UCelLK=null;
			this.type= (UC.FreeBC==2)? type: W;

			FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
		}

        public override bool Equals( object obj ){
            GroupedLinkEx Q = obj as GroupedLinkEx;
            if( Q==null )  return true;
            if( this.type!=Q.type )  return false;
//            if( this.tfx!=Q.tfx )    return false;
            if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
            if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
            return true;
        }
		public bool EqualNull(){ return (tfx==-1); }

        public int CompareTo( object obj ){
            GroupedLinkEx Q = obj as GroupedLinkEx;
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
                if( this is ALSExLinkEx ){
                    ALSExLinkEx A=this as ALSExLinkEx;
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