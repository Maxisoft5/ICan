function initDatetimePicker(date) {
	$("#dateAddtimepicker").datetimepicker(
		{
			defaultDate: date,
			//defaultDate: dateNow,
			showTodayButton: true,
			format: 'DD.MM.YYYY HH:mm:ss',
			showClose: true,
			showClear: true,
			toolbarPlacement: 'top',
			//stepping: 15,
			//locale: "ru",
			icons: {
				time: "fa fa-clock-o",
				date: "fa fa-calendar",
				up: "fa fa-arrow-up",
				down: "fa fa-arrow-down"
			}
		});
}

