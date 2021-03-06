using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using CDTControl;
using CDTLib;
using DevExpress.XtraBars;

namespace CDT
{

    public partial class BusinessProcess : DevExpress.XtraEditors.XtraUserControl
    {
        private DataTable dtImages;
        public DataTable dtMenu;
        public Command _cmd;
        public int _sysMenuParent = -1;
        public SysMenu _sysMenu;
        private List<SimpleButton> _lstBtnNV = new List<SimpleButton>(24);
        private List<PictureEdit> _lstImg = new List<PictureEdit>(30);
        private List<LayoutControlItem> _lstLciNV = new List<LayoutControlItem>(24);
        private int _delayPopup = 0;

        public BusinessProcess()
        {
            InitializeComponent();
            AddToList();
            AddEventToButtons();
            this.Load += new EventHandler(BusinessProcess_Load);
        }

        public void BusinessProcess_Load(object sender, EventArgs e)
        {
            if (_sysMenu == null)
                return;
            if (_sysMenuParent == -1)
                lgrNV.TextVisible = false;
            lgrNV.Text = Config.GetValue("Language").ToString() == "0" ? "Quy trình nghiệp vụ" : "Business process";
            lcgDM.Text = Config.GetValue("Language").ToString() == "0" ? "Thiết lập danh mục" : "List of data";
            dtImages = _sysMenu.GetImageProcess(_sysMenuParent);
            dtImages.PrimaryKey = new DataColumn[] { dtImages.Columns["ImageIndex"] };
            VisibleButtons();
            LoadImages();
        }

        private void AddToList()
        {
            _lstBtnNV.AddRange(new SimpleButton[] { btn5, btn6, btn7, btn8, btn9, btn10, btn11, btn12, btn13, btn14, btn15, btn16, btn1, btn2, btn3, btn17, btn18, btn19, btn20, btn21, btn22, btn23, btn24, btn25 });
            _lstLciNV.AddRange(new LayoutControlItem[] { lciNV5, lciNV6, lciNV7, lciNV8, lciNV9, lciNV10, lciNV11, lciNV12, lciNV13, lciNV14, lciNV15, lciNV16, lciNV1, lciNV2, lciNV3, lciNV17, lciNV18, lciNV19, lciNV20, lciNV21, lciNV22, lciNV23, lciNV24, lciNV25 });
            _lstImg.AddRange(new PictureEdit[] { img11, img12, img13, img14, 
                img15, img16, img17, img18, img19, img20, img21, img22, img23, img24, img25, img26, img27, img28, img29, img30, img31, img32, img33, img34, img35, img36, img37, img38, img39, img40});
            for (int i = 0; i < lcgNV.Items.Count; i++)
                if (lcgNV.Items[i].GetType() == typeof(LayoutControlItem))
                    lcgNV.Items[i].Padding.All = 0;
        }

        private void AddEventToButtons()
        {
            foreach (SimpleButton btn in _lstBtnNV)
            {
                btn.Click += new EventHandler(Button_Click);
                btn.MouseEnter += new EventHandler(btn_MouseEnter);
                btn.MouseLeave += new EventHandler(btn_MouseLeave);
            }
        }

        private void Execute(object sender, bool isClick)
        {
            SimpleButton btn = sender as SimpleButton;
            if (btn == null)
                return;
            DataRow dr = btn.Tag as DataRow;
            if (dr == null)
                return;
            Control p = this.Parent;
            if (isClick && p.GetType() == typeof(DevExpress.XtraBars.PopupControlContainer))
            {
                (p as DevExpress.XtraBars.PopupControlContainer).HidePopup();
                _cmd.ExecuteCommand(dr, null);
                return;
            }
            if (this.ParentForm.GetType() == typeof(FrmVisualUI))
            {
                (this.ParentForm as FrmVisualUI).DrCurrent = dr;
                (this.ParentForm as FrmVisualUI).ButtonAction(Control.MousePosition, isClick, null);
            }
        }

        public void ChangeImageStatus()
        {
            if (!_lstImg[0].Properties.ReadOnly)
                SaveImages();
            foreach (PictureEdit img in _lstImg)
            {
                img.Properties.AllowFocused = !img.Properties.AllowFocused;
                img.Properties.ReadOnly = !img.Properties.ReadOnly;
                img.Properties.ShowMenu = !img.Properties.ShowMenu;
            }
        }

        private Image GetImage(byte[] b)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(b);
            if (ms == null)
                return null;
            Image im = Image.FromStream(ms);
            return (im);
        }

        private void LoadImages()
        {
            for (int i = 0; i < _lstImg.Count; i++)
                _lstImg[i].Image = null;
            for (int i = 0; i < dtImages.Rows.Count; i++)
            {
                int n = Int32.Parse(dtImages.Rows[i]["ImageIndex"].ToString());
                _lstImg[n].Image = GetImage(dtImages.Rows[i]["Image"] as byte[]);
            }
        }

        private void SaveImages()
        {
            for (int i = 0; i < _lstImg.Count; i++)
            {
                DataRow drResult = dtImages.Rows.Find(i);
                if (drResult != null)
                {
                    if (_lstImg[i].Image != null && _lstImg[i].IsModified)
                        drResult["Image"] = _lstImg[i].EditValue;   //chua chay dung truong hop sua -> xoa roi them
                    else
                        if (_lstImg[i].Image == null)
                            drResult.Delete();
                }
                else
                {
                    if (_lstImg[i].Image != null)
                    {
                        DataRow drNew = dtImages.NewRow();
                        drNew["ImageIndex"] = i;
                        drNew["Image"] = _lstImg[i].EditValue;
                        drNew["SysMenuParent"] = _sysMenuParent;
                        drNew["SysSiteID"] = CDTLib.Config.GetValue("sysSiteID");
                        dtImages.Rows.Add(drNew);
                    }
                }
            }
            _sysMenu.UpdateImageProcess(_sysMenuParent, dtImages);
        }


