using Sky.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sky.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [DisplayName("ReciverName")]
        public string ReciverName { get; set; }
        [Required]
        [DisplayName("ReciverAddress")]
        public string ReciverAddress { get; set; }
        [Required]
        [DisplayName("ReciverPhone")]
        public string ReciverPhone { get; set; }
        [Required]
        [DisplayName("ReciverEmail")]
        public string ReciverEmail { get; set; }




        [Required]
        [DisplayName("OrderStatus")]
        public string OrderStatus { set; get; }
        [Required]
        [DisplayName("OrderNote")]
        public string OrderNote { set; get; }
        [Required]
        [DisplayName("OrderDate")]
        public DateTime OrderDate { set; get; }
        [Required]
        [DisplayName("OrderPrice")]
        public int OrderPrice { set; get; }



        [Required]
        //Khóa chính, khóa ngoại
        public string UserId { set; get; }


        //1 Đơn hàng có nhiều chi tiết, Bảng chi tiết là bảng trung gian của: Quan hệ n-n DonHang và SanPham
        public List<OrderDetail> OrderDetails { set; get; }

    }
}
