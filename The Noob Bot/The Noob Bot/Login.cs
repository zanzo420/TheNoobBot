﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using nManager;
using nManager.Helpful;
using nManager.Wow.MemoryClass;
using Process = System.Diagnostics.Process;
using Usefuls = nManager.Wow.Helpers.Usefuls;

namespace The_Noob_Bot
{
    public partial class Login : Form
    {
        private const string UpdateCheck = "573-567-555-554-606-605-593";
        private const string KeyNManager = "dfs,kl,se8JDè__fs_vcss454fzdse&é";
        private int _autoStarted = -1;
        private bool _loginInfoProvided = false;

        public Login()
        {
            InitializeComponent();
            InitializeProgram();
            Translate();
        }

        private void SetToolTypeIfNeeded(Control label)
        {
            using (Graphics g = CreateGraphics())
            {
                SizeF size = g.MeasureString(label.Text, label.Font);
                if (size.Width > label.Width)
                {
                    toolTip.SetToolTip(label, label.Text);
                }
            }
        }

        public void AutoStart(int sessId, string productName, string profileName, string battleNet, string wowEmail, string wowPassword, string realmName, string character,
            bool loginInfoProvided)
        {
            _autoStarted = sessId;
            nManagerSetting.AutoStartLoggingInfoProvided = loginInfoProvided;
            if (string.IsNullOrEmpty(productName))
                return;
            nManagerSetting.AutoStartProduct = true;
            nManagerSetting.AutoStartProductName = productName;
            if (string.IsNullOrEmpty(profileName))
                return;
            // following is facultative
            nManagerSetting.AutoStartProfileName = profileName;
            nManagerSetting.AutoStartBattleNet = battleNet;
            nManagerSetting.AutoStartEmail = wowEmail;
            nManagerSetting.AutoStartPassword = wowPassword;
            nManagerSetting.AutoStartRealmName = realmName;
            nManagerSetting.AutoStartCharacter = character;
        }

