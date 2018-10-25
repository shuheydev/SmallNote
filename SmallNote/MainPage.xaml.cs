using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SmallNote
{
    public partial class MainPage : PhoneApplicationPage
    {
        ViewModel NoteView;
        //DataBase DB;// = new DataBase();

        string EmailAddress;

        //bool SortedFlag = false;

        App MyApp;

        string SortBy="Title";

        string SearchKeyword = "";

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            #region ユーザからロケーションサービス使用の許可を得る手続き
            if ((bool)IsolatedStorageSettings.ApplicationSettings["FirstTimeMessagePop"] == false)
            {
                var result = MessageBox.Show("In order to display your location and provide optimal user experience, we need access to your current location.Your location information will not be stored or shared, and you can always disable this feature in the settings page. Do you want to enable access to location service?", "Location Service", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationService"] = true;
                }
                IsolatedStorageSettings.ApplicationSettings["FirstTimeMessagePop"] = true;
            }
            #endregion 

            MyApp = Application.Current as App;

        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            EmailAddress = (string)IsolatedStorageSettings.ApplicationSettings["Email"];


            //検索ボックスを非表示にする
            //Grid_SearchBox.Visibility = Visibility.Collapsed;
            //Button_ResetSearch.Visibility = Visibility.Collapsed;

            SortBy = (string)IsolatedStorageSettings.ApplicationSettings["Sort"];
            SearchKeyword = (string)IsolatedStorageSettings.ApplicationSettings["SearchKeyword"];



            //ノートデータの読み込み
            //NoteView = DB.LoadInfoFromXML();
            NoteView = MyApp.NoteView;

            System.Diagnostics.Debug.WriteLine(NoteView.Notes.Count);
            //ノートが0個の時は、「No Items」と表示
            if (NoteView.Notes.Count == 0)
                TextBlock_NoItemSign.Visibility = Visibility.Visible;
            else
                TextBlock_NoItemSign.Visibility = Visibility.Collapsed;

            //リストの読み込みと表示
            
            MyCollection.Source = NoteView.Notes;//
            MyCollection.View.Filter = null;

            initList();
            /*
            MyCollection.View.SortDescriptions.Clear();
            MyCollection.View.SortDescriptions.Add(new SortDescription("OpenDetailDate",ListSortDirection.Descending));
            //MyCollection.View.SortDescriptions.Add(new SortDescription("ModifyDate", ListSortDirection.Descending));
            */
            TextBox_SearchBox.Text = SearchKeyword;

            base.OnNavigatedTo(e);
        }
        private void initList()
        {
            initSortOption();
            filteringByKeyword();
            SortCollection();
        }

        //メインページから離脱した時の処理。
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["Sort"] = SortBy;
            IsolatedStorageSettings.ApplicationSettings["SearchKeyword"] = SearchKeyword;

            //DB.SaveInfoToIsoStrage(NoteView);
            base.OnNavigatedFrom(e);
        }

        #region コンテキストメニュー
        private void ContextMenuItemEmail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var holdedItem = (sender as MenuItem).DataContext as NoteModel;
            CultureInfo cc = Thread.CurrentThread.CurrentCulture;
            string link;
            if (holdedItem.Location.IsUnknown == false)
                link = string.Format("http://maps.google.com/maps?q={0},{1}&hl={2}", holdedItem.Location.Latitude, holdedItem.Location.Longitude, cc.ToString());
            else
                link = "";

            string Body = "" + System.Environment.NewLine + System.Environment.NewLine
                               + holdedItem.Content + System.Environment.NewLine + System.Environment.NewLine
                               + "Create Date: " + holdedItem.CreateDate + System.Environment.NewLine
                               + "Modify Date: " + holdedItem.ModifyDate + System.Environment.NewLine
                               + "Location: " + (holdedItem.Location.IsUnknown == true ? "" : holdedItem.Location.Latitude.ToString() + " , " + holdedItem.Location.Longitude.ToString()) + System.Environment.NewLine
                //+ "Longitude: " + (holdedItem.Location.IsUnknown == true ? "" : holdedItem.Location.Longitude.ToString()) + System.Environment.NewLine
                               + "Address: " + holdedItem.Address + System.Environment.NewLine
                               + "Link: " + link;



            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = holdedItem.Title;
            emailComposeTask.Body = Body;
            emailComposeTask.To = EmailAddress;
            emailComposeTask.Show();

            holdedItem.Shared = Visibility.Visible;
        }

        private void ContextMenuItemSMS_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var holdedItem = (sender as MenuItem).DataContext as NoteModel;

            string Body = "" + System.Environment.NewLine + System.Environment.NewLine
                + "Title: " + holdedItem.Title + System.Environment.NewLine + System.Environment.NewLine
                               + holdedItem.Content + System.Environment.NewLine + System.Environment.NewLine
                //+ "Create Date: " + holdedItem.CreateDate + System.Environment.NewLine
                //+ "Modify Date: " + holdedItem.ModifyDate + System.Environment.NewLine
                               + "Location: " + (holdedItem.Location.IsUnknown == true ? "" : holdedItem.Location.Latitude.ToString() + " , " + holdedItem.Location.Longitude.ToString()) + System.Environment.NewLine
                               + "Address: " + holdedItem.Address + System.Environment.NewLine;


            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.To = "";
            smsComposeTask.Body = Body;
            smsComposeTask.Show();
        }

        private void ContextMenuItemShare_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var holdedItem = (sender as MenuItem).DataContext as NoteModel;
            CultureInfo cc = Thread.CurrentThread.CurrentCulture;
            string link;
            if (holdedItem.Location.IsUnknown == false)
                link = string.Format("http://maps.google.com/maps?q={0},{1}&hl={2}", holdedItem.Location.Latitude, holdedItem.Location.Longitude, cc.ToString());
            else
                link = "";

            string Body = "" + System.Environment.NewLine + System.Environment.NewLine
                + "Title: " + holdedItem.Title + System.Environment.NewLine + System.Environment.NewLine
                               + holdedItem.Content + System.Environment.NewLine + System.Environment.NewLine
                               + "Create Date: " + holdedItem.CreateDate + System.Environment.NewLine
                               + "Modify Date: " + holdedItem.ModifyDate + System.Environment.NewLine
                               + "Location: " + (holdedItem.Location.IsUnknown == true ? "" : holdedItem.Location.Latitude.ToString() + " , " + holdedItem.Location.Longitude.ToString()) + System.Environment.NewLine
                               + "Address: " + holdedItem.Address + System.Environment.NewLine
                               + "Link: " + link;




            ShareStatusTask shareStatusTask = new ShareStatusTask();
            shareStatusTask.Status = Body;
            shareStatusTask.Show();
        }


        private void ContextMenuItemDelete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var holdedItem = (sender as MenuItem).DataContext as NoteModel;

            var result = MessageBox.Show("Are you sure to delete ''" + holdedItem.Title + "'' ?", "Delete", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                NoteView.Notes.Remove(holdedItem);
            }

            if (NoteView.Notes.Count == 0)
                TextBlock_NoItemSign.Visibility = Visibility.Visible;
            else
                TextBlock_NoItemSign.Visibility = Visibility.Collapsed;
        }


        #endregion



        private void AddNewNoteButton_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/EditPage.xaml?transitionType=newNote", UriKind.Relative));  
            IsolatedStorageSettings.ApplicationSettings["EditingType"] = "New";
            NavigationService.Navigate(new Uri("/EditPage.xaml?EditingType=New", UriKind.Relative));  
        }



        private void ListBox_Notes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            //System.Diagnostics.Debug.WriteLine(sender.ToString());
            var selectedItem = ListBox_Notes.SelectedItem as NoteModel;
            //NoteView.SetSelected(selectedItem);
            if (selectedItem != null)
                //選択中のファイルを識別するために、ノートの作成時刻を使う。
                IsolatedStorageSettings.ApplicationSettings["SelectedItemID"] = selectedItem.CreateDate.ToString();
        
             */
        }

        private void StackPanel_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            //System.Diagnostics.Debug.WriteLine((sender as StackPanel).ToString());
            var selectedItem = (sender as StackPanel).DataContext as NoteModel;

            selectedItem.OpenDetailDate = DateTime.Now;
            (Application.Current as App).NewestNoteID = selectedItem.CreateDate;

            //選択中のファイルを識別するために、ノートの作成時刻を使う。
            //IsolatedStorageSettings.ApplicationSettings["SelectedItemID"] = selectedItem.CreateDate.ToFileTimeUtc();

            //NavigationService.Navigate(new Uri("/EditPage.xaml?transitionType=editNote", UriKind.Relative));
            IsolatedStorageSettings.ApplicationSettings["EditingType"] = "Modify";
            NavigationService.Navigate(new Uri("/EditPage.xaml?EditingType=Modify&SelectedItemID="+selectedItem.CreateDate.ToString(), UriKind.Relative));  //ここはtostring()でいい。touniversaltime()にすると、この時刻を現地時刻ととらえてそこから世界標準時を計算してしまうので、ずれてしまうから。
        }

        #region 検索ボックス

        private void TextBox_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                e.Handled = true;
                this.Focus();



                SearchKeyword = TextBox_SearchBox.Text;

                filteringByKeyword();

                Pivot_ListPage.SelectedItem = PivotItem1;
            }
        }
        private void filteringByKeyword()
        {
            CompareInfo compareInfo = System.Globalization.CultureInfo.CurrentCulture.CompareInfo;
            if (SearchKeyword != "")
            {
                MyCollection.View.Filter = delegate(object o)
                {
                    NoteModel note = (o as NoteModel);

                    string compareString = note.Title + note.Content + note.Address;

                    if (compareInfo.IndexOf(compareString, SearchKeyword, 0, CompareOptions.IgnoreCase) != -1)
                        return true;
                    else if (compareInfo.IndexOf(compareString, SearchKeyword, 0, CompareOptions.IgnoreKanaType) != -1)
                        return true;
                    return false;
                };

                //SortedFlag = true;
                //Button_ResetSearch.Visibility = Visibility.Visible;
            }
            else
            {
                MyCollection.View.Filter = null;
                //SortedFlag = false;
                //Button_ResetSearch.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region アプリケーションバー
        private void AppbarMenu_Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        private void AppbarMenu_About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AppInfoPage.xaml", UriKind.Relative));
        }

        #endregion


        private void MapButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative)); 
        }

        private void TextBox_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void Button_SortOption_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SortBy = tempCheckedSortOption;
            SortCollection();
            Pivot_ListPage.SelectedItem = PivotItem1;
        }

        string tempCheckedSortOption = "";
        private void RadioButton_SortOption_Checked(object sender, RoutedEventArgs e)
        {
            var checkedItem = sender as RadioButton;

            tempCheckedSortOption = checkedItem.Content.ToString();
        }

        private void SortCollection()
        {
            MyCollection.View.SortDescriptions.Clear();

            switch (SortBy)
            {
                case "Title":
                    {
                        MyCollection.View.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));

                        break;
                    }
                case "Location":
                    {                      
                        MyCollection.View.SortDescriptions.Add(new SortDescription("Location.Latitude", ListSortDirection.Descending));//メンバ変数にもアクセスできる！普通に.を使えばいい。うひょー。
                        MyCollection.View.SortDescriptions.Add(new SortDescription("Location.Longitude", ListSortDirection.Descending));

                        break;
                    }
                case "Created Date":
                    {
                        MyCollection.View.SortDescriptions.Add(new SortDescription("CreateDate", ListSortDirection.Descending));
                        break;
                    }
                case "Modified Date":
                    {
                        MyCollection.View.SortDescriptions.Add(new SortDescription("ModifyDate", ListSortDirection.Descending));
                        break;
                    }
                case "Recent Opened":
                    {
                        MyCollection.View.SortDescriptions.Add(new SortDescription("OpenDetailDate", ListSortDirection.Descending));
                        break;
                    }
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            initSortOption();

            var pivot = sender as Pivot;

            switch (pivot.SelectedIndex)
            { 
                case 0:
                    ApplicationBar.IsVisible = true;
                    break;
                case 1:
                    ApplicationBar.IsVisible = false;
                    break;
            }

        }

        private void initSortOption()
        {
            //ラジオボタンにチェックを入れるだけ
            switch (SortBy)
            {
                case "Title":
                    {
                        RadioButton_SortOption_Title.IsChecked = true;
                        break;
                    }
                case "Location":
                    {
                        RadioButton_SortOption_Location.IsChecked = true;
                        break;
                    }
                case "Created Date":
                    {
                        RadioButton_SortOption_CreateDate.IsChecked = true;
                        break;
                    }
                case "Modified Date":
                    {
                        RadioButton_SortOption_ModifyDate.IsChecked = true;
                        break;
                    }
                case "Recent Opened":
                    {
                        RadioButton_SortOption_OpenDetailDate.IsChecked = true;
                        break;
                    }
            }
        }



    }
}