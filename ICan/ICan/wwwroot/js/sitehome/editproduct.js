const addTagBtn = $('.add-tag');
const deleteTagBtn = $('.delete-tag');
const addImageBtn = $('.add-image');
const deleteImageBtn = $('.delete-image');
const copyImageBtn = $('.copy-image-path');
const editMarketplaceLnk = $('.edit-product-marketplace');
const addMarketplaceLnk = $('.add-product-marketplace');
const deleteMarketplaceLnk = $('.delete-product-marketplace');
const deleteVideoLnk = $('.delete-video');

$(
	function () {
		tinymce.init(
			{
				selector: '.product-description, .product-information, .product-reviews-text',
				plugins: ['autolink lists link preview textcolor colorpicker image code paste'],
				menubar: false,
				relative_urls: false,
				toolbar: ' undo redo |  formatselect | bold italic backcolor forecolor | alignleft aligncenter alignright alignjustify | bullist numlist image | removeformat | preview code',
			});
		tinymce.init(
			{
				selector: '.product-longdescription',
				plugins: ['autolink lists link preview textcolor colorpicker image code paste'],
				menubar: false,
				width: 1500,
				relative_urls: false,
				toolbar: ' undo redo |  formatselect | bold italic backcolor forecolor | alignleft aligncenter alignright alignjustify | bullist numlist image | removeformat | preview code',
			});
		addTagBtn.on("click", addTag);
		deleteTagBtn.on("click", deleteTag);

		addImageBtn.on("click", addImage);
		deleteImageBtn.on("click", deleteImage);
		copyImageBtn.on("click", copyImagePath);
		addMarketplaceLnk.on("click", { action: 'add' }, manageMarketplace);
		editMarketplaceLnk.on("click", { action: 'edit' }, manageMarketplace);
		deleteMarketplaceLnk.on("click", deleteMarketplace);
		const hash = window.location.hash.substr(1);
		$(`#nav-tab a[href="#${hash}"]`).tab('show');
		deleteVideoLnk.on("click", deleteVideoFile);
	}
);

function addImage(e) {
	e.preventDefault();
	let productId = $("#ProductId").val();
	let url = window.urls.addSiteProductImage + "?productId=" + productId;
	let header = "Добавление иллюстрации";
	loadUniversalForm(url, header, function () {
		$(".image-type").on("change", function () {
			const val = $(".image-type").val();
			if (val == "5"/*rich  content*/) {
				$(".image-upload").attr("accept", "image/jpeg");
			}
			else {
				$(".image-upload").attr("accept", "image/png");
			}
		});
		$(".image-type").change();
	});
}

function addTag(e) {
	e.preventDefault();
	let productId = $("#ProductId").val();
	let url = window.urls.addSiteProductTag + "?productId=" + productId;
	let header = "Добавление тэга";
	loadUniversalForm(url, header);
}

function deleteTag(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let productTagId = elm.data("productTagId");
	let productId = elm.data("productId");
	if (!confirm("Вы уверены, что хотите удалить элемент?"))
		return;
	$.post(`${window.urls.deleteSiteProductTag}?productTagId=${productTagId}&productId=${productId}`)
		.done(function () {
			$(elm).parents("div.product-tag").remove();
		})
		.fail(function (ex) {
			console.log(ex);
			alert("Не удалось удалить запись");
		});
}


function deleteImage(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let imageId = elm.data("imageId");
	let productId = $("#ProductId").val();
	if (!confirm("Вы уверены, что хотите удалить элемент?"))
		return;
	$.post(`${window.urls.deleteSiteProductImage}?imageId=${imageId}&productId=${productId}`)
		.done(function () {
			$(elm).parents("div.row").remove();
		})
		.fail(function (ex) {
			console.log(ex);
			alert("Не удалось удалить запись");
		});
}

function deleteVideoFile(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let productId = $("#ProductId").val();
	if (!confirm("Вы уверены, что хотите удалить видео файл?"))
		return;
	$.post(`${window.urls.deleteProductVideo}?productId=${productId}`)
		.done(function () {
			window.location.reload();
		})
		.fail(function (ex) {
			console.log(ex);
			alert("Не удалось удалить запись");
		});
}

async function copyImagePath(e) {
	e.preventDefault();
	const elm = $(e.target);
	const imagePath = elm.data("imagePath");
	const objectStorage = await getSetting("Cloud:ObjectStorage");
	const bucketName = await getSetting("Cloud:BucketName");
	const url = `${objectStorage}${bucketName}/${imagePath}`;
	navigator.clipboard.writeText(url);
	let mark = elm.siblings(".src-copied");
	mark.show();
	setTimeout(() => { mark.hide(); }, 2500);
}

function manageMarketplace(e) {
	e.preventDefault();
	const action = e.data.action;
	let header = "Редактирование";
	let elm = $(findTarget(e));
	let url = "";
	if (action === "add") {
		let productId = $("#ProductId").val();
		header = "Добавление";
		url = `${window.urls.addMarketplaceProduct}?productId=${productId}`;
	}
	else {
		let marketPlaceProductId = elm.data("marketplace-product");
		url = `${window.urls.editMarketplaceProduct}?marketPlaceProductId=${marketPlaceProductId}`;
	}
	header = `${header} информации о товаре`;
	loadUniversalForm(url, header);
}

function deleteMarketplace(e) {
	e.preventDefault();
	let elm = $(findTarget(e));

	if (!confirm("Вы уверены, что хотите удалить элемент?"))
		return;
	let marketPlaceProductId = elm.data("marketplace-product");
	url = `${window.urls.deleteMarketplaceProduct}?marketPlaceProductId=${marketPlaceProductId}`;

	$.post(url)
		.done(function () {
			alert("Запись удалена");
			$(elm).parents("div.row.marketplace-product").remove();
		})
		.fail(function (ex) {
			console.log(ex);
			alert("Не удалось удалить запись");
		});
}