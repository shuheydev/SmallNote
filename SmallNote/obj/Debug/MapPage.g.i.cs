﻿#pragma checksum "E:\Document\プログラミング\windows phone app\Projects\SmallNote2\SmallNote\MapPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0585FF99747CF8354F9CA06D438C9F07"
//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.18046
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace SmallNote {
    
    
    public partial class MapPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.Maps.Map MyMap;
        
        internal Microsoft.Phone.Controls.Maps.MapTileLayer MapTileLayer_gMap;
        
        internal Microsoft.Phone.Controls.Maps.Pushpin CurrentMark;
        
        internal System.Windows.Media.RotateTransform CurrentMarkRotate;
        
        internal Microsoft.Phone.Controls.Maps.MapItemsControl MapItemsControl_PushPins;
        
        internal System.Windows.Controls.Grid Grid_ZoomUp;
        
        internal System.Windows.Controls.Grid Grid_GPSStatus;
        
        internal System.Windows.Controls.TextBlock TextBlock_GPSStatus;
        
        internal System.Windows.Controls.Grid Grid_ZoomDown;
        
        internal System.Windows.Controls.Grid Grid_GoCurrentLocation;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton Appbar_MainPageButton;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/SmallNote;component/MapPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.MyMap = ((Microsoft.Phone.Controls.Maps.Map)(this.FindName("MyMap")));
            this.MapTileLayer_gMap = ((Microsoft.Phone.Controls.Maps.MapTileLayer)(this.FindName("MapTileLayer_gMap")));
            this.CurrentMark = ((Microsoft.Phone.Controls.Maps.Pushpin)(this.FindName("CurrentMark")));
            this.CurrentMarkRotate = ((System.Windows.Media.RotateTransform)(this.FindName("CurrentMarkRotate")));
            this.MapItemsControl_PushPins = ((Microsoft.Phone.Controls.Maps.MapItemsControl)(this.FindName("MapItemsControl_PushPins")));
            this.Grid_ZoomUp = ((System.Windows.Controls.Grid)(this.FindName("Grid_ZoomUp")));
            this.Grid_GPSStatus = ((System.Windows.Controls.Grid)(this.FindName("Grid_GPSStatus")));
            this.TextBlock_GPSStatus = ((System.Windows.Controls.TextBlock)(this.FindName("TextBlock_GPSStatus")));
            this.Grid_ZoomDown = ((System.Windows.Controls.Grid)(this.FindName("Grid_ZoomDown")));
            this.Grid_GoCurrentLocation = ((System.Windows.Controls.Grid)(this.FindName("Grid_GoCurrentLocation")));
            this.Appbar_MainPageButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("Appbar_MainPageButton")));
        }
    }
}

