$(function () {
	$(".upload-marketplace-file").click(uploadFile);
});

function uploadFile(e) {
	e.preventDefault();

	$(".universal-modal .modal-body").load(window.urls.addMarketPlaceFile,
		function () {
			$(".universal-modal-title").text("Загрузка файла");

			$(".universal-modal_save").off("click");
			$(".universal-modal_save").on("click", function () {
				$("form.universal-modal-form").submit();
			});
			$(".universal-modal").modal();
		});
}
