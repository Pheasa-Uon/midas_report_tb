using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Report.Models
{
    public class ReportItem
    {
        public int id { get; set; }
        public string routing { get; set; }
        public string title { get; set; }
        public List<SubItem> items { get; set; }

        public ReportItem()
        {
            this.items = new List<SubItem>();
        }
    }
}