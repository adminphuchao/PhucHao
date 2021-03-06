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

namespace LayNL
{
    public class LayNL : ICControl
    {
        string tb;
        DataRow drCur;
        GridView gvMain;
        ReportPreview frmDS;
        GridView gvDS;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        #region ICControl Members

        public void AddEvent()
        {
            tb = _data.DrTableMaster["TableName"].ToString();
            if ("MT41,MT45".Contains(tb))
            {
                gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
                //thêm nút chọn DH
                LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
                SimpleButton btnChon = new SimpleButton();
                btnChon.Name = "btnChon";
                btnChon.Text = "Chọn nhanh NL";
                LayoutControlItem lci = lcMain.AddItem("", btnChon);
                lci.Name = "cusChon";
                btnChon.Click += new EventHandler(btnChon_Click);
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
            if (tb == "MT41")
            {
                if (drCur["MaNCC"] == DBNull.Value)
                {
                    XtraMessageBox.Show("Vui lòng chọn nhà cung cấp và nhóm hàng trước!",
                        Config.GetValue("PackageName").ToString());
                    return;
                }
                //dùng cách này để truyền tham số vào report
                Config.NewKeyValue("@MaNCC", drCur["MaNCC"]);
            }
            if (drCur["MaNhom"] == DBNull.Value)
            {
                XtraMessageBox.Show("Vui lòng chọn nhóm hàng!",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            //dùng cách này để truyền tham số vào report
            Config.NewKeyValue("@MaNHOM", drCur["MaNhom"]);
            //dùng report 1514 trong sysReport
            frmDS = FormFactory.FormFactory.Create(FormType.Report, "1519") as ReportPreview;
            gvDS = (frmDS.Controls.Find("gridControlReport", true)[0] as GridControl).MainView as GridView;
            //viết xử lý cho nút F4-Xử lý trong report
            SimpleButton btnXuLy = (frmDS.Controls.Find("btnXuLy", true)[0] as SimpleButton);
            btnXuLy.Text = "Chọn nguyên liệu";
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
                XtraMessageBox.Show("Bạn chưa chọn nguyên liệu", Config.GetValue("PackageName").ToString());
                return;
            }
            frmDS.Close();
            //add du lieu vao danh sach
            DataTable dtDTKH = (_data.BsMain.DataSource as DataSet).Tables[1];
            string pk = _data.DrTableMaster["Pk"].ToString();
            foreach (DataRow dr in drs)
            {
                if (dtDTKH.Select(string.Format(pk + " = '{0}' and MaNL = '{1}'", drCur[pk], dr["Ma"])).Length > 0)
                    continue;
                gvMain.AddNewRow();
                gvMain.SetFocusedRowCellValue(gvMain.Columns["MaNL"], dr["Ma"]);
                gvMain.UpdateCurrentRow();
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
