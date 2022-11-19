function deleteSemiproduct(e) {
	e.preventDefault();
	let elm = $(e.target);
	elm.parents(".order-semiproduct").remove();
	$("#cant_add_semiproduct").remove();
	reorderSemiproducts();
	reuseSemiproductInfo();
	if ($(".order-semiproduct").length === 0) {
		$(".semiproductTypeList").prop("disabled", false);
	}
}

function reorderSemiproducts() {
	let existing = $(".existing-semiproduct").length;
	$(".existing-printorder-semiproduct").each((i, item) => $(item).prop("name", `PrintOrderSemiproducts[${i}].PrintOrderSemiproductId`));
	$(".existing-semiproduct").each((i, item) => $(item).prop("name", `PrintOrderSemiproducts[${i}].SemiproductId`));
	$(".existing-semiproduct-assembled").each((i, item) => $(item).prop("name", `PrintOrderSemiproducts[${i}].IsAssembled`));
	$(".added-semiproduct").each((i, item) => $(item).prop("name", `PrintOrderSemiproducts[${i + existing}].SemiproductId`));
}

function reuseSemiproductInfo() {
	let semiProducts = $(".order-semiproduct");
	if (semiProducts.length == 0) {
		$(".order-add-paperorder").hide();
		$(".paper-name, .haveWDVarnish, .haveStochastics").val("");
	}
	else {
		$(".order-add-paperorder").show();
		getPaperInfo();
	}
}

function assignSemiproductControls() {
	$(".delete-order-semiproduct").off("click");
	$(".delete-order-semiproduct").on("click", deleteSemiproduct);

	//$(".disassemble-printorder-semiproduct").off("click");
	//$(".disassemble-printorder-semiproduct").on("click", disa);

	//$(".assemble-printorder-semiproduc").off("click");
	//$(".assemble-printorder-semiproduc").on("click", deleteSemiproduct);
	//disassemble-printorder-semiproduct
}

