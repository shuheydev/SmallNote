using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace SmallNote
{
    public partial class SettingPage : PhoneApplicationPage
    {
        string EmailAddress;
        bool PermissionOfLocationService;


        public SettingPage()
        {
            InitializeComponent();
        }

        //ページに移動してきた時の処理。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            EmailAddress = (string)IsolatedStorageSettings.ApplicationSettings["Email"];
            TextBox_Email.Text = EmailAddress;

            PermissionOfLocationService = (bool)IsolatedStorageSettings.ApplicationSettings["LocationService"];
            ToggleSwitch_LocationService.IsChecked = PermissionOfLocationService;

            base.OnNavigatedTo(e);
        }

        //メインページから離脱した時の処理。
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["Email"] = TextBox_Email.Text;

            base.OnNavigatedFrom(e);
        }

        private void ToggleSwitch_LocationService_Changed(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["LocationService"] = ToggleSwitch_LocationService.IsChecked;
            System.Diagnostics.Debug.WriteLine(ToggleSwitch_LocationService.IsChecked);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}