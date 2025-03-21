//using System.Data;
//using DapperExtensions.xUnitTest.DbModels;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;

//namespace DapperExtensions.xUnitTest;

//public class SqlServerCrudUnitTests
//{
//    private IConfiguration Configuration { get; set; }

//    public SqlServerCrudUnitTests()
//    {
//        var builder = new ConfigurationBuilder()
//            .AddUserSecrets<SqlServerCrudUnitTests>();

//        Configuration = builder.Build();
//    }

//    public IDbConnection GetConnection()
//    {
//        var connectionString = Configuration.GetConnectionString("SQLDatabase");
//        return new SqlConnection(connectionString);
//    }

//    [Fact]
//    public void InsertingARecordWithAIdentityKey_ShouldReturnAValidEntityType()
//    {
//        var expectedDefaultInt = 0;

//        var testContact = new CampaignLead()
//        {
//            ActiniumId = 1233432434,
//            PhoneNumber = "2533473000",
//            OutboundAni = "8015994000",
//            CampaignName = "TestDatabase",
//            CampaignSource = "Lendgo-purch-refi-good-credit-pull-execellent",
//            ContactAttempt = 1,
//        };

//        using var connection = GetConnection();
//        var results = connection.Insert(testContact);
//        int id = results;

//        Assert.NotNull(results);
//        Assert.NotEqual(expectedDefaultInt, id);
//        Assert.IsType<int>(id);
//    }

//    [Fact]
//    public async Task InsertingARecordWithANonIdentityKey_ShouldReturnAValidEntityType()
//    {
//        var expectedDefaultGuid = new Guid();

//        var testContact = new UserGuid()
//        {
//            Username = "testUser"
//        };

//        using var connection = GetConnection();
//        var results = await connection.InsertAsync(testContact);

//        Guid id = results;

//        Assert.NotNull(results);
//        Assert.NotEqual(expectedDefaultGuid, id);
//        Assert.IsType<Guid>(id);
//    }
//}