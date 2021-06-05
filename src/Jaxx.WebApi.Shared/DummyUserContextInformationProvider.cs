using Jaxx.WebApi.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.WebApi.Shared
{
    public class DummyUserContextInformationProvider : IUserContextInformationProvider
    {
        private readonly string _userName;
        private readonly string _viewGroup;

        public DummyUserContextInformationProvider(string userName, string viewGroup)
        {
            _userName = userName;
            _viewGroup = viewGroup;
        }
        public string UserName
        {
            get { return _userName; }
        }

        public string GetViewGroup()
        {
            return _viewGroup;
        }
    }
}
