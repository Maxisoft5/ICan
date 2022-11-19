$(function () {
	$(".download-report").on("click", {
		selector: ".download-report",
		fileName: `${new Date().toLocaleDateString()} WB остатки на складах API.xlsx`
	}, downloadReport);
	$(".get-data").on("click", getGraphData);


	$(".btn-upload").on("click", {
		selector: ".btn-upload",
		fileName: `Сравнение ЛК и API ${new Date().toLocaleDateString()}.xlsx`
	}, downloadReport);

});

function getGraphData() {
	let form = $("form.graph-form");
 
	var formData = new FormData(form.get(0));
	
	let url = window.urls.wbGraphData;

	$.ajax({
		type: 'POST',
		url: url,
		data: formData,
		processData: false,
		contentType: false,

	})
		.done((data) => { drawGraph(data); })
		.fail(() => alert("Не удалось получить данныее для графика"))
		.always(() => toggleControls(".get-data", false));
};
function setHChartOptions() {
	let highchartsOptions = Highcharts.setOptions({
		lang: {
			loading: 'Загрузка...',
			exportButtonTitle: "Экспорт",
			printButtonTitle: "Печать",
			printChart: "Печать",
			downloadPNG: 'Скачать в PNG',
			downloadJPEG: 'Скачать в JPEG',
			downloadPDF: 'Скачать в PDF',
			downloadSVG: 'Скачать в SVG',
			downloadCSV: 'Скачать в CSV',
			downloadXLS: 'Скачать в XLS',
			viewData: 'Показать исходные данные',
			hideData: 'Скрыть исходные данные',
			viewFullscreen: 'Развернуть',
		}
	});
}

function drawGraph(data) {
	setHChartOptions();
	Highcharts.chart('graphics-container', {
		title:"Отчет по ВБ",
		legend: {
			layout: 'horizontal',
			align: 'right',
			verticalAlign: 'bottom'
		},
		xAxis: {
			categories: data.categories,
			crosshair: true
		},
		yAxis: {
			min: 0,
			title: {
				text: 'Количество'
			}
		},
		tooltip: {
			headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
			pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
				'<td style="padding:0"><b>{point.y:.0f}</b></td></tr>',
			footerFormat: '</table>',
			shared: true,
			useHTML: true
		},
		plotOptions: {
			series: {
				label: {
					connectorAllowed: false
				} 
			}
		},
		series:  data.series
	});
}
