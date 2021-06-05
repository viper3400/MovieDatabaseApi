using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.WebApi.Shared
{
    public interface IUserContextInformationProvider
    {
        // for information about global query filters see
        // - http://gunnarpeipman.com/net/ef-core-global-query-filters/
        // - https://docs.microsoft.com/en-us/ef/core/querying/filters

        string GetViewGroup();

        string UserName { get; }
    }
}
