using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks;

namespace SmallNote
{
    public partial class AppInfoPage : PhoneApplicationPage
    {
        LicenseInformation LicenseInfo = new LicenseInformation();

        public AppInfoPage()
        {
            InitializeComponent();

            if (LicenseInfo.IsTrial())
            {
                TextBlock_PurchaseStatus.Text = "Status:        Trial";
                TextBlock_Notification.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlock_PurchaseStatus.Text = "Status:        Purchased";
                Button_Purchase.Visibility = Visibility.Collapsed;
                TextBlock_Notification.Visibility = Visibility.Collapsed;
            }
        }

        private void Hyperlink_Email_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var s = sender as HyperlinkButton;

            System.Diagnostics.Debug.WriteLine(s.Content);

            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "SmallNote feedback";
            emailComposeTask.Body = "";
            emailComposeTask.To = s.Content.ToString();
            emailComposeTask.Show();
        }

        private void Button_Purchase_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var task = new MarketplaceDetailTask
            {
                ContentIdentifier = "e25cb989-3fe0-4842-b9e1-7fa0e55954f7",
                ContentType = MarketplaceContentType.Applications
            };

            task.Show();
        }
    }
}