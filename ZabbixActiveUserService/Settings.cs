using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Cassia;

namespace ZabbixActiveUserService
{
    public static class Settings
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        public static string ZabbixServer;
        public static int ZabbixPort;
        public static string ZabbixNodename;
        public static string ZabbixParam;
        public static int Delay;
        public static int IdleTime;
        public static bool SendSummary;
        public static bool SendActive;
        public static bool SendIdle;
        public static bool SendOffline;
        public static List<string> IgnoreUsername;

        public static List<ZabbixUser> list;
        public static ITerminalServicesManager manager;
        public static string WorkDirectory;

        public static void Init()
        {
            logger = LogManager.GetCurrentClassLogger();
            ZabbixServer = "127.0.0.1";
            ZabbixPort = 10051;
            ZabbixNodename = "all";
            ZabbixParam = "workcomp";
            Delay = 300;
            IdleTime = 300;
            SendSummary = true;
            SendActive = true;
            SendIdle = true;
            SendOffline = true;
            IgnoreUsername = new List<string>();
            list = new List<ZabbixUser>();
            manager = new TerminalServicesManager();
            WorkDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        }
    }
}
