$(document).ready(function () {
	$(".productSeries-list").on("change", getProductsByProductSeries);
	$(".warehouse-type").on("change", getProductsByProductSeries);
});

function getProductsByProductSeries() {
	const productSeriesId = $(".productSeries-list").val();
	const whType = $(".warehouse-type").val();
	$.ajax({
		url: window.urls.getSemiproductsByProductSeries,
		data: { productSeriesId: productSeriesId, whType: whType },
		success: function (data) {
			setProducts(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function setProducts(data) {
	if (data.length > 0) {
		let options = data.map(item => `<option value=${item.value}>${item.text}</option>`).join("");
		$('.select_productId').html(options);
	}
	else {
		$('.select_productId').html("");
	}
}