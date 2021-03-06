using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;

namespace TapHopCP
{
    public partial class FrmThang : DevExpress.XtraEditors.XtraForm
    {
        public int Thang;
        public FrmThang()
        {
            InitializeComponent();
            seThang.Value = Convert.ToDecimal(Config.GetValue("KyKeToan"));
        }

        private bool KTraDuLieu()
        {
            Database db = Database.NewDataDatabase();
            if (db.GetValue(string.Format("select MTID from MTCPSX where Nam = {0} and Thang = {1}",
                Config.GetValue("NamLamViec"), seThang.Value)) != null)
            {
                XtraMessageBox.Show("Tháng này đã tập hợp chi phí, vui lòng kiểm tra lại dữ liệu",
                    Config.GetValue("PackageName").ToString());
                return false;
            }
            return true;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!KTraDuLieu())
                return;
            Thang = (int)seThang.Value;
            this.DialogResult = DialogResult.OK;
        }
    }
}