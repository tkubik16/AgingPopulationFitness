/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
function openNav() {
    document.getElementById("nav-bar").style.width = "25%";
    document.getElementById("main-body").style.width = "75%";
    document.getElementById("open-nav-button").style.display = "none";
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
function closeNav() {
    document.getElementById("nav-bar").style.width = "0%";
    document.getElementById("main-body").style.width = "100%";
    document.getElementById("open-nav-button").style.display = "block";
}