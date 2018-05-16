using System;
using System.Data;
using System.Globalization;

namespace SQLInserScriptGenerator
{
    public class SQLCell
    {
        public SQLColumn SqlColumn { get; set; }
        public object Value { get; set; }
        public string ValueAsString
        {
            get
            {
                if (SqlColumn == null)
                {
                    throw new NoNullAllowedException();
                }

                switch (Value)
                {
                    case null:
                        return "NULL";
                    case DBNull _:
                        return "NULL";
                    default:
                        if (SqlColumn.DataType.ToLower().Equals("uniqueidentifier") ||
                            SqlColumn.DataType.ToLower().Contains("char"))
                        {
                            return "'" + Value + "'";
                        }
                        if (SqlColumn.DataType.ToLower().Equals("date"))
                        {
                            //TODO: Return correct SQL date string
                            return ((DateTime) Value).ToString();
                        }
                        if (SqlColumn.DataType.ToLower().Equals("bit"))
                        {
                            return (bool) Value ? 1.ToString() : 0.ToString();
                        }
                        break;
                }

                return Value.ToString();
            }
        }
    }
}