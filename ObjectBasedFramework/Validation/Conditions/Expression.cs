using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Validation.Conditions
{
    public abstract class Expression
    {
        public virtual List<string> GetAdditionalFieldsForDataObject() { return new List<string>(); }
        public abstract object Evaluate(Dictionary<string, object> parameters);
    }
}
