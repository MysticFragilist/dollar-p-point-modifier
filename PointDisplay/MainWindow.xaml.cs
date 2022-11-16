using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace PointDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string XMLFileAsText = "";
        public int Zoom = 190;
        public int OffsetX = 190;
        public int OffsetY = 95;

        public MainWindow()
        {
            InitializeComponent();
            ZoomText.Text = Zoom.ToString();
            OffsetXText.Text = OffsetX.ToString();
            OffsetYText.Text = OffsetY.ToString();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".xml"; // Required file extension 
            fileDialog.Filter = "XML PDollar documents (.xml)|*.xml"; // Optional file extensions

            var res = fileDialog.ShowDialog();

            if (res.HasValue && res.Value)
            {
                FldFile.Text = fileDialog.FileName;
                System.IO.StreamReader sr = new System.IO.StreamReader(fileDialog.FileName);
                XMLFileAsText = sr.ReadToEnd();
                textEditor.Text = XMLFileAsText;
                sr.Close();

                Render();
            }
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            XMLFileAsText = textEditor.Text;
        }


        private void Render()
        {
            if (string.IsNullOrEmpty(XMLFileAsText)) return;

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(XMLFileAsText);
            XmlNodeList xmlGestures = xmlDocument.DocumentElement?.SelectNodes("/Gesture")!;
            this.Title = xmlGestures[0]!.Attributes!["Name"]!.Value;

            XmlNodeList xmlPointList = xmlDocument.DocumentElement?.SelectNodes("/Gesture/Stroke/Point")!;


            using (var bmp = new Bitmap((int)myCanvas.ActualWidth, (int)myCanvas.ActualHeight))
            using (var gfx = Graphics.FromImage(bmp))
            using (var penLine = new Pen(Color.Black))
            using (var brushPoint = Brushes.Red)
            {
                // draw one thousand random white lines on a dark blue background
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                gfx.Clear(Color.White);
                System.Drawing.Point? lastPt = null, firstPt = null;

                foreach (XmlNode xmlNode in xmlPointList)
                {
                    var X = (int)Math.Round(float.Parse(xmlNode.Attributes!["X"]!.Value) * Zoom) + OffsetX;
                    var Y = (int)Math.Round(float.Parse(xmlNode.Attributes!["Y"]!.Value) * Zoom) + OffsetY;
                    var pt = new System.Drawing.Point(X, Y);
                    try
                    {
                        gfx.FillEllipse(brushPoint, new Rectangle(X - 1, Y - 1, 2, 2));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Impossible to display point({X}, {Y}): {e.Message}");
                    }

                    if (lastPt.HasValue)
                        gfx.DrawLine(penLine, lastPt.Value, pt);
                    else
                        firstPt = pt;

                    lastPt = pt;
                }

                // Draw last line
                if (lastPt.HasValue && firstPt.HasValue)
                    gfx.DrawLine(penLine, lastPt.Value, firstPt.Value);

                myImage.Source = BmpImageFromBmp(bmp);
            }
        }
        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            myImage.Height = myCanvas.Height;
            myImage.Width = myCanvas.Width;
            Render();
        }


        private void textEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Render();
            }
        }

        private void ZoomText_LostFocus(object sender, RoutedEventArgs e)
        {
            Zoom = int.Parse(ZoomText.Text);
            Render();
        }

        private void OffsetYText_LostFocus(object sender, RoutedEventArgs e)
        {

            OffsetY = int.Parse(OffsetYText.Text);
            Render();
        }

        private void OffsetXText_LostFocus(object sender, RoutedEventArgs e)
        {

            OffsetX = int.Parse(OffsetXText.Text);
            Render();
        }
    }
}
