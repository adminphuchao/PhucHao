using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using FormFactory;
using CDTLib;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms;

namespace TinhNgayGH
{
    public class TinhNgayGH : ICControl
    {
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        DateEdit ngayGH;    //dung control moi gan duoc du lieu
        ComboBoxEdit loai;  //dung control moi lay duoc du lieu
        DataRow drCur;
        Form frmDS;
        GridView gvMain;

        #region ICControl Members

        public void AddEvent()
        {
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            ngayGH = _data.FrmMain.Controls.Find("NgayGH", true)[0] as DateEdit;
            loai = _data.FrmMain.Controls.Find("Loai", true)[0] as ComboBoxEdit;
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());

            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            //thêm nút Lấy bảng in
            SimpleButton btnLayBI = new SimpleButton();
            btnLayBI.Name = "btnLayBI";
            btnLayBI.Text = "Chọn bảng in";
            LayoutControlItem lci1 = lcMain.AddItem("", btnLayBI);
            lci1.Name = "cusLayBI";
            btnLayBI.Click += new EventHandler(btnLayBI_Click);
            //thêm nút Lấy phôi sóng
            SimpleButton btnLayPS = new SimpleButton();
            btnLayPS.Name = "btnLayPS";
            btnLayPS.Text = "Chọn phôi sóng";
            btnLayPS.Click += BtnLayPS_Click;
            LayoutControlItem lci2 = lcMain.AddItem("", btnLayPS);
            lci2.Name = "cusLayPS";
        }

        private void BtnLayPS_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            Boolean duyet = Boolean.Parse(drCur["Duyet"].ToString());
            if (duyet)
            {
                XtraMessageBox.Show("Không được chọn phôi sóng cho đơn hàng đã duyệt", Config.GetValue("PackageName").ToString());
                return;
            }

