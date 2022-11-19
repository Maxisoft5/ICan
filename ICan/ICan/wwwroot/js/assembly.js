$(document).ready(function () {
	$(".product-list").on("change", productListChange);
	$(".productSeries-list").on("change", getProductsByProductSeries);
});

function getProductsByProductSeries() {
	let productSeriesId = $(".productSeries-list").val();
	$.ajax({
		url: window.urls.getProductsByProductSeries,
		data: { productSeriesId: productSeriesId },
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
		$('.product-list').html(options);
		productListChange();
	}
}

function productListChange() {
	let productId = $(".product-list").val();
	let assemblyId = $("#AssemblyId").val() || 0;
	$.ajax({
		url: window.urls.getSemiproductsAndOrders,
		data: { productId: productId, assemblyId: assemblyId},
		success: function (data) {
			setSemiproductsAndPrintOrders(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function setSemiproductsAndPrintOrders(data) {
	$(".semiprods-orders").html("");
	for (let i = 0; i < data.length; i++) {
		let options = data[i].orders.map(item => `<option value=${item.key}>${item.value}</option>`).join("");
		let keyFieldName = data[i].semiproduct.semiproductTypeName === "Наклейки"  ? "NotchOrderId" : "PrintOrderSemiproductId";
		let html = `<div class="form-group">
						<label class="control-label">${data[i].semiproduct.semiproductTypeName}</label>
						<select class="form-control list-orders[${i}]" name="AssemblySemiproducts[${i}].${keyFieldName}">
							${options}
						</select>
					</div>`;
		$(".semiprods-orders").append(html);
	}
}