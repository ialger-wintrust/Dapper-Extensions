using DapperExtensions.Predicate;
using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.Async.SqlServer;

public static class CrudFixture
{
    [Collection(nameof(NonParallelTestCollection))]
    public class InsertMethod : SqlServerBaseAsyncFixture
    {
        [Fact]
        public async Task AddsEntityToDatabase_ReturnsKey()
        {
            var p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            var id = await Db.Insert(p);
            Assert.Equal(1, id);
            Assert.Equal(1, p.Id);
        }

        [Fact]
        public async Task AddsEntityToDatabase_ReturnsCompositeKey()
        {
            var m = new Multikey { Key2 = "key", Value = "foo" };
            var key = await Db.Insert(m);
            Assert.Equal(1, key.Key1);
            Assert.Equal("key", key.Key2);
        }

        [Fact]
        public async Task AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
        {
            var a1 = new Animal { Name = "Foo" };
            Db.Insert(a1);

            var a2 = await Db.Get<Animal>(a1.Id);
            Assert.NotEqual(Guid.Empty, a2.Id);
            Assert.Equal(a1.Id, a2.Id);
        }

        [Fact]
        public async Task AddsMultipleEntitiesToDatabase()
        {
            var a1 = new Animal { Name = "Foo" };
            var a2 = new Animal { Name = "Bar" };
            var a3 = new Animal { Name = "Baz" };

            Db.Insert<Animal>(new[] { a1, a2, a3 });

            var animals = await Db.GetList<Animal>();

            Assert.Equal(3, animals.Count());
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class GetMethod : SqlServerBaseAsyncFixture
    {
        [Fact]
        public async Task UsingKey_ReturnsEntity()
        {
            var p1 = new Person
            {
                Active = true,
                FirstName = "Foo",
                LastName = "Bar",
                DateCreated = DateTime.UtcNow
            };
            var id = await Db.Insert(p1);

            var p2 = await Db.Get<Person>(id);
            Assert.Equal(id, p2.Id);
            Assert.Equal("Foo", p2.FirstName);
            Assert.Equal("Bar", p2.LastName);
        }

        [Fact]
        public async Task UsingCompositeKey_ReturnsEntity()
        {
            var m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = await Db.Insert(m1);

            var m2 = await Db.Get<Multikey>(new { key.Key1, key.Key2 });
            Assert.Equal(1, m2.Key1);
            Assert.Equal("key", m2.Key2);
            Assert.Equal("bar", m2.Value);
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class DeleteMethod : SqlServerBaseAsyncFixture
    {
        private static void Arrange(out Person p1, out Person p2, out Person p3)
        {
            p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
        }

        [Fact]
        public async Task UsingKey_DeletesFromDatabase()
        {
            Arrange(out var p1, out var _, out var _);
            var id = await Db.Insert(p1);

            var p2 = await Db.Get<Person>(id);

            var actualDeleted = await Db.Delete(p2);
            Assert.True(actualDeleted);
            Task<Person> aux = Db.Get<Person>(id);

            Assert.Null(aux.AsyncState);
            Dispose();
        }

        [Fact]
        public async Task UsingCompositeKey_DeletesFromDatabase()
        {
            var m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = await Db.Insert(m1);

            var m2 = await Db.Get<Multikey>(new { key.Key1, key.Key2 });
            var actualDeleteResults = await Db.Delete(m2);
            Assert.True(actualDeleteResults);
            var aux = Db.Get<Multikey>(new { key.Key1, key.Key2 });

            Assert.Null(aux.AsyncState);
            Dispose();
        }

        [Fact]
        public async Task UsingPredicate_DeletesRows()
        {
            Arrange(out var p1, out var p2, out var p3);
            Db.Insert(p1);
            Db.Insert(p2);
            Db.Insert(p3);

            var list = await Db.GetList<Person>();
            Assert.Equal(3, list.Count());

            var pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
            var result = await Db.Delete<Person>(pred);
            Assert.True(result);

            list = await Db.GetList<Person>();
            Assert.Equal(1, list.Count());
        }

        [Fact]
        public async Task UsingObject_DeletesRows()
        {
            Arrange(out var p1, out var p2, out var p3);
            Db.Insert(p1);
            Db.Insert(p2);
            Db.Insert(p3);

            var list = await Db.GetList<Person>();
            Assert.Equal(3, list.Count());

            var result = await Db.Delete<Person>(new { LastName = "Bar" });
            Assert.True(result);

            list = await Db.GetList<Person>();
            Assert.Equal(1, list.Count());
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class UpdateMethod : SqlServerBaseAsyncFixture
    {
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
            var id = await Db.Insert(p1);

            var p2 = await Db.Get<Person>(id);
            p2.FirstName = "Baz";
            p2.Active = false;

            Db.Update(p2);

            var p3 = await Db.Get<Person>(id);
            Assert.Equal("Baz", p3.FirstName);
            Assert.Equal("Bar", p3.LastName);
            Assert.Equal(false, p3.Active);
        }

        [Fact]
        public async Task UsingCompositeKey_UpdatesEntity()
        {
            var m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = await Db.Insert(m1);

            var m2 = await Db.Get<Multikey>(new { key.Key1, key.Key2 });
            m2.Key2 = "key";
            m2.Value = "barz";
            Db.Update(m2);

            var m3 = await Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
            Assert.Equal(1, m3.Key1);
            Assert.Equal("key", m3.Key2);
            Assert.Equal("barz", m3.Value);
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class GetListMethod : SqlServerBaseAsyncFixture
    {
        private void Arrange()
        {
            Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });
        }

        [Fact]
        public async Task UsingNullPredicate_ReturnsAll()
        {
            Arrange();

            var list = await Db.GetList<Person>();
            Assert.Equal(4, list.Count());
        }

        [Fact]
        public async Task UsingPredicate_ReturnsMatching()
        {
            Arrange();

            var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
            var personResult = await Db.GetList<Person>(predicate, null);
            var personList = personResult.ToList();
            Assert.Equal(2, personList.Count());
            Assert.True(personList.All(p => p.FirstName == "a" || p.FirstName == "c"));
        }

        [Fact]
        public async Task UsingObject_ReturnsMatching()
        {
            Arrange();

            var predicate = new { Active = true, FirstName = "c" };
            var list = await Db.GetList<Person>(predicate, null);
            Assert.Equal(1, list.Count());
            Assert.True(list.All(p => p.FirstName == "c"));
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class GetPageMethod : SqlServerBaseAsyncFixture
    {
        private void SetData(out dynamic id1, out dynamic id2, out dynamic id3, out dynamic id4)
        {
            id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });
        }

        [Fact]
        public async Task UsingNullPredicate_ReturnsMatching()
        {
            SetData(out var id1, out var id2, out var id3, out var id4);

            var sort = new List<ISort>
            {
                Predicates.Sort<Person>(p => p.LastName),
                Predicates.Sort<Person>("FirstName")
            };

            var list = await Db.GetPage<Person>(null, sort, 0, 2);
            Assert.Equal(2, list.Count());
            var expectedFirstResults = await id1;
            var expectedSecondResults = await id2;

            Assert.Equal(expectedFirstResults, list.Skip(1).First().Id);
            Assert.Equal(expectedSecondResults, list.First().Id);
        }

        [Fact]
        public async Task UsingPredicate_ReturnsMatching()
        {
            SetData(out var id1, out var id2, out var id3, out var id4);

            var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
            var sort = new List<ISort>
            {
                Predicates.Sort<Person>(p => p.LastName),
                Predicates.Sort<Person>("FirstName")
            };

            var list = await Db.GetPage<Person>(predicate, sort, 0, 2);
            Assert.Equal(2, list.Count());
            Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta" || p.FirstName == "Iota"));
        }

        [Fact]
        public async Task NotFirstPage_Returns_NextResults()
        {
            SetData(out var id1, out var id2, out var id3, out var id4);

            var sort = new List<ISort>
            {
                Predicates.Sort<Person>(p => p.LastName),
                Predicates.Sort<Person>("FirstName")
            };

            var list = await Db.GetPage<Person>(null, sort, 2, 2);
            Assert.Equal(2, list.Count());

            var expectedThirdResults = await id3;
            var expectedFourthResults = await id4;

            Assert.Equal(expectedThirdResults, list.Skip(1).First().Id);
            Assert.Equal(expectedFourthResults, list.First().Id);
        }

        [Fact]
        public async Task UsingObject_ReturnsMatching()
        {
            SetData(out var id1, out var id2, out var id3, out var id4);

            var predicate = new { Active = true };
            var sort = new List<ISort>
            {
                Predicates.Sort<Person>(p => p.LastName),
                Predicates.Sort<Person>("FirstName")
            };

            var list = await Db.GetPage<Person>(predicate, sort, 0, 3);
            Assert.Equal(2, list.Count());
            Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class CountMethod : SqlServerBaseAsyncFixture
    {
        private void Arrange()
        {
            Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
        }

        [Fact]
        public async Task UsingNullPredicate_Returns_Count()
        {
            Arrange();

            var count = await Db.Count<Person>(null);
            Assert.Equal(4, count);
        }

        [Fact]
        public async Task UsingPredicate_Returns_Count()
        {
            Arrange();

            var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
            var count = await Db.Count<Person>(predicate);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task UsingObject_Returns_Count()
        {
            Arrange();

            var predicate = new { FirstName = new[] { "b", "d" } };
            var count = await Db.Count<Person>(predicate);
            Assert.Equal(2, count);
        }
    }

    [Collection(nameof(NonParallelTestCollection))]
    public class GetMultipleMethod : SqlServerBaseAsyncFixture
    {
        [Fact]
        public async Task ReturnsItems()
        {
            await Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            await Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            await Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            await Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            await Db.Insert(new Animal { Name = "Foo" });
            await Db.Insert(new Animal { Name = "Bar" });
            await Db.Insert(new Animal { Name = "Baz" });

            var predicate = new GetMultiplePredicate();
            predicate.Add<Person>(null);
            predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
            predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

            var result = await Db.GetMultiple(predicate);
            var people = result.Read<Person>().ToList();
            var animals = result.Read<Animal>().ToList();
            var people2 = result.Read<Person>().ToList();

            Assert.Equal(4, people.Count);
            Assert.Equal(2, animals.Count);
            Assert.Equal(1, people2.Count);
            Dispose();
        }
    }
}