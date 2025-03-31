//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GenerateCountSqlTests : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void WithoutPredicate_ThrowsException()
//    {
//        Arrange();

//        var result = Generator.Object.Count(ClassMap.Object, null, new Dictionary<string, object>());
//        Assert.Equal("SELECT COUNT(*) AS !Total^ FROM TableName", result, StringComparer.InvariantCultureIgnoreCase);
//        Generator.Verify();
//        Dialect.Verify();
//    }

//    [Fact]
//    public void WithPredicate_ThrowsException()
//    {
//        Arrange();

//        var parameters = new Dictionary<string, object>();
//        var predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere").Verifiable();

//        var result = Generator.Object.Count(ClassMap.Object, predicate.Object, parameters);
//        Assert.Equal("SELECT COUNT(*) AS !Total^ FROM TableName WHERE PredicateWhere", result, StringComparer.InvariantCultureIgnoreCase);
//        Generator.Verify();
//        predicate.Verify();
//        Dialect.Verify();
//    }
//}