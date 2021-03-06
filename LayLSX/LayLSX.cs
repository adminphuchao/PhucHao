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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using CDTDatabase;
using System.IO;
using System.Text.RegularExpressions;
namespace LayLSX
{
    public class LayLSX : ICControl
    {
        GridHitInfo downHitInfo = null;
        DataRow drCur;
        ReportPreview frmDS;
        GridView gvDS;
        GridView gvMain;
        DataCustomFormControl _data;
        Database db = Database.NewDataDatabase();
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        DataTable dtKyHieu;
        DataTable dtTHS;
        DataTable tableTinhGiay = new DataTable("mDTTinhGiay");
        GridControl gridControl1 = new GridControl();
        GridView gridView1 = new GridView();
        GridColumn gridColumn1 = new GridColumn();
        GridColumn gridColumn2 = new GridColumn();
        GridColumn gridColumn3 = new GridColumn();
        GridColumn gridColumn4 = new GridColumn();
        #region ICControl Members

        public void AddEvent()
        {
            ////Them grid Tính giấy vào form

            tableTinhGiay.Columns.Add("Kho", typeof(decimal));
            tableTinhGiay.Columns.Add("LoaiGiay", typeof(string));
            tableTinhGiay.Columns.Add("SlChay", typeof(decimal));
            tableTinhGiay.Columns.Add("SlTon", typeof(decimal));

            //định dạng thêm cho grid chi tiết hàng
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            Font f = new Font(gvMain.Columns["ChKho"].AppearanceCell.Font, FontStyle.Bold);
            gvMain.Columns["ChKho"].AppearanceCell.Font = f;
            gvMain.Columns["SoTam"].AppearanceCell.Font = f;
            gvMain.Columns["SLTon"].AppearanceCell.Font = f;
            List<string> lstField = new List<string>(new string[] { "ChKho", "ChDai", "SoTam", "SoMT", "Dao", "Lan1", "Cao1", "Lan2", "SoLuong", "SLSX" });
            foreach (GridColumn gc in gvMain.Columns)
            {
                if (lstField.Contains(gc.FieldName))
                {
                    //RepositoryItemCalcEdit reCalc = gc.ColumnEdit as RepositoryItemCalcEdit;
                    gc.AppearanceCell.BackColor = Color.LightCyan;
                }
                //Không cho sroll thay đổi số trong calcedit
                if (gc.ColumnEdit != null && gc.ColumnEdit.EditorTypeName == "CalcEdit")
                {
                    RepositoryItemCalcEdit reCalc = gc.ColumnEdit as RepositoryItemCalcEdit;
                    reCalc.Spin += new DevExpress.XtraEditors.Controls.SpinEventHandler(reCalc_Spin);
                }
            }
            gvMain.ShowingEditor += new System.ComponentModel.CancelEventHandler(gvMain_ShowingEditor);
            //chức năng drag drop để thay đổi thứ tự các dòng
            gvMain.Columns["Stt"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            gvMain.GridControl.AllowDrop = true;
            gvMain.MouseDown += new MouseEventHandler(gvMain_MouseDown);
            gvMain.MouseMove += new MouseEventHandler(gvMain_MouseMove);
            gvMain.GridControl.DragOver += new DragEventHandler(GridControl_DragOver);
            gvMain.GridControl.DragDrop += new DragEventHandler(GridControl_DragDrop);
            //thêm nút chọn LSX
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnChon = new SimpleButton();
            btnChon.Name = "btnChon";
            btnChon.Text = "Chọn LSX";
            LayoutControlItem lci = lcMain.AddItem("", btnChon);
            lci.Name = "cusChon";
            btnChon.Click += new EventHandler(btnChon_Click);
            //thêm nút Kết thúc khổ
            SimpleButton btnKTKho = new SimpleButton();
            btnKTKho.Name = "btnKTKho";
            btnKTKho.Text = "Kết thúc khổ";
            LayoutControlItem lci1 = lcMain.AddItem("", btnKTKho);
            lci1.Name = "cusKTKho";
            btnKTKho.Click += new EventHandler(btnKTKho_Click);
            //thêm nút Xuất file
            SimpleButton btnXuatFile = new SimpleButton();
            btnXuatFile.Name = "btnXuatFile";
            btnXuatFile.Text = "Xuất file";
            LayoutControlItem lci2 = lcMain.AddItem("", btnXuatFile);
            lci2.Name = "cusXuatFile";
            btnXuatFile.Click += new EventHandler(btnXuatFile_Click);

            //thêm nút Sắp xếp tự động
            SimpleButton btnSapXep = new SimpleButton();
            btnSapXep.Name = "btnSapXep";
            btnSapXep.Text = "Sắp xếp tự động";
            LayoutControlItem lci3 = lcMain.AddItem("", btnSapXep);
            lci3.Name = "cusSapXep";
            btnSapXep.Click += new EventHandler(btnSapXep_Click);


            //thêm nút Tính giấy
            SimpleButton btnTinhGiay = new SimpleButton();
            btnTinhGiay.Name = "btnTinhGiay";
            btnTinhGiay.Text = "Tính giấy";
            LayoutControlItem lci4 = lcMain.AddItem("", btnTinhGiay);
            lci4.Name = "cusTinhGiay";
            btnTinhGiay.Click += new EventHandler(btnTinhGiay_Click);


            //thêm nút sap xep
            SimpleButton btnSXThuTu = new SimpleButton();
            btnSXThuTu.Name = "btnSXThuTu";
            btnSXThuTu.Text = "Sắp xếp lại thứ tự";
            LayoutControlItem lci6 = lcMain.AddItem("", btnSXThuTu);
            lci6.Name = "cusSXThuTu";
            btnSXThuTu.Click += new EventHandler(btnSXThuTu_Click);



            //them grid tinh giay
            gridControl1 = new GridControl();
            gridView1 = new GridView();
            gridColumn1 = new GridColumn();
            gridColumn2 = new GridColumn();
            gridColumn3 = new GridColumn();
            gridColumn4 = new GridColumn();

            var repositoryItemCalcEdit1 = new RepositoryItemCalcEdit();

            ((System.ComponentModel.ISupportInitialize)(gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(repositoryItemCalcEdit1)).BeginInit();
            // 
            // repositoryItemCalcEdit1
            // 
            repositoryItemCalcEdit1.AutoHeight = false;
            repositoryItemCalcEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            repositoryItemCalcEdit1.Name = "repositoryItemCalcEdit1";
            repositoryItemCalcEdit1.EditMask = "###,###,##0";
            repositoryItemCalcEdit1.Mask.UseMaskAsDisplayFormat = true;

            // 
            // gridControl1
            // 
            //gridControl1.Dock = System.Windows.Forms.DockStyle.Top;
            //gridControl1.EmbeddedNavigator.Name = "";
            //gridControl1.Location = new System.Drawing.Point(0, 0);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridTinhGiay";
            gridControl1.Size = new Size(305, 175);
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {gridView1});
            gridControl1.RepositoryItems.AddRange(new RepositoryItem[] {repositoryItemCalcEdit1});

            // 
            // gridColumn1
            // 
            gridColumn1.Caption = "Loại giấy";
            //this.gridColumn1.DisplayFormat.FormatString = "###";
            //this.gridColumn1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
            //gridColumn1.ColumnEdit = repositoryItemCalcEdit1;
            gridColumn1.FieldName = "LoaiGiay";
            gridColumn1.Name = "gridColumn1";
            gridColumn1.OptionsColumn.AllowEdit = false;
            gridColumn1.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            gridColumn1.Visible = true;
            gridColumn1.VisibleIndex = 1;

            // 
            // gridColumn2
            // 
            gridColumn2.Caption = "SL Chạy";
            gridColumn2.ColumnEdit = repositoryItemCalcEdit1;
            gridColumn2.DisplayFormat.FormatString = "###,###,###0";
            gridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridColumn2.FieldName = "SlChay";
            gridColumn2.Name = "gridColumn2";
            gridColumn2.OptionsColumn.AllowEdit = false;
            gridColumn2.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            gridColumn2.Visible = true;
            gridColumn2.VisibleIndex = 2;

            // 
            // gridColumn3
            // 
            gridColumn3.Caption = "SL Tồn";
            gridColumn3.ColumnEdit = repositoryItemCalcEdit1;
            gridColumn3.DisplayFormat.FormatString = "###,###,###0";
            gridColumn3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridColumn3.FieldName = "SlTon";
            gridColumn3.Name = "gridColumn3";
            gridColumn3.OptionsColumn.AllowEdit = false;
            gridColumn3.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            gridColumn3.Visible = true;
            gridColumn3.VisibleIndex = 3;

            // 
            // gridColumn3
            // 
            gridColumn4.Caption = "Khổ";
            //gridColumn3.DisplayFormat.FormatString = "#########0";
            gridColumn4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
            gridColumn4.FieldName = "Kho";
            gridColumn4.Name = "gridColumn4";
            gridColumn4.OptionsColumn.AllowEdit = false;
            gridColumn4.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            gridColumn4.Visible = true;
            gridColumn4.VisibleIndex = 0;
            gridColumn4.GroupIndex = 0;

            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new GridColumn[] { gridColumn4, gridColumn1, gridColumn2, gridColumn3 });
            gridView1.GridControl = gridControl1;
            gridView1.GroupSummary.AddRange(new GridSummaryItem[] { new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.None, "Kho", null, "") });
            gridView1.Name = "gvTinhGiay";
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.SortInfo.AddRange(new GridColumnSortInfo[] { new GridColumnSortInfo(gridColumn1, DevExpress.Data.ColumnSortOrder.Ascending) });

