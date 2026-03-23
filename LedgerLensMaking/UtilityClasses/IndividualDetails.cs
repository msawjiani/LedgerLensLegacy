using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class IndividualDetails
    {
        public Individual GetIndividual()
        {
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Individual";

                using (OleDbCommand command = new OleDbCommand(query, connection))
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)

                    {
                        if (reader.Read())
                        {
                            Individual individual = new Individual
                            {
                                IndividualName = reader.GetString(0),
                                PANNumber = reader.GetString(1),
                                PhotoFile = reader.GetString(2),
                                InterestAccountCode = reader.GetInt32(3),
                                InterestAccountDesc = reader.GetString(4),
                                LTCGAccountCode= reader.GetInt32(5),
                                LTCGAccountDesc = reader.GetString(6),
                                STCGAccountCode = reader.GetInt32(7),
                                STCGAccountDesc = reader.GetString(8),
                                LTCLAccountCode = reader.GetInt32(9),
                                LTCLAccountDesc = reader.GetString(10),
                                STCLAccountCode = reader.GetInt32(11),
                                STCLAccountDesc = reader.GetString(12),
                                RetainedEarningsId = reader.GetInt32(13),
                                // Map other columns as needed
                            };
                            return individual;
                        }
                    }
                }
            }
            return null; // No data found
        }

    }
}
