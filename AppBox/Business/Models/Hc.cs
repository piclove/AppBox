using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppBox
{
    public partial class Hc : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(255)]
        public string part_name { get; set; }

        [Required, StringLength(255)]
        public string pn { get; set; }

        [Required, StringLength(255)]
        public string sn { get; set; }

        [StringLength(10)]
        public string part_type { get; set; }

        [StringLength(10)]
        public string condition { get; set; }    //全新件 New、返修件 ReNew、待修件 Rep、待报废件 Scr

        [StringLength(20)]
        public string loc { get; set; }

        public int onhand { get; set; }

        public string part_des { get; set; }

        public string remark { get; set; }

        public DateTime? rec_date { get; set; }    //接收时间

        [StringLength(20)]
        public string rec_name { get; set; }    //接收人
    }
}
