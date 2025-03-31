using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperExtensions
{
    /// <summary>
    /// Interface for asyncImplementor
    /// </summary>
    public interface IDapperAsyncImplementor
    {
        public ISqlGenerator SqlGenerator { get; }

        public DbDapperCommand? LastExecutedCommand { get; }

        Task<int> CountAsync<T>(IDbConnection connection, object? predicate = null, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        Task<T?> FindAsync<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        // TODO What does buffered and cols to select Actually do
        Task<T?> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null, /* bool buffered = false, IList<IProjection>? colsToSelect = null,*/ IList<IReferenceMap>? includedProperties = null);

        Task<IEnumerable<T>?> ListAsync<T>(IDbConnection connection, object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, /*bool buffered = false, IList<IProjection>? colsToSelect = null,*/ IList<IReferenceMap>? includedProperties = null);

        Task<IEnumerable<T>?> PageAsync<T>(IDbConnection connection, object? predicate = null, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 10, IDbTransaction? transaction = null, int? commandTimeout = null, /*bool buffered = false, IList<IProjection>? colsToSelect = null,*/ IList<IReferenceMap>? includedProperties = null);

        Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        Task<T> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = default);

        Task<int> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false, IList<IProjection>? colsToUpdate = null);

        Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        Task<int> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        //Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object? predicate = null, IList<ISort>? sort = null, int firstResult = 1, int maxResults = 10,
        //    IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection>? colsToSelect = null, IList<IReferenceMap>? includedProperties = null);
    }

    public class DapperAsyncImplementor : DapperImplementors, IDapperAsyncImplementor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperAsyncImplementor"/> class.
        /// </summary>
        /// <param name="sqlGenerator">The SQL generator.</param>
        public DapperAsyncImplementor(ISqlGenerator sqlGenerator)
            : base(sqlGenerator) { }

        public async Task<int> CountAsync<T>(IDbConnection connection, object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = CountCommand<T>(predicate, includedProperties);
            var results = await connection.ExecuteScalarAsync<int>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return results;
        }

        public async Task<T?> FindAsync<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null,
            IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = FindCommand<T>(predicate, includedProperties);
            var results = await connection.QuerySingleOrDefaultAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(results);
        }

        public async Task<T?> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            DbDapperCommand dapperCommand = GetCommand<T>(id, includedProperties);
            var results = await connection.QuerySingleOrDefaultAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(results);
        }

        public async Task<IEnumerable<T>?> ListAsync<T>(IDbConnection connection, object? predicate, IList<ISort>? sort, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = ListCommand<T>(predicate, includedProperties, sort: sort);
            var query = await connection.QueryAsync<dynamic>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(query);
        }

        public async Task<IEnumerable<T>?> PageAsync<T>(IDbConnection connection, object? predicate, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000,
            IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = PageCommand<T>(predicate, includedProperties, sort: sort, page: page, resultsPerPage: resultsPerPage);
            var query = await connection.QueryAsync<dynamic>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(query);
        }

        public async Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            if (SqlGenerator.SupportsMultipleStatements)
            {
                return await GetMultipleByBatch(connection, predicate, transaction, commandTimeout, includedProperties);
            }

            return await GetMultipleBySequence(connection, predicate, transaction, commandTimeout, includedProperties);
        }

        protected async Task<GridReaderResultReader> GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = GetMultipleBatchedCommand(predicate, includedProperties);
            var grid = await connection.QueryMultipleAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected async Task<SequenceReaderResultReader> GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            var readerResults = new List<SqlMapper.GridReader>();

            foreach (var predicateItem in predicate.Items)
            {
                var dapperCommand = GetMultipleSequenceCommand(predicateItem, includedProperties);
                var queryResult = await connection.QueryMultipleAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
                readerResults.Add(queryResult);
            }

            return new SequenceReaderResultReader(readerResults);
        }

        public async Task<T> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            var dapperCommand = InsertCommand(entity);
            var results = await connection.QuerySingleAsync<T>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return results;
        }

        public async Task<int> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false, IList<IProjection>? colsToUpdate = null)
        {
            var dapperCommand = UpdateCommand<T>(entity);
            return await connection.ExecuteAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
        }

        public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            var dapperCommand = DeleteCommand<T>(entity);
            return await connection.ExecuteAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public async Task<int> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            var dapperCommand = DeleteCommand<T>(predicate);
            return await connection.ExecuteAsync(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
        }
    }
}