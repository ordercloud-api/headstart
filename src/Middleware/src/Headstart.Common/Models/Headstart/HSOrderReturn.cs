using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSOrderReturn : OrderReturn<OrderReturnXp>
    {
    }

    public class OrderReturnXp
    {
        public string SellerComments { get; set; }

        public ReturnEventDetails SubmittedStatusDetails { get; set; }

        public ReturnEventDetails ApprovedStatusDetails { get; set; }

        public ReturnEventDetails DeclinedStatusDetails { get; set; }

        public ReturnEventDetails CompletedStatusDetails { get; set; }

        public ReturnEventDetails CanceledStatusDetails { get; set; }
    }

    public class ReturnEventDetails
    {
        public string ProcessedByName { get; set; }

        public string ProcessedByUserId { get; set; }

        public string ProcessedByCompanyId { get; set; }

        public decimal RefundAmount { get; set; }
    }
}
