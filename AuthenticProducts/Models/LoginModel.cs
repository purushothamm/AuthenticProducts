using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AuthenticProducts.Models
{
    public class LoginModel
    {
         public string LoginId { get; set; }

        [DisplayName("User Name")]
        [Required(ErrorMessage ="User Name is Required.")]
        public string LoginUserName { get; set; }

        [DisplayName("Password")]
        [Required(ErrorMessage ="Password is Required")]
        [DataType(DataType.Password)]
        public string LoginPassword { get; set; }

        [DisplayName("User Type")]
        public List<UserType> LoginUserTypes { get; set; }
        //public PreUserType LoginUserType { get; set; }

        [DisplayName("User Type Name")]
        [Required(ErrorMessage="User Type Missing")]
        public string UserTypeText { get; set; }
        public UserType UserTypeID { get; set; }

        public string ErrorMessage { get; set; }
    }

    public static class UserControl
    {
        public static int UserID { get; set; }
        public static string UserName { get; set; }
        public static string UserEmail { get; set; }
        public static int UserTypeID { get; set; }
    }

    public enum PreUserType
    {
        Admin,
        Client
    }
}