using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using FormFactory;
using DevExpress.XtraGrid.Views.Grid;
using CDTDatabase;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using System.Data;
using CDTLib;
using System.Windows.Forms;
using DevExpress.XtraGrid.Columns;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.Drawing;

namespace LayLSX2
{
    public class LayLSX2 : ICControl
    {
        ReportPreview frmDS;
        GridView gvDS;
        GridView gvMain;
        DataCustomFormControl _data;
        Database db = Database.NewDataDatabase();
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        DataRow drCur;
        GridHitInfo downHitInfo = null;

        #region ICControl Members

        public void AddEvent()
        {
            //thêm nút chọn LSX
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnChon = new SimpleButton();
            btnChon.Name = "btnChon";
            btnChon.Text = "Chọn LSX";
            LayoutControlItem lci = lcMain.AddItem("", btnChon);
            lci.Name = "cusChon";
            btnChon.Click += new EventHandler(btnChon_Click);
            //thêm nút Xuất file
            SimpleButton btnXuatFile = new SimpleButton();
            btnXuatFile.Name = "btnXuatFile";
            btnXuatFile.Text = "Xuất file";
            LayoutControlItem lci2 = lcMain.AddItem("", btnXuatFile);
            lci2.Name = "cusXuatFile";
            btnXuatFile.Click += new EventHandler(btnXuatFile_Click);
            //chuc nang danh dau hoan thanh
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            foreach (GridColumn col in gvMain.Columns)
            {
                if (col.Name == "clHT")
                    col.ColumnEdit.EditValueChanged += new EventHandler(HT_EditValueChanged);
            }
            //chức năng drag drop để thay đổi thứ tự các dòng
            gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gvMain.GridControl.AllowDrop = true;
            gvMain.MouseDown += new MouseEventHandler(gvMain_MouseDown);
            gvMain.MouseMove += new MouseEventHandler(gvMain_MouseMove);
            gvMain.GridControl.DragOver += new DragEventHandler(GridControl_DragOver);
            gvMain.GridControl.DragDrop += new DragEventHandler(GridControl_DragDrop);
        }

        void GridControl_DragDrop(object sender, DragEventArgs e)
        {
            GridControl grid = sender as GridControl;
            GridView view = grid.MainView as GridView;
            if (!view.Editable)
                return;
            GridHitInfo srcHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
            GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
            int sourceRow = srcHitInfo.RowHandle;
            int targetRow = hitInfo.RowHandle;
            MoveRow(sourceRow, targetRow);
        }

