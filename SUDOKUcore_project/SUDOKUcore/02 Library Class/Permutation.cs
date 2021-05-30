using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static System.Math;

namespace GNPXcore{
/*
    //permutation test
    Permutation perm = new Permutation(10,6);

    int xx = 5;
    for( ; perm!=null; perm=perm.Successor(xx) ){

        string st="";
        Array.ForEach( perm.Pwrk, x=> st+=(" "+x) );
        WriteLine( st );
    }
*/
    public class Permutation{
        protected readonly int N;
        protected readonly int R;
        private int[] Pwrk=null;
        public  int[] Index=null;
        private bool  First;
 
        public Permutation(int N,int R=-1){
            this.N=N;
            this.R=R;
            if(R<=0 || R>N) this.R=N;
            if(N>0){
                Pwrk = Enumerable.Range(0,N).ToArray();
                Index = Enumerable.Range(0,this.R).ToArray();
            }
            First=true;
        }
        public bool Successor(int nxtX=-1){
            if(N<=0) return false;
            if(First || Pwrk==null){ First=false; return (Pwrk!=null); }
            int r = (nxtX>=0)? nxtX: R-1;
            r = Min(r,R-1);
            
            do{
                if(r<0)  break;
                int A=Pwrk[r];
        
            L_1: 
                if(A>=N-1){ r--; continue; }
                A++;
                for(int nx=0; nx<r; nx++ ){ if(Pwrk[nx]==A) goto L_1; }  
                Pwrk[r]=A;
                if(r<N-1){
                    if(N<=64){
                        ulong bp=0;
                        for(int k=0; k<=r; k++) bp |= (1u<<Pwrk[k]);
                        r++;
                        for(int n=0; n<N; n++ ){
                            if((bp&(1u<<n))==0){
                                Pwrk[r++]=n;
                                if(r>=N) break;
                            }
                        }
                    }
                    else{
                        int[] wx = Enumerable.Range(0,N).ToArray();
                        for(int k=0; k<=r; k++) wx[Pwrk[k]]=-1;

                        int s=0;
                        for(int k=r+1; k<N; k++){
                            for(; s<N; s++){
                                if(wx[s]<0) continue;
                                Pwrk[k]=wx[s++];
                                break;
                            }
                        }
                    }
                }
                for(int k=0; k<R; ++k) Index[k]=Pwrk[k];
                return true;
            }while(true);
            return false;
        }
        public override string ToString(){
            string st=""; Array.ForEach(Index, p=> st+=(" "+p));
            st += "  ";   Array.ForEach(Pwrk, p=> st+=(" "+p));
            return st;
        }
    }
}