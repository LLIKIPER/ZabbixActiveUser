using Cassia;
using IniFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Ysq.Zabbix;

namespace ZabbixActiveUserService
{
    public partial class Service : ServiceBase
    {

        ServiceThread thr;
        public Service()
        {
            InitializeComponent();
            Settings.Init();
        }

        protected override void OnStart(string[] args)
        {
            Settings.Init();
            Settings.logger.Info("Попытка запуска службы");
            bool iniError = false;
            IniFile INI = new IniFile(Settings.WorkDirectory + "\\config.ini");


            if (!INI.KeyExists("ZabbixServer", "General")) { INI.Write("General", "ZabbixServer", "192.168.0.1"); iniError = true; }
            if (!INI.KeyExists("ZabbixPort", "General")) { INI.Write("General", "ZabbixPort", "10051"); iniError = true; }
            if (!INI.KeyExists("ZabbixNodename", "General")) { INI.Write("General", "ZabbixNodename", "all"); iniError = true; }
            if (!INI.KeyExists("ZabbixParam", "General")) { INI.Write("General", "ZabbixParam", "workcomp"); iniError = true; }
            if (!INI.KeyExists("Delay", "General")) { INI.Write("General", "Delay", "300"); iniError = true; }
            if (!INI.KeyExists("IdleTime", "General")) { INI.Write("General", "IdleTime", "300"); iniError = true; }
            if (!INI.KeyExists("SendSummary", "General")) { INI.Write("General", "SendSummary", "1"); iniError = true; }
            if (!INI.KeyExists("SendActive", "General")) { INI.Write("General", "SendActive", "1"); iniError = true; }
            if (!INI.KeyExists("SendIdle", "General")) { INI.Write("General", "SendIdle", "1"); iniError = true; }
            if (!INI.KeyExists("SendOffline", "General")) { INI.Write("General", "SendOffline", "1"); iniError = true; }
            if (!INI.KeyExists("IgnoreUsername", "General")) { INI.Write("General", "IgnoreUsername", "HOSTNAME/USERNAME1,HOSTNAME/USERNAME2"); iniError = true; }

            if (iniError)
            {
                Settings.logger.Info("Отсутствутю обязательные параметры в конфиг файле(пример записаны в конфиг)");
                Environment.Exit(0);
            }
            else
            {
                Settings.ZabbixServer = INI.ReadINI("General", "ZabbixServer");
                Settings.ZabbixPort = Convert.ToInt32(INI.ReadINI("General", "ZabbixPort"));
                Settings.ZabbixNodename = INI.ReadINI("General", "ZabbixNodename");
                Settings.ZabbixParam = INI.ReadINI("General", "ZabbixParam");
                Settings.Delay = 1000 * Convert.ToInt32(INI.ReadINI("General", "Delay"));
                Settings.IdleTime = Convert.ToInt32(INI.ReadINI("General", "IdleTime"));

                Settings.SendSummary = INI.ReadINI("General", "SendSummary") == "1" ? true : false;
                Settings.SendActive = INI.ReadINI("General", "SendActive") == "1" ? true : false;
                Settings.SendIdle = INI.ReadINI("General", "SendIdle") == "1" ? true : false;
                Settings.SendOffline = INI.ReadINI("General", "SendOffline") == "1" ? true : false;
                Settings.IgnoreUsername = INI.ReadINI("General", "IgnoreUsername").Split(',').ToList();
            }

            #region Печать всех пользователей в лог файл
            Settings.logger.Info("*********************************");
            Settings.logger.Info("Список всех пользователей:");
            using (ITerminalServer server = Settings.manager.GetLocalServer())
            {
                server.Open();
                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    if (session.UserAccount == null)
                    {
                        continue;
                    }
                    Settings.logger.Info("  {0}:", session.UserAccount);
                }
            }
            Settings.logger.Info("*********************************");
            Settings.logger.Info("");
            #endregion

            #region Чтение списка отправляемх пользователей
            int UserCount = 0;
            while (true)
            {
                UserCount++;

                String _username = INI.ReadINI("User" + UserCount, "Username");
                String _zabbix_username = INI.ReadINI("User" + UserCount, "ZabbixUsername");

                if (_username.Trim().Length == 0 || _zabbix_username.Trim().Length == 0)
                {
                    UserCount--;
                    break;
                }
                Settings.list.Add(new ZabbixUser(_username, _zabbix_username));
            }

            if (UserCount == 0)
            {
                Settings.logger.Info("Отсутствутю обязательное перечисление пользователей в конфиг файле(пример записан в конфиг)");
                INI.Write("User1", "Username", "home-pc\\myusername");
                INI.Write("User1", "ZabbixUsername", "zab-user1");
                Environment.Exit(0);
            }

            Settings.logger.Info("*********************************");
            Settings.logger.Info("Будут отправляться сведения по следующим пользователям:");
            foreach (ZabbixUser _user in Settings.list)
            {
                Settings.logger.Info("  {0}", _user.ToString());
            }
            Settings.logger.Info("*********************************");
            Settings.logger.Info("");
            #endregion

