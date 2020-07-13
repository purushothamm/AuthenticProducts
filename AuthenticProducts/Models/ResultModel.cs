using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticProducts.Models
{
    public class ResultModel
    {
        public bool _Result { get; set; }
        public string _ResultText{ get; set; }
        public string _ResultError { get; set; }
        public string _ResultTransHash { get; set; }
    }
}