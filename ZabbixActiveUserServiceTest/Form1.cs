using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZabbixActiveUserServiceTest
{
    public partial class Form1 : Form
    {
        ServiceTest service;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            service = new ServiceTest();

            service.TestStart(new string[0]);

            button1.Enabled = false;
            button2.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            service.TestStop();
            service.Dispose();

            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            button1_Click(sender, null);
        }
    }
}
