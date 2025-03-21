//using DapperExtensions.Mapper;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GetColumnNameMethod : SqlGeneratorFixtureBase
//{
//    private void Arrange(out Mock<IMemberMap> property)
//    {
//        property = new Mock<IMemberMap>();
//        property.SetupGet(p => p.ColumnName).Returns("Column").Verifiable();
//        property.SetupGet(p => p.Name).Returns("Name").Verifiable();

//        Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
//        Dialect.Setup(d => d.GetColumnName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("FullColumnName").Verifiable();
//    }

//    [Fact]
//    public void DoesNotIncludeAliasWhenParameterIsFalse()
//    {
//        Arrange(out var property);

//        var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, false);
//        Assert.Equal("FullColumnName", result, StringComparer.InvariantCultureIgnoreCase);
//        property.Verify();
//        Generator.Verify();
//    }

//    [Fact]
//    public void DoesNotIncludeAliasWhenColumnAndNameAreSame()
//    {
//        Arrange(out var property);

//        var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true, false);
//        Assert.Equal("FullColumnName", result, StringComparer.InvariantCultureIgnoreCase);
//        property.Verify();
//        Generator.Verify();
//    }

//    [Fact]
//    public void IncludesAliasWhenColumnAndNameAreDifferent()
//    {
//        Arrange(out var property);

//        var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true, false);
//        Assert.Equal("FullColumnName", result, StringComparer.InvariantCultureIgnoreCase);
//        property.Verify();
//        Generator.Verify();
//    }
//}