            ((System.ComponentModel.ISupportInitialize)(gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(repositoryItemCalcEdit1)).EndInit();
            

            LayoutControlItem lci5 = lcMain.AddItem("", gridControl1);
            lci5.Name = "cusGridTinhGiay";

            //chuc nang danh dau hoan thanh
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            // To mau cho grid
            gridView1.RowStyle += new RowStyleEventHandler(gridView1_RowStyle);

            foreach (GridColumn col in gvMain.Columns)
            {
                if (col.Name == "clHT")
                    col.ColumnEdit.EditValueChanged += new EventHandler(HT_EditValueChanged);
            }
            //cHT = _data.FrmMain.Controls.Find("clHT", true)[0] as CheckEdit;
            //cHT.EditValueChanged += new EventHandler(cHT_EditValueChanged);

            //Tô màu ch khổ
            gvMain.Appearance.FocusedRow.BackColor = Color.CadetBlue;
            gvMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(gvMain_CustomDrawCell);
            _data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            //Tăng độ cao dòng của grid
            gvMain.RowHeight = 30;

            _data.BsMain.PositionChanged += new EventHandler(BsMain_PositionChanged);
        }

        private void BsMain_PositionChanged(object sender, EventArgs e)
        {
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            LayoutControlItem lci5 = (LayoutControlItem)lcMain.Items.FindByName("cusGridTinhGiay");
            lci5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0 && gridView1.IsDataRow(e.RowHandle))
            {
                object slChay = gridView1.GetRowCellValue(e.RowHandle, "SlChay");
                object slTon = gridView1.GetRowCellValue(e.RowHandle, "SlTon");
                if (slChay == DBNull.Value || slTon == DBNull.Value)
                    e.Appearance.BackColor = Color.Transparent;
                else
                {
                    if (decimal.Parse(slChay.ToString()) > decimal.Parse(slTon.ToString()))
                        e.Appearance.BackColor = Color.OrangeRed;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                }
            }
        }

        private void btnSXThuTu_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            Sapxep();
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            LayoutControlItem lci5 = (LayoutControlItem)lcMain.Items.FindByName("cusGridTinhGiay");
            lci5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;

            _data.FrmMain.WindowState = FormWindowState.Maximized;
            gridColumn4.GroupIndex = 0;
            gridView1.ExpandAllGroups();

