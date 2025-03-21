using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class ListMethodTest : SqlServerBaseFixture
{
    [Fact]
    public void UsingNullPredicate_ReturnsAll()
    {
        Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        IEnumerable<Person> list = Db.List<Person>();
        Assert.Equal(4, list.Count());
    }

    [Fact]
    public void UsingPredicate_ReturnsMatching()
    {
        Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
        IEnumerable<Person> list = Db.List<Person>(predicate, null);
        Assert.Equal(2, list.Count());
        Assert.True(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
    }

    [Fact]
    public void UsingObject_ReturnsMatching()
    {
        Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
        Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

        var predicate = new { Active = true, FirstName = "c" };
        IEnumerable<Person> list = Db.List<Person>(predicate, null);
        Assert.Equal(1, list.Count());
        Assert.True(list.All(p => p.FirstName == "c"));
    }
}