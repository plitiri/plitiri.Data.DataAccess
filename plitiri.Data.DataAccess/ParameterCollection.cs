using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plitiri.Data.DataAccess;
public class ParameterCollection : Dictionary<string, object>
{
    /// <summary>
    /// Same as Dictionary&lt;string, object&gt;
    /// </summary>
    public ParameterCollection() : base() { }
}

