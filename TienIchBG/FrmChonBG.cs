using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using DevExpress.XtraGrid.Columns;

namespace TienIchBG
{
    public partial class FrmChonBG : DevExpress.XtraEditors.XtraForm
    {
        public DataTable dtDSBG;
        Database db = Database.NewDataDatabase();

        public FrmChonBG()
        {
            InitializeComponent();
        }

        private void FrmChonBG_Load(object sender, EventArgs e)
        {
            string s = @"select mt.SoBG, Chon = cast(0 as bit), mt.NgayCT, mt.NgayHH, mt.MaKH, dt.* 
                        from MTBaoGia mt inner join DTBaoGia dt on mt.MTBGID = dt.MTBGID
                        where mt.Duyet = 1 and mt.Huy = 0 and mt.HetHan = 0";
            dtDSBG = db.GetDataTable(s);
            if (dtDSBG.Rows.Count == 0)
            {
                XtraMessageBox.Show("Không có báo giá phù hợp", Config.GetValue("PackageName").ToString());
                this.Close();
            }
            gcMain.DataSource = dtDSBG;
            List<string> lstField = new List<string>(new string[] {"SOBG", "CHON", "NGAYCT", "NGAYHH", "MAKH", "TENHANG", "LOP", "SOLUONG",
                "SOMAU", "DAI", "RONG", "CAO", "GIABAN", "GHICHU"});
            List<string> lstCaption = new List<string>(new string[] {"Số", "Chọn", "Ngày", "Ngày hết hạn", "Khách hàng", "Tên hàng", "Số lớp", "Số lượng",
                "Số màu", "Dài", "Rộng", "Cao", "Giá bán", "Ghi chú"});
            foreach (GridColumn gc in gvMain.Columns)
                if (lstField.Contains(gc.FieldName.ToUpper()))
                {
                    gc.Caption = lstCaption[lstField.IndexOf(gc.FieldName.ToUpper())];
                    if (gc.ColumnType == typeof(Decimal))
                    {
                        gc.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gc.DisplayFormat.FormatString = "###,###.###";
                    }
                    if (gc.FieldName != "Chon")
                        gc.OptionsColumn.AllowEdit = false;
                }
                else
                    gc.Visible = false;
            gvMain.Columns["MaKH"].Group();
            gvMain.Columns["SoBG"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            gvMain.ExpandAllGroups();
            gvMain.BestFitColumns();
        }

        private void FrmChonBG_FormClosing(object sender, FormClosingEventArgs e)
        {
            gvMain.MoveFirst();
        }


    }
}