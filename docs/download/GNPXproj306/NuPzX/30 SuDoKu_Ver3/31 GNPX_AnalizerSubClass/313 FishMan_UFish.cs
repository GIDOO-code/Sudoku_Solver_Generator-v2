using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
    public class FishManEx{
        private List<UCellEx>   pBDL;             
        private Bit81[]       pHouseCells;
        private Bit81[]       pConnectedCells; 

        private int           sz;
        private int           no;

        private List<Bit81F>  HBLst=new List<Bit81F>();
        private bool          extFlag;

        public FishManEx( AnalyzerBaseV3 AnB, int FMSize, int no, int sz, bool extFlag=false ){

            this.pBDL = AnB.pBDL;
            this.pHouseCells = AnalyzerBaseV3.HouseCells;
            this.pConnectedCells = AnalyzerBaseV3.ConnectedCells;
//            Bit81F.pConnectedCells = AnalyzerBaseV3.ConnectedCells;
            this.extFlag=extFlag;                     
            this.no=no; this.sz=sz;
            int noB=(1<<no);

            Bit81 Q, BPnoB=new Bit81(pBDL,noB);
          //debugPrint(pBDL," sz:"+sz+" no:"+(no+1)+"  "); //########
            for( int tfx=0; tfx<FMSize; tfx++ ){ 
                Q = pHouseCells[tfx]&BPnoB;
                if( !Q.IsZero() && !HBLst.Contains(Q) ) HBLst.Add(new Bit81F(Q,ID:tfx));
//              if( !Q.IsZero() && !HBLst2.Contains(Q) ){ Q.ID=tfx; HBLst2.Add(new Bit81F(Q)); }
            }
            if( HBLst.Count<sz*2 ){ HBLst=null; return; }
        }
        private void __debugPrint( List<UCellEx> BDL, string st ){
            string po;
            WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int nn = BDL[r*9+c].No;
                    if( nn==0 ) po+= "  .";
                    else po += nn.ToString("##0").PadLeft(3);
                }
                WriteLine(po);
            }
        }

        private class Bit81F: Bit81{
            public int rcbB=0;
            public Bit81F( Bit81 rcB, int ID ): base(rcB){
                int rr=0, cc=0, bb=0;
                this.ID=ID;
                foreach(var rc in rcB.IEGet_rc()){
                    rr|=(1<<(rc/9)); cc|=(1<<(rc%9)); //bb|=(1<<(rc/27*3+(rc%9)/3));
                }
                rcbB = (rr) | (cc<<9) | (bb<<18);
              //WriteLine( rcB.ToRCString()+" "+rcbB.ToBitString27() );
            }
        }

        public IEnumerable<UFishEx> IEGet_BaseSetEx( int BaseSel, bool FinnedF=false ){
            if(HBLst==null)  yield break;

            GeneralLogicGen.ChkBas1=0;
            GeneralLogicGen.ChkBas2=0;
            Bit81 Q;
            
            bool basicFish=(BaseSel.BitCount()<=9) & !FinnedF;//not F/M & notF/M 
            int  BaseSelR = 0x3FFFF ^ BaseSel;
            var prmBas=new Permutation(HBLst.Count,sz);
            int nxt=int.MaxValue;
            while( prmBas.Successor(nxt) ){
                                    int chk1=++GeneralLogicGen.ChkBas1;
                int   usedLK=0;
                Bit81 HB81=new Bit81();
                Bit81 OHB81=new Bit81();
                int rcbB=0;
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=prmBas.Index[k];
                    Bit81F HBF=HBLst[nx];
                    if( ((1<<HBF.ID)&BaseSel)==0 )  goto nxtCmb;
                    if( !(Q=HB81&HBF).IsZero() )    goto nxtCmb; 
                    usedLK |= 1<<HBF.ID;  //house Number
                    HB81   |= HBF;        //Bit81
                    rcbB |= HBF.rcbB;
                    if(basicFish && k>0 && (rcbB&BaseSelR).BitCount()>sz ) goto nxtCmb; 
                }
                if( extFlag && !IsLinked9(HB81) )  continue;
                                    int chk2=++GeneralLogicGen.ChkBas2;

                UFishEx UF = new UFishEx(no,sz,usedLK,HB81);
                //if(sz>=3 && BaseSel==0x7FFFFFF) Debug_PattenPrint(UF);
                yield return UF;

              nxtCmb:
                continue;
            }
            yield break;
        }

        public IEnumerable<UFishEx> IEGet_BaseSet( int BaseSel ){
            if(HBLst==null)  yield break;

            GeneralLogicGen.ChkBas1=0;
            GeneralLogicGen.ChkBas2=0;
            Bit81 Q;
            Combination cmbBas=new Combination(HBLst.Count,sz);
            int nxt=int.MaxValue;
            while( cmbBas.Successor(nxt) ){
                                    int chk1=++GeneralLogicGen.ChkBas1;
                int   usedLK=0;
                Bit81 HB81=new Bit81();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=cmbBas.Index[k];
                    if( ((1<<HBLst[nx].ID)&BaseSel)==0 )  goto nxtCmb;
                    if( !(Q=HB81&HBLst[nx]).IsZero() )    goto nxtCmb; 
                    usedLK |= 1<<HBLst[nx].ID;  //house Number
                    HB81   |= HBLst[nx];        //Bit81
                }
                if( extFlag && !IsLinked9(HB81) )  continue;
                                    int chk2=++GeneralLogicGen.ChkBas2;

                UFishEx UF = new UFishEx(no,sz,usedLK,HB81);
                //if(sz>=3 && BaseSel==0x7FFFFFF) Debug_PattenPrint(UF);
                yield return UF;

              nxtCmb:
                continue;
            }
            yield break;
        }
        public bool IsLinked9( Bit81 HB81 ){
            Bit81 Colored=new Bit81(), Processed=new Bit81();
            int rc0 = HB81.FindFirstrc();
            Colored.BPSet(rc0);
            while(true){
                Bit81 T = Colored-Processed;
                if( (rc0=T.FindFirstrc())<0 ) break;

                Processed.BPSet(rc0);
                Colored |= HB81&pConnectedCells[rc0];
                if( HB81.IsHit(rc0) ) Colored.BPSet(rc0);
            }
            return (HB81-Colored).IsZero();
        }
        private void Debug_PattenPrint( UFishEx UF ){
            WriteLine("no="+no+ " sz="+sz +"  BaseSet: " + UF.HouseB.HouseToString() );
            Bit81 BPnoB=new Bit81(pBDL,1<<no);
            string noST=" "+no.ToString();
            for( int r=0; r<9; r++ ){
                string st="";
                BPnoB.GetRowList(r).ForEach(p=>st+=(p==0? " .": noST));
                st+=" ";
                UF.BaseB81.GetRowList(r).ForEach(p=>st+=(p==0? " .": " B"));
                st+=" ";
                (BPnoB-UF.BaseB81).GetRowList(r).ForEach(p=>st+=(p==0? " .": " X"));
                WriteLine(st);
            }
        }

        public IEnumerable<UFishEx> IEGet_CoverSet( UFishEx BSet, int CoverSel, bool Finned ){
            if(HBLst==null)  yield break;

            List<Bit81> HCLst=new List<Bit81>();
            foreach( var P in HBLst.Where(q=>(BSet.HouseB&(1<<q.ID))==0) ){
                if( ((1<<P.ID)&CoverSel)==0 )  continue;
                if( BSet.BaseB81.IsHit(P) )  HCLst.Add(P);
            }
            if(HCLst.Count<sz) yield break;

            Bit81 Q;
            Combination cmbCov=new Combination(HCLst.Count,sz);
            int nxt=int.MaxValue;
            while( cmbCov.Successor(nxt) ){
                int chk1=++GeneralLogicGen.ChkCov1;

                int   usedLK=0;
                Bit81 HC81=new Bit81();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=cmbCov.Index[k];                   
                    if( !(Q=HC81&HCLst[nx]).IsZero() )  goto nxtCmb;
                    usedLK |= 1<<HCLst[nx].ID;  //house Number
                    HC81   |= HCLst[nx];        //Bit81
                }

                Bit81 FinB81=BSet.BaseB81-HC81;
                if( Finned!=(FinB81.Count>0) ) continue;
                UFishEx UF = new UFishEx(BSet,usedLK,HC81,FinB81);
                //if(sz>=3 && CoverSel==0x7FFFFFF)  WriteLine("  CoverSet: " + UF.HouseC.HouseToString() ); //**********
                yield return UF;
                
              nxtCmb:
                continue;
            }
            yield break;
        }
    }

    public class UFishEx{
        public int      ID;
        public int      no;
        public int      sz;
        public Bit81    BaseB81=null;
        public int      HouseB=0;

        public UFishEx    BaseSet=null;
        public int      HouseC=0;
        public Bit81    CoverB81=null;
        public Bit81    FinB81=null;

        public UFishEx(){ }

        public UFishEx( int no, int sz, int HouseB, Bit81 BaseB81 ){
            this.no=no;
            this.sz=sz;
            this.HouseB =HouseB;
            this.BaseB81=BaseB81;
        }
          
        public UFishEx( UFishEx BaseSet, int HouseC, Bit81 CoverB81, Bit81 FinB81 ){
            this.BaseSet =BaseSet;
            this.HouseB  =BaseSet.HouseB;
            this.HouseC  =HouseC;
            this.CoverB81=CoverB81;
            this.FinB81  =FinB81;
        }
        public string ToString( string ttl ){
            string st = ttl + HouseB.HouseToString();
            return st;
        }
    }
}