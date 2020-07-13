using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AuthenticProducts.Models
{
    public class ProductModel
    {
        [DisplayName("Serial Number")]
        [Required(ErrorMessage = "Serial number is rquired")]
        public string SerialNumber { get; set; }
        public long ProductId { get; set; }
        public string ErrorMessage { get; set; }
        [DisplayName("Product Name")]
        public string ProductName { get; set; }
        [DisplayName("Product Key")]
        public string ProductKey { get; set; }
        [DisplayName("Product Type")]
        public string ProductType { get; set; }
        [DisplayName ("Product Detail")]
        public string ProductDetail { get; set; }
        [DisplayName("Is Active")]
        public byte ProductIsActive { get; set; }
        [DisplayName("Upload File")]
        public string ImagePath { get; set; }
        [DisplayName("Product Image")]
        public HttpPostedFileBase ImageFile { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }
}