//using DapperExtensions.Mapper;
//using DapperExtensions.Predicate;
//using Moq;

//namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

//public class GenerateCountSqlTests : SqlGeneratorFixtureBase
//{
//    private void Arrange()
//    {
//        var property1 = new Mock<IMemberMap>();
//        var property2 = new Mock<IMemberMap>();
//        var properties = new List<IMemberMap>
//        {
//            property1.Object,
//            property2.Object
//        };

//        var table = new Table
//        {
//            Alias = "y_1",
//            EntityType = ClassMap.Object.EntityType,
//            Name = ClassMap.Object.TableName,
//            ReferenceName = "",
//            Identity = ClassMap.Object.Identity,
//            ParentIdentity = ClassMap.Object.Identity,
//            IsVirtual = false,
//            PropertyInfo = null,
//            ClassMapper = ClassMap.Object,
//            LastIdentity = Guid.Empty,
//            ParentEntityType = null
//        };

//        var column1 = new Column("Column1", "@Column1", "@i_0", property1.Object, table);
//        var column2 = new Column("Column2", "@Column2", "@i_1", property2.Object, table);
//        //Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()))
//        //    .Returns<IColumn, bool, bool>((column, alias, prefix) => column.Alias).Verifiable();
//        ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

//        Dialect.SetupGet(d => d.OpenQuote).Returns('!').Verifiable();
//        Dialect.SetupGet(d => d.CloseQuote).Returns('^').Verifiable();
//        Dialect.Setup(d => d.GetCountSql(It.IsAny<string>()))
//            .Returns<string>(sql => $"SELECT COUNT(*) AS {Dialect.Object.OpenQuote}Total{Dialect.Object.CloseQuote} FROM {sql}").Verifiable();

//        Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
//            .Returns("TableName").Verifiable();
//    }

//    [Fact]
//    public void WithNullParameters_ThrowsException()
//    {
//        var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Count(ClassMap.Object, null, null));
//        Assert.Contains("cannot be null", ex.Message);
//        Assert.Equal("Parameters", ex.ParamName, StringComparer.InvariantCultureIgnoreCase);
//    }

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