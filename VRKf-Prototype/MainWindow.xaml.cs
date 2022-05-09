using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
using VRKf_WMS_Prototype.Models;

namespace VRKf_Prototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LocalizationDataProcessing LDP;

        public MainWindow()
        {
            LDP = new LocalizationDataProcessing();
            InitializeComponent();
            Version2();
        }


        public async void Version2()
        {
            Longtitude.Text = LDP.Longtitude.ToString();
            Latitude.Text = LDP.Latitude.ToString();
            MapHeight.Text = LDP.MapHeight.ToString();
            MapWidth.Text = LDP.MapWidth.ToString();
            Response.Content = "Loading maps";
            RawMap.Source = ConvertToImage(BuildingRecognition.GetBitmap(await LDP.GetOrtoMap()));
            PeriMap.Source = ConvertToImage(BuildingRecognition.GetBitmap(await LDP.GetPremiterMap()));

            List<Blob> buildings = LDP.GetBuildings();
            List<float> buildingsSizes = new List<float>();
            string buildingsSizesInString = "Building Sizes: ";
            float sum = 0;
            foreach (Blob item in buildings)
            {
                buildingsSizes.Add(item.Area);
                // Obliczenie wielkości budynków
                float a = (float)LDP.GetBuildingSize(item);
                buildingsSizesInString += a + " m2, ";
                sum += item.Area;
            }

            //ViewData["BuildingsSizes"] = buildingsSizesInString;
            //ViewData["BuildingsSizesSum"] = "Building sizes sum: " + sum;
            Response.Content = "Done!";
        }


        public BitmapImage ConvertToImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(Longtitude.Text, out float lo) && float.TryParse(Latitude.Text, out float la))
                LDP.SetCoords(lo, la);
            if (int.TryParse(MapHeight.Text, out int height) && int.TryParse(MapWidth.Text, out int width))
                LDP.SetMapSize(width, height);
            Version2();
        }

        private void SetAddress_Click(object sender, RoutedEventArgs e)
        {
            Response.Content = "Loading address";
            LDP.SetAddress(Address.Text);
            Response.Content = "Done!";
            Longtitude.Text = LDP.Longtitude.ToString();
            Latitude.Text = LDP.Latitude.ToString();
        }
    }
}
