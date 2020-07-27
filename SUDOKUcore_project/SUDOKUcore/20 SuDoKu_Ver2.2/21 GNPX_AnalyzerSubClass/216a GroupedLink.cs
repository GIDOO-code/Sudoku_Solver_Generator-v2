using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

namespace GNPXcore {
    public class GroupedLink: IComparable, IEquatable<GroupedLink> {
        static public bool _IDsetB=true;
//        private const int S=1, W=2;  
        static public int   _ID0;
        public int          ID;
        public bool         AvailableF=true;
        public UCellLink    UCelLK=null;
        public int          GenNo;
//      public int          UGLSize{ get{ return UGCellsA.Size+UGCellsB.Size; } }


        public UGrCells     UGCellsA;
        public UGrCells     UGCellsB;
        public int no{  get{ return UGCellsA.no; } }
        public int no2{ get{ return UGCellsB.no; } }

		public bool			rootF; 
        public int          type;
        public int          tfx=-1;         //house number(-1:in-cell Link)
        public readonly int FreeB;          //element number of UGCells
        public bool         LoopFlag;       //last link of loop
        public Bit81        UsedCs=new Bit81();
        public object       preGrpedLink=null;

        public GroupedLink(){
             ID=_ID0++;   
        }

        public GroupedLink( UGrCells UGCellsA, UGrCells UGCellsB, int tfx, int type ): this(){
            this.UGCellsA = UGCellsA; this.UGCellsB = UGCellsB; //this.tfx=tfx;
            this.type=type;

            FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
        }
        public GroupedLink(UGrCells UGCellsA, UGrCells UGCellsB, int type ): this(){
            this.UGCellsA=UGCellsA; this.UGCellsB=UGCellsB; this.type=type;
        }

        public GroupedLink( UCellLink LK ): this(){
            UCelLK=LK;
            UGCellsA=new UGrCells(LK.tfx,LK.no,LK.UCe1);
            UGCellsB=new UGrCells(LK.tfx,LK.no,LK.UCe2);
            this.type=LK.type;
            this.tfx =LK.tfx;

            FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
        }

		public GroupedLink( UCell UC, int no1, int no2, int type, bool rootF=false ): this(){
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
			this.type= (UC.FreeBC==2)? type: 2; //2:WeakLink

			FreeB = UGCellsA.Aggregate(0,(Q,P)=>Q|P.FreeB);
            FreeB = UGCellsB.Aggregate(FreeB,(Q,P)=>Q|P.FreeB);
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
                string st= $"tfx:{tfx.ToString().PadLeft(2)} no:{no} type:{type} ";
                st += UGCellsA.ToString()+ " -> "+UGCellsB.ToString();
#if DEBUG
                if(_IDsetB) st += $"ID:{ID} {st}";
#endif
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
                string st="[";

#if DEBUG
                if(_IDsetB) st += "ID:"+ID+" ";
#endif
                st+=(type==1? "S": "W")+" ";
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

        public override bool Equals(object obj) {
            return Equals(obj as GroupedLink);
        }

        public bool Equals(GroupedLink other) {
            return other != null &&
                   EqualityComparer<UCellLink>.Default.Equals(UCelLK, other.UCelLK) &&
                   EqualityComparer<UGrCells>.Default.Equals(UGCellsA, other.UGCellsA) &&
                   EqualityComparer<UGrCells>.Default.Equals(UGCellsB, other.UGCellsB) &&
                   no == other.no &&
                   no2 == other.no2 &&
                   type == other.type &&
                   FreeB == other.FreeB;
        }

        public override int GetHashCode() {
            var hashCode = 1413630847;
            hashCode = hashCode * -1521134295 + EqualityComparer<UCellLink>.Default.GetHashCode(UCelLK);
            hashCode = hashCode * -1521134295 + EqualityComparer<UGrCells>.Default.GetHashCode(UGCellsA);
            hashCode = hashCode * -1521134295 + EqualityComparer<UGrCells>.Default.GetHashCode(UGCellsB);
            hashCode = hashCode * -1521134295 + no.GetHashCode();
            hashCode = hashCode * -1521134295 + no2.GetHashCode();
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + FreeB.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GroupedLink left, GroupedLink right) {
            return EqualityComparer<GroupedLink>.Default.Equals(left, right);
        }

        public static bool operator !=(GroupedLink left, GroupedLink right) {
            return !(left == right);
        }
        /*
       public override int GetHashCode(){ return base.GetHashCode(); }
       public override bool Equals( object obj ){
           GroupedLink Q = obj as GroupedLink;
           if( Q==null )  return true;
           if( this.type!=Q.type )  return false;
//            if( this.tfx!=Q.tfx )    return false;
           if( !this.UGCellsA.Equals(Q.UGCellsA) ) return false;
           if( !this.UGCellsB.Equals(Q.UGCellsB) ) return false;
           return true;
       }
*/
    }
}