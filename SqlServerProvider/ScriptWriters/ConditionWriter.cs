using ClussPro.Base.Data.Conditions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.SqlServerProvider.ScriptWriters
{
    internal static class ConditionWriter
    {
        public static string WriteCondition(ICondition condition, SqlParameterCollection parameters)
        {
            string sql;

            if (condition is Condition)
            {
                sql = WriteCondition((Condition)condition, parameters);
            }
            else if (condition is ConditionGroup)
            {
                sql = WriteConditionGroup((ConditionGroup)condition, parameters);
            }
            else
            {
                throw new InvalidCastException("Could not determine Condition when writing");
            }

            return sql;
        }

        private static string WriteCondition(Condition condition, SqlParameterCollection parameters)
        {
            StringBuilder sqlBuilder = new StringBuilder("(");

            sqlBuilder.Append(OperandWriter.WriteOperand(condition.Left, parameters));

            switch(condition.ConditionType)
            {
                case Condition.ConditionTypes.Equal:
                    sqlBuilder.Append("=");
                    break;
                case Condition.ConditionTypes.Greater:
                    sqlBuilder.Append(">");
                    break;
                case Condition.ConditionTypes.GreaterEqual:
                    sqlBuilder.Append(">=");
                    break;
                case Condition.ConditionTypes.Less:
                    sqlBuilder.Append("<");
                    break;
                case Condition.ConditionTypes.LessEqual:
                    sqlBuilder.Append("<=");
                    break;
                case Condition.ConditionTypes.NotEqual:
                    sqlBuilder.Append("!=");
                    break;
                case Condition.ConditionTypes.List:
                    sqlBuilder.Append(" IN ");
                    break;
                case Condition.ConditionTypes.NotList:
                    sqlBuilder.Append(" NOT IN ");
                    break;
                default:
                    throw new InvalidCastException("Could not determine Condition Type while writing");
            }

            sqlBuilder.Append(OperandWriter.WriteOperand(condition.Right, parameters));

            sqlBuilder.Append(")");

            return sqlBuilder.ToString();
        }

        private static string WriteConditionGroup(ConditionGroup conditionGroup, SqlParameterCollection parameters)
        {
            StringBuilder sqlBuilder = new StringBuilder("(");

            bool first = true;
            foreach(ICondition condition in conditionGroup.Conditions)
            {
                if (!first)
                {
                    switch(conditionGroup.ConditionGroupType)
                    {
                        case ConditionGroup.ConditionGroupTypes.And:
                            sqlBuilder.Append(" AND ");
                            break;
                        case ConditionGroup.ConditionGroupTypes.Or:
                            sqlBuilder.Append(" OR ");
                            break;
                        default:
                            throw new InvalidCastException("Could not determine Condition Group Type while writing");
                    }
                }

                sqlBuilder.Append(WriteCondition(condition, parameters));
            }

            sqlBuilder.Append(")");

            return sqlBuilder.ToString();
        }
    }
}