function addSemiproduct() {
	let existing = $(".existing-semiproduct").toArray();
	let added = $(".added-semiproduct").toArray();
	let semiproductTypeId = $(".semiproductTypeList").val();
	let existingIds = [];
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push($(item).val()));
	}
	if (added.length > 0) {
		added.forEach(item => existingIds.push($(item).find("option:selected").val()));
	}
	$.ajax({
		url: window.urls.getAvailableSemiproducts,
		data: { existingIds: existingIds.join(), semiproductTypeId: semiproductTypeId },
		success: function (data) {
			addRow(data);
			getPaperInfo(data[0].value);
			$(".order-add-paperorder").show();
			$('.semiproductTypeList').attr('disabled', true);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function addSecondPartOfBox(elem) {
	let semiproductTypeId = $(elem).children('option:selected').data('typeid');
	let semiproductId = $(elem).children('option:selected').val();
	if (semiproductTypeId === 4) {
		$.ajax({
			url: window.urls.getSecondPartOfBox,
			data: { semiproductId: semiproductId },
			success: function (data) {
				addRow(data);
			},
			error: function (data) {
				console.log(data.error);
			}
		});
	}
}

function getPaperInfo() {
	let semiproductIds = [];
	$(".added-semiproduct, .existing-semiproduct").each((i, item) => semiproductIds.push($(item).val()));
	$.ajax({
		url: window.urls.getPaperName,
		data: { semiproductIds: semiproductIds },
		success: function (data) {
			setPaperInfo(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function setPaperInfo(data) {
	$(".paper-name").val(data.paperNames);

	if (data.haveWDVarnish)
		$(".haveWDVarnish").val("С ВД лаком");
	else
		$(".haveWDVarnish").val("Без ВД лака");

	if (data.haveStochastics)
		$(".haveStochastics").val("Да");
	else
		$(".haveStochastics").val("Нет");
}

function addRow(selectData) {
	if (selectData.length > 0) {
		let i = $(".order-semiproduct").length;
		let options = selectData.map(item => `<option value=${item.value} data-typeId=${item.typeId}>${item.text}</option>`).join("");
		let html = `
				 <div class='row order-semiproduct'>
					<div class='col-11'>
						<select name='PrintOrderSemiproducts[${i}].SemiproductId' class='form-control added-semiproduct'>${options}</select>
					</div>
					<div class="col-1">
						<a href="#" class="delete-order-semiproduct">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".order-semiproducts").append(html);
		assignSemiproductControls();
	}
	else {
		let html = `<span id="cant_add_semiproduct">Больше нет доступных для добавления полуфабрикатов</span>`;
		let check = $(".order-semiproducts").find('#cant_add_semiproduct');
		if (check.length === 0)
			$(".order-semiproducts").append(html);
	}
}

//=====paperOrder
function deletePaperOrder(e) {
	e.preventDefault();
	let elm = $(e.target);
	elm.parents(".order-paperorder").remove();
	reorderPaperOrders();
	$("#cant_add_paperOrder").remove();
}

function reorderPaperOrders() {
	let existing = $(".existing-order-paperorder").length;
	$(".existing-order-printpaperorder").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i}].PrintOrderPaperId`));
	$(".existing-order-paperorder").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i}].PaperOrderId`));
	$(".existing-order-paperorder-sheets").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i}].SheetsTakenAmount`));
	$(".existing-order-printpaperorder-issent").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i}].IsSent`));
	$(".added-order-paperorder").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i + existing}].PaperOrderId`));
	$(".added-order-paperorder-sheets").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i + existing}].SheetsTakenAmount`));
	$(".added-order-paperorder-issent").each((i, item) => $(item).prop("name", `PrintOrderPapers[${i + existing}].IsSent`));
}

function assignPaperOrderHandlers() {
	$(".delete-order-paperorder").off("click");
	$(".delete-order-paperorder").on("click", deletePaperOrder);
	$(".added-order-paperorder-sheets").off('input');
	$(".added-order-paperorder-sheets").on('input', calcPaperDiff);

	setCheckBoxHanders("added-order-paperorder-fake-issent", "added-order-paperorder-issent");
	$(".added-order-paperorder-fake-issent").change();
}

function addPaperOrder(e) {
	e.preventDefault();
	let existing = $(".existing-order-paperorder").toArray();
	let added = $(".added-order-paperorder").toArray();

	let semiProduct = $(".added-semiproduct, .existing-semiproduct");
	let semiProductId = 0;
	if (semiProduct.length > 0) {
		semiProductId = semiProduct.val();
	}
	else {
		alert("Необходимо добавить полуфабрикат");
		return;
	}

	let existingIds = [];
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push($(item).val()));
	}
	if (added.length > 0) {
		added.forEach(item => existingIds.push($(item).find("option:selected").val()));
	}

	$.ajax({
		url: window.urls.getAvailablePaperOrders,
		data: { existingIds: existingIds.join(), semiProductId: semiProductId },
		success: function (data) {
			addPaperOrderRow(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function addPaperOrderRow(selectData) {
	if (selectData.length > 0) {
		let i = $(".order-paperorder").length;
		let options = selectData.map(item => `<option value=${item.value}>${item.text}</option>`).join("");
		let html = `
				 <div class='row order-paperorder'>
					<div class="col-1">
						<input type="checkbox"
								class="added-order-paperorder-fake-issent"
								  />
						<input type="hidden"
								 name='PrintOrderPapers[${i}].IsSent'
									class="added-order-paperorder-issent"
							  />
					</div>

					<div class='col-10 form-inline' style="justify-content: space-between;">
						<select name='PrintOrderPapers[${i}].PaperOrderId' style="font-size:small; " 
							class='form-control added-order-paperorder'>${options}</select>
				 
						<input type="number"
								name='PrintOrderPapers[${i}].SheetsTakenAmount' 
								class="form-control added-order-paperorder-sheets"
								style="font-size:smaller; width: 75px;float:right;height:30px"
								value="0" />
					</div>
					<div class="col-1">
						<a href="#" class="delete-order-paperorder">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".order-paperorders").append(html);
		assignPaperOrderHandlers();
	}
	else {
		if ($(".order-paperorders").find("#cant_add_paperOrder").length == 0) {
			let html = `<span id="cant_add_paperOrder">Больше нет доступных заказов бумаги!</span>`;
			$(".order-paperorders").append(html);
		}
	}
}
//=====end paperOrder

function deleteOrderIncome(e) {
	if (!confirm("Вы уверены, что хотите удалить приход?"))
		return;
	e.preventDefault();
	let elm = $(findTarget(e));

	let url = window.urls.deleteIncoming + "?id=" + elm.data("print-order-incoming-id");
	$.ajax(
		{
			url: url,
			method: "POST"
		})
		.fail(function (data) {
			if (data.responseText) {
				$(".alert-danger").html(data.responseText);
				$(".alert-danger").show();
			}
			else {
				alert("При удалении возникла ошибка");
			};
		})
		.done(
			function () {
				alert("Успешно удалено");
				elm.parents(".order-income").remove();
				document.location.reload();
			});
}

