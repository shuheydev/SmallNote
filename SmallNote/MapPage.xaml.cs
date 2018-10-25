using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;
using System.IO.IsolatedStorage;


namespace SmallNote
{
    public partial class MapPage : PhoneApplicationPage
    {
        App MyApp;
        ViewModel NoteView=new ViewModel();

        GeoCoordinateWatcher GCWatcher;// = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        bool PermissionOfLocationService;

        public MapPage()
        {
            InitializeComponent();

            PermissionOfLocationService = (bool)IsolatedStorageSettings.ApplicationSettings["LocationService"];

            //現在地マークの初期化
            CurrentMark.PositionOrigin = PositionOrigin.Center;
            CurrentMark.Visibility = Visibility.Collapsed;

            MyApp = Application.Current as App;

            Dispatcher.BeginInvoke(() => {
                ViewModel noteview = MyApp.NoteView;

                //位置情報が入っているnoteだけ抽出
                foreach (var note in noteview.Notes)
                {
                    if (note.Location.IsUnknown == false)
                        NoteView.Notes.Add(note);
                }

                DataContext = NoteView;
            });

        }


                //ページに移動してきた時の処理。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            //GPSの初期化
            //GPSセンサーを高精度に設定。
            if (PermissionOfLocationService == true)
            {
                try
                {
                    this.GCWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                    this.GCWatcher.StatusChanged += GCWatcher_StatusChanged;
                }
                catch
                { }
                
                if (this.GCWatcher != null)
                {
                    GCWatcher.Start();


                    if (GCWatcher.Position.Location.IsUnknown == false)//デバイスのロケーションサービスがoffの場合を想定
                    {
                        CurrentMark.Location = GCWatcher.Position.Location;
                        if (CurrentMark.Visibility == Visibility.Collapsed)
                        {
                            CurrentMark.Visibility = Visibility.Visible;

                        }
                        MyMap.SetView(GCWatcher.Position.Location, 15);
                    }
                    else
                    {
                        if (CurrentMark.Visibility == Visibility.Visible)
                        {
                            CurrentMark.Visibility = Visibility.Collapsed;
                        }
                        MyMap.SetView(new GeoCoordinate(0, 0), 1);
                    }



                }
                 
            }
            else
            {
                GCWatcher = null;
                MyMap.SetView(new GeoCoordinate(0, 0), 1);
            }
            
        }

        void GCWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            var status = GCWatcher.Status;

            switch (status)
            {
                case GeoPositionStatus.Disabled:
                    {
                        TextBlock_GPSStatus.Text = "Disable";
                        break;
                    }
                case GeoPositionStatus.Initializing:
                    {
                        TextBlock_GPSStatus.Text = "Initializing";
                        break;
                    }
                case GeoPositionStatus.NoData:
                    {
                        TextBlock_GPSStatus.Text = "NoData";
                        break;
                    }
                case GeoPositionStatus.Ready:
                    {
                        TextBlock_GPSStatus.Text = "Ready";
                        break;
                    }
            }
        }

                //メインページから離脱した時の処理。
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (GCWatcher != null)
            {
                GCWatcher.Stop();
                GCWatcher.Dispose();
            }
        }


        private void Image_ZoomUp_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MyMap.ZoomLevel++;
        }

        private void Image_ZoomDown_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MyMap.ZoomLevel--;
        }

        private void Image_MoveToCurrent_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //現在地を中心に地図を表示
            if (PermissionOfLocationService == false)
                return;

            if (GCWatcher.Position.Location.IsUnknown == false)
            {
                MyMap.SetView(GCWatcher.Position.Location, MyMap.ZoomLevel);
                CurrentMark.Location = GCWatcher.Position.Location;
                
            }

        }

        private void Pushpin_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var tappedPin = (sender as Pushpin).DataContext as NoteModel;

            tappedPin.OpenDetailDate = DateTime.Now;
            (Application.Current as App).NewestNoteID = tappedPin.CreateDate;

            IsolatedStorageSettings.ApplicationSettings["EditingType"] = "Modify";
            NavigationService.Navigate(new Uri("/EditPage.xaml?EditingType=Modify&SelectedItemID=" + tappedPin.CreateDate.ToString(), UriKind.Relative));  //ここはtostring()でいい。touniversaltime()にすると、この時刻を現地時刻ととらえてそこから世界標準時を計算してしまうので、ずれてしまうから。

        }

        private void GoMainPageButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); 
        }
    }
}