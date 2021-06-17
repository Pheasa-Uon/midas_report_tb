namespace Report.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string imagePath { get; set; }
        public bool isSuperAdmin { get; set; }
        
    }
}