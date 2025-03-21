//using DapperExtensions.Mapper;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GetColumnNameUsingStirngMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void ThrowsExceptionWhenDoesNotFindProperty()
//    {
//        ClassMap.SetupGet(c => c.Properties).Returns(new List<IMemberMap>()).Verifiable();
//        var ex = Assert.Throws<ArgumentException>(() => Generator.Object.GetColumnName(ClassMap.Object, "property", true));
//        Assert.Contains("Could not find 'property'", ex.Message);
//        ClassMap.Verify();
//    }

//    [Fact]
//    public void CallsGetColumnNameWithProperty()
//    {
//        var property = new Mock<IMemberMap>();
//        property.Setup(p => p.Name).Returns("property").Verifiable();
//        ClassMap.SetupGet(c => c.Properties).Returns(new List<IMemberMap> { property.Object }).Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, property.Object, true, false, true)).Returns("ColumnName").Verifiable();
//        var result = Generator.Object.GetColumnName(ClassMap.Object, "property", true, true);
//        Assert.Equal("ColumnName", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        property.Verify();
//        Generator.Verify();
//    }
//}