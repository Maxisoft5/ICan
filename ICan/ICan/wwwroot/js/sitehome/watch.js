$(document).ready(function () {
	$(".gtm-watch").bind('contextmenu', function (e) {
		const elm = $(recurseFindTarget(e.target));
		const eventName = elm.data("event-name");
		if (dataLayer) {
			dataLayer.push({
				'event': eventName
			});
		}
	});
	$(".gtm-watch-copy").bind('copy', function (e) {
		const elm = $(recurseFindTarget(e.target));
		const eventName = elm.data("event-name");
		if (dataLayer) {
			dataLayer.push({
				'event': eventName
			});
		}
	})
});
