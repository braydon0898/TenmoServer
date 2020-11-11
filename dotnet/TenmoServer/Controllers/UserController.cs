using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TenmoServer.DAO;
using TenmoServer.Models;
using TenmoServer.Security;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserDAO userDao;

        private readonly ITransferDAO transferDAO;

        private int GetCurrentUserId()
        {
            string userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return -1;
            int.TryParse(userId, out int userIdInt);
            return userIdInt;
        }
        public UserController(IUserDAO dao, ITransferDAO tdao)
        {
            userDao = dao;
            transferDAO = tdao;

        }
      

        [HttpGet]

        public List<User> GetUsers()
        {
            return userDao.GetUsers();
        }

        [HttpGet("/user/{userName}")]

        public User GetUser(string userName)
        {
            return userDao.GetUser(userName);
        }
        
        

        [HttpGet("/user/balance")]

        public ActionResult<decimal> GetUserBalance()
        {
            
            return Ok(userDao.GetCurrentBalance(GetCurrentUserId()));
        }
        [HttpGet("/user/transfers")]
        public List<Transfer>ShowTransfers ()
        {

            return transferDAO.ShowUserTransfers(GetCurrentUserId());
        }
        [HttpGet("/user/transfers/{transferId}")]
        public Transfer ShowTransfer(int transferId)
        {
            return transferDAO.GetTransfer(transferId);
        }


        [HttpPut("/user/{transactionAmount}/{recipient}")]
        public ActionResult<bool> TransferFunds(decimal transactionAmount, int recipient)
        {
            
            int transactionType = 1;
            int transferStatusID = 2;

            bool result = transferDAO.TransferFunds(transactionAmount, GetCurrentUserId(), recipient);
            transferDAO.AddTransfer(Math.Abs(transactionAmount), transactionType, transferStatusID, GetCurrentUserId(), recipient);
            return Ok(result);
        }
    }
}
