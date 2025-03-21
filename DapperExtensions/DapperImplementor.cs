using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;

namespace DapperExtensions
{
    public interface IDapperImplementor
    {
        public ISqlGenerator SqlGenerator { get; }

        public DbDapperCommand? LastExecutedCommand { get; }

        int Count<T>(IDbConnection connection, object? predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        T? Find<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        T? Get<T>(IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        IEnumerable<T>? List<T>(IDbConnection connection, object? predicate, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null);

        IEnumerable<T>? Page<T>(IDbConnection connection, object? predicate, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null);

        IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null);

        T Insert<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        int Update<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        bool Delete<T>(IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null);

        bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null);

        //TOut? GetPartial<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class;

        //bool UpdatePartial<TIn, TOut>(IDbConnection connection, TIn entity, Expression<Func<TIn, TOut>> func, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = true) where TIn : class;

        // TODO Allowing for a null predicate to be passed in here could return a ton of results.
        //IEnumerable<T> GetList<T>(IDbConnection connection, object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null);

        //IEnumerable<TOut> GetPartialList<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null) where TIn : class;

        //IEnumerable<TOut> GetPartialListAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null) where TIn : class;

        //IEnumerable<TOut> GetPartialPageAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null) where TIn : class where TOut : class;

        //IEnumerable<T> GetSet<T>(IDbConnection connection, object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null);

        //IEnumerable<TOut> GetPartialSet<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class;

        // Bulk Operations

        // void Update<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = true);
        // void UpdatePartial<TIn, TOut>(IDbConnection connection, IEnumerable<TIn> entities, Expression<Func<TIn, TOut>> func, IDbTransaction? transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = true) where TIn : class;
        // bool Delete<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction? transaction = null, int? commandTimeout = null);
        // void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction? transaction = null, int? commandTimeout = null);
    }

    public class DapperImplementor : DapperImplementors, IDapperImplementor
    {
        private static readonly Dictionary<Type, IList<IProjection>?> ColsBuffer = new Dictionary<Type, IList<IProjection>?>();

        public DapperImplementor(ISqlGenerator sqlGenerator) : base(sqlGenerator)
        {
        }

        public int Count<T>(IDbConnection connection, object? predicate, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = CountCommand<T>(predicate, includedProperties);
            var results = connection.ExecuteScalar<int>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return results;
        }

