using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class GetMultipleMethod : SqlServerBaseFixture
{
    [Fact]
    public void ReturnsItems()
    {
        Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
        Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
        Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

        Db.Insert(new Animal { Name = "Foo" });
        Db.Insert(new Animal { Name = "Bar" });
        Db.Insert(new Animal { Name = "Baz" });

        var predicate = new GetMultiplePredicate();
        predicate.Add<Person>(null);
        predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
        predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

        var result = Db.GetMultiple(predicate);
        var people = result.Read<Person>().ToList();
        var animals = result.Read<Animal>().ToList();
        var people2 = result.Read<Person>().ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(2, animals.Count);
        Assert.Equal(1, people2.Count);
        Dispose();
    }
}