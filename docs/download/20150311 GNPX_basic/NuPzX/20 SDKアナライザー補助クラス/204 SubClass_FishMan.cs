using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk {
    public partial class GNPZ_Analyzer{
        public class FishMan{
            private GNPZ_Analyzer pSA;
            private List<UCell>   pBDL;             
            private Bit81[]       pHouseCells;

            private int           sz;
            private int           no;
            public  List<UFish>   Fishs;
            public  Bit81         BPnoB;
            private List<Bit81>   HBL = new List<Bit81>();
            public  List<FishCell> FishCellLst=null;

            public FishMan( GNPZ_Analyzer pSA, int FMSize, int no, int sz ){
                this.pSA  = pSA;
                this.pBDL = pSA.pBDL;
                this.pHouseCells = pSA.HouseCells;
                UFish.pSA = pSA;
                
                this.no = no;
                this.sz = sz;
                int noB=(1<<no);
                BPnoB=new Bit81(pBDL,noB);

                for( int tfx=0; tfx<FMSize; tfx++ ){ 
                    Bit81 Q = pHouseCells[tfx]&BPnoB;
                    if( !Q.IsZero() && !HBL.Contains(Q) ){ Q.ID=tfx; HBL.Add(Q); }
                }

            }

            public IEnumerable<UFish> IEGet_BaseSet( int BaseSel, bool EndoFlg ){ //EndoF==true:EndoFin許容
                if( HBL.Count<sz*2 )  yield break;
                Combination cmbBas=new Combination(HBL.Count,sz);
                int nxtB=99;
                Bit81 Q;
                Bit81 EndoFin=new Bit81();
                while( cmbBas.Successor(nxtB) ){
                    Bit81 PBase=new Bit81();
                    int   HBase=0, EndoCC=0;
                    for( int k=0; k<sz; k++ ){
                        nxtB=k;
                        int nx=cmbBas.Cmb[k];
                        if( ((1<<HBL[nx].ID)&BaseSel) ==0 )  goto LNxtBase;
                        if( !(Q=PBase&HBL[nx]).IsZero() ){  //BaseSetに重なりあり
                            if( !EndoFlg ) goto LNxtBase;
                            EndoFin|=Q;
                            EndoCC++;
                        }
                        PBase |= HBL[nx];               //BaseSetセル
                        HBase |= 1<<HBL[nx].ID;         //BaseSet house番号
                    }
                    if( EndoFlg && EndoCC==0 ) goto LNxtBase; //Endo Fin は複数

                    UFish UF = new UFish(no,sz,HBase,PBase,EndoFin);
                    yield return UF;

                LNxtBase: 
                    if(EndoCC>0) EndoFin=new Bit81();
                    continue;
                }
                yield break;
            }        
            public IEnumerable<UFish> IEGet_CoverSet( UFish BSet, int CoverSel, bool CannFlg ){
                Bit81 CannFin=new Bit81();  //Cannibalistic Fin
                Combination cmbCov=new Combination(HBL.Count,sz);
                int nxtC=99;
                while( cmbCov.Successor(nxtC) ){
                    Bit81 PCover=new Bit81();
                    int   HCover=0;
                
                    Bit81 Q;
                    int CannCC=0;
                    for( int k=0; k<sz; k++ ){
                        nxtC=k;
                        int nx=cmbCov.Cmb[k];
                        if( ((1<<HBL[nx].ID)&CoverSel) ==0 )    goto LNxtCover;//行列ブロック条件
                        if( (BSet.HouseB&(1<<HBL[nx].ID))>0 )   goto LNxtCover;//BaseSetで使用済み

                        if( (BSet.BaseB81&HBL[nx]).IsZero() )   goto LNxtCover;//BaseSetをカバーしない

                        if( !(Q=PCover&HBL[nx]&BSet.BaseB81).IsZero() ){  //CoverSetに重なりあり
                            if( !CannFlg ) goto LNxtCover;
                            CannFin|=Q;                         //CannibalisticCells
                            CannCC++;
                        }
                        PCover |= HBL[nx];                      //CoverSetセル
                        HCover |= 1<<HBL[nx].ID;                //CoverSet house番号
                    }
                    if( CannFlg && CannFin.IsZero() )  goto LNxtCover;

                    Bit81 FinB81=BSet.BaseB81-PCover;
                    UFish UF = new UFish(BSet,HCover,PCover,FinB81,CannFin);
                    yield return UF;

                LNxtCover:    
                    if(CannCC>0) CannFin=new Bit81();
                    continue;
                }
                yield break;
            }
            
            public IEnumerable<UFish> IEGet_CoverSet_old( UFish BSet, int CoverSel, bool CannFlg ){
                Bit81 CannFin=new Bit81();  //Cannibalistic Fin
                Combination cmbCov=new Combination(HBL.Count,sz);
                int nxtC=99;
                while( cmbCov.Successor(nxtC) ){
                    Bit81 PCover=new Bit81();
                    int   HCover=0;
                
                    Bit81 BasCvr=new Bit81();    //
                    Bit81 Q;
                    int CannCC=0;
                    for( int k=0; k<sz; k++ ){
                        nxtC=k;
                        int nx=cmbCov.Cmb[k];
                        if( ((1<<HBL[nx].ID)&CoverSel) ==0 )    goto LNxtCover;//行列ブロック条件
                        if( (BSet.HouseB&(1<<HBL[nx].ID))>0 )   goto LNxtCover;//BaseSetで使用済み

                        if( (Q=BSet.BaseB81&HBL[nx]).IsZero() ) goto LNxtCover;//BaseSetをカバーしない

                        if( !(BasCvr&Q).IsZero() ){             //既にカバーしたセルと一致
                            if( !CannFlg ) goto LNxtCover;
                            CannFin|=Q;                         //CannibalisticCells
                            CannCC++;
                        }
                        BasCvr |= Q;                            //既にカバーしたセルの累積   
                        PCover |= HBL[nx];                      //CoverSetセル
                        HCover |= 1<<HBL[nx].ID;                //CoverSet house番号
                    }
                    if( CannFlg && CannFin.IsZero() )  goto LNxtCover;

                    Bit81 FinB81=BSet.BaseB81-PCover;
                    UFish UF = new UFish(BSet,HCover,PCover,FinB81,CannFin);
                    yield return UF;

                LNxtCover:    
                    if(CannCC>0) CannFin=new Bit81();
                    continue;
                }
                yield break;
            }
        }

        public class UFish{
            static public GNPZ_Analyzer pSA;
            public int      no;
            public int      sz;
            public int      HouseB=0;
            public Bit81    BaseB81=null;
            public Bit81    EndoFin=null;
            public List<FishCell> FCLstB;

            public UFish    BaseSet=null;
            public int      HouseC=0;
            public Bit81    CoverB81=null;
            public Bit81    FinB81=null;
            public Bit81    CannFin=null;
            public List<FishCell> FCLstC;

            public UFish( int no, int sz, int HouseB, Bit81 BaseB81, Bit81 EndoFin ){
                this.no=no;
                this.sz=sz;
                this.HouseB =HouseB;
                this.BaseB81=BaseB81;
                this.EndoFin=EndoFin;
            }
            public UFish( int no, int sz, int HouseB, Bit81 BaseB81, List<FishCell> FCLstB ){
                this.no=no;
                this.sz=sz;
                this.HouseB =HouseB;
                this.BaseB81=BaseB81;
                this.FCLstB= FCLstB;
            }                      
            public UFish( UFish BaseSet, int HouseC, Bit81 CoverB81, Bit81 FinB81, Bit81 CannFin ){
                this.BaseSet =BaseSet;
                this.HouseC  =HouseC;
                this.CoverB81=CoverB81;
                this.FinB81  =FinB81;
                this.CannFin =CannFin;
            }

            public UFish( UFish BaseSet, int HouseC, Bit81 CoverB81, Bit81 FinB81, List<FishCell> FCLstC ){
                this.BaseSet =BaseSet;
                this.HouseC  =HouseC;
                this.CoverB81=CoverB81;
                this.FinB81  =FinB81;
                this.FCLstC= FCLstC;
            }                   
            public int CompareTo( object obj ){
                UFish Q = obj as UFish;
                if( this.no!=Q.no ) return (this.no-Q.no);
                if( this.sz!=Q.sz ) return (this.sz-Q.sz);
                if( this.HouseB!=Q.HouseB ) return (this.HouseB-Q.HouseB);
                return this.BaseB81.CompareTo(Q.BaseB81);
            }
            public override bool Equals( object obj ){
                UFish Q = obj as UFish;
                if( this.no!=Q.no ) return false;
                if( this.sz!=Q.sz ) return false;
                if( this.HouseB!=Q.HouseB ) return false;
                return (BaseB81==Q.BaseB81);
            }
            public string ToString( string ttl ){
                string st = ttl + pSA._HouseToString(HouseB);
                return st;
            }
        }

        public class FishCell{
            public  int rc;
            public  int Bc; //Base
            public  int Cc; //Cover
            public FishCell( int rc, int Bc, int Cc ){
                this.rc=rc;
                this.Bc=Bc;
                this.Cc=Cc;
            }
            public override bool Equals( object obj ){
                var P=obj as FishCell;
 	            return (this.rc==P.rc);
            }
        }
    }
}
