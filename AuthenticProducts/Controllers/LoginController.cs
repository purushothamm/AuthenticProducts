using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AuthenticProducts.Models;

namespace AuthenticProducts.Controllers
{
    public class LoginController : Controller
    {
        AuthProductEntities db = new AuthProductEntities();
        // GET: Login
        public ActionResult Index()
        {
            LoginModel LogMod = new LoginModel();
            //LogMod.LoginUserTypes = db.UserTypes.Select(x => new SelectListItem { Text = x.UserTypeName, Value = x.UserTypeID.ToString() }).ToList();
            LogMod.LoginUserTypes = db.UserTypes.ToList();
            return View(LogMod);
        }

        [HttpPost]
        public ActionResult Autherize(LoginModel loginuser)
        {
            if (loginuser.UserTypeText == "admin")
            {
                var userDetail = db.Employees.FirstOrDefault(e => e.Employee_Name == loginuser.LoginUserName && e.Employee_Password == loginuser.LoginPassword);
                if (userDetail == null)
                {
                    loginuser.ErrorMessage = "Wrong User name or Password !!";
                    return View("Index", loginuser);
                }
                else
                {
                    UserControl.UserID = userDetail.Employee_ID;
                    UserControl.UserEmail = userDetail.Employee_email;
                    UserControl.UserName = userDetail.Employee_Name;
                    UserControl.UserTypeID = userDetail.Employee_Type_ID;
                    return RedirectToAction("Index", "Products");
                }
            }
            else if (loginuser.UserTypeText == "user")
            {
                var userDetail = db.Users.FirstOrDefault(u => u.UserName == loginuser.LoginUserName && u.UserPassword == loginuser.LoginPassword);
                if (userDetail == null)
                {
                    loginuser.ErrorMessage = "Wrong User name or Password !!";
                    return View("Index", loginuser);
                }
                else
                {
                    UserControl.UserID = userDetail.UserID;
                    UserControl.UserEmail = userDetail.UserEmail;
                    UserControl.UserName = userDetail.UserName;
                    UserControl.UserTypeID = 2;

                    return RedirectToAction("UserProdDetail", "Users", new {  });//id = userDetail.UserID
                }
            }
            else
            {
                loginuser.ErrorMessage = "User Type shoud be admin or user..";
                return View("Index", loginuser);
            }
            //return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}