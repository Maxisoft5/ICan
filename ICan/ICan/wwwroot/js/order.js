var isOrderBigEnough = false;

function addressChange() {
	const addrElm = $(".order-address-text");
	
	if (((addrElm.val() === "" || addrElm.val().length < 3) && addrElm.is(":visible"))  
		 || !isOrderBigEnough) {
		$(".btn-update-order, .btn-make-order").prop("disabled", true).removeClass("btn-default");
	}
	else {
		$(".btn-update-order, .btn-make-order").prop("disabled", false).addClass("btn-default");
	}
}

function orderNumChange() {

	var clientType = Number($(".client-type").val());

	var currentOrderSize = 0;
	var currentOrderInitialSum = 0;
	let now = new Date();
	let orderDate = $(".order-date").data("full-date") ||
		`${now.getFullYear()}-${now.getMonth() + 1}-${now.getDate()}T${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;

	var orderSizesRaw = window.orderSizeData.find(t => t.clientType == clientType).orderSizes;

	var orderSizes = orderSizesRaw
		.filter(size => (size.dateStart <= orderDate && orderDate <= size.dateEnd)
			|| (size.dateStart <= orderDate && !size.dateEnd));

	var minOrderSizeSum = orderSizes.sort(t => t.from)[0].from;

	$(".book-order-num").each(function (i, e) {
		var elm = $(e);
		var currentVal = Number(elm.val().toString().replace(",", "."));
		currentOrderSize += currentVal * Number(elm.data("acting-amount"));
		currentOrderInitialSum += currentVal * Number(elm.data("price").toString().replace(",", "."));
	});

	if (!window.orderBounds)
		setInitial();
	$(".order-amount-num").text(currentOrderSize);
	$(".order-sum").text(Math.round(currentOrderInitialSum, 2));
	var orderSizeDiscount = orderSizes.find(size => size.from <= currentOrderInitialSum
		&& currentOrderInitialSum < size.to);

	var orderSizeDiscountPercent = 0;
	if (!!orderSizeDiscount)
		orderSizeDiscountPercent = orderSizeDiscount.discountPercent;

	$(".order-size-discount").text(orderSizeDiscountPercent + "%");

	let discounteSum = [];
	let eventdiscount = Number($("#event-discount-percent").val()) || 0;
	var actingDiscount = eventdiscount + orderSizeDiscountPercent;

	$(".order-num").each(function (i, e) {
		var elm = $(e);
		var currentVal = Number(elm.val());
		var price = elm.data("price").toString().replace(",", ".");
		if (currentVal > 0) {
			var price = elm.data("price").toString().replace(",", ".");
			let actingPrice = price;
			if (elm.hasClass("book-order-num")) {
				let discountedPrice = Math.floor((100 - actingDiscount) * price / 100);
				actingPrice = discountedPrice;
			}
			discounteSum.push(actingPrice * currentVal);

			elm.parents("tr").find(".item-price-with-discount").val(actingPrice);
			elm.parents("tr").find(".item-price-with-discount").text(actingPrice);
			elm.parents("tr").find(".item-total-with-discount").val(actingPrice * currentVal);
			elm.parents("tr").find(".item-total-with-discount").text(actingPrice * currentVal);
		}
		else {
			var initialPrice = Math.floor(price);
			elm.parents("tr").find(".item-price-with-discount").val(initialPrice);
			elm.parents("tr").find(".item-price-with-discount").text(initialPrice);
			elm.parents("tr").find(".item-total-with-discount").val(0);
			elm.parents("tr").find(".item-total-with-discount").text(0);
		}

		var weightSpanId = elm.prop("id").substr(1);
		var weight = currentVal * Number.parseFloat(elm.data("weight").toString().replace(",", "."));
		$("#w" + weightSpanId).text(weight.toFixed(1));

	});
	$(".order-sum").text(currentOrderInitialSum.toFixed(2).toLocaleString("ru-RU"));

	const reducer = (accumulator, currentValue) => accumulator + currentValue;
	let discounted = discounteSum.reduce(reducer);
	$(".order-sum-discounted").text(discounted.toFixed(2).toLocaleString("ru-RU"));

	if (currentOrderInitialSum < minOrderSizeSum) {
		isOrderBigEnough = false;
	//	$(".btn-update-order, .btn-make-order").prop("disabled", true).removeClass("btn-default");
	}
	else {
		isOrderBigEnough = true;
		///$(".btn-update-order, .btn-make-order").prop("disabled", false).addClass("btn-default");
	}
	addressChange();
}


function checkDate(querySelector, errorSelector) {
	let elem = $(querySelector);
	let orderDate = moment($(".order-date").data("full-date")).format('l');
	const rawValue = elem.val();
	if (!rawValue)
		return;

	let value = moment(rawValue).format('l');
	let errorMessage = $(errorSelector);
	const oDate = new Date(orderDate);
	const vDate = new Date(value);
	if (oDate > vDate) {
		elem.val("");
		errorMessage.show();
	}
	else {
		errorMessage.hide();
	}
}

function makeOrder(e) {
	let btn = $(e.target);
	if (!btn) {
		return;
	}
	btn.prop('disabled', true);
	const action = e.data.action;
	let values = [];

	if ($(".client").is(":visible") && $("#clientId").val() == "") {
		alert("так не пойдёт");
		return;
	}

	$.each($(".order-num"), function (i, obj) {
		var id = Number(obj.id.substr(1));
		values.push({ id: id, amount: Number($(obj).val()) });
	});
	const addrElm = $(".order-address-text");
	
	if (action === "add") {
		let error = false;
		if (addrElm.val() === "" || addrElm.val().length < 3) {
			$(".order-address-error").text("Необходимо внести адрес. Минимальная длина - три символа")
			addrElm.focus();
			error = true;
		}
		 
		if (error)
			return;
	}
	let clientOrder = null;
	let postUrl = "";
	let successUrl = "";
	let orderId = null;

	if (action === "add") {
		postUrl = window.urls.addOrder;
		clientOrder = {
			orderItems: values,
			clientId: $("#clientId").val(),
			eventId: $(".active-discount-event").val(),
			address: addrElm.val()
		}
	}
	else {
		postUrl = window.urls.updateProductsInOrder,
			orderId = $(".order-id").val();
		clientOrder = { orderItems: values, orderId: orderId };
	}

	$.ajax({
		url: postUrl,
		type: 'POST',
		contentType: 'application/json',
		data:
			JSON.stringify(clientOrder),
		success: function (order) {
			if (action === "add") {
				successUrl = window.urls.detailsOrder + "/" + order.orderId;
			}
			else {
				successUrl = `${window.urls.editOrder}?id=${orderId}`;
			}
			location.href = successUrl;
		}
	});
}


function setVisibility(showOrder) {
	if (!!showOrder) {
		$(".buttons").hide();
		$(".order-num").show();
		$(".order-amount").show();
		$(".order-address").show();
		$(".btn-make-order").show();
	}
	else {
		$(".buttons").show();
		$(".order-num").hide();
		$(".order-amount").hide();
		$(".order-address").hide();
		$(".btn-make-order").hide();
	}
}

$(function () {
	$(".btn-make-order").on("click", { action: 'add' }, makeOrder);
	$(".btn-update-order").on("click", { action: 'edit' }, makeOrder);

	$(".order-address-text").on("input", addressChange);

	let orderId = $(".order-id").val();
	if (window.user) {
		setVisibility(window.user.isClient || orderId);
	}
	if (orderId) {
		orderNumChange();
		$(".order-address").hide();
		$(".btn-make-order").hide();
	}
	$(".check-assembly-date").change(() => checkDate(".check-assembly-date", ".assembly-date-valid"));
	$(".check-done-date").change(() => checkDate(".check-done-date", ".done-date-valid"));
});

