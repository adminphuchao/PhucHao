using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;
using System.Windows.Forms;

namespace NhapNhacNo
{
    public class NhapNhacNo : ICData
    {
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.Single);
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
            DataRow dr = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (dr.RowState == DataRowState.Deleted)
                return;
            bool checkNN = dr.RowState == DataRowState.Added ||
                (dr["HTTT", DataRowVersion.Original] != dr["HTTT"] ||
                dr["HMNo", DataRowVersion.Original] != dr["HMNo"] ||
                dr["TGNo", DataRowVersion.Original] != dr["TGNo"] ||
                dr["Coc", DataRowVersion.Original] != dr["Coc"]);
            if (!checkNN)
            {
                _info.Result = true;
                return;
            }
            switch (dr["HTTT"].ToString())
            {
                case "Hạn mức":
                    if (dr["HMNo"].ToString() == "")
                    {
                        XtraMessageBox.Show("Vui lòng nhập số tiền hạn mức",
                            Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _info.Result = false;
                    }
                    else
                        _info.Result = true;
                    break;
                case "Hóa đơn":
                    if (dr["TGNo"].ToString() == "")
                    {
                        XtraMessageBox.Show("Vui lòng nhập số ngày",
                            Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _info.Result = false;
                    }
                    else
                        _info.Result = true;
                    break;
                case "Theo đợt":
                    if (dr["Coc"].ToString() == "")
                    {
                        XtraMessageBox.Show("Vui lòng nhập số đợt",
                            Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _info.Result = false;
                    }
                    else
                        _info.Result = true;
                    break;
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
