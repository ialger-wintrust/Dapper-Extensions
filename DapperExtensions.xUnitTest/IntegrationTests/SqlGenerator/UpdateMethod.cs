//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using DapperExtensions.Sql;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class UpdateMethod : SqlGeneratorFixtureBase
//{
//    private void SetupGenerator()
//    {
//        Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns("Column").Verifiable();
//        Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
//    }

//    [Fact]
//    public void WithNullPredicate_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, null, new Dictionary<string, object>(), false, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Predicate", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var predicate = new Mock<IPredicate?>();
//        var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, null, false, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

//    [Fact]
//    public void WithNoMappedColumns_Throws_Exception()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

//        var property2 = new Mock<IMemberMap>();
//        property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };

//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();
//        Mock<IPredicate?> predicate = new Mock<IPredicate?>();
//        Dictionary<string, object> parameters = new Dictionary<string, object>();

//        var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null));

//        Assert.Contains("columns were mapped", ex.Message);
//        ClassMap.Verify();
//        property1.Verify();
//        property2.Verify();
//    }

//    [Fact]
//    public void DoesNotGenerateIdentityColumns()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

//        var property2 = new Mock<IMemberMap>();
//        property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
//        property2.Setup(p => p.Name).Returns("Name").Verifiable();

//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };

//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        SetupGenerator();

//        Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

//        var predicate = new Mock<IPredicate?>();
//        var parameters = new Dictionary<string, object>();
//        predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
//            .Returns("Predicate").Verifiable();

//        var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

//        Assert.Equal("UPDATE TableName SET Column = @u_1 WHERE Predicate", result, StringComparer.InvariantCultureIgnoreCase);

//        predicate.Verify();
//        ClassMap.Verify();
//        property1.Verify();
//        property1.VerifyGet(p => p.Name, Times.Once());
//        property2.Verify();

//        Generator.Verify();
//        Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
//    }

//    [Fact]
//    public void DoesNotGenerateIgnoredColumns()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.Setup(p => p.Ignored).Returns(true).Verifiable();

//        var property2 = new Mock<IMemberMap>();
//        property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
//        property2.Setup(p => p.Name).Returns("Name").Verifiable();

//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };

//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        SetupGenerator();

//        Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

//        Mock<IPredicate?> predicate = new Mock<IPredicate?>();
//        Dictionary<string, object> parameters = new Dictionary<string, object>();
//        predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
//            .Returns("Predicate").Verifiable();

//        var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

//        Assert.Equal("UPDATE TableName SET Column = @u_1 WHERE Predicate", result, StringComparer.InvariantCultureIgnoreCase);

//        predicate.Verify();
//        ClassMap.Verify();
//        property1.Verify();
//        property1.VerifyGet(p => p.Name, Times.Once());
//        property2.Verify();

//        Generator.Verify();
//        Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
//    }

//    [Fact]
//    public void DoesNotGenerateReadonlyColumns()
//    {
//        var property1 = new Mock<IMemberMap>();
//        property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

//        var property2 = new Mock<IMemberMap>();
//        property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
//        property2.Setup(p => p.Name).Returns("Name").Verifiable();

//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };

//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        SetupGenerator();

//        Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

//        var parameters = new Dictionary<string, object>();
//        Mock<IPredicate?> predicate = new Mock<IPredicate?>();
//        predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
//            .Returns("Predicate").Verifiable();

//        var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

//        Assert.Equal("UPDATE TableName SET Column = @u_1 WHERE Predicate", result, StringComparer.InvariantCultureIgnoreCase);

//        predicate.Verify();
//        ClassMap.Verify();
//        property1.Verify();
//        property1.VerifyGet(p => p.Name, Times.Once());
//        property2.Verify();

//        Generator.Verify();
//        Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
//    }
//}