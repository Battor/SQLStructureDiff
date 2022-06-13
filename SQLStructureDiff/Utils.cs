using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SQLStructureDiff
{
    public class Utils
    {
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="msg"></param>
        public static void ShowMsg(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 序列化数据库对象
        /// </summary>
        /// <param name="dbs"></param>
        public static void SerializeObject(List<DataBase> dbs, string fileName = "db_data")
        {
            IFormatter formater = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            formater.Serialize(stream, dbs);
            stream.Close();
        }

        /// <summary>
        /// 反序列化数据库对象
        /// </summary>
        /// <returns></returns>
        public static List<DataBase> DeSerializeObject(string fileName = "db_data")
        {
            IFormatter formater = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            return formater.Deserialize(stream) as List<DataBase>;
        }

        /// <summary>
        /// 比较不同的数据库结构对象（以对象引用的形式返回）
        /// </summary>
        /// <param name="baseDb"></param>
        /// <param name="targetDb"></param>
        /// <param name="redundant"></param>
        /// <param name="missing"></param>
        public static void DiffDataBaseObject(List<DataBase> baseDb, List<DataBase> targetDb,
            ref List<DataBase> redundant, ref List<DataBase> missing)
        {
            List<string> redundantDbs = new List<string>();
            List<string> missingDbs = new List<string>();
            List<string> intersectDbs = new List<string>();

            DiffListString(baseDb.Select(a => a.DatabaseName), targetDb.Select(a => a.DatabaseName),
                                    ref redundantDbs, ref missingDbs, ref intersectDbs);
            foreach (string db in redundantDbs)
            {
                redundant.Add(new DataBase { DatabaseName = db, Tables = null });
            }
            foreach (string db in missingDbs)
            {
                missing.Add(new DataBase { DatabaseName = db, Tables = null });
            }

            foreach (string db in intersectDbs)
            {
                DataBase redundantTmp = new DataBase { DatabaseName = db, Tables = new List<Table>() };
                DataBase missingTmp = new DataBase { DatabaseName = db, Tables = new List<Table>() };

                DataBase baseDbObj = baseDb.Where(a => a.DatabaseName == db).FirstOrDefault();
                DataBase targetDbObj = targetDb.Where(a => a.DatabaseName == db).FirstOrDefault();

                List<string> redundantTables = new List<string>();
                List<string> missingTables = new List<string>();
                List<string> intersectTables = new List<string>();

                DiffListString(baseDbObj.Tables.Select(a => a.TableName), targetDbObj.Tables.Select(a => a.TableName),
                                ref redundantTables, ref missingTables, ref intersectTables);

                foreach (string table in redundantTables)
                {
                    redundantTmp.Tables.Add(new Table { TableName = table, Columns = null });
                }
                foreach (string table in missingTables)
                {
                    missingTmp.Tables.Add(new Table { TableName = table, Columns = null });
                }


                foreach (string table in intersectTables)
                {
                    Table redundantTableTmp = new Table { TableName = table, Columns = new List<Column>() };
                    Table missingTableTmp = new Table { TableName = table, Columns = new List<Column>() };

                    Table baseTableObj = baseDbObj.Tables.Where(a => a.TableName == table).FirstOrDefault();
                    Table targetTableObj = targetDbObj.Tables.Where(a => a.TableName == table).FirstOrDefault();

                    List<string> redundantColumns = new List<string>();
                    List<string> missingColumns = new List<string>();
                    List<string> intersectColumns = new List<string>();

                    DiffListString(baseTableObj.Columns.Select(a => a.ColumnName), targetTableObj.Columns.Select(a => a.ColumnName),
                                ref redundantColumns, ref missingColumns, ref intersectColumns);

                    foreach (string column in redundantColumns)
                    {
                        redundantTableTmp.Columns.Add(new Column { ColumnName = column });
                    }
                    foreach (string column in missingColumns)
                    {
                        missingTableTmp.Columns.Add(new Column { ColumnName = column });
                    }

                    if (redundantTableTmp.Columns.Count > 0)
                    {
                        redundantTmp.Tables.Add(redundantTableTmp);
                    }
                    if (missingTableTmp.Columns.Count > 0)
                    {
                        missingTmp.Tables.Add(missingTableTmp);
                    }
                }

                if (redundantTmp.Tables.Count > 0)
                {
                    redundant.Add(redundantTmp);
                }
                if (missingTmp.Tables.Count > 0)
                {
                    missing.Add(missingTmp);
                }
            }
        }

        /// <summary>
        /// 比较不同的字符串列表（以对象引用的形式返回）
        /// </summary>
        /// <param name="baseList"></param>
        /// <param name="targetList"></param>
        /// <param name="redundant"></param>
        /// <param name="missing"></param>
        /// <param name="intersect"></param>
        private static void DiffListString(IEnumerable<string> baseList, IEnumerable<string> targetList,
            ref List<string> redundant, ref List<string> missing, ref List<string> intersect)
        {
            redundant = targetList.Except(baseList).ToList();
            missing = baseList.Except(targetList).ToList();
            intersect = baseList.Intersect(targetList).ToList();
        }

        public static void ShowDatabaseDifferent(List<DataBase> redundants, List<DataBase> missings)
        {
            Utils.ShowMsg("相比较基础数据库实例多出的部分：");

            foreach (var redundant in redundants)
            {
                if (redundant.Tables == null || redundant.Tables.Count == 0)
                {
                    Utils.ShowMsg(string.Format("多出数据库：{0}", redundant.DatabaseName));
                }
                else
                {
                    foreach (var table in redundant.Tables)
                    {
                        if (table.Columns == null || table.Columns.Count == 0)
                        {
                            Utils.ShowMsg(string.Format("多出数据库 {0} 中的表 {1}", redundant.DatabaseName, table.TableName));
                        }
                        else
                        {
                            Utils.ShowMsg(string.Format("多出数据库 {0} 中表 {1} 的字段 {2}", redundant.DatabaseName, table.TableName, string.Join("，", table.Columns.Select(a => a.ColumnName))));
                        }
                    }
                }
            }

            Utils.ShowMsg("相比较基础数据库实例缺失的部分：");

            foreach (var missing in missings)
            {
                if (missing.Tables == null || missing.Tables.Count == 0)
                {
                    Utils.ShowMsg(string.Format("缺失数据库：{0}", missing.DatabaseName));
                }
                else
                {
                    foreach (var table in missing.Tables)
                    {
                        if (table.Columns == null || table.Columns.Count == 0)
                        {
                            Utils.ShowMsg(string.Format("缺失数据库 {0} 中的表 {1}", missing.DatabaseName, table.TableName));
                        }
                        else
                        {
                            Utils.ShowMsg(string.Format("缺失数据库 {0} 中表 {1} 的字段 {2}", missing.DatabaseName, table.TableName, string.Join("，", table.Columns.Select(a => a.ColumnName))));
                        }
                    }
                }
            }
        }
    }
}
