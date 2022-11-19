using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class SpringOrderManager : BaseManager
	{
		private readonly ISpringOrderRepository _springOrderRepository;
		private readonly ISpringOrderIncomingRepository _springOrderIncomingRepository;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IWarehouseJournalRepository _whJournalRepository;

		public SpringOrderManager(
				ILogger<BaseManager> logger,
				IMapper mapper,
				ISpringOrderRepository springOrderRepository,
				ISpringOrderIncomingRepository springOrderIncomingRepository,
				IPaymentRepository paymentRepository,
				IWarehouseJournalRepository whJournalRepository)
			: base(mapper, logger)
		{
			_springOrderRepository = springOrderRepository;
			_springOrderIncomingRepository = springOrderIncomingRepository;
			_paymentRepository = paymentRepository;
			_whJournalRepository = whJournalRepository;
		}

		public IEnumerable<SpringOrderModel> GetSpringOrders()
		{
			var orders = _springOrderRepository.GetSpringOrders();
			return _mapper.Map<IEnumerable<SpringOrderModel>>(orders);
		}

		public async Task Create(SpringOrderModel springOrderModel)
		{
			var optSpringOrder = _mapper.Map<OptSpringOrder>(springOrderModel);
			await _springOrderRepository.Add(optSpringOrder);
		}

		public async Task<SpringOrderModel> GetById(int id)
		{
			var springOrder = await _springOrderRepository.GetById(id);
			var payments = await _paymentRepository.GetByOrdersId(id, PaymentType.SpringOrder);
			springOrder.Payments = payments;
			return _mapper.Map<SpringOrderModel>(springOrder);
		}

		public async Task Edit(SpringOrderModel model)
		{
			var mappedModel = _mapper.Map<OptSpringOrder>(model);
			await _springOrderRepository.Update(mappedModel);
		}

		public async Task Delete(int id)
		{
			await _springOrderRepository.Delete(id);
		}

		public async Task AddIncoming(SpringOrderIncomingModel springOrderIncoming)
		{
			var incoming = _mapper.Map<OptSpringOrderIncoming>(springOrderIncoming);
			var springOrder = await _springOrderRepository.GetById(springOrderIncoming.SpringOrderId);
			await _springOrderIncomingRepository.Add(incoming);
			var journalItem = new OptWarehouseJournal
			{
				WarehouseTypeId = (int)WarehouseType.Spings,
				ActionDate = springOrderIncoming.IncomingDate,
				ActionTypeId = (int)WhJournalActionType.Income,
				ActionExtendedTypeId = (int)WhJournalActionExtendedType.SpringsIncoming,
				ActionId = incoming.SpringOrderIncomingId.ToString(),
				ObjectTypeId = (int)WhJournalObjectType.Spring,
				ObjectId = springOrder.SpringId,
				Amount = incoming.NumberOfTurnsCount
			};

			await _whJournalRepository.Add(journalItem);
		}

		public async Task DeleteIncoming(int incomingId)
		{
			await _whJournalRepository.RemoveRangeByAction(incomingId.ToString(), (int)WhJournalActionExtendedType.SpringsIncoming, false);
			await _springOrderIncomingRepository.Delete(incomingId);
		}

		public async Task<int> AddPayment(PaymentModel payment)
		{
			var optPayment = new OptPayment
			{
				PaymentDate = payment.PaymentDate,
				Amount = payment.Amount,
				OrderId = payment.OrderId,
				PaymentType = PaymentType.SpringOrder
			};
			return await _paymentRepository.Add(optPayment);
		}

		public async Task DeletePayment(int id)
		{
			await _paymentRepository.Delete(id);
		}
	}
}
