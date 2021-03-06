using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace TaoSoLSX
{
    public class TaoSoLSX : ICData
    {
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {

        }

        public void UpdateDonHang()
        {
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.Deleted | DataViewRowState.ModifiedCurrent;
            foreach (DataRowView drv in dv)
            {
                string dhid = drv["DTDHID"].ToString();
                switch (drv.Row.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                        string slsx = "update DTDonHang set TinhTrang = N'{0}' where DTDHID = '{1}'";
                        _data.DbData.UpdateByNonQuery(string.Format(slsx, "LSX", dhid));
                        break;
                    case DataRowState.Deleted:
                        slsx = "update DTDonHang set TinhTrang = null where DTDHID = '{0}'";
                        _data.DbData.UpdateByNonQuery(string.Format(slsx, dhid));
                        break;
                }
            }
        }

        public void ExecuteBefore()
        {
            UpdateDonHang();

            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            DataRow[] drs = _data.DsData.Tables[1].Select("MTLSXID = '" + drCur["MTLSXID"].ToString() + "'");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Chưa có đơn hàng, không thể lưu lệnh sản xuất này",
                    Config.GetValue("PackageName").ToString());
                _info.Result = false;
            }
            if (drCur.RowState != DataRowState.Added)
                return;
            DateTime NgayCT = (DateTime)drCur["NgayCT"];
            // Tháng: 2 chữ số
            // Năm: 2 số cuối của năm
            string Thang = NgayCT.Month.ToString();
            string Nam = NgayCT.Year.ToString();

            if (Thang.Length == 1)
                Thang = "0" + Thang;
            Nam = Nam.Substring(2, 2);

            string suffix = "-" + Thang + Nam;
            string mact = "LSX", soctNew = string.Empty;

            string sql = string.Format(@" SELECT   Top 1 SoLSX  
                                       FROM     DTLSX
                                       WHERE    SoLSX LIKE '{0}-%-{1}'
                                       ORDER BY SoLSX DESC", mact, suffix);
            DataTable dt = _data.DbData.GetDataTable(sql);
            if (dt.Rows.Count > 0)
                soctNew = dt.Rows[0]["SoLSX"].ToString();
            else
                soctNew = string.Format("{0}-0000-{1}", mact, suffix);
            
            try
            {
                int n = Int32.Parse(soctNew.Split('-')[1]);
                foreach (DataRow dr in drs)
                {
                    n++;
                    dr["SoLSX"] = string.Format("{0}-{1}-{2}", mact, n.ToString("D4"), suffix);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi tạo số lệnh sản xuất\n" + ex.Message,
                    Config.GetValue("PackageName").ToString());
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