        private void VisibleButtons()
        {
            DataView dv = new DataView(dtMenu);
            string order = dtMenu.Columns.Contains("ProcessOrder") ? "ProcessOrder" : "MenuOrder";
            dv.Sort = order;
            if (dtMenu.Columns.Contains("ProcessOrder"))
            {
                dv.RowFilter = "ProcessOrder is not null or ProcessOrder <> ''";
                if (dv.Count == 0)  //tam thoi su dung
                {
                    dv.RowFilter = "UIType = 3 or UIType = 0";
                    order = "MenuOrder";
                }
            }
            else
                dv.RowFilter = "UIType = 3 or UIType = 0";  //Nghiep vu
            for (int i = 0; i < _lstLciNV.Count; i++)
                _lstLciNV[i].Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            foreach (DataRowView drv in dv)
            {
                int n = Int32.Parse(drv[order].ToString()) - 1;
                if (n < 0 || n >= _lstLciNV.Count)
                    continue;
                string exe = Boolean.Parse(Config.GetValue("Admin").ToString()) ? "" : drv["Executable"].ToString();
                _lstLciNV[n].Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                _lstBtnNV[n].Enabled = (exe == "" || Boolean.Parse(exe));
                _lstBtnNV[n].Tag = drv.Row;
                if (drv["Icon"] == DBNull.Value)
                {
                    Font font;
                    if (_lstBtnNV[n].Enabled)
                        font = new Font(_lstBtnNV[n].Font, FontStyle.Bold);
                    else
                        font = new Font(_lstBtnNV[n].Font, FontStyle.Italic);
                    _lstBtnNV[n].Font = font;
                    _lstBtnNV[n].Text = _cmd.CreateHotKey(Config.GetValue("Language").ToString() == "0" ? drv["MenuName"].ToString() : drv["MenuName2"].ToString());
                    _lstLciNV[n].Text = "";
                    _lstLciNV[n].TextVisible = false;
                    _lstBtnNV[n].BackgroundImage = null;
                }
                else
                {
                    _lstBtnNV[n].Text = "";
                    _lstLciNV[n].TextLocation = DevExpress.Utils.Locations.Bottom;
                    _lstLciNV[n].Text = Config.GetValue("Language").ToString() == "0" ? drv["MenuName"].ToString() : drv["MenuName2"].ToString();
                    _lstLciNV[n].TextVisible = true;
                    _lstLciNV[n].TextSize = new Size(28, 14);
                    _lstLciNV[n].AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    _lstLciNV[n].AppearanceItemCaption.ForeColor = Color.DarkRed;
                    _lstLciNV[n].AppearanceItemCaption.Options.UseTextOptions = true;
                    _lstLciNV[n].AppearanceItemCaption.Options.UseFont = true;
                    Font font = new Font(_lstLciNV[n].AppearanceItemCaption.Font, FontStyle.Bold);
                    _lstLciNV[n].AppearanceItemCaption.Font = font;
                    _lstBtnNV[n].StyleController = null;
                    _lstBtnNV[n].Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    _lstBtnNV[n].Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
                    _lstBtnNV[n].Appearance.BackColor = System.Drawing.Color.Transparent;
                    _lstBtnNV[n].LookAndFeel.UseDefaultLookAndFeel = false;
                    _lstBtnNV[n].LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
                    _lstBtnNV[n].BackgroundImage = GetImage(drv["Icon"] as byte[]);
                    _lstBtnNV[n].BackgroundImageLayout = ImageLayout.Zoom;
                }
            }

            dv.RowFilter = "UIType = 1";  //Danh muc
            int m = 15;
            foreach (DataRowView drv in dv)
            {
                if (m >= _lstLciNV.Count)
                    return;
                string exe = Boolean.Parse(Config.GetValue("Admin").ToString()) ? "" : drv["Executable"].ToString();
                _lstLciNV[m].Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                _lstBtnNV[m].Text = _cmd.CreateHotKey(Config.GetValue("Language").ToString() == "0" ? drv["MenuName"].ToString() : drv["MenuName2"].ToString());
                _lstBtnNV[m].Enabled = (exe == "" || Boolean.Parse(exe));
                _lstBtnNV[m].Tag = drv.Row;
                m++;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Execute(sender, true);
        }

        void btn_MouseLeave(object sender, EventArgs e)
        {
            if (this.ParentForm == null || this.ParentForm.GetType() != typeof(FrmVisualUI)
                || !(this.ParentForm as FrmVisualUI).PMenu.Opened)
                return;
            if (!(this.ParentForm as FrmVisualUI).PopupRect.Contains(Control.MousePosition))
                (this.ParentForm as FrmVisualUI).PMenu.HidePopup();
        }

        void btn_MouseEnter(object sender, EventArgs e)
        {
            _delayPopup = 0;
            timer1.Tag = sender;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _delayPopup += 1;
            if (_delayPopup == 5)
            {
                Rectangle r = this.RectangleToScreen((timer1.Tag as Control).Bounds);
                Point p = Control.MousePosition;
                if (r.Contains(p) && (this.TopLevelControl as Form).ActiveMdiChild == this.ParentForm)
                {
                    Execute(timer1.Tag, false);
                }
                _delayPopup = 0;
                timer1.Enabled = false;
            }
        }
    }
}
