namespace TextIntegrationService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceZabbix = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceZabbix
            // 
            this.serviceZabbix.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceZabbix.Password = null;
            this.serviceZabbix.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Отправка в Zabbix активности пользователей";
            this.serviceInstaller1.DisplayName = "Zabbix active user";
            this.serviceInstaller1.ServiceName = "ZabbixActiveUser";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceZabbix,
            this.serviceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceZabbix;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}