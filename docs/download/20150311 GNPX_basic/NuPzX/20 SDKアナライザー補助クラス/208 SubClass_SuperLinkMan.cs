using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;

using GIDOO_space;

namespace GNPZ_sdk {
    public partial class GNPZ_Analyzer{ 
        public class SuperLinkMan{
            private const int      S=1, W=2;
            private GNPZ_Analyzer  pSA;

            public CellLinkMan     CeLKMan; //C-C
            public GroupedLinkMan  GLKMan;  //Cs-Cs
            public ALSLinkMan      pALSMan;

            public SuperLinkMan( GNPZ_Analyzer pSA ){
                this.pSA  = pSA;
                CeLKMan = new CellLinkMan(pSA);
                GLKMan  = new GroupedLinkMan(pSA);
                pALSMan = pSA.ALSMan;
            }
            public void Reset(){
                CeLKMan.SWCtrl=0;
                GLKMan.GrpCeLKLst=null;
            }
            public void Prepare(){
                CeLKMan.PrepareCellLink(1+2);    //strongLink,weakLink生成
                if( GNumPzl.GMthdOption["GroupedCells"]=="1" )  GLKMan.PrepareGroupedLinkMan();
                if( GNumPzl.GMthdOption["ALS"]=="1" )           pALSMan.SearchALSLink();
            }
            
            public IEnumerable<GroupedLink> IEGet_SuperLinkFirst( UCell UC, int no ){
                int SWCtrl=3;
// /*  ***** debugのための除外　完成時は生き
                List<UCellLink> Plst=CeLKMan.CeLK81[UC.rc];
                if( Plst!=null ){
                    foreach( var LK in Plst.Where(p=> ((p.no==no)&&((p.type&SWCtrl)>0))) ){
                        yield return (new GroupedLink(LK));      
                    }
                }
// */
                UGrCells GUC=new UGrCells(-9,no,UC);
                if( GNumPzl.GMthdOption["GroupedCells"]=="1" ){
                    foreach( var GLK in GLKMan.GrpCeLKLst ){
                        if( GLK.UGCellsA.Equals(GUC) ) yield return GLK;
                    }
                }

                //最初のリンクはALSではない
                yield break;
            }

