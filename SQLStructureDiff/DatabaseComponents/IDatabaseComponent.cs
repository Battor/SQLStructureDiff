using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLStructureDiff.DatabaseComponents
{
    public interface IDatabaseComponent
    {
        List<DataBase> GetDataBasesInfos(string connStr);
    }
}
