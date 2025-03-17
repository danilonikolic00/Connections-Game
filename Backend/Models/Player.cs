namespace Backend.Models
{
    public class Player : User
    {
        public double Played { get; set; }
        public double Solved { get; set; }
        public double SuccessPercentage { get; set; }
    }
}