function showHideAddSemiproduct() {
	const incomeCount = $(".row.order-income").length;
	if (incomeCount > 0) {
		$(".add-semiproduct").hide();
	}
	else {
		$(".add-semiproduct").show();
	}
}

function enableAmountControls(e) {
	e.preventDefault();
	const selectedVal = $(".incoming-type").val();
	if (selectedVal == 1 /*Ordinary*/) {
		$(".amount-check", ".add-incoming-form").removeAttr("disabled");
	}
	else {
		$(".amount-check", ".add-incoming-form").attr("disabled", "disabled");
		$(".amount-check[data-exists='true']", ".add-incoming-form").removeAttr("disabled");
	}
}

function getNewRow(model) {
	let row = [`<tr data-amount="${model.amount}">`];
	row.push(`<td>${model.displayDate}</td>`);
	row.push(`<td>${model.displayAmount}</td>`);
	row.push(`<td>`);
	if (user.isAdmin || user.isAssembler)
		row.push(`<button data-print-order-payment-id="${model.printOrderPaymentId}" class="btn btn-link delete-print-order-payment">
					<i class="far fa-times-circle "></i>
				</button>`);
	row.push(`</td>`);
	row.push("</tr>");
	return row.join();
}

$(function () {
	$(document).on('change', '.added-semiproduct', function () { addSecondPartOfBox(this) });
	$(".add-semiproduct").click(addSemiproduct);
	$(".paper-planned-expense, .existing-order-paperorder-sheets").on('input', calcPaperDiff);
	$(".order-add-paperorder").click(addPaperOrder);
	assignSemiproductControls();
	assignPaperOrderHandlers();
	$(".delete-order-income").click(deleteOrderIncome);
	showHideAddSemiproduct();

	$(".add-incoming").on("click", function (elm) {
		elm.preventDefault();
		let printOrderId = $("#PrintOrderId").val();
		$(".add-incoming-modal .modal-body").load(window.urls.addIncoming + "/" + printOrderId,
			function () {
				$(".incoming-type").off("click");
				$(".incoming-type").on("click", enableAmountControls);


				$(".add-incoming-modal_save").click(saveIncoming);
				$(".add-incoming-modal").modal();
			});
	});

	$(".print-order-payment-save").on("click", function (e) {
		e.preventDefault();
		let dataContainer = $(".add-print-order-payment-container");
		let date = $("#Date", dataContainer).val();
		let amount = $("#Amount", dataContainer).val();
		if (!date || !amount) {
			alert("Некорректные данные");
			return;
		}
		let printOrderId = $("#PrintOrderId", dataContainer).val();

		$.ajax({
			url: window.urls.addPrintOrderPayment,
			type: 'POST',
			data: { amount: amount, date: date, printOrderId: printOrderId },
			success: function (paymentModel) {
				$("#addPrintOrderPayment").modal("hide");
				$("tbody", ".print-order-payments").append(getNewRow(paymentModel));
				$(".empty-row", ".print-order-payments").remove();
				assignDeletePayment();
			},
			error: function (data) {
				alert("Невозможно сохранить информацию");
			}
		});
	});

	assignDeletePayment();
	calcPaperDiff();
	$(document).on("keypress", ".amount-check", function (event) {
		return event.charCode >= 48 && event.charCode <= 57;
	});

	$(document).ready(function () {
		$("input.existing-order-fake-issent:checkbox").click(function () {
			if ($(this).is(':checked') == true) {
				let printOrderPaper = $(this).parent().next().children()[0];
				$.ajax({
					url: "/opt/MovePaper/CreateAutoMovePaper",
					method: "POST",
					data: {
						"printOrderPaperId": printOrderPaper.value
					}
				});
			}
			else if ($(this).is(':checked') == false) {
				let printOrderPaper = $(this).parent().next().children()[0];
				$('#savePrintOrderSaving').click(function () {
					swal({
						title: "Подтвердить удаление?",
						text: "Убрав отметки с отправки заказов, вы удалите перемещения бумаг для заказов!",
						icon: "warning",
						buttons: true,
						dangerMode: true,
					})
						.then((willDelete) => {
							if (willDelete) {
								$.ajax({
									url: "/opt/MovePaper/DeleteAutoMovePaper",
									method: "DELETE",
									data: {
										"printOrderPaperId": printOrderPaper.value
									},
									success: 
										swal("Перемещения бумаг удалены!", {
											icon: "success",
										})
								});
							
							} else {
								swal("Заказы на перемещения бумаг оставлены");
							}
					});
				});
			}
		});
	});

	setCheckBoxHanders("existing-order-fake-issent", "existing-order-printpaperorder-issent");
	setCheckBoxHanders("added-order-paperorder-fake-issent", "added-order-paperorder-issent");
	setCheckBoxHanders("existing-semiproduct-fake-assembled", "existing-semiproduct-assembled");
});


