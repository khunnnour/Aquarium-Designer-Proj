function ResizeElements() 
{
	var newW = window.innerWidth;
	var newH = window.innerHeight;
	//console.log(newW + ", " + newH);

	document.documentElement.style.width = newW + "px";
	document.documentElement.style.height = newH + "px";

	document.body.style.width = newW;
	document.body.style.height = newH;
}

window.onload = ResizeElements;
window.onresize = ResizeElements;