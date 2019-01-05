using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZabbixActiveUserService;

namespace ZabbixActiveUserServiceTest
{
    class ServiceTest : Service
    {
        public void TestStart(string[] args)
        {
            OnStart(args);
        }

        public void TestStop()
        {
            OnStop();
        }

    }
}
