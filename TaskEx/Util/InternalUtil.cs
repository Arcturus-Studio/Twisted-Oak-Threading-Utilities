using System;
using System.Linq;

namespace TwistedOak.Element.Util {
    public static class InternalUtil {
        ///<summary>Returns the result of flattening and, if it contains a single exceptions unwrapping the given aggregate exception.</summary>
        public static Exception Collapse(this AggregateException ex) {
            if (ex == null) throw new ArgumentNullException("ex");
            var flattened = ex.Flatten();
            var exs = flattened.InnerExceptions.Distinct().ToArray();
            if (exs.Length == 1) return exs.Single();
            return flattened;
        }
    }
}
