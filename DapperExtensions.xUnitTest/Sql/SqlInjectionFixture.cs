using DapperExtensions.Sql;
using NUnit.Framework;
using Assert = Xunit.Assert;

namespace DapperExtensions.xUnitTest.Sql
{
    [Parallelizable(ParallelScope.All)]
    public static class SqlInjectionFixture
    {
        public abstract class SqlInjectionFixtureBase
        {
            protected IDapperExtensionsConfiguration Configuration = new DapperExtensionsConfiguration();
        }

        public class SqlInjectionTest : SqlInjectionFixtureBase
        {
            [Fact]
            public void NotExisting_WithoutValue_ReturningNull()
            {
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest));

                Assert.Null(result);
            }

            [Fact]
            public void NotExisting_WithValue_ReturningNull()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);

                Assert.NotNull(result);
                Assert.Equal(value, result);
                Assert.Equal("foo", value.Property);
                Assert.Equal("select foo from dual", value.Sql);
                Assert.Equal(value.EntityType, result.EntityType);
            }

            [Fact]
            public void Existing_WithoutValue_ReturningValue()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                _ = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), null);

                Assert.Equal(value, result);
            }

            [Fact]
            public void Existing_WithValue_ReturningNull()
            {
                var value = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "foo", Sql = "select foo from dual" };
                _ = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), value);
                var newValue = new SqlInjection { EntityType = typeof(SqlInjectionTest), Property = "bar", Sql = "select bar from dual" };
                var result = Configuration.GetOrSetSqlInjection(typeof(SqlInjectionTest), newValue);

                Assert.Equal(value, result);
            }
        }
    }
}