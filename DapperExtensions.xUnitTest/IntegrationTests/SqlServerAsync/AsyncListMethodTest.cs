using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncListMethodTest : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task List_UsingExtensionInvocation_ShouldReturnAll()
    {
        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        var list = await Db.Connection.ListAsync<Person>();
        Assert.NotNull(list);
        Assert.Equal(4, list.Count());
    }

    [Fact]
    public async Task UsingNullPredicate_ReturnsAll()
    {
        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        IEnumerable<Person> list = await Db.ListAsync<Person>();
        Assert.Equal(4, list.Count());
    }

    [Fact]
    public async Task UsingPredicate_ReturnsMatching()
    {
        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
        IEnumerable<Person> list = await Db.ListAsync<Person>(predicate, null);
        Assert.Equal(2, list.Count());
        Assert.True(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
    }

    [Fact]
    public async Task UsingObject_ReturnsMatching()
    {
        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        var predicate = new { Active = true, FirstName = "c" };
        IEnumerable<Person> list = await Db.ListAsync<Person>(predicate, null);
        Assert.Equal(1, list.Count());
        Assert.True(list.All(p => p.FirstName == "c"));
    }
}