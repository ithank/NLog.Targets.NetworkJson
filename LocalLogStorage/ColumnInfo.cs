using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalLogStorage
{
    public class ColumnInfo
    {
        public ColumnInfo(string columnName, string columnDDL, DbType parameterType, int columnIndex)
        {
            ColumnName = columnName;
            ColumnDDL = columnDDL;
            Index = columnIndex;
            ParameterName = $"@{columnName}";
            ParameterType = parameterType;
        }

        public string ColumnName { get; }
        public string ColumnDDL { get; }
        public int Index { get; }
        public string ParameterName { get; }
        public DbType ParameterType { get; }

        public SQLiteParameter GetParamterForColumn()
        {
            return new SQLiteParameter(ParameterName, ParameterType, ColumnName);
        }
    }
}
