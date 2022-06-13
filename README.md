# SQLStructureDiff

#### 介绍
比较两个关系型数据库的表结构差异。Compare two relational databases.

由于习惯使用 *Entity Framework* 的 *DB First* 模式进行开发，经常出现 *开发库* 和 *生产库* 的结构不一致的问题，因此编写小工具。

通过查询数据库实例的系统表，取得*数据库*、*表*、*列*和*索引（暂未实现）*的数据并保存为文件。读取两个数据库对应的文件，使用 *Linq* 进行差异比较并输出。


#### 使用

##### 1. 生成数据表结构数据
```
> .\SQLStructureDiff generate <DBTYPE> <CONN_STR>
```

其中 **DBTYPE** 只能为 *"MySQL"* 或 *"SQLServer"*，**CONN_STR** 为 *数据库连接串*。此操作会在当前目录生成 *db_data* 文件。

##### 2. 比较
```
> .\SQLStructureDiff diff <DBTYPE> <BASE_FILE_NAME> <TARGET_FILE_NAME>
```

此操作会打印差异。

##### 3. 查看帮助
```
> .\SQLStructureDiff /?
```

