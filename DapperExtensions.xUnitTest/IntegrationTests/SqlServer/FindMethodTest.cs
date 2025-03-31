using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class FindMethodTest : SqlServerBaseFixture
{
    [Fact]
    public void FindByKey_UsingExtensionInvocation_ReturnsEntity()
    {
        var testP1 = new Person
        {
            Active = true,
            FirstName = "Foo",
            LastName = "Bar",
            DateCreated = DateTime.UtcNow
        };

        var person1 = Db.Connection.Insert(testP1);
        var idPredicate = Predicates.Field<Person>(f => f.Id, Operator.Eq, person1.Id);
        var activePredicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);

        var groupPredicate = Predicates.Group(GroupOperator.And, idPredicate, activePredicate);

        var actualP1 = Db.Connection.Find<Person>(groupPredicate);
        Assert.NotNull(actualP1);
        Assert.Equal(person1.Id, actualP1.Id);
        Assert.Equal("Foo", actualP1.FirstName);
        Assert.Equal("Bar", actualP1.LastName);
    }

    [Fact]
    public void UsingKey_ReturnsEntity()
    {
        var testP1 = new Person
        {
            Active = true,
            FirstName = "Foo",
            LastName = "Bar",
            DateCreated = DateTime.UtcNow
        };

        var person1 = Db.Insert(testP1);
        var idPredicate = Predicates.Field<Person>(f => f.Id, Operator.Eq, person1.Id);
        var activePredicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);

        var groupPredicate = Predicates.Group(GroupOperator.And, idPredicate, activePredicate);

        var actualP1 = Db.Find<Person>(groupPredicate);
        Assert.NotNull(actualP1);
        Assert.Equal(person1.Id, actualP1.Id);
        Assert.Equal("Foo", actualP1.FirstName);
        Assert.Equal("Bar", actualP1.LastName);
    }

    [Fact]
    public void UsingCompositeKey_ReturnsEntity()
    {
        var m1 = new Multikey { Key2 = "key", Value = "bar" };
        var multikey = Db.Insert(m1);

        var actualMultikey = Db.Get<Multikey>(new { multikey.Key1, multikey.Key2 });
        Assert.NotNull(actualMultikey);
        Assert.Equal(1, actualMultikey.Key1);
        Assert.Equal("key", actualMultikey.Key2);
        Assert.Equal("bar", actualMultikey.Value);
    }
}