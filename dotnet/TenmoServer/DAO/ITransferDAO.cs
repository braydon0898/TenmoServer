using System.Collections.Generic;
using System.Net;
using TenmoServer.Models;


namespace TenmoServer.DAO
{
    public interface ITransferDAO
    {
        bool TransferFunds(decimal transactionAmount, int currentUserId, int recipient);
        bool AddTransfer(decimal transferAmount, int transfer_type_id, int transfer_status_id, int account_from, int account_to);
        List<Transfer> ShowUserTransfers(int currentUserId);
        Transfer GetTransfer(int transferId);
    }
}
