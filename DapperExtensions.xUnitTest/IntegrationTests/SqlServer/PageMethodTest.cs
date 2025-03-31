using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class PageMethodTest : SqlServerBaseFixture
{
    [Fact]
    public void Page_UsingExtensionInvocation_ShouldReturnCorrectSortTakeAndSkip()
    {
        var person1 = Db.Connection.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = Db.Connection.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = Db.Connection.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = Db.Connection.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = Db.Connection.Page<Person>(null, sort, 0, 2);
        Assert.Equal(2, list.Count());
        Assert.Equal(person2.Id, list.First().Id);
        Assert.Equal(person1.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public void UsingNullPredicate_ReturnsMatching()
    {
        var person1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = Db.Page<Person>(null, sort, 0, 2);
        Assert.Equal(2, list.Count());
        Assert.Equal(person2.Id, list.First().Id);
        Assert.Equal(person1.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public void UsingPredicate_ReturnsMatching()
    {
        var person1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = Db.Page<Person>(predicate, sort, 0, 2);
        Assert.Equal(2, list.Count());
        Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta" || p.FirstName == "Iota"));
    }

    [Fact]
    public void NotFirstPage_Returns_NextResults()
    {
        var person1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        IEnumerable<Person>? list = Db.Page<Person>(null, sort, 2, 2);
        Assert.Equal(2, list.Count());
        Assert.Equal(person4.Id, list.First().Id);
        Assert.Equal(person3.Id, list.Skip(1).First().Id);
    }

    [Fact]
    public void UsingObject_ReturnsMatching()
    {
        var person1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
        var person3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
        var person4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

        var predicate = new { Active = true };
        IList<ISort> sort = new List<ISort>
        {
            Predicates.Sort<Person>(p => p.LastName),
            Predicates.Sort<Person>("FirstName")
        };

        var list = Db.Page<Person>(null, sort, 0, 3);
        Assert.Equal(3, list.Count());
        Assert.True(list.All(p => p.FirstName is "Sigma" or "Delta" or "Iota"));
    }
}