using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository()
        {
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
            _connectionString = connectionString;
        }

        protected OleDbConnection CreateConnection()
        {
            return new OleDbConnection(_connectionString);
        }
    }
}
