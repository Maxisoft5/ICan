using AutoMapper;
using ICan.Business.Services;
using ICan.Common;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ICan.Business.Managers
{
	public class OrderMailManager : BaseManager
	{
		private readonly IEmailSender _emailSender;

		public OrderMailManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger,
			IEmailSender emailSender) : base(mapper, context, logger)
		{
			_emailSender = emailSender;
		}

		public void SendNewOrderMail(Guid orderId, string email)
		{
			try
			{
				var order = _context.OptOrder
					.Include(ord => ord.Requisites)
					.First(ord => ord.OrderId == orderId);
				var header = string.Format(Const.OrderMessageTheme, order.ShortOrderId);

				var messageBody = order.Requisites?.RequisitesText ??
					"Уважаемый клиент! <br/> " +
					$"Ваш заказ принят. Номер заказа №{order.ShortOrderId}.<br/><br/>" +
					"С уважением, <br/>" +
					"Команда \"Я могу\"!";

				_logger.LogInformation("Пытаемся отправить сообщение пользователю");
				_emailSender.SendEmail(email, header, messageBody);
				_logger.LogInformation("Вроде бы отправили письмо");
			}
			catch (Exception ex)
			{
				_logger.LogError("Возникла ошибка при отправлении email пользователю", ex);
			}
		}


		public void SendNewTrackNoMail(string shortOrderId, string trackNo, string email)
		{
			try
			{
				var header = string.Format(Const.NewTrackNo, shortOrderId, trackNo);

				var message = string.Format(Const.NewTrackBody, shortOrderId, trackNo);
				_logger.LogInformation("Пытаемся отправить сообщение пользователю");
				_emailSender.SendEmail(email, header, message);
				_logger.LogInformation("Вроде бы, отправили письмо");
			}
			catch (Exception ex)
			{
				_logger.LogError("Возникла ошибка при отправлении email пользователю", ex);
			}
		}
	}
}
