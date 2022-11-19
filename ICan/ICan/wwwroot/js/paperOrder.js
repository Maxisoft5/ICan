$(function () {
	$("#SheetCount").on("change", function (e) {
		calcPaperOrderSheetPrice(e);
		calcPaperOrderWeight();
	});
	$("#Weight").on("change", calcPaperOrderSheetCount);
	$("#paperId").on("change", calcPaperOrderWeight);
	$("#OrderSum").on("change", calcPaperOrderSheetPrice);
	$("#IsPaid").on("change", setInitialPaidSum);

	$("#InvoiceDate").on("change", { suppliers: '@Html.Raw(ViewData["Suppliers"])' }, calcPaperOrderPaymentDate);
	$("#supplierCounterPartyId").on("change", { suppliers: '@Html.Raw(ViewData["Suppliers"])' }, calcPaperOrderPaymentDate);
	$(".delete-order-income").on("click", deleteIncoming);
	$(".add-paper-order-incoming").click(addIncoming);
});

function deleteIncoming(e) {
	if (!confirm("Вы уверены, что хотите удалить приход?"))
		return;

	e.preventDefault();
	let elm = $(findTarget(e));
	let url = window.urls.deletePaperOrderIncoming + "?id=" + elm.data("paper-order-incoming-id");
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

function addIncoming(e) {
	e.preventDefault();
	let paperOrderId = $("#PaperOrderId").val();
	$(".add-incoming-modal .modal-body").load(window.urls.addPaperOrderIncoming + "/" + paperOrderId,
		function () {
			$(".add-incoming-modal_save").click(function () {
				$("form.add-incoming-form").submit();
			});
			$(".add-incoming-modal").modal();
		});
}


function calcPaperOrderSheetPrice(e) {
	let orderSum = $("#OrderSum").val() || 0;
	let sheetCount = $("#SheetCount").val();
	if (sheetCount && sheetCount > 0) {
		$("#SheetPrice").val(Math.round(orderSum / sheetCount * 100) / 100);
	}
}

function calcPaperOrderPaymentDate(e) {
	let invoiceDateRaw = $("#InvoiceDate").val();
	let suppId = $("#supplierCounterPartyId").val();
	let suppliers = JSON.parse(e.data.suppliers);
	if (invoiceDateRaw) {
		if (suppId) {
			let invoiceDate = new Date(invoiceDateRaw);
			let paymentDelay = suppliers.find(x => x.CounterpartyId == suppId).PaymentDelay || 0;
			let calcDateTicks = invoiceDate.setDate(invoiceDate.getDate() + paymentDelay);
			let calcDate = new Date(calcDateTicks);
			let calcDateString = `${calcDate.getFullYear()}-${("0" + (calcDate.getMonth() + 1)).slice(-2)}-${("0" + calcDate.getDate()).slice(-2)}`;
			$("#PaymentDate").val(calcDateString);
		}
		else {
			$("#PaymentDate").val(invoiceDateRaw);
		}
		return;
	}
	$("#PaymentDate").val("");
}

function setInitialPaidSum(e) {
	let orderSum = $("#OrderSum").val() || 0;
	let paidSumControl = $("#PaidSum");
	let isPaidControl = $("#IsPaid");
	if (isPaidControl.is(":checked")) {
		let paidSum = paidSumControl.val();
		if (!paidSum || paidSum == 0) {
			paidSumControl.val(orderSum);
		}
	}
	else {
		paidSumControl.val(0);
	}
}

function calcPaperOrderWeight() {
	let sheetCount = $("#SheetCount").val();
	let paperId = $("#paperId").val();
	$.ajax({
		url: window.urls.getPaperOrderWeight,
		data: { sheetCount: sheetCount, paperId: paperId },
		success: function (data) {
			$("#Weight").val(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function calcPaperOrderSheetCount() {
	let weight = $("#Weight").val();
	let paperId = $("#paperId").val();
	$.ajax({
		url: window.urls.getPaperOrderSheetCount,
		data: { weight: weight, paperId: paperId },
		success: function (data) {
			$("#SheetCount").val(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}