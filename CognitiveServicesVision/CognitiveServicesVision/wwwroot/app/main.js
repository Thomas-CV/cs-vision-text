function attachLoaderOnClick(imageIndex, destination, loader) {
    document.getElementById(destination).addEventListener("click", function () {
        document.getElementById(loader).style.visibility = "visible";
        window.location.href = '/' + imageIndex;
    });
}
