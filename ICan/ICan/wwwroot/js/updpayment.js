$(function () {
	$(".carry-upd-payment").click(carryUpdPayment);
});

function carryUpdPayment(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	const sum = elm.data("sum");
	const month = elm.data("expectingMonth");
	if (!confirm(`Вы уверены, что хотите подтвердить оплату суммы ${sum} в ${month}?`))
		return;
	const form = elm.parents("form");
	form.submit();
}