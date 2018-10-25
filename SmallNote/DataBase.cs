using System;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Device.Location;
using System.Windows.Media;


namespace SmallNote
{
    public class DataBase
    {

        private const string DBFileName = "NoteInfo.xml";
        private const string ImageDIR = "Images";

        public DataBase()
        {
            InitDB();
        }

        //分離ストレージのDBファイルの初期化
        private void InitDB()
        {
            //分離ストレージの初期化。ファイルが存在しなければ、新しく作る。
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isoFile.FileExists(DBFileName))
            {
                //xmlドキュメントの初期化
                XDocument XmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Notes"));


                //xmlドキュメントをファイルへ書き込む
                IsolatedStorageFileStream strm = new IsolatedStorageFileStream(DBFileName, FileMode.Create, FileAccess.ReadWrite, isoFile);
                XmlDoc.Save(strm);
                strm.Dispose();//ストリームを閉じる

            }

            //書籍画像保存用ディレクトリの作成
            if (!isoFile.DirectoryExists(ImageDIR))//ディレクトリがない場合
            {
                isoFile.CreateDirectory(ImageDIR);//ディレクトリ作成

                /*
                //画像が取得できなかったとき用の画像をimages/0として保存
                StreamResourceInfo source = Application.GetResourceStream(new Uri("Images/appbar.questionmark.rest.png", UriKind.Relative));
                BitmapImage bi = new BitmapImage();
                bi.SetSource(source.Stream);

                IsolatedStorageFileStream strm = new IsolatedStorageFileStream(ImageDIR + "/0", FileMode.Create, FileAccess.ReadWrite, isoFile);
                WriteableBitmap wb = new WriteableBitmap(bi);
                wb.SaveJpeg(strm, wb.PixelWidth, wb.PixelHeight, 0, 100);

                strm.Dispose();
                 */

            }

            isoFile.Dispose();




        }


        public ViewModel LoadInfoFromXML()
        {
            //目的地データの読み込み。
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream strm = new IsolatedStorageFileStream(DBFileName, FileMode.Open, FileAccess.Read, isoFile);
            XDocument xmlDoc = XDocument.Load(strm);
            strm.Dispose();
            isoFile.Dispose();

            var notes = from note in xmlDoc.Descendants("Note")
                        orderby note.Element("ModifyDate").Value descending
                               select note;

            ViewModel NoteView = new ViewModel();
            NoteModel newNote;//=new PushPinModel();
            foreach (var note in notes)
            {
                newNote = new NoteModel();

                newNote.Title = note.Element("Title").Value;
                newNote.Content = note.Element("Content").Value;

                try
                {
                    newNote.CreateDate = DateTime.FromFileTime((long)Convert.ToDouble(note.Element("CreateDate").Value));
                }
                catch
                {
                    newNote.CreateDate = DateTime.Parse(note.Element("CreateDate").Value);
                }

                try
                {
                    newNote.ModifyDate = DateTime.FromFileTime((long)Convert.ToDouble(note.Element("ModifyDate").Value));
                }
                catch
                {
                    newNote.ModifyDate = DateTime.Parse(note.Element("ModifyDate").Value);
                }

                if (note.Element("Latitude").Value != "NaN")
                    newNote.Location = new GeoCoordinate(Convert.ToDouble(note.Element("Latitude").Value), Convert.ToDouble(note.Element("Longitude").Value));
                else
                    newNote.Location = GeoCoordinate.Unknown;
                newNote.Resume = Convert.ToBoolean(note.Element("Resume").Value);
                if (newNote.Resume == true)
                {
                    newNote.Visibility = Utility.GetVisibilityFromString("Collapsed");
                }
                else
                {
                    newNote.Visibility = Utility.GetVisibilityFromString("Visible");
                }
                newNote.Selected = Convert.ToBoolean(note.Element("Selected").Value);


                newNote.CreateDayOfWeek = newNote.CreateDate.DayOfWeek;
                newNote.ModifyDayOfWeek = newNote.ModifyDate.DayOfWeek;
                newNote.Address = note.Element("Address").Value;
                newNote.Shared = Utility.GetVisibilityFromString(note.Element("Shared").Value);


                try
                {
                    newNote.OpenDetailDate = DateTime.FromFileTime((long)Convert.ToDouble(note.Element("OpenDetailDate").Value));
                }
                catch
                {
                    newNote.OpenDetailDate = DateTime.Parse(note.Element("OpenDetailDate").Value);
                }

                NoteView.Notes.Add(newNote);
            }


            return NoteView;
        }






        public void SaveInfoToIsoStrage(ViewModel NoteView)
        {
            //xmlドキュメントの初期化
            XDocument xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Notes"));

            //コレクションのアイテムをひとつづつXMLに入れていく
            foreach (var note in NoteView.Notes)
            {
                XElement noteElement = new XElement("Note");

                noteElement.Add(new XElement("Latitude", note.Location.Latitude));
                noteElement.Add(new XElement("Longitude", note.Location.Longitude));
                noteElement.Add(new XElement("Title", note.Title));
                noteElement.Add(new XElement("Content", note.Content));
                noteElement.Add(new XElement("CreateDate", note.CreateDate.ToUniversalTime()));
                noteElement.Add(new XElement("ModifyDate", note.ModifyDate.ToUniversalTime()));
                noteElement.Add(new XElement("Visibility",note.Visibility.ToString()));

                //destination.Add(new XElement("IsNewNote", dest.IsNewNote));
                noteElement.Add(new XElement("Selected", note.Selected));
                noteElement.Add(new XElement("Resume", note.Resume));
                //System.Diagnostics.Debug.WriteLine(dest.CreateDate.ToFileTime());
                noteElement.Add(new XElement("Address",note.Address));
                noteElement.Add(new XElement("Shared",note.Shared.ToString()));
                try
                {
                    noteElement.Add(new XElement("OpenDetailDate", note.OpenDetailDate.ToUniversalTime()));
                }
                catch
                {
                    noteElement.Add(new XElement("OpenDetailDate", DateTime.Now.ToUniversalTime()));
                }
                xmlDoc.Root.Add(noteElement);
            }


            //XMLファイルを分離ストレージに保存。

            //ロードとセーブでそれぞれストリームを開いて閉じること。でなければ正しく保存できない。
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

            //xmlファイルの更新。FileMode.Createとすると、ファイルが存在する場合、同名の（空の）ファイルを新規に作り直してくれる。
            //ここでもしFileMode.Openにすると、下の書き込みのときに、
            //元の内容を一文字ずつ上書きしていく形でファイルの先頭から文字列（XML）
            //が書き込まれていくようで、
            //要素が削除されて短くなっているので、
            //その後ろに以前の文がはみだしてくっついてしまう。
            //そのため、次に読み込むときにエラーが出てしまう。
            //ほんと、要注意。
            //なんでこんな仕様なの？XMLのときだけ？
            //やってることはファイルを消してまた書き込んでいるだけなので、
            //ファイルが大きくなった場合が心配。
            //データ操作のオーバーヘッドはどうなるんだろう？
            //
            IsolatedStorageFileStream strm = new IsolatedStorageFileStream(DBFileName, FileMode.Create, FileAccess.Write, isoFile);
            //xmlDoc.Save(strm);
            xmlDoc.Save(strm);
            strm.Dispose();//セーブしたら閉じる
            isoFile.Dispose();
        }


    }
}