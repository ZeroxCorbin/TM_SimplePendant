using ApplicationSettingsNS;
using SimpleModbus;
using SocketManagerNS;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TM_Comms;

namespace TM_SimplePendant
{
    /// <summary>
    /// Interaction logic for ModbusWindow.xaml
    /// </summary>
    public partial class ModbusWindow : Window
    {
        SimpleModbusTCP ModbusTCP { get; set; }

        private SolidColorBrush Good { get; } = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        private SolidColorBrush Bad { get; } = new SolidColorBrush(Colors.Red);
        private SolidColorBrush Meh { get; } = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush Disabled { get; } = new SolidColorBrush(Colors.White);

        bool isRunning { get; set; } = false;
        private object LockObject { get; set; } = new object();
        private int NumModbusDictionary { get; set; } = 15;
        private int NumModbusUserRegisters { get; set; } = 15;

        public ModbusWindow(Window owner)
        {
            Owner = owner;

            InitializeComponent();

           // Window_LoadSettings();

            ResetModbusRegisterList();
            ResetModbusUserRegisterList();

            RecalcUserRegisters();

            if (App.Settings.Version == TMflowVersions.V1_80_xxxx)
            {
                BdrRemoteAuto.IsEnabled = true;
                BdrRemoteAuto.Visibility = Visibility.Visible;
            }
            else
            {
                BdrRemoteAuto.Visibility = Visibility.Collapsed;
                BdrRemoteAuto.IsEnabled = false;  
            }
        }

