using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Sql.Dialects;
using DapperExtensions.Test.Entities;
using DapperExtensions.Test.Maps;
using DapperExtensions.xUnitTest.Helpers;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlServerAsync
{
    [Collection(nameof(NonParallelTestCollection))]
    public class DapperAsyncExtensionsTests
    {
        [ExcludeFromCodeCoverage]
        private class TestDefaultClassMapper<T> : ClassMapper<T>
        {
        }

        [ExcludeFromCodeCoverage]
        private class TestSqlInjectionEntity
        {
            public int Id { get; set; }

            public string? Message { get; set; }
            public required string Foo { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class TestEntityWithMapper
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private class EntityWithMapperMapper : ClassMapper<TestEntityWithMapper>
        {
            public EntityWithMapperMapper()
            {
                Map(p => p.Key).Column("EntityKey").Key(KeyType.Assigned);
                AutoMap();
            }
        }

        [ExcludeFromCodeCoverage]
        private class EntityWithInterfaceMapper
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class EntityWithInterfaceMapperMapper : IClassMapper<EntityWithInterfaceMapper>
        {
            public string SchemaName { get; }
            public string TableName { get; }
            public IList<IMemberMap> Properties { get; }
            public Type EntityType { get; }

            public string SimpleAlias { get; }

            public IList<IReferenceMap> References { get; }

            public Guid Identity { get; private set; }

            public Guid ParentIdentity { get; private set; }

            public MemberMap Map(Expression<Func<EntityWithInterfaceMapper, object>> expression)
            {
                throw new NotImplementedException();
            }

            public MemberMap Map(PropertyInfo propertyInfo)
            {
                throw new NotImplementedException();
            }

            public void SetIdentity(Guid identity)
            {
                throw new NotImplementedException();
            }

            public void SetParentIdentity(Guid identity)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void SqlDialect_ShouldReturnTheSqlDialectThatWasConfigured()
        {
            var expectedSqlDialect = new SqlServerDialect();

            DapperAsyncExtensions.SqlDialect = expectedSqlDialect;
            var actualSqlDialect = DapperAsyncExtensions.SqlDialect;

            Assert.Equal(expectedSqlDialect, actualSqlDialect);
        }

        [Fact]
        public void InstanceFactory_ShouldReturnTheInstanceFactoryThatWasConfigured()
        {
            var expectedDapperImplementor = new DapperAsyncImplementor(new SqlGeneratorImpl(new DapperExtensionsConfiguration()));

            DapperAsyncExtensions.InstanceFactory = _ => expectedDapperImplementor;
            var actualDapperImplementor = DapperAsyncExtensions.InstanceFactory.Invoke(null);

            Assert.Equal(expectedDapperImplementor, actualDapperImplementor);
        }

        [Fact]
        public void GetOrSetSqlInjectionWithNoSqlInjectionSet_ShouldReturnNull()
        {
            var result = typeof(TestSqlInjectionEntity).GetOrSetSqlInjection();
            Assert.Null(result);
        }

        [Fact]
        public void GetOrSetSqlInjection_ShouldSetAndGetTheSqlInjection()
        {
            var value = new SqlInjection { EntityType = typeof(TestSqlInjectionEntity), Property = "foo", Sql = "select foo from dual" };
            var result = typeof(TestSqlInjectionEntity).GetOrSetSqlInjection(value);

            Assert.NotNull(result);
            Assert.Equal(value, result);
            Assert.Equal("foo", value.Property);
            Assert.Equal("select foo from dual", value.Sql);
            Assert.Equal(value.EntityType, result.EntityType);
        }

        [Fact]
        public void GetOrSetSqlInjection_ShouldReturnTheFirstValueItWasSetWith_WhenTheSameEntityTypeIsUpdated()
        {
            var Configuration = new DapperExtensionsConfiguration();
            var expectedValue = new SqlInjection { EntityType = typeof(TestSqlInjectionEntity), Property = "foo", Sql = "select foo from dual" };
            _ = Configuration.GetOrSetSqlInjection(typeof(TestSqlInjectionEntity), expectedValue);
            var newOverwritingValue = new SqlInjection { EntityType = typeof(TestSqlInjectionEntity), Property = "bar", Sql = "select bar from dual" };
            var actualResult = Configuration.GetOrSetSqlInjection(typeof(TestSqlInjectionEntity), newOverwritingValue);

            Assert.Equal(expectedValue, actualResult);
        }

        [Fact]
        public void CaseSensitiveSearch_ShouldReturnTheConfiguredCaseSensitiveSearch()
        {
            var expectedCaseSensitiveSearch = true;

            DapperAsyncExtensions.CaseSensitiveSearch = expectedCaseSensitiveSearch;
            var actualCaseSensitiveSearch = DapperAsyncExtensions.CaseSensitiveSearch;

            Assert.Equal(expectedCaseSensitiveSearch, actualCaseSensitiveSearch);
        }

        [Fact]
        public void CaseSensitiveSearch_WhenNotConfigured_ShouldReturnFalse()
        {
            var actualCaseSensitiveSearch = DapperAsyncExtensions.CaseSensitiveSearch;

            Assert.False(actualCaseSensitiveSearch);
        }

        [Fact]
        public void GetMapForAnEntityTypeWithOutAMapper_ShouldReturnDefaultAutoMapper()
        {
            DapperAsyncExtensions.DefaultMapper = typeof(AutoClassMapper<>);
            var expectedType = typeof(AutoClassMapper<TestEntity>);
            var mapper = typeof(TestEntity).GetMap();
            var actualType = mapper.GetType();
            Assert.Equal(expectedType, actualType);
        }

        [Fact]
        public void GetMapTypeForAnEntityTypeWithAMapper_ShouldReturnTheMappedMapper()
        {
            var expectedMapperType = typeof(EntityWithInterfaceMapperMapper);

            var config = new DapperExtensionsConfiguration();
            DapperAsyncExtensions.Configure(config);

            var actualMapperType = typeof(EntityWithInterfaceMapper).GetMapType();
            Assert.Equal(expectedMapperType, actualMapperType);
        }

        [Fact]
        public async Task ClassMapperDescendant_Returns_DefinedClass()
        {
            var mapper = await DapperAsyncExtensions.GetMap<TestEntityWithMapper>();
            Assert.Equal(typeof(EntityWithMapperMapper), mapper.GetType());
        }

        [Fact]
        public async Task ClassMapperInterface_Returns_DefinedMapper()
        {
            var mapper = await DapperAsyncExtensions.GetMap<EntityWithInterfaceMapper>();
            Assert.Equal(typeof(EntityWithInterfaceMapperMapper), mapper.GetType());
        }

        [Fact]
        public async Task GetMap_WithExternallyMappedEntities_ReturnsTheMapperWhenTheAssemblyIsLoaded_OrADefaultMapperWhenItIsNot()
        {
            var assemblies = (new[] { typeof(ExternallyMappedMap).Assembly }).ToList();
            DapperAsyncExtensions.SetMappingAssemblies(assemblies);
            var mapper = await DapperAsyncExtensions.GetMap<ExternallyMapped>();
            Assert.Equal(typeof(ExternallyMappedMap.ExternallyMappedMapper), mapper.GetType());

            DapperAsyncExtensions.SetMappingAssemblies(null);
            mapper = await DapperAsyncExtensions.GetMap<ExternallyMapped>();
            Assert.Equal(typeof(AutoClassMapper<ExternallyMapped>), mapper.GetType());
        }

        [Fact]
        public async Task GetNextGuid_WhenCalledMultipleTimes_ShouldNotDuplicateAGuidEntry()
        {
            var list = new List<Guid>();
            for (var i = 0; i < 1000; i++)
            {
                var id = await DapperAsyncExtensions.GetNextGuid();
                Assert.DoesNotContain(id, list);
                list.Add(id);
            }
        }

        [Fact]
        public void DefaultMapper_ShouldReturnTheDefaultMapperThatWasConfigured()
        {
            var expectedDefaultMapper = typeof(TestDefaultClassMapper<>);

            DapperAsyncExtensions.DefaultMapper = expectedDefaultMapper;
            var actualDefaultMapper = DapperAsyncExtensions.DefaultMapper;

            Assert.Equal(expectedDefaultMapper, actualDefaultMapper);
        }
    }
}