        public T? Find<T>(IDbConnection connection, object predicate, IDbTransaction? transaction = null, int? commandTimeout = null,
            IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = FindCommand<T>(predicate, includedProperties);
            var results = connection.QuerySingleOrDefault(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(results);
        }

        public T? Get<T>(IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null, IList<IReferenceMap>? includedProperties = null)
        {
            DbDapperCommand dapperCommand = GetCommand<T>(id, includedProperties);
            var results = connection.QuerySingleOrDefault(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return MapColumns<T>(results);
        }

        public IEnumerable<T>? List<T>(IDbConnection connection, object? predicate, IList<ISort>? sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = ListCommand<T>(predicate, includedProperties, sort: sort);
            var query = connection.Query<dynamic>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            return MapColumns<T>(query);
        }

        public IEnumerable<T>? Page<T>(IDbConnection connection, object? predicate, IList<ISort>? sort = null, int page = 1, int resultsPerPage = 1000,
            IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = PageCommand<T>(predicate, includedProperties, sort: sort, page: page, resultsPerPage: resultsPerPage);
            var query = connection.Query<dynamic>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            return MapColumns<T>(query);
        }

        public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null)
        {
            if (SqlGenerator.SupportsMultipleStatements)
            {
                return GetMultipleByBatch(connection, predicate, transaction, commandTimeout, includedProperties);
            }

            return GetMultipleBySequence(connection, predicate, transaction, commandTimeout, includedProperties);
        }

        protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null)
        {
            var dapperCommand = GetMultipleBatchedCommand(predicate, includedProperties);
            var grid = connection.QueryMultiple(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null)
        {
            var readerResults = new List<SqlMapper.GridReader>();

            foreach (var predicateItem in predicate.Items)
            {
                var dapperCommand = GetMultipleSequenceCommand(predicateItem, includedProperties);
                var queryResult = connection.QueryMultiple(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
                readerResults.Add(queryResult);
            }

            return new SequenceReaderResultReader(readerResults);
        }

        public T Insert<T>(IDbConnection connection, T entity, IDbTransaction? transaction, int? commandTimeout)
        {
            var dapperCommand = InsertCommand<T>(entity);

            var t = dapperCommand.ToString();
            var results = connection.QuerySingle<T>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
            return results;
        }

        public int Update<T>(IDbConnection connection, T entity, IDbTransaction? transaction, int? commandTimeout)
        {
            var dapperCommand = UpdateCommand<T>(entity);
            var t = dapperCommand.ToString();
            return connection.Execute(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
        }

        public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction? transaction, int? commandTimeout)
        {
            var dapperCommand = DeleteCommand<T>(entity);
            return connection.Execute(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction? transaction, int? commandTimeout)
        {
            var dapperCommand = DeleteCommand<T>(predicate);
            return connection.Execute(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        //public DynamicParameters GetDynamicParameters<T>(IClassMapper classMap, T? entity, bool useColumnAlias = false)
        //{
        //    var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity)?.ToList();
        //    var foreignKeys = classMap.Properties.Where(p => p.KeyType == KeyType.ForeignKey).Select(p => p.MemberInfo).ToList();
        //    var ignored = classMap.Properties.Where(x => x.Ignored).Select(p => p.MemberInfo).ToList();

        //    if (sequenceIdentityColumn?.Count > 1)
        //        throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

        //    return GetDynamicParameters(entity, classMap, sequenceIdentityColumn, foreignKeys, ignored, useColumnAlias);
        //}

        //public DynamicParameters GetDynamicParameters<T>(T? entity, DynamicParameters dynamicParameters, IMemberMap keyColumn, bool useColumnAlias = false)
        //{
        //    dynamicParameters ??= new DynamicParameters();
        //    foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
        //        .Where(p => p.Name != keyColumn.Name))
        //        AddParameter(entity, dynamicParameters, new MemberMap(prop), useColumnAlias);

        //    return dynamicParameters;
        //}

        //public TOut? GetPartial<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, dynamic id, IDbTransaction? transaction, int? commandTimeout, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class
        //{
        //    var colsToSelect = GetBufferedCols<TOut>();
        //    DbDapperCommand dapperCommand = GetCommand<TIn>(id, includedProperties, colsToSelect);
        //    var results = connection.QuerySingleOrDefault(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, commandTimeout, CommandType.Text);
        //    var data = MapColumns<TIn>(results);
        //    var returnedObject = data?.SingleOrDefault();

        //    if (returnedObject == null)
        //    {
        //        return null;
        //    }

        //    var f = func.Compile();
        //    return f.Invoke(returnedObject);
        //}

        //public IEnumerable<TOut> ListPartial<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class
        //{
        //    var cols = GetBufferedCols<TOut>();

        //    var dapperCommand = ListCommand<TIn>(predicate, includedProperties, sort: sort, page: page, resultsPerPage: resultsPerPage);
        //    var query = connection.Query<TIn>(dapperCommand.SqlString, dapperCommand.DynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        //    IEnumerable<TIn>? results = MapColumns<dynamic>(query);

        //    var f = func.Compile();
        //    return results.Select(i => f.Invoke(i));
        //}

        //public bool UpdatePartial<TIn, TOut>(IDbConnection connection, TIn? entity, Expression<Func<TIn, TOut>> func, IDbTransaction? transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where TIn : class
        //{
        //    var cols = GetBufferedCols<TOut>();
        //    return InternalUpdate<TIn>(connection, entity, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        //}

        //public IEnumerable<T> GetList<T>(IDbConnection connection, object? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return GetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, includedProperties);
        //}

        //public IEnumerable<T> GetListAutoMap<T>(IDbConnection connection, object? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return InternalGetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, null, includedProperties);
        //}

        //public IEnumerable<TOut> GetPartialList<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate = null, IList<ISort>? sort = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap>? includedProperties = null) where TIn : class
        //{
        //    return GetPartialListAutoMap<TIn, TOut>(connection, func, predicate, sort, transaction, commandTimeout, buffered, includedProperties);
        //}

        //public IEnumerable<TOut> GetPartialListAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null) where TIn : class
        //{
        //    var cols = GetBufferedCols<TOut>();
        //    var te = InternalGetListAutoMap<TIn>(connection, predicate, sort, transaction, commandTimeout, buffered, cols, includedProperties).ToList();

        //    // Transform TIn object to Anonymous type
        //    var f = func.Compile();
        //    return te.Select(i => f.Invoke(i));
        //}

        //protected IEnumerable<T> InternalGetListAutoMap<T>(IDbConnection connection, object? predicate, IList<ISort>? sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
        //    return GetListAutoMap<T>(connection, colsToSelect, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, includedProperties);
        //}

        //public bool Delete<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction? transaction, int? commandTimeout)
        //{
        //    var classMap = SqlGenerator.Configuration.GetMap<T>();
        //    var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);

        //    if (identityColumn == null)
        //    {
        //        throw new NotSupportedException("A valid single Identity column must be present. multiple Identities are not supported");
        //    }

        //    var entityIdentities = entities.Select(entity => identityColumn.GetValue(entity)).ToList();

        //    var predicate = GetIdPredicate(classMap, entityIdentities);

        //    return Delete<T>(connection, predicate, transaction, commandTimeout);
        //}

        //protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate? predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return GetPageAutoMap<T>(connection, classMap, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties);
        //}

        //public IEnumerable<T> GetPageAutoMap<T>(IDbConnection connection, object? predicate, IList<ISort>? sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return InternalGetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, null, includedProperties);
        //}

        //public IEnumerable<TOut> GetPartialPageAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class
        //{
        //    var cols = GetBufferedCols<TOut>();
        //    var te = InternalGetPageAutoMap<TIn>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, cols, includedProperties).ToList();

        //    // Transform TIn object to Anonymous type
        //    var f = func.Compile();
        //    return te.Select(i => f.Invoke(i));
        //}

        //protected IEnumerable<T> InternalGetPageAutoMap<T>(IDbConnection connection, object? predicate, IList<ISort>? sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);

        //    return GetPageAutoMap<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties);
        //}

        //protected IEnumerable<T> GetPageAutoMap<T>(IDbConnection connection, IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, int page, int resultsPerPage, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    if (sort == null)
        //    {
        //        var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);

        //        if (identityColumn == null)
        //        {
        //            throw new ArgumentException(
        //                $"There is no identity column found for table {classMap.TableName} please provide a valid sort or fix your entity mappings");
        //        }

        //        var identitySort = Predicates.Sort<T>(identityColumn.Name);
        //        sort = new List<ISort>() { identitySort };
        //    }

        //    var parameters = new Dictionary<string, object>();
        //    var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters, colsToSelect, includedProperties);
        //    var dynamicParameters = GetDynamicParameters(parameters);

        //    LastExecutedCommand = sql;
        //    LastExecutedParameters = parameters;
        //    var query = connection.Query<dynamic>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);

        //    return MapColumns<T>(query);
        //}

        //public IEnumerable<TOut> GetPartialSet<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null) where TIn : class where TOut : class
        //{
        //    var cols = GetBufferedCols<TOut>();
        //    var te = InternalGetSet<TIn>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, cols, includedProperties).ToList();

        //    // Transform TIn object to Anonymous type
        //    var f = func.Compile();
        //    return te.Select(i => f.Invoke(i));
        //}

        //public IEnumerable<T> GetSet<T>(IDbConnection connection, object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return InternalGetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, null, includedProperties);
        //}

        //public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction? transaction, int? commandTimeout)
        //{
        //    //Got the information here to avoid doing it for each item and so we speed up the execution
        //    var classMap = SqlGenerator.Configuration.GetMap<T>();
        //    var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
        //    var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
        //    var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
        //    var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

        //    foreach (var e in entities)
        //        InternalInsert(connection, e, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        //}

        //public void Update<T>(IDbConnection connection, IEnumerable<T?> entities, IDbTransaction? transaction, int? commandTimeout, bool ignoreAllKeyProperties = false)
        //{
        //    InternalUpdate(connection, entities, transaction, null, commandTimeout, ignoreAllKeyProperties);
        //}

        //public void UpdatePartial<TIn, TOut>(IDbConnection connection, IEnumerable<TIn?> entities, Expression<Func<TIn, TOut>> func, IDbTransaction? transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where TIn : class
        //{
        //    var cols = GetBufferedCols<TOut>();
        //    InternalUpdate<TIn>(connection, entities, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        //}

        //protected static IPredicate? GetEntityPredicate(IClassMapper classMap, object? entity)
        //{
        //    var notIgnoredColumns = classMap.Properties.Where(p => !p.Ignored);

        //    var keyValuePairs = ReflectionHelper.GetObjectValues(entity)
        //        .Where(property => notIgnoredColumns.Any(c => c.Name == property.Key));

        //    IList<IPredicate> predicates = new List<IPredicate>();
        //    var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        //    foreach (var kvp in keyValuePairs)
        //    {
        //        var value = kvp.Value is Func<object>
        //            ? kvp.Value()
        //            : kvp.Value;

        //        var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
        //        fieldPredicate!.Operator = value is IEnumerable<object>
        //            ? Operator.In
        //            : Operator.Eq;
        //        fieldPredicate.PropertyName = kvp.Key;
        //        fieldPredicate.Value = value;
        //        predicates.Add(fieldPredicate);
        //    }

        //    return ReturnPredicate(predicates);
        //}

        //protected static IPredicate? GetIdPredicate(IClassMapper classMap, List<object>? ids)
        //{
        //    if (ids == null || ids.Count == 0)
        //    {
        //        return null;
        //    }

        //    var key = classMap.Properties.SingleOrDefault(p => p.KeyType != KeyType.NotAKey);

        //    if (key == null)
        //    {
        //        throw new NotSupportedException("We cannot generate a predicate with multiple keys Please created your own predicated in an implementation");
        //    }
        //    var predicates = new List<IPredicate>();

        //    var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        //    var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
        //    fieldPredicate.Operator = Operator.In;
        //    fieldPredicate.PropertyName = key.Name;
        //    fieldPredicate.Value = ids;
        //    predicates.Add(fieldPredicate);

        //    return ReturnPredicate(predicates);
        //}

        //protected static IPredicate? GetKeyPredicate<T>(IClassMapper classMap, T? entity)
        //{
        //    var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey && p.KeyType != KeyType.ForeignKey).ToList();
        //    if (!whereFields.Any())
        //    {
        //        throw new ArgumentException("At least one Key column must be defined.");
        //    }

        //    IList<IPredicate> predicates = new List<IPredicate>();
        //    var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        //    foreach (var field in whereFields)
        //    {
        //        var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
        //        fieldPredicate!.Operator = Operator.Eq;
        //        fieldPredicate.PropertyName = field.Name;
        //        fieldPredicate.Value = field.GetValue(entity);
        //        predicates.Add(fieldPredicate);
        //    }

        //    return ReturnPredicate(predicates);
        //}

        //protected virtual DynamicParameters AddParameter<T>(T? entity, DynamicParameters parameters, IMemberMap prop, bool useColumnAlias = false)
        //{
        //    var propValue = prop.GetValue(entity);
        //    var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, prop.Name, propValue);
        //    var alias = GetSimpleAliasFromColumnAlias(parameter.Name);
        //    var name = useColumnAlias ? string.IsNullOrEmpty(alias) ? parameter.Name : alias : parameter.Name;

        //    parameters ??= new DynamicParameters();

        //    if (prop.MemberInfo.DeclaringType == typeof(bool) || (prop.MemberInfo.DeclaringType.IsGenericType && prop.MemberType.GetGenericTypeDefinition() == typeof(Nullable<>) && prop.MemberInfo.DeclaringType.GetGenericArguments()[0] == typeof(bool)))
        //    {
        //        var value = (bool?)propValue;
        //        if (!value.HasValue)
        //        {
        //            parameters.Add(name, value, parameter.DbType,
        //                          parameter.ParameterDirection, parameter.Size, parameter.Precision,
        //                          parameter.Scale);
        //        }
        //        else
        //        {
        //            parameters.Add(name, value.Value ? 1 : 0, parameter.DbType,
        //                          parameter.ParameterDirection, parameter.Size, parameter.Precision,
        //                          parameter.Scale);
        //        }
        //    }
        //    else
        //    {
        //        parameters.Add(name, parameter.Value, parameter.DbType,
        //                          parameter.ParameterDirection, parameter.Size, parameter.Precision,
        //                          parameter.Scale);
        //    }

        //    return parameters;
        //}

        //protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate? predicate, IDbTransaction? transaction, int? commandTimeout)
        //{
        //    var parameters = new Dictionary<string, object>();
        //    var sql = SqlGenerator.Delete(classMap, predicate, parameters);
        //    var dynamicParameters = GetDynamicParameters(parameters);

        //    return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        //}

        //protected IEnumerable<T> GetList<T>(IDbConnection connection, IList<IProjection> colsToSelect, IClassMapper classMap, IPredicate? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    return GetListAutoMap<T>(connection, colsToSelect, classMap, predicate, sort, transaction, commandTimeout, buffered, includedProperties);
        //}

        //protected IEnumerable<T> GetListAutoMap<T>(IDbConnection connection, IList<IProjection> colsToSelect, IClassMapper classMap, IPredicate? predicate, IList<ISort> sort, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IReferenceMap>? includedProperties = null)
        //{
        //    var parameters = new Dictionary<string, object>();
        //    var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties);
        //    var dynamicParameters = GetDynamicParameters(parameters);

        //    LastExecutedCommand = sql;
        //    LastExecutedParameters = parameters;

        //    var query = connection.Query<dynamic>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);

        //    return MapColumns<T>(query);
        //}

        //protected virtual void GetMapAndPredicate<T>(object? predicateValue, out IClassMapper classMapper, out IPredicate? wherePredicate, bool keyPredicate = false)
        //{
        //    classMapper = SqlGenerator.Configuration.GetMap<T>();
        //    wherePredicate = keyPredicate
        //        ? GetKeyPredicate(classMapper, predicateValue)
        //        : GetPredicate(classMapper, predicateValue);
        //}

        //protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    var parameters = new Dictionary<string, object>();
        //    var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, colsToSelect, includedProperties);
        //    var dynamicParameters = GetDynamicParameters(parameters);

        //    LastExecutedCommand = sql;
        //    LastExecutedParameters = parameters;

        //    return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        //}

        //protected string GetSimpleAliasFromColumnAlias(string columnAlias)
        //{
        //    if (SqlGenerator.AllColumns.Any(c => c.Alias.Equals(columnAlias, StringComparison.InvariantCultureIgnoreCase)))
        //        return SqlGenerator.AllColumns.Where(c => c.Alias.Equals(columnAlias, StringComparison.InvariantCultureIgnoreCase)).Select(c => c.SimpleAlias).Single();
        //    return "";
        //}

        //protected bool InternalDelete<T>(IDbConnection connection, T? entity, IDbTransaction? transaction, int? commandTimeout)
        //{
        //    GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);

        //    var parameters = new Dictionary<string, object>();
        //    var sql = SqlGenerator.Delete(classMap, predicate, parameters);
        //    var dynamicParameters = GetDynamicParameters(parameters);

        //    LastExecutedCommand = sql;
        //    LastExecutedParameters = parameters;

        //    return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        //}

        //protected IEnumerable<T> InternalGetSet<T>(IDbConnection connection, object? predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction? transaction, int? commandTimeout, bool buffered, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
        //    return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties);
        //}

        //protected dynamic InternalInsert<T>(IDbConnection connection, T? entity, IDbTransaction? transaction, int? commandTimeout,
        //    IClassMapper classMap, IList<IMemberMap> nonIdentityKeyProperties, IMemberMap identityColumn,
        //    IMemberMap triggerIdentityColumn, IList<IMemberMap> sequenceIdentityColumn)
        //{
        //    DynamicParameters dynamicParameters = null;

        //    foreach (var column in nonIdentityKeyProperties)
        //    {
        //        if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity) == Guid.Empty)
        //        {
        //            var comb = SqlGenerator.Configuration.GetNextGuid();
        //            column.SetValue(entity, comb);
        //        }
        //    }

        //    IDictionary<string, object?> keyValues = new ExpandoObject();
        //    var sql = SqlGenerator.Insert(classMap);
        //    if (triggerIdentityColumn != null || identityColumn != null)
        //    {
        //        var keyColumn = triggerIdentityColumn ?? identityColumn;
        //        object keyValue;

        //        dynamicParameters = GetDynamicParameters(entity, dynamicParameters, keyColumn, true);

        //        if (triggerIdentityColumn != null)
        //        {
        //            keyValue = InsertTriggered(connection, entity, transaction, commandTimeout, sql, triggerIdentityColumn, dynamicParameters);
        //        }
        //        else
        //        {
        //            keyValue = InsertIdentity(connection, transaction, commandTimeout, sql, identityColumn, dynamicParameters);
        //        }

        //        //var expectedValue = Convert.ChangeType(keyValue, keyColumn.MemberType);

        //        keyValues.Add(keyColumn.Name, keyValue);
        //        try
        //        {
        //            keyColumn.SetValue(entity, keyValue);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //    else
        //    {
        //        dynamicParameters = GetDynamicParameters(classMap, entity, true);

        //        if (sequenceIdentityColumn.Count > 0)
        //        {
        //            if (sequenceIdentityColumn.Count > 1)
        //                throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

        //            AddSequenceParameter(connection, entity, sequenceIdentityColumn[0], dynamicParameters, keyValues);
        //        }
        //        else if (nonIdentityKeyProperties != null)
        //        {
        //            AddKeyParameters(entity, nonIdentityKeyProperties, dynamicParameters, true);
        //        }

        //        LastExecutedCommand = sql;
        //        connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
        //    }

        //    foreach (var column in nonIdentityKeyProperties)
        //    {
        //        keyValues.Add(column.Name, column.GetValue(entity));
        //    }

        //    if (keyValues.Count == 1)
        //    {
        //        return keyValues.First().Value;
        //    }

        //    return keyValues;
        //}

        //protected bool InternalUpdate<T>(IDbConnection connection, T? entity, IClassMapper classMap, IPredicate? predicate, IDbTransaction? transaction, IList<IProjection>? cols, int? commandTimeout, bool ignoreAllKeyProperties = false)
        //{
        //    var parameters = new Dictionary<string, object>();
        //    string sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties, cols);

        //    var dynamicParameters = GetDynamicParameters(classMap, entity, true);
        //    dynamicParameters.AddDynamicParams(GetDynamicParameters(parameters));

        //    LastExecutedCommand = sql;
        //    return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        //}

        //protected bool InternalUpdate<T>(IDbConnection connection, T? entity, IDbTransaction? transaction, IList<IProjection>? cols, int? commandTimeout, bool ignoreAllKeyProperties = false)
        //{
        //    GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
        //    return InternalUpdate(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        //}

        //protected void InternalUpdate<T>(IDbConnection connection, IEnumerable<T?> entities, IDbTransaction? transaction, IList<IProjection>? cols, int? commandTimeout, bool ignoreAllKeyProperties = false)
        //{
        //    GetMapAndPredicate<T>(entities.FirstOrDefault(), out var classMap, out var predicate, true);

        //    foreach (var e in entities)
        //        InternalUpdate(connection, e, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        //}

        ///// <summary>
        ///// Return property liste from (anonymous) type
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //private static IList<IProjection>? GetBufferedCols<T>()
        //{
        //    Type outType = typeof(T);

        //    lock (ColsBuffer)
        //    {
        //        if (ColsBuffer.TryGetValue(outType, out IList<IProjection>? cols) == false)
        //        {
        //            cols = new List<IProjection>();

        //            typeof(T).GetProperties().
        //                Select(i => i.Name).
        //                ToList().
        //                ForEach(p => cols.Add(new Projection(p)));

        //            ColsBuffer.Add(outType, cols);
        //        }

        //        return cols;
        //    }
        //}

        //private void AddKeyParameters<T>(T? entity, IList<IMemberMap> keyList, DynamicParameters dynamicParameters, bool useColumnAlias = false)
        //{
        //    foreach (var prop in keyList)
        //    {
        //        dynamicParameters = AddParameter(entity, dynamicParameters, prop, useColumnAlias);
        //    }
        //}

        //private IDictionary<string, object?> AddSequenceParameter<T>(IDbConnection connection, T? entity,
        //    IMemberMap key, DynamicParameters dynamicParameters, IDictionary<string, object?> keyValues)
        //{
        //    var query = $"select {key.SequenceName}.nextval seq from dual";
        //    var value = connection.ExecuteScalar<int>(query);

        //    key.SetValue(entity, value);

        //    AddParameter(entity, dynamicParameters, key, true);

        //    keyValues ??= new ExpandoObject();
        //    keyValues.Add(key.Name, value);

        //    return keyValues;
        //}

        //private DynamicParameters GetDynamicParameters<T>(T? entity, IClassMapper classMap, IList<IMemberMap> sequenceColumn, IList<MemberInfo> foreignKeys, IList<MemberInfo> ignoredColumns, bool useColumnAlias)
        //{
        //    var keyColumns = sequenceColumn.Count == 0 ? classMap.Properties.Where(p => p.KeyType == KeyType.Assigned || p.KeyType == KeyType.Guid)?.ToList() : sequenceColumn;

        //    var dynamicParameters = new DynamicParameters();
        //    var insertableProperties = entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public)
        //        .Where(p => !keyColumns.Any(k => k.Name.Equals(p.Name)) && !foreignKeys.Contains(p) && !ignoredColumns.Contains(p));
        //    foreach (var prop in insertableProperties)
        //        dynamicParameters = AddParameter(entity, dynamicParameters, new MemberMap(prop), useColumnAlias);

        //    return dynamicParameters;
        //}

        //private object? InsertIdentity(IDbConnection connection, IDbTransaction? transaction,
        //    int? commandTimeout, string sql, IMemberMap identityColumn, DynamicParameters dynamicParameters)
        //{
        //    var identitySql = SqlGenerator.IdentitySql(identityColumn);

        //    if (SqlGenerator.SupportsMultipleStatements)
        //    {
        //        sql += SqlGenerator.Configuration.Dialect.BatchSeperator + identitySql;
        //    }
        //    else
        //    {
        //        connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
        //        sql = identitySql;
        //    }

        //    LastExecutedCommand = sql;
        //    var result = connection.Query<dynamic>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);

        //    // We are only interested in the first identity, but we are iterating over all resulting items (if any).
        //    // This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
        //    var hasResult = false;
        //    object keyValue = null;
        //    foreach (var identityValue in result)
        //    {
        //        if (hasResult)
        //        {
        //            continue;
        //        }
        //        keyValue = identityValue.Id;
        //        hasResult = true;
        //    }

        //    if (!hasResult)
        //    {
        //        throw new InvalidOperationException("The source sequence is empty.");
        //    }

        //    return keyValue;
        //}

        //private object InsertTriggered<T>(IDbConnection connection, T entity, IDbTransaction? transaction,
        //    int? commandTimeout, string sql, IMemberMap key, DynamicParameters dynamicParameters)
        //{
        //    // defaultValue need for identify type of parameter
        //    var defaultValue = entity.GetType().GetProperty(key.Name).GetValue(entity, null);
        //    dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

        //    LastExecutedCommand = sql;
        //    connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);

        //    return dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
        //}
    }
}