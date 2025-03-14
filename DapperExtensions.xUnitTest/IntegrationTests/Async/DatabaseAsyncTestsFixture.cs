using System.Data.Common;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Sql.Dialects;

namespace DapperExtensions.xUnitTest.IntegrationTests.Async
{
    public abstract class DatabaseAsyncTestsFixture : DatabaseTestsFixture
    {
        protected DatabaseAsyncTestsFixture(string configPath = null) : base(configPath)
        {
        }

        public new IAsyncDatabase Db { get; private set; }

        protected override void CommonSetup(DbConnection connection, SqlDialectBase sqlDialect)
        {
            var config = DapperAsyncExtensions.Configure(typeof(AutoClassMapper<>), new List<Assembly>(), sqlDialect);
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new AsyncDatabase(connection, sqlGenerator);
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                Db.Dispose();
            }
        }
    }
}