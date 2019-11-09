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
    public class NXGLinks{
        private const int     S=1, W=2;
        private GNPX_AnalyzerMan pAnMan;
        private List<UCell>   pBDL{ get{ return pAnMan.pBDL; } }  
        private Bit81[]       pConnectedCells{ get{ return AnalyzerBaseV2.ConnectedCells; } }

        public  Bit81[][]     nxgConnectedCellsNo;
        public  Bit81[]       Qroute;

        public  List<UCellLink>[] revLink;

        public  CellLinkMan   CeLKMan;
        public  ALSLinkMan    ALSMan;

        public NXGLinks( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan = pAnMan;

            CeLKMan = new CellLinkMan(pAnMan);
            ALSMan  = new ALSLinkMan(pAnMan);
            nxgConnectedCellsNo=null;
        }

        public void Initialize(){ 
            CeLKMan.Initialize();
            ALSMan.Initialize();
            nxgConnectedCellsNo=null;
        }
             
        public Bit81[] GetCellLinks( int rc0, int no0 ){
            if(nxgConnectedCellsNo==null)  nxgConnectedCellsNo=new Bit81[81][];
            if(nxgConnectedCellsNo[rc0]==null){
                CeLKMan.PrepareCellLink(1+2);    //strongLink,weakLink生成

                revLink = new List<UCellLink>[81];
                Qroute = new Bit81[9];
                var Qtrue=new Bit81[9];
                var Qfalse=new Bit81[9];
                for( int k=0; k<9; k++ ){
                    Qroute[k]=new Bit81(); Qtrue[k]=new Bit81();  Qfalse[k]=new Bit81();
                }

                var rcQue=new Queue<UCellLink>();
                var R=new UCellLink(null,pBDL[rc0],no0,S); //最初はS
                rcQue.Enqueue(R);   //W
           
                bool first=true;
                while(rcQue.Count>0){
                    R = rcQue.Dequeue();
                    int rcX=R.rc2;
                    if( !first && rcX==rc0 )  continue;
                    int noX=R.no;          
                    if( rcX!=rc0 ) Qroute[noX].BPSet(rcX);
                    first=false;

                    switch(R.type){
                        case S:
                            foreach( var L in CeLKMan.IEGetRcNoType(rcX,noX,W) ){
                                if( Qroute[noX].IsHit(L.rc2) )  continue;
                                rcQue.Enqueue(L);
                                if(revLink[L.rc2]==null) revLink[L.rc2]=new List<UCellLink>();
                                revLink[L.rc2].Add(L);
                                if( L.rc2!=rc0 ) Qroute[noX].BPSet(L.rc2);
                                Qfalse[noX].BPSet(L.rc2);
                            }
                            foreach( int no2 in pBDL[rcX].FreeB.IEGet_BtoNo() ){
                                if(no2==noX)  continue;
                                if( rcX!=rc0 ) Qroute[no2].BPSet(rcX);
                                Qfalse[no2].BPSet(rcX);
                                foreach( var L in CeLKMan.IEGetRcNoType(rcX,no2,S) ){
                                    if( Qroute[no2].IsHit(L.rc2) )  continue;
                                    rcQue.Enqueue(L);
                                    if(revLink[L.rc2]==null) revLink[L.rc2]=new List<UCellLink>();
                                    revLink[L.rc2].Add(L);
                                    if( L.rc2!=rc0 ) Qroute[no2].BPSet(L.rc2);
                                    Qtrue[no2].BPSet(L.rc2);
                                }
                            }
                            break;

                        case W:
                            foreach( var L in CeLKMan.IEGetRcNoType(rcX,noX,S) ){
                                if( Qroute[noX].IsHit(L.rc2) )  continue;
                                rcQue.Enqueue(L); 
                                if(revLink[L.rc2]==null) revLink[L.rc2]=new List<UCellLink>();
                                revLink[L.rc2].Add(L);
                                if( L.rc2!=rc0 ) Qroute[noX].BPSet(L.rc2);
                                Qtrue[noX].BPSet(L.rc2);
                            }
                            if( pBDL[rcX].FreeBC==2 ){
                                int no2 = pBDL[rcX].FreeB.BitReset(noX).BitToNum();
                                if( rcX!=rc0 ) Qroute[no2].BPSet(rcX);
                                Qtrue[no2].BPSet(rcX);
                                foreach( var L in CeLKMan.IEGetRcNoType(rcX,no2,W) ){
                                    if( Qroute[no2].IsHit(L.rc2) )  continue;
                                    rcQue.Enqueue(L);
                                    if(revLink[L.rc2]==null) revLink[L.rc2]=new List<UCellLink>();
                                    revLink[L.rc2].Add(L);
                                    if( L.rc2!=rc0 ) Qroute[no2].BPSet(L.rc2);
                                    Qfalse[no2].BPSet(L.rc2);
                                }
                            }
                            break;
                    }
                }

                revLink[rc0] = revLink[rc0].Distinct().ToList();
                nxgConnectedCellsNo[rc0]=Qfalse;               
            }
            return nxgConnectedCellsNo[rc0];
        }
   /*   
        private IEnumerable< Stack<UCellLink> > Get_NLs( int rc0 ){
            if( revLink==null || revLink[rc0]==null )  yield break;
            List<UCellLink> P=new List<UCellLink>();

            int rcX=rc0;
            while(){



            Stack<UCellLink> SolStack=new Stack<UCellLink>();
            yield return SolStack;
        }
*/
    }
}
