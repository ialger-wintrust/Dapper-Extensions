using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Test.Entities;
using DapperExtensions.Test.Maps;

namespace DapperExtensions.xUnitTest.IntegrationTests.Async
{
    public static class NonCrudAsyncFixture
    {
        public class GetNextGuidMethod
        {
            [Fact]
            public void GetMultiple_DoesNotDuplicate()
            {
                var list = new List<Guid>();
                for (var i = 0; i < 1000; i++)
                {
                    var id = DapperAsyncExtensions.GetNextGuid().Result;
                    Assert.False(list.Contains(id));
                    list.Add(id);
                }
            }
        }

        public class GetMapMethod
        {
            [Fact]
            public void NoMappingClass_ReturnsDefaultMapper()
            {
                var mapper = DapperAsyncExtensions.GetMap<EntityWithoutMapper>().Result;
                Assert.Equal(typeof(AutoClassMapper<EntityWithoutMapper>), mapper.GetType());
            }

            [Fact]
            public void ClassMapperDescendant_Returns_DefinedClass()
            {
                var mapper = DapperAsyncExtensions.GetMap<EntityWithMapper>().Result;
                Assert.Equal(typeof(EntityWithMapperMapper), mapper.GetType());
            }

            [Fact]
            public void ClassMapperInterface_Returns_DefinedMapper()
            {
                var mapper = DapperAsyncExtensions.GetMap<EntityWithInterfaceMapper>().Result;
                Assert.Equal(typeof(EntityWithInterfaceMapperMapper), mapper.GetType());
            }

            [Fact]
            public void MappingClass_ReturnsFromDifferentAssembly()
            {
                DapperAsyncExtensions.SetMappingAssemblies(new[] { typeof(ExternallyMappedMap).Assembly });
                var mapper = DapperAsyncExtensions.GetMap<ExternallyMapped>().Result;
                Assert.Equal(typeof(ExternallyMappedMap.ExternallyMappedMapper), mapper.GetType());

                DapperAsyncExtensions.SetMappingAssemblies(null);
                mapper = DapperAsyncExtensions.GetMap<ExternallyMapped>().Result;
                Assert.Equal(typeof(AutoClassMapper<ExternallyMapped>), mapper.GetType());
            }

            [ExcludeFromCodeCoverage]
            private class EntityWithoutMapper
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            [ExcludeFromCodeCoverage]
            private class EntityWithMapper
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            private class EntityWithMapperMapper : ClassMapper<EntityWithMapper>
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
        }
    }
}