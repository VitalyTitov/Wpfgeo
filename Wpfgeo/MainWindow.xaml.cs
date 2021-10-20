using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
//using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            // choose your provider here
            // mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;

            MapView.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //MapView.MapProvider = GMapProviders.YandexMap;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            MapView.ShowCenter = false;
            MapView.MinZoom = 2;
            MapView.MaxZoom = 17;
            // whole world zoom
            MapView.Zoom = 10;
            // lets the map use the mousewheel to zoom
            MapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            MapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            MapView.DragButton = MouseButton.Left;
            //
            //MapView.Position = new GMap.NET.PointLatLng(55.75393, 37.620795);
            MapView.Position = new GMap.NET.PointLatLng(48.866383, 2.323575);
            //List<PointLatLng> points = new List<PointLatLng>();
            //points.Add(new PointLatLng(48.866383, 2.323575));
            //points.Add(new PointLatLng(48.863868, 2.321554));
            //points.Add(new PointLatLng(48.861017, 2.330030));
            //points.Add(new PointLatLng(48.863727, 2.331918));
            //GMapPolygon mapPolygon = new GMapPolygon(points);
            //MapView.RegenerateShape(mapPolygon);
            ////setting line style
            //(mapPolygon.Shape as System.Windows.Shapes.Path).Stroke = Brushes.DarkBlue;
            //(mapPolygon.Shape as System.Windows.Shapes.Path).StrokeThickness = 1.5;
            //(mapPolygon.Shape as System.Windows.Shapes.Path).Effect = null;
            //MapView.Markers.Add(mapPolygon);
            
        }
        private void MapSearch(object sender, RoutedEventArgs e)
        {
            //Запрос к ОСМ 
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
  
            File.WriteAllText(@"D:\user.json", jsonString);
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
            for (int i = 0; i < PointСollection.Count-1; i+=frequency)
            {               
                points.Add(new PointLatLng(double.Parse(PointСollection[i+1],CultureInfo.InvariantCulture), 
                                           double.Parse(PointСollection[i],CultureInfo.InvariantCulture)));                            
            }
            
            if (PointСollection.Count>8)
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

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
