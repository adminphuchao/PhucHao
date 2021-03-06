using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using CDTLib;
using FormFactory;

namespace TapHopCP
{
    public class TapHopCP : ICControl
    {
        private Database _db = Database.NewDataDatabase();
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        LayoutControl lcMain;
        LayoutControlItem lciBtnTHLai;
        BaseLayoutItem lcgTHLai;
        private string sms = @"select	round(sum(case
				                                when lop = 3 then dai * rong * soluong / 10000
				                                when lop = 5 then dai * rong * soluong * 2 / 10000
				                                when lop = 7 then dai * rong * soluong * 3 / 10000
			                                end), 0)
                                from	MT22 m inner join DT22 d on m.MT22ID = d.MT22ID
                            where	month(m.NgayCT) = {0} and year(m.NgayCT) = {1}";
        private string sdd = @"select	round(sum(case
				                            when lop = 3 then (cao+rong+0.2) * ((dai+rong)*2+3) * soluong / 10000
				                            when lop = 5 then (cao+rong+0.3) * ((dai+rong)*2+4) * soluong / 10000
			                            end), 0)
                            from	MTNPhoi m inner join DTNPhoi d on m.MTID = d.MTID
                            where	month(m.NgayCT) = {0} and year(m.NgayCT) = {1}";
        private string sin = @"select	round(sum(case
				                            when dh.lop = 3 then (dh.cao+dh.rong+0.2) * ((dh.dai+dh.rong)*2+3) * d.soluong / 10000
				                            when dh.lop = 5 then (dh.cao+dh.rong+0.3) * ((dh.dai+dh.rong)*2+4) * d.soluong / 10000
			                            end), 0)
                            from	MT22 m inner join DT22 d on m.MT22ID = d.MT22ID
		                            inner join DTDonHang dh on d.DTDHID = dh.DTDHID
                            where	month(m.NgayCT) = {0} and year(m.NgayCT) = {1} and dh.SoMau > 0";
        #region ICControl Members

        public void AddEvent()
        {
             
            _data.BsMain.PositionChanged += new EventHandler(BsMain_PositionChanged);   //tập hợp số liệu cần tập hợp lại (lệch) khi xem
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);//khai báo sự kiện để tự động tập hợp chi phí khi thêm mới
            BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
            _data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            //thêm nút tập hợp lại
            lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnTHLai = new SimpleButton();
            btnTHLai.Name = "btnTHLai";
            btnTHLai.Text = "Tập hợp lại";
            btnTHLai.Appearance.ForeColor = System.Drawing.Color.Red;
            lciBtnTHLai = lcMain.AddItem("", btnTHLai);
            lciBtnTHLai.Name = "cusTHLai";
            btnTHLai.Click += new EventHandler(btnTHLai_Click);
            //sự kiện double click vào grid để xem chi tiết
            GridView gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvMain.DoubleClick += new EventHandler(gvMain_DoubleClick);
        }

        void gvMain_DoubleClick(object sender, EventArgs e)
        {
            GridView gvMain = sender as GridView;
            Point pt = gvMain.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = gvMain.CalcHitInfo(pt);
            if (!info.InRow && !info.InRowCell)
                return;
            if (!gvMain.IsDataRow(gvMain.FocusedRowHandle))
                return;
            //dùng cách này để truyền tham số vào report
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            DateTime dt1 = new DateTime(Convert.ToInt32(drMaster["Nam"]), Convert.ToInt32(drMaster["Thang"]), 1);
            DateTime dt2 = dt1.AddMonths(1).AddDays(-1);
            Config.NewKeyValue("@NgayCT1", dt1);
            Config.NewKeyValue("@NgayCT2", dt2);
            DataRow drDetail = gvMain.GetDataRow(gvMain.FocusedRowHandle);
            Config.NewKeyValue("@LoaiCP", drDetail["LoaiCP"]);
            Config.NewKeyValue("@MaCP", drDetail["MaCP"]);
            Config.NewKeyValue("@Nguon", drDetail["Nguon"]);
            Config.NewKeyValue("@BoPhan", drDetail["BoPhan"]);
            XtraForm frmDS = FormFactory.FormFactory.Create(FormType.Report, "1565") as ReportPreview;
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void BsMain_PositionChanged(object sender, EventArgs e)
        {
            if (_data.BsMain.Current == null)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Unchanged && !Convert.ToBoolean(drMaster["Duyet"]))
                TapHop2(drMaster);
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            if (lcgTHLai == null)
                lcgTHLai = lcMain.Items.FindByName("item6");
            DataTable dtMaster = (_data.BsMain.DataSource as DataSet).Tables[0];
            if (dtMaster.Rows.Count > 0 && dtMaster.Rows[dtMaster.Rows.Count - 1].RowState == DataRowState.Added)     //thêm để chạy khi thêm lần đầu
                dtMaster_RowChanged(dtMaster, new DataRowChangeEventArgs(dtMaster.Rows[dtMaster.Rows.Count - 1], DataRowAction.Add));
            //thêm để tập hợp lại khi mở form chi tiết lần đầu
            if (_data.BsMain.Current == null)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Unchanged && !Convert.ToBoolean(drMaster["Duyet"]))
                TapHop2(drMaster);
            else
            {
                lciBtnTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                if (lcgTHLai != null)
                    lcgTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            if (_data.BsMain.DataSource == null)
                return;
            DataTable dtMaster = (_data.BsMain.DataSource as DataSet).Tables[0];
            dtMaster.RowChanged += new DataRowChangeEventHandler(dtMaster_RowChanged);
        }

        void dtMaster_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
                TapHop(e.Row);
        }

        void btnTHLai_Click(object sender, EventArgs e)
        {
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            decimal cpms2 = Convert.ToDecimal(drMaster["CPMS2"]);
            decimal cpdd2 = Convert.ToDecimal(drMaster["CPDD2"]);
            decimal cpin2 = Convert.ToDecimal(drMaster["CPIN2"]);
            decimal slms2 = Convert.ToDecimal(drMaster["SLMS2"]);
            decimal sldd2 = Convert.ToDecimal(drMaster["SLDD2"]);
            decimal slin2 = Convert.ToDecimal(drMaster["SLIN2"]);
            if (cpms2 == 0 && cpdd2 == 0 && cpin2 == 0
                && slms2 == 0 && sldd2 == 0 && slin2 == 0)
                return;
            _data.FrmMain.Activate();
            SendKeys.SendWait("{F3}");
            if (cpms2 != 0)
                TapHopLai(drMaster, "Máy sóng");
            if (cpdd2 != 0)
                TapHopLai(drMaster, "Đóng dán");
            if (cpin2 != 0)
                TapHopLai(drMaster, "In");
            if (slms2 != 0)
            {
                drMaster["SLMS"] = _db.GetValue(string.Format(sms, drMaster["Thang"], drMaster["Nam"]));
                drMaster["SLMS2"] = 0;
            }
            if (sldd2 != 0)
            {
                drMaster["SLDD"] = _db.GetValue(string.Format(sdd, drMaster["Thang"], drMaster["Nam"]));
                drMaster["SLDD2"] = 0;
            }
            if (slin2 != 0)
            {
                drMaster["SLIN"] = _db.GetValue(string.Format(sin, drMaster["Thang"], drMaster["Nam"]));
                drMaster["SLIN2"] = 0;
            }
            _data.FrmMain.Activate();
            SendKeys.SendWait("{F12}");
            lciBtnTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            if (lcgTHLai != null)
                lcgTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }

        private void TapHop2(DataRow drMaster)
        {
            try
            {
                string sql = "select sum(ThanhTien) from wCPSX where month(NgayCT) = {0} and year(NgayCT) = {1} and BoPhan = N'{2}'";
                decimal cpms = Convert.ToDecimal(drMaster["CPMS"]);
                decimal cpdd = Convert.ToDecimal(drMaster["CPDD"]);
                decimal cpin = Convert.ToDecimal(drMaster["CPIN"]);
                decimal slms = Convert.ToDecimal(drMaster["SLMS"]);
                decimal sldd = Convert.ToDecimal(drMaster["SLDD"]);
                decimal slin = Convert.ToDecimal(drMaster["SLIN"]);
                //chi phi moi
                decimal cpms2 = Convert.ToDecimal(_db.GetValue(string.Format(sql, drMaster["Thang"], drMaster["Nam"], "Máy sóng")));
                decimal cpdd2 = Convert.ToDecimal(_db.GetValue(string.Format(sql, drMaster["Thang"], drMaster["Nam"], "Đóng dán")));
                decimal cpin2 = Convert.ToDecimal(_db.GetValue(string.Format(sql, drMaster["Thang"], drMaster["Nam"], "In")));
                //san luong thuc te moi
                decimal slms2 = Convert.ToDecimal(_db.GetValue(string.Format(sms, drMaster["Thang"], drMaster["Nam"])));
                decimal sldd2 = Convert.ToDecimal(_db.GetValue(string.Format(sdd, drMaster["Thang"], drMaster["Nam"])));
                decimal slin2 = Convert.ToDecimal(_db.GetValue(string.Format(sin, drMaster["Thang"], drMaster["Nam"])));
                if (cpms2 != cpms)
                    drMaster["CPMS2"] = cpms2 - cpms;
                if (cpdd2 != cpdd)
                    drMaster["CPDD2"] = cpdd2 - cpdd;
                if (cpin2 != cpin)
                    drMaster["CPIN2"] = cpin2 - cpin;
                if (slms != slms2)
                    drMaster["SLMS2"] = slms2 - slms;
                if (sldd != sldd2)
                    drMaster["SLDD2"] = sldd2 - sldd;
                if (slin != slin2)
                    drMaster["SLIN2"] = slin2 - slin;
                if ((cpms2 != cpms) || (cpdd2 != cpdd) || (cpin2 != cpin)
                    || (slms != slms2) || (sldd != sldd2) || (slin != slin2))
                {
                    lciBtnTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    if (lcgTHLai != null)
                        lcgTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    drMaster.AcceptChanges();
                }
                else
                {
                    lciBtnTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    if (lcgTHLai != null)
                        lcgTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                }
            }
            catch (Exception) {}
        }

        private void TapHopLai(DataRow drMaster, string BoPhan)
        {
            string sql = @" select	Nguon, MaCP, LoaiCP, BoPhan, sum(ThanhTien) as ThanhTien
                                    , TyLe = round((sum(ThanhTien) / (select sum(ThanhTien) from wCPSX w1
                                                where month(w1.NgayCT) = {0} and year(w1.NgayCT) = {1} and w1.BoPhan = w.BoPhan)), 4)
                            from	wCPSX w
                            where month(NgayCT) = {0} and year(NgayCT) = {1} and BoPhan = N'{2}'
                            group by MaCP, LoaiCP, BoPhan, Nguon
                            order by Nguon, LoaiCP, MaCP, BoPhan";
            DataTable dtData = _db.GetDataTable(string.Format(sql, drMaster["Thang"], drMaster["Nam"], BoPhan));
            //cap nhat thong tin master
            if (BoPhan == "Máy sóng")
            {
                drMaster["CPMS"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'Máy sóng'");
                drMaster["CPMS2"] = 0;
            }
            if (BoPhan == "Đóng dán")
            {
                drMaster["CPDD"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'Đóng dán'");
                drMaster["CPDD2"] = 0;
            }
            if (BoPhan == "In")
            {
                drMaster["CPIN"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'In'");
                drMaster["CPIN2"] = 0;
            }

            string mtid = drMaster["MTID"].ToString();
            DataTable dtDetail = (_data.BsMain.DataSource as DataSet).Tables[1];
            DataRow[] drs = dtDetail.Select("MTID = " + mtid + " and BoPhan = '" + BoPhan + "'");
            foreach (DataRow dr in drs)
                dr.Delete();
            foreach (DataRow dr in dtData.Rows)
            {
                DataRow drn = dtDetail.NewRow();
                drn["MTID"] = mtid;
                drn["Nguon"] = dr["Nguon"];
                drn["MaCP"] = dr["MaCP"];
                drn["LoaiCP"] = dr["LoaiCP"];
                drn["BoPhan"] = dr["BoPhan"];
                drn["ThanhTien"] = dr["ThanhTien"];
                drn["TyLe"] = dr["TyLe"];
                dtDetail.Rows.Add(drn);
            }
        }

        private void TapHop(DataRow drMaster)
        {
            if (!_data.FrmMain.Visible)
                return;
            FrmThang frm = new FrmThang();
            frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                _data.FrmMain.Activate();
                SendKeys.SendWait("{ESC}");
            }

            drMaster["Thang"] = frm.Thang;
            string sql = @" select	Nguon, MaCP, LoaiCP, BoPhan, sum(ThanhTien) as ThanhTien
                                    , TyLe = round((sum(ThanhTien) / (select sum(ThanhTien) from wCPSX w1
                                                where month(w1.NgayCT) = {0} and year(w1.NgayCT) = {1} and w1.BoPhan = w.BoPhan)), 4)
                            from	wCPSX w
                            where month(NgayCT) = {0} and year(NgayCT) = {1}
                            group by MaCP, LoaiCP, BoPhan, Nguon
                            order by Nguon, LoaiCP, MaCP, BoPhan";
            DataTable dtData = _db.GetDataTable(string.Format(sql, drMaster["Thang"], drMaster["Nam"]));
            //cap nhat thong tin master
            drMaster["CPMS"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'Máy sóng'");
            drMaster["CPDD"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'Đóng dán'");
            drMaster["CPIN"] = dtData.Compute("sum(ThanhTien)", "BoPhan = 'In'");
            DataSet ds = _data.BsMain.DataSource as DataSet;

            DataTable dtDetail = (_data.BsMain.DataSource as DataSet).Tables[1];
            foreach (DataRow dr in dtData.Rows)
            {
                DataRow drn = dtDetail.NewRow();
                drn["MTID"] = drMaster["MTID"];
                drn["Nguon"] = dr["Nguon"];
                drn["MaCP"] = dr["MaCP"];
                drn["LoaiCP"] = dr["LoaiCP"];
                drn["BoPhan"] = dr["BoPhan"];
                drn["ThanhTien"] = dr["ThanhTien"];
                drn["TyLe"] = dr["TyLe"];
                dtDetail.Rows.Add(drn);
            }
            //cap nhat san luong thuc te
            drMaster["SLMS"] = _db.GetValue(string.Format(sms, drMaster["Thang"], drMaster["Nam"]));
            drMaster["SLDD"] = _db.GetValue(string.Format(sdd, drMaster["Thang"], drMaster["Nam"]));
            drMaster["SLIN"] = _db.GetValue(string.Format(sin, drMaster["Thang"], drMaster["Nam"]));
            //an cac control tap hop lai
            lciBtnTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            if (lcgTHLai != null)
                lcgTHLai.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
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
