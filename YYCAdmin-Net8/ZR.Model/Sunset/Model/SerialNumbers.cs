using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model
{
    [SugarTable("base_SerialNumbers ", "流水号")]
    [Tenant("0")]
    public class SerialNumbers
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int id { get; set; }

        [SugarColumn(ColumnName = "serial_date")]
        public string serialDate {  get; set; }

        [SugarColumn(ColumnName = "serial_Number")]
        public int serialNumber { get; set; }

        [SugarColumn(ColumnName = "full_serial")]
        public string fullSerial {  get; set; }


    }
}
