$(function () {

	$(".copy-route").click(copyRoute);
})

function copyRoute(e) {
	e.preventDefault();
	let elm = $(findTarget(e));
	let filter = elm.data("filter");
	navigator.clipboard.writeText(`${window.applicationBaseUrl}category/${filter}`);
	let mark = elm.siblings(".src-copied");
	mark.show();
	setTimeout(() => { mark.hide(); }, 2500);
}