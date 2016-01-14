/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElevationProfile.AddIns
{
    public partial class DrawingToolsDialog : UserControl
    {
        Draw _myDrawObject;
        GraphicsLayer _lineGraphicLayer;
        GraphicsLayer _pointGraphicLayer;
        CancellationTokenSource _cts;
        ElevationProfileDialog _elevationProfileDialog;

        public DrawingToolsDialog()
        {
            InitializeComponent();

            var lineRenderer = (SimpleRenderer)LayoutRoot.Resources["LineRenderer"];
            _lineGraphicLayer = new GraphicsLayer() 
            { 
                ShowLegend = false,
                Renderer = lineRenderer
            };
            _lineGraphicLayer.Graphics.Add(new Graphic()
            {
                Geometry = null,
            });
            MapApplication.Current.Map.Layers.Add(_lineGraphicLayer);

            _pointGraphicLayer = new GraphicsLayer() 
            { 
                ShowLegend = false,
                Renderer = (SimpleRenderer)LayoutRoot.Resources["PointRenderer"]
            };
            _pointGraphicLayer.Graphics.Add(new Graphic()
            {
                Geometry = new MapPoint(Double.NaN, Double.NaN)
            });
            MapApplication.Current.Map.Layers.Add(_pointGraphicLayer);

            _myDrawObject = new Draw(MapApplication.Current.Map)
            {
                LineSymbol = (LineSymbol)lineRenderer.Symbol
            };
            _myDrawObject.DrawComplete += _myDrawObject_DrawComplete;

            _elevationProfileDialog = new ElevationProfileDialog()
            {
                PointLayer = _pointGraphicLayer
            };
        }

        internal void CloseProfile()
        {
            MapApplication.Current.HideWindow(_elevationProfileDialog);
        }

        internal void ClearLayers()
        {
            _lineGraphicLayer.Graphics[0].Geometry = null;
            (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).X = Double.NaN;
            (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).Y = Double.NaN;
        }

        private void Tool_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseProfile();

            switch ((sender as Button).Tag as string)
            {
                case "DrawPolyline":
                    _myDrawObject.DrawMode = DrawMode.Polyline;
                    _myDrawObject.IsEnabled = true;
                    break;
                case "DrawFreehand":
                    _myDrawObject.DrawMode = DrawMode.Freehand;
                    _myDrawObject.IsEnabled = true;
                    break;
                default:
                    _myDrawObject.DrawMode = DrawMode.None;
                    break;
            }
        }

        private async void _myDrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (e.Geometry == null)
            {
                MapApplication.Current.HideWindow(_elevationProfileDialog);
                return;
            }

            try
            {
                _myDrawObject.IsEnabled = false;

                ProfileCalculationBusyIndicator.Visibility = Visibility.Visible;

                _lineGraphicLayer.Graphics[0].Geometry = e.Geometry;

                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                Geoprocessor geoprocessorTask = new Geoprocessor(
                    "http://elevation.arcgis.com/arcgis/rest/services/Tools/ElevationSync/GPServer/Profile");

                List<GPParameter> parameters = new List<GPParameter>();
                parameters.Add(new GPFeatureRecordSetLayer("InputLineFeatures", e.Geometry));
                parameters.Add(new GPString("returnM", "true"));
                parameters.Add(new GPString("returnZ", "true"));

                GPExecuteResults results = await geoprocessorTask.ExecuteTaskAsync(parameters, _cts.Token);

                if (results == null || results.OutParameters.Count == 0 || (results.OutParameters[0] as GPFeatureRecordSetLayer).FeatureSet.Features.Count == 0)
                {
                    MessageBox.Show("Fail to get elevation data. Draw another line");
                    return;
                }

                ESRI.ArcGIS.Client.Geometry.Polyline elevationLine =
                    (results.OutParameters[0] as GPFeatureRecordSetLayer).FeatureSet.Features[0].Geometry
                    as ESRI.ArcGIS.Client.Geometry.Polyline;

                foreach (MapPoint p in elevationLine.Paths[0])
                {
                    p.M = Math.Round(p.M / 1000, 2);
                    p.Z = Math.Round(p.Z, 2);
                }

                MapPoint lastPoint = elevationLine.Paths[0][elevationLine.Paths[0].Count - 1];

                _elevationProfileDialog.ProfileDistance = lastPoint.M;
                _elevationProfileDialog.ProfileData = elevationLine.Paths[0];


                var top = ((FrameworkElement)Application.Current.RootVisual).ActualHeight - 400;
                if (MapApplication.Current.IsEditMode)
                    top -= 20;
                MapApplication.Current.ShowWindow("", _elevationProfileDialog, false, (s, a) => ClearLayers(), null, WindowType.Floating, top);
            }
            catch (Exception ex)
            {
                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(),
                        (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
            finally
            {
                ProfileCalculationBusyIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
