using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class UpdateMethod : SqlServerBaseFixture
{
    [Fact]
    public void UsingKey_UpdatesEntity()
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
        p2.FirstName = "Baz";
        p2.Active = false;

        Db.Update(p2);

        var p3 = Db.Get<Person>(id);
        Assert.Equal("Baz", p3.FirstName);
        Assert.Equal("Bar", p3.LastName);
        Assert.Equal(false, p3.Active);
    }

    [Fact]
    public void UsingCompositeKey_UpdatesEntity()
    {
        var m1 = new Multikey { Key2 = "key", Value = "bar" };
        var key = Db.Insert(m1);

        var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
        m2.Key2 = "key";
        m2.Value = "barz";
        Db.Update(m2);

        var m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
        Assert.Equal(1, m3.Key1);
        Assert.Equal("key", m3.Key2);
        Assert.Equal("barz", m3.Value);
    }
}