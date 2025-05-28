using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;

namespace ZR.Model.Sunset.Model.Dto
{
    public class PackageQuery
    {
        public string createName { get; set; }

        public Boolean isZero { get; set; }

        //public string remark { get; set; }

        public List<SimulatedData> simulatedDatas { get; set; }
    }
}
