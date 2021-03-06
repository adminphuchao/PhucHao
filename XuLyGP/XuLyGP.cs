using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors;
using System.Data;
using CDTLib;
using DevExpress.XtraGrid.Columns;
using System.Drawing;

namespace XuLyGP
{
    public class XuLyGP : ICReport
    {
        private Database db = Database.NewDataDatabase();
        private DataCustomReport _data;
        private InfoCustomReport _info = new InfoCustomReport(IDataType.Report);
        GridView gvMain;
        private bool blAuto = false;
        #region ICControl Members

        public void Execute()
        {
            gvMain = (_data.FrmMain.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            //gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            //gvMain.DataSourceChanged += new EventHandler(gvMain_DataSourceChanged);             //xu ly dinh dang trong su kien nay
            SimpleButton btnXL = _data.FrmMain.Controls.Find("btnXuLy", true)[0] as SimpleButton;
            btnXL.Text = "F4 - Xử lý GP";
            btnXL.Click += new EventHandler(btnXL_Click);
            gvMain.RowStyle += GvMain_RowStyle;

        }

        private void GvMain_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                double sln = Convert.ToDouble(View.GetRowCellDisplayText(e.RowHandle, View.Columns["Số lượng nhập"]).Trim());
                double slpo = Convert.ToDouble(View.GetRowCellDisplayText(e.RowHandle, View.Columns["SLPO"]).Trim());

                if (sln > slpo + 5)
                {
                    //green
                    e.Appearance.BackColor = Color.LightGreen;
                    e.Appearance.BackColor2 = Color.LightGreen;

                }
                else if (sln > 0 && sln < slpo - 5)
                {
                    //red
                    e.Appearance.BackColor = Color.LightSalmon;
                    e.Appearance.BackColor2 = Color.LightSalmon;
                }
            }
        }

        void btnXL_Click(object sender, EventArgs e)
        {
            //kiem tra quyen su dung
            bool admin = Convert.ToBoolean(Config.GetValue("Admin"));
            bool hasRight = admin ||
                (_data.DrReport.Table.Columns.Contains("sInsert") && Convert.ToBoolean(_data.DrReport["sInsert"])) ||
                (_data.DrReport.Table.Columns.Contains("sUpdate") && Convert.ToBoolean(_data.DrReport["sUpdate"])) ||
                (_data.DrReport.Table.Columns.Contains("sDelete") && Convert.ToBoolean(_data.DrReport["sDelete"]));
            if (hasRight)
                UpdateGP();
            else
                XtraMessageBox.Show("Người dùng không có quyền thực hiện chức năng này\nVui lòng liên hệ quản trị hệ thống!",
                    Config.GetValue("PackageName").ToString());
        }

        private void UpdateGP()
        {
            DataView dv = gvMain.DataSource as DataView;
            dv.Table.AcceptChanges();
            dv.RowFilter = "[Chon] = 1 and [Tồn cuối] <> 0";
            if (dv.Count == 0)
            {
                dv.RowFilter = "";
                XtraMessageBox.Show("Vui lòng đánh dấu chọn mặt hàng xử lý phế", Config.GetValue("PackageName").ToString());
                return;
            }
             
            string sql = @" update	DT32
                            set		SLTonCuoi = SLTonCuoi + {0}, isGP = 1, GhiChuID = 6, GhiChuGP = N'Khác'
                            where DT32ID = 
                            (select top 1 dt32id from dt32 d inner join mt32 m on d.mt32id = m.mt32id
                            where d.dtdhid = '{1}' and m.ngayct <= '{2}' and d.TenHang = N'{3}'
                            order by m.ngayct desc)

                            update	blvt 
                            set		SLXGP = SLXGP + {0}, PsCoGP = DonGia * (SLXGP + {0}), GhiChuGP = 6
                            where MTIDDT = 
                            (select top 1 dt32id from dt32 d inner join mt32 m on d.mt32id = m.mt32id
                            where d.dtdhid = '{1}' and m.ngayct <= '{2}' and d.TenHang = N'{3}'
                            order by m.ngayct desc)";
            bool rs = true;
            foreach (DataRowView drv in dv)
            {
                rs = db.UpdateByNonQuery(string.Format(sql, drv["Tồn cuối"], drv["DTDHID"], Config.GetValue("@NgayCT2"), drv["TenHH"]));
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

        public DataCustomReport Data
        {
            set { _data = value; }
        }

        public InfoCustomReport Info
        {
            get { return _info; }
        }

        #endregion

    }
}
