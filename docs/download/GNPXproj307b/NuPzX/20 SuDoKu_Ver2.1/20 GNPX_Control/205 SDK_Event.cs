using System;
using static System.Console;

namespace GNPZ_sdk{
 
    public delegate void SDKEventHandler( object sender, SDKEventArgs args );
    public delegate void SDKSolutionEventHandler( object sender, SDKSolutionEventArgs args );

    public class SDKEventArgs: EventArgs{
	    public string eName;
	    public int    eCode;
        public int    ProgressPer;
        public bool   Cancelled;
        public int[]  SDK81;

	    public SDKEventArgs( string eName=null, int eCode=-1, int ProgressPer=-1, bool Cancelled=false ){
            try{
		        this.eName = eName;
		        this.eCode = eCode;
                this.ProgressPer = ProgressPer;
                this.Cancelled = Cancelled;
            }
            catch(Exception e ){
                WriteLine(e.Message);
                WriteLine(e.StackTrace);
            }
	    }
        public SDKEventArgs( int[] SDK81 ){
            this.SDK81=SDK81;
        }
    }

    public class SDKSolutionEventArgs: EventArgs{
        public UProbS    UPB;
        public UPuzzle  GPX;
	    public SDKSolutionEventArgs( UPuzzle  GPX ){
            this.UPB = new UProbS(GPX);
            this.GPX = GPX;
	    }
    }
}