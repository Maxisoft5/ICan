$(
	function () {
		tinymce.init(
			{
				selector: 'textarea',
				plugins: ['autolink lists link preview textcolor colorpicker table  image code paste'],
				menubar: false,
				toolbar: ' undo redo |  formatselect | bold italic backcolor forecolor | alignleft aligncenter alignright alignjustify | bullist numlist ] table tabledelete | tableprops tablerowprops tablecellprops | tableinsertrowbefore tableinsertrowafter tabledeleterow | tableinsertcolbefore tableinsertcolafter tabledeletecol | image | link | removeformat | preview code',
				convert_urls: false,
				default_link_target: '_blank'
			});


		$(".add-image").click(function (e) {
			e.preventDefault();
			let url = window.urls.addCampaignImage;
			let header = "Добавление вложения";
			loadUniversalForm(url, header, function () {
			});
		});

		$(document).on("click", ".copy-image-path", function (e) {
			e.preventDefault();
			const elm = $(e.target);
			const imagePath = elm.data("image-path");
			console.log(imagePath);
			navigator.clipboard.writeText(imagePath);
			let mark = elm.siblings(".src-copied");
			mark.show();
			setTimeout(() => { mark.hide(); }, 2500);
		});

		$(document).on("submit", ".universal-modal-form", function (e) {
			e.preventDefault();
			let input = $(".image-upload")[0];
			let campaignId = $("#CampaignId").val();
			let files = input.files;
			let formData = new FormData();
			formData.append("file", files[0]);
			formData.append("campaignId", campaignId);

			$.ajax(
				{
					url: window.urls.addCampaignImage,
					data: formData,
					processData: false,
					contentType: false,
					type: "POST",
					success: function (data) {
						$(".universal-modal").modal("toggle");
						$(".image-list").append(
							`<tr>
                                <td><img src="${data}" style="max-width:400px"></td>
                                <td><button type="button" class="btn btn-link copy-image-path" data-image-path="${data}">${files[0].name}</button></td>
                            </tr>`
						);
					}
				}
			);
		});
	}
);