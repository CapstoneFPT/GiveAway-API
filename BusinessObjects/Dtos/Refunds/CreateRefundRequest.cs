﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Refunds
{
    public class CreateRefundRequest
    {
        public required string Description { get; set; }
    }
}