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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;


namespace SmallNote
{
    public partial class App : Application
    {
        /// <summary>
        /// Phone アプリケーションのルート フレームへの容易なアクセスを提供します。
        /// </summary>
        /// <returns>Phone アプリケーションのルート フレームです。</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Application オブジェクトのコンストラクターです。
        /// </summary>
        public App()
        {
            // キャッチできない例外のグローバル ハンドラーです。 
            UnhandledException += Application_UnhandledException;

            // Silverlight の標準初期化
            InitializeComponent();

            // Phone 固有の初期化
            InitializePhoneApplication();

            // デバッグ中にグラフィックスのプロファイル情報を表示します。
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 現在のフレーム レート カウンターを表示します。
                Application.Current.Host.Settings.EnableFrameRateCounter = false;

                // 各フレームで再描画されているアプリケーションの領域を表示します。
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // 試験的な分析視覚化モードを有効にします。 
                // これにより、色付きのオーバーレイを使用して、GPU に渡されるページの領域が表示されます。
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // アプリケーションの PhoneApplicationService オブジェクトの UserIdleDetectionMode プロパティを Disabled に設定して、
                // アプリケーションのアイドル状態の検出を無効にします。
                // 注意: これはデバッグ モードのみで使用してください。ユーザーが電話を使用していないときに、ユーザーのアイドル状態の検出を無効にする
                // アプリケーションが引き続き実行され、バッテリ電源が消耗します。
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        public DataBase DB { get; set; }
        public ViewModel NoteView { get; set; }
        public DateTime NewestNoteID { get; set; }

        // (たとえば、[スタート] メニューから) アプリケーションが起動するときに実行されるコード
        // このコードは、アプリケーションが再アクティブ化済みの場合には実行されません
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            if (DB == null)
                DB = new DataBase();
            NoteView = DB.LoadInfoFromXML();


            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingTitle") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingTitle"] = null;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingContent") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingContent"] = "";
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingCreateDate") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingCreateDate"] =0;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingLatitude") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingLatitude"] =0;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingLongitude") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingLongitude"] =0;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("Email") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["Email"] = "";
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("FirstTimeMessagePop") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["FirstTimeMessagePop"] = false;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationService") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["LocationService"] = false;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Resume") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["Resume"] = false;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("EditingType") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["EditingType"] = "";
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("SelectedItemID") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["SelectedItemID"] = "";
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Sort") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["Sort"] = "Title";
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("SearchKeyword") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["SearchKeyword"] = "";
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("ResumingImageFilePath") == false)
            {
                IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = "";
            }


            /*
            if ((string)IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] != "")
            {
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isoStore.DeleteFile((string)IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"]);
                    IsolatedStorageSettings.ApplicationSettings["ResumingImageFilePath"] = "";
                }
            }
            */
        }

        // アプリケーションがアクティブになった (前面に表示された) ときに実行されるコード
        // このコードは、アプリケーションの初回起動時には実行されません
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // アプリケーションが非アクティブになった (バックグラウンドに送信された) ときに実行されるコード
        // このコードは、アプリケーションの終了時には実行されません
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            DB.SaveInfoToIsoStrage(NoteView);

            updateLiveTile();


        }

        // (たとえば、ユーザーが戻るボタンを押して) アプリケーションが終了するときに実行されるコード
        // このコードは、アプリケーションが非アクティブになっているときには実行されません
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            DB.SaveInfoToIsoStrage(NoteView);

            updateLiveTile();
        }

        // ナビゲーションに失敗した場合に実行されるコード
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            DB.SaveInfoToIsoStrage(NoteView);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // ナビゲーションに失敗しました。デバッガーで中断します。
                //System.Diagnostics.Debugger.Break();
                MessageBox.Show("App Crashed.Sorry");
            }
        }

        // ハンドルされない例外の発生時に実行されるコード
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // ハンドルされない例外が発生しました。デバッガーで中断します。
                //System.Diagnostics.Debugger.Break();
                MessageBox.Show("App Crashed.Sorry.");
            }
        }


        private void updateLiveTile()
        {
            ShellTile tileToUpdate = ShellTile.ActiveTiles.First();    // 2if (tileToFind == null)
            if(tileToUpdate!=null)
            {
                var tile = new StandardTileData();// 3
                
                tile.Title = "SmallNote";
                if (NoteView.Notes.Count() != 0)
                {
                    var note = NoteView.GetNoteByTimeStamp(NewestNoteID);//最後に開いたnoteを取得
                    //BackgroundImage = new Uri("Background.png", UriKind.Relative),
                    if (note != null)
                    {

                        tile.BackTitle = note.Title;
                        //tile.BackBackgroundImage = new Uri("BackBackground.png", UriKind.Relative);
                        tile.BackContent = note.Content;
                    }

                    tile.Count = NoteView.Notes.Count();
                }
                else
                {
                    //BackgroundImage = new Uri("Background.png", UriKind.Relative),
                    tile.Count = 0;
                    tile.BackTitle = "";
                    //tile.BackBackgroundImage = new Uri("BackBackground.png", UriKind.Relative);
                    tile.BackContent = "No Items";
                }

                tileToUpdate.Update(tile);

            }
        }

        /*
        private void updateLiveTile()
        {
   
            Version TargetVersion = new Version(7, 10, 8858);
            if (Environment.OSVersion.Version >= TargetVersion)
            {

                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData,Microsoft.Phone");
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile,Microsoft.Phone");
                var tileToUpdate = ShellTile.ActiveTiles.First();

                if (tileToUpdate != null)
                {
                    var UpdateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                    // Set the properties. 
                    SetProperty(UpdateTileData, "Title", "SmallNote");

                    //SetProperty(UpdateTileData, "SmallBackgroundImage","");
                    //SetProperty(UpdateTileData, "BackBackgroundImage", "");
                    //SetProperty(UpdateTileData, "WideBackgroundImage", "");
                    //SetProperty(UpdateTileData, "WideBackBackgroundImage", "")
                    if (NoteView.Notes.Count() != 0)
                    {
                        var note = NoteView.GetNoteByTimeStamp(NewestNoteID);//最後に開いたnoteを取得
                        if (note != null)
                        {
                            SetProperty(UpdateTileData, "WideBackContent", note.Content);
                            SetProperty(UpdateTileData, "BackTitle", note.Title);
                            SetProperty(UpdateTileData, "BackContent", note.Content);
                        }
                        SetProperty(UpdateTileData, "Count", NoteView.Notes.Count);
                        shellTileType.GetMethod("Update").Invoke(tileToUpdate, new Object[] { UpdateTileData });
                    }
                    else
                    {
                        SetProperty(UpdateTileData, "WideBackContent", "");
                        SetProperty(UpdateTileData, "Count", 0);
                        SetProperty(UpdateTileData, "BackTitle", "");
                        SetProperty(UpdateTileData, "BackContent", "");

                        shellTileType.GetMethod("Update").Invoke(tileToUpdate, new Object[] { UpdateTileData });
                    }
                    // Invoke the new version of ShellTile.Update.

                }
            }
        }
        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }
        */

        #region Phone アプリケーションの初期化

        // 初期化の重複を回避します
        private bool phoneApplicationInitialized = false;

        // このメソッドに新たなコードを追加しないでください
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // フレームを作成しますが、まだ RootVisual に設定しないでください。これによって、アプリケーションがレンダリングできる状態になるまで、
            // スプラッシュ スクリーンをアクティブなままにすることができます。
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();

            RootFrame.Language = System.Windows.Markup.XmlLanguage.GetLanguage(
                System.Globalization.CultureInfo.CurrentUICulture.Name);


            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // ナビゲーション エラーを処理します
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // 再初期化しないようにします
            phoneApplicationInitialized = true;
        }

        // このメソッドに新たなコードを追加しないでください
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // ルート visual を設定してアプリケーションをレンダリングできるようにします
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // このハンドラーは必要なくなったため、削除します
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}