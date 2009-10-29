﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Switcheroo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AppWindow> WindowList;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;                
        private HotKey hotkey;

        public MainWindow()
        {           
            InitializeComponent();
            Hide();  
        
            // Handle notification icon stuff            
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "Switcheroo";           
            Bitmap bmp = Switcheroo.Properties.Resources.arrow_switch;
            m_notifyIcon.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());                                          
            m_notifyIcon.Visible = true;

            //Create right-click menu on notification icon
            m_notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem("Options", (s, e) => Options()),
                new System.Windows.Forms.MenuItem("Quit", (s, e) => Quit())               
            });

            Model.Initialize();
            hotkey = Model.hotkey;
            WindowList = Model.WindowList;
            hotkey.HotkeyPressed += new EventHandler(hotkey_HotkeyPressed);
            try {
                hotkey.Enabled = true;
            }
            catch (ManagedWinapi.HotkeyAlreadyInUseException) {
                System.Windows.MessageBox.Show("Could not register hotkey (already in use).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Options()
        {            
            Window opts = new Switcheroo.OptionsWindow();            
            opts.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            opts.ShowDialog();
        }
        
        private void Quit()
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
            hotkey.Dispose();
            Environment.Exit(0);  
        }       

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {           
            e.Cancel = true;
            Hide();
        }            
        
        void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            LoadData();            
            Show();
            Activate();
            Keyboard.Focus(tb);
        }

        public void LoadData()
        {
            WindowList.Clear();
            Model.GetWindows();
            WindowList.Sort((x, y) => string.Compare(x.title, y.title));
            lb.DataContext = null;
            lb.DataContext = WindowList;
            tb.Clear();
            tb.Focus();          
            //These two lines size upon load, but don't whiplash resize during typing
            SizeToContent = SizeToContent.Width;
            SizeToContent = SizeToContent.Manual;
            Left = (SystemParameters.PrimaryScreenWidth / 2) - (ActualWidth / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (ActualHeight / 2);            
        }

        private void PrintText(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem lbi = (sender as ListBox).SelectedItem as ListBoxItem;            
        }

        private void TextChanged(object sender, TextChangedEventArgs args)
        {            
            lb.DataContext = Model.FilterList(tb.Text);
            if (lb.Items.Count > 0) {
                lb.SelectedItem = lb.Items[0];
            }            
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {            
            switch (e.Key)
            {
                case Key.Enter:
                    if (lb.Items.Count > 0) {
                        WinAPI.SwitchToThisWindow(((AppWindow)lb.SelectedItem).handle);
                    }
                    Hide();
                    break;
                case Key.Down:
                    if (lb.SelectedIndex != lb.Items.Count - 1) {
                        lb.SelectedIndex++;
                    }
                    break;
                case Key.Up:
                    if (lb.SelectedIndex != 0) {
                        lb.SelectedIndex--;
                    }
                    break;
                case Key.Escape:
                    Hide();
                    break;                
                default:
                    break;
            }
        } 
    }
}
