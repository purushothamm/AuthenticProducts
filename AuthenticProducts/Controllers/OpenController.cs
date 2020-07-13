using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AuthenticProducts.Models;

namespace AuthenticProducts.Controllers
{
    public class OpenController : Controller
    {
        AuthProductEntities db = new AuthProductEntities();
        // GET: Login
        public ActionResult Index()
        {
            ProductModel prod = new ProductModel();
            return View(prod);
        }

        [HttpPost]
        public ActionResult SerialSearch(ProductModel pr)
        {
            var Product = db.Products.FirstOrDefault(p => p.Serial_Num == pr.SerialNumber);
            if(Product == null)
            {
                pr.ErrorMessage = "Product Serial Number dose not exist..";
                return View("Index", pr);
            }
            else
            {
                var NoUser = db.Users.FirstOrDefault(u => u.UserID == 5);
                UserControl.UserID = NoUser.UserID;
                UserControl.UserEmail = NoUser.UserEmail;
                UserControl.UserName = NoUser.UserName;
                UserControl.UserTypeID = 2;

                return RedirectToAction("DirectClaim", "ClaimProduct", new { ProdId = Product.Product_ID });
                //return RedirectToAction("Details", "Products", new { Prodid = ProductID, _isDirectClaim = 1 });
            }
            //return View();
        }
    }
}