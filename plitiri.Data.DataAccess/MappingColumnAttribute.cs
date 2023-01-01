using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plitiri.Data.DataAccess;

[System.AttributeUsage(AttributeTargets.Property)]
public class MappingColumnAttribute : System.Attribute
{
    public string? Name
    {
        get; set;
    }

    public MappingColumnAttribute(string? name)
    {
        this.Name = name;
    }
}
