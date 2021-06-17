using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Report.Models
{
    public class SubItem
    {
        public int id { get; set; }
        public string routing { get; set; }
        public string title { get; set; }
        public int parent_id { get; set; }
    }
}