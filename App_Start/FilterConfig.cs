﻿using System.Web;
using System.Web.Mvc;

namespace CLRIQTR_EMP
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
