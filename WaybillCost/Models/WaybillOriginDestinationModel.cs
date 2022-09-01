using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaybillCost.Models
{
    public class WaybillOriginDestinationModel
    {
        public WayBill waybillDetails { get; set; }
        public Origin originDetails { get; set; }
        public Destination destinationDetails { get; set; }
    }
}