const toggle = document.getElementById("toggleme");
const menu = document.querySelector(".menu");

/* Toggle mobile menu */
/* Add the "active" class to the element's classList to open the menu */
/* When closed the class is <ul class="menu"> */
/* When open the class is <ul class="menu active" */
function toggleMenu() {
    if (menu.classList.contains("active")) {
            menu.classList.remove("active");

        // adds the menu (hamburger) icon
        toggle.innerHTML = "<img src='/images/hamburger.png' />";
    } else {
            menu.classList.add("active");

        // adds the close (x) icon
        toggle.innerHTML = "<img src='/images/cross.png' />";
        }
}

/* Event Listener */
toggle.addEventListener("click", toggleMenu, false);

const items = document.querySelectorAll(".item");
