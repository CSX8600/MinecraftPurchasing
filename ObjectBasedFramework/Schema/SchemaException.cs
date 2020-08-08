using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Schema
{
    public class SchemaException : Exception
    {
        internal SchemaException(string message) : base(message) { }
    }
}
