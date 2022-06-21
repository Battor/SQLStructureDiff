# SQLStructureDiff

#### Description
Compare two relational databases.

Used to use *Entity Framework*'s *DB First* mode to develop, I have the problems with the diffenrences of *develop database* and *production database* occasionally, So I write the tool.

By query system tables in database, I get the metadata of *databases*, *tables*, *columns* and *indexes(not implemented yet)* and save them to a file. Read two files and use *linq* to compare can finally get the differences.

#### Usage

##### 1. Generate database structure
```
> .\SQLStructureDiff generate <DBTYPE> <CONN_STR>
```

**DBTYPE** can only be *"MySQL"* or *"SQLServer"*, **CONN_STR** is *database connection string*.This operation will generate file *db_data* in current directory.

##### 2. Compare
```
> .\SQLStructureDiff diff <DBTYPE> <BASE_FILE_NAME> <TARGET_FILE_NAME>
```

This operation will print differences。

##### 3. Show Help
```
> .\SQLStructureDiff /?
```

#### Thanks

Thanks for Xie Mengtian's creation of icon, which makes the program more *lively*.

