using System.Data;

namespace DapperExtensions.Repositories;

public interface IDapperDbConnectionFactory
{
    public IDbConnection Create();
}