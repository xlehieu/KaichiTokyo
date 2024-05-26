$(document).ready(function () {
    $(".removeItem").click(function () {
        var productId = $(this).data("productid");
        $.ajax({
            url: "/api/cart/remove",
            type: 'post',
            dataType: 'json',
            data: {
                productId: productId,
            },
            success: function () {
                location.reload();
            },
            error: function (error) {
                alert("Lỗi: " + error);
            }
        });
    });
    $(".add-to-cart").click(function () {
        var productId = $("#ProductId").val();
        var soluong = $("#txtsoluong").val();
        $.ajax({
            url: "/api/cart/add",
            type: 'post',
            dataType: "Json",
            data: {
                productId: productId,
                amount: soluong
            },
            success: function (response) {
                location.reload();
            },
            error: function (error) {
                alert("Lỗi: " + error.responseText);
            }
        });
    });
    $('.cartItemSoLuong').change(function () {
        var productId = $(this).data('productid');
        var amount = $(this).val();
        updateCartItem(productId, amount);
    });
    $('.minus').click(function () {
        var productId = $(this).data('productid');
        var currentAmount = $(".cartItemSoLuong-" + productId).val();
        var amount = parseInt(currentAmount) - 1;
        alert("Mã sản phẩm: " + productId + "\nSố lượng: " + amount);
        updateCartItem(productId, amount);
    });
    $('.plus').click(function () {
        var productId = $(this).data('productid');
        var currentAmount = $(".cartItemSoLuong-" + productId).val();
        var amount = parseInt(currentAmount) + 1;
        alert("Mã sản phẩm: " + productId + "\nSố lượng: " + amount);
        updateCartItem(productId, amount);
    });
    function updateCartItem(productId, amount) {
        $.ajax({
            url: '/api/cart/update',
            type: 'post',
            dataType: 'json',
            data: {
                productId: productId,
                amount: amount,
            },
            success: function () {
                location.reload();
            },
            error: function (err) {
                alert("Lỗi: " + err);
            }
        });
    }
    function loadHeaderCart() {
        $("#minicart").load("/AjaxContent/HeaderCart");
        $("#numbercart").load("/AjaxContent/NumberCart");
    }
    $(".add__to__cart").click(function () {
        var productId = $(this).data('productid');
         $.ajax({
             url: '/api/cart/add',
             type: 'post',
             dataType: 'Json',
             data: {
                 productId: productId,
             },
             success: function () {
                 location.reload();
             },
             error: function (err) {
                 alert(error);
             }
         });
    });
    $(".add__to__favourite").click(function () {
        var productId = $(this).data('productid');
        $.ajax({
            url: '/api/favourite/add',
            type: 'post',
            dataType: 'Json',
            data: {
                productId: productId,
            },
            success: function () {
                location.reload();
            },
            error: function (err) {
                alert(error);
            }
        });
    });
});