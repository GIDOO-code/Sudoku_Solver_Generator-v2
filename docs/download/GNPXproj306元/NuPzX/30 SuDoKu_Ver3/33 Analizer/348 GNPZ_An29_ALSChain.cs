using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPZ_sdk{
    public partial class ALSTechGenEx: AnalyzerBaseV3{
        public bool ALS_Chain(){
			Prepare();
            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=3 ) return false;
            ALSMan.QSearch_ALS2ALS_Link(true); //F:only singly T:doubly+singly
            
            for( int szCtrl=3; szCtrl<=12; szCtrl++ ){  //Search from small size ALS-Chain
                var SolStack=new Stack<UALSPairEx>();
                foreach( var ALSHead in ALSMan.ALSLst.Where(p=>p.ConnLst!=null && !p.LimitF) ){
                    if( !ALSHead.singly )  continue;
                    bool limitF=false;
                    foreach( var LK0 in ALSHead.ConnLst ){
                        SolStack.Push(LK0);
                        LK0.rcUsed = LK0.ALSpre.B81 | LK0.ALSnxt.B81;
                        int szCtrlX = szCtrl-LK0.ALSpre.Size-LK0.ALSnxt.Size;
                        _Search_ALSChain(LK0,LK0,SolStack, szCtrlX,ref limitF);  //Recursive Search
                        if(SolCode>0) return true;
                        SolStack.Pop();
                    }
                    if(!limitF) ALSHead.LimitF=true;　//When the solution is within the size limit, do not search by the next size
                }
            }
            return false;
        }

        private bool _Search_ALSChain( UALSPairEx LK0, UALSPairEx LKpre, Stack<UALSPairEx> SolStack, int szCtrl, ref bool limitF ){
            int nRccPre=LKpre.nRCC;
            foreach( var LKnxt in LKpre.ALSnxt.ConnLst.Where(p=>(p.nRCC!=nRccPre)) ){           
                UALSEx UAnxt=LKnxt.ALSnxt;
                if( !UAnxt.singly )  continue;
                int szCtrlX = szCtrl-UAnxt.Size;
                if(szCtrlX<0){ limitF=true; return false; }
                if( !(LKpre.rcUsed&UAnxt.B81).IsZero() ) continue;

                SolStack.Push(LKnxt);

                Bit81 rcUsedNxt = LKpre.rcUsed|UAnxt.B81;
                if( _CheckSolution_ALSChain(LK0,LKnxt,rcUsedNxt,SolStack) ) return true;
                
                LKnxt.rcUsed = rcUsedNxt;
                if( _Search_ALSChain(LK0,LKnxt,SolStack,szCtrlX,ref limitF) ) return true;
                SolStack.Pop();
            }
            return false;
        }

        private bool _CheckSolution_ALSChain( UALSPairEx LK0, UALSPairEx LKn, Bit81 rcUsed, Stack<UALSPairEx> SolStack ){   
            int ElmBH = LK0.ALSpre.FreeB.BitReset(LK0.nRCC);
            int ElmBT = LKn.ALSnxt.FreeB.BitReset(LKn.nRCC);
            int ElmB =ElmBH&ElmBT;
            if( ElmB==0 ) return false;

            foreach( int Eno in ElmB.IEGet_BtoNo() ){
                int EnoB=(1<<Eno);
                Bit81 Ez=new Bit81();
                foreach( var P in LK0.ALSpre.UCellLst.Where(p=>(p.FreeB&EnoB)>0)) Ez.BPSet(P.rc);
                foreach( var P in LKn.ALSnxt.UCellLst.Where(p=>(p.FreeB&EnoB)>0)) Ez.BPSet(P.rc);

                Bit81 TBD=(new Bit81(pBDL,EnoB))-rcUsed;
                foreach (var rc in TBD.IEGet_rc() ){
                    if( (Ez-ConnectedCells[rc]).IsZero() ){ pBDL[rc].CancelB|=EnoB; SolCode=2; }
                }
            }

            if(SolCode>0){
                _SolResult_ALSChain(SolStack);
                return true;
            }
            return false;
        }
        private void _SolResult_ALSChain( Stack<UALSPairEx> SolStack ){
            Result = "ALS Chain";
        }
    }
}