using System;
using System.Collections.Generic;
using System.Linq;

using GNPZ_sdk;

namespace PermutationTest{
    class Program {
        static void Main( string[ ] args ) {
            Console.WriteLine( "* Permutation(4) *\n  Successor()" );
            Permutation perm = new Permutation(4);
            while(perm.Successor()) Console.WriteLine( perm );

            Console.WriteLine( "\n* Permutation(4,2) *\n  Successor()" );
                perm = new Permutation(4,2);
            while(perm.Successor() ) Console.WriteLine( perm );

            Console.WriteLine( "\n* Permutation(4,3) *\n  Successor(1)" );
                perm = new Permutation(4,3);
            while(perm.Successor(1) ) Console.WriteLine( perm );
           
            Console.Write( "\nEnd with key input：" );
            Console.ReadKey();
        }
    }
}
