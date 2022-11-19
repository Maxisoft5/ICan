const addImageBtn = $('.add-image');
const deleteImageBtn = $('.delete-image');
const copyImageBtn = $('.copy-image-path');

$(
	function () {
		tinymce.init(
			{
				selector: '.material-content',
				plugins: ['autolink lists link preview textcolor colorpicker image code paste'],
				menubar: false,
				relative_urls: false,
				toolbar: ' undo redo |  formatselect | bold italic backcolor forecolor | alignleft aligncenter alignright alignjustify | bullist numlist image | removeformat | preview code',
			});
 
		addImageBtn.on("click", addImage);
		deleteImageBtn.on("click", deleteImage);
		copyImageBtn.on("click", copyImagePath);
		const hash = window.location.hash.substr(1);
		$(`#nav-tab a[href="#${hash}"]`).tab('show');
	}
);

function addImage(e) {
	e.preventDefault();
	let id = $("#MaterialId").val();
	let url = window.urls.addImage + "?id=" + id+"&objectTypeId=2";
	let header = "Добавление иллюстрации";
	loadUniversalForm(url, header, function () {
		const onlyImages = "image/jpeg,image/png";
		const withPdf = `${onlyImages},application/pdf`;
		$(".image-type").on("change", function () {
			const val = $(".image-type").val();
			if (val == 7 /*material preview*/) {
				$(".image-upload").attr("accept", onlyImages);
			}
			else {
				$(".image-upload").attr("accept", withPdf);
			}
		});
		$(".image-type").change();


			$(".image-upload").attr("accept", );
	});
}
 

function deleteImage(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let imageId = elm.data("imageId");
	let id = $("#MaterialId").val();
	if (!confirm("Вы уверены, что хотите удалить элемент?"))
		return;
	$.post(`${window.urls.deleteImage}?imageId=${imageId}&objectId=${id}`)
		.done(function () {
			$(elm).parents("div.row-image").remove();
		})
		.fail(function (ex) {
			console.log(ex);
			alert("Не удалось удалить запись");
		});
} 