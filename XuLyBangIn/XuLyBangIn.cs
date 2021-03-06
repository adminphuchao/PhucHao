using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using CDTDatabase;
using System.Data;
using System.Windows.Forms;
using CDTLib;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace XuLyBangIn
{
    public class XuLyBangIn : ICControl
    {
        Database db = Database.NewDataDatabase();
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.SingleDt);
        PictureEdit peHinh1, peHinh2, peHinh3, peHinh4, peHinh5, peHinh6;
        GridLookUpEdit gluMau1, gluMau2, gluMau3, gluMau4, gluMau5, gluMau6, gluMaKH;
        string sqlLayMau = "select Hinh from DMMS where MaM = '{0}'";
        ZoomPictureEdit peHinhZoom = new ZoomPictureEdit();
        SimpleButton btnXemFile;
        Point _startPoint;

        #region ICControl Members

        public void AddEvent()
        {
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            peHinh1 = _data.FrmMain.Controls.Find("Hinh1", true)[0] as PictureEdit;
            gluMau1 = _data.FrmMain.Controls.Find("Mau1", true)[0] as GridLookUpEdit;
            peHinh2 = _data.FrmMain.Controls.Find("Hinh2", true)[0] as PictureEdit;
            gluMau2 = _data.FrmMain.Controls.Find("Mau2", true)[0] as GridLookUpEdit;
            peHinh3 = _data.FrmMain.Controls.Find("Hinh3", true)[0] as PictureEdit;
            gluMau3 = _data.FrmMain.Controls.Find("Mau3", true)[0] as GridLookUpEdit;
            peHinh4 = _data.FrmMain.Controls.Find("Hinh4", true)[0] as PictureEdit;
            gluMau4 = _data.FrmMain.Controls.Find("Mau4", true)[0] as GridLookUpEdit;
            peHinh5 = _data.FrmMain.Controls.Find("Hinh5", true)[0] as PictureEdit;
            gluMau5 = _data.FrmMain.Controls.Find("Mau5", true)[0] as GridLookUpEdit;
            peHinh6 = _data.FrmMain.Controls.Find("Hinh6", true)[0] as PictureEdit;
            gluMau6 = _data.FrmMain.Controls.Find("Mau6", true)[0] as GridLookUpEdit;
            gluMaKH = _data.FrmMain.Controls.Find("MaKH", true)[0] as GridLookUpEdit;
            peHinh1.Properties.NullText = peHinh2.Properties.NullText = peHinh3.Properties.NullText
             = peHinh4.Properties.NullText = peHinh5.Properties.NullText = peHinh6.Properties.NullText = " ";
            peHinh1.Properties.ShowMenu = peHinh2.Properties.ShowMenu = peHinh3.Properties.ShowMenu
             = peHinh4.Properties.ShowMenu = peHinh5.Properties.ShowMenu = peHinh6.Properties.ShowMenu = false;

            gluMaKH.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMaKH_CloseUp);
            gluMau1.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau1_CloseUp);
            gluMau2.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau2_CloseUp);
            gluMau3.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau3_CloseUp);
            gluMau4.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau4_CloseUp);
            gluMau5.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau5_CloseUp);
            gluMau6.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluMau6_CloseUp);

            //thêm nút Nạp file hình
            SimpleButton btnNapFile = new SimpleButton();
            btnNapFile.Name = "btnNapFile";
            btnNapFile.Text = "Nạp hình ảnh";
            LayoutControlItem lci1 = lcMain.AddItem("", btnNapFile);
            lci1.Name = "cusNapFile";
            btnNapFile.Click += new EventHandler(btnNapFile_Click);

            //thêm nút Xem file hình
            btnXemFile = new SimpleButton();
            btnXemFile.Name = "btnXemFile";
            btnXemFile.Text = "Xem hình ảnh";
            LayoutControlItem lci2 = lcMain.AddItem("", btnXemFile);
            lci2.Name = "cusXemFile";
            btnXemFile.Click += new EventHandler(btnXemFile_Click);

            //custom control cho hình ảnh
            peHinhZoom.Name = "peHinhZoom";
            LayoutControlItem lci = lcMain.AddItem("", peHinhZoom);
            lci.Name = "cusHinhZoom";
            peHinhZoom.Properties.ShowMenu = false;
            peHinhZoom.Properties.Scrollable = true;
            peHinhZoom.DataBindings.Add("EditValue", _data.BsMain, "HinhAnh", true, DataSourceUpdateMode.OnValidation);
            peHinhZoom.EditValueChanged += new EventHandler(peHinhZoom_EditValueChanged);
            peHinhZoom.Properties.MouseDown += new MouseEventHandler(Properties_MouseDown);
            peHinhZoom.Properties.MouseMove += new MouseEventHandler(Properties_MouseMove);
            peHinhZoom.Properties.MouseUp += new MouseEventHandler(Properties_MouseUp);
        }

        void Properties_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Cursor.Current == Cursors.Hand)
                Cursor.Current = Cursors.Default;
        }

        void Properties_MouseMove(object sender, MouseEventArgs e)
        {
            bool isScrollable = peHinhZoom.Properties.Scrollable &&
                (peHinhZoom.HScroll.Visible || peHinhZoom.VScroll.Visible);
            if (e.Button == MouseButtons.Left && isScrollable)
            {
                Point changePoint = new Point(e.Location.X - _startPoint.X,
                                              e.Location.Y - _startPoint.Y);
                if (peHinhZoom.HScroll.Visible)
                {
                    peHinhZoom.HScroll.Value -= changePoint.X;
                    peHinhZoom.Properties.XIndent = peHinhZoom.HScroll.Value;
                    peHinhZoom.Properties.MaximumXIndent = peHinhZoom.HScroll.Maximum;
                }
                if (peHinhZoom.VScroll.Visible)
                {
                    peHinhZoom.VScroll.Value -= changePoint.Y;
                    peHinhZoom.Properties.YIndent = peHinhZoom.VScroll.Value;
                    peHinhZoom.Properties.MaximumYIndent = peHinhZoom.VScroll.Maximum;
                }
                if (isScrollable)
                    peHinhZoom.Refresh();
            }
        }

        void Properties_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _startPoint = e.Location;
                Cursor.Current = Cursors.Hand;
            }
        }

        void peHinhZoom_EditValueChanged(object sender, EventArgs e)
        {
            btnXemFile.Enabled = peHinhZoom.EditValue != null;
            peHinhZoom.Properties.ZoomFactor = 100;

            peHinhZoom.Properties.YIndent = peHinhZoom.Properties.XIndent = 0;
            peHinhZoom.Properties.MaximumYIndent = peHinhZoom.Properties.MaximumXIndent = 0;
            peHinhZoom.Refresh();
        }

        void btnXemFile_Click(object sender, EventArgs e)
        {
            if (peHinhZoom.Image != null)
            {
                try
                {
                    string extension = ".jpg";
                    string fileName = Path.GetRandomFileName();
                    fileName = Path.ChangeExtension(fileName, extension);
                    fileName = Path.Combine(Path.GetTempPath(), fileName);
                    peHinhZoom.Image.Save(fileName);
                    if (File.Exists(fileName))
                        Process.Start(fileName);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi xem tập tin hình ảnh:\n" + ex.Message,
                        Config.GetValue("PackageName").ToString());
                }
            }
        }

        void btnNapFile_Click(object sender, EventArgs e)
        {
            if (gluMaKH.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Vui lòng chọn chế độ Thêm/Sửa dữ liệu",
                        Config.GetValue("PackageName").ToString());
                return;
            }

            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "All Picture Files (*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.ico;*.tif) | *.bmp;*.jpg;*.jpeg;*.png;*.gif;*.ico;*.tif";
            if (f.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image image = Image.FromFile(f.FileName);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, image.RawFormat);
                        peHinhZoom.EditValue = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi nạp tập tin hình ảnh:\n" + ex.Message,
                        Config.GetValue("PackageName").ToString());
                }
            }
        }

        void gluMaKH_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            DataTable dtKH = (gluMaKH.Properties.DataSource as BindingSource).DataSource as DataTable;
            drMaster["NVPT"] = dtKH.Select(string.Format("MaKH = '{0}'", e.Value))[0]["NVPT"];

            object o = db.GetValue("select replace(MaBangIn, MaKH + '-', '') from DMBI where MaKH = '" + e.Value + "' and isnumeric(replace(MaBangIn, MaKH + '-', '')) = 1");
            drMaster["MaBangIn"] = string.Format("{0}-{1}", e.Value, (o == null ? "0001" : (Convert.ToInt32(o) + 1).ToString("D4")));
        }

        void gluMau1_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh1"] = db.GetValue(string.Format(sqlLayMau, e.Value));
        }

        void gluMau2_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh2"] = db.GetValue(string.Format(sqlLayMau, e.Value));
        }

        void gluMau3_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh3"] = db.GetValue(string.Format(sqlLayMau, e.Value));
        }

        void gluMau4_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh4"] = db.GetValue(string.Format(sqlLayMau, e.Value));
        }

        void gluMau5_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh5"] = db.GetValue(string.Format(sqlLayMau, e.Value));
        }

        void gluMau6_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Cancel || e.Value == null || e.Value == DBNull.Value)
                return;
            DataRow drMaster = (_data.BsMain.Current as DataRowView).Row;
            drMaster["Hinh6"] = db.GetValue(string.Format(sqlLayMau, e.Value));
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
