using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Report.Models
{
    public class Procedure
    {
        public string field_name { get; set; }
        public MySqlDbType sql_db_type { get; set; }
        public string value_name { get; set; }
    }
}