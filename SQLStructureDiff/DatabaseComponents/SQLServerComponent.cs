using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLStructureDiff.DatabaseComponents
{
    /// <summary>
    /// SQL Server 部分
    /// </summary>
    public class SQLServerComponent : IDatabaseComponent
    {
        /// <summary>
        ///  需要排除的系统数据库名称
        /// </summary>
        private static string[] EXCLUDE_SYSTEM_DB_NAME = new string[4] { "master", "model", "msdb", "tempdb" };

        /// <summary>
        /// 查询实例中数据库 的 SQL 语句
        /// </summary>
        private const string QUERY_DBS = "SELECT name FROM Master..SysDatabases ORDER BY Name";

        /// <summary>
        /// 查询数据库中表的 SQL 语句
        /// </summary>
        private const string QUERY_DB_TABLES = "SELECT name FROM {0}.dbo.sysobjects WHERE xtype='U' OR xtype = 'V' ORDER BY xtype";

        /// <summary>
        /// 查询表中字段的 SQL 语句
        /// </summary>
        private const string QUERY_DB_TABLES_COLUMNS = "SELECT TABLE_NAME, COLUMN_NAME FROM {0}.INFORMATION_SCHEMA.COLUMNS";

        public List<DataBase> GetDataBasesInfos(string connStr)
        {
            List<DataBase> dataBasesInfos = new List<DataBase>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    using (SqlCommand queryDBcmd = new SqlCommand(QUERY_DBS, conn))
                    using (SqlDataReader reader = queryDBcmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string dbName = reader.GetString(0);
                            if (EXCLUDE_SYSTEM_DB_NAME.Contains(dbName))
                            {
                                continue;
                            }
                            else
                            {
                                dataBasesInfos.Add(new DataBase() { DatabaseName = dbName });
                            }
                        }
                    }

                    foreach(var databaseInfo in dataBasesInfos)
                    {
                        List<ColumnInDB> columns = new List<ColumnInDB>();
                        using (SqlCommand queryColumnCmd = new SqlCommand(string.Format(QUERY_DB_TABLES_COLUMNS, databaseInfo.DatabaseName), conn))
                        using (SqlDataReader reader = queryColumnCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(new ColumnInDB(null, reader.GetString(0), reader.GetString(1)));
                            }
                        }

                        List<Table> thisDBTableList = columns.GroupBy(a => a.TableName).Select(a => new Table
                        {
                            TableName = a.Key,
                            Columns = a.Select(column => new Column { ColumnName = column.ColumnName })
                                        .OrderBy(column => column.ColumnName).ToList()
                        }).OrderBy(a => a.TableName).ToList();

                        databaseInfo.Tables = thisDBTableList;
                    }

                    dataBasesInfos = dataBasesInfos.OrderBy(a => a.DatabaseName).ToList();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMsg(ex.ToString());
            }            

            return dataBasesInfos;
        }

        /// <summary>
        /// 列信息（从 SQLServer 系统表中获取）
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
