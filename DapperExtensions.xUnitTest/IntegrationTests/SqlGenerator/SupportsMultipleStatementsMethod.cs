//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class SupportsMultipleStatementsMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void CallsDialect()
//    {
//        Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(true).Verifiable();
//        var result = Generator.Object.SupportsMultipleStatements;
//        Assert.True(result);
//        Dialect.Verify();
//    }
//}