        private void Translate()
        {
            try
            {
                LangSelection.Items.Clear();
                foreach (string l in Others.GetFilesDirectory(Application.StartupPath + "\\Data\\Lang\\", "*.xml"))
                {
                    Application.DoEvents();
                    LangSelection.Items.Add(l.Remove(l.Length - 1 - 3));
                }

                string langSelected = "English.xml";
                if (Others.ExistFile(Application.StartupPath + "\\Settings\\lang.txt"))
                {
                    string langTemp = Others.ReadFile(Application.StartupPath + "\\Settings\\lang.txt");
                    if (!string.IsNullOrEmpty(langTemp))
                    {
                        if (Others.ExistFile(Application.StartupPath + "\\Data\\Lang\\" + langTemp))
                            langSelected = langTemp;
                    }
                }
                if (!nManager.Translate.Load(langSelected))
                    return;

                LangSelection.Text = langSelected.Remove(langSelected.Length - 1 - 3);
                LangSelection.SelectedIndexChanged += LangSelection_SelectedIndexChanged;

                MainHeader.TitleText = nManager.Translate.Get(nManager.Translate.Id.LoginFormTitle) + @" - " + Information.MainTitle;
                Text = MainHeader.TitleText;
                Identifier.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormDefaultIdentifier);
                Remember.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormRemember);
                SetToolTypeIfNeeded(Remember);
                Register.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormRegister);
                SetToolTypeIfNeeded(Register);
                LoginButton.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormStart);
                SetToolTypeIfNeeded(LoginButton);
                RefreshButton.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormRefresh);
                SetToolTypeIfNeeded(RefreshButton);
                WebsiteLink.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormWebsite);
                SetToolTypeIfNeeded(WebsiteLink);
                ForumLink.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormForum);
                SetToolTypeIfNeeded(ForumLink);
                UseKey.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormUseKey);
                SetToolTypeIfNeeded(UseKey);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void InitializeProgram()
        {
            try
            {
                // File .exe.config
                Process tempsProcess = Process.GetCurrentProcess();
                bool restartBot = !Others.ExistFile(Application.StartupPath + "\\" + tempsProcess.ProcessName + ".exe.config");
                var sw = new StreamWriter(Application.StartupPath + "\\" + tempsProcess.ProcessName + ".exe.config");
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine("<configuration>");
                sw.WriteLine("  <startup>");
                sw.WriteLine("    <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.5\"/>");
                sw.WriteLine("  </startup>");
                sw.WriteLine("  <runtime>");
                sw.WriteLine("    <loadFromRemoteSources enabled=\"true\"/>");
                sw.WriteLine("    <assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">");
                sw.WriteLine("    <probing privatePath=\"products\"/>");
                sw.WriteLine("  </assemblyBinding>");
                sw.WriteLine("  </runtime>");
                sw.WriteLine("</configuration>");
                sw.Close();
                if (restartBot)
                {
                    Process.Start(Application.StartupPath + "\\" + tempsProcess.ProcessName + ".exe");
                    tempsProcess.Kill();
                }

                LangSelection.DropDownStyle = ComboBoxStyle.DropDownList;
                Others.GetVisualStudioRedistribuable2013();
            }
            catch (Exception ex)
            {
                Logging.WriteError("Login > InitializeProgram(): " + ex);
            }
        }

        private void MainFormOnLoad(object sender, EventArgs e)
        {
            try
            {
                if (Others.ExistFile(Application.StartupPath + "\\Settings\\.login"))
                {
                    var strReader = new StreamReader(Application.StartupPath + "\\Settings\\.login", Encoding.Default);
                    try
                    {
                        string text = Others.DecryptString(strReader.ReadLine());
                        string[] text2 = text.Split('#');
                        Identifier.Text = text2[0];
                        Password.Text = text2[1];
                        Identifier.ForeColor = Color.FromArgb(118, 118, 118);
                        Password.ForeColor = Color.FromArgb(118, 118, 118);
                        if (Identifier.Text != "")
                        {
                            Remember.Checked = true;
                            if (Password.Text == "Password")
                            {
                                UseKey.Checked = true;
                                Password.Hide();
                            }
                        }
                    }
                    catch
                    {
                        Identifier.Text = "";
                        Password.Text = "";
                        Remember.Checked = false;
                    }
                    strReader.Close();
                }

                LoginButton.Enabled = false;
                RefreshButton.Enabled = false;
                LoginServer.CheckServerIsOnline();
                while (!LoginServer.IsOnlineserver)
                {
                    Thread.Sleep(30);
                    Application.DoEvents();
                }
                LoginServer.CheckUpdate();
                LoginButton.Enabled = true;
                RefreshButton.Enabled = true;

                RefreshProcessList();

                /* Begin AutoStart code */
                bool sIdFound = false;
                if (_autoStarted > 0)
                {
                    for (int i = 0; i < SessionList.Items.Count; i++)
                    {
                        Application.DoEvents();
                        object item = SessionList.Items[i];
                        if (item.ToString().Contains(_autoStarted + " -"))
                        {
                            SessionList.SelectedIndex = i;
                            sIdFound = true;
                            break;
                        }
                    }
                    if (sIdFound)
                    {
                        nManagerSetting.ActivateProductTipOff = false;
                        LoginButton_Click(new object(), new EventArgs());
                    }
                }
                /*Process.Start("https://goo.gl/TtMsEh");*/
                //else
                //    MessageBox.Show(@"Blizzard is currently very active in the Anti-Bot fight and it may not be wise to use the bot at that very moment in time.", @"WARNING / ATTENTION / ARTUNG / внимание / 注意");
                /* End AutoStart code */
            }
            catch (Exception ex)
            {
                Logging.WriteError("MainFormOnLoad(object sender, EventArgs e): " + ex);
            }
        }

        private bool LoginOnServer()
        {
            try
            {
                if (LoginServer.IsConnected)
                    return true;

                if ((Identifier.Text.Replace(" ", "") != "" && Password.Text.Replace(" ", "") != ""))
                {
                    if (Remember.Checked)
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\Settings\\");
                        var sw = new StreamWriter(Application.StartupPath + "\\Settings\\.login");
                        sw.WriteLine(Others.EncryptString(Identifier.Text.Trim() + "#" + Password.Text.Trim()));
                        sw.Close();
                    }
                    else
                    {
                        string fileToDelete = Application.StartupPath + "\\Settings\\.login";
                        if (File.Exists(fileToDelete))
                            File.Delete(fileToDelete);
                    }

                    LoginServer.Connect(Identifier.Text.Trim(), Password.Text.Trim());
                    while (!LoginServer.IsConnected)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }

                    return LoginServer.IsConnected;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("LoginOnServer(): " + ex);
            }
            MessageBox.Show(nManager.Translate.Get(nManager.Translate.Id.Please_enter_your_user_name_and_password) + ".",
                nManager.Translate.Get(nManager.Translate.Id.Error), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private bool AttachProcess()
        {
            try
            {
                // Fight against cracked version, use sneaky strings name. Constant UpdateCheck equal "TNBAuth".
                Process[] Updater = Process.GetProcesses();
                for (int i = 0; i < Updater.Length; i++)
                {
                    Application.DoEvents();
                    Process AbortUpdate = Updater[i];
                    if (AbortUpdate.MainWindowTitle != Others.DecryptString(UpdateCheck) && AbortUpdate.ProcessName != Others.DecryptString(UpdateCheck))
                        continue;
                    AbortUpdate.Kill();
                    break;
                }

                if (SessionList.SelectedIndex < 0)
                    MessageBox.Show(
                        nManager.Translate.Get(nManager.Translate.Id.Please_select_game_Process_and_connect_to_the_game) +
                        ".", nManager.Translate.Get(nManager.Translate.Id.Stop),
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);

                Pulsator.Dispose();

                if (SessionList.SelectedIndex >= 0)
                {
                    string[] idStringArray =
                        SessionList.SelectedItem.ToString().Replace(" ", "").Split('-');

                    int idProcess = Others.ToInt32(idStringArray[0]);

                    if (!Hook.IsInGame(idProcess) && !nManagerSetting.AutoStartLoggingInfoProvided)
                    {
                        MessageBox.Show(nManager.Translate.Get(nManager.Translate.Id.Please_connect_to_the_game) + ".",
                            nManager.Translate.Get(nManager.Translate.Id.Stop), MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
                        return false;
                    }
                    if (Hook.WowIsUsed(idProcess) && !nManagerSetting.AutoStartLoggingInfoProvided)
                    {
                        DialogResult resulMb =
                            MessageBox.Show(
                                nManager.Translate.Get(
                                    nManager.Translate.Id.The_Game_is_currently_used_by_TheNoobBot_or_contains_traces) +
                                "\n\n" +
                                nManager.Translate.Get(
                                    nManager.Translate.Id.If_no_others_session_of_TheNoobBot_is_currently_active),
                                nManager.Translate.Get(nManager.Translate.Id.Use_this_Game) + "?" + @" - " +
                                Hook.PlayerName(idProcess), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (resulMb == DialogResult.No)
                        {
                            return false;
                        }
                    }

                    Pulsator.Pulse(idProcess, KeyNManager);
                    Logging.Write("Select game process: " + SessionList.SelectedItem);
                    if (Pulsator.IsActive)
                    {
                        if (Usefuls.InGame && !Usefuls.IsLoading)
                            return true;
                        if (nManagerSetting.AutoStartLoggingInfoProvided)
                        {
                            Others.LoginToWoW();
                            if (Usefuls.InGame && !Usefuls.IsLoading)
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("AttachProcess(): " + ex);
            }
            return false;
        }

        private void RefreshProcessList()
        {
            try
            {
                RefreshButton.Enabled = false;
                if (Process.GetProcessesByName("WoW-64").Length > 0)
                    MessageBox.Show(nManager.Translate.Get(nManager.Translate.Id.WoW_Client_64bit),
                        nManager.Translate.Get(nManager.Translate.Id.Title_WoW_Client_64bit),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                SessionList.Items.Clear();
                SessionList.SelectedIndex = -1;
                var usedProcess = new List<string>();
                var listWowProcess = new List<string> {"Wow", "WowT", "WowB", "WowTR"};
                foreach (string s in listWowProcess)
                {
                    Application.DoEvents();
                    for (int i = nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s).Length - 1; i >= 0; i--)
                    {
                        Application.DoEvents();
                        if (SessionList.SelectedIndex == -1 && !Hook.WowIsUsed(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id))
                        {
                            SessionList.Items.Add(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id + " - " +
                                                  Hook.PlayerName(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id));
                            SessionList.SelectedIndex = 0;
                        }
                        else
                        {
                            string used = "";
                            if (Hook.WowIsUsed(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id))
                                used = " - " + nManager.Translate.Get(nManager.Translate.Id.In_use) + ".";
                            usedProcess.Add(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id + " - " +
                                            Hook.PlayerName(nManager.Wow.MemoryClass.Process.ListeProcessIdByName(s)[i].Id) + used);
                        }
                    }
                }

                foreach (string v in usedProcess)
                {
                    Application.DoEvents();
                    SessionList.Items.Add(v);
                }
                if (SessionList.Items.Count == 0)
                    SessionList.Items.Add(nManager.Translate.Get(nManager.Translate.Id.Please_connect_to_the_game));
                RefreshButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Logging.WriteError("RefreshProcessList(): " + ex);
                RefreshButton.Enabled = true;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Pulsator.Dispose(true);
        }

        private void Identifier_Enter(object sender, EventArgs e)
        {
            FormFocusLogin.Visible = true;
            if (Identifier.Text == nManager.Translate.Get(nManager.Translate.Id.LoginFormDefaultIdentifier) ||
                Identifier.Text == nManager.Translate.Get(nManager.Translate.Id.LoginFormKey))
            {
                Identifier.Text = "";
                Identifier.ForeColor = Color.FromArgb(118, 118, 118);
            }
        }

        private void Identifier_Leave(object sender, EventArgs e)
        {
            FormFocusLogin.Visible = false;
            if (Identifier.Text == "")
            {
                Identifier.Text = nManager.Translate.Get(UseKey.Checked ? nManager.Translate.Id.LoginFormKey : nManager.Translate.Id.LoginFormDefaultIdentifier);
                Identifier.ForeColor = Color.FromArgb(202, 202, 202);
            }
        }

        private void Password_Enter(object sender, EventArgs e)
        {
            FormFocusPassword.Visible = true;
            if (Password.Text == "Password")
            {
                Password.Text = "";
                Password.ForeColor = Color.FromArgb(118, 118, 118);
            }
        }

        private void Password_Leave(object sender, EventArgs e)
        {
            FormFocusPassword.Visible = false;
            if (Password.Text == "")
            {
                Password.Text = "Password";
                Password.ForeColor = Color.FromArgb(202, 202, 202);
            }
        }

        private void Register_Click(object sender, EventArgs e)
        {
            Others.OpenWebBrowserOrApplication("http://thenoobbot.com/login/?action=register");
        }

        private void Register_MouseEnter(object sender, EventArgs e)
        {
            Register.BackColor = Color.FromArgb(147, 181, 22);
            Register.ForeColor = Color.FromArgb(255, 255, 255);
        }

        private void Register_MouseLeave(object sender, EventArgs e)
        {
            Register.BackColor = Color.FromArgb(232, 232, 232);
            Register.ForeColor = Color.FromArgb(98, 160, 229);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                Identifier.Enabled = false;
                Password.Enabled = false;
                Register.Enabled = false;
                Remember.Enabled = false;
                LoginButton.Enabled = false;
                RefreshButton.Enabled = false;
                if (LoginOnServer() && AttachProcess())
                {
                    var formMain = new Main();
                    formMain.Show();
                    Hide();
                }
                else
                {
                    Identifier.Enabled = true;
                    Password.Enabled = true;
                    Register.Enabled = true;
                    Remember.Enabled = true;
                    LoginButton.Enabled = true;
                    RefreshButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Identifier.Enabled = true;
                Password.Enabled = true;
                Register.Enabled = true;
                Remember.Enabled = true;

                LoginButton.Enabled = true;
                RefreshButton.Enabled = true;
                Logging.WriteError("LoginButton_Click(object sender, EventArgs e): " + ex);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        private void WebsiteLink_MouseEnter(object sender, EventArgs e)
        {
            WebsiteLink.BackColor = Color.FromArgb(147, 181, 22);
            WebsiteLink.ForeColor = Color.FromArgb(255, 255, 255);
        }

        private void WebsiteLink_MouseLeave(object sender, EventArgs e)
        {
            WebsiteLink.BackColor = Color.FromArgb(232, 232, 232);
            WebsiteLink.ForeColor = Color.FromArgb(98, 160, 229);
        }

        private void ForumLink_MouseEnter(object sender, EventArgs e)
        {
            ForumLink.BackColor = Color.FromArgb(147, 181, 22);
            ForumLink.ForeColor = Color.FromArgb(255, 255, 255);
        }

        private void ForumLink_MouseLeave(object sender, EventArgs e)
        {
            ForumLink.BackColor = Color.FromArgb(232, 232, 232);
            ForumLink.ForeColor = Color.FromArgb(98, 160, 229);
        }

        private void ForumLink_Click(object sender, EventArgs e)
        {
            Others.OpenWebBrowserOrApplication("http://thenoobbot.com/community/");
        }

        private void WebsiteLink_Click(object sender, EventArgs e)
        {
            Others.OpenWebBrowserOrApplication("http://thenoobbot.com/");
        }

        private void EsterEggTrigger_MouseEnter(object sender, EventArgs e)
        {
            EasterEgg.Visible = true;
        }

        private void EsterEggTrigger_MouseLeave(object sender, EventArgs e)
        {
            EasterEgg.Visible = false;
        }

        private void LangSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Application.StartupPath + "\\Settings\\");
                Others.WriteFile(Application.StartupPath + "\\Settings\\lang.txt", LangSelection.Text + ".xml");
                Others.OpenWebBrowserOrApplication(Process.GetCurrentProcess().ProcessName + ".exe");
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Logging.WriteError("LangSelection_SelectedIndexChanged(object sender, EventArgs e): " + ex);
            }
        }

        private void UseKey_CheckedChanged(object sender, EventArgs e)
        {
            if (UseKey.Checked)
            {
                Password.Hide();
                if (Identifier.Text == nManager.Translate.Get(nManager.Translate.Id.LoginFormDefaultIdentifier))
                    Identifier.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormKey);
            }
            else
            {
                Password.Show();
                if (Identifier.Text == nManager.Translate.Get(nManager.Translate.Id.LoginFormKey))
                    Identifier.Text = nManager.Translate.Get(nManager.Translate.Id.LoginFormDefaultIdentifier);
            }
        }

        private void EasterEgg_Click(object sender, EventArgs e)
        {
        }
    }
}