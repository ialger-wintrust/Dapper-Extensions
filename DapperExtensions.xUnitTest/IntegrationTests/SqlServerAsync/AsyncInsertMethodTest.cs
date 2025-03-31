using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync;

[Collection(nameof(NonParallelTestCollection))]
public class AsyncInsertMethodTest : AsyncSqlServerBaseFixture
{
    [Fact]
    public async Task InsertingAnEntityWithAnIdentityKey_UsingExtensionInvocation_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var person = await Db.Connection.InsertAsync(p);
        Assert.Equal(1, person.Id);
    }

    [Fact]
    public async Task InsertingAnEntityWithAnIdentityKey_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var person = await Db.InsertAsync(p);
        Assert.Equal(1, person.Id);
    }

    [Fact]
    public async Task InsertingAnEntityWithMultipleKeys_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var m = new Multikey { Key2 = "key", Value = "foo" };
        var key = await Db.InsertAsync(m);
        Assert.Equal(1, key.Key1);
        Assert.Equal("key", key.Key2);
    }

    [Fact]
    public async Task InsertingAnAssignedKey_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var a1 = new Animal { Name = "Foo" };
        await Db.InsertAsync(a1);

        var a2 = await Db.GetAsync<Animal>(a1.Id);
        Assert.NotEqual(Guid.Empty, a2.Id);
        Assert.Equal(a1.Id, a2.Id);
    }

    [Fact]
    public async Task InsertingMultipleEntities_ShouldAddAllTheEntityToTheDatabase()
    {
        var a1 = new Animal { Name = "Foo" };
        var a2 = new Animal { Name = "Bar" };
        var a3 = new Animal { Name = "Baz" };

        await Db.InsertAsync(a1);
        await Db.InsertAsync(a2);
        await Db.InsertAsync(a3);

        var animals = await Db.ListAsync<Animal>();
        Assert.Equal(3, animals.Count());
    }
}