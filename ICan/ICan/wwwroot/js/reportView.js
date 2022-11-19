$(function () {
	function getColumns() {
		let reportColums = [
			{
				title: 'Магазин',
				field: 'shopName',
				sortable: true,
				filterControl: 'select',
				filterData: 'url:/opt/shop/dict',
				align: 'center',
				width: '150'
			},
			{
				title: 'Название файла',
				field: 'fileName',
				sortable: true,
				filterControl: 'input',
				alignt: 'center',
				formatter: formaFileName
			},
			{
				title: 'Номер отчёта',
				field: 'reportNum',
				sortable: true,
				filterControl: 'input',
				align: 'center',
				width: '170',
			},
			{
				title: 'Дата отчёта',
				field: 'reportDate',
				sortable: true,
				filterControl: 'input',
				align: 'center',
				formatter: formateDate
			},
			{
				title: 'Год отчёта',
				field: 'reportYear',
				sortable: true,
				filterControl: 'select',
				align: 'center'
			},
			{
				title: 'Вирт',
				field: 'isVirtual',
				align: 'center',
				sortable: true,
				formatter: virtualFormatter,
				width: '50'
			},
			{
				title: 'Общая сумма',
				field: 'totalSumFormatted',
				sortable: true,
				align: 'center'
			},
			{
				title: 'Дата загрузки',
				field: 'uploadDate',
				sortable: true,
				align: 'center',
				filterControl: 'input',
				formatter: formateDateTime
			}
		];
		return reportColums;
	}

	function formateDateTime(value, row, index) {
		return moment(value).format("DD.MM.YYYY HH:mm");
	}

	function formateDate(value, row, index) {
		return moment(value).format("DD.MM.YYYY");
	}

	function virtualFormatter(value, row, index) {
		if (row.isVirtual === true)
			return `<i class="fas fa-check"></i>`;
		return "";
	}

	function formaFileName(value, row, index) {
		return `<input type='hidden' value='${row.reportId}'/><span>${row.fileName}</span>`;
	}

	initReportView();
	function initReportView() {
		$(".report-table").bootstrapTable(
			{
				filterControl: true,
				columns: getColumns()
			});
	}
});