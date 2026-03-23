using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class HisotricalDataRepository : BaseRepository
    {
        public HisotricalDataRepository(string connectionString) : base() { }


        public int InsertHistoricalData(HistoricalData historicalData)
        {

            using (var connection = CreateConnection())
            {
                var command = new OleDbCommand("INSERT INTO HistoricalData ( YearId,StartDate,EndDate,AccountId,Balance    ) VALUES (?,?,?,?,?)", connection);
                command.Parameters.AddWithValue("@YearId", historicalData.YearId);
                command.Parameters.AddWithValue("@StartDate", historicalData.StartDate);
                command.Parameters.AddWithValue("@EndDate", historicalData.EndDate);
                command.Parameters.AddWithValue("@AccountId", historicalData.AccountId);
                command.Parameters.AddWithValue("@Balance", historicalData.Balance);

                command.Parameters.AddWithValue("?", historicalData.YearId);
                command.Parameters.AddWithValue("?", historicalData.StartDate);
                command.Parameters.AddWithValue("?", historicalData.EndDate);
                command.Parameters.AddWithValue("?", historicalData.AccountId);
                command.Parameters.AddWithValue("?", historicalData.Balance);


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
