using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKBR_Report_Puller.Domain
{
    public class TradeExecution : TradeBase
    {
        public decimal IBCommission { get; set; }
        public string IBCommissionCurrency { get; set; }
        public string IbExecID { get; set; }
    }
}
