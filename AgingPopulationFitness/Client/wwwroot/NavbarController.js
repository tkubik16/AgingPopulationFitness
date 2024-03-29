﻿/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
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

function setBenefitSelectExerciseFilter( value) {
    const $select = document.querySelector('#SelectBenefitExerciseFilter');
    $select.value = value;
}

function setTypeSelectExerciseFilter(value) {
    const $select = document.querySelector('#SelectTypeExerciseFilter');
    $select.value = value;
}

function setInjuryLocationInjuryDialog(value) {
    const $select = document.querySelector('#SelectInjuryLocationInjuryDialog');
    $select.value = value;
}

function setInjuryLocationViewInjury(value) {
    const $select = document.querySelector('#select-injury-in-view-injury');
    $select.value = value;
}

function setSelectInputToValue(value, string) {
    const $select = document.querySelector(string);
    $select.value = value;
}