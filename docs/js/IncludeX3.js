//JavaScript で File API を使用してファイルを読み取る
//http://www.html5rocks.com/ja/tutorials/file/dndfiles/

function include(filename/*, afterfunc*/) {
	// Check for the various File API support.
	if (window.File && window.FileReader && window.FileList && window.Blob) {
	  // Great success! All the File APIs are supported.
	} else {
		alert('The File APIs are not fully supported in this browser.');
		return;
	}
/*
	var reader = new FileReader();	//FileReaderの作成
	reader.readAshtml(filename);	//テキスト形式で読み込む
	
	reader.onload = function(ev){
   		document.write(reader.result);
	}
*/
}

function handleFileSelect(evt) {
	var files = evt.target.files; // FileList object
	
	// files is a FileList of File objects. List some properties.
	var output = [];
	for (var i = 0, f; f = files[i]; i++) {
		output.push('<li><strong>', escape(f.name), '</strong> (', f.type || 'n/a', ') - ',
		          f.size, ' bytes, last modified: ',
		          f.lastModifiedDate.toLocaleDateString(), '</li>');
	}
	document.getElementById('list').innerHTML = '<ul>' + output.join('') + '</ul>';
}

function handleFileRead( filename ){
	var reader = new FileReader();
	reader.readAsText(filename);
	return reader.result;
}
