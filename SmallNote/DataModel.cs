using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Phone.Controls;
using System.Windows.Media;
using System.Device.Location;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SmallNote
{

    public class ViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<NoteModel> _notes;
        public ObservableCollection<NoteModel> Notes
        {
            get { return _notes; }
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged("Notes");
                }
            }
        }

        public ViewModel()
        {
            this.Notes = new ObservableCollection<NoteModel>();
        }


        public NoteModel GetSelcted()
        {
            var items = from item in this.Notes
                        where item.Selected == true
                        select item;

            if (items.Count() != 0)
            {
                return items.First();
            }
            else
            {
                return null;
            }
        }

        public void SetSelected(NoteModel item)
        {
            if (item == null)
                return;
            this.ClearSelected();
            item.Selected = true;
            //item.Color = new SolidColorBrush(Utility.GetColorFromHexString(SelectedColor_Hex));
        }

        public void ClearSelected()
        {
            var prev_selectedNote = this.GetSelcted();
            if (prev_selectedNote != null)
            {
                prev_selectedNote.Selected = false;
            }
        }

        public NoteModel GetResume()
        {
            var items = from item in this.Notes
                        where item.Resume == true
                        select item;

            if (items.Count() != 0)
            {
                return items.First();
            }
            else
            {
                return null;
            }
        }

        public NoteModel GetNoteByTimeStamp(DateTime timeStamp)
        {
            var items = from item in this.Notes
                        where item.CreateDate == timeStamp
                        select item;

            if (items.Count() != 0)
            {
                return items.First();
            }
            else
            {
                return null;
            }
        }

    }      
               

    public class NoteModel : INotifyPropertyChanged
    {
        public NoteModel() 
        {
            Shared = Visibility.Collapsed;
            Content = "";
            Title = "";
            CreateDate = new DateTime(0);
            ModifyDate = new DateTime(0);
            Location = new GeoCoordinate();
            OpenDetailDate = new DateTime(0);
        
        }


        private DateTime _createDate;
        private DateTime _modifyDate;
        private string _title;
        private string _content;
        private GeoCoordinate _location;
        private Visibility _shared;
        private DateTime _openDetailDate;

        //public bool IsNewNote = true;
        public bool Selected=false;
        public bool Resume = false;
        public Visibility Visibility { get; set; }


        //リストの表示に使用。表示時に内容を生成するので、xmlに保存しない。
        public DayOfWeek CreateDayOfWeek { get; set; }
        public DayOfWeek ModifyDayOfWeek { get; set; }
        public string Address { get; set; }


        public Visibility Shared
        {
            get
            {
                return _shared;
            }
            set
            {
                if (_shared != value)
                {
                    _shared = value;
                    OnPropertyChanged("VIsibility");
                }
            }
        }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public DateTime CreateDate
        {
            get
            {
                return _createDate;
            }
            set
            {
                if (_createDate != value)
                {
                    _createDate = value;
                    OnPropertyChanged("CreateDate");
                }
            }
        }

        public DateTime ModifyDate
        {
            get
            {
                return _modifyDate;
            }
            set
            {
                if (_modifyDate != value)
                {
                    _modifyDate = value;
                    OnPropertyChanged("ModifyDate");
                }
            }
        }

        public GeoCoordinate Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        public DateTime OpenDetailDate
        {
            get
            {
                return _openDetailDate;
            }
            set
            {
                if (_openDetailDate != value)
                {
                    _openDetailDate = value;
                    OnPropertyChanged("OpenDetailDate");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}
