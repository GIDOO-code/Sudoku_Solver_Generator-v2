using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UGrCells: List<UCell>, IComparable{
        public readonly int      tfx;  //house No.
        public readonly int      no;   
        public Bit81             B81;  //Bit representation of cell position
        public int FreeB{
            get{
                int FreeB=0;
                this.ForEach(P=>FreeB|=P.FreeB);
                return FreeB;
            }
        }

        public UGrCells( UCell X, int no ){
            this.tfx=-9; this.no=no;
            B81=new Bit81();
            this.Add(X);
        }
        public UGrCells( int tfx, int no ){
            this.tfx=tfx; this.no=no;
            B81=new Bit81();
        }
        public UGrCells( int tfx, int no, UCell Q ): this(tfx,no){
            Add(Q);
        }
        public UGrCells Copy(){
            UGrCells Pcpy=new UGrCells(tfx,no);
            Pcpy.B81=B81.Copy();
            return Pcpy;
        }

        public new void Add( UCell P ){ base.Add(P); B81.BPSet(P.rc); }
        public void Add( List<UCell> PL ){ PL.ForEach(P=>Add(P)); }

        public override bool Equals( object obj ){
            UGrCells Q=obj as UGrCells;
            if( Q==null ) return false;

            if( no!=Q.no ) return false;
            if( Count!=Q.Count ) return false;
            for( int k=0; k<Count; k++ ){
                if( this[k].rc!=Q[k].rc ) return false;
            }
            return true;
        }

        public bool EqualsRC( object obj ){
            UGrCells Q=obj as UGrCells;
            if( Q==null ) return false;

            if( Count!=Q.Count ) return false;
            for( int k=0; k<Count; k++ ){
                if( this[k].rc!=Q[k].rc ) return false;
            }
            return true;
        }
        public int CompareTo( object obj ){
            UGrCells Q = obj as UGrCells;
            if( this.no  !=Q.no   ) return (this.no-Q.no);
            if( this.Count !=Q.Count  ) return (this.Count-Q.Count);
            for( int k=0; k<Count; k++ ){
                if( this[k].rc !=Q[k].rc ) return (this[k].rc-Q[k].rc);
            }
            return 0;
        }
        public override int GetHashCode(){ return base.GetHashCode(); }

        public override string ToString(){
            string st="";
            if( Count<=0 ) st=".";
            else if( Count==1 ) st = this[0].rc.ToRCString();
            else{
                this.ForEach(P=>{st+=" "+P.rc.ToRCString();});
                st="<"+st.ToString_SameHouseComp()+">";
            }
            return st;
        }

        public string GCToString(){
            string st="";
            if( Count<=0 ) st=".";
            else{
                this.ForEach(P=>{st+=" "+P.rc.ToRCString();});
                st =  "no:"+no+" <"+st.ToString_SameHouseComp()+">";
            }
            return st;
        }
    }
}