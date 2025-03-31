using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using Newtonsoft.Json.Linq;
using Slapper;

namespace DapperExtensions;

public abstract class DapperImplementors
{
    public DbDapperCommand? LastExecutedCommand { get; protected set; }

    public ISqlGenerator SqlGenerator { get; }

    protected DapperImplementors(ISqlGenerator sqlGenerator)
    {
        LastExecutedCommand = null;
        SqlGenerator = sqlGenerator;
    }

    protected DbDapperCommand CountCommand<T>(object? predicate, IList<IReferenceMap>? includedProperties = null, IList<IProjection>? colsToSelect = null)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();

        var wherePredicate = GetPredicate(classMap, predicate);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Count(classMap, wherePredicate, parameters, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand FindCommand<T>(object predicate, IList<IReferenceMap>? includedProperties, IList<IProjection>? colsToSelect = null)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var wherePredicate = GetPredicate(classMap, predicate);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Select(classMap, wherePredicate, null, parameters, colsToSelect, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand GetCommand<T>(dynamic id, IList<IReferenceMap>? includedProperties, IList<IProjection>? colsToSelect = null)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var predicate = GetIdPredicate(classMap, id);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Select(classMap, predicate, null, parameters, colsToSelect, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand ListCommand<T>(object? predicate, IList<IReferenceMap>? includedProperties, IList<IProjection>? colsToSelect = null, IList<ISort>? sort = null, int page = 0, int resultsPerPage = 1000)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var wherePredicate = GetPredicate(classMap, predicate);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Select(classMap, wherePredicate, sort, parameters, colsToSelect, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand PageCommand<T>(object? predicate, IList<IReferenceMap>? includedProperties, IList<IProjection>? colsToSelect = null, IList<ISort>? sort = null, int page = 0, int resultsPerPage = 1000)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var wherePredicate = GetPredicate(classMap, predicate);

        if (sort == null)
        {
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);

            if (identityColumn == null)
            {
                throw new ArgumentException(
                    $"There is no identity column found for table {classMap.TableName} please provide a valid sort or fix your entity mappings");
            }

            var identitySort = Predicates.Sort<T>(identityColumn.Name);
            sort = new List<ISort>() { identitySort };
        }

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.SelectPaged(classMap, wherePredicate, sort, page, resultsPerPage, parameters, colsToSelect, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand DeleteCommand<T>(T entity)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var predicate = GetKeyPredicate(classMap, entity);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Delete(classMap, predicate, parameters);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand GetMultipleBatchedCommand(GetMultiplePredicate predicate, IList<IReferenceMap>? includedProperties = null)
    {
        var parameters = new Dictionary<string, object>();
        var sqlBuilder = new StringBuilder();
        foreach (var item in predicate.Items)
        {
            var classMap = SqlGenerator.Configuration.GetMap(item.Type);
            var itemPredicate = item.Value as IPredicate;
            if (itemPredicate == null && item.Value != null)
            {
                itemPredicate = GetPredicate(classMap, item.Value);
            }

            sqlBuilder.Append(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters, null, includedProperties)).AppendLine(SqlGenerator.Configuration.Dialect.BatchSeperator);
        }

        var sql = sqlBuilder.ToString();
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand GetMultipleSequenceCommand(GetMultiplePredicateItem predicateItem, IList<IReferenceMap>? includedProperties = null)
    {
        var parameters = new Dictionary<string, object>();
        var classMap = SqlGenerator.Configuration.GetMap(predicateItem.Type);
        var itemPredicate = predicateItem.Value as IPredicate;
        if (itemPredicate == null && predicateItem.Value != null)
        {
            itemPredicate = GetPredicate(classMap, predicateItem.Value);
        }

        var sql = SqlGenerator.Select(classMap, itemPredicate, predicateItem.Sort, parameters, null, includedProperties);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand DeleteCommand<T>(object? predicate)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var wherePredicate = GetPredicate(classMap, predicate);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Delete(classMap, wherePredicate, parameters);
        var dynamicParameters = GetDynamicParameters(parameters);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand InsertCommand<T>(T entity)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();

        var guidKeyProperties = classMap.Properties!.Where(p => p.KeyType is KeyType.Guid).ToList();

        foreach (var column in guidKeyProperties)
        {
            if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity)! == Guid.Empty)
            {
                var comb = SqlGenerator.Configuration.GetNextGuid();
                column.SetValue(entity, comb);
            }
        }

        var sql = SqlGenerator.Insert<T>(classMap);

        var dynamicParameters = GetDynamicParameters(classMap, entity, true);

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected DbDapperCommand UpdateCommand<T>(T entity)
    {
        var classMap = SqlGenerator.Configuration.GetMap<T>();
        var wherePredicate = GetKeyPredicate(classMap, entity);

        var parameters = new Dictionary<string, object>();
        var sql = SqlGenerator.Update(classMap, entity, wherePredicate, parameters);

        var dynamicParameters = GetDynamicParameters(classMap, entity, true, true);
        dynamicParameters.AddDynamicParams(GetDynamicParameters(parameters));

        var dbDapperCommand = new DbDapperCommand
        {
            SqlString = sql,
            DynamicParameters = dynamicParameters
        };

        LastExecutedCommand = dbDapperCommand;

        return dbDapperCommand;
    }

    protected static IPredicate? GetEntityPredicate(IClassMapper classMap, object? entity)
    {
        var notIgnoredColumns = classMap.Properties.Where(p => !p.Ignored);

        var keyValuePairs = ReflectionHelper.GetObjectValues(entity)
            .Where(property => notIgnoredColumns.Any(c => c.Name == property.Key));

        IList<IPredicate> predicates = new List<IPredicate>();
        var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        foreach (var kvp in keyValuePairs)
        {
            var value = kvp.Value is Func<object>
                ? kvp.Value()
                : kvp.Value;

            var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
            fieldPredicate!.Operator = value is IEnumerable<object>
                ? Operator.In
                : Operator.Eq;
            fieldPredicate.PropertyName = kvp.Key;
            fieldPredicate.Value = value;
            predicates.Add(fieldPredicate);
        }

        return ReturnPredicate(predicates);
    }

    public DynamicParameters GetDynamicParameters<T>(IClassMapper classMap, T entity, bool useColumnAlias = false, bool excludeIdentityKeys = false)
    {
        var foreignKeys = classMap.Properties.Where(p => p.KeyType == KeyType.ForeignKey).Select(p => p.MemberInfo).ToList();
        var ignored = classMap.Properties.Where(x => x.Ignored).Select(p => p.MemberInfo).ToList();

        var filteredEntityProperties = entity?.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public)
            .Where(p => !foreignKeys.Contains(p) && !ignored.Contains(p)).ToList();

        if (excludeIdentityKeys)
        {
            var identityColumns = classMap.Properties.Where(item => item.KeyType is KeyType.Identity or KeyType.SequenceIdentity).ToList();

            if (identityColumns?.Count > 1)
            {
                throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");
            }

            filteredEntityProperties = filteredEntityProperties?
                .Where(p => identityColumns != null && !identityColumns.Any(k => k.Name.Equals(p.Name))).ToList();
        }

        var dynamicParameters = new DynamicParameters();

        if (filteredEntityProperties == null)
        {
            throw new ArgumentException("No Properties were selected");
        }

        foreach (var prop in filteredEntityProperties)
        {
            dynamicParameters = AddParameter(entity, dynamicParameters, new MemberMap(prop), useColumnAlias);
        }

        return dynamicParameters;
    }

    protected DynamicParameters AddParameter<T>(T entity, DynamicParameters parameters, IMemberMap prop, bool useColumnAlias = false)
    {
        var propValue = prop.GetValue(entity);
        var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, prop.Name, propValue);

        var name = parameter.Name;
        if (useColumnAlias)
        {
            var alias = SqlGenerator.AllColumns
                .FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.InvariantCultureIgnoreCase))
                ?.SimpleAlias;

            if (!string.IsNullOrEmpty(alias))
            {
                name = alias;
            }
        }

