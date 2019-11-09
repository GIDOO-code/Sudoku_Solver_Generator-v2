using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GNPZ_sdk{
    /*      
    for( int k=0; k<5; k++ ){
        Combination cmb = new Combination(6,4);
        Console.WriteLine("\r ===== k={0}",k );
        while( cmb.Successor(k) ){
            Console.WriteLine(cmb);
        }
        Console.ReadKey();
    }
    */

    public class Combination{
        public readonly int N;
        public readonly int R;
        public int[] Cmb=null;
        private bool First=false;
        public Combination( int N, int R ){
            this.N=N;
            this.R=R;
            if( R>0 && R<=N ){
                Cmb=new int[R];
                Cmb[0]=0;
                for( int m=1; m<R; m++ ) Cmb[m]=Cmb[m-1]+1;
                First=true;
            }
        }
        public bool Successor(){
            if( N<=0 ) return false;
            if( First ){ First=false; }
            else{
                int m=R-1;
                while( m>=0 && Cmb[m]==(N-R+m) ) m--;
                if( m<0 ){ Cmb=null; return false; }
                Cmb[m]++;
                for( int k=m+1; k<R; k++ ) Cmb[k]=Cmb[k-1]+1;
            }
            return true;
        }
 
        public bool Successor( int skip ){
            if( N<=0 ) return false;
            if( First ){ First=false; return (Cmb!=null); }
 
            int k;
            if( Cmb[0]==N-R ) return false;
            if( skip<R-1 ){
                for( k=skip; k>=0; k-- ){ if( Cmb[k]<=N-R ) break; }
                if( k<0 )  return false;
            }
            else{
                for( k=R-1; k>0 && Cmb[k]==N-R+k; --k );
            }

            ++Cmb[k]; 
            for( int j=k; j<R-1; ++j )  Cmb[j+1]=Cmb[j]+1;
            return true;
        }

        public override string ToString(){
            string st="";
            Array.ForEach( Cmb, p=>{ st+=(" "+p);} );
            return st;
        }
    }
}
