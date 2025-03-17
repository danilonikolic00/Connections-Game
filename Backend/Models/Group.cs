namespace Backend.Models
{
    public class Group 
    {
        public int GroupId { get; set; }
        public required string GroupName { get; set; }
        public List<Term>? Terms { get; set; }
    }
}