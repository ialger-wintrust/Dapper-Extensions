using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncDeleteMethodTest : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task DeleteByKey_UsingExtensionInvocation_DeletesFromDatabase()
    {
        var p1 = new Person
        {
            Active = true,
            FirstName = "Foo",
            LastName = "Bar",
            DateCreated = DateTime.UtcNow
        };

        var id = await Db.Connection.InsertAsync(p1);

        var p2 = await Db.Connection.GetAsync<Person>(id);
        await Db.Connection.DeleteAsync(p2);

        var deletedPerson = await Db.Connection.GetAsync<Person>(id);
        Assert.Null(deletedPerson);
    }

    [Fact]
    public async Task DeleteByPredicate_UsingExtensionInvocation_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        await Db.Connection.InsertAsync(p1);
        await Db.Connection.InsertAsync(p2);
        await Db.Connection.InsertAsync(p3);

        var list = await Db.Connection.ListAsync<Person>();
        Assert.Equal(3, list.Count());

        var result = await Db.Connection.DeleteAsync<Person>(new { LastName = "Bar" });
        Assert.True(result > 0);

        list = await Db.Connection.ListAsync<Person>();
        Assert.Single(list);
    }

    [Fact]
    public async Task UsingKey_DeletesFromDatabase()
    {
        var p1 = new Person
        {
            Active = true,
            FirstName = "Foo",
            LastName = "Bar",
            DateCreated = DateTime.UtcNow
        };

        var id = await Db.InsertAsync(p1);

        var p2 = await Db.GetAsync<Person>(id);
        await Db.DeleteAsync(p2);

        var deletedPerson = await Db.GetAsync<Person>(id);
        Assert.Null(deletedPerson);
    }

    [Fact]
    public async Task UsingCompositeKey_DeletesFromDatabase()
    {
        var m1 = new Multikey { Key2 = "key", Value = "bar" };
        var key = await Db.InsertAsync(m1);

        var m2 = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
        await Db.DeleteAsync(m2);

        var actualResults = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
        Assert.Null(actualResults);
    }

    [Fact]
    public async Task UsingPredicate_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        await Db.InsertAsync(p1);
        await Db.InsertAsync(p2);
        await Db.InsertAsync(p3);

        var list = await Db.ListAsync<Person>();
        Assert.Equal(3, list.Count());

        IPredicate? pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
        var result = await Db.DeleteAsync<Person>(pred);
        Assert.True(result > 0);

        list = await Db.ListAsync<Person>();
        Assert.Equal(1, list.Count());
    }

    [Fact]
    public async Task UsingMultipleKeys_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        var person1 = await Db.InsertAsync(p1);
        var person2 = await Db.InsertAsync(p2);
        var person3 = await Db.InsertAsync(p3);

        var ids = new List<Person>
        {
            person1,
            person2,
            person3
        };

        var list = await Db.ListAsync<Person>();
        Assert.Equal(3, list.Count());

        var result = await Db.DeleteAsync<Person>(list);
        Assert.True(result > 0);

        var actualPersonList = await Db.ListAsync<Person>();
        Assert.Empty(actualPersonList);
    }

    [Fact]
    public async Task UsingObject_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        await Db.InsertAsync(p1);
        await Db.InsertAsync(p2);
        await Db.InsertAsync(p3);

        var list = await Db.ListAsync<Person>();
        Assert.Equal(3, list.Count());

        var result = await Db.DeleteAsync<Person>(new { LastName = "Bar" });
        Assert.True(result > 0);

        list = await Db.ListAsync<Person>();
        Assert.Single(list);
    }
}