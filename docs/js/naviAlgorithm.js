function writeNaviAlgorithm(){
	var html = "";
//	html += '<nav>';

	html += '<nav class="naviL">';
	    html += '<p>Algorithm</p>';
    	html += '<ul id="navigation">';
			html += '<li><a href="?Single">Single</a>';
				html += '<ul>';		
					html += '<li><a href="page31a.html">Last Digit</a></li>';
					html += '<li><a href="page31b.html">Naked Single</a></li>';
					html += '<li><a href="page31c.html">Hidden Single</a></li>';
			    html += '</ul>';
			html += '</li>';
	    	
	    	html += '<li><a href="page32.html">LockedCandidate</a></li>';
			html += '<li><a href="page33.html">LockedSet</a></li>';

	   		html += '<li><a href="?Fish">Fish</a>';
				html += '<ul>';	
					html += '<li><a href="page34.html">X-Wing(Fish)</a></li>';
		        	html += '<li><a href="page35.html">Finned Fish</a></li>';
		        	html += '<li><a href="page36.html">(Finned)Franken/Mutant Fish</a></li>';
		        	html += '<li><a href="page37.html">(Finned)Endo Fish</a></li>';
		        	html += '<li><a href="page37.html#Cannibalism">(Finned)Cannibalism</a></li>';
		        	html += '<li><a href="page38.html">Kraken(Finned)Fish</a></li>';
			    html += '</ul>';
			html += '</li>';
	 	
	   		html += '<li><a href="?Link">Link</a>';
				html += '<ul>';		
					html += '<li><a href="page40.html">Skyscraper</a></li>';
					html += '<li><a href="page41.html">EmptyRectangle</a></li>';
					html += '<li><a href="page48.html">X-Chain</a></li>';
					html += '<li><a href="page49.html">XY-Chain</a></li>';
					html += '<li><a href="page46.html">(Multi)Coloring</a></li>';
					html += '<li><a href="page42.html">[BV]XY-Wing</a></li>';
					html += '<li><a href="page43.html">[BV]W-Wing</a></li>';
					html += '<li><a href="page47.html">[BV]RemotePair</a></li>';
			    html += '</ul>';
			html += '</li>';

	   		html += '<li><a href="?XYZWing">XYZ-Wing</a>';
				html += '<ul>';		
					html += '<li><a href="page44.html">XYZ-Wing</a></li>';
					html += '<li><a href="page44.html#XYZWingALS">XYZ-Wing(ALS)</a></li>';
			    html += '</ul>';
			html += '</li>';

			html += '<li><a href="page45.html">SueDeCoq</a></li>';
			
			html += '<li><a href="?ALS">ALS</a>';
				html += '<ul>';		
		        	html += '<li><a href="page50.html">ALS XZ</a></li>';
		        	html += '<li><a href="page51.html">ALS XY-Wing</a></li>';
		        	html += '<li><a href="page52.html">ALS Chain</a></li>';
					html += '<li><a href="page53.html">ALS DeathBlossom</a></li>';
			    html += '</ul>';
			html += '</li>';

			html += '<li><a href="?NiceLoop">Nice Loop</a>';
				html += '<ul>';		
		        	html += '<li><a href="page54.html">Nice Loop</a></li>';
		        	html += '<li><a href="page55.html">Grouped Nice Loop</a></li>';
			    html += '</ul>';
			html += '</li>'; 

			html += '<li><a href="?ForceChain">ForceChain</a>';
				html += '<ul>';		
		        	html += '<li><a href="page56.html">ForceChain_Cell</a></li>';
		        	html += '<li><a href="page56.html#FC_House">ForceChain_House</a></li>';
		        	html += '<li><a href="page56.html#FC_Null">ForceChain_Contradiction</a></li>';
			    html += '</ul>';
			html += '</li>';
			
			html += '<li><a href="page60.html">General Logic</a></li>';	
  	
		html += '</ul>';
			 	
	    html += '<p>Base Class</p>';
	    	html += '<ul>';
	        html += '<li><a href="page21.html">Cell(UCell)</a></li>';
	        html += '<li><a href="page22.html">Problem(UProblem)</a></li>';
	        html += '<li><a href="page23.html">Bit81</a></li>';
	        html += '<li><a href="page24.html">FishUFish FishMan)</a></li>';
	        html += '<li><a href="page25.html">Link(UCellLink、CellLinkMan)</a></li>';
			html += '<li><a href="page26.html">ALS(UALS,ALSMan）</a></li>';
			html += '<li><a href="page27.html">Link2(Cell-ALS)</a></li>';
			html += '<li><a href="page28.html">SuperLink</a></li>';
	    html += '</ul>';
	//<!-- 	<hr> -->
		
		html += '<p>Element Tech</p>';
	    html += '<ul>';
	        html += '<li><a href="page11.html">Permutation</a></li>';
	        html += '<li><a href="page12.html">Combination</a></li>';
	        html += '<li><a href="page14.html">Latin Square</a></li>';
	        html += '<li><a href="page4.html">Conversion/standardization of Sudoku problem</a></li>';
	    html += '</ul>';
	//<!-- 	<hr> -->

	    html += '<p>Links</p>';
	    html += '<ul>';
	        html += '<li><a href="http://hodoku.sourceforge.net/en/index.php">HODOKU</a></li>';
	        html += '<li><a href="http://forum.enjoysudoku.com/">Sudoku Player&acute;s Forum</a></li>';
	    html += '</ul>';
	//<!-- 	<hr> -->

	html += '</nav>'; 
	document.write(html);
}