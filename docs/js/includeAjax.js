//Prototype.jsによる外部HTMLファイル読み込み方法
//http://suh45.blog9.fc2.com/blog-entry-625.html

function includeAjax(id, file) {
  document.open();
  document.write('<div id="'+id+'"></div>');
  document.close();
   
  var p=new Ajax.Updater(id, file, {method: 'get'});
  return p
}