$(function () {
	const formSelector = "form.upload-form";

	$(".btn-upload", formSelector).click(submitForm);

	function submitForm() {
		$(".alert-danger").text("");
		$(formSelector).submit();
	}
});