function setCheckBoxHanders(fakeElmClass, targetElmClass) {
	$(`.${fakeElmClass}`).off("change");
	$(`.${fakeElmClass}`).change(function (e) {
		let checked = $(e.target).is(":checked");
		$(e.target).siblings(`.${targetElmClass}`).val(checked);
	});
}

function calcPaperDiff(e) {
	const diffResultElm = $(".paper-expense-diff");
	let planned = Number($(".paper-planned-expense").val());
	let taken = 0;
	$(".existing-order-paperorder-sheets").each((_, item) => taken += Number($(item).val()));
	$(".added-order-paperorder-sheets").each((_, item) => taken += Number($(item).val()));
	diffResultElm.html(planned - taken);
}

function assignDeletePayment() {
	$(".delete-print-order-payment").on("click", deletePayment);
};

function deletePayment(e) {
	if (!confirm("Вы уверены, что хотите удалить эту запись:"))
		return;

	let elm = $(e.target);
	let printOrderPaymentId = elm.parents("button").data("printOrderPaymentId");
	$.ajax({
		url: window.urls.deletePrintOrderPayment,
		type: 'DELETE',
		data: { "printOrderPaymentId": printOrderPaymentId },
		success: function () {
			elm.parents("tr").remove();
			if ($(".print-order-payments tbody tr").length == 0) {
				$(".print-order-payments tbody").append(`<tr class="empty-row"><td colspan="3" style="text-align:center">Нет данных для отображения</td></tr>`);
			}
			clearIsPaid();
		},
		error: function (data) {
			alert("Невозможно удалить запись");
		}
	});
}

function clearIsPaid() {
	let payments = $("tr:not(.empty-row)", ".print-order-payments > tbody");
	let sum = 0;
	payments.each((i, item) => sum += Number($(item).data("amount")
		.replace(",", ".")));
	let orderSum = $("#orderSum").val();
	if (sum < orderSum) {
		$("#isPaid").checked = false;
		$("#isPaid").val(false);
		$("#isPaid").removeAttr("checked")
	}
}

function saveIncoming() {
	// set 0 for disabled 
	let semiprodsOverPrinting = '';
	let printing = parseInt($(".printing-amount").val());

	$(".amount-check").each(function () {
		let currentIncomeAmount = parseInt($(this).val());
		if (currentIncomeAmount > 0) {
			const currentIncomingType = $(".incoming-type").val();
			let cuurentMultiplier = currentIncomingType == 3 /*Fla*/ ? -1 : 1;

			let totalSum = cuurentMultiplier * currentIncomeAmount;
			let parentBlock = $(this).parents(".order-income");
			let poSemiprodId = $(".printorder-semiproduct-id", parentBlock).val();
			let poSemiprodTypeId = $(".printorder-semiproduct-type-id", parentBlock).val();

			let amountFromIncome = $(`.printOrderSemiproductId-${poSemiprodId} .print-order-incoming-amount`);
			if (amountFromIncome.length > 0) {
				amountFromIncome.each(function () {
					const incomingType = parseInt($(this).data("printOrderIncomingType"));
					let multiplier = incomingType == 3 /*Flaw*/ ? -1 : 1;

					totalSum += multiplier * parseInt($(this).data("amount"));
				});
			}
			let poSemiprodName = $(".semiproduct-name", parentBlock).text();
			if (poSemiprodTypeId != 5 /*Cursor*/ && totalSum > printing) {
				semiprodsOverPrinting += poSemiprodName + "\n";
			}
			// there can be up to (printing * 10) in one order and it is not over printing 
			if (poSemiprodTypeId == 5 /*Cursor*/ && totalSum > printing * 10) {
				semiprodsOverPrinting += poSemiprodName + "\n";
			}

		}
	});

	if (semiprodsOverPrinting.length > 0)
		if (!confirm(semiprodsOverPrinting + "превышают тираж, продолжить?"))
			return;
	$("form.add-incoming-form").submit();
}