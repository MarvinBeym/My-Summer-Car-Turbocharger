using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatsumaTurboCharger
{
    public static class Extension
    {
        public static string ToStringOrEmpty(this Object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}
