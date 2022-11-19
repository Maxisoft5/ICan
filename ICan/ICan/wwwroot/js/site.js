class User {
	constructor(data) {
		this.id = data.id;
		this.firstName = data.firstName;
		this.lastName = data.lastName;
		this.email = data.email;
		this.isClient = data.isClient;
		this.isAdmin = data.isAdmin;
		this.isOperator = data.isOperator;
		this.isStorekeeper = data.isStorekeeper;
		this.isAssembler = data.isAssembler;
		this.isDesigner = data.isDesigner;
	}
}


function checkValidLetters(e) {
	if (!(e.value.replace(/[^0-9]/, '') == e.value))
		e.value = '';
}

function checkValidNumber(e) {
	var n = e.value;
	var valid = n.search(/^[0-9]+.?[0-9]{0,2}$/) == 0 ? true : false;

	if (!valid)
		$(e).addClass("input-validation-error");
}

function findTarget(e) {
	return e.target.nodeName !== "A" ? e.target.parentElement : e.target; //A
}


function recurseFindTarget(target, deep = 0) {
	if (deep > 10)
		return null;
	return target.nodeName !== "A" ? recurseFindTarget(target.parentElement, ++deep) : target; //A
}

function removeZero(e) {
	var currentVal = e.value;
	if (currentVal === "0") {
		e.value = '';
	}
}


function setEmptyValues() {
	$(".input-nullable").each((_, item) => {
		const currentItem = $(item);

		if (!currentItem.val()) {
			currentItem.val(0);
		}
	})
}

function populateAjax(url, selector, selected) {
	$.ajax({
		url: url,
		type: "GET"
	})
		.done(function (data) {
			let selectList = $(selector);
			$("option", selectList).remove();
			// selectList.val(null);
			data.forEach(function (item) {
				let option = '<option ';
				if (item.value == selected)
					option += ' selected = "selected"';
				option += 'value = "' + item.value + '" > ' + item.text + '</option > ';
				selectList.append(option);
			});
		})
		.fail(function (data) {
			console.log(data);
			console.log("error in populateAjax");
		});
}

function setPaymentDate(e) {
	let paymetnDateControl = $("#PaymentDate");
	let isPaidControl = $("#IsPaid");
	let date = new Date();
	let day = ("0" + date.getDate()).slice(-2);
	let month = ("0" + (date.getMonth() + 1)).slice(-2);
	var today = date.getFullYear() + "-" + (month) + "-" + (day);
	if (isPaidControl.is(":checked")) {
		paymetnDateControl.val(today);
	}
	else {
		paymetnDateControl.val("");
	}
}




function datesTimeSorter(a, b) {
	let [isEmpty, result] = checkEmpty(a, b);
	if (isEmpty) {
		return result;
	}

	var dateA = moment(a.trim(), "DD.MM.YYYY h:mm:ss").toDate();
	var dateB = moment(b.trim(), "DD.MM.YYYY h:mm:ss").toDate();
	if (dateA < dateB) return 1;
	if (dateA > dateB) return -1;
	return 0;
}

function checkEmpty(a, b) {
	if (a.trim() == "" && b.trim() == "")
		return [true, 0];
	if (a.trim() == "")
		return [true, 1];
	if (b.trim() == "")
		return [true, -1];
	return [false];
}

function formattedNumberSorter(a, b) {
	let [isEmpty, result] = checkEmpty(a, b);
	if (isEmpty) {
		return result;
	}
	var numA = Number(a.trim().replace(/&nbsp;/gi, "").replace(",", "."));
	var numB = Number(b.trim().replace(/&nbsp;/gi, "").replace(",", "."));
	if (numA < numB) return 1;
	if (numA > numB) return -1;
	return 0;
}

function datesSorter(a, b) {
	let [isEmpty, result] = checkEmpty(a, b);
	if (isEmpty) {
		return result;
	}

	let dateA = moment(a.trim(), "DD.MM.YYYY").toDate();
	let dateB = moment(b.trim(), "DD.MM.YYYY").toDate();
	if (dateA < dateB) return 1;
	if (dateA > dateB) return -1;
	return 0;
}


function setInitial() {
	$.ajax({
		url: window.urls.getOrderSize,
		async: false
	}).done(function (data) {
		window.orderSizeData = data;
	});
}

function setUser() {
	const url = window.urls.getUserInfo;
	$.ajax({ url: url, async: false }).
		done(function (data) {
			window.user = new User(data);
		});
}

function initCommonControls() {
	var iSubmit = $('i.submit.index');

	iSubmit.on('click', function () {
		if (confirm("Вы уверены, что хотите удалить элемент?"))
			$(this).closest('form').submit();
	});

	$(".save-button").on("click", submitData);
}

function submitData(e) {
	let btn = $(e.target);
	if (btn) {
		btn.prop('disabled', true);
		$("#loading").show();
		btn.parents("form").submit();
	};
}

function downloadReport(e) {
	let btn = $(e.target);
	let selector = e.data.selector;
	let fileName = e.data.fileName;
	if (btn) {

		let form = btn.parents("form");
		toggleControls(selector, true);

		let url = form.attr('action');
		var formData = new FormData(form.get(0));

		fetch(url, {
			method: 'POST',
			body: formData
		})
			.then(resp => resp.blob())
			.then(blob => {
				const url = window.URL.createObjectURL(blob);
				const a = document.createElement('a');
				a.style.display = 'none';
				a.href = url;
				a.download = fileName;
				document.body.appendChild(a);
				a.click();
				window.URL.revokeObjectURL(url);
				toggleControls(selector, false);
			})
			.catch(() => toggleControls(selector, false));
	};
}

async function getSetting(name) {
	const url = `${window.urls.getSetting}?settingName=${name}`;
	const result = await fetch(url)
		.then((response) => {
			return response.text();
		})
		.then((body) => {
			return body;
		});
	return result;
}

function toggleControls(selector, disable) {
	if (disable) {
		$(selector).prop('disabled', true);
		$("#loading").show();
	}
	else {
		$(selector).prop('disabled', false);
		$("#loading").hide();
	}
}

function loadUniversalForm(url, header, callback) {
	$(".universal-modal .modal-body").load(url,
		function () {
			$(".universal-modal-title").text(header);
			$(".universal-modal_save").off("click");
			$(".universal-modal_save").click(function () {
				let form = $("form.universal-modal-form");
				if (!form.valid()) {
					form.validate().showErrors();
				}
				else {
					form.submit();
				}
			});
			$(".universal-modal").modal();
			if (callback) {
				callback();
			}
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
