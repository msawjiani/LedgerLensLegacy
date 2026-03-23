using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace LedgerLensMaking.Windows
{
    /// <summary>
    /// Interaction logic for BankReceiptSubledgerWindow.xaml
    /// </summary>
    /// 
    public partial class BankReceiptSubledgerWindow : Window
    {
       
        public BankReceiptSubledgerWindow()
        {
            InitializeComponent();
            {
                InitializeComponent();
                {
                    InitializeComponent();
                    Loaded += OnLoaded;
                }
            }
        }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                // Disable the close button and system menu items
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                DisableCloseButton(hWnd);
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_DISABLED = 0x00000002;
        private const uint SC_CLOSE = 0xF060;

        private void DisableCloseButton(IntPtr hWnd)
        {
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED | MF_DISABLED);
            }
        }
    }
}
