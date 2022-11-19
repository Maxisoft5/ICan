using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICan.Common.Domain
{
	public class OptSpringOrder
	{
		public int SpringOrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public string Provider { get; set; }
		public decimal Cost { get; set; }
		public string InvoiceNumber { get; set; }
		public string UPDNumber { get; set; }
		public int SpoolCount { get; set; }
		public bool IsAssembled { get; set; }
		public int SpringId { get; set; }
		public OptSpring Spring { get; set; }
		public IEnumerable<OptSpringOrderIncoming> SpringOrderIncomings { get; set; }
		[NotMapped]
		public IEnumerable<OptPayment> Payments { get; set; }
	}
}
