/**
 *
 * You can write your JS code here, DO NOT touch the default style file
 * because it will make it harder for you to update.
 * 
 */
"use strict";

$(".btn-order-index").on('click', function () {
    $(".edit__order__wrapper").addClass("show__edit__order__wrapper");
    $(".edit__order__overlay").addClass("active");
    $("body").addClass("over_hid");
});

$(".edit__order__overlay").on('click', function () {
    $(".edit__order__wrapper").removeClass("show__edit__order__wrapper");
    $(".edit__order__overlay").removeClass("active");
    $("body").removeClass("over_hid");
});