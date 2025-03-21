using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServer;

[Collection(nameof(NonParallelTestCollection))]
public class InsertMethodTest : SqlServerBaseFixture
{
    [Fact]
    public void InsertingAnEntityWithAnIdentityKey_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
        var person = Db.Insert(p);
        Assert.Equal(1, person.Id);
    }

    [Fact]
    public void InsertingAnEntityWithMultipleKeys_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var m = new Multikey { Key2 = "key", Value = "foo" };
        var key = Db.Insert(m);
        Assert.Equal(1, key.Key1);
        Assert.Equal("key", key.Key2);
    }

    [Fact]
    public void InsertingAnAssignedKey_ShouldAddTheEntityToTheDatabase_ReturningTheInsertedEntity()
    {
        var a1 = new Animal { Name = "Foo" };
        Db.Insert(a1);

        var a2 = Db.Get<Animal>(a1.Id);
        Assert.NotEqual(Guid.Empty, a2.Id);
        Assert.Equal(a1.Id, a2.Id);
    }

    [Fact]
    public void InsertingMultipleEntities_ShouldAddAllTheEntityToTheDatabase()
    {
        var a1 = new Animal { Name = "Foo" };
        var a2 = new Animal { Name = "Bar" };
        var a3 = new Animal { Name = "Baz" };

        Db.Insert(a1);
        Db.Insert(a2);
        Db.Insert(a3);

        var animals = Db.List<Animal>().ToList();
        Assert.Equal(3, animals.Count);
    }
}