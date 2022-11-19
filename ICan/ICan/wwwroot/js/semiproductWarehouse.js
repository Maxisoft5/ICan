$(document).ready(function () {
	$(".warehouse-type").on("change", setAvailableSemiproducts);
});
 

function setAvailableSemiproducts() {
	const whType = $(".warehouse-type").val();
	if (whType == 2 /*semiproduct ready*/) {
		$(".whitem-info").each(function (i, whInfo) {
			$(".block", whInfo).prop("disabled", !$(".block-id", whInfo).val());
			$(".stickers", whInfo).prop("disabled", !$(".stickers-id", whInfo).val());
			$(".covers", whInfo).prop("disabled", !$(".covers-id", whInfo).val());
			$(".box-front", whInfo).prop("disabled", !$(".box-front-id", whInfo).val());
			$(".box-back", whInfo).prop("disabled", !$(".box-back-id", whInfo).val());
			$(".cursor", whInfo).prop("disabled", !$(".cursor-id", whInfo).val());
		});
	}
	else {
		$(".only-ready-warehouse").prop("disabled", true);
	}
}