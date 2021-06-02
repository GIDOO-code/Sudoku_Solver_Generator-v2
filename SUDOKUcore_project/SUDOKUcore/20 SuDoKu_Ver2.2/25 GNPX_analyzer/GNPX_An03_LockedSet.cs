using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPXcore{
    public class LockedSetGen: AnalyzerBaseV2{
        public LockedSetGen( GNPX_AnalyzerMan AnMan ): base(AnMan){ }

        //http://csdenpe.web.fc2.com/page33.html
        public bool LockedSet2(){ return LockedSetSub(2); }//2D
        public bool LockedSet3(){ return LockedSetSub(3); }//3D
        public bool LockedSet4(){ return LockedSetSub(4); }//4D
        public bool LockedSet5(){ return LockedSetSub(5); }//complementary to 4D Hidden
        public bool LockedSet6(){ return LockedSetSub(6); }//complementary to 3D Hidden
        public bool LockedSet7(){ return LockedSetSub(7); }//complementary to 2D Hidden

        public bool LockedSet2Hidden(){ return LockedSetSub(2,HiddenFlag:true); } //2D Hidden
        public bool LockedSet3Hidden(){ return LockedSetSub(3,HiddenFlag:true); } //3D Hidden
        public bool LockedSet4Hidden(){ return LockedSetSub(4,HiddenFlag:true); } //4D Hidden
        public bool LockedSet5Hidden(){ return LockedSetSub(5,HiddenFlag:true); } //complementary to 4D 
        public bool LockedSet6Hidden(){ return LockedSetSub(6,HiddenFlag:true); } //complementary to 3D 
        public bool LockedSet7Hidden(){ return LockedSetSub(7,HiddenFlag:true); } //complementary to 2D 

        public bool LockedSetSub( int sz, bool HiddenFlag=false ){
            string resST="";
            UCell UC;
            for (int tfx=0; tfx<27; tfx++ ){
                List<UCell>  BDLstF = pBDL.IEGetCellInHouse(tfx,0x1FF).ToList();    //selecte cells in house
                int ncF = BDLstF.Count;
                if(ncF<=sz) continue;
                
                Combination cmbG = new Combination(ncF,sz);
                while(cmbG.Successor()){
                    BDLstF.ForEach(p=>p.Selected=false);        //(Initialization of cells in the house. The target is several.)
                    int selBlk=0;
                    Array.ForEach(cmbG.Index, p=> { (UC=BDLstF[p]).Selected=true; selBlk|=1<<UC.b; });  //selecte cells by Combination

                    int noBSel=0, noBNon=0;
                    BDLstF.ForEach(p=>{
                        if(p.Selected) noBSel |= p.FreeB;
                        else           noBNon |= p.FreeB;
                    } ); 
                    if((noBSel&noBNon)==0) continue;                                //any digits that can be excluded?

                    //=============== Naked Locked Set ===============
                    if(!HiddenFlag){
                        if(noBSel.BitCount()==sz){                                  //Number of selected cell's dijits is sz
                            if(tfx<18 && selBlk.BitCount()==1){
                                // in searching row or column, there are elements that can be excluded in block.
                                int tfx2=selBlk.BitToNum()+18;                      //bit expression -> House_No(18-26)
                                List<UCell> PLst = pBDL.IEGetCellInHouse(tfx2,noBSel).ToList();
                                BDLstF.ForEach(P=> {if(P.Selected) PLst.Remove(P); });
                                if(PLst.Count>0) PLst.ForEach(P=> { P.CancelB=P.FreeB&noBSel; } );
                            }

                            resST="";
                            foreach(var P in BDLstF){
                                if(P.Selected){
                                    P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                    resST += " "+P.rc.ToRCString();
                                }
                                else P.CancelB=P.FreeB&noBSel;
                            }
                            resST = resST.ToString_SameHouseComp()+" #"+noBSel.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP()) return true;
                        }
                    }

                    //=============== Hidden Locked Set ===============
                    if(HiddenFlag){
                        if(noBNon.BitCount()==(ncF-sz)){                            //Number of unselected cell's dijits is (ncF-sz)
                            resST="";
                            foreach(var P in BDLstF.Where(p=>p.Selected)){
                                P.CancelB = P.FreeB&noBNon;
                                P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                resST += " "+P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp()+" #"+nobR.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP()) return true;
                        }
                    }
                }
            }
            return false;
        }

        private void _LockedSetResult( int sz, string resST, bool HiddenFlag ){
            SolCode = 2;
            string LSmsg="";
            switch(sz){
                case 2: LSmsg = "Pair[2D]"; break;
                case 3: LSmsg = "Triple[3D]"; break;
                case 4: LSmsg = "Quartet[4D]"; break;

                case 5: LSmsg = "Set[5D]"; break;
                case 6: LSmsg = "Set[6D]"; break;
                case 7: LSmsg = "Set{7D}"; break;
            }
            string SolMsg="Locked"+LSmsg;
            if(HiddenFlag) SolMsg += " hidden";
            SolMsg += " "+resST;
            Result=SolMsg;
            ResultLong=SolMsg;
        }
    }
}