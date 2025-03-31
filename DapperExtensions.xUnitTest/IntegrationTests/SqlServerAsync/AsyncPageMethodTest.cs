using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncPageMethodTest : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task Page_UsingExtensionInvocation_ShouldReturnsMatchingSortSkipAndTake()
    {
        var person1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        var list = await Db.Connection.PageAsync<Person>(null, sort, 0, 2);
        Assert.NotNull(list);
        Assert.Equal(2, list.Count());
        Assert.Equal(person2.Id, list.First().Id);
        Assert.Equal(person1.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public async Task UsingNullPredicate_ReturnsMatching()
    {
        var person1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = await Db.PageAsync<Person>(null, sort, 0, 2);
        Assert.Equal(2, list.Count());
        Assert.Equal(person2.Id, list.First().Id);
        Assert.Equal(person1.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public async Task UsingPredicate_ReturnsMatching()
    {
        var person1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = await Db.PageAsync<Person>(predicate, sort, 0, 2);
        Assert.Equal(2, list.Count());
        Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta" || p.FirstName == "Iota"));
    }

    [Fact]
    public async Task NotFirstPage_Returns_NextResults()
    {
        var person1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = await Db.PageAsync<Person>(null, sort, 2, 2);
        Assert.Equal(2, list.Count());
        Assert.Equal(person4.Id, list.First().Id);
        Assert.Equal(person3.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public async Task UsingObject_ReturnsMatching()
    {
        var person1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        var predicate = new { Active = true };
        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        var list = await Db.PageAsync<Person>(null, sort, 0, 3);
        Assert.Equal(3, list.Count());
        Assert.True(list.All(p => p.FirstName is "Sigma" or "Delta" or "Iota"));
    }
}