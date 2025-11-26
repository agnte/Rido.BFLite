using Rido.BFLite.Core.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rido.BFLite.Core
{
    public class ActivityException(string message, Exception ex, Activity activity) : Exception(message, ex)
    {
        public Activity Activity { get; } = activity;
    }
}
