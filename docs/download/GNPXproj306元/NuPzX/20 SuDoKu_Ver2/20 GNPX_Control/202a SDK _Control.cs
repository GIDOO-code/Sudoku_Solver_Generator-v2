using System;
using System.Collections.Generic;
using static System.Math;
using static System.Console;

namespace GNPZ_sdk{
    public partial class SDK_Ctrl{
        private const int ParaSolsNo=65536;
        public LatinSqureGen LSP;
        private int s1=0, s2=0;
        static public int rxCTRL=-1;      //=0:

        private bool __ret000=false;
        private bool __ret001=false;
#if false

      #region Latin Square
        public int[] GenSolPatternsList( bool RandF, int GenLSTyp, bool DispB=false ){
            bool retB=true; 
            switch(GenLSTyp){
                case 0: retB=GenerateLatinSqure0(ref rxCTRL,Sol99); break;
                case 1: retB=GenerateLatinSqure1(ref rxCTRL,Sol99); break;
                case 2: retB=GenerateLatinSqure2(ref rxCTRL,Sol99); break;
            }
            if(GenLSTyp==2 && !retB)  return null;

            int[] SolX = new int[81];
            for( int rc=0; rc<81; rc++ ) SolX[rc] = Sol99[rc/9,rc%9];
            if( RandF ) _DspNumRandmize(SolX); //Randamize
            _ApplyPattern(SolX);

            if(DispB)  __DBUGprint2(Sol99, "GenerateLatinSqureX ");
            return SolX;
        }      
        public List<int[]> GenSolPatternsList( bool RandF, int GenLSTyp, int SolNo ){  //for Parallel
            bool retB=true; 
            var ASDKList = new List<int[]>();
            int nc = Max( SolNo, ParaSolsNo );
            for( int k=0; k<nc; k++ ){
                switch(GenLSTyp){
                    case 0: retB=GenerateLatinSqure0(ref rxCTRL,Sol99); break;
                    case 1: retB=GenerateLatinSqure1(ref rxCTRL,Sol99); break;
                    case 2: retB=GenerateLatinSqure2(ref rxCTRL,Sol99); break;
                }
                if(!retB) break;

                int[] SolX = new int[81];
                Sol99.CopyTo(SolX,0);
                if( RandF ) _DspNumRandmize(SolX); //Randamize
                _ApplyPattern(SolX);//
                ASDKList.Add(SolX);
            }
            return ASDKList;
        }

        private Permutation[] prmLst=new Permutation[9];