            public IEnumerable<GroupedLink> IEGet_SuperLink( GroupedLink GLKpre ){
                int SWCtrl=GLKpre.type;
                bool ALSpre=GLKpre is ALSLink;

                if( GLKpre.UGCellsB.Count==1 ){
                    UCell U=GLKpre.UGCellsB[0];
                    List<UCellLink> Plst=CeLKMan.CeLK81[U.rc];
                    if( Plst!=null ){
                        foreach( var LK in Plst ){
                            if( ALSpre && LK.type!=W ) continue;
                            GroupedLink GLK = new GroupedLink(LK);
                            if( Check_SuperLinkSequence(GLKpre,GLK) ) yield return GLK;      
                        }
                    }
                }

                if( GNumPzl.GMthdOption["GroupedCells"]=="1" ){
                    foreach( var GP in GLKMan.GrpCeLKLst){
                        if( ALSpre && GP.type!=W ) continue;                       
                        if( !GLKpre.UGCellsB.EqualsRC(GP.UGCellsA) )  continue;
                        if( GLKpre.no2!=GP.no ) continue;
                        if( Check_SuperLinkSequence(GLKpre,GP) ) yield return GP; 
                    }
                }

             // if( !ALSpre && GNumPzl.GMthdOption["ALS"]=="1" && pALSMan.ALSLinkLst!=null ){
                if( GNumPzl.GMthdOption["ALS"]=="1" && pALSMan.ALSLinkLst!=null ){
                    if( GLKpre.type==W ){
                        foreach( var GP in pALSMan.ALSLinkLst ){
                            if( GLKpre.UGCellsB.Equals(GP.UGCellsA) ) yield return GP; 
                        }
                    }
                    else{ //前がALSLink
                        foreach( var GP in pALSMan.ALSLinkLst ){
                            if( GLKpre.UGCellsB.Equals(GP.UGCellsA) ){
                                if( (GLKpre.UGCellsB.B81&GP.ALSbase.B81).IsZero() ) yield return GP;
                            }
                        }
                    }

                }

                if( ALSpre ){
                    ALSLink ALK=GLKpre as ALSLink;
                    int noB = 1<<ALK.no2;
                    Bit81 BPnoB = new Bit81(pSA.pBDL,noB);

                    Bit81 BP= BPnoB&ALK.UGCellsB.B81;
              //      ALK.UGCellsB.ForEach(P=>{ if((P.FreeB&noB)>0) BP.BPSet(P.rc); });

                    Bit81 UsedCs=GLKpre.UsedCs;
                    for( int tfx=0; tfx<27; tfx++ ){
                        Bit81 HS = BPnoB&pSA.HouseCells[tfx];
                        if( !(BP-HS).IsZero() )  continue;
                        if( (HS-BP).IsZero()  )  continue;

                        Bit81 NxtBP= HS-BP-UsedCs;
                        if( NxtBP.IsZero()  )  continue;

//C                        Console.WriteLine("\n tfx:"+tfx );
//C                        Console.WriteLine( "   BP:"+BP );
//C                        Console.WriteLine( "   HS:"+HS );
//C                        Console.WriteLine( "HS-BP:"+(HS-BP) );
//C                        Console.WriteLine( "NxtBP:"+NxtBP );

                        List<UCell> NxtCs= NxtBP.ToList().ConvertAll(rc=>pSA.pBDL[rc]);
                        for( int k=1; k<(1<<NxtCs.Count); k++ ){
                            UGrCells NxtGrpdCs=new UGrCells(tfx,ALK.no2);
                            int kb=k;
                            for( int n=0; n<NxtCs.Count; n++){
                                if( (kb&1)>0 )  NxtGrpdCs.Add( new UGrCells(NxtCs[n],ALK.no2) );
                                kb>>=1;
                            }
                            GroupedLink GP = new GroupedLink(GLKpre.UGCellsB,NxtGrpdCs,tfx,W);
//C                            Console.WriteLine( GP );
                            yield return GP; 
                        }

                    }
                }
                yield break;
            }
           
            public bool Check_SuperLinkSequence( GroupedLink GLKpre, GroupedLink GLKnxt ){ 
                List<UCell>   qBDL=pSA.pGP.BDL;

                int typP=GLKpre.type;
                if( GLKpre is ALSLink )  typP=S;
                int noP =GLKpre.no2;
                 
                int typN=GLKnxt.type;
                int noN=GLKnxt.no;
                
                UCellLink LKpre = GLKpre.UCelLK;
                UCellLink LKnxt = GLKnxt.UCelLK;

                int FreeBC=0;
                if( LKpre!=null ){ 
                    FreeBC = qBDL[LKpre.rc2].FreeBC;

                    if( LKnxt!=null ){  //singleLink -> singleLink
                        return _Check_SWSequenceSub(typP,noP, LKnxt.type,noN, FreeBC);
                    }
                    else{               //singleLink -> multiLink
                        UGrCells UGrCs=GLKnxt.UGCellsA;
                        if( UGrCs.Count==1 ){ //singleCell -> singleCell
                            return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
                        }
                    }
                }
                else if( GLKpre.UGCellsB.Count==1 && LKnxt!=null ){ // multiLink -> singleLink
                    FreeBC=GLKpre.UGCellsB.FreeB.BitCount();
                    return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
                }

                FreeBC=GLKpre.UGCellsB.FreeB.BitCount();
                return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
            }
  
            private bool _Check_SWSequenceSub( int typPre, int noPre, int typNxt, int noNxt, int FreeBC ){ 
                switch(typPre){
                    case S:
                        switch(typNxt){
                            case S: return (noPre!=noNxt);  //S->S
                            case W: return (noPre==noNxt);  //S->W
                        }
                        break;
                    case W:
                        switch(typNxt){
                            case S: return (noPre==noNxt);  //W->S
                            case W: return ((noPre!=noNxt)&&(FreeBC==2)); //W->W
                        }
                        break;
                }
                return false;
            }
        }
    }
}