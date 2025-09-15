$("#txtArquivo").change(function () {
  var file = this.files[0];
  $('.fileName').text(file.name);
  if(Math.floor(file.size/Math.pow(1024,2)) > 10)
      $('.size').text("O tamanho do arquivo é muito grande (" + bytesToSize(file.size) + "), O tamanho máximo é de 10MB");
  else
    $('.size').text(bytesToSize(file.size));
});

function bytesToSize(bytes) {
   var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
   if (bytes == 0) return '0 Byte';
   var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
   return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
};