﻿using CDTDatabase;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LayDuLieuKhuyenMai
{
    public partial class DsSanPham : XtraForm
    {
        Database db = Database.NewDataDatabase();
        public BindingSource source = new BindingSource();
        public bool IsView  = true;
        public DsSanPham(bool IsViewMode = false)
        {
            InitializeComponent();
            GetDataSP();
            IsView = IsViewMode;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            if (!IsViewMode)
            {
                gridView1.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
                gridControl1.ProcessGridKey += GridControl1_ProcessGridKey;
            } else
            {
                gridView1.OptionsBehavior.Editable = false;
                btnSave.Visible = false;
                simpleButton2.Visible = false;
            }

            (repositoryItemCalcEdit1 as RepositoryItemCalcEdit).EditMask = "###,###,##0";
            (repositoryItemCalcEdit1 as RepositoryItemCalcEdit).Mask.UseMaskAsDisplayFormat = true;
        }

        private void GridControl1_ProcessGridKey(object sender, KeyEventArgs e)
        {
            var grid = sender as GridControl;
            var view = grid.FocusedView as GridView;
            if (e.KeyData == Keys.Delete)
            {
                view.DeleteSelectedRows();
                e.Handled = true;
            }
        }

        private void GetDataSP()
        {
           
            string sql = string.Format(@"SELECT MaSP, TenSP FROM mDMSP WHERE IsBanSP = 0");
            DataTable dt = db.GetDataTable(sql);

            repositoryItemGridLookUpEdit1.DataSource = dt;
            repositoryItemGridLookUpEdit1.DisplayMember = "TenSP";
            repositoryItemGridLookUpEdit1.ValueMember = "MaSP";
            repositoryItemGridLookUpEdit1.View.OptionsView.ShowAutoFilterRow = true;        }

        private void DsSanPham_Load(object sender, EventArgs e)
        {
            gridControl1.DataSource = source;
            var mainview = gridControl1.MainView as GridView;
            mainview.BestFitColumns();
            if (!IsView)
            {
                mainview.AddNewRow();
            }
               
        }

        private void DsSanPham_FormClosed(object sender, FormClosedEventArgs e)
        {
            gridControl1.RefreshDataSource();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            gridControl1.RefreshDataSource();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            gridControl1.RefreshDataSource();
            this.Close();
        }
    }
}
