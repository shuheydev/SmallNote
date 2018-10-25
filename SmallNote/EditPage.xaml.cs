using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Device.Location;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SmallNote
{
    public partial class EditPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask;
        private const string ImageDIR = "Images";

        ViewModel NoteView;

        GeoCoordinateWatcher GCWatcher;// = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        bool hasGPS=false;

        //DataBase DB;// = new DataBase();

        NoteModel ModifyingNote;
        NoteModel ResumedNote;
        NoteModel NewNote;

        bool PermissionOfLocationService;

        string EditingType = "";

        string SelectedItemID;

        //セーブをする必要があるかを判断するために、ページ開始時と終了時のタイトル、本文を保持する。
        //string StrBefore="";//
        //string StrAfter="";
        //bool NeedResume;//内容に変更が生じたときにtrueにする。
        bool doNotResume = false;

        App MyApp;
        public EditPage()
        {

            PermissionOfLocationService = (bool)IsolatedStorageSettings.ApplicationSettings["LocationService"];

            InitializeComponent();

            //if (DB == null)
            //    DB = new DataBase();


            MyApp = Application.Current as App;

            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
        }


        //string transitionType;

        //ページに移動してきた時の処理。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //GPSの初期化
            //GPSセンサーを高精度に設定。
            if (PermissionOfLocationService == true)
            {
                this.GCWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                if (GCWatcher != null)
                {
                    hasGPS = true;
                }
                else
                {
                    hasGPS = false;
                }
            }
            else
            {
                this.GCWatcher = null;
            }


            EditingType = NavigationContext.QueryString["EditingType"];//(string)IsolatedStorageSettings.ApplicationSettings["EditingType"];
            //NeedResume = (bool)IsolatedStorageSettings.ApplicationSettings["Resume"];

            //ノートデータ読み込み
            //NoteView = DB.LoadInfoFromXML();
            NoteView = MyApp.NoteView;


            if (PermissionOfLocationService == true)
                GCWatcher.Start();


            //transitionType = NavigationContext.QueryString["transitionType"];

            #region ノートデータをテキストボックス等に表示する
            if (ResumedNote != null)//||(bool)IsolatedStorageSettings.ApplicationSettings["Resume"]==true)
            {
                //transitionType = "resume";

                //編集を中断したノートデータを読み込む。ノートデータはメンバ変数Resumeがtrueになっているので、それで判断。
                setResumeNoteToUI();
                ResumedNote = null;
                doNotResume = false;
            }
            else if (EditingType == "New")
            {
                setNewNoteToUI();
                getAddressAndLocation("New");
            }
            else if (EditingType == "Modify")
            {
                SelectedItemID = NavigationContext.QueryString["SelectedItemID"];//(string)IsolatedStorageSettings.ApplicationSettings["SelectedItemID"];
                //ModifyingNote = NoteView.GetSelcted();
                //メインページで取得しておいた選択されたノートの作成時刻をIDとして検索
                ModifyingNote = NoteView.GetNoteByTimeStamp(DateTime.Parse(SelectedItemID));
                if (ModifyingNote != null)
                    setSelectedNoteToUI();
                else
                {
                    //みつからなかったら新規作成扱いに
                    EditingType = "New";
                    setNewNoteToUI();//とりあえず
                }
            }
            #endregion


            Panorama.DefaultItem = Panorama.Items[1];


            //タイトルにフォーカスを移す
            //TextBox_Title.Focus();


            base.OnNavigatedTo(e);
        }




        private void getAddressAndLocation(string mode)
        {
            //ロケーションサービスの使用許可があれば、必要に応じて住所を取得
            if (PermissionOfLocationService == true)
            {
                if (TextBox_Latitude.Text != "" && TextBox_Longitude.Text != "" && (TextBox_Address.Text=="" || TextBox_Address.Text=="Address"))
                {
                    GeoCoding geoCode = new GeoCoding();

                    geoCode.DownloadGeoCodeResultCompleted += geoCode_DownloadGeoCodeResultCompleted;
                    GeoCoordinate location = new GeoCoordinate(Convert.ToDouble(TextBox_Latitude.Text), Convert.ToDouble(TextBox_Longitude.Text));
                    geoCode.GetAddressFromGeoCoordinate(location);
                }
                else if ((TextBox_Latitude.Text == "" || TextBox_Longitude.Text == "") && (TextBox_Address.Text != "" && TextBox_Address.Text != "Address"))
                {
                    GeoCoding geoCode = new GeoCoding();

                    geoCode.DownloadGeoCodeResultCompleted += geoCode_DownloadReverseGeoCodeResultCompleted;
                    string address = TextBox_Address.Text;
                    geoCode.GetGeoCoordintateFromAddress(address);
                }
                else if ((TextBox_Latitude.Text == "" || TextBox_Longitude.Text == "") && (TextBox_Address.Text == "" || TextBox_Address.Text == "Address"))
                {
                    if (mode == "Download")
                    {
                        TextBox_Latitude.Text = GCWatcher.Position.Location.Latitude.ToString();
                        TextBox_Longitude.Text = GCWatcher.Position.Location.Longitude.ToString();

                        GeoCoding geoCode = new GeoCoding();

                        geoCode.DownloadGeoCodeResultCompleted += geoCode_DownloadGeoCodeResultCompleted;
                        GeoCoordinate location = new GeoCoordinate(Convert.ToDouble(TextBox_Latitude.Text), Convert.ToDouble(TextBox_Longitude.Text));
                        geoCode.GetAddressFromGeoCoordinate(location);
                    }
                }



                /*
                //住所が空欄だったら住所を取得する。
                if (TextBox_Address.Text == "" || TextBox_Address.Text == "Address")
                {
                    //経緯度のどちらかが空欄の場合は住所欄も空欄に
                    if (TextBox_Latitude.Text == "" || TextBox_Longitude.Text == "")
                    {
                        TextBox_Address.Text = "";
                        //return;
                        //これを初期状態として記録
                        StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
                    }
                    else//どちらも記入されている場合は、住所取得を試みる
                    {
                        GeoCoding geoCode = new GeoCoding();

                        geoCode.DownloadGeoCodeResultCompleted += geoCode_DownloadGeoCodeResultCompleted;
                        GeoCoordinate location = new GeoCoordinate(Convert.ToDouble(TextBox_Latitude.Text), Convert.ToDouble(TextBox_Longitude.Text));
                        geoCode.GetAddressFromGeoCoordinate(location);
                    }
                }
                else//住所欄が空欄ではない場合
                {
                    //経緯度が両方共記入されている場合は、住所・経緯度すべて記入されているということ。
                    if (TextBox_Latitude.Text != "" && TextBox_Longitude.Text != "")
                    {
                        //return;
                        //何もせずにこれを初期状態として記録
                        StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
                    }
                    else//経緯度のどちらかが空欄の場合、住所から経緯度取得を試みる
                    {
                        GeoCoding geoCode = new GeoCoding();

                        geoCode.DownloadGeoCodeResultCompleted += geoCode_DownloadReverseGeoCodeResultCompleted;
                        string address = TextBox_Address.Text;
                        geoCode.GetGeoCoordintateFromAddress(address);
                    }
                }
                */

            }
            else//ロケーションサービスへのアクセスが許可されていない場合
            {
                //現時点を初期状態として記録
                //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
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

            /*
            //ページ離脱直前の状態を記録
            StrAfter = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
            //比較して、変更があればレジュームデータとして記録する必要があることをしめすフラグを立てる。
            if (StrBefore.CompareTo(StrAfter) != 0)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingType"]="Resume";
            }
            
            //フラグがレジュームの場合はレジュームデータを作成。
            if ((string)IsolatedStorageSettings.ApplicationSettings["EditingType"] == "Resume")
            {
                saveResumeNote();
            }
            */

            if (doNotResume == false)//
            {
                saveResumeNote();

                string imageFilePath = ImageDIR + "/" + DateTime.Parse(TextBlock_CreateDate.Text).ToFileTimeUtc();
                IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = imageFilePath;
            }

            base.OnNavigatedFrom(e);
        }

        //住所データのダウンロードの結果が出たら呼び出される
        void geoCode_DownloadGeoCodeResultCompleted(object sender, DownloadGeoCodeResultCompletedEventArgs e)
        {
            //住所を取得できなかった場合の処理
            if (e.Status != "Completed")//Error,TimeOut,NoNetWork含む
            {
                TextBox_Address.Text = "";


                //return;
            }
            else
            {
                try
                {
                    var xmlDoc = XElement.Parse(e.Result);
                    var ns = xmlDoc.Name.Namespace;
                    var places = from p in xmlDoc.Descendants(ns + "Placemark")
                                 select p;

                    //結果の中から先頭の候補を取り出す
                    if (places.Count() == 0)
                    {
                        TextBox_Address.Text = "";
                    }
                    else
                    {
                        string address = (string)places.First().Element(ns + "address").Value;

                        TextBox_Address.Text = address;

                        if (EditingType == "Modify")
                        {
                            //既存のノートの場合は、住所を取得できたらセーブする
                            UIToNoteView();
                        }
                    }
                }
                catch
                {
                    //notsupportedexceptionだったかな？ログイン画面に飛ばされるWifiに接続した状態だと、htmlを取得してしまうのでxmlとして解析できない。
                    TextBox_Address.Text = "";
                }
            }

            //住所データ取得結果をUIに反映させたあとで、初期状態として記録
            //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
        }

        //住所から経緯度を取得
        private void geoCode_DownloadReverseGeoCodeResultCompleted(object sender, DownloadGeoCodeResultCompletedEventArgs e)
        {
            if (e.Status != "Completed")
            {

                //return;
            }
            else
            {
                try
                {
                    var xmlDoc = XElement.Parse(e.Result);
                    var ns = xmlDoc.Name.Namespace;
                    var places = from place in xmlDoc.Descendants(ns + "Placemark")
                                 select place;

                    if (places.Count() == 0)
                    {
                        TextBox_Latitude.Text = "";
                        TextBox_Longitude.Text = "";
                    }
                    else
                    {
                        var firstElement = places.First();
                        string[] geocoordinate = ((string)firstElement.Element(ns + "Point").Element(ns + "coordinates").Value).Split(',');

                        TextBox_Latitude.Text = geocoordinate[1];
                        TextBox_Longitude.Text = geocoordinate[0];
                    }
                }
                catch
                {
                    TextBox_Latitude.Text = "";
                    TextBox_Longitude.Text = "";
                }
            }

            //経緯度を取得できなかった場合に、現状を初期状態として記録
            //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
        }


        #region ノートデータをUIに表示する
        private void setSelectedNoteToUI()
        {
            if (ModifyingNote == null)
                return;

            TextBox_Title.Text = ModifyingNote.Title;
            TextBox_Note.Text = ModifyingNote.Content;

            TextBlock_CreateDate.Text = ModifyingNote.CreateDate.ToString();
            TextBlock_CreateDayOfWeek.Text = ModifyingNote.CreateDayOfWeek.ToString();
            TextBlock_ModifyDate.Text = ModifyingNote.ModifyDate.ToString();
            TextBlock_ModifyDayOfWeek.Text = ModifyingNote.ModifyDayOfWeek.ToString();

            if (ModifyingNote.Location.IsUnknown == false)
            {
                TextBox_Latitude.Text = ModifyingNote.Location.Latitude.ToString();
                TextBox_Longitude.Text = ModifyingNote.Location.Longitude.ToString();
            }
            else
            {
               
                TextBox_Latitude.Text = "";
                TextBox_Longitude.Text = "";
            }

            TextBox_Address.Text = ModifyingNote.Address;

            string imageFilePath = ImageDIR + "/" + DateTime.Parse(TextBlock_CreateDate.Text).ToFileTimeUtc();
            loadPicture(imageFilePath);
            //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
        }

        private void setNewNoteToUI()
        {
            TextBox_Title.Text = "New Note";
            TextBox_Note.Text = "";

            TextBlock_CreateDate.Text = DateTime.Now.ToString();
            TextBlock_CreateDayOfWeek.Text = DateTime.Now.DayOfWeek.ToString();
            TextBlock_ModifyDate.Text = DateTime.Now.ToString();
            TextBlock_ModifyDayOfWeek.Text = DateTime.Now.DayOfWeek.ToString();

            if (GCWatcher!=null && GCWatcher.Position.Location.IsUnknown == false)
            {
                TextBox_Latitude.Text = GCWatcher.Position.Location.Latitude.ToString();
                TextBox_Longitude.Text = GCWatcher.Position.Location.Longitude.ToString();
            }
            else
            {
                TextBox_Latitude.Text = "";
                TextBox_Longitude.Text = "";
            }

            TextBox_Address.Text = "";

        }



        private void setResumeNoteToUI()
        {
            //ResumedNoteが空だったら即return
            //if (ResumedNote == null)
              //  return;

            TextBox_Title.Text = ResumedNote.Title;
            TextBox_Note.Text = ResumedNote.Content;
            TextBlock_CreateDate.Text = ResumedNote.CreateDate.ToString();
            TextBlock_CreateDayOfWeek.Text = ResumedNote.CreateDayOfWeek.ToString();
            TextBlock_ModifyDate.Text = ResumedNote.ModifyDate.ToString();
            TextBlock_ModifyDayOfWeek.Text = ResumedNote.ModifyDayOfWeek.ToString();

            if (Double.IsNaN(ResumedNote.Location.Latitude) == false)
                TextBox_Latitude.Text = ResumedNote.Location.Latitude.ToString();
            else
                TextBox_Latitude.Text = "";
            if (Double.IsNaN(ResumedNote.Location.Longitude) == false)
                TextBox_Longitude.Text = ResumedNote.Location.Longitude.ToString();
            else
                TextBox_Longitude.Text = "";

            TextBox_Address.Text = ResumedNote.Address;

            string imageFilePath = ImageDIR + "/" + DateTime.Parse(TextBlock_CreateDate.Text).ToFileTimeUtc();
            loadPicture(imageFilePath);
        }

        #endregion


        private void saveResumeNote()
        {
            //IsolatedStorageSettings.ApplicationSettings["Resume"] = true;
            ResumedNote = new NoteModel();


            ResumedNote.Title = TextBox_Title.Text;
            ResumedNote.Content = TextBox_Note.Text;
            ResumedNote.CreateDate =DateTime.Parse( TextBlock_CreateDate.Text);
            ResumedNote.ModifyDate =DateTime.Parse( TextBlock_ModifyDate.Text);

            System.Diagnostics.Debug.WriteLine(TextBox_Latitude.Text);
            if (TextBox_Latitude.Text != "")
                ResumedNote.Location.Latitude =Convert.ToDouble(TextBox_Latitude.Text);
            else
                ResumedNote.Location.Latitude = Double.NaN;

            if (TextBox_Longitude.Text!="")
            {
                ResumedNote.Location.Longitude =Convert.ToDouble(TextBox_Longitude.Text);
            }
            else
            {
                ResumedNote.Location.Longitude = Double.NaN;
            }

            ResumedNote.Address = TextBox_Address.Text;

            ResumedNote.Resume = true;

        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            UIToNoteView();
            IsolatedStorageSettings.ApplicationSettings["EditingType"] = "";

            doNotResume = true;
            IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = "";

            NavigationService.GoBack();


            /*
            if (TextBox_Latitude.Text == "" || TextBox_Longitude.Text == "")
            {
                //どちらか一方が空欄だったら、両方とも空欄として扱う。
                TextBox_Latitude.Text = "";
                TextBox_Longitude.Text = "";

                UIToNoteView();
                IsolatedStorageSettings.ApplicationSettings["EditingType"] = "";
                NavigationService.GoBack();
            }
            else
            {//経緯度両方にデータが入力されている場合
                
                #region　有効な経緯度を入力するように促す処理
                var latitude = Convert.ToDouble(TextBox_Latitude.Text);
                var longitude = Convert.ToDouble(TextBox_Longitude.Text);

                int checkInputFlag = 0;

                if (latitude > -90 && latitude < 90)
                {

                }
                else
                {
                    checkInputFlag = 1;//latitudeが範囲外
                }

                if (longitude > -180 && longitude < 180)
                {

                }
                else
                {
                    checkInputFlag = checkInputFlag + 2;//longitudeが範囲外
                }


                switch (checkInputFlag)
                {
                    case 0:
                        {
                            UIToNoteView();
                            //IsolatedStorageSettings.ApplicationSettings["EditingType"] = "";
                            doNotResume = true;
                            NavigationService.GoBack();
                            break;
                        }
                    case 1:
                        {
                            MessageBox.Show("Latitude is out of range." + System.Environment.NewLine + "Latitude must be..." + System.Environment.NewLine + "-90 < Latitude < 90");

                            break;
                        }
                    case 2:
                        MessageBox.Show("Longitude is out of range." + System.Environment.NewLine + "Longitude must be..." + System.Environment.NewLine + "-180 < Longitude < 180");

                        break;
                    case 3:
                        MessageBox.Show("Latitude and Longitude are out of range." + System.Environment.NewLine + "Location data must be..." + System.Environment.NewLine + "-90 < Latitude < 90" + System.Environment.NewLine + "-180 < Longitude < 180");
                        break;
                }
                #endregion
            }

            */
        }
        
        //UIに表示されている情報をNoteModelに格納→NoteViewに追加→XMLで保存
        private void UIToNoteView()
        {
            if (EditingType == "New")
            {
                NewNote = new NoteModel();
                if (TextBox_Latitude.Text != "" &&TextBox_Longitude.Text!="")
                {
                    NewNote.Location = new GeoCoordinate(Convert.ToDouble(TextBox_Latitude.Text), Convert.ToDouble(TextBox_Longitude.Text));
                }
                else
                {
                    NewNote.Location = GeoCoordinate.Unknown;
                }

                NewNote.Title = TextBox_Title.Text;
                NewNote.Content = TextBox_Note.Text;
                NewNote.CreateDate =DateTime.Parse( TextBlock_CreateDate.Text);
                //NewNote.CreateDayOfWeek = DateTime.Parse( NewNote.CreateDate).DayOfWeek.ToString();
                NewNote.ModifyDate =DateTime.Parse( TextBlock_ModifyDate.Text);
                NewNote.ModifyDayOfWeek = NewNote.ModifyDate.DayOfWeek;
                NewNote.Address = TextBox_Address.Text;
                NewNote.Shared = Visibility.Collapsed;

                NewNote.OpenDetailDate = DateTime.Now;

                NoteView.Notes.Add(NewNote);

                (Application.Current as App).NewestNoteID = NewNote.CreateDate;
            }
            else if (EditingType == "Modify")
            {
                if (TextBox_Latitude.Text != "" && TextBox_Longitude.Text != "")
                {
                    ModifyingNote.Location = new GeoCoordinate(Convert.ToDouble(TextBox_Latitude.Text), Convert.ToDouble(TextBox_Longitude.Text));
                }
                else
                {
                    ModifyingNote.Location = GeoCoordinate.Unknown;
                }

                ModifyingNote.Title = TextBox_Title.Text;
                ModifyingNote.Content = TextBox_Note.Text;
                ModifyingNote.ModifyDate = DateTime.Now;
                ModifyingNote.ModifyDayOfWeek =ModifyingNote.ModifyDate.DayOfWeek;
                ModifyingNote.Address = TextBox_Address.Text;
            }

            //ページを離れるときにセーブしないようにするため
            //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;

            //DB.SaveInfoToIsoStrage(NoteView);

        }
        

        private void TextBox_Title_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                e.Handled=true;
                //TextBox_Note.Focus();
                this.Focus();
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure to delete ''" + TextBox_Title.Text + "'' ?", "Delete", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                //IsolatedStorageSettings.ApplicationSettings["Editing"] = "";
                if (EditingType == "New")
                {
                    //NavigationService.GoBack();
                }
                else if(EditingType=="Modify")
                {
                    NoteView.Notes.Remove(ModifyingNote);

                }

                string imageFilePath = ImageDIR + "/" + DateTime.Parse(TextBlock_CreateDate.Text).ToFileTimeUtc();

                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isoStore.DeleteFile(imageFilePath);
                    IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = "";
                }

                doNotResume = true;
                NavigationService.GoBack();

            }

        }

        //bool backkeypressFlag = false;
        //物理的な戻るボタンが押された時の処理
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //IsolatedStorageSettings.ApplicationSettings["EditingType"] = "";

            //ページを離れるときにセーブしないようにするため
            //StrBefore = TextBox_Title.Text + TextBox_Note.Text + TextBox_Latitude.Text + TextBox_Longitude.Text + TextBox_Address.Text;
            IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = "";

            doNotResume = true;
            base.OnBackKeyPress(e);
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
           (sender as TextBox).SelectAll();
        }

        private void TextBox_Title_TextChanged(object sender, TextChangedEventArgs e)
        {
            Panorama.Title = TextBox_Title.Text;
        }


        private void TextBox_Note_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                //e.Handled = true;
                this.ScrollViewer_Note.UpdateLayout();
                this.ScrollViewer_Note.ScrollToVerticalOffset(this.TextBox_Note.ActualHeight);
            }
        }

        private void TextBox_Note_TextInputStart(object sender, TextCompositionEventArgs e)
        {

            this.ScrollViewer_Note.UpdateLayout();
            this.ScrollViewer_Note.ScrollToVerticalOffset(this.TextBox_Note.ActualHeight);
        }


        private void TextBox_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (sender as PhoneTextBox).SelectAll();

        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            getAddressAndLocation("Download");

        }

        private void CameraButton_Click(object sender, EventArgs e)
        {
            
            try
            {
                cameraCaptureTask.Show();
            }
            catch
            {

            }
        }

        void cameraCaptureTask_Completed(object sender, Microsoft.Phone.Tasks.PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bi = new BitmapImage();
                bi.SetSource(e.ChosenPhoto);


                //writeableBitmap.LoadJpeg(e.ChosenPhoto);

                string imageFilePath = ImageDIR + "/" +DateTime.Parse(TextBlock_CreateDate.Text).ToFileTimeUtc();

                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream strm = new IsolatedStorageFileStream(imageFilePath, FileMode.OpenOrCreate, FileAccess.Write, isoFile);

                WriteableBitmap wb = new WriteableBitmap(bi);

                wb.SaveJpeg(strm, wb.PixelWidth, wb.PixelHeight, 0, 100);

                strm.Dispose();
                isoFile.Dispose();

                /*
                using (var isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var stream = isoFile.CreateFile(imageFilePath))
                    {
                        writeableBitmap.SaveJpeg(stream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 100);
                    }
                }
                */

                loadPicture(imageFilePath);

            }
        }


        void loadPicture(string path)
        {
            
            //BitmapImage imageFromStorage = new BitmapImage();

            using (var isoFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoFile.FileExists(path) == false)
                    return;

                using(var strm = isoFile.OpenFile(path, FileMode.Open))
                {
                    BitmapImage bi = new BitmapImage();

                    bi.SetSource(strm);
                    Image_Picture.Source = bi;
                    transform.Rotation = 90d;//Angle = 90d;

                    /*
                    ManipulateExif maniExif=new ManipulateExif();
                    var orientation = maniExif.GetJpegOrientation(strm);

                    switch (orientation)
                    { 
                        case 1:
                            ImageRotate.Angle = 0d;
                            break;
                        case 2:
                            break;
                        case 3:
                            ImageRotate.Angle = 180d;
                            break;
                        case 4:
                            break;
                        case 5:
                            ImageRotate.Angle = 270d;
                            break;
                        case 6:
                            ImageRotate.Angle = 90d;
                            break;
                        case 7:
                            ImageRotate.Angle = 90d;
                            break;
                        case 8:
                            ImageRotate.Angle = 270d;
                            break;

                        default:
                            break;

                    }
                     */
                }
            }
            

            
        }

        // double initialAngle;
        double initialScale;

        /*ダブルタップすると、画像を初期位置初期サイズに戻します。*/
        private void OnDoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            transform.ScaleX = transform.ScaleY = 1;
            transform.TranslateX = transform.TranslateY = 0;
        }

        private void OnHold(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
            transform.ScaleX = transform.ScaleY = 1;
            transform.Rotation = 0;
        }

        /*ドラッグで画像を移動させています。*/
        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            transform.TranslateX += e.HorizontalChange;
            transform.TranslateY += e.VerticalChange;
        }

        /*ピンチアウトする前にサイズを保存しておきます、コメントアウトを外すと回転もできます。*/
        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            //initialAngle = transform.Rotation;
            initialScale = transform.ScaleX;
        }

        /*ピンチアウト中ですね、倍率を見て拡大しています。コメントアウトを外すと回転もできます。*/
        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            //transform.Rotation = initialAngle + e.TotalAngleDelta;
            transform.ScaleX = transform.ScaleY = initialScale * e.DistanceRatio;
        }



    }
}