using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DapperExtensions.xUnitTest;

public class SqlServerCrudUnitTests
{
    private IConfiguration Configuration { get; set; }

    public SqlServerCrudUnitTests()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<SqlServerCrudUnitTests>();

        Configuration = builder.Build();
    }

    public IDbConnection GetConnection()
    {
        var connectionString = Configuration.GetConnectionString("SQLDatabase");
        return new SqlConnection(connectionString);
    }

    [Fact]
    public async Task TestIntIdAdd()
    {
        var testContact = new CampaignLead()
        {
            ActiniumId = 1233432434,
            PhoneNumber = "2533473000",
            OutboundAni = "8015994000",
            CampaignName = "TestDatabase",
            CampaignSource = "Lendgo-purch-refi-good-credit-pull-execellent",
            ContactAttempt = 1,
        };

        using (var connection = GetConnection())
        {
            var results = await connection.InsertAsync(testContact);
            int id = results;
            connection.Close();
        }

        Console.Write("Finished");
    }

    [Fact]
    public async Task TestGuidIdAdd()
    {
        var testContact = new UserGuid()
        {
            Username = "testUser"
        };
        try
        {
            using var connection = GetConnection();
            var results = await connection.InsertAsync(testContact);

            Guid id = results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    //[Fact]
    //public void TestWorks()
    //{
    //    Assert.True(true);
    //}
}

public class CampaignLead
{
    [Key]
    public int Id { get; set; }

    public int ActiniumId { get; set; }

    [Required]
    [StringLength(11)]
    public string PhoneNumber { get; set; }

    [StringLength(11)]
    public string OutboundAni { get; set; }

    [Required]
    [StringLength(50)]
    public string CampaignName { get; set; }

    [StringLength(255)]
    public string CampaignSource { get; set; }

    public byte? ContactAttempt { get; set; }
}

public class UserGuid
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }
}