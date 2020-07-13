using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticProducts.Models
{
    public class ProductAddressModel
    {
        public EthAddress _ethAddress { get; set; }
        public Product _product { get; set;}
    }
}