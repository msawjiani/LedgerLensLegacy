using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class AccountingYearRepository : BaseRepository
    {
        public AccountingYearRepository(string connectionString) : base() { }


        public int InsertYear(AccountingYear accountingYear)
        {

            using (var connection = CreateConnection())
            {
                var command = new OleDbCommand("INSERT INTO AccountYears (StartDate, EndDate) VALUES (?,?)", connection);
                command.Parameters.AddWithValue("@StartDate", accountingYear.StartDate);
                command.Parameters.AddWithValue("@EndDate", accountingYear.EndDate);
                command.Parameters.AddWithValue("?", accountingYear.StartDate);
                command.Parameters.AddWithValue("?", accountingYear.EndDate);
                connection.Open();
                command.ExecuteNonQuery();

                command.CommandText = "SELECT @@IDENTITY";
                object result = command.ExecuteScalar();
                int id = Convert.ToInt32(result);

                return id;
            }
        }
    }

}
