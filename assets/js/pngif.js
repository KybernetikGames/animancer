const collection = document.getElementsByClassName("pngif");
for (let i = 0; i < collection.length; i++) {
  var item = collection[i];
  item.setAttribute("onclick", "pngif(this);");
  pngif(item);
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