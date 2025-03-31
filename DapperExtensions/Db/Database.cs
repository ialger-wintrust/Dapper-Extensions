using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;

namespace DapperExtensions.Db
{
    #region Interfaces

    public interface IDatabase : IBaseDatabase
    {
        int Count<T>(object? predicate = null, IDbTransaction? transaction = null, int? commandTimeout = null);

        T? Find<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        T? Get<T>(dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null);

        T Insert<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        int Update<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        bool Delete<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        int Delete<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

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

    #endregion Interfaces

    #region Implementation

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
            return _dapper.Insert(Connection, entity, transaction, commandTimeout);
        }

        public int Update<T>(T? entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Update(Connection, entity, transaction, commandTimeout);
        }

        public bool Delete<T>(T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            return _dapper.Delete(Connection, entity, transaction, commandTimeout);
        }

        public int Delete<T>(object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
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

    #endregion Implementation
}