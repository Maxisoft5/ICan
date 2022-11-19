$(function () {
	changeIsKitDisabled();
	$("#warehouseActionTypeId").change(changeIsKitDisabled);
	$(".form-warehouse").submit(setEmptyValues);
	$(".wh-item").change(changeNotebookAmount);
});

function changeNotebookAmount(e) {
	const target = $(e.target) ;
	const elm = target[0];
	const actionTypeId = $("#warehouseActionTypeId").val();
	if (actionTypeId == 8 /*single inventory*/) {
		const needToDisable = (target.val() > 0);
		if (needToDisable) {
			$(".wh-item").each((i, item) => {
				if (item != elm) {
					$(item).attr("disabled", "disabled");
				}
			});
		}
		else {
			$(".wh-item").each((i, item) => $(item).removeAttr("disabled"));
		}
	}
}


function changeIsKitDisabled(e) {
	let newActionTypeIdVal = $("#warehouseActionTypeId").val();
	let isArrival = Number(newActionTypeIdVal) === 1;//arrival
	let onlyKit = Number(newActionTypeIdVal) === 7;//kitassembly
 
	$("p.only-one-product").hide();

	if (isArrival) {
		$("input[data-assembles-as-kit]").prop("disabled", "disabled");
		$(".row.assembly").show();
	}
	else {
		$(".row.assembly").hide();
		$(".row.assembly").find("select").prop('selectedIndex', 0);
		if (onlyKit) {
			$("input[data-assembles-as-kit='False']").prop("disabled", "disabled");
			$("input[data-assembles-as-kit='True']").removeAttr("disabled");
			$("p.only-one-product").show();
		}
		else {
			$("input.wh-item").removeAttr("disabled");
		}
	}
}
