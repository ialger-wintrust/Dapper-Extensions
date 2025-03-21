//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class SelectPagedMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void WithNoSort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//    }

//    [Fact]
//    public void WithEmptySort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//        Assert.Equal("Sort", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var sort = new Sort();
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithSort_GeneratesSql()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
//        property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var property2 = new Mock<IMemberMap>();
//        property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };
//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        List<ISort> sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters, null)).Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectPaged(ClassMap.Object, null, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        Generator.Verify();
//        Dialect.Verify();
//    }

//    [Fact]
//    public void WithPredicateAndSort_GeneratesSql()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
//        property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var property2 = new Mock<IMemberMap>();
//        property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };
//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        List<ISort> sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        Mock<IPredicate?> predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters, null))
//            .Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectPaged(ClassMap.Object, predicate.Object, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        predicate.Verify();
//        Generator.Verify();
//    }
//}//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class SelectPagedMethod : SqlGeneratorFixtureBase
//{
//    [Fact]
//    public void WithNoSort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//    }

//    [Fact]
//    public void WithEmptySort_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
//        Assert.Contains("null or empty", ex.Message);
//        Assert.Equal("Sort", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var sort = new Sort();
//        var ex = Assert.Throws<ArgumentNullException>(
//            () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithSort_GeneratesSql()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
//        property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var property2 = new Mock<IMemberMap>();
//        property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };
//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        List<ISort> sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters, null)).Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectPaged(ClassMap.Object, null, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        Generator.Verify();
//        Dialect.Verify();
//    }

//    [Fact]
//    public void WithPredicateAndSort_GeneratesSql()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
//        property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var property2 = new Mock<IMemberMap>();
//        property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };
//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        IDictionary<string, object> parameters = new Dictionary<string, object>();
//        var sortField = new Mock<ISort>();
//        sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
//        sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
//        List<ISort> sort = new List<ISort>
//        {
//            sortField.Object
//        };

//        Mock<IPredicate?> predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//        Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
//        Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

//        Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters, null))
//            .Returns("PagedSQL").Verifiable();

//        var result = Generator.Object.SelectPaged(ClassMap.Object, predicate.Object, sort, 2, 10, parameters, null);
//        Assert.Equal("PagedSQL", result, StringComparer.InvariantCultureIgnoreCase);
//        ClassMap.Verify();
//        sortField.Verify();
//        predicate.Verify();
//        Generator.Verify();
//    }
//}