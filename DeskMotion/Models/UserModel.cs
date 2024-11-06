namespace DeskMotion.Models
{
    public class User
    {
        private string passwordHash;
        public int UserId { get; set;}
        public string? Name { get; set;}
        public string? Email { get; set;}
        public string? PasswordHash 
            { 
                get {
                    return passwordHash;
                } 
                set {
                    this.passwordHash = ""; //change after implementing veryfication         
                }
            }
        public Role Role { get; set;}
    }
}