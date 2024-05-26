(function ($) {
    $("#search__bar").on("input", function () {
        $(".suggest_block").addClass("show_product_suggestion");
        var keyword = $("#search__bar").val();
        if (keyword.length > 0) {
            $.ajax({
                url: '/SearchProduct/SuggestProduct',
                type: 'post',
                datatype: 'json',
                data: {
                    keyword: keyword,
                },
                async: true,
                success: function (result) {
                    $("#suggest__product").html("");
                    $("#suggest__product").html(result);
                },
                error: function (err) {
                    console.error(err);
                    alert("Lỗi, không tìm thấy sản phẩm");
                }
            });
        }
        else {
            $("#suggest__product").html("");
            $(".suggest_block").removeClass("show_product_suggestion");
        }
    });
})(jQuery)
//$(document).ready(function () {
//    $("#search__bar").on("input", function () {
//        $(".suggest_block").addClass("show_product_suggestion");
//        var keyword = $("#search__bar").val();
//        if (keyword.length > 0) {
//            $.ajax({
//                url: '/SearchProduct/SuggestProduct',
//                type: 'post',
//                datatype: 'json',
//                data: {
//                    keyword: keyword,
//                },
//                async: true,
//                success: function (result) {
//                    $("#suggest__product").html("");
//                    $("#suggest__product").html(result);
//                },
//                error: function (err) {
//                    console.error(err);
//                    alert("Lỗi, không tìm thấy sản phẩm");
//                }
//            });
//        }
//        else {
//            $("#suggest__product").html("");
//        }
//    });
//    var searchBar = $("#search__bar");
//    if (searchBar.length == 0) {
//        $("#suggest__product").html("");
//    }
//});