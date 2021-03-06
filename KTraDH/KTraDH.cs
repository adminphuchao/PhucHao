using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;
using System.Windows.Forms;

namespace KTraDH
{
    public class KTraDH : ICData
    {
        DataTable dtNL;
        //List<string> lstNL = new List<string>(new string[] { "Mat_", "SB_", "MB_", "SC_", "MC_", "SE_", "ME_" });
        DataRow drCur;
        string tableName;
        List<string> lstTB = new List<string>(new string[] { "MTDonHang", "MTLSX" });
        List<string> lstPk = new List<string>(new string[] { "MTDHID", "MTLSXID" });
        DataTable dtDMNL;
        Database db = Database.NewDataDatabase();
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

        public void ExecuteBefore()
        {
            tableName = _data.DrTableMaster["TableName"].ToString();
            if (!lstTB.Contains(tableName))
                return;
            drCur = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drCur.RowState == DataRowState.Deleted)
                return;

            if (tableName == "MTDonHang")
            {
                string pk = lstPk[lstTB.IndexOf(tableName)];
                string pkValue = drCur[pk].ToString();
                DataTable dt = _data.DsData.Tables[1];
                DataRow[] drs = dt.Select(pk + " = '" + pkValue + "'");
                foreach (DataRow dr in drs)
                {
                    if (Convert.ToBoolean(dr["isXa"]) & Convert.ToBoolean(dr["isCL"]))
                    {
                        XtraMessageBox.Show("Mặt hàng " + dr["TenHang"].ToString() + " không thể đánh dấu cả 'Xả' và 'Cán lằn'!",
                            Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                    if (Convert.ToBoolean(dr["isXa"]))
                    {
                        if (dr["XaX"].ToString() == "" || dr["DaoX"].ToString() == "")
                        {
                            XtraMessageBox.Show("Mặt hàng " + dr["TenHang"].ToString() + " đánh dấu 'Xả' cần nhập Dao và Xả!",
                                Config.GetValue("PackageName").ToString());
                            _info.Result = false;
                            return;
                        }
                    }
                    if (Convert.ToBoolean(dr["isCL"]))
                    {
                        if (dr["DaiCL"].ToString() == ""
                            || dr["RongCL"].ToString() == ""
                            || dr["CaoCL"].ToString() == ""
                            || dr["LopCL"].ToString() == ""
                            || dr["Lan"].ToString() == "")
                        {
                            XtraMessageBox.Show("Mặt hàng " + dr["TenHang"].ToString() + " đánh dấu 'Cán lằn' cần nhập quy cách thùng và loại lằn!",
                                Config.GetValue("PackageName").ToString());
                            _info.Result = false;
                            return;
                        }
                    }
                    if (dr["Loai"].ToString() == "Thùng" && dr["Lan"].ToString() == "")
                    {
                        XtraMessageBox.Show("Mặt hàng thùng " + dr["TenHang"].ToString() + " cần nhập loại lằn!",
                            Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                }
                //thong bao ve don hang chua san xuat
                string makh = drCur["MaKH"].ToString();
                //ngày 22/4/2015: thêm điều kiện ngày chứng từ > năm 2014 để loại những đơn hàng cũ mà người dùng chưa nhập đúng số liệu mt.LSX
                string s = @"select mt.SoDH, mt.NgayCT, dt.* 
                        from MTDonHang mt inner join DTDonHang dt on mt.MTDHID = dt.MTDHID
                        where mt.NgayCT >= '1/1/2015' and (mt.LSX is null or dt.TinhTrang is null) and mt.MaKH = '" + makh + "' and mt.MTDHID <> '" + drCur["MTDHID"].ToString() + "'";
                DataTable dtDSDH = db.GetDataTable(s);
  
                if (dtDSDH.Rows.Count > 0)
                {
                    int isShowDH = Config.GetValue("xemDonHang") != null ? Convert.ToInt32(Config.GetValue("xemDonHang").ToString()) : 0;
                    if (isShowDH == 1)
                    {
                        if (XtraMessageBox.Show(string.Format("Khách hàng này có {0} đơn hàng chưa sản xuất\n" +
                       "Bạn có muốn xem không?", dtDSDH.Rows.Count), Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            FrmDSDH frm = new FrmDSDH(dtDSDH);
                            frm.ShowDialog();
                        }
                    }
                }


                //thong bao ve 4 thong so don hang
                string msg = "Giá trị đơn hàng: {0}\nGiá trị đơn hàng chưa sản xuất: {4}\nCông nợ hiện tại: {1}\nHạn mức công nợ: {2}\nTrị giá tồn kho: {3}";
                object m0 = _data.DsData.Tables[1].Compute("sum(ThanhTien)", "MTDHID = '" + drCur["MTDHID"].ToString() + "'");
                decimal d0 = m0 == DBNull.Value ? 0 : decimal.Parse(m0.ToString());
                //cong no khach hang
                s = string.Format(@"    select		w.dt23id,w.mt23id,w.tenhang,w.dvt,w.soluong,w.dongia,w.thue,
		                                            w.mahh,w.soct,w.ngayct,w.makh,w.mact,w.sohd,w.dtdhid,w.dieuchinh,m.ttienhang
		                                            ,sum(w.phatsinh) [phatsinh],sum(w.thanhtoan) [thanhtoan],sum(w.hangtra) [hangtra]
                                        into		#tempPThu
                                        from		wcnpthu w left join mt33 m on w.soct = m.soct
                                        where		w.makh = '{0}'
                                        group by	w.dt23id,w.mt23id,w.tenhang,w.dvt,w.soluong,w.dongia,w.thue,
		                                            w.mahh,w.soct,w.ngayct,w.makh,w.mact,w.sohd,w.dtdhid,w.dieuchinh,m.ttienhang

                                        select sum(PhatSinh + Thue - ThanhToan - HangTra) from #tempPThu where MaKH = '{0}'
                                        drop table  #tempPThu", makh);
                object m1 = db.GetValue(s);
                decimal d1 = m1 == DBNull.Value ? 0 : decimal.Parse(m1.ToString());

                //Giá trị đơn hàng chưa sản xuất:
               // s = @"select sum (dt.ThanhTien) 
                 //       from MTDonHang mt inner join DTDonHang dt on mt.MTDHID = dt.MTDHID
                 //       where mt.LSX is null and mt.MaKH = '" + makh + "' and mt.MTDHID = '" + drCur["MTDHID"].ToString() + "'";

                s = @"select sum (dt.ThanhTien)
                        from MTDonHang mt inner join DTDonHang dt on mt.MTDHID = dt.MTDHID
                        where mt.NgayCT >= '1/1/2015' and (mt.LSX is null or dt.TinhTrang is null) and mt.MaKH = '" + makh + "' and mt.MTDHID <> '" + drCur["MTDHID"].ToString() + "'";

                object m4 = db.GetValue(s);
                decimal d4 = m4 == DBNull.Value ? 0 : decimal.Parse(m4.ToString());

                //han muc cong no
                s = string.Format(@"--chuẩn bị điều kiện tháng năm
                                    declare @m int,@y int, @m1 int, @y1 int
                                    set @m = month('{0}')
                                    set @y = year('{0}')
                                    if (@m = 1)
                                    begin
	                                    set @m1 = 12
	                                    set @y1 = @y - 1
                                    end
                                    else
                                    begin
	                                    set @m1 = @m - 1
	                                    set @y1 = @y
                                    end

                                    select	sum(PhatSinh) * dm.HMNo/100
                                    from	wCNPThu w inner join DMKH dm on w.MaKH = dm.MaKH
                                    where	dm.HTTT = N'Hạn mức' and month(w.NgayCT) = @m1 and year(w.NgayCT) = @y1
                                            and w.MaKH = '{1}'
									group by w.MaKH, dm.HMNo", drCur["NgayCT"], makh);
                object m2 = db.GetValue(s);
                decimal d2 = (m2 == null || m2 == DBNull.Value) ? 0 : decimal.Parse(m2.ToString());
                //tri gia ton kho
                s = @"select	sum(
		                    (case when b.loai = N'Thùng' then (b.soluong * d.giaban) else (round(b.soluong * d.dai * d.rong/10000,0) * round(d.giaban,0)) end) 
		                    - (case when b.loai = N'Thùng' then (soluong_x * d.giaban) else (round(soluong_x * d.dai * d.rong/10000,0) * round(d.giaban,0)) end) 
		                    - isnull((case when b.loai = N'Thùng' then (slxgp * d.giaban) else (round(slxgp * d.dai * d.rong/10000,0) * round(d.giaban,0)) end), 0))
                    from	blvt b 
                            inner join dtdonhang d on b.dtdhid = d.dtdhid
                            inner join mtdonhang m on m.mtdhid = d.mtdhid 
                    where	m.makh = '" + makh + "' and b.loi = 0";
                object m3 = db.GetValue(s);
                decimal d3 = m3 == DBNull.Value ? 0 : decimal.Parse(m3.ToString());
                XtraMessageBox.Show(string.Format(msg, d0.ToString("###,###,###,##0"), d1.ToString("###,###,###,##0"),
                    d2.ToString("###,###,###,##0"), d3.ToString("###,###,###,##0"), d4.ToString("###,###,###,##0")),
                    Config.GetValue("PackageName").ToString());
            }
            //if (tableName == "MTDonHang")
            //    CapNhatNL(drCur);
            if (!_info.Result)      //khong chon duoc nguyen lieu voi kho giay phu hop
                return;

            int isShowNl = Config.GetValue("xemNguyenLieu") != null ? Convert.ToInt32(Config.GetValue("xemNguyenLieu")) : 0;
            if (isShowNl == 1)
            {
                //thong bao ve danh sach nguyen lieu su dung va ton kho
                if (XtraMessageBox.Show("Bạn có muốn xem nguyên liệu sử dụng không?",
                    Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //LẤY SỐ TỒN TRONG VIEW wDMNL2
                    dtDMNL = db.GetDataTable("Select * from wDMNL2");
                    DataTable dtDSNL = LayDSNL(drCur);
                    FrmDSNL frmNL = new FrmDSNL(dtDSNL);
                    frmNL.ShowDialog();
                }
            }

            TaoPhieuXuat();

            if (tableName == "MTLSX")
            {
                SoMT();
            }
            string dienGiai = tableName == "MTDonHang" ? "đơn hàng" : "lệnh sản xuất";
            _info.Result = (XtraMessageBox.Show("Bạn có muốn lưu " + dienGiai + " này không?",
                Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        private void TaoPhieuXuat()
        {
            if (tableName == "MTDonHang")
            {
                string pk = lstPk[lstTB.IndexOf(tableName)];
                string pkValue = drCur[pk].ToString();
                DataTable dt = _data.DsData.Tables[1];
                DataRow[] drs = dt.Select(pk + " = '" + pkValue + "'");

                if (drCur.RowState == DataRowState.Modified
                    && Boolean.Parse(drCur["Duyet", DataRowVersion.Current].ToString()) == false
                    && Boolean.Parse(drCur["Duyet", DataRowVersion.Original].ToString()) == true)
                {
                    //trường hợp bỏ duyệt
                    foreach (DataRow row in drs)
                    {
                        if (!string.IsNullOrEmpty(row["DTDHPSID"].ToString()))
                        {
                            XtraMessageBox.Show("Đơn hàng này đã chọn phôi sóng và lập phiếu xuất phôi sóng, không được quyền bỏ duyệt!",
                           Config.GetValue("PackageName").ToString());
                            _info.Result = false;
                            return;
                        }
                    }
                }

                //trường hợp duyệt
                if (drCur.RowState == DataRowState.Modified
                   && Boolean.Parse(drCur["Duyet", DataRowVersion.Current].ToString()) == true
                   && Boolean.Parse(drCur["Duyet", DataRowVersion.Original].ToString()) == false)
                {
                    // kiểm tra số lượng tồn trước khi duyệt.
                    foreach (DataRow row in drs)
                    {
                        if (!string.IsNullOrEmpty(row["DTDHPSID"].ToString()))
                        {
                            string dtdhpsid = row["DTDHPSID"].ToString();
                            string sql = string.Format("SELECT [SL đã nhập] + [SL hàng trả] - [SL đã xuất] - [SL giấy phế] as [SL tồn] FROM wDTTONTP WHERE dtdhid = '{0}'", dtdhpsid);
                            object slTon = db.GetValue(sql);
                            if (slTon != null)
                            {
                                double soluongT = Convert.ToDouble(slTon);
                                double soluongX = Convert.ToDouble(row["SoLuong"]);
                                double dao = Convert.ToDouble(row["Dao"]);

                                if (soluongT < soluongX * dao)
                                {
                                    XtraMessageBox.Show("Đơn hàng này có số lượng vượt quá số lượng tồn. Vui lòng kiểm tra lại.",
                                    Config.GetValue("PackageName").ToString());
                                    _info.Result = false;
                                    return;
                                }
                            }
                        }
                    }

                    //tạo phiếu xuất kho cho BLVT
                    string insertQuery = @"INSERT INTO BLVT (MaCT,MTID,SoCT,NgayCT,DienGiai,MaKH,TenTat,PsCo,NhomDk,MaHH,DonGia,Soluong_x,MTIDDT,Loai,DTDHID,TenHH)
                                            VALUES ('{0}','{1}' ,'{2}' ,'{3}', N'{4}' ,'{5}' ,'{6}' ,'{7}','{8}' ,'{9}' ,'{10}' ,'{11}','{12}' ,'{13}' ,'{14}','{15}')";

                    string SoCT = drCur["SoDH"].ToString();
                    string NgayCT = drCur["NgayCT"].ToString();
                    string MaCT = "XBH";
                    string mtid = drCur["MTDHID"].ToString();
                    string diengiai = "Xuất phôi sóng từ đơn hàng";
                    string makh = drCur["MaKH"].ToString();
                    string nhomdk = "XDH1";


                    foreach (DataRow row in drs)
                    {
                        if (!string.IsNullOrEmpty(row["DTDHPSID"].ToString()))
                        {
                            string dtdhpsid = row["DTDHPSID"].ToString();
                            double soluongX = double.Parse(row["SoLuong"].ToString()) * double.Parse(row["Dao"].ToString());
                            string mtiddt = row["DTDHID"].ToString();

                            string query = "SELECT * FROM wDTTONTP WHERE dtdhid = '{0}'";
                            DataTable data = db.GetDataTable(string.Format(query, dtdhpsid));
                            if (data.Rows.Count > 0)
                            {
                                var currentRow = data.Rows[0];
                                string mahh = currentRow["mahh"].ToString();
                                string dongia = currentRow["dongia"].ToString();
                                string psCo = (soluongX * Convert.ToDouble(currentRow["dongia"])).ToString();
                                string loai = currentRow["loai"].ToString();
                                string tentat = currentRow["tenkh"].ToString();

                                string dtdhid = dtdhpsid;
                                string tenhh = currentRow["tenhang"].ToString();
                                db.UpdateByNonQuery(string.Format(insertQuery, MaCT, mtid, SoCT, NgayCT, diengiai, makh, tentat, psCo, nhomdk, mahh, dongia, soluongX, mtiddt, loai, dtdhid, tenhh));
                            }
                        }
                    }
                }
            }
        }

        private DataTable LayDSNL(DataRow drCur)
        {
            if (dtNL == null)
            {
                dtNL = db.GetDataTable("select Ma, Ten, MaPhu, Kho from wDMNL");
                dtNL.PrimaryKey = new DataColumn[] { dtNL.Columns["Ma"] };
            }

            DataTable dtDSNL = new DataTable();
            dtDSNL.Columns.Add("Ma", typeof(String));
            dtDSNL.Columns.Add("Ten", typeof(String));
            dtDSNL.Columns.Add("SLSD", typeof(Decimal));
            dtDSNL.Columns.Add("SLTK", typeof(Decimal));
            dtDSNL.Columns.Add("SLCL", typeof(Decimal), "SLTK - SLSD");
            dtDSNL.PrimaryKey = new DataColumn[] { dtDSNL.Columns["Ma"] };

            string pk = lstPk[lstTB.IndexOf(tableName)];
            string pkValue = drCur[pk].ToString();
            DataTable dt = _data.DsData.Tables[1];
            DataRow[] drs = dt.Select(pk + " = '" + pkValue + "'");

            List<string> lstMa = new List<string>();
            foreach (DataRow dr in drs)
            {
                string m = dr["Mat_Giay"].ToString();
                string sb = dr["SB_Giay"].ToString();
                string mb = dr["MB_Giay"].ToString();
                string sc = dr["SC_Giay"].ToString();
                string mc = dr["MC_Giay"].ToString();
                string se = dr["SE_Giay"].ToString();
                string me = dr["ME_Giay"].ToString();
                string mdl = dr["Mat_DL"].ToString();
                string sbdl = dr["SB_DL"].ToString();
                string mbdl = dr["MB_DL"].ToString();
                string scdl = dr["SC_DL"].ToString();
                string mcdl = dr["MC_DL"].ToString();
                string sedl = dr["SE_DL"].ToString();
                string medl = dr["ME_DL"].ToString();
                ThemNL(dtDSNL, dtNL, lstMa, m, mdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, sb, sbdl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, mb, mbdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, sc, scdl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, mc, mcdl, dr, 1);
                ThemNL(dtDSNL, dtNL, lstMa, se, sedl, dr, 1.5M);
                ThemNL(dtDSNL, dtNL, lstMa, me, medl, dr, 1);
            }

            return dtDSNL;
        }

        private void ThemNL(DataTable dtDSNL, DataTable dtNL, List<string> lstMa, string maNL, string dl, DataRow dr, decimal hs)
        {
            if (maNL == "" || dl == "")
                return;
            string sl = dr["SoLuong"].ToString();
            decimal dt;
            if (dr["Loai"].ToString() == "Thùng")
                dt = decimal.Parse(dr["DienTich"].ToString());
            else
                dt = decimal.Parse(dr["Dai"].ToString()) * decimal.Parse(dr["Rong"].ToString()) / 10000;
            decimal d = hs * decimal.Parse(dl) * dt * (sl == "" ? 0 : decimal.Parse(sl)) / 1000;
            if (!lstMa.Contains(maNL))
            {
                DataRow[] row = dtDMNL.Select("Ma='" + maNL.ToString() + "'");
                decimal ton = 0;
                if (row[0]["Ton"] != DBNull.Value)
                    ton = Convert.ToDecimal(row[0]["Ton"].ToString());
                string ten = dtNL.Rows.Find(maNL)["Ten"].ToString();
                dtDSNL.Rows.Add(new object[] { maNL, ten, d, ton });
                lstMa.Add(maNL);
            }
            else
            {
                DataRow drNL = dtDSNL.Rows.Find(maNL);
                drNL["SLSD"] = decimal.Parse(drNL["SLSD"].ToString()) + d;
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        private void SoMT()
        {
            DataTable dv = _data.DsData.Tables[1].GetChanges(DataRowState.Added | DataRowState.Modified);
            if (dv == null || dv.Rows.Count == 0)
                return;
            foreach (DataRow drv in dv.Rows)
            {

                if (drv.RowState == DataRowState.Added)
                {

                    decimal dai = Convert.ToDecimal(drv["Dai"]);
                    decimal rong = Convert.ToDecimal(drv["Rong"]);
                    int lop = Convert.ToInt32(drv["Lop"]);
                    decimal dao = Convert.ToDecimal(drv["Dao"]) == 0 ? 1 : Convert.ToDecimal(drv["Dao"]);
                    decimal soluong = Convert.ToDecimal(drv["SoLuong"]);
                    if (drv["Loai"].ToString() == "Tấm")
                    {
                        //Số mét tới
                        drv["SoMT"] = dai * soluong / dao / 100;
                        //Chiều khổ
                        drv["ChKho"] = drv["Mat_Kho"];
                    }
                    else
                    {
                        //Số mét tới
                        decimal somt = (((dai + rong) * 2 + (lop == 3 ? 4 : 5)) * soluong / dao) / 100;
                        drv["SoMT"] = somt;
                        //Chiều khổ
                        drv["ChKho"] = drv["KhoMax"];
                    }
                }

                if (drv.RowState == DataRowState.Modified)
                {
                    decimal dai1 = Convert.ToDecimal(drv["Dai"]);
                    decimal rong1 = Convert.ToDecimal(drv["Rong"]);
                    int lop1 = Convert.ToInt32(drv["Lop"]);
                    decimal dao1 = Convert.ToDecimal(drv["Dao"]) == 0 ? 1 : Convert.ToDecimal(drv["Dao"]);
                    decimal soluong1 = Convert.ToDecimal(drv["SoLuong"]);
                    string loai1 = drv["Loai"].ToString();
                    decimal dai2 = Convert.ToDecimal(drv["Dai", DataRowVersion.Original]);
                    decimal rong2 = Convert.ToDecimal(drv["Rong", DataRowVersion.Original]);
                    int lop2 = Convert.ToInt32(drv["Lop", DataRowVersion.Original]);
                    decimal dao2 = Convert.ToDecimal(drv["Dao", DataRowVersion.Original]) == 0 ? 1 : Convert.ToDecimal(drv["Dao", DataRowVersion.Original]);
                    decimal soluong2 = Convert.ToDecimal(drv["SoLuong", DataRowVersion.Original]);
                    string loai2 = drv["Loai", DataRowVersion.Original].ToString();
                    decimal khomax1 = Convert.ToDecimal(drv["KhoMax"].ToString() == "" ? "0" : drv["KhoMax"].ToString());
                    decimal khomax2 = Convert.ToDecimal(drv["KhoMax", DataRowVersion.Original].ToString() == "" ? "0" : drv["KhoMax", DataRowVersion.Original].ToString());
                    decimal mat_kho1 = Convert.ToDecimal(drv["Mat_Kho"].ToString() == "" ? "0" : drv["Mat_Kho"].ToString());
                    decimal mat_kho2 = Convert.ToDecimal(drv["Mat_Kho", DataRowVersion.Original].ToString() == "" ? "0" : drv["Mat_Kho", DataRowVersion.Original].ToString());
                    //Số mét tới
                    if (dai1 != dai2 || rong1 != rong2 || lop1 != lop2 || dao1 != dao2 || soluong1 != soluong2 || loai1 != loai2)
                    {
                        if (loai1 == "Tấm")
                        {
                            drv["SoMT"] = dai1 * soluong1 / dao1 / 100;
                        }
                        else
                        {
                            decimal somt1 = (((dai1 + rong1) * 2 + (lop1 == 3 ? 4 : 5)) * soluong1 / dao1) / 100;
                            drv["SoMT"] = somt1;
                        }
                    }
                    //Chiều khổ
                    if (mat_kho1 != mat_kho2 || khomax1 != khomax2 || loai1 != loai2)
                    {
                        if (loai1 == "Tấm")
                            drv["ChKho"] = mat_kho1;
                        else
                            drv["ChKho"] = khomax1;
                    }
                }
                //update kết quả
                DataRow[] dr;
                dr = _data.DsData.Tables[1].Select("DTLSXID = '" + drv["DTLSXID"].ToString() + "'");
                dr[0]["SoMT"] = drv["SoMT"];
                dr[0]["ChKho"] = drv["ChKho"];
            }
        }
        #endregion
    }
}
