/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
function openNav() {
    document.getElementById("nav-bar-mobile").style.width = "33.333%";
    //document.getElementById("main-body").style.width = "75%";
   document.getElementById("open-nav-button-mobile").style.display = "none";
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
function closeNav() {
    //document.getElementById("nav-bar").style.width = "0%";
    document.getElementById("nav-bar-mobile").style.removeProperty('width');
    //document.getElementById("main-body").style.width = "100%";
    document.getElementById("open-nav-button-mobile").style.display = "block";
    // test
}