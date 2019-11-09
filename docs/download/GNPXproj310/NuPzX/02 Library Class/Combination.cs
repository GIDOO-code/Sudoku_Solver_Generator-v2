using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace GNPZ_sdk{
    /*  for test    
    for(int k=0; k<5; k++ ){
        Combination cmb = new Combination(6,4);
        WriteLine($"\r ===== k={k}" );
        while( cmb.Successor(k) ){
            WriteLine(cmb);
        }
        Console.ReadKey();
    }
    */

    public class Combination{
        public readonly int N;
        public readonly int R;
        public int[] Index=null;
        private bool First=false;
        public Combination( int N, int R ){
            this.N=N;
            this.R=R;
            if(R>0 && R<=N){
                Index=new int[R];
                Index[0]=0;
                for(int m=1; m<R; m++) Index[m]=Index[m-1]+1;
                First=true;
            }
        }
        public bool Successor(){
            if(N<=0) return false;
            if(First){ First=false; }
            else{
                int m=R-1;
                while(m>=0 && Index[m]==(N-R+m)) m--;
                if(m<0){ Index=null; return false; }
                Index[m]++;
                for(int k=m+1; k<R; k++) Index[k]=Index[k-1]+1;
            }
            return true;
        }
 
        public bool Successor(int skip=int.MaxValue){
            if(N<=0) return false;
            if(First){ First=false; return (Index!=null); }
 
            int k;
            if(Index[0]==N-R) return false;
            if(skip<R-1){
                for(k=skip; k>=0; k--){ if(Index[k]<=N-R) break; }
                if(k<0)  return false;
            }
            else{
                for(k=R-1; k>0 && Index[k]==N-R+k; --k);
            }

            ++Index[k]; 
            for(int j=k; j<R-1; ++j)  Index[j+1]=Index[j]+1;
            return true;
        }

        public override string ToString(){
            string st="";
            Array.ForEach(Index, p=>{ st+=(" "+p);} );
            return st;
        }
    }
}
