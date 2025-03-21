using System.Diagnostics.CodeAnalysis;
using DapperExtensions.Sql.Dialects;
using Microsoft.Data.SqlClient;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer
{
    public class SqlServerBaseFixture : DatabaseTestsFixture
    {
        public SqlServerBaseFixture()
        {
            Setup();
        }

        [ExcludeFromCodeCoverage]
        private SqlConnection SetupDatabase()
        {
            var connection = new SqlConnection(ConnectionString("SqlServerDBA"));

            ExecuteScripts(connection, true, "Setup");

            connection.Close();

            return new SqlConnection(ConnectionString("SqlServer"));
        }

        public void Setup()
        {
            var connection = new SqlConnection(ConnectionString("SqlServer"));

            try
            {
                CommonSetup(connection, new SqlServerDialect());
            }
            catch (SqlException ex)
            {
                if (ex.Number == 18456)
                {
                    connection = SetupDatabase();
                    CommonSetup(connection, new SqlServerDialect());
                }
                else
                    throw;
            }

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}