        parameters ??= new DynamicParameters();

        if (prop.MemberInfo.DeclaringType == typeof(bool) || (prop.MemberInfo.DeclaringType.IsGenericType && prop.MemberType.GetGenericTypeDefinition() == typeof(Nullable<>) && prop.MemberInfo.DeclaringType.GetGenericArguments()[0] == typeof(bool)))
        {
            var value = (bool?)propValue;
            if (!value.HasValue)
            {
                parameters.Add(name, value, parameter.DbType,
                    parameter.ParameterDirection, parameter.Size, parameter.Precision,
                    parameter.Scale);
            }
            else
            {
                parameters.Add(name, value.Value ? 1 : 0, parameter.DbType,
                    parameter.ParameterDirection, parameter.Size, parameter.Precision,
                    parameter.Scale);
            }
        }
        else
        {
            parameters.Add(name, parameter.Value, parameter.DbType,
                parameter.ParameterDirection, parameter.Size, parameter.Precision,
                parameter.Scale);
        }

        return parameters;
    }

    protected T? MapColumns<T>(dynamic? value)
    {
        if (value == null)
        {
            return default;
        }

        var columnDictionary = PopulateDynamicDictionary(value);

        SetAutoMapperIdentifier(SqlGenerator.MappedTables);

        var t = AutoMapper.Map<T>(columnDictionary, false);
        return t;
    }

    protected IEnumerable<T>? MapColumns<T>(IEnumerable<dynamic>? values)
    {
        if (values == null)
        {
            return null;
        }

        var list = new List<Dictionary<string, object>>();

        foreach (var d in values.ToList())
        {
            Dictionary<string, object> dictionary = PopulateDynamicDictionary(d);

            list.Add(dictionary);
        }

        SetAutoMapperIdentifier(SqlGenerator.MappedTables);

        return AutoMapper.Map<T>(list, false);
    }

    protected void SetAutoMapperIdentifier(IList<Table> tables)
    {
        foreach (var table in tables)
        {
            var map = SqlGenerator.Configuration.GetMap(table.EntityType);

            var properties = map
                .Properties
                .Where(p => p.KeyType == KeyType.Assigned ||
                            p.KeyType == KeyType.Identity ||
                            p.KeyType == KeyType.SlapperIdentifierKey ||
                            p.KeyType == KeyType.SequenceIdentity)
                .Select(p => p.Name)
                .ToList();

            AutoMapper.Configuration.AddIdentifiers(table.IsVirtual ? table.EntityType.BaseType : table.EntityType, properties);
        }
    }

    private Dictionary<string, object> PopulateDynamicDictionary(dynamic d)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> kvp in d)
        {
            var alias = GetColumnAlias(kvp.Key);
            if (!string.IsNullOrEmpty(alias))
                dictionary.Add(alias, kvp.Value);
        }

        return dictionary;
    }

    // TODO this needs to be re evaluated. Right now we can only work with singular objects we will want to be able to work with bulk objects at some point in time.
    protected string GetColumnAlias(string columnKey)
    {
        var columnName = SqlGenerator.AllColumns.FirstOrDefault(c =>
            string.Equals(c.Name, columnKey, StringComparison.InvariantCultureIgnoreCase))?.Name;

        if (string.IsNullOrEmpty(columnName))
        {
            columnName = SqlGenerator.AllColumns.FirstOrDefault(c =>
                string.Equals(c.SimpleAlias, columnKey, StringComparison.InvariantCultureIgnoreCase))?.Name;
        }

        ArgumentException.ThrowIfNullOrEmpty(columnName, nameof(columnName));

        return columnName;
    }

    private static IPredicate? GetIdPredicate(IClassMapper classMap, object? id)
    {
        var isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
        var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
        IDictionary<string, Func<object>> paramValues = null;
        var predicates = new List<IPredicate>();
        if (!isSimpleType)
        {
            paramValues = ReflectionHelper.GetObjectValues(id);
        }

        foreach (var key in keys)
        {
            var value = id;
            if (!isSimpleType)
            {
                value = paramValues[key.Name];
            }

            var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

            var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
            fieldPredicate.Operator = Operator.Eq;
            fieldPredicate.PropertyName = key.Name;
            fieldPredicate.Value = value;
            predicates.Add(fieldPredicate);
        }

        return ReturnPredicate(predicates);
    }

    private static DynamicParameters GetDynamicParameters(Dictionary<string, object> parameters)
    {
        var dynamicParameters = new DynamicParameters();
        foreach (var parameter in parameters)
        {
            if (parameter.Value is Parameter p)
            {
                dynamicParameters.Add(p.Name, p.Value, p.DbType,
                    p.ParameterDirection, p.Size, p.Precision,
                    p.Scale);
            }
            else
            {
                dynamicParameters.Add(parameter.Key, GetParameterValue(parameter));
            }
        }
        return dynamicParameters;
    }

    private static object GetParameterValue(KeyValuePair<string, object> parameter)
    {
        var value = parameter.Value;
        if (parameter.Value is JToken)
        {
            var val = (JToken)value;
            value = Convert.ChangeType(val, val.Type.GetType());
        }
        return value;
    }

    private static IPredicate? ReturnPredicate(IList<IPredicate> predicates)
    {
        return predicates.Count == 1
            ? predicates[0]
            : new PredicateGroup
            {
                Operator = GroupOperator.And,
                Predicates = predicates
            };
    }

    protected static IPredicate? GetIdPredicate(IClassMapper classMap, List<object>? ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return null;
        }

        var key = classMap.Properties.SingleOrDefault(p => p.KeyType != KeyType.NotAKey);

        if (key == null)
        {
            throw new NotSupportedException("We cannot generate a predicate with multiple keys Please created your own predicated in an implementation");
        }
        var predicates = new List<IPredicate>();

        var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
        fieldPredicate.Operator = Operator.In;
        fieldPredicate.PropertyName = key.Name;
        fieldPredicate.Value = ids;
        predicates.Add(fieldPredicate);

        return ReturnPredicate(predicates);
    }

    protected static IPredicate? GetKeyPredicate<T>(IClassMapper classMap, T? entity)
    {
        var whereFields = classMap.Properties?.Where(p => p.KeyType == KeyType.Identity && p.KeyType != KeyType.ForeignKey).ToList();

        if (whereFields == null || !whereFields.Any())
        {
            throw new ArgumentException("At least one Key column must be defined.");
        }

        var predicates = new List<IPredicate>();
        var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
        foreach (var field in whereFields)
        {
            var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
            fieldPredicate!.Operator = Operator.Eq;
            fieldPredicate.PropertyName = field.Name;
            fieldPredicate.Value = field.GetValue(entity);
            fieldPredicate.UseTableAlias = true;
            predicates.Add(fieldPredicate);
        }

        return ReturnPredicate(predicates);
    }

    protected static IPredicate? GetPredicate(IClassMapper classMap, object? predicate)
    {
        var wherePredicate = predicate as IPredicate;
        if (wherePredicate == null && predicate != null)
        {
            wherePredicate = GetEntityPredicate(classMap, predicate);
        }

        return wherePredicate;
    }
}

public class DbDapperCommand
{
    public required string SqlString { get; set; }
    public DynamicParameters? DynamicParameters { get; set; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(SqlString);

        if (DynamicParameters == null)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine("{");

        foreach (var parameterName in DynamicParameters.ParameterNames)
        {
            var parameterValue = DynamicParameters.Get<object>(parameterName);
            stringBuilder.AppendLine($"\t{parameterName}={parameterValue}");
        }

        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }
}