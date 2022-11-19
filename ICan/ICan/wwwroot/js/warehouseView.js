$(function () {
	function deleteWarehouse() {
		if (!confirm("Вы уверены, что хотите удалить запись?"))
			return;
		$(this).closest('form').submit();
	};

	function initControls() {
		let deleteButtons = $('.delete-warehouse');
		deleteButtons.on('click', deleteWarehouse);
	};

	function warehouseFormatter(value, row, index) {
		return row.warehouseActionTypeName
 	}

	function operationFormatter(value, row, index) {
		const template = [];
		if (user.isAdmin && row.warehouseActionTypeId == 2 /*inventory*/
			|| ((user.isAdmin || user.isOperator) && row.warehouseActionTypeId == 6 /*returning*/
			)) {
			template.push(`<a href='${window.urls.warehouseEdit}?id=${row.warehouseId}'
			title="Редактировать"><i class="far fa-edit"></i></a>
					`, `<span>|</span>`);
		}

		template.push(`<a href='${window.urls.warehouseDetails}?id=${row.warehouseId}'
			title="Редактировать"><i class="far fa-file-alt"></i></a>`);

		if (user.isAdmin || user.isStoreKeeper && row.warehouseActionTypeId != 2 /*inventory*/)
			template.push(`<span>|</span>`,
				`<form action='${window.urls.warehouseDelete}?id=${row.warehouseId}'
						class="delete-order-form" method='POST'
									  style="display: inline">
										<i class="delete-warehouse index far fa-times-circle"></i>
								</form>`);
		return template.join("");
	}

	function getColumns() {
		var host = window.location.origin;
		var productUrl = window.urls.getProducts;
		var warehouseActions = window.urls.getWarehouseActions;

		let columns = [
			{
				title: 'Действие',
				field: 'warehouseActionTypeId',
				sortable: true,
				filterControl: "select",
				filterData: `url:${host}${warehouseActions}`,
				class: "narrow-130",
				formatter: warehouseFormatter
			},
			{
				title: 'Дата добавления',
				field: 'dateAddStr',
				sortable: true,
				class: "narrow-120",
			},
			{
				title: 'Дата сборки',
				field: 'assemblyDateStr',
				sortable: true,
				class: "narrow-100",
			},
			{
				title: 'Единственная тетрадь',
				field: 'soleItemName',
				sortable: true,
				class: "narrow-250",
				filterControl: "select",
				filterData: `url:${host}${productUrl}`
			},
			{
				title: 'Количество',
				sortable: true,
				align: "right",
				class: "narrow-90",
				field: 'soleItemAmount',
				filterControl: "input",
			},
			{
				title: 'Комментарий',
				field: "comment",
				sortable: true,
				filterControl: "input",
				class: "maxwidth-350"
			},
			{
				title: '',
				align: "right",
				class: "narrow-100",
				formatter: operationFormatter
			}];
		return columns;
	}

	initView();

	function initView() {

		$(".warehouse-table").bootstrapTable(
			{
				filterControl: true,
				columns: getColumns()
			});

		$(".warehouse-table").on("reset-view.bs.table", function () {
			initView();
			initControls();
		});
	}
});

