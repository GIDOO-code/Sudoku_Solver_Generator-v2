using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using GNPZ_sdk;

namespace CombinationSample {
    class Program {
        static void Main( string[ ] args ){
            for( int px=0; px<5; px++ ){
                Combination cmb = new Combination(14,4);
                Console.WriteLine("\n ===== Combination(6,4) px={0}",px );
                while( cmb.Successor(px) ){
                    Console.WriteLine(cmb);
                }
                Console.Write(">");
                Console.ReadKey();
            }
        }
    }
}
