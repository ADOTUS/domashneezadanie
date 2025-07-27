using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        private readonly string _connectionString;

        public DataContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(_connectionString);
        }
    }
}
