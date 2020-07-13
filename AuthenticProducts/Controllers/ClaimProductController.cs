using AuthenticProducts.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AuthenticProducts.Controllers
{
    public class ClaimProductController : Controller
    {
        private AuthProductEntities db = new AuthProductEntities();
        private EthAddressesController ethac = new EthAddressesController();
        // GET: ClaimProduct
        public ActionResult Index()
        {
            return View();
        }

        // GET: ClaimProduct/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ClaimProduct/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Claim(int id)
        {
            ProductEthAddress_V prod = new ProductEthAddress_V();
            prod.UserID = id;
            return View(prod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult Claim([Bind(Include = "Product_Name,Product_Key, EthPublicKey, EthPrivateKey")] ProductEthAddress_V ProdEA)
        {
            if (ModelState.IsValid)
            {
                ProductEthAddress_V prod_v = db.ProductEthAddress_V.FirstOrDefault(p => p.Product_Name == ProdEA.Product_Name && p.Product_Key == ProdEA.Product_Key 
                && p.EthPublicKey == ProdEA.EthPublicKey && p.EthPrivateKey == ProdEA.EthPrivateKey);
                if(prod_v != null)
                {
                    if (prod_v.UserID == 0 || prod_v.UserID == null)
                    {
                        ResultModel bddResult = BlockchainAndDB(ref prod_v);
                        if (bddResult._Result)
                        {
                            ProdEA.ErrorMessage = "Claim is Successfull..";
                        }
                        else
                        {
                            ProdEA.ErrorMessage = "Claim is not Successfull. Error: " + bddResult._ResultError;
                        }
                    }
                    else
                    {
                        ProdEA.ErrorMessage = "This produect is already claimed by other User, you can't reclaim this product. Please contact the owner of the product";
                    }
                }
                else
                {
                    ProdEA.ErrorMessage = "OOPS!!!. Product Details are not matching. Please enter the correct detail.";
                    return View(ProdEA);
                }
            }
            return View(ProdEA);
        }

        [HttpGet]
        public ActionResult DirectClaim(long ProdId)
        {
            if (ProdId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductEthAddress_V product = db.ProductEthAddress_V.FirstOrDefault(p => p.Product_ID == ProdId);
            if (product == null)
            {
                return HttpNotFound();
            }
            string imgpath = db.ImgFiles.Where(i => i.ProductID == ProdId).Select(i => i.Path).FirstOrDefault();
            product.ImagePath = imgpath == null? "" : imgpath;
            product.EthPrivateKey = null;
            product.Product_Key = null;

            product.IsDirectClaim = 1;
            product.UserID = product.UserID == null ? 0 : product.UserID;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult DirectClaim(ProductEthAddress_V ProdEA)
        {
            var TransResult = Task.Run<Models.Transaction>(async () => await ethac.GetTransactionByHash(ProdEA.TransactionHash_First)).Result;
            
            var prodcheck = db.ProductEthAddress_V.FirstOrDefault(p => p.Product_ID == ProdEA.Product_ID);
            string EthProductKey = TransResult.InputDataText.Count() == 0 ? prodcheck.Product_Key : TransResult.InputDataText;

            if (EthProductKey == ProdEA.Enter_ProductKey)
            {
                ResultModel bddResult = BlockchainAndDB(ref prodcheck);
                if (bddResult._Result)
                {
                    prodcheck.ErrorMessage = "Claim is Successfull..";
                }
                else
                {
                    prodcheck.ErrorMessage = "Claim is not Successfull. Error: " + bddResult._ResultError;
                }
            }
            else
            {
                prodcheck.ErrorMessage = "Please enter the correct detail to claim the product..";
                prodcheck.UserID = ProdEA.UserID;
                prodcheck.IsDirectClaim = ProdEA.IsDirectClaim;
                prodcheck.EthPrivateKey = null;
                prodcheck.Product_Key = null;
            }
            return View(prodcheck);
        }

        private ResultModel BlockchainAndDB(ref ProductEthAddress_V prod_v)
        {
            string product_key = prod_v.Product_Key;
            string Private_key = prod_v.EthPrivateKey;
            string Public_key = prod_v.EthPublicKey;
            Decimal Amount = 0.00015m;
            ResultModel result = new ResultModel();

            try
            {
                EthAddressesController ethac = new EthAddressesController();
                Decimal GetBalance = Task.Run<Decimal>(async () => await ethac.GetAccountBalance(Public_key)).Result;
                if(Amount >= GetBalance)
                {
                    result._Result = false;
                    result._ResultError = "No Sufficient Balance in ETH Account";

                    return result;
                }

                Models.Transaction TR = Task.Run<Models.Transaction>(async () => await ethac.SendTransactionAndData(ethac.MasterEthPublicKey, Amount, product_key, Private_key)).Result;
                //Task.Run<Models.Transaction>(async () => await ethac.SendTransaction(ethac.MasterEthPublicKey, 0.00015m, prod_v.EthPrivateKey)).Result;
                TR.ProductID = prod_v.Product_ID;
                TR.CreatedBy = UserControl.UserID;
                TR.UserTypeID = UserControl.UserTypeID;
                TR.EthAddressID = prod_v.EthAddressID ?? default(int);
                TR.CreatedDate = DateTime.Now;
                db.Transactions.Add(TR);
                db.SaveChanges();

                var product_db = db.Products.Find(prod_v.Product_ID);
                product_db.UserID = UserControl.UserID;
                product_db.TransactionID = TR.TransactionID;
                db.SaveChanges();

                result._ResultTransHash = TR.TransactionHash;
                result._Result = true;

                prod_v.UserID = product_db.UserID;
                prod_v.TransactionID = product_db.TransactionID;
            }
            catch (Exception ex)
            {
                result._Result = false;
                result._ResultError = ex.Message;

                return result;
            }

            return result;
        }
    }
}
