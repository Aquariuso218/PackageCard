using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;

namespace ZR.Model.Sunset.Model.Dto
{
    public class PackageQuery1
    {
        public string createName { get; set; }

        public List<SimulatedDto> simulatedDatas { get; set; }
    }
}
