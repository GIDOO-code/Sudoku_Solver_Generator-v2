using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public class LockedCandidateGen: AnalyzerBaseV2{
        public LockedCandidateGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        public bool LockedCandidate( ){
            for(int no=0; no<9; no++ ){
                int noB=(1<<no);
                int[] BRCs = new int[9];
                foreach( var P in pBDL.Where(Q=>(Q.FreeB&noB)>0) ){ BRCs[P.b] |= (1<<P.r)|(1<<(P.c+9)); }

                //==== Type-1 =====
                for(int b0=0; b0<9; b0++ ){
                    for(int hs=0; hs<10; hs+=9 ){  //0:row 9:collumn
                        int RCH=BRCs[b0]&(0x1FF<<hs);
                        if(RCH.BitCount()!=1) continue;
                        int hs0=RCH.BitToNum(18);
                        if( pBDL.IEGetCellInHouse(hs0,noB).All(Q=>Q.b==b0) )  continue;

                        SolCode=2; //----- found -----
                        foreach( var P in pBDL.IEGetCellInHouse(hs0,noB) ){ 
                            if(P.b!=b0) P.CancelB=noB;
                            else        P.SetNoBBgColor(noB,AttCr3,SolBkCr);
                        }
                        string SolMsg= $"Locked Candidate B{(b0+1)} #{(no+1)}";
                        Result=SolMsg;
                        if(__SimpleAnalizerB__) return true;
                        if(SolInfoB) ResultLong=SolMsg;
                        if(!pAnMan.SnapSaveGP())  return true;
                        return true;
                    }
                }
                
                //==== Type-2 =====
                for(int b0=0; b0<9; b0++ ){
                    int b1, b2, rcB0, rcB1, rcB2, rcB12, hs0;
                    for(int hs=0; hs<10; hs+=9 ){  //0:row 9:collumn
                        int hsX=0x1FF<<hs;
                        if(hs==0){ b1=b0/3*3+(b0+1)%3; b2=b0/3*3+(b0+2)%3; } // b1,b2:block(row direction)
                        else{      b1=(b0+3)%9;        b2=(b0+6)%9; }        // b1,b2:block(collumn direction)

                        if((rcB0=BRCs[b0]&hsX).BitCount()<=1)  continue;
                        if((rcB1=BRCs[b1]&hsX)<=0)  continue;
                        if((rcB2=BRCs[b2]&hsX)<=0)  continue;

                        if((rcB12=rcB1|rcB2).BitCount()!=2)  continue;
                        if((hs0=rcB0.DifSet(rcB12).BitToNum(18))<0) continue;;
                      
                        SolCode=2; //----- found -----
                        foreach( var P in pBDL.IEGetCellInHouse(18+b0,noB) ){ //the cell
                            if( !HouseCells[hs0].IsHit(P.rc) ) P.CancelB=noB;
                            else                               P.SetNoBBgColor(noB,AttCr3,SolBkCr);
                        }
                        string SolMsg= $"Locked Candidate B{(b0+1)} #{(no+1)}";
                        Result=SolMsg; 
                        if(__SimpleAnalizerB__)  return true;
                        foreach(var P in pBDL.IEGetCellInHouse(18+b1,noB)) P.SetNoBBgColor(noB,AttCr3,SolBkCr); //the another cell
                        foreach(var P in pBDL.IEGetCellInHouse(18+b2,noB)) P.SetNoBBgColor(noB,AttCr3,SolBkCr);
                        if(SolInfoB) ResultLong=SolMsg;
                        if(!pAnMan.SnapSaveGP())  return true;
                    //   }
                    }
                }
            }
            return false;
        }
    }
}