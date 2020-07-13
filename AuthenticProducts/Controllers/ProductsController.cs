using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AuthenticProducts.Models;
using Nethereum.RPC.Eth.DTOs;

namespace AuthenticProducts.Controllers
{
    public class ProductsController : Controller
    {
        private AuthProductEntities db = new AuthProductEntities();

        // GET: Products
        public ActionResult Index()
        {
           List<ProductEthAddress_V > products = new List<ProductEthAddress_V>();
            if (UserControl.UserTypeID == 1)
                products = db.ProductEthAddress_V.Where(p => p.P_CreatedBy == UserControl.UserID).ToList();
            else
                products = db.ProductEthAddress_V.ToList();

            return View(products);
        }

        // GET: Products/Details_1
        public ActionResult Details(long? Prodid, int _isDirectClaim = 0)
        {
            if (Prodid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductEthAddress_V product = db.ProductEthAddress_V.FirstOrDefault(p=> p.Product_ID == Prodid);
            var Img = db.ImgFiles.FirstOrDefault(i => i.ProductID == Prodid);
            if(Img != null)
            {
                product.ImagePath = Img.Path;
            }
            if (product == null)
            {
                return HttpNotFound();
            }
            if (_isDirectClaim > 0)
            {
                product.IsDirectClaim = _isDirectClaim;
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,ProductName,ProductKey,SerialNumber,ProductType,ProductDetail,ProductIsActive,ImagePath,ImageFile")] ProductModel prodM)
        {
            if (ModelState.IsValid)
            {
                Decimal Amount = 0.0002m;
                EthAddressesController ethac = new EthAddressesController();
                Decimal GetBalance = Task.Run<Decimal>(async () => await ethac.GetAccountBalance()).Result;
                if (Amount >= GetBalance)
                {
                    return View(prodM);
                }

                var ethadd = ethac.GenerateEthAddress();
                ethadd.CreatedBy = UserControl.UserID;
                ethadd.CreatedDate = DateTime.Now;
                db.EthAddresses.Add(ethadd);
                db.SaveChanges();

                Models.Transaction TR = Task.Run<Models.Transaction>(async () => await ethac.SendTransactionAndData(ethadd.EthPublicKey, 0.0002m, prodM.ProductKey)).Result;
                //Task.Run<Models.Transaction>(async () => await ethac.SendTransaction(ethadd.EthPublicKey, 0.0002m)).Result;
                //TR.ProductID = product.Product_ID;
                TR.EthAddressID = ethadd.EthAddressID;
                TR.CreatedBy = UserControl.UserID;
                TR.CreatedDate = DateTime.Now;
                db.Transactions.Add(TR);
                db.SaveChanges();

                Product product = new Product
                {
                    Product_Name = prodM.ProductName,
                    Product_Key = prodM.ProductKey,
                    Serial_Num = prodM.SerialNumber,
                    Product_Type = prodM.ProductType,
                    Product_Detail = prodM.ProductDetail,
                    IsActive = prodM.ProductIsActive,
                    EthAddressID = ethadd.EthAddressID,
                    FirstTransactionID = TR.TransactionID,
                    CreatedBy = UserControl.UserID,
                    CreatedDate = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(prodM);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Prod_db = db.Products.FirstOrDefault(p => p.Product_ID == id);
            ProductModel product = new ProductModel
            {
                ProductId = Prod_db.Product_ID,
                ProductName = Prod_db.Product_Name,
                ProductKey = Prod_db.Product_Key,
                SerialNumber = Prod_db.Serial_Num,
                ProductDetail = Prod_db.Product_Detail,
                ProductType = Prod_db.Product_Type,
                ProductIsActive = Prod_db.IsActive
            };

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,ProductName,ProductKey,SerialNumber,ProductType,ProductDetail,ProductIsActive,ImagePath,ImageFile")] ProductModel product)
        {
            if (ModelState.IsValid)
            {
                //string fileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                //string Extension = Path.GetExtension(product.ImageFile.FileName);
                //string newfileName = fileName + DateTime.Now.ToString("yymmssfff") + Extension;
                //product.ImagePath = "~/Uploads/" + newfileName;

                //newfileName = Path.Combine(Server.MapPath("~/Uploads/"), newfileName);
                //product.ImageFile.SaveAs(newfileName);

                //db.ImgFiles.Add(new ImgFile
                //{
                //    Name = fileName,
                //    ContentType = Extension,
                //    Path = product.ImagePath,
                //    ProductID = product.ProductId,
                //    CreatedBy = UserControl.UserID,
                //    CreatedDate = DateTime.Now
                //});

                if (product.ImageFile != null)
                {
                    if (UploadImage(product.ProductId, product.ImageFile) != "Done")
                        product.ErrorMessage = "Problem in uploading image..";
                }

                var Prod_db = db.Products.Find(product.ProductId);
                Prod_db.Product_Type = product.ProductType;
                Prod_db.Product_Detail = product.ProductDetail;
                Prod_db.IsActive = product.ProductIsActive;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public string UploadImage(long ProductID, HttpPostedFileBase ImageFile)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                if(fileName.Length > 50)
                {
                    fileName = fileName.Substring(0, 50);
                }
                string Extension = Path.GetExtension(ImageFile.FileName);
                string newfileName = fileName + DateTime.Now.ToString("yymmssfff") + Extension;
                string ImagePath = "~/Uploads/" + newfileName;

                newfileName = Path.Combine(Server.MapPath("~/Uploads/"), newfileName);
                ImageFile.SaveAs(newfileName);

                db.ImgFiles.Add(new ImgFile
                {
                    Name = fileName,
                    ContentType = Extension,
                    Path = ImagePath,
                    ProductID = ProductID,
                    CreatedBy = UserControl.UserID,
                    CreatedDate = DateTime.Now
                });

                db.SaveChanges();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return "Done";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
