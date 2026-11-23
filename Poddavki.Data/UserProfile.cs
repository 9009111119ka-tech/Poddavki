
namespace Poddavki.Data
{
    public class UserProfile
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}
