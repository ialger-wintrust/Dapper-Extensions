using DapperExtensions.Mapper;

namespace DapperExtensions.xUnitTest.IntegrationTests.Mapper;

public class PluralizedAutoClassMapperTests
{
    private class Equipment;

    private class Person;

    private class Ox;

    private class Moose;

    [Fact]
    public void TestPluralizeOnUnpluralizableName_ShouldNotPluralize()
    {
        var expectedTableName = "Equipment";
        var testPluralizedAutoClassMapper = new PluralizedAutoClassMapper<Equipment>();
        var actualTableName = testPluralizedAutoClassMapper.TableName;

        Assert.Equal(expectedTableName, actualTableName, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void TestPluralizePersonClass_ShouldReturnPeople()
    {
        var expectedTableName = "People";
        var testPluralizedAutoClassMapper = new PluralizedAutoClassMapper<Person>();
        var actualTableName = testPluralizedAutoClassMapper.TableName;

        Assert.Equal(expectedTableName, actualTableName, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void TestPluralizeMooseClass_ShouldReturnMoose()
    {
        var expectedTableName = "Oxen";
        var testPluralizedAutoClassMapper = new PluralizedAutoClassMapper<Ox>();
        var actualTableName = testPluralizedAutoClassMapper.TableName;

        Assert.Equal(expectedTableName, actualTableName, StringComparer.InvariantCultureIgnoreCase);
    }
}