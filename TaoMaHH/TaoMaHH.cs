using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTLib;
using System.Data;
using System.Globalization;
using DevExpress.XtraEditors;


namespace TaoMaHH
{
    public class TaoMaHH : ICData
    {
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        NumberFormatInfo nfi = new NumberFormatInfo();
        //CultureInfo ci = Apli;

        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            using (DataView dv = new DataView(_data.DsData.Tables[1]))
            {
                dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
                string sql = @"if not exists (select MaHH from DMHH where MaHH = @@MaHH)
                                insert into DMHH(MaHH, TenHH, DVT, QuyCach, GiaBan)
                                values(@@MaHH, @@TenHH, @@DVT, @@QuyCach, @@GiaBan)";
                string[] paraNames = new string[] { "@@MaHH", "@@TenHH", "@@DVT", "@@QuyCach", "@@GiaBan" };
                foreach (DataRowView drv in dv)
                {
                    string qc = float.Parse(drv["Dai"].ToString()).ToString() + "*" + float.Parse(drv["Rong"].ToString()).ToString() +
                        (drv["Cao"].ToString() == "" ? "" : "*" + float.Parse(drv["Cao"].ToString()).ToString()) + "_" + drv["Lop"].ToString() + "L";
                    //string qc = (decimal)drv["Dai"] + "*" + (decimal)drv["Rong"] +
                    //  (drv["Cao"] == DBNull.Value ? "" : "*" + (decimal)drv["Cao"]) + "_" + drv["Lop"].ToString() + "L";

                    string mahh = drCur["MaKH"].ToString() + "_" + qc;
                    object[] obj = new object[] { mahh, drv["TenHang"], drv["DVT"], qc, drv["GiaBan"] };

                    //if (!_data.DbData.UpdateByNonQuery(string.Format(sql, mahh, drv["TenHang"], drv["DVT"], qc, drv["GiaBan"].ToString().Replace(",","."))))
                    //    return;
                    if (!_data.DbData.UpdateDatabyPara(sql, paraNames, obj))
                        return;
                }
            }
        }

        public void ExecuteBefore()
        {
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;
            using (DataView dv = new DataView(_data.DsData.Tables[1]))
            {
                dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
                foreach (DataRowView drv in dv)
                {
                    if (drv["Loai"].ToString() == "Thùng"
                        && drv["CD2"].ToString() != "0")
                    {
                        XtraMessageBox.Show("Vui lòng chọn qua công đoạn 2 cho mặt hàng thùng " + drv["TenHang"].ToString(),
                            Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                    if (Boolean.Parse(drv["isIn"].ToString()) == false)
                        continue;
                    if (drv["MaBangIn"].ToString() == string.Empty)
                    {
                        XtraMessageBox.Show("Vui lòng chọn bảng in cho mặt hàng " + drv["TenHang"].ToString(),
                            Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                }
                _info.Result = true;
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
