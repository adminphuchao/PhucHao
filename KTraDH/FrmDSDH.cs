using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using DevExpress.XtraGrid.Columns;

namespace KTraDH
{
    public partial class FrmDSDH : DevExpress.XtraEditors.XtraForm
    {
        Database db = Database.NewDataDatabase();
        public FrmDSDH(DataTable dt)
        {
            InitializeComponent();
            gcDSDH.DataSource = dt;
        }

        private void FrmDSDH_Load(object sender, EventArgs e)
        {
            List<string> lstField = new List<string>(new string[] {"SODH", "NGAYCT", "TENHANG", "LOP", "SOLUONG",
                "SOMAU", "DAI", "RONG", "CAO", "GIABAN", "THANHTIEN", "GHICHU"});
            List<string> lstCaption = new List<string>(new string[] {"Số", "Ngày", "Tên hàng", "Số lớp", "Số lượng",
                "Số màu", "Dài", "Rộng", "Cao", "Giá bán", "Thành tiền", "Ghi chú"});
            foreach (GridColumn gc in gvDSDH.Columns)
                if (lstField.Contains(gc.FieldName.ToUpper()))
                {
                    gc.Caption = lstCaption[lstField.IndexOf(gc.FieldName.ToUpper())];
                    if (gc.ColumnType == typeof(Decimal))
                    {
                        gc.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gc.DisplayFormat.FormatString = "###,###.###";
                    }
                    if (gc.ColumnType == typeof(DateTime))
                    {
                        gc.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        gc.DisplayFormat.FormatString = "dd/MM/yyyy";
                    }
                }
                else
                    gc.Visible = false;
            gvDSDH.Columns["SoDH"].Group();
            gvDSDH.Columns["SoDH"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            gvDSDH.ExpandAllGroups();
            gvDSDH.BestFitColumns();
        }
    }
}