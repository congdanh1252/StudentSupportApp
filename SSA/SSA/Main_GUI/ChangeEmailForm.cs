﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace StudentSupportApp
{
    public partial class ChangeEmailForm : Form
    {
        [DllImport("user32")]
        private static extern bool ReleaseCapture();

        [DllImport("user32")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wp, int lp);
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 161, 2, 0);
            }
        }
        internal static class NativeWinAPI
        {
            internal static readonly int GWL_EXSTYLE = -20;
            internal static readonly int WS_EX_COMPOSITED = 0x02000000;

            [DllImport("user32")]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32")]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }

        EmailVerify emailVerify;
        Connect Connection = new Connect();
        MainForm parent;
        string sID, sPasswd;
        public ChangeEmailForm()
        {
            InitializeComponent();
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
        }

        public ChangeEmailForm(MainForm parent, string id)
        {
            InitializeComponent();
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);

            this.parent = parent;
            this.sID = id;
            GetEncodedPasswd();
            SetColor(Properties.Settings.Default.Color);
        }

        private void GetEncodedPasswd()
        {
            try
            {
                string sGetPass = "SELECT PASS FROM USERS WHERE ID_USER = '" + this.sID + "'";
                this.Connection.OpenConnection();
                SqlCommand command = this.Connection.CreateSQLCmd(sGetPass);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.HasRows)
                {
                    if (reader.Read() == false)
                        break;
                    if (reader.GetString(0) != null)
                    {
                        this.sPasswd = reader.GetString(0);
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        private void btnExitEmailF_Click(object sender, EventArgs e)
        {
            this.parent.Show();
            this.Close();
        }

        private void tbxCurPass_OnValueChanged(object sender, EventArgs e)
        {
            tbxCurPass.isPassword = true;
            lbWrongPasswd.Visible = false;
        }

        private int CheckCode()
        {
            if (this.emailVerify.GetCode != tbxChangeMailVC.Text)
                return 0;
            else return 1;
        }

        private int CheckEmail()
        {
            if (!(tbxNewEmail.Text.Contains("@")))
                return 0;
            return 1;
        }

        private void btnSendCodeCE_Click(object sender, EventArgs e)
        {
            try
            {
                USER temp = new USER();
                temp.Email = this.tbxNewEmail.Text;

                if (CheckEmail() == 0)
                {
                    lbInvalidEmailAddr.Visible = true;
                }
                else if (temp.CheckEmail() == 1)
                {
                    this.emailVerify = new EmailVerify(tbxNewEmail.Text);
                    this.emailVerify.GetRandomCode();
                    this.emailVerify.SendMail();
                    lbSentCode.Show();
                }
                else lbUsedEmail.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi, vui lòng liên hệ đội ngũ phát triển!");
                ReportError rp = new ReportError(this, ex);
                rp.Show();
            }
        }

        private void tbxNewEmail_OnValueChanged(object sender, EventArgs e)
        {
            lbInvalidEmailAddr.Visible = false;
            lbUsedEmail.Visible = false;
            lbSentCode.Visible = false;
        }

        private void btnChangeEmailF_Click(object sender, EventArgs e)
        {
            MD5Encoder PasswdEncoder = new MD5Encoder();

            try
            {
                if (tbxCurPass.Text == "" | tbxChangeMailVC.Text == "" | tbxNewEmail.Text == "")
                {
                    MessageBox.Show("vui lòng không để trống thông tin!", "Đổi email");
                }
                else
                {
                    if (PasswdEncoder.FromString(tbxCurPass.Text) == this.sPasswd && CheckCode() == 1)
                    {
                        try
                        {
                            Connection.OpenConnection();

                            string sql = @"UPDATE USERS SET EMAIL ='" + tbxNewEmail.Text + "' WHERE(ID_USER= '" + sID + "')";
                            SqlCommand command = Connection.CreateSQLCmd(sql);
                            command.ExecuteNonQuery();

                            MessageBox.Show("Đổi email thành công!", "Đổi email");
                        }
                        catch (Exception a)
                        {
                            MessageBox.Show(a.Message);
                        }
                        finally
                        {
                            Connection.CloseConnection();
                        }
                    }
                    else if (tbxCurPass.Text != this.sPasswd && CheckCode() == 1)
                    {
                        lbWrongPasswd.Visible = true;
                    }
                    else if (tbxCurPass.Text == this.sPasswd && CheckCode() == 0)
                    {
                        lbWrongVeriCode.Visible = true;
                    }
                    else
                    {
                        lbWrongVeriCode.Visible = true;
                        lbWrongPasswd.Visible = true;
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

        private void tbxCurPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tbxNewEmail.Focus();
        }

        private void tbxNewEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tbxChangeMailVC.Focus();
        }

        private void tbxChangeMailVC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnChangeEmailF_Click(sender, e);
        }

        private void ChangeEmailForm_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void tbxChangeMailVC_OnValueChanged(object sender, EventArgs e)
        {
            lbWrongVeriCode.Visible = false;
        }

        void SetColor(Color x)
        {
            GradientPanelAddDeadline.BackColor =
                GradientPanelAddDeadline.GradientBottomLeft = GradientPanelAddDeadline.GradientBottomRight =
                GradientPanelAddDeadline.GradientTopLeft = GradientPanelAddDeadline.GradientTopRight =

                tbxCurPass.LineFocusedColor = tbxCurPass.LineMouseHoverColor =
                tbxNewEmail.LineFocusedColor = tbxNewEmail.LineMouseHoverColor =
                tbxChangeMailVC.LineFocusedColor = tbxChangeMailVC.LineMouseHoverColor =

                btnSendCodeCE.ActiveFillColor = btnSendCodeCE.ForeColor = btnSendCodeCE.IdleForecolor = btnSendCodeCE.IdleLineColor =
                 btnChangeEmailF.ActiveFillColor = btnChangeEmailF.ForeColor = btnChangeEmailF.IdleForecolor = btnChangeEmailF.IdleLineColor =
                  btnExitEmailF.ActiveFillColor = btnExitEmailF.ForeColor = btnExitEmailF.IdleForecolor = btnExitEmailF.IdleLineColor = x;
        }    
    }
}