        private int[,] PatOn = new int[9,7];
        public bool GenerateLatinSqure0( ref int RX, int[,] LS ){
            try{
                if( RX<3 ){
                    PatternCC++;
                    LSP.GeneratePara( ref Sol99, s1, s2 );
                    URow=new int[9]; UCol=new int[9];
                    for( int r=0; r<3; r++ ){
                        for( int c=3; c<9; c++ ){
                            UCol[c] |= (1<<LS[r,c]);
                            URow[c] |= (1<<LS[c,r]); //r,c
                        }
                    }
                    RX=3; prmLst[RX] = null;
                }

                do{
                  LNxtLevel:
                    Permutation prm=prmLst[RX];
                    if( prm==null ) prmLst[RX]=prm=new Permutation(9,6);
                
                    int[] UCo2 = new int[9];
                    int[] UBlk = new int[9];
                    for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                    for( int r=3; r<RX; r++ ){
                        for( int c=3; c<9; c++ ){
                            int no=LS[r,c];
                            UCo2[c] |= (1<<no);
                            UBlk[r/3*3+c/3] |= (1<<no);
                        }
                    }

                    int nxtX=9;
                    while( prm.Successor(nxtX) ){
                        for( int cx=3; cx<9; cx++ ){
                            nxtX=cx-3;
                            int no=prm.Index[nxtX]+1;
                            int noB = 1<<no;
                            if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;
                            if( (URow[RX]&noB)>0 ) goto LNxtPrm;
                            if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm;
                            LS[RX,cx] = no;
                        }
                        if(true){//RX==8 ){ //***********************
                            if( _DEBUGmode_ )  __DBUGprint2(LS, "    ");
                            if(RX==8) return true; //****************
                        }
                        prmLst[++RX]=null;
                        goto LNxtLevel;

                      LNxtPrm:
                        continue;
                    }
                }while((--RX)>=3);
            }
            catch(Exception e){ WriteLine(e.Message+"\r"+e.StackTrace); }

            return false;
        }
        public bool GenerateLatinSqure1( ref int RX, int[,] LS ){
            //Fluctuation part generation for exposure numerals
            if( RX<3 ){
                PatternCC++;
                LSP.GeneratePara( ref Sol99, s1, s2 );
                URow=new int[9]; UCol=new int[9];
                for( int r=0; r<3; r++ ){
                    for( int c=3; c<9; c++ ){
                        UCol[c] |= (1<<LS[r,c]);
                        URow[c] |= (1<<LS[c,r]); //r,c:
                    }
                }
                RX=3; prmLst[RX]=null;

                for( int r=3; r<9; r++ ){
                    int nc=0;
                    for( int c=3; c<9; c++ ){
                        if( PatGen.GPat[r,c]>0 ) PatOn[r,nc++]=c;
                    }
                    PatOn[r,6]=nc;

                //    Write($"\r##  {r}:");
                //    for( int c=3; c<7; c++ )  Console.Write(" "+PatOn[r,c] );
                }
            }

            if( RX==8 )  while(PatOn[RX,6]<=0) RX--;//variable portion is one line blank

            do{
              LNxtLevel:
                Permutation prm=prmLst[RX];
                if( prm==null ) prmLst[RX]=prm=new Permutation(9,PatOn[RX,6]);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=LS[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }
                int nc=PatOn[RX,6];
                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    for( int cx2=0; cx2<nc; cx2++ ){
                        nxtX=cx2;
                        int cx=PatOn[RX,cx2];
                        int no=prm.Index[nxtX]+1;
                        int noB = 1<<no;
                        if((UCo2[cx]&noB)>0) goto LNxtPrm;
                        if((URow[RX]&noB)>0) goto LNxtPrm;
                        if((UBlk[RX/3*3+cx/3]&noB)>0) goto LNxtPrm;
                        LS[RX,cx] = no;
                    }
                    if(true){ //if(RX==8){
                        if(_DEBUGmode_)  __DBUGprint2(LS, "    ");
                        if(RX==8) return true;//
                    }
                    prmLst[++RX]=null;
                    goto LNxtLevel;

                  LNxtPrm:
                    continue;
                }
                while( (--RX)>=3 && PatOn[RX,6]<=0 );
            }while(RX>=3);

