$(function () {
	$(".add-incoming").click(addIncoming);
	$(".delete-order-incoming").click(deleteIncoming);
	$(".spring-order-payment-save").on("click", savePayment);
	$(document).on("click", ".delete-payment", deletePayment);
});

function deletePayment(e) {
	if (!confirm("Вы уверены, что хотите удалить эту запись:"))
		return;

	let elm = $(e.target);
	let springOrderPaymentId = elm.parents("button").data("spring-order-payment-id");
	$.ajax({
		url: window.urls.deleteSpringOrderPayment,
		type: 'DELETE',
		data: { paymentId: springOrderPaymentId },
		success: function () {
			elm.parents("tr").remove();
			if ($(".payments-table tbody tr").length == 0) {
				$(".payments-table tbody").append(`<tr class="empty-row"><td colspan="3" style="text-align:center">Нет данных для отображения</td></tr>`);
			};
			alert("Платеж удалён");
		},
		error: function (data) {
			alert("Невозможно удалить запись");
		}
	});
}

function savePayment(e) {
	e.preventDefault();
	let dataContainer = $(".add-spring-order-payment-container");
	let date = $("#Date", dataContainer).val();
	let amount = $("#Amount", dataContainer).val();
	if (!date || !amount) {
		alert("Некорректные данные");
		return;
	}
	let springOrderId = $("#OrderId", dataContainer).val();

	$.ajax({
		url: window.urls.addSpringOrderPayment,
		type: 'POST',
		data: { amount: amount, date: date, springOrderId: springOrderId },
		success: function (paymentModel) {
			$("#addPayment").modal("hide");
			$("tbody", ".payments-table").append(getNewRow(paymentModel));
			$(".empty-row", ".payments-table").remove();
		},
		error: function (data) {
			alert("Невозможно сохранить информацию");
		}
	});
}

function addIncoming(e) {
	e.preventDefault();
	let paperOrderId = $("#SpringOrderId").val();
	let url = window.urls.addSpringOrderIncoming + "/" + paperOrderId;
	let title = "Добавление прихода";
	loadUniversalForm(url, title, function(){
		$(".spool-amount").keyup(calculateNumberOfTurns);
	});
}

function calculateNumberOfTurns(elem) {
	let springNumerOfTurns = $(".number-of-turns").val();
	let spoolAmount = $(this).val();
	$(".number-of-turns-count").val(spoolAmount * springNumerOfTurns); 
}

function deleteIncoming(e) {
	if (!confirm("Вы уверены, что хотите удалить приход?"))
		return;

	e.preventDefault();
	let elm = $(findTarget(e));
	let url = window.urls.deleteSpringOrderIncoming + "?incomingId=" + elm.data("spring-order-incoming-id");

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

function getNewRow(model) {
	let row = [`<tr data-amount="${model.amount}">`];
	row.push(`<td>${model.displayDate}</td>`);
	row.push(`<td>${model.displayAmount}</td>`);
	row.push(`<td>`);
	console.log(model.paymentId);
	if (user.isAdmin || user.isAssembler)
		row.push(`<button data-spring-order-payment-id="${model.paymentId}" class="btn btn-link delete-payment">
					<i class="far fa-times-circle "></i>
				</button>`);
	row.push(`</td>`);
	row.push("</tr>");
	return row.join();
}

