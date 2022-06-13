using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLStructureDiff.DatabaseComponents
{
    /// <summary>
    /// MySQL 部分
    /// </summary>
    public class MySQLComponent : IDatabaseComponent
    {
        private const string SCHEMA_QUERY_SQL = "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA";
        private const string TABLE_QUERY_SQL = "SELECT TABLE_SCHEMA, TABLE_NAME FROM information_schema.TABLES";
        private const string COLUMN_QUERY_SQL = "SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME FROM information_schema.COLUMNS";
        private const string INDEX_QUERY_SQL = "SELECT * FROM information_schema.STATISTICS";

        private static string[] schemasToExclude = new string[] { "information_schema", "performance_schema", "sys", "mysql" };

        string schemaQueryWhereStr = $"WHERE SCHEMA_NAME NOT IN ({ string.Join(", ", schemasToExclude.Select(a => $"'{a}'"))  })";
        string otherQueryWhereStr = $"WHERE TABLE_SCHEMA NOT IN ({ string.Join(", ", schemasToExclude.Select(a => $"'{a}'"))  })";

        /// <summary>
        /// 获取数据库数据
        /// </summary>
        /// <returns></returns>
        public List<DataBase> GetDataBasesInfos(string connStr)
        {
            List<SchemaInDB> schemas = new List<SchemaInDB>();
            List<TableInDB> tables = new List<TableInDB>();
            List<ColumnInDB> columns = new List<ColumnInDB>();

            #region 读取数据库的数据

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    using (MySqlCommand schemaQueryCmd = new MySqlCommand($"{ SCHEMA_QUERY_SQL } { schemaQueryWhereStr }", conn))
                    using (MySqlDataReader reader = schemaQueryCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schemas.Add(new SchemaInDB(reader.GetString("SCHEMA_NAME")));
                        }
                    }

                    using (MySqlCommand tableQueryCmd = new MySqlCommand($"{ TABLE_QUERY_SQL } { otherQueryWhereStr }", conn))
                    using (MySqlDataReader reader = tableQueryCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(new TableInDB(reader.GetString("TABLE_SCHEMA"), reader.GetString("TABLE_NAME")));
                        }
                    }

                    using (MySqlCommand columnQueryCmd = new MySqlCommand($"{ COLUMN_QUERY_SQL } { otherQueryWhereStr }", conn))
                    using (MySqlDataReader reader = columnQueryCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(new ColumnInDB(reader.GetString("TABLE_SCHEMA"), reader.GetString("TABLE_NAME"), reader.GetString("COLUMN_NAME")));
                        }
                    }
                }

                schemas = schemas.OrderBy(a => a.SchemaName).ToList();
                tables = tables.OrderBy(a => a.SchemaName).ThenBy(a => a.TableName).ToList();
                columns = columns.OrderBy(a => a.SchemaName).ThenBy(a => a.TableName).ThenBy(a => a.ColumnName).ToList();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            #endregion

            List<Table> groupedTable = columns.GroupBy(column => column.TableName).Select(g => new Table
            {
                TableName = g.Key,
                Columns = g.Select(a => new Column { ColumnName = a.ColumnName }).ToList()
            }).ToList();

            List<DataBase> dataBaseResult = (from tableInDB in tables
                                             join table in groupedTable on tableInDB.TableName equals table.TableName
                                             group new { tableInDB, table } by tableInDB.SchemaName into result
                                             select new DataBase
                                             {
                                                 DatabaseName = result.Key,
                                                 Tables = result.Select(a => a.table).ToList()
                                             }).ToList();

            return dataBaseResult;
        }

        /// <summary>
        /// 数据库信息（从 MySQL 系统表中获取）
        /// </summary>
        private class SchemaInDB
        {
            /// <summary>
            /// 数据库名称
            /// </summary>
            public string SchemaName { get; set; }

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="schemaName"></param>
            public SchemaInDB(string schemaName)
            {
                this.SchemaName = schemaName;
            }
        }

        /// <summary>
        /// 表信息（从 MySQL 系统表中获取）
        /// </summary>
        private class TableInDB
        {
            /// <summary>
            /// 数据库名
            /// </summary>
            public string SchemaName { get; set; }
            /// <summary>
            /// 表名
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="schemaName"></param>
            /// <param name="tableName"></param>
            public TableInDB(string schemaName, string tableName)
            {
                this.SchemaName = schemaName;
                this.TableName = tableName;
            }
        }

        /// <summary>
        /// 列信息（从 MySQL 系统表中获取）
        /// </summary>
        private class ColumnInDB
        {
            /// <summary>
            /// 数据库名
            /// </summary>
            public string SchemaName { get; set; }
            /// <summary>
            /// 表名
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// 列名
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="schemaName"></param>
            /// <param name="tableName"></param>
            /// <param name="columnName"></param>
            public ColumnInDB(string schemaName, string tableName, string columnName)
            {
                SchemaName = schemaName;
                TableName = tableName;
                ColumnName = columnName;
            }
        }
    }
}