            return false;
        }
      #endregion
#endif
        private void __DBUGprint2_Ans( List<UCell> UQ, string st="" ){
            string po;
            WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int wk=UQ[r*9+c].No;
                    if(wk==0) po += " .";
                    else po += wk.ToString().PadLeft(2);
                }
                WriteLine(po);
            }
        }

        private List<_LSpattern> LSlst=null;
        private int LSUcc;
        private int __ca=0, __cb=0;
        public int[] GenSolPatternsListA( bool RandF,  int GenLSTyp ){  //for Parallel
#if DEBUG            
            if(__ret000){
                if(LSlst[LSUcc-1].cnt>=2 ){
                    WriteLine("LSUcc="+LSUcc+ " cnt="+LSlst[LSUcc-1].cnt);

                    (LSlst[LSUcc-1].Sol99lst).ForEach(P=>{__DBUGprint2(P, "          Sol99 "); });
                }
                int __unmatch=0;
                var QQ=pGNPX_Eng.pGP.BDL;
                foreach( var Q in LSlst[LSUcc-1].Sol99lst ){
                    for( int k=0; k<81; k++ ){
                        if( Q[k/9,k%9] != Abs(QQ[k].No) ){ __unmatch++;  break; }
                    }
                }
                if( __unmatch==LSlst[LSUcc-1].Sol99lst.Count ){
                    (LSlst[LSUcc-1].Sol99lst).ForEach(P=>{__DBUGprint2(P, "different Sol99 "); });
                    __DBUGprint2_Ans(QQ, "Sol99 ");
                }
                __ca++;
                if(__ret001) __cb++;
                __ret000=false; __ret001=false;
            }
#endif            

            if( rxCTRL<=0 || LSlst==null || LSUcc>=LSlst.Count ){
                rxCTRL=1;
                LSlst=new List<_LSpattern>();
                
                do{
                    int __cc=0;
                    foreach( var P in GenerateLatinSqure0A(RandF:RandF) ){
                        __cc++;
                        var Q=LSlst.Find(x=>(x.has==P.has));
                        if(Q==null){ 
                            LSlst.Add(P); Q=P;
                            Q.Sol99lst=new List<int[,]>();
                        }
                        int[,] S99=new int[9,9];
                        for(int k=0; k<81; k++ ) S99[k/9,k%9] = P.Sol99[k/9,k%9];
                        Q.Sol99lst.Add(S99);
                        Q.cnt++;
                    }
                    LSUcc=0;
#if DEBUG
                    if(__ca>0)  WriteLine(" solution:"+__cb+"/"+__ca);
                    else        WriteLine();
                    
                    double per = LSlst.Count*100.0/__cc;
                    Write( "========== LSpattern = "+ LSlst.Count+"/"+__cc + "("+per.ToString("0.00")+"%)" );
#endif
                    if(GenLS_turbo) LSlst = LSlst.FindAll(p=>(p.cnt==1));
#if DEBUG
                    if(GenLS_turbo){
                        per = LSlst.Count*100.0/__cc;
                        Write( "  =>(turbo) "+ LSlst.Count+"/"+__cc + "("+per.ToString("0.00")+"%)" );
                    }
#endif
                }while(LSlst.Count<=0);
                 __ca=0; __cb=0;
            }

            return LSlst[LSUcc++].SolX;
        }

        private void _DspNumRandmize( int[] P ){
            List<int> ranNum = new List<int>();
            for( int r=0; r<9; r++ )  ranNum.Add( rnd.Next(0,9)*10 + r );
            ranNum.Sort( (x,y) => (x-y) );
            for( int r=0; r<9; r++) ranNum[r] %= 10;

            for( int rc=0; rc<81; rc++ ){
                int n=P[rc];
                if( n>0 ) P[rc] = ranNum[n-1]+1;
            }
        } 
        public IEnumerable<_LSpattern> GenerateLatinSqure0A( bool RandF){
            int RX=-1;
            int[,] Sol99=new int[9,9];
            List<uint> unique=new List<uint>();
            Permutation[] prmLstA=new Permutation[9];
            int[] URow=new int[9];
            int[] UCol=new int[9];

            PatternCC++;
            LSP.GeneratePara( ref Sol99, s1, s2 );
            for( int r=0; r<3; r++ ){
                for( int c=3; c<9; c++ ){
                    UCol[c] |= (1<<Sol99[r,c]);
                    URow[c] |= (1<<Sol99[c,r]); //r,c
                }
            }
            for( int r=0; r<9; r++ ){
                for( int c=0; c<9; c++ ){
                    if(r<3 || c<3 ) NuPz_Win.Sol99sta[r,c]=Sol99[r,c];
                }
            }
            RX=3; prmLstA[RX] = null;

            do{
                LNxtLevel:
                Permutation prm=prmLstA[RX];
                if( prm==null ) prmLstA[RX]=prm=new Permutation(9,6);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=Sol99[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }

                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    for( int cx=3; cx<9; cx++ ){
                        nxtX=cx-3;
                        int no=prm.Index[nxtX]+1;
                        int noB = 1<<no;
                        if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;
                        if( (URow[RX]&noB)>0 ) goto LNxtPrm;
                        if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm;
                        Sol99[RX,cx] = no;
                    }
                    if(RX<8){
                        prmLstA[++RX]=null;
                        goto LNxtLevel;
                    }
                    else{
                        int[] SolX = new int[81];
                        for(int k=0; k<81; k++) SolX[k]=Sol99[k/9,k%9];
                        
                        _ApplyPattern(SolX);//
                        uint q=0;
                        for( int k=0; k<81; k++ ){ if((SolX[k])>0) q = (q*3)^(uint)(k*719+(SolX[k])*7); }

                     //   if( _DEBUGmode_ )  __DBUGprint2(SolX, false, "GenerateLatinSqure0A");
                     //   __DBUGprint2(Sol99, "Sol99 "+q.ToString()+" ");

                        if( RandF ) _DspNumRandmize(SolX); //Randamize
                        yield return (new _LSpattern(q,SolX,Sol99));
                    }

                  LNxtPrm:
                    continue;
                }
            }while((--RX)>=3);

            yield break;
        }

        public class _LSpattern{
            public uint   has;
            public int[]  SolX;
            public int[,] Sol99;
            public List<int[,]> Sol99lst;
            public int    cnt=0;
            public _LSpattern(uint has, int[] SolX, int[,] Sol99 ){
                this.has=has; this.SolX=SolX; this.Sol99=Sol99;
            }
        }

        public IEnumerable<int[]> GenerateLatinSqure0B( bool RandF){ //#### In development
            int RX=3, pn;
            int[,] Sol99=new int[9,9];
            List<int>[] PatSel = new List<int>[9];
            for(int r=3; r<9; r++ ) PatSel[r]=new List<int>();
            Permutation[] prmLstB=new Permutation[9];
            int[] URow=new int[9];
            int[] UCol=new int[9];

            PatternCC++;
            LSP.GeneratePara( ref Sol99, s1, s2 );
                        if(_DEBUGmode_) __DBUGprint2(Sol99, "LS_0B"); //
            _ApplyPattern(Sol99);
                        if(_DEBUGmode_) __DBUGprint2(Sol99, "LS_0B&Pat"); //

            for( int rc=0; rc<81; rc++ ){
                int r=rc/9, c=rc%9, p=Sol99[r,c];
                if(p>0){ UCol[c]=1<<p; URow[r]=1<<p; }
                if(r>=3 & c>=3 & PatGen.GPat[r,c]>0)  PatSel[r].Add(c);
            }
                        if(_DEBUGmode_){
                            for( int r=3; r<9; r++ ){
                                Write($"\r##  {r}:");
                                PatSel[r].ForEach(c=> Write(" "+c) );
                            }
                            WriteLine();
                        }
            do{
              LNxtLevel:
                while((pn=PatSel[RX].Count)<=0 && RX<8) RX++;
                if(RX==8 && pn==0){
                    int[] SolX = new int[81];
                    for(int k=0; k<81; k++) SolX[k]=Sol99[k/9,k%9];
                        //if( _DEBUGmode_ ) __DBUGprint2(SolX, false, "SuccLS0B");
                    if(RandF) _DspNumRandmize(SolX); //Randamize
                    yield return SolX; //****************
                    while((pn=PatSel[RX].Count)<=0 && RX>3) RX--;
                }

                Permutation prmB=prmLstB[RX] ?? new Permutation(9,pn);
                prmLstB[RX] = prmB;

                        if(_DEBUGmode_)  WriteLine("======= RX="+RX);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=Sol99[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }
                pn=PatSel[RX].Count;
                int nxtX=9;
                while( prmB.Successor(nxtX) ){
                        if(_DEBUGmode_){ Array.ForEach(prmB.Index,P=>Write(" "+P)); WriteLine(); }

                    for( int cx2=0; cx2<pn; cx2++ ){
                        nxtX=cx2;
                        int cxS=PatSel[RX][cx2];
                        int no=prmB.Index[cx2]+1;
                        int noB=1<<no;
                        if((UCo2[cxS]&noB)>0) goto LNxtPrm;
                        if((URow[RX]&noB)>0)  goto LNxtPrm;
                        if((UBlk[RX/3*3+cxS/3]&noB)>0) goto LNxtPrm;
                        Sol99[RX,cxS] = no;
                    }
                        if(_DEBUGmode_)  __DBUGprint2(Sol99, "SuccLS0B RX="+RX+" ");
                 
                    if(RX<8){ prmLstB[++RX]=null; goto LNxtLevel; }
                    
                    int[] SolX = new int[81];
                    for(int k=0; k<81; k++) SolX[k]=Sol99[k/9,k%9];

                        //if( _DEBUGmode_ ) __DBUGprint2(SolX, false, "SuccLS0B");

                    if(RandF) _DspNumRandmize(SolX); //Randamize
                    yield return SolX; //****************
                    for( int c=3; c<9; c++ ) Sol99[RX,c]=0;
                    
                  LNxtPrm:
                    continue;
                }
                do{
                    for( int c=3; c<9; c++ ) Sol99[RX,c]=0;
                }while( (--RX)>=3 && PatSel[RX].Count<=0 );
            }while(RX>=3);

            yield break;
        }

      #region inspection
        private LatSqrRow[] LSR;
        public bool GenerateLatinSqure2( ref int RX, int[,] LS ){
            if(RX<0 || LSR==null){
                LSR=new LatSqrRow[9];
                for( int k=0; k<9; k++ ) LSR[k]=new LatSqrRow(PatGen,k);
                RX=0;
                LSR[RX].SetPreInfo(null);
            }
            if(RX==8)  while(LSR[RX].nc<=0) RX--;//variable portion is one line blank
            do{
                int[] P=LSR[RX].Gen_LS_RowX( );
                if(P!=null){
                    for( int c=0; c<9; c++ ) LS[RX,c]=P[c];
                    if(RX==8) return true;
                    LSR[RX+1].SetPreInfo(LSR[RX++]);
                }
                else while(RX>0 && LSR[--RX].nc<=0);
            }while(RX>=0);
            return false;
        }   
        public void Force_NextSuccessor( int RX ){
            rxCTRL=RX-1;
            for( int r=RX; r<9; r++ ) LSR[r].firstB=true;

            List<UCell> BDL = GeneratePuzzleCandidate( );  //problem generation
            UProblem P = new UProblem(BDL);
            pGNPX_Eng.SetGP(P);
        }

        public class LatSqrRow{
            static int rowH7=7;    //7=1+2+4
            static int colH147=73; //73=1+8+64
            private patternGenerator PatGen;
            private int rowN;
            private LatSqrRow preLSR;
            private int[] rowH=new int[9];
            private int[] colX=new int[9];
            private int[] colH=new int[9];
            private int[] blkH=new int[9];
            private Permutation prm;
            public bool firstB ;
            public  int nc;

            public LatSqrRow( patternGenerator PatGen, int rowN ){
                this.PatGen=PatGen;
                this.rowN=rowN;            
                firstB=true;
            }
            public void SetPreInfo( LatSqrRow preLSR ){ this.preLSR=preLSR; }
            public void Force_NextSuccessor(){ prm.Successor(); }

            public int[] Gen_LS_RowX( ){
                if(firstB){
                    nc=0;
                    for(int c=0; c<9; c++) if(PatGen.GPat[rowN,c]>0) colX[nc++]=c;
                    prm=new Permutation(9,nc);
                    firstB=false;
                }
                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    if(nxtX>0){
                        if(preLSR==null){ for( int c=0; c<9; c++ ){ colH[c]=blkH[c]=0; } }
                        else{ for( int c=0; c<9; c++ ){ colH[c]=preLSR.colH[c]; blkH[c]=preLSR.blkH[c]; } }
                    }

                    for(int k=0; k<nc; k++){
                        nxtX=k;
                        int n=prm.Index[k];
                        int nb=1<<n;
                        int c=colX[k];

                        if(c<3){
                            if(rowN<3){ if(n!=(rowN*3+c) ) goto nxtPerm; }
                            else{ if( ((colH147<<c)&nb)>0 ) goto nxtPerm; }
                        }
                        else if(rowN<3){ if(((rowH7<<(rowN*3))&nb)>0) goto nxtPerm; }

                        if((colH[c]&nb)>0) goto nxtPerm;
                        int b=rowN/3*3+c/3;
                        if((blkH[b]&nb)>0) goto nxtPerm;

                        colH[c] |= nb;
                        blkH[b] |= nb;
                        rowH[c]=n+1;
                    }
                    return rowH;

                  nxtPerm:
                    continue;
                }
                firstB=true;
                return null;
            }
        }
      #endregion inspection
 
      #region Latin Squares ID code generation for Standadization
        public string Get_SDKNumPattern( int[] TrPara, int[] AnsNum ){

        //Standadization(Number)
            int[] ChgNum=new int[10];
            for( int k=0; k<9; k++ ){
                int n=Abs(AnsNum[(k/3*9)+(k%3)]);
                ChgNum[n]=k+1;
            }
            int[,] AnsN2=new int[9,9];
            for( int rc=0; rc<81; rc++ ){
                int n=Abs(AnsNum[rc]);
                AnsN2[rc/9,rc%9]=ChgNum[n];
            }
                      __DBUGprint2(AnsNum,true,"Before");
                      __DBUGprint2(AnsN2,"After");

        //Block 2347
            int[] PTop=new int[8];
            int[] PLft=new int[8];
            
            for( int s=0; s<8; s++ ) PTop[s]=-1;
            LSP._LatinSqureSub_01R( AnsN2, PTop );

            for( int s=0; s<8; s++ ) PLft[s]=-1;
            LSP._LatinSqureSub_11R( AnsN2, PLft );

                      string st="PTop";
                      Array.ForEach(PTop,P=>st+=" "+P);
                      WriteLine(st);
                      st="PLft";
                      Array.ForEach(PLft,P=>st+=" "+P);
                      WriteLine(st);

        //Block 5689
            int ID=LSP.GetLatSqrID(AnsN2);

        //ID
            int N=PTop[0]*10+PTop[1];
            for( int n=2; n<8; n++ ) N=(N*10)+PTop[n];
            TrPara[12]=N;   //14:Block 23

            N=PLft[0]*10+PLft[1];
            for( int n=2; n<8; n++ ) N=(N*10)+PLft[n];
            TrPara[13]=N;   //15:Block 47

            TrPara[14]=ID;  //16:Block 5689

            N=0;
            for( int n=0; n<10; n++ ) N=(N*10)+ChgNum[n];           
            TrPara[15]=N;   //13:Exchange
                      st="ID";
                      Array.ForEach(TrPara,P=>st+=" "+P);
                      WriteLine(st);
        //SuDoKu Standadization Code
            string po="===== Standadization Code=====";
            po +="\rPattern Code:\r";
            for( int k=9; k<12; k++ ) po+=" "+TrPara[k];

            po+="\r\rLatin Square Code\r";
            for( int k=12; k<15; k++ ) po+=" "+TrPara[k];

            po+="\r\r===== Transformation Parameter=====\rPattern:";
            po+=" "; for( int k=0; k<4; k++ ) po+=TrPara[k];
            po+=" "; for( int k=4; k<8; k++ ) po+=TrPara[k];
            po+=" "+TrPara[8];

            po+="\rNumber:";
            po+=TrPara[16].ToString()+" -> 123456789";

            return po;
        }
      #endregion
    }
}
