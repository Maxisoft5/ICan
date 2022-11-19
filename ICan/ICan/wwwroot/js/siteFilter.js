$(function () {
	$(".add-filter-product").click(addFilterProduct);
	$(".edit-filter-product").click(editFilter);
});

function addFilterProduct(e) {
	e.preventDefault();
	let siteFilterId = $(".site-filter-id").val(); 
	$(".add-filter-product-modal .modal-body").load(window.urls.addSiteFilterProduct + "/" + siteFilterId,
		function() {
			$(".add-filter-product-modal_save").click(function () {
				$("form.add-filter-product-form").submit();
			});
			$(".add-filter-product-modal").modal();
		});
}

function editFilter(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let siteFilterProductId = elm.data("filter-product-id"); 
	let url = window.urls.editSiteFilterProduct + "?siteFilterProductId=" + siteFilterProductId;
	loadUniversalForm(url,"Редактировие тетрди в фильтре")
}
