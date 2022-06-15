using SQLStructureDiff.DatabaseComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLStructureDiff
{
    class Program
    {
        /// <summary>
        /// 支持的数据库类型
        /// </summary>
        private static string[] SUPPORT_DBTYPEs = new string[] { "mysql", "sqlserver" };

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            // 仅支持三种命令：
            // SQLStructureDiff generate <DBTYPE> <CONN_STR>
            // SQLStructureDiff diff <DBTYPE> <BASE_FILE_NAME> <TARGET_FILE_NAME>
            // SQLStructureDiff /?
            // DBTYPE 只能为 MySQL 或 SQLServer（SQLServer 还未实现）

            if (args.Length == 0)
            {
                Utils.ShowMsg("缺少参数！");
            }
            else if (args[0] == "generate" && !SUPPORT_DBTYPEs.Contains(args[1].ToLower()))
            {
                Utils.ShowMsg("数据库类型参数 DBTYPE 只能为 MySQL 或 SQLServer");
            }
            else if (args[0] == "generate" && args.Length == 3)
            {
                IDatabaseComponent dbComponent = null;

                if(args[1].ToLower() == "mysql")
                {
                    dbComponent = new MySQLComponent();
                }
                else if(args[1].ToLower() == "sqlserver")
                {
                    dbComponent = new SQLServerComponent();
                }
                else
                {
                    throw new Exception("数据库类型参数 DBTYPE 只能为 MySQL 或 SQLServer");    // 由于有判断因此不会触发
                }

                Utils.ShowMsg("正在生成数据库结构...");

                List<DataBase> dataBaseInfos = dbComponent.GetDataBasesInfos(args[2]);
                Utils.SerializeObject(dataBaseInfos);

                Utils.ShowMsg("数据库结构生成成功，请注意查看当前目录下的 db_data 文件！");
            }
            else if (args[0] == "diff" && args.Length == 3)
            {
                string baseFileName = args[1];
                string targetFileName = args[2];

                var tmp1 = Utils.DeSerializeObject(baseFileName);
                var tmp2 = Utils.DeSerializeObject(targetFileName);

                var rData = new List<DataBase>();
                var mData = new List<DataBase>();

                Utils.DiffDataBaseObject(tmp1, tmp2, ref rData, ref mData);
                Utils.ShowDatabaseDifferent(rData, mData);
            }
            else if (args[0] == "/?")
            {
                Utils.ShowMsg("仅支持三种命令：");
                Utils.ShowMsg("SQLServerStructureDiff generate <DBTYPE> <CONN_STR>");
                Utils.ShowMsg("SQLServerStructureDiff diff <BASE_FILE_NAME> <TARGET_FILE_NAME>");
                Utils.ShowMsg("SQLServerStructureDiff /?");
            }
            else
            {
                Utils.ShowMsg("参数不正确！");
            }            
        }

        /// <summary>
        /// 自定义解析以查找资源文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);

            var fileName = assemblyName.Name + ".dll";
            string res = executingAssembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(fileName));

            using (var stream = executingAssembly.GetManifestResourceStream(res))
            {
                if (stream == null) return null;
                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
