using Azure;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Slapper.AutoMapper;

namespace DapperExtensions
{
    #region Interfaces

    public interface IBaseDatabase : IDisposable
    {
        bool HasActiveTransaction { get; }
        IDbConnection Connection { get; }

        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        void Commit();

        void Rollback();

        void RunInTransaction(Action action);

        T RunInTransaction<T>(Func<T> func);
    }

    public interface IDatabase : IBaseDatabase
    {
        int Count<T>(object? predicate = null, IDbTransaction? transaction = null, int? commandTimeout = null);

        T? Find<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        T? Get<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null);

        T Insert<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        int Update<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        bool Delete<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        bool Delete<T>(object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        IEnumerable<T>? List<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = true);

        IEnumerable<T>? Page<T>(object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = true);

        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        Guid GetNextGuid();

        IClassMapper GetMap<T>();

        void ClearCache();

        //IEnumerable<T> GetSet<T>(object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = true);

        //IEnumerable<T> GetList<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = true);
        //void Insert<T>(IEnumerable<T> entities, IDbTransaction? transaction = null, int? commandTimeout = null);
        //bool Update<T>(IEnumerable<T>? entities, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false);
        //bool Delete<T>(IEnumerable<T>? entities, IDbTransaction? transaction = null, int? commandTimeout = null);
    }

    public interface IAsyncDatabase : IBaseDatabase
    {
        Task<int> CountAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<T?> FindAsync<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<T?> GetAsync<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<dynamic> InsertAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<int> UpdateAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false);

        Task<bool> DeleteAsync<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<bool> DeleteAsync<T>(object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<IEnumerable<T>> ListAsync<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null /*bool buffered = true*/);

        Task<IEnumerable<T>> PageAsync<T>(object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000, IDbTransaction? transaction = null, int? commandTimeout = null /*bool buffered = true*/);

        Task<IMultipleResultReader> GetMultipleAsync(GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<Guid> GetNextGuidAsync();

        Task<IClassMapper> GetMapAsync<T>();

        void ClearCache();
    }

    #endregion Interfaces

    #region Implementation

    public abstract class BaseDatabase : IBaseDatabase
    {
        protected IDbTransaction _transaction;

        protected BaseDatabase(IDbConnection connection)
        {
            Connection = connection;

            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public bool HasActiveTransaction
        {
            get
            {
                return _transaction != null;
            }
        }

        public IDbConnection Connection { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    if (_transaction != null)
                    {
                        _transaction.Rollback();
                    }

                    Connection.Close();
                }
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action?.Invoke();
                Commit();
            }
            catch (Exception)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                var result = func.Invoke();
                Commit();
                return result;
            }
            catch (Exception)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw;
            }
        }

        protected virtual void ClearCache(IDapperImplementor dapper)
        {
            dapper.SqlGenerator.Configuration.ClearCache();
        }
    }

    public class Database : BaseDatabase, IDatabase
    {
        private readonly IDapperImplementor _dapper;

        public Database(IDbConnection connection, ISqlGenerator sqlGenerator) : base(connection)
        {
            _dapper = new DapperImplementor(sqlGenerator);
        }

        public virtual void ClearCache()
        {
            ClearCache(_dapper);
        }

        public int Count<T>(object? predicate = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Count<T>(Connection, predicate, transaction, commandTimeout);
        }

        public T? Find<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Find<T>(Connection, predicate, transaction, commandTimeout);
        }

        public T? Get<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Get<T>(Connection, id, transaction, commandTimeout);
        }

        public T Insert<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Insert<T>(Connection, entity, transaction, commandTimeout);
        }

        public int Update<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Update(Connection, entity, transaction, commandTimeout);
        }

        public bool Delete<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Delete(Connection, entity, transaction, commandTimeout);
        }

        public bool Delete<T>(object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Delete<T>(Connection, predicate, transaction, commandTimeout);
        }

        public IEnumerable<T>? List<T>(object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null,
            int? commandTimeout = null, bool buffered = true)
        {
            return _dapper.List<T>(Connection, predicate, sort, transaction, commandTimeout);
        }

        public IEnumerable<T>? Page<T>(object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000,
            IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = true)
        {
            return _dapper.Page<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.GetMultiple(Connection, predicate, transaction, commandTimeout);
        }

        public virtual Guid GetNextGuid()
        {
            return _dapper.SqlGenerator.Configuration.GetNextGuid();
        }

        public virtual IClassMapper GetMap<T>()
        {
            return _dapper.SqlGenerator.Configuration.GetMap<T>();
        }
    }

    public class AsyncDatabase : BaseDatabase, IAsyncDatabase
    {
        private readonly IDapperAsyncImplementor _dapper;

        public AsyncDatabase(IDbConnection connection, ISqlGenerator sqlGenerator) : base(connection)
        {
            //_dapper = new DapperAsyncImplementor(sqlGenerator);
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

        public virtual async Task<dynamic> InsertAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return await _dapper.InsertAsync(Connection, entity, transaction, commandTimeout);
        }

        public virtual async Task<int> UpdateAsync<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null,
            bool ignoreAllKeyProperties = false)
        {
            return await _dapper.UpdateAsync(Connection, entity, transaction, commandTimeout);
        }

        public virtual async Task<bool> DeleteAsync<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return await _dapper.DeleteAsync(Connection, entity, transaction, commandTimeout);
        }

        public virtual async Task<bool> DeleteAsync<T>(object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return await _dapper.DeleteAsync(Connection, predicate, transaction, commandTimeout);
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

    #endregion Implementation
}