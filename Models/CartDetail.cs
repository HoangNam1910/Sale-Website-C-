﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sky.Models
{
    [Table("CartDetail")]
    public class CartDetail
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }



        [Required]
        [DisplayName("OrderProductName")]
        public string OrderProductName { set; get; }



        public Product Product { set; get; }

        public Cart Cart { set; get; }
    }
}
