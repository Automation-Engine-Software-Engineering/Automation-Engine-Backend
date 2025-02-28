﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ResultViewModel
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
    }
}
