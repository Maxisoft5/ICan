$(
	function () {
		assignHandlers();
	}
);

function assignHandlers() {
	$(".add-product-url").off("click");
	$(".add-product-url").on("click", addRow);
	$(".delete-product-url").off("click");
	$(".delete-product-url").on("click", deleteRow);
}

function deleteRow(e) {
	e.preventDefault();
	const elm = $(recurseFindTarget(e.target));
	elm.parents(".product-url").remove();
	reorderRows();
}

function addRow() {
	const total = $(".product-url").length;
	$(".product-urls").append(`<div class="row product-url">
								<div class="col-10">									 
									<input name="Urls[${total}].Url"
										   class="form-control product-url-item-link"
										   />
								</div>
								<div class="col-2">
									<a href="#" class="delete-product-url">
										<i class="index far fa-times-circle" style="color:rgb(0, 123, 255)"></i>
									</a>
								</div>
							</div>`);
	assignHandlers();
}


function reorderRows() {
	$(".product-url").each((i, item) => {
		$(".product-url-item-id", item).prop("name", `Urls[${i}].MarketplaceProductUrlId`);
		$(".product-url-item-link", item).prop("name", `Urls[${i}].Url`);
	});
}