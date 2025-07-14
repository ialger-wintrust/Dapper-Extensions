using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DapperExtensions.Predicate;

namespace DapperExtensions.Repositories;

public class DapperSqlRepository<T> : IDapperRepository<T> where T : class
{
    private readonly IDapperDbConnectionFactory _dbConnectionFactory;

    public DapperSqlRepository(IDapperDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected IDbConnection DbConnection => _dbConnectionFactory.Create();

    public async Task<T?> Get(object id)
    {
        using var connection = DbConnection;
        return await connection.GetAsync<T>(id);
    }

    public async Task<IEnumerable<T>?> GetAll(Predicate<T>? predicate = null, IList<ISort>? sorts = null, int page = 1, int resultsPerPage = 10, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.PageAsync<T>(predicate, sorts, page, resultsPerPage, transaction);
    }

    public async Task<IEnumerable<T>?> Find(Predicate<T> predicate, IList<ISort>? sorts = null, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.ListAsync<T>(predicate, sorts, transaction);
    }

    public async Task<dynamic> Add(T entity, IDbTransaction? transaction = null)
    {
        try
        {
            using var connection = DbConnection;
            var results = await connection.InsertAsync(entity);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task Add(IEnumerable<T> entities, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        await connection.InsertAsync(entities);
    }

    public async Task<int> Update(T entity, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.UpdateAsync(entity, transaction, ignoreAllKeyProperties: true);
    }

    public async Task<int> Update(IEnumerable<T> entities, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.UpdateAsync(entities, transaction, ignoreAllKeyProperties: true);
    }

    public async Task<bool> Delete(T entity, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.DeleteAsync(entity, transaction);
    }

    public async Task<bool> Delete(IEnumerable<T> entities, IDbTransaction? transaction = null)
    {
        using var connection = DbConnection;
        return await connection.DeleteAsync(entities, transaction);
    }
}