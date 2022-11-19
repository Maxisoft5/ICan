$(function () {
	showSemiproductField('.field-name', 4);
	showSemiproductField('.field-cutLength', 2);
	showSemiproductField('.blockTypeId', 1);
	$('#semiproductTypeId').on('change', checkValue);
	checkValue();

	$(".add-semiproduct-paper").click(addSemiproductPaper);
	$(".isUniversal").change(function () {
		if (this.checked) {
			showProductList();
			$(".add-relationProduct").show();
		}
		else {
			deleteRelatedProducts();
			$(".add-relationProduct").hide();
		}
	});
	$(".add-relationProduct").click(function () {
		showProductList();
	});
	$('#productId').change(function () {
		showUniversalBlock($(this).children('option:selected').data("countryid"));
	});
	assignDelete();
});

function deleteRelatedProducts() {
	$(".product_list").empty();
}
 


function showProductList() {
	let existing = $(".existing-relatedProduct").toArray();
	let added = $(".added-product").toArray();
	let current = $(".hidden-product-id").val();
	let existingIds = [];
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push(Number($(item).val())));
	}
	if (added.length > 0) {
		added.forEach(item => existingIds.push(Number($(item).find("option:selected").val())));
	}
	$.ajax({
		url: window.urls.getAvailableProducts + "?current=" + current+ "&" + decodeURI($.param({ existingIds: existingIds })),
		success: function (data) {
			addRowWithProduct(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function addRowWithProduct(selectData) {
	if (selectData.length > 0) {
		let i = $(".semiproduct-product-relation").length;
		let options = selectData.map(item => `<option value=${item.value}>${item.text}</option>`).join("");
		let html = `
				 <div class='row semiproduct-relatedProducts'>
					<div class='col-10'>
						<select name='RelatedProducts[${i}].ProductId' class='added-product form-control' style='max-width:100%'>${options}</select>
					</div>
					<div class="col-2">
						<a href="#" class="delete-semiproduct-product">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".product_list").append(html);
		assignDelete();
		reorderProducts();
	}
	else {
		let html = `<span id="cant_add_products">Больше нет доступных вариантов </span>`;
		let check = $(".product_list").find('#cant_add_products');
		if (check.length === 0)
			$(".product_list").append(html);
	}
}


function addSemiproductPaper() {
	let existing = $(".existing-paper").toArray();
	let added = $(".added-paper").toArray();
	let existingIds = [];
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push(Number($(item).val())));
	}
	if (added.length > 0) {
		added.forEach(item => existingIds.push(Number($(item).find("option:selected").val())));
	}
	$.ajax({
		url: window.urls.getAvailablePapers + "?" + decodeURI($.param({ existingIds: existingIds })),
		success: function (data) {
			addRow(data);
			//$(".order-add-paperorder").show();
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function addRow(selectData) {
	if (selectData.length > 0) {
		let i = $(".semiproduct-paper").length;
		let options = selectData.map(item => `<option value=${item.value}>${item.text}</option>`).join("");
		let html = `
				 <div class='row semiproduct-paper'>
					<div class='col-8'>
						<select name='SemiproductPapers[${i}].PaperId' class='form-control added-paper' style='max-width:300px;'>${options}</select>
					</div>
					<div class="col-3">
						<a href="#" class="delete-semiproduct-paper">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".semiproduct-papers").append(html);
		assignDelete();
	}
	else {
		let html = `<span id="cant_add_semiproduct">Больше нет доступных вариантов </span>`;
		let check = $(".semiproduct-papers").find('#cant_add_semiproduct');
		if (check.length === 0)
			$(".semiproduct-papers").append(html);
	}
}

function assignDelete() {
	$(".delete-semiproduct-paper").off("click");
	$(".delete-semiproduct-paper").on("click", deletePaper);
	$(".delete-semiproduct-product").off("click");
	$(".delete-semiproduct-product").on("click", deleteProduct);
}

function deletePaper(e) {
	let elm = $(e.target);
	elm.parents(".semiproduct-paper").remove();
	$("#cant_add_semiproduct").remove();
	reorderPapers();
}

function deleteProduct(e) {
	let elm = $(e.target);
	elm.parents(".semiproduct-relatedProducts").remove();
	$("#cant_add_products").remove();
	reorderProducts();
}

function reorderProducts() {
	let existing = $(".existing-relatedProducts").length;
	$(".existing-relatedProduct").each((i, item) => $(item).prop("name", `RelatedProducts[${i}].ProductId`));
	$(".added-product").each((i, item) => $(item).prop("name", `RelatedProducts[${i + existing}].ProductId`));
}

function reorderPapers() {
	let existing = $(".existing-paper").length;
	$(".existing-paper").each((i, item) => $(item).prop("name", `SemiproductPapers[${i}].PaperId`));
	$(".existing-semiproduct-paper").each((i, item) => $(item).prop("name", `SemiproductPapers[${i}].SemiproductPaperId`));
	$(".added-paper").each((i, item) => $(item).prop("name", `SemiproductPapers[${i + existing}].PaperId`));
}


function checkValue() {
	$(".is-universal-block").hide();
	$(".field-name, .field-cutLength").hide().children("input");
	let value = $('#semiproductTypeId').val();	
	showSemiproductField('.blockTypeId', 1);
	if (value != 1) {		
		$("#BlockTypeId option[value='']").attr('selected', true);
	} 

	if (value == 4) {
		showSemiproductField('.field-name', 4);
		//changeSelect("onlyKit");
		$(".field-name").children("option.null-val").remove();
	}
	else {
		$(".field-name").append("<option value='' class='null-val'></option>");
		$(".field-name").children("option.null-val").attr('selected', 'selected');
	}
	if (value == 2) {
		showSemiproductField('.field-cutLength', 2);
		//changeSelect("all");
	}
	let countryId = $('#productId option:selected').data('countryid');
	showUniversalBlock(countryId);

	//if (value == 1 || value == 3) {
	//	changeSelect("withoutKit");
	//}
  
	//if (value == 1 || value == 3) {
	//	changeSelect("withoutKit");
	//}
}

function showUniversalBlock(countryId) {
	const semiproductType = $("#semiproductTypeId").val();
	if (!countryId&& semiproductType && semiproductType == 2 /*Наклейки*/ ) {
		$(".is-universal-block").show();
	}
	else {
		$(".is-universal-block").hide();
    }
}

function showSemiproductField(fieldClass, value) {
	let field = $(fieldClass);
	let fieldInput = field.children('input');
	if ($('#semiproductTypeId').val() == value) {
		field.show();
		fieldInput.prop('required', true);
	}
	else {
		field.hide();
		fieldInput.prop('required', false);
	}
}

function changeSelectOptions(data) {
	let selectedId  = $('#productId').val() ;
	let selectBlock = $('#productId').html('');
	let options = data.map(item => `<option  ${item.value == selectedId ? "selected" : ""} value=${item.value} ${item.countryId ? "data-countryId="+item.countryId : ''}>${item.text}</option>`).join("");
	selectBlock.append(options);
}
