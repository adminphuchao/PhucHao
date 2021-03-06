using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;
using System.Windows.Forms;

namespace KtraThu
{
    public class KtraThu:ICData
    {
        #region ICData Members
        private DataCustomData _data;
        private InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);

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
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            _info.Result = true;
            DataRow[] drs = _data.DsData.Tables[1].Select("MT11ID = '" + drMaster["MT11ID"] + "'");
            //kiem tra chon doi tuong neu co theo doi cong no
            string sql = "select IsCN from DMThu where ID = " + drMaster["LoaiThu"];
            Database db = Database.NewDataDatabase();
            object isCN = db.GetValue(sql);
            if (Convert.ToBoolean(isCN))
            {
                bool rs = true;
                foreach (DataRow dr in drs)
                {
                    if (dr.RowState == DataRowState.Deleted)
                        continue;
                    if (dr["MaKH"] == DBNull.Value)
                    {
                        rs = false;
                        break;
                    }
                }
                _info.Result = rs;
                if (rs == false)
                    XtraMessageBox.Show("Cần chọn khách hàng cho loại thu này (thu công nợ)",
                        Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            //kiem tra chon TK ngan hang neu chuyen khoan
            if (drMaster["HinhThucTT"].ToString() == "Chuyển khoản")
            {
                bool rs = true;
                foreach (DataRow dr in drs)
                {
                    if (dr.RowState == DataRowState.Deleted)
                        continue;
                    if (dr["TaiKhoan"] == DBNull.Value)
                    {
                        rs = false;
                        break;
                    }
                }
                _info.Result = rs;
                if (rs == false)
                    XtraMessageBox.Show("Cần chọn tài khoản ngân hàng đối với hình thức chuyển khoản",
                        Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
