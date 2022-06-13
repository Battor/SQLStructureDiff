using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLStructureDiff
{
    /// <summary>
    /// 列
    /// </summary>
    [Serializable]
    public class Column
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
    }

    /// <summary>
    /// 表
    /// </summary>
    [Serializable]
    public class Table
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表所包含的列
        /// </summary>
        public List<Column> Columns { get; set; }
    }

    /// <summary>
    /// 数据库
    /// </summary>
    [Serializable]
    public class DataBase
    {
        /// <summary>
        /// 数据库名
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// 数据库所包含的表
        /// </summary>
        public List<Table> Tables { get; set; }
    }
}
