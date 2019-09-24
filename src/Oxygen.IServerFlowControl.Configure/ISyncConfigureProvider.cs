﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl.Configure
{
    public interface ISyncConfigureProvider
    {
        Task<ServiceConfigureInfo> GetConfigure(string key);
        Task SetConfigure(string key, ServiceConfigureInfo newConfigure);
        Task InitConfigure(string key, ServiceConfigureInfo newConfigure);
    }
}
