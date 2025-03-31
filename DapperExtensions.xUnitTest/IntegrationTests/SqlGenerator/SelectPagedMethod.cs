using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.DbModels;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

public class SelectPagedMethod : SqlGeneratorFixtureBase
{
    [Fact]
    public void WithNoSort_ThrowsException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Generator.SelectPaged(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
        Assert.Contains("null or empty", ex.Message);
    }

    [Fact]
    public void WithEmptySort_ThrowsException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Generator.SelectPaged(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
        Assert.Contains("null or empty", ex.Message);
        Assert.Equal("Sort", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void WithNullParameters_ThrowsException()
    {
        var sort = new Sort();
        var ex = Assert.Throws<ArgumentNullException>(
            () => Generator.SelectPaged(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
        Assert.Contains("cannot be null", ex.Message);
        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void WithSort_GeneratesSql()
    {
        var expectedSql = """Select Id, Username From [UserGuid] [y_1] ORDER BY [y_1].[Id] ASC OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY""";
        var userGuidClassMapper = Generator.Configuration.GetMap<UserGuid>();
        IDictionary<string, object> parameters = new Dictionary<string, object>();
        ISort[] sorts = [Predicates.Sort<UserGuid>(nameof(UserGuid.Id))];

        var actualResults = Generator.SelectPaged(userGuidClassMapper, null, sorts, 2, 10, parameters, null);
        Assert.Equal(expectedSql, actualResults, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void WithPredicateAndSort_GeneratesSql()
    {
        var expectedSql = """Select Id, Username From [UserGuid] [y_1] WHERE ([y_1].[Username] LIKE @Username_0) ORDER BY [y_1].[Id] ASC OFFSET (@skipRows) ROWS FETCH NEXT @maxResults ROWS ONLY""";
        var userGuidClassMapper = Generator.Configuration.GetMap<UserGuid>();
        IDictionary<string, object> parameters = new Dictionary<string, object>();
        ISort[] sorts = [Predicates.Sort<UserGuid>(nameof(UserGuid.Id))];
        var predicate = Predicates.Field<UserGuid>(userGuid => userGuid.Username, Operator.Like, "test");

        var actualResults = Generator.SelectPaged(userGuidClassMapper, predicate, sorts, 2, 10, parameters, null);
        Assert.Equal(expectedSql, actualResults, StringComparer.InvariantCultureIgnoreCase);
    }
}