        void GridControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GridHitInfo)))
            {
                GridHitInfo downHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
                if (downHitInfo == null)
                    return;

                GridControl grid = sender as GridControl;
                GridView view = grid.MainView as GridView;
                GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
                if (hitInfo.InRow && hitInfo.RowHandle != downHitInfo.RowHandle && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        void gvMain_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(downHitInfo, DragDropEffects.All);
                    downHitInfo = null;
                }
            }
        }

        void gvMain_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            downHitInfo = null;

            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                downHitInfo = hitInfo;
        }

        private void MoveRow(int sourceRow, int targetRow)
        {
            if (sourceRow == targetRow || sourceRow == targetRow + 1)
                return;

            DataRow row1 = gvMain.GetDataRow(targetRow);
            DataRow row2 = gvMain.GetDataRow(targetRow + 1);
            DataRow dragRow = gvMain.GetDataRow(sourceRow);
            decimal val1 = (decimal)row1["Stt"];
            if (row2 == null)
                dragRow["Stt"] = val1 + 1;
            else
            {
                decimal val2 = (decimal)row2["Stt"];
                dragRow["Stt"] = (val1 + val2) / 2;
            }
            gvMain.RefreshData();
        }

        void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(Config.GetValue("Admin")) &&
                (!_data.DrTable.Table.Columns.Contains("sApprove")
                || !Convert.ToBoolean(_data.DrTable["sApprove"])))
            {
                XtraMessageBox.Show("Bạn không có quyền sử dụng tính năng này\nVui lòng liên hệ quản trị hệ thống!",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            if (_data.BsMain.Current == null)
                return;
            if (gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng thực hiện xuất file ở chế độ xem phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV file|*.csv";
            if (sfd.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            string file = sfd.FileName;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            DataSet dsData = _data.BsMain.DataSource as DataSet;
            int i = -1;
            try
            {
                //lấy nội dung file
                using (DataView dvDetail = new DataView(dsData.Tables[1]))
                {
                    dvDetail.RowFilter = string.Format("MTKHID = '{0}'", drMaster["MTKHID"]);
                    dvDetail.Sort = "Stt";
                    string[] contents = new string[dvDetail.Count];
                    for (i = 0; i < dvDetail.Count; i++)
                        contents[i] = string.Format(",{0},{1},{2},{3},{4},{5},{6},{7}",
                            dvDetail[i]["THS"],
                            dvDetail[i]["SoLSX"],
                            dvDetail[i]["MaKH"],
                            dvDetail[i]["TenHang"],
                            dvDetail[i]["QuyCach"],
                            (Convert.ToDouble(dvDetail[i]["SLSX"])).ToString("###,###.0"),
                            "\"" + dvDetail[i]["GhiChu"] + "\"",
                            dvDetail[i]["Loai"]);
                    //chép ra file
                    File.WriteAllLines(file, contents);
                    XtraMessageBox.Show("Đã xuất file thành công!", Config.GetValue("PackageName").ToString());
                }
            }
            catch (Exception ex)
            {
                if (i == -1)
                    XtraMessageBox.Show("Có lỗi phát sinh khi xuất file.\n" + ex.Message);
                else
                    XtraMessageBox.Show(string.Format("Có lỗi phát sinh khi xuất file ở dòng thứ {0}.\n", i + 1) + ex.Message);
            }
        }

        void btnChon_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            drCur = (_data.BsMain.Current as DataRowView).Row;
            //dùng report 1514 trong sysReport
            Config.NewKeyValue("@CD2", 0);
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1514") as ReportPreview;
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn LSX";
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
                XtraMessageBox.Show("Bạn chưa chọn lệnh sản xuất", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            using (DataTable tmp = dtDTKH.Clone())
            {
                tmp.PrimaryKey = null;
                tmp.Columns["DTKHID"].AllowDBNull = true;
                foreach (DataRow dr in drs)
                    tmp.ImportRow(dr);
                decimal n = gvMain.DataRowCount == 0 ? 0 : (decimal)gvMain.GetRowCellValue(gvMain.DataRowCount - 1, "Stt");
                foreach (DataRow dr in tmp.Rows)
                {
                    if (dtDTKH.Select(string.Format("MTKHID = '{0}' and DTLSXID = '{1}'", drCur["MTKHID"], dr["DTLSXID"])).Length > 0)
                        continue;
                    n++;
                    gvMain.AddNewRow();

                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DTKHID"], Guid.NewGuid());
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Stt"], n);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["TenHang"], dr["TenHang"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaKH"], dr["MaKH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["TenTat"], dr["TenTat"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLSX"], dr["SoLSX"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayLSX"], dr["NgayLSX"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["GhiChu"], dr["GhiChu"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Loai"], dr["Loai"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["QuyCach"], dr["QuyCach"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["THS"], dr["THS"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["GHIM"], dr["Ghim"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Dan"], dr["Dan"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], false);
                    //đưa 2 lệnh set ID này vào đây để tăng tốc xử lý
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MTKHID"], drCur["MTKHID"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DTLSXID"], dr["DTLSXID"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SLSX"], dr["SLSX"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLuong"], dr["SoLuong"]);
                    gvMain.UpdateCurrentRow();
                }
            }
        }

        void HT_EditValueChanged(object sender, EventArgs e)
        {
            CheckEdit ce = sender as CheckEdit;
            if (ce.Properties.ReadOnly == true)
                return;
            if (Convert.ToBoolean(ce.EditValue) == true)
            {
                object o = gvMain.GetFocusedRowCellValue("SoLuong");
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SLSX"], o);
            }
            else
            {
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SLSX"], 0);
            }
        }

        void gvMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "SLSX" && e.Value != DBNull.Value)
            {
                object o = gvMain.GetFocusedRowCellValue("SLTon");
                if (o != null && o.ToString() != "" && decimal.Parse(o.ToString()) <= 0)
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], true);
                else
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], false);

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