        private void Window_LoadSettings()
        {
            if(double.IsNaN(App.Settings.ModbusWindow.Left)
                || !CheckOnScreen.IsOnScreen(this)
                || Keyboard.IsKeyDown(Key.LeftShift))
            {
                Left = Owner.Left;
                Top = Owner.Top + Owner.Height;
                Height = 768;
                Width = 1024;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ModbusTCP_Message(string message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    (Action)(() =>
                    {
                        TxtSocketOutput.Text += $"{message}\r\n";
                        TxtSocketOutput.ScrollToEnd();
                    }));
        }

        //Control Pad
        //private void AsyncRecieveThread_DoWork(object sender)
        //{
        //    isRunning = true;
        //    while (isRunning)
        //    {
        //        if (!Socket.IsConnected) break;

        //        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
        //        {
        //            lock (LockObject)
        //            {
        //                elpPowerLight.Fill = Good;

        //                if (ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["M/A Mode"].Addr) == 1)
        //                {
        //                    elpAutoLight.Fill = Good;
        //                    elpManualLight.Fill = Disabled;
        //                }
        //                else
        //                {
        //                    elpAutoLight.Fill = Disabled;
        //                    elpManualLight.Fill = Good;
        //                }

        //                if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["EStop"].Addr))
        //                    elpEstopButton.Fill = Bad;
        //                else
        //                    elpEstopButton.Fill = Disabled;

        //                if (App.Settings.Version == TMflowVersions.V1_80_xxxx)
        //                {
        //                    if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Get Control"].Addr))
        //                        ElpGetControl.Fill = Good;
        //                    else
        //                        ElpGetControl.Fill = Bad;

        //                    if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr))
        //                        ElpAutoActive.Fill = Good;
        //                    else
        //                        ElpAutoActive.Fill = Bad;

        //                    if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Enabled"].Addr))
        //                        ElpAutoEnable.Fill = Good;
        //                    else
        //                        ElpAutoEnable.Fill = Bad;
        //                }

        //                btnStop.Background = new SolidColorBrush(Colors.Transparent);
        //                btnPlayPause.Background = new SolidColorBrush(Colors.Transparent);

        //                if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Project Running"].Addr))
        //                    btnPlayPause.Background = new RadialGradientBrush(Color.FromArgb(255, 0, 255, 255), Colors.Transparent);

        //                else if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Project Paused"].Addr))
        //                    btnPlayPause.Background = new RadialGradientBrush(Colors.Yellow, Colors.Transparent);

        //                else if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Project Editing"].Addr))
        //                    btnStop.Background = new RadialGradientBrush(Colors.Yellow, Colors.Transparent);

        //                else
        //                    btnStop.Background = new RadialGradientBrush(Colors.Red, Colors.Transparent);

        //                if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Error"].Addr))
        //                    elpIsError.Fill = Bad;
        //                else
        //                    elpIsError.Fill = Good;


        //                uint code = (uint)ModbusTCP.GetInt32(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Code"].Addr);
        //                if (code != 0)
        //                {
        //                    string dat = $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Month"].Addr)}/" +
        //                                    $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Date"].Addr)}/" +
        //                                    $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Year"].Addr)} " +
        //                                    $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Hour"].Addr)}:" +
        //                                    $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Minute"].Addr)}:" +
        //                                    $"{ModbusTCP.GetInt16(ModbusDictionary.ModbusData[App.Settings.Version]["Last Error Time Second"].Addr)} ";
        //                    if (DateTime.TryParse(dat, out DateTime date))
        //                        txtxErrorDate.Text = date.ToString();
        //                }
        //                else
        //                    txtxErrorDate.Text = "";

        //                txtErrorCode.Text = code.ToString("X");

        //                if (ErrorCodes.Codes.TryGetValue(code, out string val))
        //                    txtErrorDescription.Text = val;
        //                else
        //                    txtErrorDescription.Text = "CAN NOT FIND ERROR IN TABLE.";
        //            }
        //        }));
        //        Thread.Sleep(1000);
        //    }

        //    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
        //            {
        //                elpPowerLight.Fill = Meh;
        //                elpManualLight.Fill = Disabled;
        //                elpAutoLight.Fill = Disabled;
        //                elpEstopButton.Fill = Disabled;
        //                ElpGetControl.Fill = Disabled;
        //                ElpAutoActive.Fill = Disabled;
        //                ElpAutoEnable.Fill = Disabled;

        //                btnStop.Background = new SolidColorBrush(Colors.Transparent);
        //                btnPlayPause.Background = new SolidColorBrush(Colors.Transparent);

        //                elpIsError.Fill = Disabled;
        //                txtErrorCode.Text = "";
        //                txtErrorDescription.Text = "";
        //            }));
        //}
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Stop"].Addr, true);
            }
        }
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Play/Pause"].Addr, true);
            }
        }
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Stick-"].Addr, true);
            }
        }
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Stick+"].Addr, true);
            }
        }
        private void BtnAutoActive_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                if (ModbusTCP.GetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr))
                    ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr, false);
                else
                    ModbusTCP.SetBool(ModbusDictionary.ModbusData[App.Settings.Version]["Auto Remote Mode Active"].Addr, true);
            }
        }

        //Modbus Registers
        private void ResetModbusRegisterList()
        {
            Button btn;

            stackModbusComboBox.Children.Clear();
            stackModbusReadButton.Children.Clear();
            stackModbusText.Children.Clear();
            stackModbusWriteButton.Children.Clear();

            stackModbusComboBox.Children.Add(new Label() { Content = "Modbus Register Name", Height = 26, Width = 240, Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Center });

            btn = new Button() { Content = "Read All", Height = 26, Width = 90, Margin = new Thickness(2) };
            btn.Click += BtnModbusReadAll_Click;
            stackModbusReadButton.Children.Add(btn);

            stackModbusText.Children.Add(new Label() { Content = "Value", Height = 26, Width = 90, Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Center });
            //stackModbusWriteButton.Children.Add(new Label() { Content = "", Height = 26, Width = 60, Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Center });

            for (int i = 1; i <= NumModbusDictionary; i++)
            {
                ComboBox cmb = new ComboBox() { Height = 26, Width = 240, Margin = new Thickness(2) };
                cmb.Tag = i;
                cmb.ItemsSource = ModbusDictionary.ModbusData[App.Settings.Version].Keys;
                cmb.SelectionChanged += CmbModbusRegister_SelectionChanged;
                stackModbusComboBox.Children.Add(cmb);

                btn = new Button() { Height = 26, Width = 90, Margin = new Thickness(2) };
                btn.Tag = i;
                btn.Click += BtnModbusRead_Click;
                stackModbusReadButton.Children.Add(btn);

                stackModbusText.Children.Add(new TextBox() { Height = 26, MinWidth = 90, Margin = new Thickness(2), IsReadOnly = true });

                //btn = new Button() { Height = 26, Width = 60, Margin = new Thickness(2) };
                //btn.Tag = i;
                //btn.Click += BtnModbusWrite_Click;
                //btn.IsEnabled = false;
                //btn.Visibility = Visibility.Hidden;
                //btn.Content = "Write";
                //stackModbusWriteButton.Children.Add(btn);
            }

            int ii = 1;
            foreach (string str in App.Settings.ModbusComboBoxIndices)
                if (ii <= NumModbusDictionary)
                    ((ComboBox)stackModbusComboBox.Children[ii++]).SelectedValue = str;
                else
                    break;
        }
        private void CmbModbusRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = ((ComboBox)sender);
            Button rbtn = (Button)stackModbusReadButton.Children[(int)cmb.Tag];
            //Button wbtn = (Button)stackModbusWriteButton.Children[(int)cmb.Tag];

            rbtn.Content = $"{ModbusDictionary.ModbusData[App.Settings.Version][(string)cmb.SelectedValue].Addr} / {ModbusDictionary.ModbusData[App.Settings.Version][(string)cmb.SelectedValue].Addr:X2}";

            //if (ModbusDictionary.ModbusData[App.Settings.Version][(string)cmb.SelectedValue].Access == ModbusDictionary.MobusValue.AccessTypes.R)
            //{
            //    wbtn.IsEnabled = false;
            //    wbtn.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    wbtn.IsEnabled = true;
            //    wbtn.Visibility = Visibility.Visible;
            //}

            ((TextBox)stackModbusText.Children[(int)cmb.Tag]).Text = string.Empty;
        }
        private void BtnModbusWrite_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject) { };

            int i = (int)((Button)sender).Tag;
            string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
            int addr = ModbusDictionary.ModbusData[App.Settings.Version][val].Addr;

            if (val == null) return;

            if (ModbusTCP != null)
            {
                switch (ModbusDictionary.ModbusData[App.Settings.Version][val].Type)
                {
                    case ModbusDictionary.MobusValue.DataTypes.Bool:
                        if (bool.TryParse(((TextBox)stackModbusText.Children[i]).Text, out bool b))
                            ModbusTCP.SetBool(addr, b);
                        else
                            ((TextBox)stackModbusText.Children[i]).Background = Bad;
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Float:
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int16:
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int32:
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.String:
                        break;
                }
            }
        }
        private void BtnModbusRead_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {

                int i = (int)((Button)sender).Tag;
                string val = (String)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue;
                if (val == null) return;
                int addr = ModbusDictionary.ModbusData[App.Settings.Version][val].Addr;

                if (val == null) return;

                switch (ModbusDictionary.ModbusData[App.Settings.Version][val].Type)
                {
                    case ModbusDictionary.MobusValue.DataTypes.Bool:
                        ((TextBox)stackModbusText.Children[i]).Text = ModbusTCP.GetBool(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Float:
                        ((TextBox)stackModbusText.Children[i]).Text = ModbusTCP.GetFloat(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int16:
                        ((TextBox)stackModbusText.Children[i]).Text = ModbusTCP.GetInt16(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int32:
                        ((TextBox)stackModbusText.Children[i]).Text = ModbusTCP.GetInt32(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.String:
                        ((TextBox)stackModbusText.Children[i]).Text = ModbusTCP.GetString(addr);
                        break;
                }
            };
        }
        private void BtnModbusReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            for (int i = 1; i <= NumModbusDictionary; i++)
                BtnModbusRead_Click(stackModbusReadButton.Children[i], new RoutedEventArgs());
        }

        //Mobus User Registers
        private void ResetModbusUserRegisterList()
        {
            Button btn;

            stackModbusUserComboBox.Children.Clear();
            stackModbusUserReadButton.Children.Clear();
            stackModbusUserText.Children.Clear();
            stackModbusUserWriteButton.Children.Clear();

            stackModbusUserComboBox.Children.Add(new Label() { Content = "Data Type", Height = 26, Width = 80, Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Center });

            btn = new Button() { Content = "Read All", Height = 26, Width = 70, Margin = new Thickness(2) };
            btn.Click += BtnModbusUserReadAll_Click;
            stackModbusUserReadButton.Children.Add(btn);

            stackModbusUserText.Children.Add(new Label() { Content = "Value", Height = 26, Width = 90, Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Center });

            btn = new Button() { Content = "Write All", Height = 26, Width = 70, Margin = new Thickness(2) };
            btn.Click += BtnModbusUserWriteAll_Click;
            stackModbusUserWriteButton.Children.Add(btn);

            for (int i = 1; i <= NumModbusUserRegisters; i++)
            {
                ComboBox cmb = new ComboBox() { Height = 26, Width = 80, Margin = new Thickness(2) };
                cmb.Items.Add("Bool");
                cmb.Items.Add("Int16");
                cmb.Items.Add("Int32");
                cmb.Items.Add("Float");
                cmb.Items.Add("String");
                cmb.SelectionChanged += CmbModbusUserRegister_SelectionChanged;
                stackModbusUserComboBox.Children.Add(cmb);

                btn = new Button() { Height = 26, Width = 70, Margin = new Thickness(2) };
                btn.Tag = i;
                btn.Click += BtnModbusUserRead_Click;
                stackModbusUserReadButton.Children.Add(btn);

                stackModbusUserText.Children.Add(new TextBox() { Height = 26, MinWidth = 90, Margin = new Thickness(2) });

                btn = new Button() { Height = 26, Width = 70, Margin = new Thickness(2) };
                btn.Tag = i;
                btn.Content = "Write";
                btn.Click += BtnModbusUserWrite_Click;
                stackModbusUserWriteButton.Children.Add(btn);
            }

            int ii = 1;
            foreach (string str in App.Settings.ModbusUserComboBoxIndices)
                if (ii <= NumModbusUserRegisters)
                    ((ComboBox)stackModbusUserComboBox.Children[ii++]).SelectedValue = str;
                else
                    break;
        }
        private void CmbModbusUserRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = ((ComboBox)sender);
            cmb.Tag = Enum.Parse(typeof(ModbusDictionary.MobusValue.DataTypes), (string)cmb.SelectedValue);

            if (IsLoaded) RecalcUserRegisters();
        }
        private void RecalcUserRegisters()
        {
            int num = 9000;

            for (int i = 1; i <= NumModbusUserRegisters; i++)
            {
                object tag = ((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
                if (tag == null)
                {
                    ((Button)stackModbusUserReadButton.Children[i]).Content = string.Empty;
                    ((TextBox)stackModbusUserText.Children[i]).Text = string.Empty;
                    continue;
                }
                ModbusDictionary.MobusValue.DataTypes val = (ModbusDictionary.MobusValue.DataTypes)tag;

                ((Button)stackModbusUserReadButton.Children[i]).Content = num.ToString();

                switch (val)
                {
                    case ModbusDictionary.MobusValue.DataTypes.Bool:
                        num += 1;
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Float:
                        num += 2;
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int16:
                        num += 1;
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int32:
                        num += 2;
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.String:
                        num += 32;
                        break;
                }
            }
        }
        private void BtnModbusUserWrite_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                int ind = (int)((Button)sender).Tag;

                object tag = ((ComboBox)stackModbusUserComboBox.Children[ind]).Tag;
                if (tag == null) return;
                ModbusDictionary.MobusValue.DataTypes type = (ModbusDictionary.MobusValue.DataTypes)tag;

                int addr = Convert.ToInt32((string)((Button)stackModbusUserReadButton.Children[ind]).Content);

                TextBox tb = (TextBox)stackModbusUserText.Children[ind];
                int res = 0;
                switch (type)
                {
                    case ModbusDictionary.MobusValue.DataTypes.Bool:
                        res = bool.TryParse(tb.Text, out bool b) ? 0 : 1;
                        if (res == 0) res = (ModbusTCP.SetBool(addr, b) ? 1 : 0) << 1 | res;
                        break;

                    case ModbusDictionary.MobusValue.DataTypes.Float:
                        res = float.TryParse(tb.Text, out float f) ? 0 : 1;
                        if (res == 0) res = (ModbusTCP.SetFloat(addr, new float[] { f }) ? 1 : 0) << 1 | res;
                        break;

                    case ModbusDictionary.MobusValue.DataTypes.Int16:
                        res = short.TryParse(tb.Text, out short s) ? 0 : 1;
                        if (res == 0) res = (ModbusTCP.SetInt16(addr, new short[] { s }) ? 1 : 0) << 1 | res;
                        break;

                    case ModbusDictionary.MobusValue.DataTypes.Int32:
                        res = int.TryParse(tb.Text, out int i) ? 0 : 1;
                        if (res == 0) res = (ModbusTCP.SetInt32(addr, new int[] { i }) ? 1 : 0) << 1 | res;
                        break;

                    case ModbusDictionary.MobusValue.DataTypes.String:

                        break;
                }
                switch (res)
                {
                    case 0:
                        tb.Background = Good;
                        break;
                    case 1:
                        tb.Background = Meh;
                        break;
                    case 2:
                        tb.Background = Bad;
                        break;
                }
            };
        }
        private void BtnModbusUserRead_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            lock (LockObject)
            {
                int i = (int)((Button)sender).Tag;

                object tag = ((ComboBox)stackModbusUserComboBox.Children[i]).Tag;
                if (tag == null) return;
                ModbusDictionary.MobusValue.DataTypes type = (ModbusDictionary.MobusValue.DataTypes)tag;

                int addr = Convert.ToInt32((string)((Button)stackModbusUserReadButton.Children[i]).Content);

                switch (type)
                {
                    case ModbusDictionary.MobusValue.DataTypes.Bool:
                        ((TextBox)stackModbusUserText.Children[i]).Text = ModbusTCP.GetBool(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Float:
                        ((TextBox)stackModbusUserText.Children[i]).Text = ModbusTCP.GetFloatHr(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int16:
                        ((TextBox)stackModbusUserText.Children[i]).Text = ModbusTCP.GetInt16Hr(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.Int32:
                        ((TextBox)stackModbusUserText.Children[i]).Text = ModbusTCP.GetInt32Hr(addr).ToString();
                        break;
                    case ModbusDictionary.MobusValue.DataTypes.String:
                        ((TextBox)stackModbusUserText.Children[i]).Text = ModbusTCP.GetString(addr).ToString();
                        break;
                }
            };
        }
        private void BtnModbusUserReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            for (int i = 1; i <= NumModbusUserRegisters; i++)
                BtnModbusUserRead_Click(stackModbusUserReadButton.Children[i], new RoutedEventArgs());
        }
        private void BtnModbusUserWriteAll_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCP == null) return;

            for (int i = 1; i <= NumModbusUserRegisters; i++)
                BtnModbusUserWrite_Click(stackModbusUserWriteButton.Children[i], new RoutedEventArgs());
        }


        private SocketManager Socket { get; set; }
        //private void BtnConnect_Click(object sender, RoutedEventArgs e)
        //{
        //    if (BtnConnect.Tag == null)
        //    {
        //        if (Connect())
        //        {
        //            BtnConnect.Content = "Close";
        //            BtnConnect.Tag = 1;
        //            return;
        //        }
        //    }
        //    CleanSock();
        //}
        private bool Connect()
        {
            CleanSock();

            Socket = new SocketManager($"{App.Settings.RobotIP}:502");

            Socket.ConnectState += Socket_ConnectState;

            if (Socket.Connect())
                return true;
            else
            {
                CleanSock();
                return false;
            }
        }
        private void CleanSock()
        {
            //isRunning = false;
            //lock (LockObject) { }

            //if (ModbusTCP != null)
            //    ModbusTCP.Message -= ModbusTCP_Message;

            //if (Socket != null)
            //{
            //    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //            (Action)(() =>
            //            {
            //                BtnConnect.Content = "Connect";
            //                BtnConnect.Tag = null;
            //            }));

            //    Socket.ConnectState -= Socket_ConnectState;

            //    Socket.StopReceiveAsync();
            //    Socket.Close();

            //    Socket = null;
            //}
        }
        private void Socket_ConnectState(object sender, bool data)
        {
            //if (!data)
            //{
            //    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //            (Action)(() =>
            //            {
            //                BtnConnect.Content = "Connect";
            //                BtnConnect.Tag = null;
            //            }));

            //    CleanSock();
            //}
            //else
            //{
            //    Dispatcher.BeginInvoke(DispatcherPriority.Render,
            //            (Action)(() =>
            //            {
            //                //ConnectionActive();
            //            }));

            //    ModbusTCP = new SimpleModbusTCP(Socket);
            //    ModbusTCP.Message += ModbusTCP_Message;

            //    //ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRecieveThread_DoWork));
            //}
        }

        //Window Changes
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Settings.ModbusComboBoxIndices = new string[NumModbusDictionary];
            for (int i = 1; i <= NumModbusDictionary; i++)
                App.Settings.ModbusComboBoxIndices[i - 1] = ((string)((ComboBox)stackModbusComboBox.Children[i]).SelectedValue);

            App.Settings.ModbusUserComboBoxIndices = new string[NumModbusUserRegisters];
            for (int i = 1; i <= NumModbusUserRegisters; i++)
                App.Settings.ModbusUserComboBoxIndices[i - 1] = ((string)((ComboBox)stackModbusUserComboBox.Children[i]).SelectedValue);

            CleanSock();
        }


    }
}
