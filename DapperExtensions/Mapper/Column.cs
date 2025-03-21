using System;

namespace DapperExtensions.Mapper
{
    public interface IColumn
    {
        Guid TableIdentity { get; set; }
        string Name { get; set; }
        string Alias { get; set; }
        string SimpleAlias { get; set; }
        IClassMapper ClassMapper { get; set; }
        IMemberMap Property { get; set; }
        Table Table { get; set; }
    }

    public class Column : IColumn
    {
        public Guid TableIdentity { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string SimpleAlias { get; set; }
        public IMemberMap Property { get; set; }
        public IClassMapper ClassMapper { get; set; }

        public Table Table { get; set; }

        [Obsolete]
        public Column()
        {
        }

        [Obsolete]
        public Column(string columnName, IMemberMap property, IClassMapper classMapper, Table table)
        {
            Name = columnName;
            ClassMapper = classMapper;
            Property = property;
            TableIdentity = table.Identity;
            Table = table;
        }

        public Column(string name, string alias, string simpleAlias, IMemberMap property, Table table)
        {
            Name = name;
            Alias = alias;
            SimpleAlias = simpleAlias;
            Property = property;
            ClassMapper = property.ClassMapper;
            TableIdentity = table.Identity;
            Table = table;
        }
    }
}