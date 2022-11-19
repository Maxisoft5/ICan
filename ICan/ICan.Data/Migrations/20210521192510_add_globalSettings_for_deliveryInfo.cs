using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_globalSettings_for_deliveryInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into opt_globalsetting(GlobalSettingId, Content, Comment) values(8, '<div class=\"text-title-1\">" +
                "<h3 class=\"content__title\">Оформление заказа</h3>" +
                "<div class=\"content__text\">" +
                "<p>Обработка заказов осуществляется в рабочие дни с 9-00 до 17-00.</p>" +
                "<h3>Доставка</h3>" +
                "<p>При заказе от 25000 рублей(с учетом скидки) доставка осуществляется бесплатно до ПВЗ ПЭК в России.</p>" +
                "<p>По Москве в пределах МКАД доставка осуществляется в течение 1-2 рабочих дней и стоит 500 рублей.&nbsp;<br>Доставка осуществляется до подъезда.Водитель на этаж НЕ ПОДНИМАЕТ!</p>" +
                "<p><br><strong>Самовывоз по адресу:</strong><br>Московская обл., пос.Совхоза им. Ленина, стр. 1<br><br>В регионы доставка осуществляется транспортными компаниями.<br>Доставка от нашего склада до терминала ТК ПЭК, СДЭК, Деловые Линии осуществляется бесплатно.<br>Стоимость доставки от нашего склада до других ТК составляет 500 рублей.</p>" +
                "</div>" +
                "</div>', 'Информация о доставке');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from opt_globalsetting where GlobalSettingId = 8");
        }
    }
}
