using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using DapperExtensions.Sql.Dialects;
using Moq;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator
{
    public abstract class SqlGeneratorFixtureBase
    {
        public SqlGeneratorFixtureBase()
        {
            Setup();
        }

        protected Mock<IDapperExtensionsConfiguration> Configuration;
        protected Mock<ISqlDialect> Dialect;
        protected Mock<IClassMapper> ClassMap;
        protected Mock<IList<IProjection>?> Projections;

        public void Setup()
        {
            Configuration = new Mock<IDapperExtensionsConfiguration>();
            Dialect = new Mock<ISqlDialect>();
            ClassMap = new Mock<IClassMapper>();
            Projections = new Mock<IList<IProjection>?>();

            Dialect.SetupGet(c => c.ParameterPrefix).Returns('@');
            Configuration.SetupGet(c => c.Dialect).Returns(Dialect.Object).Verifiable();
        }
    }

    //public class IdentitySqlMethod : SqlGeneratorFixtureBase
    //{
    //    [Fact]
    //    public void CallsDialect()
    //    {
    //        Dialect.Setup(d => d.GetIdentitySql("TableName")).Returns("IdentitySql").Verifiable();
    //        Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
    //        var result = Generator.Object.IdentitySql(ClassMap.Object);
    //        Assert.Equal("IdentitySql", result, StringComparer.InvariantCultureIgnoreCase);
    //        Generator.Verify();
    //        Dialect.Verify();
    //    }
    //}
}