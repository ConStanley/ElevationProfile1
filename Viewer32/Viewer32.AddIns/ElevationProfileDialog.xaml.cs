using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Viewer32.AddIns
{
    public partial class ElevationProfileDialog : UserControl
    {
        public ElevationProfileDialog()
        {
            InitializeComponent();
        }

        internal GraphicsLayer PointLayer { get; set; }

        private double _profileDistance;
        internal double ProfileDistance
        {
            get { return _profileDistance; }
            set
            {
                _profileDistance = value;
                lblDistance.Text = string.Format("Total Distance {0} Miles", _profileDistance);
            }
        }

        private PointCollection _profileData;
        internal PointCollection ProfileData
        {
            get { return _profileData; }
            set
            {
                _profileData = value;
                (ElevationChart.Series[0] as LineSeries).ItemsSource = _profileData;
            }
        }

        private void ElevationChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(((System.Windows.FrameworkElement)(((System.Windows.RoutedEventArgs)(e)).OriginalSource)) is Ellipse))
            {
                (PointLayer.Graphics[0].Geometry as MapPoint).X = Double.NaN;
                (PointLayer.Graphics[0].Geometry as MapPoint).Y = Double.NaN;
            }
            else
            {
                Ellipse chartPoint = ((System.Windows.FrameworkElement)(((System.Windows.RoutedEventArgs)(e)).OriginalSource)) as Ellipse;
                MapPoint mapPoint = chartPoint.DataContext as MapPoint;

                (PointLayer.Graphics[0].Geometry as MapPoint).X = mapPoint.X;
                (PointLayer.Graphics[0].Geometry as MapPoint).Y = mapPoint.Y;
            }
        }
    }
}
