const copyPathBtn = $('.copy-material-path');

$(
	function () {
		copyPathBtn.on("click", copyPath);
	}
);

async function copyPath (e) {
	e.preventDefault();
	const elm = $(recurseFindTarget(e.target));
	const id = elm.data("materialId");
	const url = `${window.location.origin}${window.urls.materialList}#${id}`;
	navigator.clipboard.writeText(url);
	let mark = elm.parents("tr").find(".src-copied");
	mark.show();
	setTimeout(() => { mark.hide(); }, 2500);
}