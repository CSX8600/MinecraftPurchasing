using ClussPro.Base.Data.Operand;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.SqlServerProvider.ScriptWriters
{
    internal static class OperandWriter
    {
        public static string WriteOperand(IOperand operand, SqlParameterCollection parameters)
        {
            if (operand is Field)
            {
                return WriteField((Field)operand);
            }

            if (operand is Literal)
            {
                return WriteLiteral((Literal)operand, parameters);
            }

            if (operand is CSV)
            {

            }

            throw new InvalidCastException("Could not determine IOperand type for writing");
        }

        private static string WriteField(Field field)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(field.TableAlias))
            {
                sql += $"[{field.TableAlias}].";
            }

            sql += $"[{field.FieldName}]";

            return sql;
        }

        private static string WriteLiteral(Literal literal, SqlParameterCollection parameters)
        {
            int paramCount = parameters.Count;
            parameters.AddWithValue(paramCount.ToString(), literal.Value);
            return $"@{paramCount}";
        }

        public static string WriteCSV(CSV csv, SqlParameterCollection parameters)
        {
            StringBuilder builder = new StringBuilder("(");
            bool first = true;
            foreach(object value in csv.Values)
            {
                if (!first)
                {
                    builder.Append(",");
                }
                first = false;

                int paramCount = parameters.Count;
                parameters.AddWithValue(paramCount.ToString(), value);
                builder.Append($"@{paramCount}");
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}
