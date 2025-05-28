using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    public class DispatchListDto
    {
        /// <summary>
        /// 发货单号
        /// </summary>
        public string cDLCode { get; set; }

        public string dDate { get; set; }

        public string cCusName { get; set; }

        public string iDLsID { get; set; }
        public string cInvCode { get; set; }
        public string cInvName { get; set; }
        public string cWhCode { get; set; }
        public decimal iQuantity { get; set; }
        public string cBatch { get; set; }
        public string irowno { get; set; }

        public string startDate { get; set; }
        public string endDate { get; set; }
        public string keyword { get; set; }

    }
}
