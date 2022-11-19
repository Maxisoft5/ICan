$(document).ready(function () {
	$(".add-stickers").click(loadPrintOrders);
	$(".delete-notch-order-item").on("click", deleteNotchOrderItem);
	$(".add-notch-order-incoming").click(addNotchOrderIncoming);
	$(".delete-notch-incoming").on("click", deleteIncoming);
	$(".is-assembled-controll").change(function (elem) {
		let value = $(elem.target).val();
		if (value === "false") {
			$(elem.target).val("true");
		}
		else {
			$(elem.target).val("false");
		}
	});
	assignDelete();
});

function loadPrintOrders() {
	let existingIds = [];
	let existing = $(".existing-print-order-id").toArray();
	let added = $(".added-print-order-id").toArray();
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push($(item).val()));
	}
	if (added.length > 0) {
		added.forEach(item => existingIds.push($(item).find("option:selected").val()));
	}
	const currenNotchOrderId = $(".notchorder-id").val();
	let url = window.urls.getPrintOrdersWithStickers;
	if (currenNotchOrderId) {
		url = `${url}?currentNotchOrderId=${currenNotchOrderId}`;
	}
	$("#loading").show();
	$(".add-stickers").off("click");
	$.ajax({
		url: url,
		data: { existingIds: existingIds.join() },
		success: function (data) {
			showListPrintOrders(data);
		},
		error: function (data) {
			console.log(data.error);
		},
		complete: function (data) {
			$("#loading").hide();
			$(".add-stickers").click(loadPrintOrders);
		}

	});
}

function showListPrintOrders(data) {
	if (data.length > 0) {
		let i = $(".notch-order-print-order").length;
		let options = data.map(item => `<option value=${item.id}>${item.name}</option>`).join("");
		let html = `
				 <div class='row notch-order-print-order'>
					<div class='col-11'>
						<select name='NotchOrderItems[${i}].PrintOrderId' class='form-control added-print-order-id'>${options}</select>
					</div>
					<div class="col-1 center-items">
						<a href="#" class="delete-notch-order-print-order-id">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".added-notch-order-stickers").append(html);
		assignDelete();
	}
	else {
		let html = `<span id="cant_add_notchOrderSemiproduct">Больше нет доступных для добавления заказов печати с наклейками</span>`;
		let check = $(".common-list-notch-order-stickers").find('#cant_add_notchOrderSemiproduct');
		if (check.length === 0)
			$(".common-list-notch-order-stickers").append(html);
	}
}

function assignDelete() {
	$(".delete-notch-order-print-order-id").off("click");
	$(".delete-notch-order-print-order-id").on("click", deleteNotchOrderItem);
}

function deleteNotchOrderItem(e) {
	let elem = $(e.target);
	elem.parents(".notch-order-print-order").remove();
	$("#cant_add_notchOrderSemiproduct").remove();
	reorderItems();
}

function reorderItems() {
	$(".notch-order-print-order")
		.each((i, item) => {
			$(".notch-order-item-id", item).prop("name", `NotchOrderItems[${i}].NotchOrderItemId`)
			$(".print-order-id", item).prop("name", `NotchOrderItems[${i}].PrintOrderId`)
			$(".added-print-order-id", item).prop("name", `NotchOrderItems[${i}].PrintOrderId`)
		});
}

function addNotchOrderIncoming(e) {
	e.preventDefault();
	let notchOrderId = $("#NotchOrderId").val();
	$(".add-incoming-modal .modal-body").load(window.urls.addNotchOrderIncoming + "/" + notchOrderId,
		function () {
			$(".add-incoming-modal_save").click(function () {
				$("form.add-incoming-form").submit();
			});
			$(".add-incoming-modal").modal();
		});
}

function deleteIncoming(e) {
	if (!confirm("Вы уверены, что хотите удалить приход?"))
		return;

	e.preventDefault();
	let elm = $(findTarget(e));
	let url = window.urls.deleteNotchOrderIncoming + "?id=" + elm.data("notch-order-incoming-id");
	$.ajax(
		{
			url: url,
			method: "POST"
		})
		.fail(function (data) {
			alert("При удалении возникла ошибка");
			console.log(data);
		})
		.done(function () {
			alert("Успешно удалено");
			window.location.reload();
		});
}
