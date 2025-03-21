//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class SelectSetMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void WithNoSort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectSet(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//    }

//    [Fact]
//    public void WithEmptySort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//        Assert.Equal("Sort", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var sort = new Sort();
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithSort_GeneratesSql()
//    {
//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        var sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
//            .Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectSet(ClassMap.Object, null, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        Generator.Verify();
//        Dialect.Verify();
//    }

//    [Fact]
//    public void WithPredicateAndSort_GeneratesSql()
//    {
//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        var sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        var predicate = new Mock<IPredicate>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
//            .Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectSet(ClassMap.Object, predicate.Object, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        predicate.Verify();
//        Generator.Verify();
//    }
//}