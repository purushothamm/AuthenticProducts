using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticProducts.Models
{
    public class UserProductsModel
    {
       public User _user { get; set; }
       public List<Product> _ProductList { get; set; }
    }
}