//using DapperExtensions.Predicate;
//using DapperExtensions.Sql;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class DeleteWithPredicateMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void WithNullPredicate_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, null, new Dictionary<string, object>()));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Predicate", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var predicate = new Mock<IPredicate?>();
//        var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, predicate.Object, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void GeneratesSql()
//    {
//        var predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>())).Returns("PredicateWhere");

//        Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();

//        var result = Generator.Object.Delete(ClassMap.Object, predicate.Object, new Dictionary<string, object>());
//        Assert.Equal("DELETE FROM TableName WHERE PredicateWhere", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        predicate.Verify();
//        Generator.Verify();
//    }
//}