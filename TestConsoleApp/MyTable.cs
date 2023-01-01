using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plitiri.Data.DataAccess;

namespace TestConsoleApp;
public class MyTable
{
    public long? Id
    {
        get; set;
    }

    [MappingColumn("nAmE")]
    public string? Name2
    {
        get; set;
    }
}