            gridView1.OptionsBehavior.Editable = false;
            gridColumn1.OptionsColumn.AllowEdit = false;
            gridColumn2.OptionsColumn.AllowEdit = false;
            gridColumn3.OptionsColumn.AllowEdit = false;
            gridColumn4.OptionsColumn.AllowEdit = false;
        }

        private void btnTinhGiay_Click(object sender, EventArgs e)
        {
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            LayoutControlItem lci5 = (LayoutControlItem) lcMain.Items.FindByName("cusGridTinhGiay");
            lci5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;

            TinhGiay();
        }

        private void TinhGiay()
        {
            tableTinhGiay.Clear();
            DataSet ds = _data.BsMain.DataSource as DataSet;
            DataRowView drvCurrent = _data.BsMain.Current as DataRowView;
            DataRow[] drsDT = ds.Tables[1].Select("MTKHID = '" + drvCurrent["MTKHID"].ToString() + "'");
            List<string> lstKho = new List<string>();
            foreach (DataRow dr in drsDT)
                if (!lstKho.Contains(dr["ChKho"].ToString().Split('.')[0])) // bo dau cham.
                    lstKho.Add(dr["ChKho"].ToString().Split('.')[0]);

            string sql = "SELECT KyHieu, Kho, SUM(Ton) as Ton from wDMNL2 WHERE KyHieu in ({0}) and Kho = {1} GROUP BY KyHieu, Kho";
            foreach (string drKho in lstKho)
            {
                drsDT = ds.Tables[1].Select("MTKHID = '" + drvCurrent["MTKHID"].ToString() + "' and ChKho = " + drKho);
                //khai bao danh sach nguyen lieu su dung cua kho nay
                Dictionary<string, decimal> lstNL = GetDSNL(drsDT);
                List<string> lstMaNLnew = new List<string>(lstNL.Keys);
                string dsMaKyHieu = "'" + string.Join("','", lstMaNLnew.ToArray()) + "'";
                DataTable dsSLTon = db.GetDataTable(string.Format(sql, dsMaKyHieu, drKho));

                lstMaNLnew.Sort();
                foreach (var manl in lstMaNLnew)
                {
                    var slTon = dsSLTon.Select("KyHieu = '" + manl + "' and Kho = " + drKho);
                    decimal slTonNum = slTon.Length > 0 ? Convert.ToDecimal(slTon[0]["Ton"]) : 0;
                    decimal slChay = Convert.ToDecimal(lstNL[manl].ToString());
                    if (slChay > 0)
                    {
                        DataRow newRow = tableTinhGiay.NewRow();
                        newRow["Kho"] = drKho;
                        newRow["LoaiGiay"] = manl;
                        newRow["SlChay"] = slChay;
                        newRow["SlTon"] = slTonNum;
                        tableTinhGiay.Rows.Add(newRow);
                    }
                }
            }

            LoadToGrid(tableTinhGiay);
        }

        private void LoadToGrid(DataTable tableTinhGiay)
        {
            gridControl1.DataSource = tableTinhGiay;
            gridControl1.RefreshDataSource();
            gridView1.ExpandAllGroups();
        }

        private void btnSapXep_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            drCur = (_data.BsMain.Current as DataRowView).Row;
            SapXepKhoFrom frm = new SapXepKhoFrom(drCur["NoiDungSXTD"].ToString());
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.ShowDialog();
           
            // luu tham so cua form.
            drCur["NoiDungSXTD"] = frm.NgayBD + ";" + frm.NgayKT + ";" + frm.Kho;
            //xu ly sap xep
            if (!string.IsNullOrEmpty(frm.Kho))
            {
                DataSet data = db.GetDataSet(string.Format("EXEC GetDSLSX_Ngay_Kho @TuNgay = '{0}', @DenNgay = '{1}', @CD2 = '', @strKho = '{2}'", frm.NgayBD, frm.NgayKT, frm.Kho));
                if (data.Tables[0].Rows.Count == 0)
                {
                    XtraMessageBox.Show("Không tìm thấy lệnh sản xuất nào để đưa vào kế hoạch",
                      Config.GetValue("PackageName").ToString());
                    return;
                }
                XulyLSX(data.Tables[0], true);
            }
        }

        public string ConvertToUnSign(string s)
        {
            s = s.Replace(Environment.NewLine, String.Empty);
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
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
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;

            if (Config.GetValue("May1Link") == null
                || String.IsNullOrEmpty(Config.GetValue("May1Link").ToString().Trim()))
            {
                XtraMessageBox.Show("Chưa thiết lập tham số đường dẫn xuất file cho máy 1",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            if (Config.GetValue("May2Link") == null
                || String.IsNullOrEmpty(Config.GetValue("May2Link").ToString().Trim()))
            {
                XtraMessageBox.Show("Chưa thiết lập tham số đường dẫn xuất file cho máy 2",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            // kiểm tra máy dc chọn
            if (drMaster["MayS"] == DBNull.Value || string.IsNullOrEmpty(drMaster["MayS"].ToString()))
            {
                XtraMessageBox.Show("Kế hoạch sản xuất này chưa chọn máy sản xuất, vui lòng kiểm tra lại!",
                    Config.GetValue("PackageName").ToString());
                return;
            }

            string path = "";
            if (drMaster["MayS"].ToString().Equals("Dây chuyền 2(1m8)"))
            {
                path = Config.GetValue("May2Link").ToString();
            } else
            {
                path = Config.GetValue("May1Link").ToString();
            }


            if (!Directory.Exists(path))
            {
                XtraMessageBox.Show("Không tìm thấy đường dẫn xuất file: " + path,
                    Config.GetValue("PackageName").ToString());
                return;
            }
            if (File.Exists(path + "\\OfficeH.txt"))
            {
                XtraMessageBox.Show(string.Format("File {0} chưa được xử lý, vui lòng xuất file sau!", path + "\\OfficeH.txt"),
                    Config.GetValue("PackageName").ToString());
                return;
            }
            



            if (Boolean.Parse(drMaster["IsExport"].ToString()))
            {
                XtraMessageBox.Show("Kế hoạch sản xuất này đã xuất file rồi, vui lòng kiểm tra lại!",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            DataSet dsData = _data.BsMain.DataSource as DataSet;
            int i = -1;
            try
            {
                //lấy nội dung file
                using (DataView dvDetail = new DataView(dsData.Tables[1]))
                {
                    dvDetail.RowFilter = string.Format("MTKHID = '{0}'", drMaster["MTKHID"]);
                    dvDetail.Sort = "TKL, Stt";
                    string[] contents = new string[dvDetail.Count];
                    for (i = 0; i < dvDetail.Count; i++)
                        contents[i] = ParseData(dvDetail[i], i + 1);
                    //chép ra file
                    string file = Application.StartupPath + @"\OfficeH.txt";
                    if (File.Exists(file))
                        File.Delete(file);
                    File.WriteAllLines(file, contents);
                    File.Copy(file, path + "\\OfficeH.txt");
                }
                //cập nhật tình trạng của phiếu
                DateTime dtExported = DateTime.Now;
                drMaster["IsExport"] = true;
                drMaster["ExportedDate"] = dtExported;
                drMaster.AcceptChanges();
                db.UpdateByNonQuery(string.Format("update MTKH set IsExport = 1, ExportedDate = '{0}' where MTKHID = '{1}'", dtExported, drMaster["MTKHID"]));
                XtraMessageBox.Show("Đã xuất file thành công!", Config.GetValue("PackageName").ToString());
            }
            catch (Exception ex)
            {
                if (i == -1)
                    XtraMessageBox.Show("Có lỗi phát sinh khi xuất file.\n" + ex.Message);
                else
                    XtraMessageBox.Show(string.Format("Có lỗi phát sinh khi xuất file ở dòng thứ {0}.\n", i + 1) + ex.Message);
            }
        }

        private string GetKyHieuXuatFile(DataRowView drv)
        {
            string error = "ERROR";
            int lop = Convert.ToInt32(drv["Lop"]);
            string ths = drv["THS"].ToString();
            string[] tmp = drv["KyHieu"].ToString().Split('.');
            if (lop == 3)
            {
                if (tmp.Length != 3)
                    return error;
                switch (ths)
                {
                    case "C":
                        return string.Format("{0}--------{1}{2}", tmp[2], tmp[1], tmp[0]);

                    case "B":
                        return string.Format("{0}----{1}{2}", tmp[2], tmp[1], tmp[0]);
                    case "A":
                    case "E":
                        return string.Format("{0}{1}{2}", tmp[2], tmp[1], tmp[0]);
                    default:
                        return error;
                }
            }

            if (lop == 5)
            {
                if (tmp.Length != 5)
                    return error;
                switch (ths)
                {
                    case "C-B":
                    case "B-C":
                        return string.Format("{0}----{1}{2}{3}{4}", tmp[4], tmp[3], tmp[2], tmp[1], tmp[0]);
                    case "A-C":
                    case "C-A":
                    case "C-E":
                    case "E-C":
                        return string.Format("{0}{1}{2}----{3}{4}", tmp[4], tmp[3], tmp[2], tmp[1], tmp[0]);
                    case "A-B":
                    case "B-A":
                    case "B-E":
                    case "E-B":
                    case "E-A":
                    case "A-E":
                        return string.Format("{0}{1}{2}{3}{4}", tmp[4], tmp[3], tmp[2], tmp[1], tmp[0]);
                    default:
                        return error;
                }
            }

            if (lop == 7)
            {
                if (tmp.Length != 7)
                    return error;
                return tmp[6] + tmp[5] + tmp[4] + tmp[3] + tmp[2] + tmp[1] + tmp[0];
            }
            return error;
        }

        private string ParseData(DataRowView drv, int stt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("A".PadRight(4));
            //sb.Append((stt*10).ToString("D5"));
            sb.Append("00000");
            string makh = drv["MaKH"].ToString();
            makh = makh.Length > 10 ? makh.Substring(0, 10) : makh.PadRight(10);
            sb.Append(makh);
            //string tentat = ConvertToUnSign(drv["TenTat"].ToString());
            //tentat = tentat.Length > 20 ? tentat.Substring(0, 20) : tentat.PadRight(20);

            string tentat = new String('-', 20);

            sb.Append(tentat);
            string solsx = drv["SoLSX"].ToString();
            solsx = solsx.Length > 15 ? solsx.Substring(0, 15) : solsx.PadRight(15);
            sb.Append(solsx);
            //string kyhieu = drv["KyHieu"].ToString().Replace(".", string.Empty);
            string kyhieu = GetKyHieuXuatFile(drv);
            kyhieu = kyhieu.Length > 20 ? kyhieu.Substring(0, 20) : kyhieu.PadRight(20);
            sb.Append(kyhieu);
            double chKho = Convert.ToDouble(drv["ChKho"]) * 10;
            sb.Append(chKho.ToString().PadRight(5));
            double chKho1 = Math.Round(chKho * 0.0393700787, 3);
            sb.Append(chKho1.ToString("####.###").PadRight(8));
            double rong = Convert.ToDouble(drv["RongKH"]);
            sb.Append(rong.ToString().PadRight(5));
            double rong1 = Math.Round(rong * 0.0393700787, 3);
            sb.Append(rong1.ToString("####.###").PadRight(8));
            double chDai = Convert.ToDouble(drv["ChDai"]) * 10;
            sb.Append(chDai.ToString().PadRight(5));
            double chDai1 = Math.Round(chDai * 0.0393700787, 3);
            sb.Append(chDai1.ToString("####.###").PadRight(8));
            double soTam = Math.Round(Convert.ToDouble(drv["SoTam"]), 0, MidpointRounding.ToEven);
            sb.Append(soTam.ToString().PadRight(5));
            string ths = drv["THS"].ToString().Replace("-", string.Empty);
            sb.Append(ths.PadRight(4));
            string lcl = string.Empty;

            var lan1Num = Convert.ToDouble(string.IsNullOrEmpty(drv["Lan1"].ToString()) ? "0" : drv["Lan1"].ToString());
            var cao2Num = Convert.ToDouble(string.IsNullOrEmpty(drv["Cao1"].ToString()) ? "0" : drv["Cao1"].ToString());
            var lan2Num = Convert.ToDouble(string.IsNullOrEmpty(drv["Lan2"].ToString()) ? "0" : drv["Lan2"].ToString());

            // làm tròn trên 3 số (lằn, cao lằn)
            lan1Num = Math.Ceiling(lan1Num);
            cao2Num = Math.Ceiling(cao2Num);
            lan2Num = Math.Ceiling(lan2Num);

            List<string> data = new List<string>();

            if (lan1Num != 0d)
            {
                data.Add(lan1Num.ToString());
            }

            if (cao2Num != 0d)
            {
                data.Add(cao2Num.ToString());
            }

            if (lan2Num != 0d)
            {
                data.Add(lan2Num.ToString());
            }

            if (cao2Num == 0d)
            {
                data.Clear();
            }

            if (drv["Loai"].ToString() == "Thùng")
            {
                //lcl = string.Format("{0}+{1}+{2}", Convert.ToDouble(drv["Lan1"]), Convert.ToDouble(drv["Cao1"]), Convert.ToDouble(drv["Lan2"]));
                lcl = data.Count > 0 ? string.Join("+", data.ToArray()) : "";
            }
            else
            {
                //nếu có nhập đủ sl trong lằn cao lằn thì lấy hết
                //còn nhập cao thì chỉ lấy cao(giống như tấm thường)

                //if (Convert.ToBoolean(drv["isXa"]))
                //    lcl = string.Format("{0}", Convert.ToDouble(drv["Cao1"]));
                //if (Convert.ToBoolean(drv["isCL"]))
                //{
                //    //lcl = string.Format("{0}+{1}+{2}", Convert.ToDouble(drv["Lan1"]), Convert.ToDouble(drv["Cao1"]), Convert.ToDouble(drv["Lan2"]));
                //    lcl = data.Count > 0 ? string.Join("+", data.ToArray()) : "";
                //}

                lcl = data.Count == 3 ? string.Join("+", data.ToArray()) : cao2Num != 0d ? cao2Num.ToString() : "";

            }
            sb.Append(lcl.PadRight(50));
            string ghiChu = ConvertToUnSign(drv["GhiChu"].ToString());

            string lsxid = drv["DTLSXID"].ToString();
            string sql = string.Format("SELECT CD2 FROM DTDonHang a  JOIN DTLSX b on a.DTDHID = b.DTDHID WHERE b.DTLSXID = '{0}'", lsxid);
            object cd2 = db.GetValue(sql);

            if (cd2 != null && !Convert.ToBoolean(cd2))
            {
                string tenhang = ConvertToUnSign(drv["TenHang"].ToString());
                ghiChu = "CD2 - " + tenhang + " - " + ghiChu;
            }

            ghiChu = ghiChu.Length > 50 ? ghiChu.Substring(0, 50) : ghiChu.PadRight(50);
            sb.Append(ghiChu);
            sb.Append(Convert.ToDouble(drv["Dao"]));
            string lan = string.Empty, loaiLan = drv["Lan"].ToString();
            if (loaiLan == "+ -")
                lan = "1";
            else if (loaiLan == "+ 0")
                lan = "2";
            else if (loaiLan == "+ - -")
                lan = "3";
            sb.Append(lan.PadRight(4));
            sb.Append(kyhieu.PadRight(20));
            sb.Append(chKho.ToString().PadRight(8));
            sb.Append(chKho.ToString().PadRight(8));
            sb.Append(chKho.ToString().PadRight(8));
            sb.Append(chKho.ToString().PadRight(8));
            sb.Append("".PadRight(30));

            return sb.ToString();
        }

        void gvMain_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string> lstReadOnlyCol = new List<string>(new string[] { "Lop", "MaKH", "TenTat", "TenHang", "SoDH", "QuyCach", "Dai", "Rong", "Cao" });
            if (!lstReadOnlyCol.Contains(gvMain.FocusedColumn.FieldName))
                return;
            if (Boolean.Parse(gvMain.GetFocusedRowCellValue("KCT").ToString()))
                return;
            e.Cancel = true;
        }

        void btnKTKho_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ thêm hoặc sửa phiếu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            DataSet ds = _data.BsMain.DataSource as DataSet;
            DataRowView drvCurrent = _data.BsMain.Current as DataRowView;
            DataRow[] drsDT = ds.Tables[1].Select("MTKHID = '" + drvCurrent["MTKHID"].ToString() + "'");
            //lay danh sach cac kho can tinh
            List<string> lstKho = new List<string>();
            foreach (DataRow dr in drsDT)
                if (!lstKho.Contains(dr["ChKho"].ToString()))
                    lstKho.Add(dr["ChKho"].ToString());
            FrmLstKho frm = new FrmLstKho(lstKho);
            frm.ShowDialog();
            DataTable dtKho = frm.dtKho;
            //tinh khoi luong nguyen lieu cua tung kho
            foreach (DataRow drKho in dtKho.Rows)
            {
                int tt = dtKho.Rows.Count - Convert.ToInt32(drKho["Stt"]);
                string kho = drKho["Kho"].ToString();
                drsDT = ds.Tables[1].Select("MTKHID = '" + drvCurrent["MTKHID"].ToString() + "' and ChKho = " + kho);
                //khai bao danh sach nguyen lieu su dung cua kho nay
                Dictionary<string, decimal> lstNL = GetDSNL(drsDT);

                //tao dong description cho danh sach khoi luong nguyen lieu su dung
                string tkl = "".PadLeft(tt) + "Khổ " + kho + ": ";
                List<string> lstMaNLnew = new List<string>(lstNL.Keys);
                lstMaNLnew.Sort();
                foreach (string maNL in lstMaNLnew)
                    tkl += string.Format("{0} = {1} / ", maNL, lstNL[maNL].ToString("###,###,###"));
                //cap nhat vao DTKH
                foreach (DataRow dr in drsDT)
                    dr["TKL"] = tkl;
            }
            gvMain.RefreshData();
        }

        private Dictionary<string, decimal> GetDSNL(DataRow[] drsDT)
        {
            //khai bao danh sach nguyen lieu su dung cua kho nay
            Dictionary<string, decimal> lstNL = new Dictionary<string, decimal>();
            foreach (DataRow dr in drsDT)
            {
                //lay danh sach nguyen lieu cua dong don hang nay
                string[] lstMaNL = dr["KyHieu"].ToString().Split('.');
                for (int i = 0; i < lstMaNL.Length; i++)
                {
                    string maNL = lstMaNL[i];
                    //lay caption cua cot chua khoi luong (M1 S1 M2 S2 M3 S3 M)
                    int t = i + 1;
                    string cap = (t % 2 == 0) ? "S" : "M";
                    if (t < lstMaNL.Length)
                    {
                        switch (t)
                        {
                            case 1:
                            case 2:
                                cap += "1";
                                break;
                            case 3:
                            case 4:
                                cap += "2";
                                break;
                            case 5:
                            case 6:
                                cap += "3";
                                break;
                        }
                    }
                    decimal kl = Convert.ToDecimal(dr[cap]);
                    //tinh khoi luong dua vao danh sach
                    if (lstNL.ContainsKey(maNL))
                        lstNL[maNL] += kl;
                    else
                        lstNL.Add(maNL, kl);
                }
            }

            return lstNL;
        }

        void reCalc_Spin(object sender, DevExpress.XtraEditors.Controls.SpinEventArgs e)
        {
            e.Handled = true;
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

        void gvMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            GridView gv = sender as GridView;
            if (e.RowHandle <= 0)
                return;

            List<string> lstField = new List<string>(new string[] { "ChKho", "ChDai", "SoTam", "SoMT", "Dao", "Lan1", "Lan2", "Cao", "SLSX", "SoLuong", "Cao1" });
            if (lstField.Contains(e.Column.FieldName))
                return;
            object oKCT = gv.GetRowCellValue(e.RowHandle, "KCT");
            if (oKCT != null && Convert.ToBoolean(oKCT))
            {
                e.Appearance.BackColor = Color.Red;
                e.Appearance.BackColor2 = Color.Red;
                return;
            }

            object ob1 = gv.GetDataRow(e.RowHandle)["ChKho"];
            object ob2 = gv.GetDataRow(e.RowHandle - 1)["ChKho"];
            if (ob1 == DBNull.Value || ob1.ToString() == "" || ob1 == null)
                return;
            if (ob1.ToString().Equals(ob2.ToString()))
                return;

            e.Appearance.ForeColor = Color.Black;
            e.Appearance.BackColor = Color.Yellow;
            e.Appearance.BackColor2 = Color.Yellow;

            if (gv.IsRowSelected(e.RowHandle) || gv.FocusedRowHandle == e.RowHandle)
            {
                e.Appearance.ForeColor = Color.White;
                e.Appearance.BackColor = Color.CadetBlue;
                e.Appearance.BackColor2 = Color.CadetBlue;
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
            if (e.Column.FieldName == "SoLuong" || e.Column.FieldName == "Dao")
            {
                object osl = gvMain.GetFocusedRowCellValue("SoLuong");
                decimal sl = osl == null || osl == DBNull.Value ? 0 : Convert.ToDecimal(osl);
                object oDao = gvMain.GetFocusedRowCellValue("Dao");
                decimal dao = oDao == null || oDao == DBNull.Value ? 0 : Convert.ToDecimal(oDao);
                object oLoai = gvMain.GetFocusedRowCellValue("Loai");
                string loai = oLoai == null || oLoai == DBNull.Value ? "Tấm" : oLoai.ToString();
                decimal soTam;
                if (loai == "Tấm")
                    soTam = sl;
                else
                    soTam = sl / (dao == 0 ? 1 : dao);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoTam"], soTam);
            }
            if (e.Column.FieldName == "SoTam" || e.Column.FieldName == "ChDai")
            {
                object sST = gvMain.GetFocusedRowCellValue("SoTam");
                decimal st = sST == null || sST == DBNull.Value ? 0 : Convert.ToDecimal(sST);
                object oCD = gvMain.GetFocusedRowCellValue("ChDai");
                decimal cd = oCD == null || oCD == DBNull.Value ? 0 : Convert.ToDecimal(oCD);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoMT"], st * cd / 100);
            }
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
            Config.NewKeyValue("@CD2", null);
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1514") as ReportPreview;
            //định dạng thêm cho grid của report
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            StyleFormatCondition hh = new StyleFormatCondition();
            gvDS.FormatConditions.Add(hh);
            hh.Column = gvDS.Columns["SL đã SX"];
            hh.Condition = FormatConditionEnum.NotEqual;
            hh.Value1 = 0;
            hh.Appearance.BackColor = Color.LightCyan;
            hh.ApplyToRow = true;
            gvDS.DataSourceChanged += new EventHandler(gvDS_DataSourceChanged);
            FormatGrid();
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn LSX";
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            frmDS.WindowState = FormWindowState.Maximized;
            frmDS.ShowDialog();
        }

        void FormatGrid()
        {
            for (int i = 0; i < gvDS.VisibleColumns.Count; i++)
            {
                GridColumn gc = gvDS.VisibleColumns[i];
                if (gc.FieldName == "SoLuong")
                    gc.Caption = "SL tồn";
                if (gc.FieldName == "ChKho" || gc.FieldName == "SoLuong")
                {
                    Font f = new Font(gc.AppearanceCell.Font, FontStyle.Bold);
                    gc.AppearanceCell.Font = f;
                }
                if (gc.FieldName == "SoLuong")
                    gc.AppearanceCell.BackColor = Color.Pink;
            }
        }

        void gvDS_DataSourceChanged(object sender, EventArgs e)
        {
            FormatGrid();
        }

        private decimal DanhLaiStt(DataRow dr, decimal n)
        {
            DataRowView drvMaster = _data.BsMain.Current as DataRowView;
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            using (DataView dv = new DataView(dtDTKH))
            {
                dv.RowFilter = string.Format("MTKHID = '{0}'", drvMaster["MTKHID"]);
                dv.Sort = "Stt desc";
                DataRow drPre, drNext;
                decimal chKho = Convert.ToDecimal(dr["ChKho"]);
                for (int i = 0; i < dv.Count; i++)
                {
                    if (Convert.ToDecimal(dv[i]["ChKho"]) == chKho)
                    {
                        drPre = dv[i].Row;
                        if (i == 0)
                            return Convert.ToDecimal(drPre["Stt"]) + 1;
                        drNext = dv[i - 1].Row;
                        return (Convert.ToDecimal(drPre["Stt"]) + Convert.ToDecimal(drNext["Stt"])) / 2;
                    }
                }
            }
            return n;
        }

        private string LayKyHieu(object maNL)
        {
            if (dtKyHieu == null)
            {
                dtKyHieu = db.GetDataTable("select Ma, KyHieu from DMNL");
                dtKyHieu.PrimaryKey = new DataColumn[] { dtKyHieu.Columns["Ma"] };
            }
            DataRow dr = dtKyHieu.Rows.Find(maNL);
            if (dr == null || dr["KyHieu"] == null || dr["KyHieu"] == DBNull.Value)
                return "";
            return dr["KyHieu"].ToString();
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
            XulyLSX(dtDS, false);
        }

        private void Sapxep()
        {
            drCur = (_data.BsMain.Current as DataRowView).Row;
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
          
            if (drCur["NoiDungSXTD"] != DBNull.Value)
            {
                string[] KhoList = drCur["NoiDungSXTD"].ToString().Split(';')[2].Split(',');
                int stt = 100;
                //sap xep theo kho
                foreach (var kho in KhoList)
                {
                    DataRow[] detail = dtDTKH.Select(string.Format("MTKHID = '{0}' and ChKho = {1}", drCur["MTKHID"], kho));
                    foreach (DataRow row in detail)
                    {
                        row["Stt"] = stt;
                    }
                    stt += 100;
                }
                //sap xep theo mat song mat
                foreach (var kho in KhoList)
                {
                    DataRow[] detail = dtDTKH.Select(string.Format("MTKHID = '{0}' and ChKho = {1}", drCur["MTKHID"], kho), "Mat, Song1, Mat1, Song2, Mat2, Song3, Mat3");
                    int Stt = Convert.ToInt32(detail[0]["Stt"]);
                    foreach (DataRow row in detail)
                    {
                        row["Stt"] = Stt++;
                    }
                }
               
            } else
            {
                DataRow[] detail = dtDTKH.Select(string.Format("MTKHID = '{0}'", drCur["MTKHID"]), "ChKho, Mat, Song1, Mat1, Song2, Mat2, Song3, Mat3");

                int stt = 1;
                foreach (DataRow row in detail)
                {
                    row["Stt"] = stt++;
                }
            }
            gvMain.RefreshData();
        }

        private void XulyLSX(DataTable data, bool isFromSapxep)
        {
            DataRow[] drs = isFromSapxep ? data.Select() : data.Select("Chọn = 1");

            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            using (DataTable tmp = dtDTKH.Clone())
            {
                tmp.PrimaryKey = null;
                tmp.Columns["DTKHID"].AllowDBNull = true;
                foreach (DataRow dr in drs)
                {
                    tmp.ImportRow(dr);
                    tmp.Rows[tmp.Rows.Count - 1]["SoDH"] = dr["Số ĐH"];
                }
                bool isEmpty = gvMain.DataRowCount == 0;
                decimal n = gvMain.DataRowCount == 0 ? 0 : (decimal)gvMain.GetRowCellValue(gvMain.DataRowCount - 1, "Stt");
                DataView dv = new DataView(tmp);
                dv.Sort = "ChKho desc";
                foreach (DataRow dr in dv.ToTable().Rows)
                {
                    if (dtDTKH.Select(string.Format("MTKHID = '{0}' and DTLSXID = '{1}'", drCur["MTKHID"], dr["DTLSXID"])).Length > 0)
                        continue;
                    n++;
                    gvMain.AddNewRow();

                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DTKHID"], Guid.NewGuid());
                    if (isEmpty)
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["Stt"], n);
                    else
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["Stt"], DanhLaiStt(dr, n));
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["KCT"], dr["KCT"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["TenHang"], dr["TenHang"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MaKH"], dr["MaKH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["TenTat"], dr["TenTat"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Loai"], dr["Loai"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLSX"], dr["SoLSX"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayLSX"], dr["NgayLSX"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayGH"], dr["NgayGH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Lop"], dr["Lop"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Cao"], dr["Cao"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Dai"], dr["Dai"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Rong"], dr["Rong"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["RongKH"], dr["RongKH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["ChKho"], dr["ChKho"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["THS"], dr["THS"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["GhiChu"], dr["GhiChu"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["QuyCach"], dr["QuyCach"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["NVPT"], dr["NVPT"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Lan1"], dr["Lan1"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Lan2"], dr["Lan1"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Cao1"], dr["Cao1"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["isXa"], dr["isXa"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["isCL"], dr["isCL"]);

                    #region Xử lý phần thông tin tổ hợp sóng (Công sửa ngày 2/9/2015 để lấy thứ tự mã ký hiệu theo tổ hợp sóng)
                    List<string> lstMat = new List<string>();
                    lstMat.Add(dr["Mat1"].ToString());
                    lstMat.Add(dr["Song1"].ToString());
                    lstMat.Add(dr["Mat2"].ToString());
                    lstMat.Add(dr["Song2"].ToString());
                    lstMat.Add(dr["Mat3"].ToString());
                    lstMat.Add(dr["Song3"].ToString());
                    string[] strTHS = XuLyTHS(dr["THS"].ToString());
                    string[] strChonTHS = dr["THS"].ToString().Split('-');
                    dr["Mat1"] = dr["Song1"] = dr["Mat2"] = dr["Song2"] = dr["Mat3"] = dr["Song3"] = "";
                    DataRow drNew = tmp.NewRow();
                    drNew["Mat"] = LayMaNL(dr["Mat"].ToString());
                    drNew["Mat1"] = drNew["Song1"] = drNew["Mat2"] = drNew["Song2"] = drNew["Mat3"] = drNew["Song3"] = "";

                    for (int i = 0; i < strTHS.Length; i++)
                    {
                        strTHS[i] = strTHS[i].ToString().Replace('–', '.');
                        strTHS[i] = strTHS[i].ToString().Replace('-', '.');
                        string[] str = strTHS[i].Split('.');

                        //thay doi vi tri cua dinh luong
                        drNew["MDL" + str[1]] = dr["MDL" + str[0]];
                        drNew["SDL" + str[1]] = dr["SDL" + str[0]];

                        switch (str[0])
                        {
                            case "1":
                                dr["Mat" + str[1]] = lstMat[0];
                                dr["Song" + str[1]] = lstMat[1];
                                drNew["Mat" + str[1]] = ":" + LayMaNL(lstMat[0]);
                                drNew["Song" + str[1]] = ":" + LayMaNL(lstMat[1]);
                                break;
                            case "2":
                                dr["Mat" + str[1]] = lstMat[2];
                                dr["Song" + str[1]] = lstMat[3];
                                drNew["Mat" + str[1]] = ":" + LayMaNL(lstMat[2]);
                                drNew["Song" + str[1]] = ":" + LayMaNL(lstMat[3]);
                                break;
                            case "3":
                                dr["Mat" + str[1]] = lstMat[4];
                                dr["Song" + str[1]] = lstMat[5];
                                drNew["Mat" + str[1]] = ":" + LayMaNL(lstMat[4]);
                                drNew["Song" + str[1]] = ":" + LayMaNL(lstMat[5]);
                                break;
                        }
                    }
                    //Chỉnh sửa gắn lại vị trí sóng
                    int lenChonTHS = 0;
                    for (int i = 1; i < 4; i++)
                    {
                        if (drNew["Mat" + i.ToString()].ToString() == "")
                            continue;
                        drNew["Mat" + i.ToString()] = strChonTHS[lenChonTHS] + drNew["Mat" + i.ToString()];
                        drNew["Song" + i.ToString()] = strChonTHS[lenChonTHS] + drNew["Song" + i.ToString()];
                        lenChonTHS++;
                    }
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Mat"], drNew["Mat"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Mat1"], drNew["Mat1"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Song1"], drNew["Song1"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Mat2"], drNew["Mat2"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Song2"], drNew["Song2"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Mat3"], drNew["Mat3"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Song3"], drNew["Song3"]);

                    gvMain.SetFocusedRowCellValue(gvMain.Columns["KyHieu"],
                        (string.IsNullOrEmpty(dr["Mat1"].ToString()) ? "" : LayKyHieu(dr["Mat1"]) + ".") +
                        (string.IsNullOrEmpty(dr["Song1"].ToString()) ? "" : LayKyHieu(dr["Song1"]) + ".") +
                        (string.IsNullOrEmpty(dr["Mat2"].ToString()) ? "" : LayKyHieu(dr["Mat2"]) + ".") +
                        (string.IsNullOrEmpty(dr["Song2"].ToString()) ? "" : LayKyHieu(dr["Song2"]) + ".") +
                        (string.IsNullOrEmpty(dr["Mat3"].ToString()) ? "" : LayKyHieu(dr["Mat3"]) + ".") +
                        (string.IsNullOrEmpty(dr["Song3"].ToString()) ? "" : LayKyHieu(dr["Song3"]) + ".") +
                        LayKyHieu(dr["Mat"]));

                    #endregion

                    gvMain.SetFocusedRowCellValue(gvMain.Columns["MDL"], dr["MDL"]);
                    if (!string.IsNullOrEmpty(drNew["Mat1"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["MDL1"], drNew["MDL1"]);
                    if (!string.IsNullOrEmpty(drNew["Song1"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["SDL1"], drNew["SDL1"]);
                    if (!string.IsNullOrEmpty(drNew["Mat2"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["MDL2"], drNew["MDL2"]);
                    if (!string.IsNullOrEmpty(drNew["Song2"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["SDL2"], drNew["SDL2"]);
                    if (!string.IsNullOrEmpty(drNew["Mat3"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["MDL3"], drNew["MDL3"]);
                    if (!string.IsNullOrEmpty(drNew["Song3"].ToString()))
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["SDL3"], drNew["SDL3"]);

                    gvMain.SetFocusedRowCellValue(gvMain.Columns["SoDH"], dr["SoDH"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["GHIM"], dr["Ghim"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Dan"], dr["Dan"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["DoPhu"], dr["DoPhu"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["Lan"], dr["Lan"]);
                    gvMain.SetFocusedRowCellValue(gvMain.Columns["HT"], false);
                    //bổ sung để không bị mất ch dài, dao... khi có group
                    DataRow drCurrent = gvMain.GetDataRow(gvMain.FocusedRowHandle);
                    if (drCurrent == null)
                    {
                        //đưa 2 lệnh set ID này vào đây để tăng tốc xử lý
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["MTKHID"], drCur["MTKHID"]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["DTLSXID"], dr["DTLSXID"]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["Dao"], dr["Dao"]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["ChDai"], dr["ChDai"]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["SLSX"], dr["SLSX"]);
                        gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLuong"], dr["SoLuong"]);
                        gvMain.UpdateCurrentRow();
                    }
                    else
                    {
                        //đưa 2 lệnh set ID này vào đây để tăng tốc xử lý
                        drCurrent["MTKHID"] = drCur["MTKHID"];
                        drCurrent["DTLSXID"] = dr["DTLSXID"];
                        drCurrent["Dao"] = dr["Dao"];
                        drCurrent["ChDai"] = dr["ChDai"];
                        drCurrent["SLSX"] = dr["SLSX"];
                        drCurrent["SoLuong"] = dr["SoLuong"];

                        decimal soTam;
                        decimal dao = Convert.ToDecimal(dr["Dao"] != DBNull.Value ? dr["Dao"] : "0");
                        if (dr["Loai"].ToString() == "Tấm")
                            soTam = Convert.ToDecimal(dr["SoLuong"]);
                        else
                            soTam = Convert.ToDecimal(dr["SoLuong"]) / (dao == 0 ? 1 : dao);
                        drCurrent["SoTam"] = soTam;
                        object oCD = dr["ChDai"];
                        decimal cd = oCD == null || oCD == DBNull.Value ? 0 : Convert.ToDecimal(oCD);
                        drCurrent["SoMT"] = soTam * cd / 100;
                        //drCurrent["DienTich"] = (Convert.ToDecimal(drCurrent["ChDai"]) * Convert.ToDecimal(drCurrent["ChKho"]) * soTam) / 10000;
                    }
                }
            }
            Sapxep();
        }


        private string LayMaNL(string manl)
        {
            string[] tmp = manl.Split('.');
            if (tmp.Length != 5)
                return "";
            return (tmp[2] + "." + tmp[3]);
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

        //Xử lý tổ hợp sóng
        private string[] XuLyTHS(string ths)
        {
            string[] strKQ = null;
            //Tạo công thức
            if (dtTHS == null)
                dtTHS = db.GetDataTable("select Ma, XuLy from DMTHS");
            //List<string> lstTHS = new List<string>();
            //    //3 lớp
            //lstTHS.Add("C"); lstTHS.Add("1-1");
            //lstTHS.Add("B"); lstTHS.Add("1-3");
            //lstTHS.Add("A"); lstTHS.Add("1-2");
            //lstTHS.Add("E"); lstTHS.Add("1-2");

            //lstTHS.Add("C-B"); lstTHS.Add("1-3;2-1");
            //lstTHS.Add("B-A"); lstTHS.Add("1-2;2-3");
            //lstTHS.Add("B-E"); lstTHS.Add("1-2;2-3");
            //lstTHS.Add("C-E"); lstTHS.Add("1-2;2-1");
            //lstTHS.Add("C-C"); lstTHS.Add("1-2;2-1");
            //lstTHS.Add("C-A"); lstTHS.Add("1-2;2-1");
            ////old
            //lstTHS.Add("A-B"); lstTHS.Add("1-3;2-2");
            //lstTHS.Add("B-E"); lstTHS.Add("1-3;2-2");
            //lstTHS.Add("C-E"); lstTHS.Add("1-3;2-1");
            //lstTHS.Add("A-E"); lstTHS.Add("1-3;2-2");
            //lstTHS.Add("A-C"); lstTHS.Add("1-3;2-1");
            //    //7 lớp
            //lstTHS.Add("C-C-B"); lstTHS.Add("1-3;2-1;3-2");
            //lstTHS.Add("C-C-E"); lstTHS.Add("1-3;2-1;3-2");
            //lstTHS.Add("C-B-E"); lstTHS.Add("1-2;2-3;3-1");
            //lstTHS.Add("C-A-B"); lstTHS.Add("1-3;2-1;3-2");
            //lstTHS.Add("C-B-C"); lstTHS.Add("1-2;2-3;3-1");
            //lstTHS.Add("C-B-A"); lstTHS.Add("1-2;2-3;3-1");
            DataRow[] drs = dtTHS.Select("Ma = '" + ths + "'");
            if (drs.Length > 0)
                strKQ = drs[0]["XuLy"].ToString().Split(';');
            return strKQ;

        }


    }

}
