$(document).ready(function () {
	$('.show-contractFile').click(function (e) {
		previewDocument(e);
	});
	$('.delete-contractFile').click(function (e) {
		let elm = $(e.target);
		let shopId = elm.data("shopid");
		$.ajax({
			url: window.urls.deleteContractFile,
			type: 'DELETE',
			data: { "shopId": shopId },
			success: function () {
				elm.hide();
				$('.show-contractFile').hide();
				$('.success-deteled').show();
				$('#ScanFilePath').remove();
				$('#MimeType').remove();
			}
		});
	});
});



function previewDocument(e) {
	e.preventDefault();
	let elm = $(e.target);
	let filesrc = elm.data("filesrc");
	let mimeType = elm.data("mimetype");
	let url = window.urls.previewDocument + "?url=" + filesrc + "&mimeType=" + mimeType;
	let modal = $(".document-preview-modal");
	modal.find("iframe").prop("src", url);
	setTimeout(() => { modal.modal(); }, 1000);
}