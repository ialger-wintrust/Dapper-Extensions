using DapperExtensions.xUnitTest.Data.Common;
using DapperExtensions.xUnitTest.Helpers;
using Xunit.Abstractions;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync
{
    public static class AsyncTimerFixture
    {
        private const int cnt = 1000;

        [Collection(nameof(NonParallelTestCollection))]
        public class InsertTimes : AsyncSqlServerBaseFixture
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public InsertTimes(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task IdentityKey_UsingEntity()
            {
                var p = new Person
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateCreated = DateTime.Now,
                    Active = true
                };
                await Db.InsertAsync(p);
                var start = DateTime.Now;
                var ids = new List<long>();
                for (var i = 0; i < cnt; i++)
                {
                    var p2 = new Person
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i,
                        DateCreated = DateTime.Now,
                        Active = true
                    };
                    await Db.InsertAsync(p2);
                    ids.Add(p2.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }

            [Fact]
            public async Task IdentityKey_UsingReturnValue()
            {
                var p = new Person
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateCreated = DateTime.Now,
                    Active = true
                };
                await Db.InsertAsync(p);
                var start = DateTime.Now;
                var ids = new List<long>();
                for (var i = 0; i < cnt; i++)
                {
                    var p2 = new Person
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i,
                        DateCreated = DateTime.Now,
                        Active = true
                    };
                    var person = await Db.InsertAsync(p2);
                    ids.Add(person.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }

            [Fact]
            public async Task GuidKey_UsingEntity()
            {
                var a = new Animal { Name = "Name" };
                await Db.InsertAsync(a);
                var start = DateTime.Now;
                var ids = new List<Guid>();
                for (var i = 0; i < cnt; i++)
                {
                    var a2 = new Animal { Name = "Name" + i };
                    await Db.InsertAsync(a2);
                    ids.Add(a2.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }

            [Fact]
            public async Task GuidKey_UsingReturnValue()
            {
                var a = new Animal { Name = "Name" };
                await Db.InsertAsync(a);
                var start = DateTime.Now;
                var ids = new List<Guid>();
                for (var i = 0; i < cnt; i++)
                {
                    var a2 = new Animal { Name = "Name" + i };
                    var animal = await Db.InsertAsync(a2);
                    ids.Add(animal.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }

            [Fact]
            public async Task AssignKey_UsingEntity()
            {
                var ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
                await Db.InsertAsync(ca);
                var start = DateTime.Now;
                List<string> ids = new List<string>();
                for (var i = 0; i < cnt; i++)
                {
                    var key = (i + 1).ToString().PadLeft(15, '0');
                    var ca2 = new Car { Id = key, Name = "Name" + i };
                    await Db.InsertAsync(ca2);
                    ids.Add(ca2.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }

            [Fact]
            public async Task AssignKey_UsingReturnValue()
            {
                var ca = new Car { Id = string.Empty.PadLeft(15, '0'), Name = "Name" };
                await Db.InsertAsync(ca);
                var start = DateTime.Now;
                List<string> ids = new List<string>();
                for (var i = 0; i < cnt; i++)
                {
                    var key = (i + 1).ToString().PadLeft(15, '0');
                    var ca2 = new Car { Id = key, Name = "Name" + i };
                    var car = await Db.InsertAsync(ca2);
                    ids.Add(car.Id);
                }

                var total = DateTime.Now.Subtract(start).TotalMilliseconds;
                _testOutputHelper.WriteLine("Total Time:" + total);
                _testOutputHelper.WriteLine("Average Time:" + (total / cnt));
            }
        }
    }
}