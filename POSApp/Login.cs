﻿using CDTDatabase;
using System;
using System.Data;
using System.Windows.Forms;

namespace POSApp
{
    public partial class Login : Form
    {
        public DataRow drUser;
        Database db = Database.NewDataDatabase();

        public Login()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sql = "SELECT * FROM DMNhanVien WHERE Ma = '{0}' AND pin = '{1}'";
            DataTable dt = db.GetDataTable(string.Format(sql, textBox1.Text, textBox2.Text));
            if (dt.Rows.Count > 0)
            {
                drUser = dt.Rows[0];
                this.DialogResult = DialogResult.OK;
                this.Close();
            } else
            {
                MessageBox.Show("Thông tin đăng nhập không đúng. Vui lòng kiểm tra lại.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
