$(document).ready(function () {
    $("#paperId").change(function () {
        var paperId = $("#paperId option:selected").val();
        $.ajax({
            url: window.urls.getPrintOrdersByPaperId,
            method: "POST",  
            data: { "paperId": paperId },
            success: function (data) {
                $("#printOrderId").empty();
                for (let i = 0; i < data.printOrders.length; i++) {
                    $("#printOrderId").append(`<option value ='${data.printOrders[i].value}'>${data.printOrders[i].text}</option>`);
                }
            }
        });
    });
});