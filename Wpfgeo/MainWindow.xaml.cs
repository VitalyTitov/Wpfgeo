using GMap.NET;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpfgeo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            MapView.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //MapView.MapProvider = GMapProviders.GoogleMap;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            MapView.ShowCenter = false;
            MapView.MinZoom = 2;
            MapView.MaxZoom = 17;
            MapView.Zoom = 10;
            MapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            MapView.CanDragMap = true;
            MapView.DragButton = MouseButton.Left;
            MapView.Position = new GMap.NET.PointLatLng(48.866383, 2.323575);
            
        }
        private void MapSearch(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textBox.Text))
            {
                MessageBox.Show("Enter Value Please.");
            }
            else
            {
                string url = string.Format(
                        "https://nominatim.openstreetmap.org/search?q={0}&polygon_geojson=1&format=geocodejson&limit=1",
                        Uri.EscapeDataString(textBox.Text));

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.UserAgent = ".NET Framework Test Client";

                string jsonString;
                using (var resp = request.GetResponse())
                {
                    var results = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    // results = results.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    jsonString = results;
                }

                File.WriteAllText(@"D:\map.json", jsonString);
                JObject openfile = JsonConvert.DeserializeObject<JObject>(jsonString);

                string сoordinates = Convert.ToString(openfile["features"][0]["geometry"]["coordinates"]);
                Regex regex = new Regex(@"([\d.]+)");
                List<string> PointСollection = regex.Matches(сoordinates).Cast<Match>().Select(match => match.Value).ToList();
                List<PointLatLng> points = new List<PointLatLng>();
                int frequency = 2;
                if (comboBox.Text != "")
                {
                    frequency = Convert.ToInt32(comboBox.Text);
                }
                for (int i = 0; i < PointСollection.Count - 1; i += frequency)
                {
                    points.Add(new PointLatLng(double.Parse(PointСollection[i + 1], CultureInfo.InvariantCulture),
                                               double.Parse(PointСollection[i], CultureInfo.InvariantCulture)));
                }

                if (PointСollection.Count > 8)
                {
                    MapView.Markers.Clear();
                    GMapPolygon mapPolygon = new GMapPolygon(points);
                    MapView.RegenerateShape(mapPolygon);
                    (mapPolygon.Shape as System.Windows.Shapes.Path).Stroke = Brushes.DarkBlue;
                    (mapPolygon.Shape as System.Windows.Shapes.Path).StrokeThickness = 1.5;
                    (mapPolygon.Shape as System.Windows.Shapes.Path).Effect = null;
                    MapView.Markers.Add(mapPolygon);
                }
                MapView.Position = new GMap.NET.PointLatLng(points[0].Lat, points[0].Lng);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
