/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ElevationProfile.AddIns
{
    [Export(typeof(ICommand))]
    [DisplayName("Elevation Profile")]
    [Description("Retrieve the elevation profile along a line drawn on the map")]
    [Category("Analysis")]
    public class ElevationProfileTool : ICommand
    {
        private DrawingToolsDialog _toolsDialog;

        #region ICommand members
        public void Execute(object parameter)
        {
            if (_toolsDialog == null)
                _toolsDialog = new DrawingToolsDialog() { Margin = new Thickness(10,10,0,10) };

            var rootVisual = (FrameworkElement)Application.Current.RootVisual;
            var left = rootVisual.ActualWidth - 350;
            var top = MapApplication.Current.IsEditMode ? 250 : 100;

            MapApplication.Current.ShowWindow("Elevation Profile Draw Tools", _toolsDialog, false, (s, a) => _toolsDialog.CloseProfile(), null, WindowType.Floating, top, left);
        }

        public bool CanExecute(object parameter)
        {
            // Return true so that the command can always be executed
            return true;
        }

        public event EventHandler CanExecuteChanged;

        #endregion
    }
}
