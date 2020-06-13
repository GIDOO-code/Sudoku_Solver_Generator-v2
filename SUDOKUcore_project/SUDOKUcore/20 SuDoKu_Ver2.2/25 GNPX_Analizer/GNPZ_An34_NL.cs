using System;
using System.Collections.Generic;
using System.Linq;
using GIDOO_space;
using static System.Console;

namespace GNPXcore{
    public partial class NiceLoopGen: AnalyzerBaseV2{
		private int GStageMemo;
        private const int S=1;
        public  int NiceLoopMax{ get{ return GNPXApp000.GMthdOption["NiceLoopMax"].ToInt(); } }

        public NiceLoopGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

		private void Prepare(){
			if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
				CeLKMan.Initialize();
				CeLKMan.PrepareCellLink(1+2);
			}      
		}

        public bool NiceLoop( ){  //Depth-first Search
			Prepare();
            CeLKMan.PrepareCellLink(1+2);    //Generate StrongLink,WeakLink

            for(int szCtrl=4; szCtrl<NiceLoopMax; szCtrl++){
                foreach(var P0 in pBDL.Where(p=>(p.No==0))){                      //Origin Cell

                    foreach(var no in P0.FreeB.IEGet_BtoNo()){                    //Origin Number
                        foreach(var LKH in CeLKMan.IEGetRcNoType(P0.rc,no,3)){    //First Link
                            if(pAnMan.CheckTimeOut()) return false;
                            var SolStack=new Stack<UCellLink>();
                            SolStack.Push(LKH);                 
                            Bit81 UsedCells=new Bit81(LKH.rc2);                   //Bit Representation of Used Cells
                            _NL_Search(LKH,LKH,SolStack,UsedCells,szCtrl-1);
                            if(SolCode>0) return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool _NL_Search( UCellLink LK0, UCellLink LKpre, Stack<UCellLink> SolStack, Bit81 UsedCells, int szCtrl ){
            if( szCtrl<=0 ) return false;

            foreach( var LKnxt in CeLKMan.IEGet_CeCeSeq(LKpre) ){   //links that satisfy concatenation conditions
                int rc2Nxt = LKnxt.rc2;
                if(UsedCells.IsHit(rc2Nxt)) continue;               //UsedCells does not include Origin Cell

                { //===== Chain Search =====
                    SolStack.Push(LKnxt);  
                    //___Debug_Print_NLChain(SolStack);
                    if(rc2Nxt==LK0.rc1 && szCtrl==1){
                        if( SolStack.Count>2 ){                     //Loop was formed (the next cell matches the Origin Cell)
                            int SolType=_NL_CheckSolution(LK0,LKnxt,SolStack,UsedCells);//Solved?
                            if( SolType>0 ){          
                                if(SolInfoB) _NL_SolResult(LK0,LKnxt,SolStack,SolType);

                                if(__SimpleAnalizerB__)  return true;
                                if(!pAnMan.SnapSaveGP(false))  return true;
                            }
                        }
                    }
                    else{
                        Bit81 UsedCellsNxt = UsedCells|(new Bit81(rc2Nxt));   //Create a new bit representation of used cell
                        _NL_Search(LK0,LKnxt,SolStack,UsedCellsNxt,szCtrl-1); //Next step Search(recursive call
                        if(SolCode>0) return true;
                    }
                    SolStack.Pop();                                 //Failure(Cancel link extension processing）
                } //-----------------------------
            }  
            return false;
        }

        private int _NL_CheckSolution( UCellLink LK0, UCellLink LKnxt, Stack<UCellLink> SolStack, Bit81 UsedCells ){ 
            bool SolFound=false;
            int SolType = CeLKMan.Check_CellCellSequence(LKnxt,LK0)? 1: 2; //1:Continuous 2:DisContinuous

            if(SolType==1){ //===== continuous =====
                //=== Change WeakLink to StrongLink
                List<UCellLink> SolLst=SolStack.ToList();
                Bit81 UsedCellsT = UsedCells|(new Bit81(LK0.rc1));
                foreach( var L in SolLst ){
                    int noB=1<<L.no;
                    foreach(var P in pBDL.IEGetCellInHouse(L.tfx,noB)){
                        if(UsedCellsT.IsHit(P.rc)) continue;
                        P.CancelB |= noB;
                        SolFound=true;
                    }
                }

                //=== S-S (There are no other numbers)
                SolLst.Reverse();
                SolLst.Add(LK0);                           
                var LKpre=SolLst[0];
                foreach(var LK in SolLst.Skip(1) ){
                    if(LKpre.type==1 && LK.type==1){ //S-S
                        UCell P=pBDL[LK.rc1];
                        int noB = P.FreeB.DifSet((1<<LKpre.no)|(1<<LK.no));
                        if(noB>0){ P.CancelB=noB; SolFound=true; }
                    }
                    LKpre=LK;
                }
                if(SolFound) SolCode=2;
            }
            else if(SolType==2){ //===== discontinuous =====
                UCell P=pBDL[LK0.UCe1.rc];  //(for MultiAns code)
                int dcTyp= LK0.type*10+LKnxt.type;
                switch(dcTyp){
                    case 11: 
                        P.FixedNo=LK0.no+1; //Cell number determination
                        P.CancelB=P.FreeB.DifSet(1<<(LK0.no));
                        SolCode=1; SolFound=true; //(1:Fixed）
                        break;
                    case 12: P.CancelB=1<<LKnxt.no; SolCode=2; SolFound=true; break;//(2:Exclude from candidates）
                    case 21: P.CancelB=1<<LK0.no; SolCode=2; SolFound=true; break;
                    case 22: 
                        if(LK0.no==LKnxt.no){ P.CancelB=1<<LK0.no; SolFound=true; SolCode=2; }
                        break;
                }
            }
            if(SolFound){ return SolType; }
            return -1;
        }

        private void _NL_SolResult( UCellLink LK0, UCellLink LKnxt, Stack<UCellLink> SolStack, int SolType ){
            string st = "";

            List<UCellLink> SolLst=SolStack.ToList();
            SolLst.Reverse();
            SolLst.Add(LK0);

            foreach( var LK in SolLst ){
                int noB=(1<<LK.no);
                UCell P1=pBDL[LK.rc1], P2=pBDL[LK.rc2];
                P2.SetCellBgColor(SolBkCr);
                if(LK.type==S){ P1.SetNoBColor(noB,AttCr); P2.SetNoBColor(noB,AttCr3); }
                else{           P2.SetNoBColor(noB,AttCr); P1.SetNoBColor(noB,AttCr3); }
            }

            if(SolType==1) st = "Nice Loop(Continuous)";  //continuous
            else{                                           //discontinuous
                st = $"Nice Loop(Discontinuous) r{(LK0.rc1/9+1)}c{(LK0.rc1%9+1)}";
                int dcTyp= LK0.type*10+LKnxt.type;
                switch(dcTyp){
                    case 11: st+=$" is {(LK0.no+1)}";       break;  //S->S
                    case 12: st+=$" is not {(LKnxt.no+1)}"; break;  //S->W
                    case 21: st+=$" is not {(LK0.no+1)}";   break;  //W->S
                    case 22: st+=$" is not {(LK0.no+1)}";   break;  //W->W
                }
            }

            Result = st;
            ResultLong = st+"\r"+_ToRCSequenceString(SolStack);
        }
        private string _ToRCSequenceString( Stack<UCellLink> SolStack ){    
            if( SolStack.Count==0 ) return ("[rc]:-");
            List<UCellLink> SolLst=SolStack.ToList();
            SolLst.Reverse();

            UCellLink LK0=SolLst[0];
            UCell     P0 =pBDL[LK0.rc1];
            string po = $"[rc]:[{(P0.rc/9*10+(P0.rc%9)+11)}]";
            foreach( var LK in SolLst ){
                UCell  P1 = pBDL[LK.rc2];
                string mk = (LK.type==1)? "=": "-";
                po += mk+(LK.no+1)+mk+$"[{(P1.rc/9*10+(P1.rc%9)+11)}]";
            }
            return po;
        }

        private int ___NLCC=0;
        private void ___Debug_Print_NLChain( Stack<UCellLink> SolStack ){
            WriteLine( $"<{___NLCC++}> {_ToRCSequenceString(SolStack)}" );
        }
    }
}