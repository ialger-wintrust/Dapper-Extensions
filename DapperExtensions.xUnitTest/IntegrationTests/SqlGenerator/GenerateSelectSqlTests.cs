//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using DapperExtensions.Sql.Dialects;
//using DapperExtensions.Sql;
//using DapperExtensions.xUnitTest.Data.Common;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GenerateSelectSqlTests : SqlGeneratorFixtureBase
//{
//    private class SimpleModel
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//    }

//    private class ComplexModel
//    {
//        public int Id { get; set; }
//        public Guid Key { get; set; }
//        public string Name { get; set; }
//    }

//    [Fact]
//    public void GenerateSelectWithNullParameters_ThrowsException()
//    {
//        var generator = new SqlGeneratorImpl(Configuration.Object);

//        Assert.Throws<ArgumentNullException>(
//             () => generator.Select(null, null, null, null, null));
//    }

//    [Fact]
//    public void SelectWithoutPredicateAndSort_ShouldGenerateValidSql()
//    {
//        var expectedSelectStatement = "Select Id, Name From SimpleModel";

//        var Configuration = new Mock<IDapperExtensionsConfiguration>();
//        var Dialect = new Mock<ISqlDialect>();

//        var classMapper = new AutoClassMapper<SimpleModel>();

//        Configuration
//            .SetupGet(c => c.Dialect)
//            .Returns(Dialect.Object)
//            .Verifiable();

//        Dialect
//            .SetupGet(c => c.ParameterPrefix)
//            .Returns('@');

//        Dialect
//            .Setup(d => d.GetTableName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(classMapper.TableName);

//        IDictionary<string, object> parameters = new Dictionary<string, object>();

//        var sqlGenerator = new SqlGeneratorImpl(Configuration.Object);
//        var result = sqlGenerator.Select(classMapper, null, null, parameters, null);

//        Assert.Equal(expectedSelectStatement, result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//    }

//    [Fact]
//    public void SelectWithPredicate_ShouldGenerateValidSql()
//    {
//        var expectedSelectStatement = "Select Id, Name From SimpleModel Where Name = @p_0";

//        var Configuration = new Mock<IDapperExtensionsConfiguration>();
//        var Dialect = new Mock<ISqlDialect>();
//        var Projections = new Mock<IList<IProjection>?>();

//        var classMapper = new AutoClassMapper<SimpleModel>();

//        Configuration
//            .SetupGet(c => c.Dialect)
//            .Returns(Dialect.Object)
//            .Verifiable();

//        Dialect
//            .SetupGet(c => c.ParameterPrefix)
//            .Returns('@');

//        Dialect
//            .Setup(d => d.GetTableName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(classMapper.TableName);

//        IDictionary<string, object> parameters = new Dictionary<string, object>();

//        var sqlGenerator = new SqlGeneratorImpl(Configuration.Object);
//        var result = sqlGenerator.Select(classMapper, null, null, parameters, null);

//        Assert.Equal(expectedSelectStatement, result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//    }

//    [Fact]
//    public void WithoutPredicateAndSortWithProjection_GeneratesSql()
//    {
//        IDictionary<string, object> parameters = new Dictionary<string, object>();

//        Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();

//        Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, Projections.Object, null))
//            .Returns("Columns 1, Columns 2")
//            .Verifiable();

//        var result = Generator.Object.Select(ClassMap.Object, null, null, parameters, Projections.Object);
//        Assert.Equal("SELECT Columns 1, Columns 2 FROM TableName", result.Replace("\r\n", string.Empty));
//        ClassMap.Verify();
//        Dialect.Verify();
//    }

//    [Fact]
//    public void WithPredicate_GeneratesSql()
//    {
//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

//        var mockTable = new Mock<Table>();

//        IList<IColumn> allColumnsSetup = new List<IColumn>();

//        allColumnsSetup.Add(new Column("Column1", "@Column1", "@c_0", new MemberMap(typeof(string)), mockTable.Object));
//        allColumnsSetup.Add(new Column("Column2", "@Column2", "@c_1", new MemberMap(typeof(string)), mockTable.Object));

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();

//        Generator.Setup(g => g.GetColumns())
//            .Returns(allColumnsSetup);

//        Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();

//        Generator.SetupGet(g => g.AllColumns).Returns(allColumnsSetup);

//        IList<IMemberMap>? mappedProperties;

//        var result = Generator.Object.Select(ClassMap.Object, predicate.Object, null, parameters, null);
//        Assert.Equal("SELECT Columns FROM TableName WHERE PredicateWhere", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        predicate.Verify();
//        Generator.Verify();
//        predicate.Verify();
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
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        var result = Generator.Object.Select(ClassMap.Object, null, sort, parameters, null);
//        Assert.Equal("SELECT Columns FROM TableName ORDER BY SortColumn ASC", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        Generator.Verify();
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

//        var predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        var result = Generator.Object.Select(ClassMap.Object, predicate.Object, sort, parameters, null);
//        Assert.Equal("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        predicate.Verify();
//        Generator.Verify();
//    }
//}