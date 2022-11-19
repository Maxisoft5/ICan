// Write your JavaScript code.
$(".import-link").on("click", function () {
	var elm = $(this);
	var client = elm.data("client-name");
	var clientId = elm.data("client-id");
	if (!!client) {
		$(".client").show();
		$("#import-Excel-client").text(client);
		$("#import-Excel-clientId").val(clientId);
	}
	else {
		$(".client").hide();
	}
	$("#import-Excel").modal();
});

$("#import-button").on("click", function () {
	if (confirm("Вы уверены, что хотите импортировать цены?")) {
		var input = document.getElementById("priceFile");
		var files = input.files;
		var formData = new FormData();

		for (var i = 0; i != files.length; i++) {
			formData.append("file", files[i]);
		}
		var url = window.urls.importPrices;
		$.ajax(
			{
				url: url,
				data: formData,
				processData: false,
				contentType: false,
				type: "POST",
				success: function (data) {  
					alert("Импорт прошёл успешно");
					window.location.reload();
				},
				error: function (data) {
					alert("Не удалсь импортировать файл");
				}

			}
		);
	}
});