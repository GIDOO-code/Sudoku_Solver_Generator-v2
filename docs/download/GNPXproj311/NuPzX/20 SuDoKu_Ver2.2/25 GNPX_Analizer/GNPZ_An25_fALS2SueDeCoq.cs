using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
//  Sue de Coq
//  http://hodoku.sourceforge.net/en/tech_misc.php#sdc
//  A more formal definition of SDC is given in the original Two-Sector Disjoint Subsets thread:
//  Consider the set of unfilled cells C that lies at the intersection of Box B and Row (or Column) R.
//  Suppose |C|>=2. Let V be the set of candidate values to occur in C. Suppose |V|>=|C|+2.
//  The pattern requires that we find |V|-|C|+n cells in B and R, with at least one cell in each, 
//  with at least |V|-|C| candidates drawn from V and with n the number of candidates not drawn from V.
//  Label the sets of cells CB and CR and their candidates VB and VR. Crucially,
//  no candidate from V is allowed to appear in VB and VR. 
//  Then C must contain V\(VB U VR) [possibly empty], |VB|-|CB| elements of VB and |VR|-|CR| elements of VR.
//  The construction allows us to eliminate candidates VB U (V\VR) from B\(C U CB), 
//  and candidates VR U (V\VB) from R\(C U CR).
// (\:backslash)

    public partial class AALSTechGen: AnalyzerBaseV2{
		private int GStageMemo;
		private ALSLinkMan fALS;

		public AALSTechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
			fALS = new ALSLinkMan(pAnMan);
        }

        public bool SueDeCoq( ){
			if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
				fALS.Initialize();
	            fALS.PrepareALSLinkMan(+2); //Generate ALS(+1 & +2)
			}
			
            if( fALS.ALSLst.Count<=3 ) return false;

            foreach( var ISPB in fALS.ALSLst.Where(p=> p.tfx>=18 && p.Size>=3) ){     //Selecte Block-type ALS
                if(ISPB.rcbRow.BitCount()<=1 || ISPB.rcbCol.BitCount()<=1) continue;  //Block squares have multiple rows and columns

                foreach( var ISPR in fALS.ALSLst.Where(p=> p.tfx<18 && p.Size>=3) ){　//Selecte Row-type/Column-type ALS
                    if((ISPR.rcbBlk&ISPB.rcbBlk)==0) continue;                  //Intersect with ISPB
                    if(ISPR.rcbBlk.BitCount()<2)     continue;                  //ISPR has multiple blocks

                    //Are the cell configurations of the intersections the same?
                    if( (ISPB.B81&HouseCells[ISPR.tfx]) != (ISPR.B81&HouseCells[ISPB.tfx]) ) continue;

                    // ***** the code follows HP -> http://csdenpe.web.fc2.com/page45.html *****

                    Bit81 IS = ISPB.B81&ISPR.B81;                               //Intersection
                    if(IS.Count<2) continue; 　                                 //At least 2 cells at the intersection
                    if((ISPR.B81-IS).Count==0) continue;                        //There is a part other than the intersecting part in ISPR                    

                    Bit81 PB = ISPB.B81-IS;                                     //ISPB's outside IS
                    Bit81 PR = ISPR.B81-IS;                                     //ISPR's outside IS
                    int IS_FreeB = IS.AggregateFreeB(pBDL);                     //Intersection number
                    int PB_FreeB = PB.AggregateFreeB(pBDL);                     //ISPB's number outside the IS
                    int PR_FreeB = PR.AggregateFreeB(pBDL);                     //ISPR's number outside the IS
                    if((IS_FreeB&PB_FreeB&PR_FreeB)>0) continue;

                    //A.DifSet(B)=A-B=A&(B^0x1FF)
                    int PB_FreeBn = PB_FreeB.DifSet(IS_FreeB);                  //Numbers not at the intersection of PB
                    int PR_FreeBn = PR_FreeB.DifSet(IS_FreeB);                  //Numbers not in the intersection of PR

                    int sdqNC = PB_FreeBn.BitCount()+PR_FreeBn.BitCount();      //Number of confirmed numbers outside the intersection
                    if( (IS_FreeB.BitCount()-IS.Count) != (PB.Count+PR.Count-sdqNC) ) continue;

                    int elmB = PB_FreeB | IS_FreeB.DifSet(PR_FreeB);            //Exclusion Number in PB 
                    int elmR = PR_FreeB | IS_FreeB.DifSet(PB_FreeB);            //Exclusion Number in PR                
                    if( elmB==0 && elmR==0 ) continue;

                    foreach( var P in _GetRestCells(ISPB,elmB) ){ P.CancelB|=P.FreeB&elmB; SolCode=2; }
                    foreach( var P in _GetRestCells(ISPR,elmR) ){ P.CancelB|=P.FreeB&elmR; SolCode=2; }

                    if(SolCode>0){      //--- SueDeCoq found -----
                        SolCode=2;
                        SuDoQueEx_SolResult( ISPB, ISPR );
                        if( ISPB.Level>=3 || ISPB.Level>=3 ) WriteLine("Level-3");

                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP(true))  return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<UCell> _GetRestCells( UALS ISP, int selB ){
            return pBDL.IEGetCellInHouse(ISP.tfx,selB).Where(P=>!ISP.B81.IsHit(P.rc));
        }
        private void SuDoQueEx_SolResult( UALS ISPB, UALS ISPR ){
            Result="SueDeCoq";

            if(SolInfoB){
                ISPB.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );
                ISPR.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );

                string ptmp = "";
                ISPB.UCellLst.ForEach(p=>{ ptmp+=$" {p.rc.ToRCString()}"; } );

                string po = "\r Cells";
                if( ISPB.Level==1 ) po += "(block)  ";
                else{ po += $"-{ISPB.Level}(block)"; }
                po += $": {ISPB.ToStringRCN()}";

                po += "\r Cells" + ((ISPR.Level==1)? "": "-2");
                po += ((ISPR.tfx<9)? "(row)":"(col)");
                po += ((ISPR.Level==1)? "    ": "  ");
                po += ": "+ISPR.ToStringRCN();
                ResultLong = "SueDeCoq"+po;
            }
        }
    }
}