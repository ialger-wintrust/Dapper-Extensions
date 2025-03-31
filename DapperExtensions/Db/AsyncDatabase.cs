using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;

namespace DapperExtensions.Db;

public interface IAsyncDatabase : IBaseDatabase
{
    Task<int> CountAsync<T>(object? predicate = null, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<T?> FindAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<T?> GetAsync<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<T> InsertAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<int> UpdateAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false);

    Task<bool> DeleteAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<int> DeleteAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<IEnumerable<T>> ListAsync<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null /*bool buffered = true*/);

    Task<IEnumerable<T>> PageAsync<T>(object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000, IDbTransaction? transaction = null, int? commandTimeout = null /*bool buffered = true*/);

    Task<IMultipleResultReader> GetMultipleAsync(GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

    Task<Guid> GetNextGuidAsync();

    Task<IClassMapper> GetMapAsync<T>();

    void ClearCache();
}

public class AsyncDatabase : BaseDatabase, IAsyncDatabase
{
    private readonly IDapperAsyncImplementor _dapper;

    public AsyncDatabase(IDbConnection connection, ISqlGenerator sqlGenerator) : base(connection)
    {
        _dapper = new DapperAsyncImplementor(sqlGenerator);
    }

    public virtual async Task<int> CountAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.CountAsync<T>(Connection, predicate, transaction, commandTimeout);
    }

    public virtual async Task<T?> FindAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.FindAsync<T>(Connection, predicate, transaction, commandTimeout);
    }

    public virtual async Task<T?> GetAsync<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.GetAsync<T>(Connection, id, transaction, commandTimeout);
    }

    public virtual async Task<T> InsertAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.InsertAsync(Connection, entity, transaction, commandTimeout);
    }

    public virtual async Task<int> UpdateAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null,
        bool ignoreAllKeyProperties = false)
    {
        return await _dapper.UpdateAsync(Connection, entity, transaction, commandTimeout);
    }

    public virtual async Task<bool> DeleteAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.DeleteAsync(Connection, entity, transaction, commandTimeout);
    }

    public virtual async Task<int> DeleteAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.DeleteAsync<T>(Connection, predicate, transaction, commandTimeout);
    }

    public virtual async Task<IEnumerable<T>> ListAsync<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null,
        int? commandTimeout = null/*, bool buffered = true*/)
    {
        return await _dapper.ListAsync<T>(Connection, predicate, sort, transaction, commandTimeout);
    }

    public virtual async Task<IEnumerable<T>> PageAsync<T>(object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000,
        IDbTransaction? transaction = null, int? commandTimeout = null/*, bool buffered = true*/)
    {
        return await _dapper.PageAsync<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout);
    }

    public virtual async Task<IMultipleResultReader> GetMultipleAsync(GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        return await _dapper.GetMultipleAsync(Connection, predicate, transaction, commandTimeout);
    }

    public virtual async Task<Guid> GetNextGuidAsync()
    {
        return await Task.FromResult(_dapper.SqlGenerator.Configuration.GetNextGuid());
    }

    public virtual async Task<IClassMapper> GetMapAsync<T>()
    {
        return await Task.FromResult(_dapper.SqlGenerator.Configuration.GetMap<T>());
    }

    public virtual void ClearCache()
    {
        throw new NotImplementedException("This needs to be uncommented");
        //ClearCache(_dapper);
    }
}