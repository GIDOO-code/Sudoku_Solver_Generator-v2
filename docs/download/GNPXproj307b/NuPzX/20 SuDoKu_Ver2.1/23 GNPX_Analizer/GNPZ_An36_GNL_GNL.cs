using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static System.Console;
using System.Threading;

using GIDOO_space;

namespace GNPZ_sdk{
    using pRes=Properties.Resources;
    public partial class GroupedLinkGen: AnalyzerBaseV2{

        private const int     S=1, W=2;
		private int  GStageMemo;
        public  int  NiceLoopMax{ get{ return GNPXApp000.GMthdOption["NiceLoopMax"].ToInt(); } }
        private int  SolLimBrk=0;
        private int  __SolGL=-1;

        public UPuzzle  pGP{ get{return pAnMan.pGP;} }

        public GroupedLinkGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

		private void Prepare(){
			if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
				pSprLKsMan.Initialize();
				pSprLKsMan.PrepareSuperLinkMan( AllF:true );
			}      
		}

        public bool GroupedNiceLoop( ){ //Depth-first Search
            ___GNLCC=0;
            try{
                Prepare();
                
                SolLimBrk=0; __SolGL=-1;
                for(int szCtrl=3; szCtrl<=NiceLoopMax; szCtrl++ ){

                    foreach( var P0a in pBDL.Where(p=>(p.No==0)) ){      //Origin Cell
                        var P0=pBDL[P0a.rc];

                        foreach( var no in P0.FreeB.IEGet_BtoNo() ){    //Origin Number

                            foreach( var GLKH in pSprLKsMan.IEGet_SuperLinkFirst(P0,no) ){   //First Link
                               if( pAnMan.CheckTimeOut() ) return false;

                                var SolStack=new Stack<GroupedLink>();
                                SolStack.Push(GLKH);  //##Push
                                    // ___Debug_Print_GNLChain(SolStack);

                                Bit81 UsedCs = new Bit81();  //Bit Representation of Used Cells
                                SetUsed( ref UsedCs, GLKH);
                                    // ___Debug_Print_Bit819s(UsedCs);
                                GLKH.UsedCs=UsedCs;
                                _GroupedNL_Search(GLKH,GLKH,SolStack,szCtrl-1);

                                if( SolLimBrk==int.MaxValue ) goto LblSolLimBrk;
                            }
                        }
                    }
                }

              LblSolLimBrk:
                return (__SolGL>0); //*****

            }
            catch( Exception ex ){
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);
            }
            return false;
        }

        private bool _GroupedNL_Search( GroupedLink GLK0, GroupedLink GLKpre, Stack<GroupedLink> SolStack, int szCtrl ){
            if( szCtrl<0 ) return false;

            Bit81 UsedCs=GLKpre.UsedCs;
            foreach( var GLKnxt in pSprLKsMan.IEGet_SuperLink(GLKpre) ){  //links that satisfy concatenation conditions          
                UGrCells GCsNxt = GLKnxt.UGCellsB;
                int no=GLKnxt.no;

                { //===== Chain Search =====
                    SolStack.Push(GLKnxt);  //( link extension --> Push )
                    ___Debug_Print_GNLChain(SolStack);
                
                    //Loop was formed (the next cell matches the Origin Cell)
                    if( _GroupedNL_LoopCheck(GLK0,GLKnxt) ){
                        if( szCtrl==0 && SolStack.Count>3 ){
                            int SolType=_GroupedNL_CheckSolution(GLK0,GLKnxt,SolStack,UsedCs);
                            if( SolType>0 ){ 

                                if(___GNLCC==2898)  WriteLine("-----------------");
                                ___Debug_Print_GNLChain(SolStack,"<+><+>");         
                                
                                if(SolInfoB) _GroupedNL_SolResult(GLK0,GLKnxt,SolStack,SolType); 

                                __SolGL=SolCode;

                                if(__SimpleAnalizerB__)  return true;
                                if(!pAnMan.SnapSaveGP(false))  return false;
                                Thread.Sleep(10);
                                if((++SolLimBrk)>(int)SDK_Ctrl.MltAnsOption["OneMethod"]){
                                    SolLimBrk=int.MaxValue;
                                    return false;
                                }
                            }
                        }
                    }
                    else if( !CheckUsed( (UsedCs|GLK0.UsedCs), GLKnxt) ){
                        Bit81 UsedCsNxt = new Bit81(UsedCs);   //Create a new bit representation of used cell
                        SetUsed( ref UsedCsNxt, GLKnxt);

                        GLKnxt.UsedCs=UsedCsNxt;
                        _GroupedNL_Search(GLK0,GLKnxt,SolStack,szCtrl-1); //Next step Search(recursive call
                        if(SolLimBrk==int.MaxValue) return false;
                        if(SolCode>0) return true;
                    }
                    SolStack.Pop();     // Failure(Cancel link extension processing --> Pop )
                } //-----------------------------
            }
            return false;
        }

        private void SetUsed( ref Bit81 UsedCs, GroupedLink GLKnxt){ 
            UsedCs |= GLKnxt.UGCellsB.B81;
        }
        private bool CheckUsed( Bit81 UsedPre, GroupedLink GLKnxt){
            Bit81 BP=GLKnxt.UGCellsB.B81;
            if( GLKnxt is ALSLink ) BP -= (GLKnxt.UGCellsA.B81);
            return UsedPre.IsHit(BP); //Overlap Check
        }

        private bool _GroupedNL_LoopCheck( GroupedLink GLK0, GroupedLink GLKnxt ){
            UGrCells Qorg=GLK0.UGCellsA;
            UGrCells Qdst=GLKnxt.UGCellsB;
            if( Qdst.Count!=1 ) return false;
            return  (Qdst[0].rc==Qorg[0].rc);
        }

        private int _GroupedNL_CheckSolution( GroupedLink GLK0, GroupedLink GLKnxt, 
                                              Stack<GroupedLink> SolStack, Bit81 UsedCs ){ 
            bool SolFound=false;
            int SolType = pSprLKsMan.Check_SuperLinkSequence(GLKnxt,GLK0)? 1: 2; //1:Continuous 2:DisContinuous

            if( SolType==1 ){ //<>continuous
                List<GroupedLink> SolLst=SolStack.ToList();
             //___Debug_Print_GNLChain(SolStack);

                SolLst.Reverse();
                SolLst.Add(GLK0); 

                Bit81 UsedCsTmp = new Bit81(UsedCs);
                SetUsed( ref UsedCsTmp, GLKnxt);

                foreach( var LK in SolLst.Where(P=>(P.type==W))){
                    int   noB=1<<LK.no;        
                    Bit81 SolBP=new Bit81();      
                    
                    LK.UGCellsA.ForEach(P=>{ if((P.FreeB&noB)>0) SolBP.BPSet(P.rc); });
                    LK.UGCellsB.ForEach(P=>{ if((P.FreeB&noB)>0) SolBP.BPSet(P.rc); });
                    if( SolBP.BitCount()<=1 ) continue;
                    foreach( var P in pBDL.Where(p=>(p.FreeB&noB)>0) ){
                        if( UsedCsTmp.IsHit(P.rc) ) continue;
                        if( !(SolBP-ConnectedCells[P.rc]).IsZero() )  continue;
                        if( (P.FreeB&noB)==0 )  continue;
                        P.CancelB |= noB;　　//exclusion digit
                        SolFound=true;
                    }
                }

                var LKpre=SolLst[0];               
                foreach( var LK in SolLst.Skip(1) ){  
                    if( LKpre.type==S && LK.type==S && LK.UGCellsA.Count==1 ){
                        var P=pBDL[LK.UGCellsA[0].rc];  //(for MultiAns code)
                        int noB2=P.FreeB-((1<<LKpre.no2)|(1<<LK.no));                       
                        if( noB2>0 ){ P.CancelB |= noB2; SolFound=true; }
                    }
                    LKpre=LK;
                }
                if(SolFound) SolCode=1;
            }
            else{           　//<>discontinuous
                int dcTyp= GLK0.type*10+GLKnxt.type;
                UCell P=pBDL[GLK0.UGCellsA[0].rc];   //(for MultiAns code)
                switch(dcTyp){
                    case 11: 
                        P.FixedNo=GLK0.no+1; //Cell number determination
                        P.CancelB=P.FreeB.DifSet(1<<(GLK0.no));
                        SolCode=1; SolFound=true; //(1:Fixed）
                        break;
                    case 12: P.CancelB=1<<GLKnxt.no; SolCode=2; SolFound=true; break;//(2:Exclude from candidates）
                    case 21: P.CancelB=1<<GLK0.no; SolCode=2; SolFound=true; break;
                    case 22: 
                        if( GLK0.no==GLKnxt.no ){ P.CancelB=1<<GLK0.no; SolFound=true; SolCode=2; }
                        break;
                }
            }

            if(SolFound) return SolType;
            return -1;
        }

        private void _GroupedNL_SolResult( GroupedLink LK0, GroupedLink LKnxt, Stack<GroupedLink> SolStack, int SolType ){          
            string st = "";

            List<GroupedLink> SolLst=SolStack.ToList();
            SolLst.Reverse();
            SolLst.Add(LK0);

            foreach( var LK in SolLst ){
                bool bALK = LK is ALSLink;
                int type = (LK is ALSLink)? S: LK.type;//ALSLink, in ALS, is S
                foreach( var P1 in LK.UGCellsA.Select(p=>pBDL[p.rc])){
                    int noB=(1<<LK.no);
                    if( !bALK )  P1.SetCellBgColor(SolBkCr);
                    if( type==S ){ P1.SetNoBColor(noB,AttCr);  }
                    else{          P1.SetNoBColor(noB,AttCr3); }
                }

                if( type==W ){
                    foreach( var P2 in LK.UGCellsB.Select(p=>pBDL[p.rc])){
                        int noB2=(1<<LK.no);
                        if( !bALK )  P2.SetCellBgColor(SolBkCr);
                        P2.SetNoBColor(noB2,AttCr);
                    }
                }
            }

            int cx=2;
            foreach( var LK in SolLst ){
                ALSLink ALK = LK as ALSLink;
                if( ALK==null )  continue;
                Color crG = _ColorsLst[cx++];
                foreach( var P in ALK.ALSbase.B81.IEGet_rc().Select(rc=>pBDL[rc]) ){
                    P.SetCellBgColor(crG);
                }
            }

            string st3="";
            if( SolType==1 ) st = "Nice Loop(Cont.)";  //<>continuous
            else{                                              //<>discontinuous
                int rc=LK0.UGCellsA[0].rc;
                var P=pBDL[rc];
                st = "Nice Loop(Discont.) r"+(rc/9+1)+"c"+(rc%9+1);
                int dcTyp= LK0.type*10+LKnxt.type; 
                switch(dcTyp){
                    case 11: st+=$" is {(LK0.no+1)}";       P.SetCellBgColor(SolBkCr2); break;
                    case 12: st+=$" is not {(LKnxt.no+1)}"; P.CancelB=1<<LKnxt.no; break;
                    case 21: st+=$" is not {(LK0.no+1)}";   P.CancelB=1<<LK0.no; break;
                    case 22: st+=$" is not {(LK0.no+1)}";   P.CancelB=1<<LK0.no; break;
                }
            }

            string st2=_ToGroupedRCSequenceString(SolStack, ref st3 );
            st = st3+st;
            Result = st;
            ResultLong = st+"\r"+st2;
        }
        private string _ToGroupedRCSequenceString( Stack<GroupedLink> SolStack, ref string st3 ){    
            if( SolStack.Count==0 ) return ("[rc]:-");
            List<GroupedLink> SolLst=SolStack.ToList();
            SolLst.Reverse();

            string po = $"[{SolLst[0].UGCellsA}]";
            foreach( var LK in SolLst ){
                string ST_LinkNo="";
                ALSLink ALK=LK as ALSLink;
                if( ALK!=null ){
                    ST_LinkNo = $"-#{(ALK.no+1)}ALS<{ALK.ALSbase.ToStringRC()}>#{(ALK.no2+1)}-";
                }
                else{
                    string mk = (LK.type==1)? "=": "-";
                    ST_LinkNo = mk+(LK.no2+1)+mk;
                }
                po += $"{ST_LinkNo}[{LK.UGCellsB}]";
            }
            
            if(po.Contains("ALS") || po.Contains("[<")) st3="Grouped ";
            return po;
        }

        private int ___GNLCC=0;
        private void ___Debug_Print_GNLChain( Stack<GroupedLink> SolStack, string msg="" ){
            if(  msg!="" ){ 
                string st3="";
                WriteLine( $"{msg}<{___GNLCC}> {_ToGroupedRCSequenceString(SolStack,ref st3)}" );
            }
            ___GNLCC++;
        }
    }
}