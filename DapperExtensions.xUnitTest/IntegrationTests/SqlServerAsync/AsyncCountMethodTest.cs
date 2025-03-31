using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncCountMethodTest : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task Count_UsingExtensionInvocation_ReturnsCount()
    {
        var expectedResults = 4;

        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        var count = await Db.Connection.CountAsync<Person>(null);
        Assert.Equal(expectedResults, count);
    }

    [Fact]
    public async Task UsingNullPredicate_Returns_Count()
    {
        var expectedResults = 4;

        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        var count = await Db.CountAsync<Person>(null);
        Assert.Equal(expectedResults, count);
    }

    [Fact]
    public async Task UsingPredicate_Returns_Count()
    {
        var expectedResults = 2;

        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
        var count = await Db.CountAsync<Person>(predicate);
        Assert.Equal(expectedResults, count);
    }

    [Fact]
    public async Task UsingObject_Returns_Count()
    {
        var expectedResults = 2;

        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        var predicate = new { FirstName = new[] { "b", "d" } };
        var count = await Db.CountAsync<Person>(predicate);
        Assert.Equal(expectedResults, count);
    }
}