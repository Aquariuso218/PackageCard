﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Business.Model.Dto
{
    public class cPackageMsg
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> GeneratedBoxNumbers { get; set; }
    }
}