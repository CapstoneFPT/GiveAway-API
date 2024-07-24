﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Refunds
{
    public class ApprovalRefundRequest
    {
        public RefundStatus Status { get; set; }
        public int RefundPercentage { get; set; }
        public string Description { get; set; }
    }
}