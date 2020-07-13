using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticProducts.Models
{
    public class ClaimProductModel
    {
        int UserId { get; set; }
        string UserName { get; set; }
        string UserPassword { get; set; }
        string ProductName { get; set; }
        string ProductKey { get; set; }
        string EthAddress { get; set; }
        string EthPrivateKey { get; set; }
        List<Product> ProductList { get; set; }
    }
}