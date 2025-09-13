using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    public class UserAccount
    {
        public UserAccount(string username, string password)
        {
            Username = username;
            Password = password;
        }

        String Username { get; set; }
        String Password { get; set; }
    }
}
