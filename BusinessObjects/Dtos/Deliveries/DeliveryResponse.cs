﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Deliveries
{
    public class DeliveryResponse
    {
        public Guid AddressId { get; set; }
        public string RecipientName { set; get; }
        public string Phone { set; get; }
        public string Residence { set; get; }
        public string AddressType { set; get; }
        public string Buyername { set; get; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
