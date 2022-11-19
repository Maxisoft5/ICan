$(function () {
	function deleteOrder() {
		if (!confirm("Вы уверены, что хотите удалить заказ?"))
			return;
		var elm = $(this);

		var orderStatus = elm.data("order-status");
		var paymentsCount = Number(elm.data("order-payments-count"));
		var isPaid = elm.data("order-ispaid");

		if (orderStatus < 3 && paymentsCount == 0 && isPaid == false) {
			setToken();
			$(this).closest('form').submit();
		}
		else {
			alert('Невозможно удалить заказ, для которого проставлен флаг "Оплачен"' +
				' или внесены платежи или заказ в статусе "Выполнен"');
		}
	};

	function initControls() {
		let deleteOrderButton = $('.delete-order');
		deleteOrderButton.on('click', deleteOrder);
	};

	function isPaidFormatter(value, row, index) {
		if (row.isPaid === true)
			return `<i class="fas fa-check"></i>`;
		return "";
	}
	function operationFormatter(value, row, index) {
		const template = [`<a href='${window.urls.orderExport}?id=${row.orderId}' 
			title="Экспортировать в Excel"><i class="far fa-file-excel"></i></a>
					`, `<span class="buttons"></span>`,
		`<a href='${window.urls.orderEdit}?id=${row.orderId}' class="buttons"><i class="far fa-edit"></i></a>`
		];
		if (user.isAdmin)
			template.push(`	<span class="buttons"> </span>`,
				`<form action='${window.urls.deleteOrder}?id=${row.orderId}' class="delete-order-form" method='POST'
									  style="display: inline">
										<i class="delete-order index far fa-times-circle"
									   data-order-status="${row.orderStatusId}"
									   data-order-payments-count="${row.paymentsCount}"
									   data-order-ispaid="${row.isPaid}"></i>
								</form>`);
		return template.join("");
	}

	function shortOrderDisplayIdFormatter(value, row, index) {
		const template = `<a href='${window.urls.editOrder}?id=${row.orderId}'>${value}</a>`
		return template;
	}

	function setMaxHeightForComment(value, row, index) {
		if (value != null) {
			let str = value;
			if (value.length > 60) {
				str = value.substr(0, 60) + '...';
			}
			const template = `${str}`
			return template;
		}
	}

	function clientFormatter(value, row, index) {
		const template = [];
		if (user.isStoreKeeper) {
			template.push(`${row.client.fullName}`);
		}
		else {
			template.push(`<a href='${window.urls.indexClient}?id=${row.client.id}'>${row.client.fullName}</a>`);
		}
		if ((user.isOperator || user.isAdmin) && row.shopName) {
			template.push(`<br />`);
			const shopName = row.shopName.replaceAll('"', '');
			const title = `${row.client.fullName} ${shopName}`;
			template.push(`<a title='${title}' href='${window.urls.editShop}?id=${row.client.shopID}'>${shopName}</a>`);
		}

		return template.join("");
	}

	function setToken() {
		const token = $("table.order-table").data("token");
		const newInput = `<input name="__RequestVerificationToken" type="hidden" value="${token}" />`;
		$(".delete-order-form").append(newInput);
	}

	function getColumns() {
		let orderColumns = [
			{
				title: '#',
				field: 'shortOrderDisplayId',
				sortable: true,
				filterControl: "input",
				align: "center",
				class: "narrow-60",
				formatter: shortOrderDisplayIdFormatter
			},
			{
				title: 'Дата заказа',
				field: 'orderDateDisplay',
				sortable: true,
				class: "narrow-100",
				filterControl: "input",
			},
		];
		if (!user.isClient) {
			orderColumns.push(
				{
					title: 'Клиент',
					sortable: true,
					filterControl: "input", width: "20",
					field: "client",
					class: "narrow-250",
					formatter: clientFormatter/*,
					filterCustomSearch:  clientsearch*/
				},
				{
					title: 'Тип клиента',
					sortable: true,
					filterControl: "select",
					class: "narrow-100",
					field: 'clientTypeName'
				});
		}
		orderColumns.push(
			{
				title: 'Статус',
				sortable: true,
				filterControl: "select",
				class: "narrow-100",
				field: 'orderStatus'
			},
			{
				title: 'Сумма, руб',
				sortable: true,
				align: "right",
				class: "narrow-90",
				field: 'discountedSumFormatted'
			},
			{
				title: 'Остаток',
				sortable: true,
				align: "right",
				class: "narrow-90",
				field: 'restSumFormatted'
			},
			{
				title: 'Оплачен',
				field: "isPaid",
				filterData: 'json:{"true":"Не оплачен", "false":"Оплачен"}',
				sortable: true,
				filterControl: "select",
				align: "center",
				class: "narrow-90",
				formatter: isPaidFormatter
			}
		);
		if (!user.isClient) {
			orderColumns.push(
				{
					title: 'Инд <br/>скидка',
					sortable: true,
					filterControl: "select",
					align: "right",
					class: "narrow-80",
					field: 'personalDiscountPercent'
				},
				{
					title: 'Комментарий',
					sortable: false,
					filterControl: 'input',
					align: 'left',
					class: "narrow-300",
					field: 'comment',
					formatter: setMaxHeightForComment
				});
		}
		orderColumns.push({
			title: '',
			align: "right",
			class: "narrow-100",
			formatter: operationFormatter
		});
		return orderColumns;
	}
	initOrderView();

	function clientsearch(text, value, field, data) {
		console.log(text, value, field);
	}

	function initOrderView() {

		$(".order-table").bootstrapTable(
			{
				filterControl: true,
				columns: getColumns()
			});


		$(".order-table").on("reset-view.bs.table", function () {
			initOrderView();
			initControls();
		});
	}
});

