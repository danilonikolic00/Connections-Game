namespace Backend.Models
{
    public class Term 
    {
        public int Id { get; set; }
        public required string TermName { get; set; }
        public required Group Group { get; set; }
    }
}