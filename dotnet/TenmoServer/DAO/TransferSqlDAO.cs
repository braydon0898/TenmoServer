using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDAO : ITransferDAO
    {

        public decimal CurrentBalance { get; set; }
        public decimal RecipientBalance { get; set; }
        private readonly string connectionString;

        public TransferSqlDAO (string dbconnectionString)
        {
            connectionString = dbconnectionString;
        }

        public bool TransferFunds(decimal transactionAmount, int currentUserId, int recipient)
        {


            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT balance FROM accounts WHERE user_id=@currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

                    CurrentBalance = Convert.ToDecimal(cmd.ExecuteScalar());

                    if (CurrentBalance < transactionAmount)
                    {
                        return false;
                    }

                    cmd = new SqlCommand("UPDATE accounts SET balance = @CurrentBalance WHERE user_id = @currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    cmd.Parameters.AddWithValue("@CurrentBalance", CurrentBalance -= transactionAmount);

                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT balance FROM accounts WHERE user_id= @recipient", conn);
                    cmd.Parameters.AddWithValue("@recipient", recipient);

                    RecipientBalance = Convert.ToDecimal(cmd.ExecuteScalar());


                    cmd = new SqlCommand("UPDATE accounts SET balance = @RecipientBalance WHERE user_id = @recipient", conn);
                    cmd.Parameters.AddWithValue("@recipient", recipient);
                    cmd.Parameters.AddWithValue("@RecipientBalance", RecipientBalance += transactionAmount);

                    cmd.ExecuteNonQuery();

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return true;
        }
        public bool AddTransfer(decimal transferAmount, int transfer_type_id, int transfer_status_id, int account_from, int account_to)
        {
            int result;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) 
                                                    VALUES(@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount)", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", transfer_type_id);
                    cmd.Parameters.AddWithValue("@transfer_status_id", transfer_status_id);
                    cmd.Parameters.AddWithValue("@account_from", account_from);
                    cmd.Parameters.AddWithValue("@account_to", account_to);
                    cmd.Parameters.AddWithValue("@amount", transferAmount);

                    result = cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
            return result > 0;
        }

        public List<Transfer> ShowUserTransfers(int currentUserId)
        {

            List<Transfer> transfers = new List<Transfer>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT transfer_id, transfer_type_id, ts.transfer_status_id, account_from, account_to, amount, u.user_id, 
                                                    u.username as accountFrom, (SELECT username FROM users WHERE user_id = account_to) as accountTo,
                                                    ts.transfer_status_desc
                                                    FROM transfers t
                                                    INNER JOIN accounts a ON t.account_from = a.account_id
                                                    INNER JOIN users u ON a.user_id = u.user_id
                                                    INNER JOIN transfer_statuses ts ON t.transfer_status_id = ts.transfer_status_id
                                                    WHERE account_from = @currentUserId OR account_to = @currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Transfer t = GetTransferFromReader(reader);
                            transfers.Add(t);
                        }

                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfers;
        }

        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer t = new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]),
                TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]),
                AccountFrom = Convert.ToInt32(reader["account_from"]),
                AccountTo = Convert.ToInt32(reader["account_to"]),
                Amount = Convert.ToDecimal(reader["amount"]),
                AccountFromName = Convert.ToString(reader["accountFrom"]),
                AccountToName = Convert.ToString(reader["accountTo"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                TransferStatusName = Convert.ToString(reader["transfer_status_desc"])

            };

            return t;
        }
        public Transfer GetTransfer(int transferId)
        {
            Transfer transfer = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT transfer_id, transfer_type_id, ts.transfer_status_id, account_from, account_to, amount, u.user_id, 
                                                    u.username as accountFrom, (SELECT username FROM users WHERE user_id = account_to) as accountTo,
                                                    ts.transfer_status_desc
                                                    FROM transfers t
                                                    INNER JOIN accounts a ON t.account_from = a.account_id
                                                    INNER JOIN users u ON a.user_id = u.user_id
                                                    INNER JOIN transfer_statuses ts ON t.transfer_status_id = ts.transfer_status_id
                                                    WHERE transfer_id = @transferId", conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        transfer = GetTransferFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfer;
        }
    }
}
