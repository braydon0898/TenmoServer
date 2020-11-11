using System.Collections.Generic;
using System.Net;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDAO
    {
        User GetUser(string userName);
        User AddUser(string username, string password);
        List<User> GetUsers();

        decimal GetCurrentBalance(int userId);
             
    }
}
