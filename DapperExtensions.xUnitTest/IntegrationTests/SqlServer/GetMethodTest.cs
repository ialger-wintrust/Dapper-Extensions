using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class GetMethodTest : SqlServerBaseFixture
{
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
        var testP2 = new Person
        {
            Active = true,
            FirstName = "Foo2",
            LastName = "Bar2",
            DateCreated = DateTime.UtcNow
        };
        var person1 = Db.Insert(testP1);
        var person2 = Db.Insert(testP2);

        var actualP1 = Db.Get<Person>(person1.Id);
        Assert.NotNull(actualP1);
        Assert.Equal(person1.Id, actualP1.Id);
        Assert.Equal("Foo", actualP1.FirstName);
        Assert.Equal("Bar", actualP1.LastName);

        var actualP2 = Db.Get<Person>(person2.Id);
        Assert.NotNull(actualP2);
        Assert.Equal(person2.Id, actualP2.Id);
        Assert.Equal("Foo2", actualP2.FirstName);
        Assert.Equal("Bar2", actualP2.LastName);
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