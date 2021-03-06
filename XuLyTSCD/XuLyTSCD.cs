using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms;

namespace XuLyTSCD
{
    public class XuLyTSCD : ICData
    {
        private DataCustomData _data;
        private InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
        }

        public void ExecuteBefore()
        {
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)  //kiểm tra không cho xóa tài sản đã tính chi phí khấu hao
            {
                using (DataView dv = new DataView(_data.DsData.Tables[1]))
                {
                    dv.RowStateFilter = DataViewRowState.Deleted;
                    dv.RowFilter = "DaTinhCP = 1";
                    if (dv.Count > 0)
                    {
                        XtraMessageBox.Show("Không được xóa tài sản đã tính chi phí khấu hao!",
                            Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                }
                return;
            }
            drMaster["MaTS"] = TaoMaTS(drMaster);
            _info.Result = KTraDuLieu(drMaster);
            TinhKhauHao(drMaster);
        }

        private bool KTraDuLieu(DataRow dr)
        {
            if (dr["MaTS"].ToString() == "")
            {
                XtraMessageBox.Show("Lỗi khi tạo mã tài sản, không thể lưu dữ liệu",
                    Config.GetValue("PackageName").ToString());
                return false;
            }
            using (DataView dv = new DataView(_data.DsData.Tables[1]))
            {
                dv.RowStateFilter = DataViewRowState.Deleted;
                if (dv.Count > 0)
                {
                    XtraMessageBox.Show("Không được xóa dữ liệu khấu hao!",
                        Config.GetValue("PackageName").ToString());
                    return false;
                }
                else
                {
                    dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
                    dv.RowFilter = "DaTinhCP = 1";
                    if (dv.Count > 0)
                    {
                        XtraMessageBox.Show("Không thể tạm ngưng tháng khấu hao đã tính chi phí!",
                            Config.GetValue("PackageName").ToString());
                        return false;
                    }
                }
            }
            if (dr.RowState == DataRowState.Modified && Convert.ToBoolean(dr["ThanhLy"])
                && dr["ThanhLy"].ToString() != dr["ThanhLy", DataRowVersion.Original].ToString())
                if (XtraMessageBox.Show("Chọn thanh lý tài sản sẽ bị ngưng khấu hao và không đưa vào chi phí\n" +
                    "Bạn có chắc chắn thanh lý tài sản này không?", Config.GetValue("PackageName").ToString(),
                     MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            if (dr.RowState == DataRowState.Modified && Convert.ToBoolean(dr["ThanhLy"])
                && dr["ThanhLy"].ToString() == dr["ThanhLy", DataRowVersion.Original].ToString())
            {
                XtraMessageBox.Show("Tài sản đã thanh lý sẽ không được điều chỉnh thông tin nữa",
                    Config.GetValue("PackageName").ToString());
                return false;
            }
            return true;
        }

        private string TaoMaTS(DataRow dr)
        {
            string oldMa = dr["MaTS"].ToString();
            if (dr.RowState == DataRowState.Modified
                && dr["TenTS"].ToString() == dr["TenTS", DataRowVersion.Original].ToString())
                return oldMa;   //neu khong sua ten thi giu nguyen ma tai san

            //lay tien to la cac chu cai dau cua ten tai san
            string tmp = "";
            string[] s = dr["TenTS"].ToString().Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string s1 in s)
                tmp += s1.Trim()[0];
            tmp = tmp.ToUpper();

            if (dr.RowState == DataRowState.Modified && oldMa.Substring(0, oldMa.Length - 2) == tmp)
                return oldMa;   //neu sua ten nhung ko lam thay doi tiền tố thi van giu nguyen ma tai san

            //lay them hau to la 2 chu so thu tu de phan biet nhung tai san cung ten
            DataRow[] drs = _data.DsData.Tables[0].Select("MaTS like '" + tmp + "%'");
            int t = drs.Length + 1;
            tmp += t.ToString("D2");
            return tmp;
        }

        private void TinhKhauHao(DataRow dr)
        {
            if (dr.RowState == DataRowState.Added)
            {
                DataRow[] drs = _data.DsData.Tables[1].Select("MTID is null");    //xóa detail cho trường hợp copy
                foreach (DataRow drOld in drs)
                    drOld.Delete();
                ThemKH(dr);
            }

            if (dr.RowState == DataRowState.Modified
                && (dr["GTKH"].ToString() != dr["GTKH", DataRowVersion.Original].ToString()
                    || dr["NgayKH"].ToString() != dr["NgayKH", DataRowVersion.Original].ToString()
                    || dr["TGDaKH"].ToString() != dr["TGDaKH", DataRowVersion.Original].ToString()
                    || dr["TGCL"].ToString() != dr["TGCL", DataRowVersion.Original].ToString()))
            {
                //xét trường hợp đã tính chi phí -> thông báo và không cho sửa
                DataRow[] drs = _data.DsData.Tables[1].Select("MTID = " + dr["MTID"] + " and DaTinhCP = 1");
                if (drs.Length > 0)
                {
                    XtraMessageBox.Show("Tài sản này đã tính chi phí khấu hao vào sản xuất\n" +
                        "Không thể thay đổi thông tin khấu hao tài sản", Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    return;
                }
                //tiếp theo sẽ confirm trước khi xóa để tạo lại
                if (XtraMessageBox.Show("Thông tin khấu hao tài sản bị thay đổi\n" +
                    "Bạn có chắc chắn thay đổi không?", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    _info.Result = false;
                    return;
                }
                drs = _data.DsData.Tables[1].Select("MTID = " + dr["MTID"]);
                foreach (DataRow drOld in drs)
                    drOld.Delete();
                ThemKH(dr);
            }
        }

        private void ThemKH(DataRow dr)
        {
            DateTime thang = Convert.ToDateTime(dr["NgayKH"]).AddMonths(Convert.ToInt32(dr["TGDaKH"]));
            thang = new DateTime(thang.Year, thang.Month, 1).AddMonths(1).AddDays(-1);     //lấy đúng ngày cuối tháng
            int tgcl = Convert.ToInt32(dr["TGCL"]);
            for (int i = 0; i < tgcl; i++)
            {
                DataRow drNew = _data.DsData.Tables[1].NewRow();
                drNew["Thang"] = thang.AddMonths(i);
                drNew["SoTien"] = dr["GTKH"];
                _data.DsData.Tables[1].Rows.Add(drNew);
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
