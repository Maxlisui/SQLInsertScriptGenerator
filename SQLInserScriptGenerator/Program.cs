using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SQLInserScriptGenerator
{
    class Program
    {
        private static void Main()
        {
            try
            {
                Console.Write("Connectionstring: ");
                var connectionString = Console.ReadLine();

                var columns = new List<SQLColumn>();
                var rows = new List<SQLRow>();
                var tableName = "";
                using (var connection = new SqlConnection(connectionString))
                {
                    Console.Write("Table name: ");
                    tableName = Console.ReadLine();

                    connection.Open();

                    using (var command = new SqlCommand("", connection))
                    {
                        command.CommandText = "SELECT * FROM " + connection.Database +
                                              ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(new SQLColumn()
                                {
                                    DataType = reader.GetString(7),
                                    Name = reader.GetString(3),
                                    Ordinal = reader.GetInt32(4) - 1
                                });
                            }
                        }
                    }

                    Console.Write("WHERE clause (without 'WHERE': ");
                    var whereClause = Console.ReadLine();

                    using (var command = new SqlCommand("", connection))
                    {
                        command.CommandText = "SELECT * FROM " + tableName + " WHERE " + whereClause;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new SQLRow()
                                {
                                    SqlCells = new List<SQLCell>()
                                };

                                foreach (var column in columns)
                                {
                                    row.SqlCells.Add(new SQLCell()
                                    {
                                        Value = reader.GetValue(column.Ordinal),
                                        SqlColumn = column
                                    });
                                }

                                rows.Add(row);
                            }
                        }
                    }
                }

                Console.Write("Unique Column Index: ");
                var uniqueColumnIndex = int.Parse(Console.ReadLine());

                var uniqueColumn = columns.FirstOrDefault(x => x.Ordinal == uniqueColumnIndex);

                if (uniqueColumn == null)
                {
                    throw new Exception("Column does not exist");
                }

                var output = "--==============GENERATED SCRIPT AT " + DateTime.Now + "==============\n\n";
                foreach (var row in rows)
                {
                    output += "IF NOT EXISTS(SELECT * FROM " + tableName + " WHERE " + uniqueColumn.Name + " = " +
                              row.SqlCells.FirstOrDefault(x => x.SqlColumn.Ordinal == uniqueColumn.Ordinal)
                                  ?.ValueAsString + ")\nBEGIN\n";

                    output += "INSERT INTO " + tableName + "(" +
                              string.Join(",", row.SqlCells.Select(x => x.SqlColumn.Name)) + ") VALUES\n(" +
                              string.Join(",", row.SqlCells.Select(x => x.ValueAsString)) + ")\n";

                    output += "END\n\n";

                }

                output += "\n\n\n";

                File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\GeneratedScript.sql", output);

                Console.WriteLine("Finished!!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }
    }
}
