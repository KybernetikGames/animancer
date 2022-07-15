const collection = document.getElementsByClassName("pngif");
var gifs = [];
for (let i = 0; i < collection.length; i++) {
  var item = collection[i];
  item.setAttribute("onclick", "pngif(this);");

  var src = item.getAttribute("src");
  src = src.slice(0, -3) + "gif";
  gifs[i] = new Image();
  gifs[i].src = src;
}

function pngif(item) {
  var src = item.getAttribute("src");
  var name = src.slice(0, -3);
  if (src[src.length - 1] == "g")
    name += "gif";
  else
    name += "png";
  item.setAttribute("src", name);
}