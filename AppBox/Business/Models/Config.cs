using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AppBox
{
    public class Config : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(50)]
        public string ConfigKey { get; set; }

        [Required, StringLength(4000)]
        public string ConfigValue { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }
    }
}