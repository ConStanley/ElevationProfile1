using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace Viewer32.AddIns
{
    [Export(typeof(ICommand))]
    [DisplayName("My Tool")]
    public class ELEV: ICommand, ISupportsConfiguration
    {
        private MyConfigDialog configDialog = new MyConfigDialog();

        #region ISupportsConfiguration members

        public void Configure()
        {
            // When the dialog opens, it shows the information saved from the last configuration
            MapApplication.Current.ShowWindow("Configuration", configDialog);
        }

        public void LoadConfiguration(string configData)
        {
            // Initialize the behavior's configuration with the saved configuration data. 
            // The dialog's textbox is used to store the configuration.
            configDialog.InputTextBox.Text = configData ?? "";
        }

        public string SaveConfiguration()
        {
            // Save the information from the configuration dialog
            return configDialog.InputTextBox.Text;
        }

        #endregion

        private DrawingToolsDialog _toolsDialog;

        #region ICommand members
        public void Execute(object parameter)
        {
            if (_toolsDialog == null)
                _toolsDialog = new DrawingToolsDialog() { Margin = new Thickness(10, 10, 0, 10) };

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
