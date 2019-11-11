using System;
using System.Collections.Generic;
using System.Linq;

namespace GNPZ_sdk{
    /*
            //permutation test

            Console.WriteLine( "* Permutation(4) *" );
            Permutation perm = new Permutation(4);
            while(perm.Successor()){
                Console.WriteLine( perm );
            }

            Console.WriteLine( "* Permutation(4,2) *" );
            perm = new Permutation(4,2);
            while(perm.Successor() ){
                Console.WriteLine( perm );
            }

            Console.WriteLine( "* Permutation(4,3) *" );
            perm = new Permutation(4,3);
            while(perm.Successor(1) ){
                Console.WriteLine( perm );
            }
    */
    public class Permutation{
        private int   N=0;
        private int   R=0;
        private int[] Pwrk=null;
        public  int[] Pnum=null;
        private bool  First;
 
        public Permutation( int N, int R=-1 ){
            this.N=N;
            this.R=R;
            if( R<=0 || R>N ) this.R=N;
            if( N>0 ){
                Pwrk = Enumerable.Range(0,N).ToArray();
                Pnum = Enumerable.Range(0,this.R).ToArray();
            }
            First=true; //(The first permutation has already been created in the constructor)
        }

        public bool Successor( int rx=-1 ){
            if( First || Pwrk==null ){ First=false; return (Pwrk!=null); }
            int r = (rx>=0)? rx: R-1;
            if( r>N-1 ) r=N-1;
        
            do{
                if( r<0 )  break;
                int A=Pwrk[r];
    
              L_1: 
                if( A>=N-1 ){ r--; continue; }
                A++;
                for( int k=0; k<r; k++ ){ if( Pwrk[k]==A ) goto L_1; }        
                Pwrk[r]=A;　　　//The next update position (r) and the number(A)
                if( r<N-1 ){           
                    int[] wx = Enumerable.Range(0,N).ToArray();
                    for( int k=0; k<=r; k++ )   wx[Pwrk[k]]=-1; //Exclude used digits
                    int n=0;
                    for( int k=r+1; k<N; k++ ){　// Fill the number after the change position
                        for( ; n<N; n++ ){
                            if( wx[n]<0 ) continue;
                            Pwrk[k]=wx[n++];
                            break;
                        }
                    }
                }
                for( int k=0; k<R; ++k ) Pnum[k]=Pwrk[k];       //(Copy to external reference array)
                return true;
            }while(true);
            return false;
        }

        public override string ToString(){
            string st="";  Array.ForEach( Pnum, p=> st+=(" "+p) );
            st += "  ";    Array.ForEach( Pwrk, p=> st+=(" "+p) );
            return st;
        }
    }
}
