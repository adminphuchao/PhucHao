using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace KTXoa
{
    public class KTXoa : ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        Database dbCDT = Database.NewStructDatabase();
        List<string> lstTable = new List<string>(new string[] { "MT22", "MT23", "MT32", "MTKH" });

         #region ICData Members
  
        public KTXoa()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            
        }
        
        public void ExecuteBefore()
        {
            if (!lstTable.Contains(_data.DrTableMaster["TableName"].ToString()))
                return;
            //Xóa master -> thông báo ko cho xóa-> tiếp tục xóa chi tiết trong master đó -> lỗi curmasterindex =-1 
            if (_data.CurMasterIndex == -1)
            {
                _info.Result = false;
                return;
            }
            DataRow drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            //mt33 mt23 mt32 mt22 mtkh
            //2 truong hop xoa master va xoa chi tiet
            switch (_data.DrTableMaster["TableName"].ToString())
            { 
                case "MT23":
                    if (drCur.RowState == DataRowState.Deleted || drCur.RowState == DataRowState.Modified)
                    {
                        using (DataView dv = new DataView(_data.DsData.Tables[1]))
                        {
                            //if (drCur.RowState == DataRowState.Modified)
                                dv.RowStateFilter = DataViewRowState.Deleted;
                            if (dv.Count == 0)
                                return;
                            string dtids = string.Empty;
                            foreach (DataRowView drv in dv)
                            {
                                dtids += string.Format("'{0}',", drv["dt32id"].ToString());
                            }
                            dtids = dtids.Remove(dtids.Length - 1);

                            DataTable dt33 = _data.DbData.GetDataTable(string.Format("select d.dt33id from dt33 d where d.dt32id in ({0})", dtids));
                            if (dt33.Rows.Count > 0)
                            {
                                XtraMessageBox.Show("Đã xuất hóa đơn, không thể xóa.", Config.GetValue("PackageName").ToString());
                                _info.Result = false;
                                return;
                            }
                        }
                    }
                    break;
                case "MT32":
                    if (drCur.RowState == DataRowState.Deleted || drCur.RowState == DataRowState.Modified)
                    {
                        using (DataView dv = new DataView(_data.DsData.Tables[1]))
                        {
                            //if (drCur.RowState == DataRowState.Modified)
                            dv.RowStateFilter = DataViewRowState.Deleted;
                            if (dv.Count == 0)
                                return;
                            string dtids = string.Empty;
                            foreach (DataRowView drv in dv)
                            {
                                dtids += string.Format("'{0}',", drv["dt32id"].ToString());
                            }
                            dtids = dtids.Remove(dtids.Length - 1);

                            DataTable dt23 = _data.DbData.GetDataTable(string.Format("select d.dt23id from dt23 d where d.dt32id in ({0})", dtids));
                            if (dt23.Rows.Count > 0)
                            {
                                XtraMessageBox.Show("Đã lập phiếu nhập trả, không thể xóa.", Config.GetValue("PackageName").ToString());
                                _info.Result = false;
                                return;
                            }
                            DataTable dt33 = _data.DbData.GetDataTable(string.Format("select d.dt33id from dt33 d where d.dt32id in ({0})", dtids));
                            if (dt33.Rows.Count > 0)
                            {
                                XtraMessageBox.Show("Đã xuất hóa đơn, không thể xóa.", Config.GetValue("PackageName").ToString());
                                _info.Result = false;
                                return;
                            }
                        }
                    }
                    break;
                case "MT22":
                    if (drCur.RowState == DataRowState.Deleted || drCur.RowState == DataRowState.Modified)
                    {
                        using (DataView dv = new DataView(_data.DsData.Tables[1]))
                        {
                            //if (drCur.RowState == DataRowState.Modified)
                                dv.RowStateFilter = DataViewRowState.Deleted;
                            if (dv.Count == 0)
                                return;
                            string dtids = string.Empty;
                            foreach (DataRowView drv in dv)
                            {
                                dtids += string.Format("'{0}',", drv["dtdhid"].ToString());
                            }
                            dtids = dtids.Remove(dtids.Length - 1);

                            DataTable dt32 = _data.DbData.GetDataTable(string.Format("select d.dt32id from dt32 d where d.dtdhid in ({0})", dtids));
                            if (dt32.Rows.Count > 0)
                            {
                                XtraMessageBox.Show("Đã lập phiếu bán hàng, không thể xóa.", Config.GetValue("PackageName").ToString());
                                _info.Result = false;
                                return;
                            }
                        }
                    }
                    break;
                case "MTKH":
                    if (drCur.RowState == DataRowState.Deleted || drCur.RowState == DataRowState.Modified)
                    {
                        using (DataView dv = new DataView(_data.DsData.Tables[1]))
                        {
                            //if (drCur.RowState == DataRowState.Modified)
                                dv.RowStateFilter = DataViewRowState.Deleted;
                            if (dv.Count == 0)
                                return;
                            string dtids = string.Empty;
                            foreach (DataRowView drv in dv)
                            {
                                dtids += string.Format("'{0}',", drv["DTLSXID"].ToString());
                            }
                            dtids = dtids.Remove(dtids.Length - 1);

                            DataTable dt22 = _data.DbData.GetDataTable(string.Format("select d.dt22id from dt22 d inner join dtlsx l on d.dtdhid = l.dtdhid where l.dtlsxid in ({0})", dtids));
                            if (dt22.Rows.Count > 0)
                            {
                                XtraMessageBox.Show("Đã lập phiếu nhập, không thể xóa.", Config.GetValue("PackageName").ToString());
                                _info.Result = false;
                                return;
                            }
                        }
                    }
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
