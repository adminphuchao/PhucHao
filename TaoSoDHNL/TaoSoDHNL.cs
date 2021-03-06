using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Plugins;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;

namespace TaoSoDHNL
{
    public class TaoSoDHNL:ICControl
    {
        Database db = Database.NewDataDatabase();
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        #region ICControl Members

        public void AddEvent()
        {
            if ("MT41,MT45".Contains(_data.DrTableMaster["TableName"].ToString()))
            {
                _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
                BsMain_DataSourceChanged(_data.BsMain, new EventArgs());
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            if (_data.BsMain == null)
                return;
            DataTable mtDH = (_data.BsMain.DataSource as DataSet).Tables[0];
            mtDH.ColumnChanged += new DataColumnChangeEventHandler(mtDT_ColumnChanged);
        }

        void mtDT_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            List<string> lstFields = new List<string>(new string[] { "MaNCC", "NgayCT" });
            if (lstFields.Contains(e.Column.ColumnName))
            {
                string mancc = e.Row["MaNCC"].ToString();
                string ngayct = e.Row["NgayCT"].ToString();
                if (mancc == "" || ngayct == "")
                    e.Row["SoDH"] = DBNull.Value;
                else
                {
                    DateTime ngay = DateTime.Parse(ngayct);
                    string s = "select count(*) from " + _data.DrTableMaster["TableName"].ToString() + " where MaNCC = '{0}' and year(NgayCT) = {1}";
                    object o = db.GetValue(string.Format(s, mancc, ngay.Year));
                    int sln = (o == null || o.ToString() == "") ? 1 : int.Parse(o.ToString()) + 1;
                    s += " and month(NgayCT) = {2}";
                    o = db.GetValue(string.Format(s, mancc, ngay.Year, ngay.Month));
                    int slt = (o == null || o.ToString() == "") ? 1 : int.Parse(o.ToString()) + 1;
                    e.Row["SoDH"] = sln.ToString() + "PH" + " - " + mancc + slt.ToString() + "/" + ngay.Month.ToString("D2")+ "/" + ngay.Year.ToString();
                }
                e.Row.EndEdit();
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