            thr = new ServiceThread();
            Settings.logger.Info("Служба успешно запущена");
        }

        protected override void OnStop()
        {
            Settings.logger.Info("Остановка службы");
            thr.Stop();
            Settings.logger.Info("Служба успешно остановлена");
        }
    }

    public class ServiceThread
    {
        Thread thr;
        bool ЗавершениеРаботы = false;

        public ServiceThread()
        {
            thr = new Thread(ZabbixSender);
            thr.IsBackground = true;
            thr.Start();
        }
        public void Stop()
        {
            ЗавершениеРаботы = true;
            thr.Join();
        }

        private void ZabbixSender()
        {
            List<User> userList = new List<User>();
            Sender sender = new Sender(Settings.ZabbixServer, Settings.ZabbixPort);
            while (!ЗавершениеРаботы)
            {
                bool errorExist = false;
                userList.Clear();
                #region Сбор состояний пользователей
                using (ITerminalServer server = Settings.manager.GetLocalServer())
                {
                    server.Open();
                    foreach (ITerminalServicesSession session in server.GetSessions())
                    {
                        if (session.UserAccount == null)
                            continue;

                        if(Settings.IgnoreUsername.Contains(session.UserAccount.ToString()))
                            continue;

                        int state = 0;
                        double idle = Math.Floor(session.IdleTime.TotalSeconds);

                        if (session.ConnectionState == Cassia.ConnectionState.Active || session.ConnectionState == Cassia.ConnectionState.Idle)
                        {
                            if (idle < Settings.IdleTime)
                                state = 2;
                            else
                                state = 1;
                        }

                        userList.Add(new User(session.UserAccount.ToString(), state));
                    }
                }
                #endregion

                foreach (User usr in userList)
                    Settings.logger.Debug("{0} = {1}", usr.Username, usr.Condition);



                Settings.logger.Debug("Отправка данных в zabbix");
                foreach (ZabbixUser zabbix_usr in Settings.list)
                {
                    User usr = userList.Find(x => x.Equals(zabbix_usr.Username));
                    if (usr == null)
                    {
                        Settings.logger.Debug("{1} = {2}", zabbix_usr.ZabbixUsername, 0);

                        string send = String.Format("user.work[{0},{1}]", zabbix_usr.ZabbixUsername, Settings.ZabbixParam);
                        try
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, "0");

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                        catch (Exception ex)
                        {
                            Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                            errorExist = true;
                        }
                    }
                    else
                    {
                        Settings.logger.Debug("{0}({1}) = {2}", usr.Username, zabbix_usr.ZabbixUsername, usr.Condition);

                        string send = String.Format("user.work[{0},{1}]", zabbix_usr.ZabbixUsername, Settings.ZabbixParam);
                        try
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, usr.Condition.ToString());

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                        catch (Exception ex)
                        {
                            Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                            errorExist = true;
                        }
                    }
                }

                #region подсчет количества
                int _CountSummary = userList.Count;
                int _CountActive = 0;
                int _CountIdle = 0;
                int _CountOffline = 0;

                foreach (User usr in userList)
                {
                    if (usr.Condition == 2) _CountActive++;
                    if (usr.Condition == 1) _CountIdle++;
                    if (usr.Condition == 0) _CountOffline++;
                }
                #endregion

                #region Отправка количества итого
                if (Settings.SendSummary)
                {
                    string send = String.Format("user.work[{0},{1}]", "Summary", Settings.ZabbixParam);
                    try
                    {
                        if (!errorExist)
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, _CountSummary.ToString());

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                        errorExist = true;
                    }
                }
                #endregion

                #region Отправка количества активных
                if (Settings.SendActive)
                {
                    string send = String.Format("user.work[{0},{1}]", "Active", Settings.ZabbixParam);
                    try
                    {
                        if (!errorExist)
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, _CountActive.ToString());

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                        errorExist = true;
                    }
                }
                #endregion

                #region Отправка количества отсутствующих
                if (Settings.SendIdle)
                {
                    string send = String.Format("user.work[{0},{1}]", "Idle", Settings.ZabbixParam);
                    try
                    {
                        if (!errorExist)
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, _CountIdle.ToString());

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                        errorExist = true;
                    }
                }
                #endregion

                #region Отправка количества отключенных
                if (Settings.SendOffline)
                {
                    string send = String.Format("user.work[{0},{1}]", "Offline", Settings.ZabbixParam);
                    try
                    {
                        if (!errorExist)
                        {
                            SenderResponse response = sender.Send(Settings.ZabbixNodename, send, _CountOffline.ToString());

                            Settings.logger.Debug(send);
                            Settings.logger.Debug(response.Response);
                            Settings.logger.Debug(response.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Settings.logger.Error(ex, "Ошибка отправки данных в zabbix:");
                        errorExist = true;
                    }
                }
                #endregion
                Settings.logger.Debug("Окончание отправки данных в zabbix");


                for (int i = 0; (i <= Settings.Delay && !ЗавершениеРаботы); i += 1000)
                    Thread.Sleep(1000);
            }
        }
    }

}
