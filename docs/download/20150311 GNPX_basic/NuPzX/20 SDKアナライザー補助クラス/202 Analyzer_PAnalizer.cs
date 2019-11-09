using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GNPZ_Analyzer{

        public class PAnalyzer{
            private List<UCell> _BDLst;
            private bool        retSol;

            public PAnalyzer( ){ }           

            public bool PSolver( List<UCell> BDLst ){
                this._BDLst=BDLst;
           
                retSol=false; //解けたときは各ルーティン内でTrueに設定
                do{
                              //  PA_Print(9,_BDLst);
                    if( _BDLst.Any(p=>p.FreeB==0) )  return false;
                    if( _BDLst.Count(p=>p.Fixed==1)==_BDLst.Count ) break;
                    if( GNP00_Single_Fix() ) continue;
                    if( GNP00_LockedSet() )  continue;
                    break;
                }while(true);
                              //  PA_Print(20,_BDLst);
                return retSol;
            }    
            public List<UCell> PSolver( List<UCell> XLst, int no ){
                int  noBrev=(1<<no)^0x1FF;
                List<UCell> QLst= new List<UCell>();
                XLst.ForEach( P=> QLst.Add(new UCell(P,P.rc,P.FreeB&noBrev)) );     
                PSolver(QLst);
                return QLst;
            }

            private bool GNP00_Single_Fix( ){
                bool sol=false, SolT=false;
                do{
                    sol=false;
                    foreach( var p in _BDLst ){
                        if( p.FreeBC==1 && p.Fixed!=1 ){
                            p.Fixed=1; 　//唯一確定
                            _BDLst.ForEach( q=> { //タップル内セルの波及処理
                                if( p!=q && (q.FreeB&p.FreeB)>0 ){
                                    sol=true; q.FreeB ^= p.FreeB;
                                }
                            } );
                        }
                    }
                    //PA_Print(11,_BDLst);
                    if( sol ) SolT=true;
                }while(sol); //唯一確定が複数あるときは連続実行

                if( SolT ) retSol=true;
                return SolT; //
            }
            private bool GNP00_LockedSet( ){
                int rcC=_BDLst.Count;
                if( rcC<2 ) return false;
                int nmPair=0;
                bool hitB=false;

                    //PA_Print(10,_BDLst);
                for( int sz=2; sz<rcC; sz++ ){
                    Combination cmb = new Combination(rcC,sz);
                    while( cmb.Successor() ){
                        _BDLst.ForEach(p=>p.Selected=false);
                        Array.ForEach(cmb.Cmb, p=> _BDLst[p].Selected=true );

                        int nmSel=0, nmNon=0;
                        _BDLst.ForEach(p=>{
                            if( p.Selected ) nmSel |= p.FreeB;
                            else             nmNon |= p.FreeB;
                        } );
                            //Console.WriteLine("nmSel:{0} nmNon:{1}", nmSel.ToBitString(9), nmNon.ToBitString(9));

                        nmPair=0;
                        //===== Naked Locked Set =====
                        if( nmSel.BitCount()==sz ){
                            if( (nmSel&nmNon)>0 ){
                                nmPair = nmSel^0x1FF;
                                for( int m=0; m<rcC; m++ ){
                                      UCell P=_BDLst[m];
                                      if( !P.Selected && (P.FreeB&nmSel)>0 ){
                                          P.CancelB = P.FreeB&nmSel; //無意味(削除される者)
                                          P.FreeB  &= nmPair;
                                          P.Fixed=2;
                                          hitB=true;
                                      }
                                }
                                if( hitB ) goto SolFond;
                            }
                        }
                        //-----------------------------

                        //===== Hidden Locked Set =====
                        int nmNonRev = nmNon^0x1FF;
                        nmPair=0;
                        int hitC=0;
                        _BDLst.ForEach(p=>{
                            if( p.Selected && (p.FreeB&nmNonRev)>0 ){
                                nmPair |= (p.FreeB&nmNonRev);
                                hitC++;
                            }
                        } );

                        if( hitC==sz && (nmPair&nmSel)==0 ){
                            foreach( var P in _BDLst.Where(p=>p.Selected) ){
                                P.CancelB = P.FreeB&nmNon; //無意味(削除される者)
                                P.FreeB  &= nmNonRev;
                                P.Fixed=2;
                                hitB=true;
                            }

                            goto SolFond;
                        }
                        //-----------------------------
                    }
                }

              SolFond:
                if( hitB ) retSol=true;
                return hitB;
            }
            public void PA_Print( int sq, List<UCell> _BDLst ){
                string po="";
                foreach( var X in _BDLst ){
                    string pa=X.FreeB.ToBitStringNZ(9);
                    if( X.FreeBC==1 ) pa = "-"+pa;
                    po += " ["+(X.r*10+X.c+11)+"]"+pa;
                }
                Console.WriteLine("    BD{0} {1}", sq, po );
            }
        }
    }
}