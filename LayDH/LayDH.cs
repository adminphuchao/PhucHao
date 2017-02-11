using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Data;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors.Repository;
using System.Drawing;
using DevExpress.XtraGrid.Columns;
using FormFactory;
using System.Windows.Forms;
using CDTDatabase;

namespace LayDH
{
    public class LayDH : ICControl
    {
        DataRow drCur;
        GridView gvMain;
        ReportPreview frmDS;
        GridView gvDS;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        #region ICControl Members

        public void AddEvent()
        {
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            //thêm nút chọn DH
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnChon = new SimpleButton();
            btnChon.Name = "btnChon";
            btnChon.Text = "Chọn hàng";
            LayoutControlItem lci = lcMain.AddItem("", btnChon);
            lci.Name = "cusChon";
            btnChon.Click += new EventHandler(btnChon_Click);
        }

        void gvMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "SoLuong")
            {
                object osl = gvMain.GetFocusedRowCellValue("SoLuong");
                object odg = gvMain.GetFocusedRowCellValue("DonGia");
                object od = gvMain.GetFocusedRowCellValue("Dai");
                object or = gvMain.GetFocusedRowCellValue("Rong");
                decimal sl = (osl == null || osl.ToString() == "") ? 0 : decimal.Parse(osl.ToString());
                decimal dg = (odg == null || odg.ToString() == "") ? 0 : decimal.Parse(odg.ToString());
                decimal d = (od == null || od.ToString() == "") ? 0 : decimal.Parse(od.ToString());
                decimal r = (or == null || or.ToString() == "") ? 0 : decimal.Parse(or.ToString());
                object l = gvMain.GetFocusedRowCellValue("Loai");
                if (l != null && l.ToString() == "Tấm")
                {
                    decimal tt = Math.Round(sl * d * r / 10000,0) * Math.Round(dg,0);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["ThanhTien"], tt - (tt % 10));
                }
                else
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["ThanhTien"], sl * dg);
            }
        } 

        void btnChon_Click(object sender, EventArgs e)
        {
            drCur = (_data.BsMain.Current as DataRowView).Row;
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            //dùng report 1514 trong sysReport
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1515") as ReportPreview;
            //định dạng thêm cho grid của report
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn đơn hàng";
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void btnXuLy_Click(object sender, EventArgs e)
        {
            DataTable dtDS = (gvDS.DataSource as DataView).Table;
            dtDS.AcceptChanges();
            DataRow[] drs = dtDS.Select("Chọn = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Bạn chưa chọn đơn hàng", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            using (DataTable tmp = dtDTKH.Clone())
            {
                tmp.PrimaryKey = null;
                tmp.Columns["DT22ID"].AllowDBNull = true;
                foreach (DataRow dr in drs)
                {
                    
                    tmp.ImportRow(dr);
                    tmp.Rows[tmp.Rows.Count - 1]["NoiDungCt"] = dr["GhiChu"];
                    tmp.Rows[tmp.Rows.Count-1]["SLPO"] = dr["SL Đặt"];
                }
                Database db = Database.NewDataDatabase();
                foreach (DataRow dr in tmp.Rows)
                {
                    if (dtDTKH.Select( string.Format("MT22ID = '{0}' and DTDHID = '{1}'"
                                        , drCur["MT22ID"], dr["DTDHID"])).Length > 0)
                        continue;
                    //Công thêm ngày 25/5/2015 kiểm tra nếu chưa có mã hàng thì insert 1 lần nữa (do code tạo mã hàng khi lập đơn hàng có vấn đề gì đó)
                    string sql = @"if not exists (select MaHH from DMHH where MaHH = @@MaHH)
                                insert into DMHH(MaHH, TenHH, DVT, QuyCach, GiaBan)
                                values(@@MaHH, @@TenHH, @@DVT, @@QuyCach, @@GiaBan)";
                    string[] paraNames = new string[] { "@@MaHH", "@@TenHH", "@@DVT", "@@QuyCach", "@@GiaBan" };
                    string mahh = dr["MaKH"].ToString() + "_" + dr["QuyCach"].ToString();
                    object[] values = new object[] { mahh, dr["TenHang"], dr["DVT"], dr["QuyCach"], dr["DonGia"] };
                    db.UpdateDatabyPara(sql, paraNames, values);
                    //them du lieu don hang vao phieu nhap thanh pham
                    gvMain.AddNewRow();
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaHH"], dr["MaHH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["TenHang"], dr["TenHang"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoDH"], dr["SoDH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayDH"], dr["NgayDH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DVT"], dr["DVT"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["QuyCach"], dr["QuyCach"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaKH"], dr["MaKH"]);  
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Dai"], dr["Dai"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Rong"], dr["Rong"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Cao"], dr["Cao"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DTDHID"], dr["DTDHID"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Loai"], dr["Loai"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Lop"], dr["Lop"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NoiDungCt"], dr["NoiDungCt"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DonGia"], dr["DonGia"]);
                    gvMain.UpdateCurrentRow();
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLuong"], dr["SoLuong"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SLPO"], dr["SLPO"]);
                    gvMain.UpdateCurrentRow();
                }
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
