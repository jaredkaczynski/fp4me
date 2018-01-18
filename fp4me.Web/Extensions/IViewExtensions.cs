using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ViewEngines
{
    public static class IViewExtensions
    {
        public static string PathWithoutExtension(this IView view)
        {
            return view.Path.Substring(0, view.Path.LastIndexOf('.'));
        }
    }
}
