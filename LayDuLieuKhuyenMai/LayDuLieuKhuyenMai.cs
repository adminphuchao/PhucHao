using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using FormFactory;
using System.Data;
using DevExpress.XtraGrid.Views.Grid;
using CDTLib;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraTab;
using CDTDatabase;

namespace LayDuLieuKhuyenMai
{
    public class LayDuLieuKhuyenMai : ICControl
    {
        XtraTabControl tcMain;
        DataRow drCurrent;
        GridView gvDH, gvSP, gvKV;
        CDTForm frmDH, frmSP, frmKV;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        Database db = Database.NewDataDatabase();
        #region ICControl Members

        public void AddEvent()
        {
            tcMain = _data.FrmMain.Controls.Find("tcMain", true)[0] as XtraTabControl;
            gvDH = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvSP = (_data.FrmMain.Controls.Find("mDTKhuyenMaiSP", true)[0] as GridControl).MainView as GridView;
            gvKV = (_data.FrmMain.Controls.Find("mDTKhuyenMaiKV", true)[0] as GridControl).MainView as GridView;
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnSP = new SimpleButton();
            btnSP.Name = "btnSP"; 
            btnSP.Text = "Chọn sản phẩm";
            LayoutControlItem lci1 = lcMain.AddItem("", btnSP);
            lci1.Name = "cusSP";
            btnSP.Click += new EventHandler(btnSP_Click);

            SimpleButton btnKV = new SimpleButton();
            btnKV.Name = "btnKV";
            btnKV.Text = "Chọn khu vực";
            LayoutControlItem lci2 = lcMain.AddItem("", btnKV);
            lci2.Name = "cusKV";
            btnKV.Click += new EventHandler(btnKV_Click);

            DataSet dsData = _data.BsMain.DataSource as DataSet;
            gvSP.CellValueChanged += GvSP_CellValueChanged;



        }

        private void GvSP_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "NhieuSP" && e.Value != DBNull.Value)
            {
                string spid = gvSP.GetFocusedRowCellValue("DTSPID").ToString();
                string query = string.Format(@"SELECT km.*, sp.TenSP FROM mDTKMNhieuSP km
                                                LEFT JOIN mDMSP sp ON km.MaSPTang = sp.MaSP
                                                WHERE DTSPID = {0}", spid);
                DataTable data = db.GetDataTable(query);


                DsSanPham frm = new DsSanPham();
                frm.source.DataSource = data;
                frm.ShowDialog();
            }
        }

        void btnKV_Click(object sender, EventArgs e)
        { 
            if (!gvSP.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            tcMain.SelectedTabPageIndex = 2;
            drCurrent = (_data.BsMain.Current as DataRowView).Row;
            frmKV = FormFactory.FormFactory.Create(FormType.Report, "1585") as ReportPreview;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy3 = (frmKV.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy3.Text = "Chọn khu vực";
            btnXuLy3.Click += new EventHandler(btnXuLyKV_Click);
            frmKV.WindowState = FormWindowState.Maximized;
            frmKV.ShowDialog();
        }

        void btnSP_Click(object sender, EventArgs e)
        {
            if (!gvSP.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            tcMain.SelectedTabPageIndex = 1;
            drCurrent = (_data.BsMain.Current as DataRowView).Row;
            frmSP = FormFactory.FormFactory.Create(FormType.Report, "1583") as ReportPreview;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy1 = (frmSP.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy1.Text = "Chọn sản phẩm";
            btnXuLy1.Click += new EventHandler(btnXuLySP_Click);
            frmSP.WindowState = FormWindowState.Maximized;
            frmSP.ShowDialog();
        }

        void btnXuLySP_Click(object sender, EventArgs e)
        {
            GridView gvDS = (frmSP.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            DataRow[] drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn sản phẩm", Config.GetValue("PackageName").ToString());
                return;
            }
            frmSP.Close();

            DataTable dtSP = (_data.BsMain.DataSource as DataSet).Tables[2];
            string filter = drCurrent["MTID"].Equals(DBNull.Value) ? "MTID is null and MaSPBan = '{0}'" :
                "MTID = " + drCurrent["MTID"].ToString() + " and MaSPBan = '{0}'";
            foreach (DataRow dr in drs)
            {
                if (dtSP.Select(string.Format(filter, dr["MaSP"])).Length > 0)
                    continue;
                gvSP.AddNewRow();
                gvSP.SetFocusedRowCellValue(gvSP.Columns["MaSPBan"], dr["MaSP"]);
                gvSP.UpdateCurrentRow();
            }
        }

        void btnXuLyKV_Click(object sender, EventArgs e)
        {
            GridView gvDS = (frmKV.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            DataRow[] drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn khu vực", Config.GetValue("PackageName").ToString());
                return;
            }
            frmKV.Close();

            DataTable dtKV = (_data.BsMain.DataSource as DataSet).Tables[3];
            string filter = drCurrent["MTID"].Equals(DBNull.Value) ? "MTID is null and KhuVuc = {0}" :
                "MTID = " + drCurrent["MTID"].ToString() + " and KhuVuc = {0}";
            foreach (DataRow dr in drs)
            {
                if (dtKV.Select(string.Format(filter, dr["ID"])).Length > 0)
                    continue;
                gvKV.AddNewRow();
                gvKV.SetFocusedRowCellValue(gvKV.Columns["KhuVuc"], dr["ID"]);
                gvKV.UpdateCurrentRow();
            }
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        #endregion
    }
}
