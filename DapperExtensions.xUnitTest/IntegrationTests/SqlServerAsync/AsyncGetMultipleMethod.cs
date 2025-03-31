using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncGetMultipleMethod : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task GetMultiple_UsingExtensionInvocation_ReturnsItems()
    {
        await Db.Connection.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.Connection.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.Connection.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.Connection.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        await Db.Connection.InsertAsync(new Animal { Name = "Foo" });
        await Db.Connection.InsertAsync(new Animal { Name = "Bar" });
        await Db.Connection.InsertAsync(new Animal { Name = "Baz" });

        var predicate = new GetMultiplePredicate();
        predicate.Add<Person>(null);
        predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
        predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

        var result = await Db.Connection.GetMultipleAsync(predicate);
        var people = result.Read<Person>().ToList();
        var animals = result.Read<Animal>().ToList();
        var people2 = result.Read<Person>().ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(2, animals.Count);
        Assert.Equal(1, people2.Count);
        Dispose();
    }

    [Fact]
    public async Task GetMultiple_ReturnsItems()
    {
        await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        await Db.InsertAsync(new Animal { Name = "Foo" });
        await Db.InsertAsync(new Animal { Name = "Bar" });
        await Db.InsertAsync(new Animal { Name = "Baz" });

        var predicate = new GetMultiplePredicate();
        predicate.Add<Person>(null);
        predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
        predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

        var result = await Db.GetMultipleAsync(predicate);
        var people = result.Read<Person>().ToList();
        var animals = result.Read<Animal>().ToList();
        var people2 = result.Read<Person>().ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(2, animals.Count);
        Assert.Equal(1, people2.Count);
        Dispose();
    }
}