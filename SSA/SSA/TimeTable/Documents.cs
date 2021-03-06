﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace StudentSupportApp
{
    public partial class Documents : Form
    {
        internal static class NativeWinAPI
        {
            internal static readonly int GWL_EXSTYLE = -20;
            internal static readonly int WS_EX_COMPOSITED = 0x02000000;

            [DllImport("user32")]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32")]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }

        MainForm parent;
        Connect connection;
        private string sUserID;
        private string[] sLessonInfo;
        ImageList imgList;
        List<string> files = new List<string> { };

        public Documents(MainForm parent, List<string> data)
        {
            this.parent = parent;
            imgList = new ImageList();
            InitializeComponent();
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
            listView1.SmallImageList = imgList;

            if (data.Count > 5)
            {
                sLessonInfo = new string[5];
                //tbxSemNaML.Text 0
                //cbxDIWML.Text 1
                //tbxSubNaML.Text 2
                //tbxSubIDML.Text 3
                //cbxTimeOrderM.Text 4

                for (int i = 0; i < data.Count - 1; i++)
                {
                    this.sLessonInfo[i] = data[i];
                }
                this.sUserID = data[5];
            }
            this.listView1.View = View.Details;
            SetColor(Properties.Settings.Default.Color);
        }

        #region EventHandler
        private void btnAddDoc_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.Filter = "All Files (*.*)|*.*";
                ofd.FileName = "Choose a file";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fileName = ofd.FileName;
                    files.Add(fileName);
                    Icon iconForFile = SystemIcons.WinLogo;

                    var info = new FileInfo(fileName);
                    if ((info.Attributes & FileAttributes.System) != FileAttributes.System)
                    {
                        iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(info.FullName);
                        imgList.Images.Add(info.Extension, iconForFile);
                        this.listView1.Items.Add(new ListViewItem(new string[] { info.Name, info.Extension, info.LastWriteTime.Date.ToShortDateString(), info.FullName }, info.Extension.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
        }

        private void btnOpenDoc_Click(object sender, EventArgs e)
        {
            try
            {
                int index = 0;
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem lvi in listView1.SelectedItems)
                    {
                        foreach (string a in files)
                        {
                            if (a.Contains(lvi.Text))
                            {
                                index = files.IndexOf(a);
                                break;
                            }
                        }
                        System.Diagnostics.Process.Start(files[index]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
        }

        private void btnSaveDoc_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                string sDocs = "";

                foreach (ListViewItem a in listView1.Items)
                {
                    sDocs += a.SubItems[3].Text + "?";
                }

                try
                {
                    connection = new Connect();
                    connection.OpenConnection();
                    string sql = @"UPDATE LESSON
                                    SET DOCUMENTS=N'" + sDocs +
                                 "' WHERE ID_USER='" + sUserID + "' AND DAYINWEEK=" + sLessonInfo[1] +
                                       " AND TIMEORDER=" + sLessonInfo[4] + " AND SUB_ID='" + sLessonInfo[3].Remove(0, 1) +
                                       "' AND SUB_NAME=N'" + sLessonInfo[2] + "' AND SEM_NAME=N'" + sLessonInfo[0] + "'";
                    SqlCommand command = connection.CreateSQLCmd(sql);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Lưu tài liệu thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                    ReportError rp = new ReportError(this, ex);
                    rp.Show();
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
            else
            {
                try
                {
                    connection = new Connect();
                    connection.OpenConnection();
                    string sql = @"UPDATE LESSON
                                    SET DOCUMENTS=null" +
                                 " WHERE ID_USER='" + sUserID + "' AND DAYINWEEK=N'" + sLessonInfo[1] +
                                       "' AND TIMEORDER=" + sLessonInfo[4] + " AND SUB_ID='" + sLessonInfo[3].Remove(0, 1) +
                                       "' AND SUB_NAME=N'" + sLessonInfo[2] + "' AND SEM_NAME=N'" + sLessonInfo[0] + "'";
                    SqlCommand command = connection.CreateSQLCmd(sql);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Lưu tài liệu thành công!");
                }
                catch (Exception a)
                {
                    MessageBox.Show(a.Message);
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
        }

        private void Documents_Load(object sender, EventArgs e)
        {
            string sDocs = "";
            try
            {
                connection = new Connect();
                connection.OpenConnection();
                string sql = @"SELECT DOCUMENTS
                                FROM LESSON" +
                             " WHERE ID_USER='" + sUserID + "' AND DAYINWEEK=N'" + sLessonInfo[1] +
                                   "' AND TIMEORDER=" + sLessonInfo[4] + " AND SUB_ID='" + sLessonInfo[3].Remove(0, 1) +
                                   "' AND SUB_NAME=N'" + sLessonInfo[2] + "' AND SEM_NAME=N'" + sLessonInfo[0] + "'";
                SqlCommand command = connection.CreateSQLCmd(sql);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            sDocs = reader.GetString(0);
                            if (AddItemsToListView(sDocs) == 0)
                                MessageBox.Show("Một số file có thể đã không còn ở nơi lưu trữ ban đầu. Hãy kiểm tra lại!");
                            reader.Close();
                        }
                        else reader.Close();
                    }
                }
                else reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        private int AddItemsToListView(string paths)
        {
            try
            {
                while (paths.Length > 0)
                {
                    string sDocPath = paths.Substring(0, paths.IndexOf("?"));
                    if (File.Exists(sDocPath))
                    {
                        files.Add(sDocPath);
                        Icon iconForFile = SystemIcons.WinLogo;

                        var info = new FileInfo(sDocPath);

                        if ((info.Attributes & FileAttributes.System) != FileAttributes.System)
                        {
                            iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(info.FullName);
                            imgList.Images.Add(info.Extension, iconForFile);
                            this.listView1.Items.Add(new ListViewItem(new string[] { info.Name, info.Extension, info.LastWriteTime.Date.ToShortDateString(), info.FullName }, info.Extension.ToString()));
                        }
                    }
                    else
                    {
                        this.listView1.Items.Add("");
                        return 0;
                    }
                    paths = paths.Remove(0, paths.IndexOf("?") + 1);
                }
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
            return 1;
        }

        private void btnDelDoc_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                const string message = "Bạn chắc chắn muốn xóa các ghi chú đã chọn?";
                const string caption = "Xóa ghi chú";
                var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    foreach (ListViewItem lvi in listView1.SelectedItems)
                    {
                        this.listView1.Items.RemoveAt(lvi.Index);
                    }
                }
            }
        }

        private void btnExitDoc_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        void SetColor(Color x)
        {
            GradientPanelDocuments.BackColor = GradientPanelDocuments.GradientTopLeft = GradientPanelDocuments.GradientTopRight
                = GradientPanelDocuments.GradientBottomLeft = GradientPanelDocuments.GradientBottomRight =
            btnAddDoc.ActiveFillColor = btnAddDoc.ForeColor = btnAddDoc.IdleLineColor = btnAddDoc.IdleForecolor = btnAddDoc.ActiveLineColor =
            btnOpenDoc.ActiveFillColor = btnOpenDoc.ForeColor = btnOpenDoc.IdleLineColor = btnOpenDoc.IdleForecolor = btnOpenDoc.ActiveLineColor =
            btnDelDoc.ActiveFillColor = btnDelDoc.ForeColor = btnDelDoc.IdleLineColor = btnDelDoc.IdleForecolor = btnDelDoc.ActiveLineColor =
            btnExitDoc.ActiveFillColor = btnExitDoc.ForeColor = btnExitDoc.IdleLineColor = btnExitDoc.IdleForecolor = btnExitDoc.ActiveLineColor =
            btnSaveDoc.ActiveFillColor = btnSaveDoc.ForeColor = btnSaveDoc.IdleLineColor = btnSaveDoc.IdleForecolor = btnSaveDoc.ActiveLineColor = x;
        }
    }
}
