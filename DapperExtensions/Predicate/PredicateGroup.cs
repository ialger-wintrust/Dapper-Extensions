using DapperExtensions.Sql;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions.Predicate
{
    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }

    /// <summary>
    /// Groups IPredicates together using the specified group operator.
    /// </summary>
    public class PredicateGroup : IPredicateGroup
    {
        public GroupOperator Operator { get; set; }
        public IList<IPredicate> Predicates { get; set; }

        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            string seperator = Operator == GroupOperator.And ? " AND " : " OR ";

            var predicateString = Predicates.Aggregate(new StringBuilder(),
                (sb, p) =>
                    (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(sqlGenerator, parameters, isDml)),
                sb =>
                {
                    var s = sb.ToString();

                    return s.Length == 0
                        ? sqlGenerator.Configuration.Dialect.EmptyExpression
                        : s;
                }
            );

            return $"({predicateString})";
        }
    }
}