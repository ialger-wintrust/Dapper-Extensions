using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DapperExtensions.Sql
{
    public interface ISqlGenerator
    {
        IList<IColumn> AllColumns { get; }
        IDapperExtensionsConfiguration Configuration { get; }
        IList<Table> MappedTables { get; }

        bool SupportsMultipleStatements { get; }

        string Count(IClassMapper classMap, IPredicate? predicate, IDictionary<string, object> parameters, IList<IReferenceMap>? includedProperties = null);

        string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);

        string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false, bool includePrefix = true);

        string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true);

        string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true);

        IEnumerable<IColumn> GetColumns();

        string GetTableName(IClassMapper map, bool useAlias = false);

        //string SelectSet(IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, int firstResult, int maxResults, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null);
        string Insert<T>(IClassMapper classMap, bool useSimpleAlias = true);

        string Select(IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null);

        string SelectPaged(IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, int page, int resultsPerPage, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null);

        string Update<T>(IClassMapper classMap, T entity, IPredicate? predicate, IDictionary<string, object> parameters, bool useSimpleAlias = true);
    }

    public class SqlGeneratorImpl : ISqlGenerator
    {
        private readonly IList<Table> TableReferencesAdded = new List<Table>();
        private readonly IList<Table> TablesAdded = new List<Table>();
        private int TableCount;

        public SqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
        {
            Configuration = configuration;
            Tables = new List<Table>();
        }

        public virtual IList<IColumn> AllColumns { get; private set; }
        public IDapperExtensionsConfiguration Configuration { get; }
        public IList<Table> MappedTables => Tables;
        public bool SupportsMultipleStatements => Configuration.Dialect.SupportsMultipleStatements;
        private IList<Table> Tables { get; set; }

        public static IList<IReferenceProperty> GetReferenceProperties(IClassMapper map)
        {
            return
                map.References
                   .SelectMany(c => c.ReferenceProperties)
                   .Select(r => r)
                   .Distinct()
                   .ToList();
        }

        public virtual string BuildSelectColumns(IClassMapper classMap, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        {
            AllColumns = new List<IColumn>();
            MapTables(classMap, includedProperties);
            AllColumns = GetColumns().ToList();

            var columns = AllColumns
                .Where(col => !col.Property.Ignored && (colsToSelect == null || colsToSelect?.Any(c => c.PropertyName.Equals(col.Property.ColumnName, StringComparison.OrdinalIgnoreCase)) == true))
                .Select(col => GetColumnName(col, true));

            var result = columns.AppendStrings();
            return string.IsNullOrEmpty(result) ? throw new NotSupportedException("Query with empty ClassMapper is not supported.") : result;
        }

        public virtual string Count(IClassMapper classMap, IPredicate? predicate, IDictionary<string, object> parameters, IList<IReferenceMap>? includedProperties = null)
        {
            var sql = new StringBuilder();

            if (includedProperties?.Count > 0 && Configuration.Dialect.SupportsCountOfSubquery)
            {
                var countSql = new StringBuilder();
                var resultSet = BuildSelectColumns(classMap, null, includedProperties);

                sql.Append("SELECT ").Append(resultSet).Append(" FROM ").Append(GetTables(classMap, parameters, includedProperties));

                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(this, parameters));
                }

                var partitionBy = GetPartitionBy();

                countSql.Append("SELECT ")
                    .Append(partitionBy)
                    .Append(", COUNT(*) OVER(PARTITION BY ")
                    .Append(partitionBy)
                    .Append(" ORDER BY ")
                    .Append(partitionBy)
                    .Append(") AS QTDPARTIONED FROM (")
                    .Append(sql)
                    .Append(") GROUP BY ")
                    .Append(partitionBy);

                return Configuration.Dialect.GetCountSql($"({countSql})");
            }

            sql.Append(GetTables(classMap, parameters, includedProperties));

            if (predicate != null)
            {
                sql.Append(" WHERE ")
                   .Append(predicate.GetSql(this, parameters));
            }

            return Configuration.Dialect.GetCountSql(sql.ToString().Trim());
        }

        public virtual string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            var sql = new StringBuilder($"DELETE FROM {GetTableName(classMap)}");
            sql.Append(" WHERE ").Append(predicate.GetSql(this, parameters, true));
            return sql.ToString();
        }

        //Todo we may want to implement this at some point to help determine what more complex queries that support joins but for now this is out of scope
        //public virtual string GetAllJointTables(IClassMapper mainMap, string mainTable, IDictionary<string, object> parameters, string referenceName = "", IList<IReferenceMap>? includedProperties = null)
        //{
        //    if (parameters == null)
        //    {
        //        throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
        //    }

        //    var result = new StringBuilder();
        //    var main = Tables.Where(t => t.Identity == t.ParentIdentity).ToList();
        //    var dependents = Tables.Where(t => t.Identity != t.ParentIdentity).ToList();

        //    foreach (var dependent in dependents)
        //    {
        //        var parentEntity = Tables
        //            .Where(t => t.Identity == dependent.ParentIdentity)
        //            .Select(c => c.EntityType)
        //            .Single();

        //        var map = Configuration.GetMap(parentEntity);

        //        result.AppendLine(GetJointTables(map, dependent, parameters, includedProperties));

        //        var sqlInjectionDictionary = Configuration.GetOrSetSqlInjection(map.EntityType);
        //        var sqlInjection = GetJoinFromSqlInjection(sqlInjectionDictionary);

        //        if (sqlInjectionDictionary != null && !result.ToString().Contains(sqlInjection))
        //        {
        //            result.AppendLine(sqlInjection);
        //        }
        //    }

        //    return result.ToString();
        //}

        public virtual string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column), $"{nameof(column)} cannot be null.");
            }

            var alias = includeAlias ? column.SimpleAlias : null;
            var prefix = includePrefix ? GetTableAlias(column) : null;

            return Configuration.Dialect.GetColumnName(prefix, column.Property.ColumnName, alias);
        }

        public virtual string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false, bool includePrefix = true)
        {
            if (isDml)
                return Configuration.Dialect.GetColumnName(GetTableName(map), property.ColumnName, "");

            if (AllColumns?.Any(c => c.Property == property) == true)
            {
                foreach (var c in AllColumns.Where(c => c.TableIdentity == map.Identity))
                {
                    c.ClassMapper.GetType().GetProperty("SimpleAlias").SetValue(c.ClassMapper, GetTableAlias(c), null);
                }
            }

            var alias = (property.ColumnName != property.Name && includeAlias) ? property.Name : null;
            var prefix = includePrefix ? (!string.IsNullOrEmpty(map.SimpleAlias) ? map.SimpleAlias : GetTableName(map)) : null;

            return Configuration.Dialect.GetColumnName(prefix, property.ColumnName, alias);
        }

        public virtual string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true)
        {
            var propertyMap = map?.Properties?.SingleOrDefault(p => propertyName.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(map, propertyMap, includeAlias, false, includePrefix);
        }

        public IEnumerable<IColumn> GetColumns()
        {
            var columns = new List<IColumn>();

            foreach (var table in Tables)
                columns.AddRange(GetColumns(table));

            return columns;
        }

        public Table GetMappedTables(Type entityType, Type? parentEntityType = null)
        {
            return Tables.Single(t => t.ParentEntityType == parentEntityType
                                  && t.EntityType == entityType);
        }

        public Table GetMappedTables(Guid identity, Guid parentIdentity)
        {
            return Tables.Single(t => t.Identity.Equals(identity)
                                  && t.ParentIdentity.Equals(parentIdentity));
        }

        public string GetReference(Table table, string parentReference = "")
        {
            var _reference = "";

            if (table.PropertyInfo != null && table.ParentIdentity != table.Identity)
            {
                _reference = (!string.IsNullOrEmpty(parentReference)) ? table.PropertyInfo.Name + "_" + parentReference : table.PropertyInfo.Name;
                var parentTable = Tables.Single(t => t.Identity == table.ParentIdentity);
                var refResult = GetReference(parentTable, parentReference);
                return !string.IsNullOrEmpty(refResult) ? refResult + "_" + _reference : _reference;
            }
            return _reference;
        }

        public virtual string GetReferenceKey(IMemberMap map)
        {
            return $"{map.ClassMapper.TableName}.{map.ColumnName}";
        }

        public virtual string GetTableName(IClassMapper map, bool useAlias = false)
        {
            return Configuration.Dialect.GetTableName(map.SchemaName, map.TableName, useAlias ? GetAliasFromTableName(map.Identity) : null);
        }

        public virtual string GetTableAlias(IColumn column)
        {
            if (column.Table == null)
            {
                throw new KeyNotFoundException("Table column not set.");
            }

            return column.Table.Alias;
        }

        public virtual string GetTables(IClassMapper map, IDictionary<string, object> parameters, IList<IReferenceMap>? includedProperties = null)
        {
            var _includeRelationalEntities = includedProperties?.Count > 0;

            if (parameters == null && _includeRelationalEntities)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var tableName = new StringBuilder();

            var mainTableName = GetTableName(map, true);
            //var joints = _includeRelationalEntities ? GetAllJointTables(map, map.TableName, parameters, includedProperties: includedProperties) : "";
            var sqlInjection = GetJoinFromSqlInjection(Configuration.GetOrSetSqlInjection(map.EntityType));

            tableName.AppendLine(mainTableName);

            //if (!string.IsNullOrEmpty(joints))
            //    tableName.AppendLine(joints);

            if (!tableName.ToString().Contains(sqlInjection))
                tableName.AppendLine(sqlInjection);

            return tableName.ToString();
        }

        public string Insert<T>(IClassMapper classMap, bool useSimpleAlias = true)
        {
            MapTables(classMap);

            AllColumns = GetColumns().ToList();

            var relevantInsertColumns = AllColumns
                .Where(p => p.Property is { Ignored: false, IsReadOnly: false, KeyType: not KeyType.Identity })
                .ToList();

            var insertColumns = useSimpleAlias
                ? relevantInsertColumns.Select(col => $"{col.SimpleAlias}").ToList()
                : relevantInsertColumns.Select(col => $"{col.Alias}").ToList();

            if (insertColumns.Count == 0)
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var columnNames = relevantInsertColumns
                .Select(p => GetColumnName(p, false, false))
                .ToList();

            var tableName = GetTableName(classMap);
            var columnNameStrings = string.Join(",", columnNames);  //columnNames.AppendStrings();
            var insertColumnsStrings = string.Join(",", insertColumns); //insertColumns.AppendStrings());
            var identitySql = Configuration.Dialect.GetIdentitySql();

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("table name must contain a valid value", nameof(tableName));
            }

            if (string.IsNullOrEmpty(columnNameStrings))
            {
                throw new ArgumentException("Column names must contain a valid value", nameof(columnNameStrings));
            }

            if (string.IsNullOrEmpty(insertColumnsStrings))
            {
                throw new ArgumentException("Insert Column names must contain a valid value", nameof(insertColumnsStrings));
            }

            var sql = string.IsNullOrEmpty(identitySql)
                ? $"INSERT INTO {tableName} ({columnNameStrings}) VALUES ({insertColumnsStrings})"
                : $"INSERT INTO {tableName} ({columnNameStrings}) {identitySql} VALUES ({insertColumnsStrings})";

            return sql;
        }

        public void MapTables(IClassMapper classMap, IList<IReferenceMap>? includedProperties = null)
        {
            Tables = new List<Table>();
            TableCount = 0;
            TablesAdded.Clear();
            TableReferencesAdded.Clear();

            Tables = GetAllMappedTables(classMap, classMap, null, false, includedProperties).ToList();
        }

        public virtual string Select(IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        {
            ArgumentNullException.ThrowIfNull(classMap, nameof(classMap));
            ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

            MapTables(classMap);

            AllColumns = GetColumns().ToList();

            var selectColumns = string.Join(", ", AllColumns.Select(c => c.Name));
            var tableName = GetTableName(classMap, true);

            var sql = new StringBuilder($"Select {selectColumns} From {tableName}");

            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));
            }

            if (sort?.Any() == true)
            {
                var orderBy = sort.Select(s =>
                {
                    var property = (s.Properties?.Count > 1) ? s.Properties?.Last() : null;
                    var type = property?.DeclaringType;

                    var map = (type != null) ? Configuration.GetMap(type) : classMap;
                    var propertyName = property?.Name ?? s.PropertyName;

                    return GetColumnName(map, propertyName, false) + (s.Ascending ? " ASC" : " DESC");
                });

                sql.Append(" ORDER BY ")
                    .Append(string.Join(", ", orderBy));
            }

            return sql.ToString();
        }

        public virtual string SelectPaged(IClassMapper classMap, IPredicate? predicate, IList<ISort>? sort, int page, int resultsPerPage, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        {
            if (sort?.Any() != true)
            {
                throw new ArgumentNullException(nameof(Sort), $"{nameof(Sort)} cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null");
            }

            var innerSql = new StringBuilder(Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties));

            var partitionBy = GetPartitionBy();

            return Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters, partitionBy);
        }

        public string Update<T>(IClassMapper classMap, T entity, IPredicate? predicate, IDictionary<string, object> parameters, bool useSimpleAlias = true)
        {
            MapTables(classMap);

            AllColumns = GetColumns().ToList();

            var relevantUpdateColumns = AllColumns
                .Where(p => p.Property is { Ignored: false, IsReadOnly: false, KeyType: not KeyType.Identity })
                .ToList();

            var updateColumns = useSimpleAlias
                ? relevantUpdateColumns.Select(col => $"{col.SimpleAlias}").ToList()
                : relevantUpdateColumns.Select(col => $"{col.Alias}").ToList();

            if (updateColumns.Count == 0)
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var tableName = GetTableName(classMap);

            var setCommands = new List<string>();

            foreach (var relevantUpdateColumn in relevantUpdateColumns)
            {
                var name = GetColumnName(relevantUpdateColumn, false, false);
                var parameterName = useSimpleAlias
                    ? relevantUpdateColumn.SimpleAlias
                    : relevantUpdateColumn.Alias;

                setCommands.Add($"{name} = {parameterName}");
            }

            var setCommandStrings = string.Join(",", setCommands); //insertColumns.AppendStrings());

            ArgumentNullException.ThrowIfNull(predicate);

            var predicateString = predicate.GetSql(this, parameters, true);

            var sql = $"UPDATE {tableName} SET {setCommandStrings} WHERE {predicateString}";

            return sql;
        }

        private static IMemberMap GetPropertyMap(IClassMapper mainMap, MemberInfo propertyInfo)
        {
            if (!mainMap.Properties.Any(p => p.MemberInfo == propertyInfo))
                throw new KeyNotFoundException($"The property {propertyInfo.Name} was not found in {mainMap.EntityType.Name} entity");
            return mainMap
                .Properties
                .Where(p => p.MemberInfo == propertyInfo)
                .Select(propertyMap => propertyMap)
                .Single();
        }

        private static void SetReferencePropertiesParentIdentity(IClassMapper mapper, Guid identity)
        {
            mapper.References.ToList().ForEach(e =>
            {
                e.SetParentIdentity(identity);
                e.ReferenceProperties.ToList().ForEach(r => r.SetParentIdentity(identity));
            });
        }

        private IClassMapper CreateVirtualClassMap(Type entityType, Guid parentIdentity)
        {
            var originalMap = Configuration.GetMap(entityType);
            var virtualClassMapper = GetVirtualClassMapper(originalMap);

            object[] argsP = { parentIdentity };
            virtualClassMapper.GetType().GetMethod("SetParentIdentity").Invoke(virtualClassMapper, argsP);

            return virtualClassMapper;
        }

        private string GetAliasFromTableName(Guid identity)
        {
            return Tables
                .Where(x => x.Identity == identity)
                .Select(s => s.Alias)
                .FirstOrDefault();
        }

        private IList<Table> GetAllMappedTables(IClassMapper parentClassMapper, IClassMapper topParentMap, PropertyInfo propertyInfo, bool isVirtualMap = false, IList<IReferenceMap>? includedProperties = null)
        {
            var tables = new List<Table>();
            var _table = new Table();

            //Set new Identity and Parent Identity to most top map.
            ProcessRelationationalIdentities(ref parentClassMapper, ref topParentMap, propertyInfo, includedProperties);

            TableCount++;
            _table = new Table
            {
                Alias = "y_" + TableCount,
                EntityType = parentClassMapper.EntityType,
                Name = parentClassMapper.TableName,
                ReferenceName = "",
                Identity = parentClassMapper.Identity,
                ParentIdentity = topParentMap.Identity,
                IsVirtual = isVirtualMap,
                PropertyInfo = propertyInfo,
                ClassMapper = parentClassMapper
            };

            /** Creates a virtual mapping for nested references in the current ClassMapper **/
            var virtualReferenceMap = GetVirtualReferenceMap(ref _table, isVirtualMap, topParentMap.Identity, parentClassMapper.EntityType, includedProperties);

            if (parentClassMapper.Identity == topParentMap.Identity || includedProperties.Any(a => a.Identity == _table.Identity))
            {
                tables.Add(_table);
                TablesAdded.Add(_table);

                tables.AddRange(ProcessReferences(parentClassMapper, topParentMap, virtualReferenceMap, includedProperties));
            }
            return tables;
        }

        private IEnumerable<IColumn> GetColumns(Table table)
        {
            var reference = GetReference(table);

            string GetParentReference(IMemberMap map) => map.ParentProperty != null ? map.ParentProperty.Name : string.Empty;

            var map = table.ClassMapper ?? Configuration.GetMap(table.EntityType);
            var propertyMaps = map.Properties?
                .Where(p => (map.References == null || map.References.Any(r => r.PropertyInfo.Name == p.Name) == false)
                            && map.Properties?.Any(mp => mp.ParentProperty == p) == false
                            && !p.Ignored)
                .ToList();

            ArgumentNullException.ThrowIfNull(propertyMaps);

            var columnList = new List<IColumn>();

            for (var i = 0; i < propertyMaps.Count; i++)
            {
                var m = propertyMaps[i];
                var name = !string.IsNullOrEmpty(reference) ? reference + GetParentReference(m) + "_" + m.Name : GetParentReference(m) + m.Name;

                if (string.IsNullOrEmpty(name))
                {
                    name = $"Column{i}";
                }

                if (!string.Equals(m.ColumnName, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Does this ever happen? I want to know if I really need to use the parent reference stuff");
                }

                var alias = $"{Configuration.Dialect.ParameterPrefix}{name}";
                var simpleAlias = $"{Configuration.Dialect.ParameterPrefix}i_{i}";
                var column = new Column(name, alias, simpleAlias, m, table);

                columnList.Add(column);
            }

            return columnList;
        }

        private IReferenceMap GetIdentityFromIncludedProperties(IList<IReferenceMap> includedProperties, PropertyInfo property, Guid parentIdentity)
        {
            return includedProperties.FirstOrDefault(p => p.ParentIdentity == parentIdentity && p.PropertyInfo == property &&
                      !TablesAdded.Any(t => t.Identity == p.Identity));
        }

        private string GetJoinFromSqlInjection(SqlInjection sqlInjection)
        {
            if (sqlInjection == null)
                return "";
            var map = Configuration.GetMap(sqlInjection.EntityType);
            var sql = sqlInjection.Sql;

            var columName = map.Properties
               .Where(p => p.Name.Equals(sqlInjection.Property, StringComparison.InvariantCultureIgnoreCase))
               .Select(c => c.ColumnName)
               .FirstOrDefault();
            return string.Format(sql, GetAliasFromTableName(map.Identity) + "." + columName);
        }

        // TODO We may want to implement this at some point.
        //private string GetJointTables(IClassMapper mainMap, Table table, IDictionary<string, object> parameters, IList<IReferenceMap>? includedProperties = null)
        //{
        //    if (parameters == null)
        //    {
        //        throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
        //    }

        //    var result = new StringBuilder();
        //    var joins = new StringBuilder();
        //    var sql = new StringBuilder();
        //    var parent = Tables.Single(t => t.Identity == table.ParentIdentity);
        //    JoinType joinType;
        //    IPredicateGroup predicate = null;
        //    var useIncludedProperties = includedProperties?.Any(r => r.PropertyInfo == table.PropertyInfo);

        //    if (useIncludedProperties == true)
        //    {
        //        var map = includedProperties.FirstOrDefault(r => r.PropertyInfo.PropertyType == table.PropertyInfo.PropertyType && r.Identity == table.Identity);

        //        joinType = map.JoinType;
        //        predicate = map.JoinPredicate;
        //    }
        //    else
        //    {
        //        joinType = mainMap.References
        //            .Where(r => r.ParentIdentity == mainMap.Identity && r.PropertyInfo == table.PropertyInfo && r.EntityType == table.EntityType)
        //            .Select(r => r.JoinType)
        //            .SingleOrDefault();
        //    }

        //    sql.Append(' ').Append(Enum.GetName(typeof(JoinType), joinType)).Append(" join ").Append(table.Name).Append(' ').Append(table.Alias).Append(" on ");

        //    joins.AppendLine(sql.ToString());

        //    var leftClasMap = Configuration.GetMap(table.EntityType);
        //    var rightClasMap = mainMap;

        //    var properties = mainMap
        //       .References
        //       .Where(r => r.ParentIdentity == mainMap.Identity && r.PropertyInfo == table.PropertyInfo)
        //      .SelectMany(c => c.ReferenceProperties)
        //      .ToList();

        //    var isFirstComparison = true;

        //    if (properties.Count > 0)
        //    {
        //        foreach (var property in properties)
        //        {
        //            var leftPropertyMap = GetPropertyMap(leftClasMap, property.LeftProperty.PropertyInfo);
        //            var rightPropertyMap = GetPropertyMap(rightClasMap, property.RightProperty.PropertyInfo);

        //            sql.Clear();
        //            sql.Append(isFirstComparison ? " " : " and ");
        //            sql.Append(' ').Append(table.Alias).Append('.').Append(leftPropertyMap.ColumnName)
        //                .Append(' ').Append(property.ComparatorSignal).Append(' ').Append(parent.Alias).Append('.').Append(rightPropertyMap.ColumnName);
        //            joins.AppendLine(sql.ToString());
        //            isFirstComparison = false;
        //        }
        //    }

        //    if (predicate != null)
        //    {
        //        joins.Append(" AND ").AppendLine(predicate.GetSql(this, parameters));
        //    }

        //    result.Append(joins);
        //    return result.ToString();
        //}

        private string GetPartitionBy()
        {
            var partitionBy = AllColumns.Where(c =>
                c.Property.KeyType == KeyType.Assigned ||
                c.Property.KeyType == KeyType.Identity ||
                c.Property.KeyType == KeyType.SequenceIdentity)
                .Select(c => c.SimpleAlias).FirstOrDefault();

            if (!string.IsNullOrEmpty(partitionBy))
            {
                var keyTable = AllColumns.Where(c => c.SimpleAlias.Equals(partitionBy)).Select(c => c.Table).First();

                return AllColumns.Where(c => (c.Property.KeyType == KeyType.Assigned || c.Property.KeyType == KeyType.Identity ||
                     c.Property.KeyType == KeyType.SequenceIdentity) && c.Table.Equals(keyTable))
                    .Select(c => c.SimpleAlias)
                    .Aggregate((prior, next) => $"{prior}, {next}");
            }
            else
            {
                return AllColumns.Select(c => c.SimpleAlias).First();
            }
        }

        //public virtual string SelectSet(IClassMapper classMap, IPredicate? predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, IList<IProjection>? colsToSelect, IList<IReferenceMap>? includedProperties = null)
        //{
        //    if (sort?.Any() != true)
        //    {
        //        throw new ArgumentNullException(nameof(Sort), $"{nameof(Sort)} cannot be null or empty.");
        //    }

        //    if (parameters == null)
        //    {
        //        throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
        //    }
        private IClassMapper GetVirtualClassMapper(IClassMapper mapper)
        {
            var assemblyBuilder = ReflectionHelper.CreateAssemblyBuilder(mapper.EntityType.Assembly.GetName().Name);
            var moduleBuilder = ReflectionHelper.CreateModuleBuilder(assemblyBuilder, "VirtualModules.dll");

            var tbEntity = ReflectionHelper.CreateTypeBuilder(moduleBuilder, mapper.EntityType.Name, mapper.EntityType);
            var virtualEntity = ReflectionHelper.CreateVirtualType(tbEntity, mapper.EntityType);

            var tbMapper = ReflectionHelper.CreateTypeBuilder(moduleBuilder, mapper.EntityType.Name + "Map", mapper.GetType());
            ReflectionHelper.CreateMapType(tbMapper, virtualEntity, mapper.GetType());

            var virtualMapInstance = Configuration.GetMap(virtualEntity);

            object[] argsP = { virtualEntity };
            virtualMapInstance.GetType().GetMethod("SetEntityType").Invoke(virtualMapInstance, argsP);

            return virtualMapInstance;
        }

        private IClassMapper? GetVirtualReferenceMap(ref Table table, bool isVirtual, Guid parentIdentity, Type parentType, IList<IReferenceMap> includedProperties)
        {
            if (!isVirtual) return null;
            var identity = TablesAdded.Where(c => c.Identity == parentIdentity && c.IsVirtual).ToList();
            if (identity.Count > 0)
                parentIdentity = identity.Select(i => i.Identity).Last();

            table.LastIdentity = table.Identity;

            var virtualReferenceMap = CreateVirtualClassMap(parentType, parentIdentity);

            //Set new identity to virtual map
            var newIdentity = GetIdentityFromIncludedProperties(includedProperties, table.PropertyInfo, table.ParentIdentity);

            if (newIdentity != null)
            {
                virtualReferenceMap.SetIdentity(newIdentity.Identity);
                SetReferencePropertiesParentIdentity(virtualReferenceMap, newIdentity.Identity);
            }

            table.Identity = virtualReferenceMap.Identity;
            table.ParentIdentity = virtualReferenceMap.ParentIdentity;
            table.EntityType = virtualReferenceMap.EntityType;

            return virtualReferenceMap;
        }

        private IList<Table> ProcessReference(IReferenceMap reference, IClassMapper mapper, IClassMapper parent, IClassMapper? virtualReferenceMap, IList<IReferenceMap> includedProperties)
        {
            IClassMapper getTopParentMap(IClassMapper? virtualMap, IReferenceMap referenceMap) =>
            virtualMap != null
            && includedProperties.Any(i => i.ParentIdentity == virtualMap.Identity && i.PropertyInfo == reference.PropertyInfo)
                ? virtualMap
                : mapper;

            var tables = new List<Table>();

            var map = Configuration.GetMap(reference.EntityType);

            var isVirtual = TablesAdded.Any(a => a.Identity == map.Identity && a.ParentIdentity == parent.Identity);
            if (!isVirtual || (isVirtual && !TablesAdded.Any(a => a.ParentIdentity == map.Identity && a.IsVirtual)))
            {
                var topParentParam = getTopParentMap(virtualReferenceMap, reference);
                tables.AddRange(GetAllMappedTables(map, mapper, reference.PropertyInfo, isVirtual, includedProperties));
            }

            return tables;
        }

        private IList<Table> ProcessReferences(IClassMapper mapper, IClassMapper parent, IClassMapper virtualReferenceMap, IList<IReferenceMap> includedProperties)
        {
            IEnumerable<IReferenceMap> getReferences(IClassMapper mapper) =>
             mapper.References.Where(r =>
                        (r.ParentIdentity == mapper.Identity)
                        && includedProperties.Any(a => a.PropertyInfo == r.PropertyInfo && a.ParentIdentity == mapper.Identity));

            var tables = new List<Table>();

            if (includedProperties?.Count > 0)
                foreach (var reference in getReferences(mapper))
                    tables.AddRange(ProcessReference(reference, mapper, parent, virtualReferenceMap, includedProperties));

            return tables;
        }

        private void ProcessRelationationalIdentities(ref IClassMapper mapper, ref IClassMapper parent, PropertyInfo propertyInfo, IList<IReferenceMap> includedProperties)
        {
            if (includedProperties?.Count > 0)
                if (mapper.Identity == parent.Identity)
                {
                    var parentIdentity = includedProperties[0].ParentIdentity;
                    mapper.SetIdentity(parentIdentity);
                    mapper.SetParentIdentity(parentIdentity);

                    parent.SetIdentity(parentIdentity);
                    parent.SetParentIdentity(parentIdentity);

                    SetReferencePropertiesParentIdentity(mapper, parentIdentity);
                    SetReferencePropertiesParentIdentity(parent, parentIdentity);
                }
                else
                {
                    var parentIdentity = parent.Identity;
                    var childIdentityFromIncluded = includedProperties.FirstOrDefault(i => TablesAdded.Any(a => a.Identity == i.ParentIdentity) &&
                        i.ParentIdentity == parentIdentity && i.PropertyInfo == propertyInfo);

                    if (childIdentityFromIncluded != null)
                    {
                        mapper.SetIdentity(childIdentityFromIncluded.Identity);
                        SetReferencePropertiesParentIdentity(mapper, childIdentityFromIncluded.Identity);
                    }
                }
        }
    }
}