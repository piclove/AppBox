using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppBox
{
    public class Online : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        public string IPAdddress { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        
        public virtual User User { get; set; }


    }
}