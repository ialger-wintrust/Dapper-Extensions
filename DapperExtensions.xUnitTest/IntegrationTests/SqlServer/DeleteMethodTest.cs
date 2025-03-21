using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class DeleteMethodTest : SqlServerBaseFixture
{
    [Fact]
    public void UsingKey_DeletesFromDatabase()
    {
        var p1 = new Person
        {
            Active = true,
            FirstName = "Foo",
            LastName = "Bar",
            DateCreated = DateTime.UtcNow
        };

        var id = Db.Insert(p1);

        var p2 = Db.Get<Person>(id);
        Db.Delete(p2);

        var deletedPerson = Db.Get<Person>(id);
        Assert.Null(deletedPerson);
    }

    [Fact]
    public void UsingCompositeKey_DeletesFromDatabase()
    {
        var m1 = new Multikey { Key2 = "key", Value = "bar" };
        var key = Db.Insert(m1);

        var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
        Db.Delete(m2);
        Assert.Null(Db.Get<Multikey>(new { key.Key1, key.Key2 }));
    }

    [Fact]
    public void UsingPredicate_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        Db.Insert(p1);
        Db.Insert(p2);
        Db.Insert(p3);

        var list = Db.List<Person>();
        Assert.Equal(3, list.Count());

        IPredicate? pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
        var result = Db.Delete<Person>(pred);
        Assert.True(result);

        list = Db.List<Person>();
        Assert.Equal(1, list.Count());
    }

    [Fact]
    public void UsingMultipleKeys_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        var person1 = Db.Insert(p1);
        var person2 = Db.Insert(p2);
        var person3 = Db.Insert(p3);

        var ids = new List<Person>
        {
            person1,
            person2,
            person3
        };

        var list = Db.List<Person>().ToList();
        Assert.Equal(3, list.Count());

        var result = Db.Delete<Person>(list);
        Assert.True(result);

        var actualPersonList = Db.List<Person>();
        Assert.Empty(actualPersonList);
    }

    [Fact]
    public void UsingObject_DeletesRows()
    {
        var p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        Db.Insert(p1);
        Db.Insert(p2);
        Db.Insert(p3);

        var list = Db.List<Person>();
        Assert.Equal(3, list.Count());

        var result = Db.Delete<Person>(new { LastName = "Bar" });
        Assert.True(result);

        list = Db.List<Person>();
        Assert.Equal(1, list.Count());
    }
}