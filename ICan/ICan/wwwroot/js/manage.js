$(document).ready(function () {
	$('.add-shop').click(function () {
        getAndShowShops();
	});
	$('.unbind-shop').click(function (elem) {
		elem.preventDefault();
		unbindShop(this);
	});
});

function unbindShop(elem) {
	let shopId = $(elem).data("shopid");
	let userId = $("#Id").val();
	$.ajax({
		url: window.urls.unbindShop,
		data: { shopId: shopId, userId: userId },
		success: function () {
			$(elem).parents(".form-group").first().remove();
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function getAndShowShops() {
	let existingIds = [];
	let added = $(".added-shop").toArray();
	if (added.length > 0) {
		added.forEach(item => existingIds.push($(item).find("option:selected").val()));
	}
	let existing = $(".existing-shop").toArray();
	if (existing.length > 0) {
		existing.forEach(item => existingIds.push($(item).val()));
	}
	$.ajax({
		url: window.urls.getShops,
		data: { existingIds: existingIds.join() },
		success: function (data) {
			addRow(data);
		},
		error: function (data) {
			console.log(data.error);
		}
	});
}

function addRow(selectData) {
	if (selectData.length > 0) {
		let i = $(".clientShop").length;
		let options = selectData.map(item => `<option value=${item.value}>${item.text}</option>`).join("");
		let html = `
				 <div class='row clientShop' style='margin-top:1rem'>
					<div class='col-11'>
						<select name='ShopIds[${i}]' class='form-control added-shop'>${options}</select>
					</div>
					<div class="col-1" style="display:flexl;align-items:center;">
						<a href="#" class="delete-shop">
							<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
						</a>
					</div>
				</div>`;
		$(".shopList").append(html);
		assignShopControls();
	}
	else {
		let html = `<span id="cant_add_shop">Больше нет доступных для добавления магазинов</span>`;
		let check = $(".shopList").find('#cant_add_shop');
		if (check.length === 0)
			$(".shopList").append(html);
	}
}

function assignShopControls() {
	$(".delete-shop").off("click");
	$(".delete-shop").on("click", deleteShop);
}

function deleteShop(e) {
	e.preventDefault();
	let elm = $(e.target);
	elm.parents(".clientShop").remove();
	$("#cant_add_semiproduct").remove();
}