//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GetTableNameMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void CallsDialect()
//    {
//        ClassMap.SetupGet(c => c.SchemaName).Returns("SchemaName").Verifiable();
//        ClassMap.SetupGet(c => c.TableName).Returns("TableName").Verifiable();
//        Dialect.Setup(d => d.GetTableName("SchemaName", "TableName", null)).Returns("FullTableName").Verifiable();
//        var result = Generator.Object.GetTableName(ClassMap.Object);
//        Assert.Equal("FullTableName", result, StringComparer.InvariantCultureIgnoreCase);
//        Dialect.Verify();
//        ClassMap.Verify();
//    }
//}