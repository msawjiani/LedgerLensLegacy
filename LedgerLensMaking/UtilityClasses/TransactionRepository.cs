using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class TransactionRepository : BaseRepository
    {
        public TransactionRepository(string connectionString) : base() { }
        public int InsertGeneralLedgerTransaction(GeneralLedger transaction)
        {

            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transactionScope = connection.BeginTransaction())
                {
                    try
                    {
                        var command = new OleDbCommand("INSERT INTO GeneralLedger (AccountId, YearId, Unix, TDate, Ref, Particulars, Amount, TransactionType, Narration) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", connection, transactionScope);

                        // Explicitly define parameter types
                        command.Parameters.Add(new OleDbParameter("AccountId", OleDbType.Integer) { Value = transaction.AccountId });
                        command.Parameters.Add(new OleDbParameter("YearId", OleDbType.Integer) { Value = transaction.YearId });
                        command.Parameters.Add(new OleDbParameter("Unix", OleDbType.Integer) { Value = transaction.Unix });
                        command.Parameters.Add(new OleDbParameter("TDate", OleDbType.Date) { Value = transaction.Tdate });
                        command.Parameters.Add(new OleDbParameter("Ref", OleDbType.VarChar) { Value = transaction.Ref });
                        command.Parameters.Add(new OleDbParameter("Particulars", OleDbType.VarChar) { Value = transaction.Particulars });
                        command.Parameters.Add(new OleDbParameter("Amount", OleDbType.Decimal) { Value = transaction.Amount });
                        command.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                        command.Parameters.Add(new OleDbParameter("Narration", OleDbType.VarChar) { Value = transaction.Narration });

                        command.ExecuteNonQuery();
                        transactionScope.Commit();
                    }
                    catch (Exception)
                    {
                        transactionScope.Rollback();
                        throw;
                    }
                }

                // After committing the transaction, retrieve the identity value
                using (var identityCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
                {
                    object result = identityCommand.ExecuteScalar();
                    int newTransactionId = Convert.ToInt32(result);
                    return newTransactionId;
                }
            }
        }
        public void InsertSubledgerTransaction(SubledgerTrans subledgerTrans, int transactionId)
        {
            using (var connection = CreateConnection())
            {
                // I dont know why transactionid is needed here maybe required when making FD
                var command = new OleDbCommand("INSERT INTO SubledgerTrans (SubledgerId, TransactionId, Unix) VALUES (?, ?, ?)", connection);
                command.Parameters.AddWithValue("?", subledgerTrans.SubledgerId);
                command.Parameters.AddWithValue("?", transactionId);
                command.Parameters.AddWithValue("?", subledgerTrans.Unix);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void InsertSharePurchaseTransaction(SharePurchases sharePurchases, int transactionId)
        {
            using (var connection = CreateConnection())
            {
                var command = new OleDbCommand("INSERT INTO SharePurchases (ShareId,PurchaseDate, TransactionId, Unix,QtyPurchased,PurchaseRate   ) VALUES (?, ?, ?,?,?,?)", connection);
                command.Parameters.AddWithValue("?", sharePurchases.ShareId);
                command.Parameters.AddWithValue("?", sharePurchases.PurchaseDate);
                command.Parameters.AddWithValue("?", sharePurchases.TransactionId);
                command.Parameters.AddWithValue("?", sharePurchases.Unix);
                command.Parameters.AddWithValue("?", sharePurchases.QtyPurchased);
                command.Parameters.AddWithValue("?", sharePurchases.PurchaseRate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void InsertShareSaleTransactiion(SharesSales shareSales)
        {
            using (var connection = CreateConnection())
            {
                var command = new OleDbCommand("INSERT INTO ShareSales (ShareId,PurchaseTransId, QtySold, SellingPrice,TransactionId,Unix   ) VALUES (?, ?, ?,?,?,?)", connection);
                command.Parameters.AddWithValue("?", shareSales.ShareId);
                command.Parameters.AddWithValue("?", shareSales.PurchaseTransId);
                command.Parameters.AddWithValue("?", shareSales.QtySold);
                command.Parameters.AddWithValue("?", shareSales.SellingPrice);
                command.Parameters.AddWithValue("?", shareSales.TransactionId);
                command.Parameters.AddWithValue("?", shareSales.Unix);


                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
