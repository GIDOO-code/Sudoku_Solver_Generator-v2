function writeNaviHeader(){

	var html = "";
	html += '<nav>';
	html += '<ul>';
	html += '<li><a href="index.html">Home</a></li>';
	html += '<li><a href="page2.html">Sudoku Algorithm</a></li>';
	html += '<li><a href="page1.html">Element Tech</a></li>';
	html += '<li><a href="page17.html">Download</a></li>';
//	html += '<li><a href="page18.html">Comment</a></li>';
    html += '<li><a href="page19.html">about this</a></li>';
//    html += '<li><a href="http://csdenp.web.fc2.com/index.html">To Japanese HP</a></li>';
	
	html += '</ul>';
	html += '</nav>';

	document.write(html);
}