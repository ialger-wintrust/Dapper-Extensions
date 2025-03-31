using DapperExtensions.xUnitTest.DbModels;

namespace DapperExtensions.xUnitTest.IntegrationTests.SqlGenerator;

public class InsertMethod : SqlGeneratorFixtureBase
{
    public InsertMethod()
    {
    }

    private void SetupGenerator()
    {
        //Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
    }

    [Fact]
    public void GetTableName_ShouldReturnTheTableNameOfTheMappedClass()
    {
        var expectedTableName = "[UserGuid]";

        var userGuidClassMapper = Generator.Configuration.GetMap<UserGuid>();

        var actualTableName = Generator.GetTableName(userGuidClassMapper);

        Assert.Equal(expectedTableName, actualTableName, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTableNameWithClassMapperAndColumn_ShouldReturnTheTableNameOfTheMappedClass()
    {
        var expectedTableName = "y_1";

        var userGuidClassMapper = Generator.Configuration.GetMap<UserGuid>();

        Generator.MapTables(userGuidClassMapper);

        var column = Generator.GetColumns().Last();

        var actualTableName = Generator.GetTableAlias(column);

        Assert.Equal(expectedTableName, actualTableName, StringComparer.InvariantCultureIgnoreCase);
    }

    //[Fact]
    //public void WithNoMappedColumns_Throws_Exception()
    //{
    //    var property1 = new Mock<IMemberMap>();
    //    property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

    //    var property2 = new Mock<IMemberMap>();
    //    property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

    //    var properties = new List<IMemberMap>
    //                                        {
    //                                            property1.Object,
    //                                            property2.Object
    //                                        };

    //    ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

    //    var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Insert(ClassMap.Object));

    //    Assert.Contains("columns were mapped", ex.Message);
    //    ClassMap.Verify();
    //    property1.Verify();
    //    property2.Verify();
    //}

    //[Fact]
    //public void DoesNotGenerateIdentityColumns()
    //{
    //    var property1 = new Mock<IMemberMap>();
    //    property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

    //    var property2 = new Mock<IMemberMap>();
    //    property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
    //    property2.Setup(p => p.Name).Returns("Name").Verifiable();

    //    var properties = new List<IMemberMap>
    //                                        {
    //                                            property1.Object,
    //                                            property2.Object
    //                                        };

    //    ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

    //    SetupGenerator();

    //    Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

    //    var result = Generator.Object.Insert(ClassMap.Object);
    //    Assert.Equal("INSERT INTO TableName (Column) VALUES (@i_1)", result, StringComparer.InvariantCultureIgnoreCase);

    //    ClassMap.Verify();
    //    property1.Verify();
    //    property1.VerifyGet(p => p.Name, Times.Once());
    //    property2.Verify();

    //    Generator.Verify();
    //    Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
    //}

    //[Fact]
    //public void DoesNotGenerateIgnoredColumns()
    //{
    //    var property1 = new Mock<IMemberMap>();
    //    property1.Setup(p => p.Ignored).Returns(true).Verifiable();

    //    var property2 = new Mock<IMemberMap>();
    //    property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
    //    property2.Setup(p => p.Name).Returns("Name").Verifiable();

    //    var properties = new List<IMemberMap>
    //                                        {
    //                                            property1.Object,
    //                                            property2.Object
    //                                        };

    //    ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

    //    SetupGenerator();

    //    Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

    //    var result = Generator.Object.Insert(ClassMap.Object);
    //    Assert.Equal("INSERT INTO TableName (Column) VALUES (@i_1)", result, StringComparer.InvariantCultureIgnoreCase);

    //    ClassMap.Verify();
    //    property1.Verify();
    //    property1.VerifyGet(p => p.Name, Times.Once());
    //    property2.Verify();

    //    Generator.Verify();
    //    Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
    //}

    //[Fact]
    //public void DoesNotGenerateReadonlyColumns()
    //{
    //    var property1 = new Mock<IMemberMap>();
    //    property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

    //    var property2 = new Mock<IMemberMap>();
    //    property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
    //    property2.Setup(p => p.Name).Returns("Name").Verifiable();

    //    var properties = new List<IMemberMap>
    //                                        {
    //                                            property1.Object,
    //                                            property2.Object
    //                                        };

    //    ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

    //    SetupGenerator();

    //    Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

    //    var result = Generator.Object.Insert(ClassMap.Object);
    //    Assert.Equal("INSERT INTO TableName (Column) VALUES (@i_1)", result, StringComparer.InvariantCultureIgnoreCase);

    //    ClassMap.Verify();
    //    property1.Verify();
    //    property1.VerifyGet(p => p.Name, Times.Once());
    //    property2.Verify();

    //    Generator.Verify();
    //    Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
    //}

    //[Fact]
    //public void DoesNotPrefixColumnListWithTableName()
    //{
    //    var property1 = new Mock<IMemberMap>();
    //    property1.SetupGet(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

    //    var property2 = new Mock<IMemberMap>();
    //    property2.SetupGet(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
    //    property2.SetupGet(p => p.Name).Returns("Name").Verifiable();
    //    property2.SetupGet(p => p.ColumnName).Returns("Name").Verifiable();

    //    var properties = new List<IMemberMap>
    //                                        {
    //                                            property1.Object,
    //                                            property2.Object
    //                                        };

    //    ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();
    //    ClassMap.SetupGet(c => c.TableName).Returns("TableName");

    //    Dialect.Setup(c => c.GetTableName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((a, b, c) => b);
    //    Dialect.Setup(d => d.GetColumnName(It.IsAny<string>(), It.IsAny<string>(), null)).Returns<string, string, string>((a, b, c) => a + (a == null ? String.Empty : ".") + b);

    //    var generator = new SqlGeneratorImpl(Configuration.Object);
    //    var sql = generator.Insert(ClassMap.Object);

    //    Assert.Equal("INSERT INTO TableName (Name) VALUES (@i_1)", sql);
    //}
}