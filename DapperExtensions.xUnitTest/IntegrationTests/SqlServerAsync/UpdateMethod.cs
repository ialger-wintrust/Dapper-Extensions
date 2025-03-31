using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class UpdateMethod : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task UpdateByKey_UsingExtensionInvocation_ShouldUpdateTheEntity()
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
        p2.FirstName = "Baz";
        p2.Active = false;

        await Db.Connection.UpdateAsync(p2);

        var p3 = await Db.GetAsync<Person>(id);
        Assert.Equal("Baz", p3.FirstName);
        Assert.Equal("Bar", p3.LastName);
        Assert.Equal(false, p3.Active);
    }

    [Fact]
    public async Task UsingKey_UpdatesEntity()
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
        p2.FirstName = "Baz";
        p2.Active = false;

        await Db.UpdateAsync(p2);

        var p3 = await Db.GetAsync<Person>(id);
        Assert.Equal("Baz", p3.FirstName);
        Assert.Equal("Bar", p3.LastName);
        Assert.Equal(false, p3.Active);
    }

    [Fact]
    public async Task UsingCompositeKey_UpdatesEntity()
    {
        var m1 = new Multikey { Key2 = "key", Value = "bar" };
        var key = await Db.InsertAsync(m1);

        var m2 = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
        m2.Key2 = "key";
        m2.Value = "barz";
        await Db.UpdateAsync(m2);

        var m3 = await Db.GetAsync<Multikey>(new { Key1 = 1, Key2 = "key" });
        Assert.Equal(1, m3.Key1);
        Assert.Equal("key", m3.Key2);
        Assert.Equal("barz", m3.Value);
    }
}