using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SmallNote
{
    public class Utility
    {

        public static System.Windows.Visibility GetVisibilityFromString(string v)
        {
            System.Windows.Visibility visibility = System.Windows.Visibility.Collapsed;
            //MessageBox.Show("util "+v);
            if (v == System.Windows.Visibility.Visible.ToString())
                visibility = System.Windows.Visibility.Visible;
            if (v == System.Windows.Visibility.Collapsed.ToString())
                visibility = System.Windows.Visibility.Collapsed;

            return visibility;
        }


    }
}
