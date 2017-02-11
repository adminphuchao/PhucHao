using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using System.Data;
using CDTLib;
using DevExpress.XtraGrid;
using System.Drawing;

namespace DieuChinhNhapKho
{
    public class DieuChinhNhapKho : ICReport
    {
        private DataCustomReport _data;
        private InfoCustomReport _info = new InfoCustomReport(IDataType.Report);
        private Database db = Database.NewDataDatabase();
        GridView gvMain;

        #region ICReport Members

        public DataCustomReport Data
        {
            set { _data = value; }

        }

        public InfoCustomReport Info
        {
            get { return _info; }
        }

        #endregion

        public void Execute()
        {
            gvMain = (_data.FrmMain.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            gvMain.DataSourceChanged += new EventHandler(gvMain_DataSourceChanged);
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            SimpleButton btnXL = _data.FrmMain.Controls.Find("btnXuLy", true)[0] as SimpleButton;
            btnXL.Text = "F4-Điều chỉnh kho";
            btnXL.Click += new EventHandler(btnXL_Click);
        }

        void gvMain_DataSourceChanged(object sender, EventArgs e)
        {
            gvMain.Columns["AdjustQTY"].DisplayFormat.FormatString = "###,##0";
            Font f = new Font(gvMain.Columns["AdjustQTY"].AppearanceCell.Font, FontStyle.Bold);
            gvMain.Columns["AdjustQTY"].AppearanceCell.Font = f;
            gvMain.Columns["TotalQTY"].AppearanceCell.Font = f;
            gvMain.Columns["TotalQTY"].AppearanceCell.BackColor = Color.Red;
        }

        void gvMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "AdjustQTY")
                gvMain.SetFocusedRowCellValue(gvMain.Columns["TotalQTY"],
                    Convert.ToDecimal(e.Value) + Convert.ToDecimal(gvMain.GetFocusedRowCellValue("FinishJobQTY")));
        }

        void btnXL_Click(object sender, EventArgs e)
        {
            //kiem tra quyen su dung
            bool admin = Convert.ToBoolean(Config.GetValue("Admin"));
            bool hasRight = admin ||
                (_data.DrReport.Table.Columns.Contains("sInsert") && Convert.ToBoolean(_data.DrReport["sInsert"])) ||
                (_data.DrReport.Table.Columns.Contains("sUpdate") && Convert.ToBoolean(_data.DrReport["sUpdate"])) ||
                (_data.DrReport.Table.Columns.Contains("sDelete") && Convert.ToBoolean(_data.DrReport["sDelete"]));
            if (!hasRight)
                XtraMessageBox.Show("Người dùng không có quyền thực hiện chức năng này\nVui lòng liên hệ quản trị hệ thống!",
                    Config.GetValue("PackageName").ToString());

            DataView dv = gvMain.DataSource as DataView;
            dv.Table.AcceptChanges();
            dv.RowFilter = "[Chon] = 1";
            if (dv.Count == 0)
            {
                dv.RowFilter = "";
                XtraMessageBox.Show("Vui lòng đánh dấu chọn đơn hàng cần điều chỉnh kho", Config.GetValue("PackageName").ToString());
                return;
            }

            bool rs = true;
            foreach (DataRowView drv in dv)
            {
                rs = db.UpdateDatabyStore("AdjustPOResult", new string[] { "ID", "AdjustQTY", "TotalQTY" },
                    new object[] { drv["ID"], drv["AdjustQTY"], drv["TotalQTY"] });
                if (rs)
                    drv.Row.Delete();
                else
                    break;
            }

            dv.Table.AcceptChanges();

            dv.RowFilter = "";//Bỏ fillter

            if (rs)
                XtraMessageBox.Show("Cập nhật dữ liệu thành công", Config.GetValue("PackageName").ToString());
        }
    }
}
