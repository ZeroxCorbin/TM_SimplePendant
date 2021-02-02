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
        public ModbusWindow()
        {
            InitializeComponent();
        }
    }
}