            if (gvMain.DataRowCount == 0)
            {
                XtraMessageBox.Show("Chưa có mặt hàng nào để chọn phôi sóng, vui lòng chọn báo giá để lập đơn hàng trước",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            string pk = _data.DrTableMaster["Pk"].ToString();

            DataSet ds = _data.BsMain.DataSource as DataSet;
            var maKh = drCur["MaKH"].ToString();

            FormDTDonHang frm = new FormDTDonHang(maKh);
            ds.Tables[1].DefaultView.RowFilter = string.Format("{0} = '{1}'", pk, drCur[pk]);
            frm.DtDonHang = ds.Tables[1];
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.ShowDialog();

        }

        void btnLayBI_Click(object sender, EventArgs e)
        {
            if (ngayGH.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            drCur = (_data.BsMain.Current as DataRowView).Row;
            if (drCur["MaKH"].ToString() == string.Empty)
            {
                XtraMessageBox.Show("Vui lòng chọn khách hàng trước",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            //dùng report 1582 trong sysReport
            Config.NewKeyValue("@MaKH", drCur["MaKH"]);
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1582") as ReportPreview;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn bảng in";
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void btnXuLy_Click(object sender, EventArgs e)
        {
            GridView gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            DataRow[] drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn bảng in", Config.GetValue("PackageName").ToString());
                return;
            }
            if (drs.Length > 1)
            {
                XtraMessageBox.Show("Bạn chỉ được chọn 1 bảng in cho 1 đơn hàng", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            GridView gvDH = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            if (gvDH.FocusedRowHandle >= 0)
            {
                gvDH.SetFocusedRowCellValue(gvDH.Columns["MaBangIn"], drs[0]["MaBangIn"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Dai"], drs[0]["Dai"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Rong"], drs[0]["Rong"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Cao"], drs[0]["Cao"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Lan"], drs[0]["LoaiLan"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["SoMau"], drs[0]["SoMau"]);
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Ghim"], drs[0]["GN"].ToString().Contains("Ghim"));
                gvDH.SetFocusedRowCellValue(gvDH.Columns["Dan"], drs[0]["GN"].ToString().Contains("Dán"));
                gvDH.UpdateCurrentRow();
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            if (_data.BsMain == null)
                return;
            DataSet ds = _data.BsMain.DataSource as DataSet;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(TinhNgayGH_ColumnChanged);
            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(TinhNgayGH1_ColumnChanged);
        }

        void TinhNgayGH1_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted || drMaster["NgayCT"] == DBNull.Value)
                return;
            if (e.Column.ColumnName == "Loai")
            {
                if (e.Row["Loai"] != DBNull.Value)
                    e.Row["NgayGH"] = NgayGH(Convert.ToDateTime(drMaster["NgayCT"]), e.Row["Loai"].ToString());
                if (e.Row["Loai"].ToString() == "Thùng")
                    e.Row["CD2"] = 0;
            }
            else if (e.Column.ColumnName == "isCL")
            {

                if (e.Row["Loai"] == DBNull.Value)
                {
                    return;
                }

                if (e.Row["Loai"].ToString() == "Tấm")
                {
                    if (e.Row["Dai"] == DBNull.Value || e.Row["Dao"] == DBNull.Value
                  || e.Row["SoLuong"] == DBNull.Value || e.Row["GiaBan"] == DBNull.Value)
                    {
                        return;
                    }
                }
                else
                {
                    if (e.Row["Dai"] == DBNull.Value || e.Row["Rong"] == DBNull.Value || e.Row["Dao"] == DBNull.Value
                  || e.Row["SoLuong"] == DBNull.Value || e.Row["GiaBan"] == DBNull.Value)
                    {
                        return;
                    }
                }

                decimal dai = Convert.ToDecimal(e.Row["Dai"]);
                decimal rong = Convert.ToDecimal(e.Row["Rong"]);
                int lop = Convert.ToInt32(e.Row["Lop"]);
                decimal dao = Convert.ToDecimal(e.Row["Dao"]) == 0 ? 1 : Convert.ToDecimal(e.Row["Dao"]);
                decimal soluong = Convert.ToDecimal(e.Row["SoLuong"]);
                decimal dongia = 0;
                bool isHasOrigin = false;
                try
                {
                    
                    dongia = Convert.ToDecimal(e.Row["GiaBan", DataRowVersion.Original]);
                    isHasOrigin = true;
                }
                catch
                {
                    dongia = Convert.ToDecimal(e.Row["GiaBan"]);
                    isHasOrigin = false;
                }
               
                decimal somt = 0;
                if (e.Row["Loai"].ToString() == "Tấm")
                {
                    //Số mét tới
                    somt = dai * soluong / dao / 100;
                }
                else
                {
                    //Số mét tới
                    somt = (((dai + rong) * 2 + (lop == 3 ? 4 : 5)) * soluong / dao) / 100;
                }

                if (somt < 1000)
                {
                    if (Convert.ToBoolean(e.Row["isCL"]))
                        // thêm 2% vào giá
                        e.Row["GiaBan"] = dongia * 102 / 100;
                    else
                    {
                        if (isHasOrigin)
                        {
                            e.Row["GiaBan"] = dongia;
                        } else
                        {
                            // giảm 2% khi thêm m
                            e.Row["GiaBan"] = dongia * 98 / 100;
                        }
                    }
                        
                }
            }
        }
        void TinhNgayGH_ColumnChanged(object sender, DataColumnChangeEventArgs e)

        {
            if (e.Row.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName == "NgayCT" && e.Row["NgayCT"] != DBNull.Value && e.Row["MTDHID"] != DBNull.Value)
            {
                DataSet ds = _data.BsMain.DataSource as DataSet;
                DataRow[] drsDH = ds.Tables[1].Select("MTDHID = '" + e.Row["MTDHID"] + "'");
                foreach (DataRow drDH in drsDH)
                    if (drDH.RowState != DataRowState.Deleted && drDH["Loai"] != DBNull.Value)
                    {
                        drDH["NgayGH"] = NgayGH(Convert.ToDateTime(e.Row["NgayCT"]), drDH["Loai"].ToString());
                        drDH.EndEdit();
                    }
            }
        }

        private bool LaNgayLe(DateTime ngay)
        {
            List<DateTime> lstNgayLe = new List<DateTime>();
            lstNgayLe.Add(new DateTime(ngay.Year, 1, 1));
            lstNgayLe.Add(new DateTime(ngay.Year, 4, 30));
            lstNgayLe.Add(new DateTime(ngay.Year, 5, 1));
            lstNgayLe.Add(new DateTime(ngay.Year, 9, 2));
            lstNgayLe.Add(new DateTime(ngay.Year + 1, 1, 1));
            foreach (DateTime le in lstNgayLe)
                if (le == ngay)
                    return true;
            return false;
        }

        private DateTime NgayGH(DateTime ngayCT, string loai)
        {
            DateTime ngaygh = ngayCT;
            int d = (loai == "Thùng") ? 4 : 3;
            while (d > 0)
            {
                ngaygh = ngaygh.AddDays(1);
                while (ngaygh.DayOfWeek == DayOfWeek.Sunday || LaNgayLe(ngaygh))
                    ngaygh = ngaygh.AddDays(1);
                d--;
            }
            return ngaygh;
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
