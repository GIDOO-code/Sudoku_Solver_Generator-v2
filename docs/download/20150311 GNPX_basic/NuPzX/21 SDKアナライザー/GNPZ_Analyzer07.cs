using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_Skyscraper( ){  //StrongLinkを使う方法
            SetConnectedCells();
            CeLKMan.PrepareCellLink(1);    //strongLink生成

            for( int no=0; no<9; no++ ){
                int noB=(1<<no);               
                var SSLst = CeLKMan.IEGetNoType(no,1).ToList(); 

                var prm=new Permutation(SSLst.Count,2);
                int nxtX=99;
                while( prm.Successor(nxtX) ){                
                    UCellLink UCLa = SSLst[prm.Pnum[0]];
                    UCellLink UCLb = SSLst[prm.Pnum[1]];
                    nxtX=0;
                    if( UCLa.ID<UCLb.ID ) continue; //2リンクはID降順
                    nxtX=1;

                    Bit81 SSB=new Bit81();
                    SSB.BPSet(UCLa.rc1); SSB.BPSet(UCLa.rc2); 
                    SSB.BPSet(UCLb.rc1); SSB.BPSet(UCLb.rc2); 
                    if( SSB.BitCount()!=4 )  continue;
                    //全てセルは異なる

                    if( !ConnectedCells[UCLa.rc1].IsHit(UCLb.rc1) ) continue;
                    if(  ConnectedCells[UCLa.rc1].IsHit(UCLb.rc2) ) continue;
                    if(  ConnectedCells[UCLa.rc2].IsHit(UCLb.rc1) ) continue;
                    if(  ConnectedCells[UCLa.rc2].IsHit(UCLb.rc2) ) continue;
                    //(UCLa.rc1)(UCLb.rc1)のみ同じハウスに属している

                    Bit81 candHit = ConnectedCells[UCLa.rc2] & ConnectedCells[UCLb.rc2];
                    candHit = candHit - (ConnectedCells[UCLa.rc1] | ConnectedCells[UCLb.rc1]);

                    bool SSfond=false;
                    foreach( UCell P in candHit.IEGetUCeNoB(pBDL,noB) ){     
                        P.CancelB = P.FreeB&noB;
                        SSfond=true;
                    }

                    if( SSfond ){
                        SolCode=2;
                        pBDL[UCLa.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLa.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLb.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLb.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);                     

                        string msg2="";
                        if( SolInfoDsp ){
                            string msg = "\r";
                            msg += "  on " + (no+1) + " in";
                            msg += " r" + (UCLa.rc1/9+1) + "c" + (UCLa.rc1%9+1);
                            msg += " r" + (UCLb.rc1/9+1) + "c" + (UCLb.rc1%9+1);
                            msg += "\r  connected by";
                            msg += " r" + (UCLa.rc2/9+1) + "c" + (UCLa.rc2%9+1);
                            msg += " r" + (UCLb.rc2/9+1) + "c" + (UCLb.rc2%9+1);
                            msg += "\r  eliminated ";

                            foreach( UCell P in candHit.IEGetUCeNoB(pBDL,noB) ){     
                                msg2 += " "+P.rc.ToRCString();
                            }

                            msg2=msg2.ToString_SameHouseComp();
                            ResultLong = "Skyscraper" + msg+msg2;
                            Result = "Skyscraper #"+(no+1) +" in "+msg2;
                        }
                        else Result = "Skyscraper #"+(no+1);
                        if( !SnapSaveGP(true) )  return true;
                    }
                }
            }
            return false;
        }

        public bool GNP00_SkyscraperOld( ){ //素朴な実装
//z         SetConnectedCells();

            for( int no=0; no<9; no++ ){
                int noB=(1<<no);
                List<UCell[]> SSLst = new List<UCell[]>();

                //「数字noを持つ２セル」の組を抽出
                for( int tfx=0; tfx<27; tfx++ ){
                    List<UCell> BDLst = pBDL.IEGet(tfx,noB).ToList();
                    if( BDLst.Count!=2 )  continue;
                    SSLst.Add(BDLst.ToArray());//(同じ組を異なるHouseで登録する場合がある)
                }
                if( SSLst.Count<2 )  continue;

                var cmb=new Combination(SSLst.Count,2);//組合せ生成クラス
                while( cmb.Successor() ){                      //２セル組を２つ選択する組み合わせを試みる
                   UCell[] UCa = SSLst[cmb.Cmb[0]];
                   UCell[] UCb = SSLst[cmb.Cmb[1]];

                    Bit81 SSB = new Bit81();
                    SSB.BPSet( UCa[0].rc ); SSB.BPSet( UCa[1].rc );
                    SSB.BPSet( UCb[0].rc ); SSB.BPSet( UCb[1].rc );
                    if( SSB.Count!=4 )  continue;
                    //(２セル組の端セルは全て異なる)

                    int hitC=0, ma=-1, mb=-1;             
                    for( int k=0; k<4; k++ ){
                        if( !ConnectedCells[UCa[k/2].rc].IsHit(UCb[k%2].rc) )  continue;
                        if( (++hitC)>1 ) goto LNxtTry;
                        ma=1-(k/2); mb=1-(k%2);  //(ma,mb:関連を持たないセル組）
                    }
                    if( hitC!=1 )  goto LNxtTry;
                    //(端セル組の１つのみ同じハウスに属している)

                    Bit81 candHit = new Bit81();
                    candHit.BPSet( UCa[ma].rc );
                    candHit.BPSet( UCb[mb].rc );

                    bool SSfond=false;
                    foreach( UCell P in pBDL.Where(p=>p.No==0) ){     
                        if( SSB.IsHit(P.rc) ) continue;
                        if( (candHit-ConnectedCells[P.rc]).IsZero() ){
                            if( (P.FreeB&noB)==0 )  continue;
                            P.CancelB = P.FreeB&noB;　//他方の端の２セルと同時に関連をもつセルPが存在する
                            SSfond=true;
                        }
                    }

                    if( SSfond ){
                        SolCode=2;
                        for( int k=0; k<2; k++ ){
                            UCell P=UCa[k];
                            if( (P.FreeB&noB)>0 ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
 
                            P=UCb[k];
                            if( (P.FreeB&noB)>0 ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                        }
                        
                        Result = "Skyscraper";
                        if( SolInfoDsp ){
                            string msg = "\r";
                            msg += "  on " + (no+1) + " in";
                            msg += " r" + (UCa[ma].r+1) + "c" + (UCa[ma].c+1);
                            msg += " r" + (UCb[mb].r+1) + "c" + (UCb[mb].c+1);
                            msg += "\r  connected by";
                            msg += " r" + (UCa[1-ma].r+1) + "c" + (UCa[1-ma].c+1);
                            msg += " r" + (UCb[1-mb].r+1) + "c" + (UCb[1-mb].c+1);
                            msg += "\r  eliminated ";

                            foreach( UCell P in pBDL ){
                                if( P.No!=0 )  continue;
                                if( SSB.IsHit(P.rc) ) continue;
                                if( (candHit-ConnectedCells[P.rc]).IsZero() ){
                                    if( (P.FreeB&noB)==0 )  continue;
                                    msg += " r"+(P.r+1) + "c"+(P.c+1);
                                }
                            }
                            ResultLong = "Skyscraper" + msg;
                        }
                        if( !SnapSaveGP() )  return true;
                    }
                LNxtTry:
                    continue;
                }
            }
            return false;
        }
    }
}
