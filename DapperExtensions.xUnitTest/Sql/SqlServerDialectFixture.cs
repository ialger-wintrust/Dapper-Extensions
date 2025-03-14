using DapperExtensions.Predicate;
using DapperExtensions.Sql.Dialects;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.Sql
{
    public static class SqlServerDialectFixture
    {
        public abstract class SqlServerDialectFixtureBase
        {
            protected SqlServerDialectFixtureBase()
            {
                Dialect = new SqlServerDialect();
            }

            protected SqlServerDialect Dialect;
        }

        public class DatabaseFunctions : SqlServerDialectFixtureBase
        {
            [Fact]
            public void DatabaseFunctionTests()
            {
                var actualDialect = Dialect.GetDatabaseFunctionString(DatabaseFunction.None, "foo");
                var actualNullDialect = Dialect.GetDatabaseFunctionString(DatabaseFunction.NullValue, "foo", "newFoo");
                var actualTruncateDialect = Dialect.GetDatabaseFunctionString(DatabaseFunction.Truncate, "foo");
                Assert.Equal("foo", actualDialect, StringComparer.InvariantCultureIgnoreCase);
                Assert.Equal("IsNull(foo, newFoo)", actualNullDialect, StringComparer.InvariantCultureIgnoreCase);
                Assert.Equal("Truncate(foo)", actualTruncateDialect, StringComparer.InvariantCultureIgnoreCase);
            }
        }

        public class Properties : SqlServerDialectFixtureBase
        {
            [Fact]
            public void CheckSettings()
            {
                Assert.Equal('[', Dialect.OpenQuote);
                Assert.Equal(']', Dialect.CloseQuote);
                Assert.Equal(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.Equal('@', Dialect.ParameterPrefix);
                Assert.True(Dialect.SupportsMultipleStatements);
            }
        }

        public class GetPagingSqlMethod : SqlServerDialectFixtureBase
        {
            [Fact]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(null, 0, 10, new Dictionary<string, object>(), ""));
                Assert.Equal("SQL", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
                Assert.Contains("cannot be null", ex.Message);
            }

            [Fact]
            public void EmptySql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql(string.Empty, 0, 10, new Dictionary<string, object>(), ""));
                Assert.Equal("SQL", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
                Assert.Contains("cannot be null", ex.Message);
            }

            [Fact]
            public void NullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetPagingSql("SELECT [schema].[column] FROM [schema].[table]", 0, 10, null, ""));
                Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
                Assert.Contains("cannot be null", ex.Message);
            }

            [Fact]
            public void NotSelect_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentException>(() => Dialect.GetPagingSql("INSERT INTO TABLE (ID) VALUES (1)", 1, 10, new Dictionary<string, object>(), ""));
                Assert.Equal("SQL", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
                Assert.Contains("must be a SELECT statement", ex.Message);
            }

            [Fact]
            public void Select_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT [column] FROM [schema].[table] ORDER BY CURRENT_TIMESTAMP OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table]", 0, 10, parameters, "");
                Assert.Equal(sql, result);
                Assert.Equal(2, parameters.Count);
                Assert.Equal(parameters["@skipRows"], 0);
                Assert.Equal(parameters["@maxResults"], 10);
            }

            [Fact]
            public void SelectDistinct_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT DISTINCT [column] FROM [schema].[table] ORDER BY CURRENT_TIMESTAMP OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT DISTINCT [column] FROM [schema].[table]", 0, 10, parameters, "");
                Assert.Equal(sql, result);
                Assert.Equal(2, parameters.Count);
                Assert.Equal(parameters["@skipRows"], 0);
            }

            [Fact]
            public void SelectOrderBy_ReturnsSql()
            {
                var parameters = new Dictionary<string, object>();
                const string sql = "SELECT [column] FROM [schema].[table] ORDER BY [column] DESC OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY";
                var result = Dialect.GetPagingSql("SELECT [column] FROM [schema].[table] ORDER BY [column] DESC", 0, 10, parameters, "");
                Assert.Equal(sql, result);
                Assert.Equal(2, parameters.Count);
                Assert.Equal(parameters["@skipRows"], 0);
            }
        }

        public class GetOrderByClauseMethod : SqlServerDialectFixtureBase
        {
            [Fact]
            public void NoOrderBy_Returns()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table");
                Assert.Null(result);
            }

            [Fact]
            public void OrderBy_ReturnsItemsAfterClause()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC");
                Assert.Equal("ORDER BY Column1 ASC, Column2 DESC", result);
            }

            [Fact]
            public void OrderByWithWhere_ReturnsOnlyOrderBy()
            {
                var result = Dialect.TestProtected().RunMethod<string>("GetOrderByClause", "SELECT * FROM Table ORDER BY Column1 ASC, Column2 DESC WHERE Column1 = 'value'");
                Assert.Equal("ORDER BY Column1 ASC, Column2 DESC", result);
            }
        }

        public class GetIdentitySqlMethod : SqlServerDialectFixtureBase
        {
            [Fact]
            public void nullTableIdentity_ShouldReturnABigIntCastedScopeIdentitySql()
            {
                var expectedIdentitySql = "SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [Id]";

                var result = Dialect.GetIdentitySql(null);
                Assert.Equal(expectedIdentitySql, result);
            }

            [Fact]
            public void LongTableIdentity_ShouldReturnABigIntCastedScopeIdentitySql()
            {
                var expectedIdentitySql = "SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [Id]";

                var result = Dialect.GetIdentitySql(typeof(long));
                Assert.Equal(expectedIdentitySql, result);
            }

            [Fact]
            public void ShortTableIdentity_ShouldReturnASmallIntCastedScopeIdentitySql()
            {
                var expectedIdentitySql = "SELECT CAST(SCOPE_IDENTITY() AS SMALLINT) AS [Id]";

                var result = Dialect.GetIdentitySql(typeof(short));
                Assert.Equal(expectedIdentitySql, result);
            }

            [Fact]
            public void IntTableIdentity_ShouldReturnAnIntCastedScopeIdentitySql()
            {
                var expectedIdentitySql = "SELECT CAST(SCOPE_IDENTITY() AS INT) AS [Id]";

                var result = Dialect.GetIdentitySql(typeof(int));
                Assert.Equal(expectedIdentitySql, result);
            }
        }
    }
}