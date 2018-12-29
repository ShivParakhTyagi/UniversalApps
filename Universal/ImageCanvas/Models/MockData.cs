using System;
using System.Collections.Generic;
using Windows.Foundation;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using Color = Windows.UI.Color;

namespace ImageCanvas.Models
{
    public class MockData
    {
        public static DiagramCoordinatesMatrix GetCoordinatesMatrix(string text)
        {
            try
            {

            string json;
            if (string.IsNullOrEmpty(text))
            {
                json =
                    "{\"BottomLeft\":[49.152,1012.6222222222223],\"BottomRight\":[2025.472,1012.6222222222223],\"LowerTubesCoordinates\":[[69.632,1422.2222222222222],[174.08,1422.2222222222222],[370.688,1422.2222222222222],[370.688,1422.2222222222222],[501.76,1422.2222222222222],[561.152,1422.2222222222222],[671.744,1422.2222222222222],[757.76,1422.2222222222222],[864.256,1422.2222222222222],[954.368,1422.2222222222222],[1052.672,1422.2222222222222],[1275.904,1422.2222222222222],[1275.904,1422.2222222222222],[1452.032,1422.2222222222222],[1452.032,1422.2222222222222],[1583.104,1422.2222222222222],[1642.496,1422.2222222222222],[1746.944,1422.2222222222222],[1839.104,1422.2222222222222],[1986.56,1422.2222222222222],[2035.712,1422.2222222222222]],\"TopLeft\":[49.152,523.3777777777779],\"TopRight\":[2025.472,523.3777777777779],\"UpperTubesCoordinates\":[[69.632,113.77777777777779],[167.936,113.77777777777779],[299.008,113.77777777777779],[432.128,113.77777777777779],[520.192,113.77777777777779],[604.16,113.77777777777779],[714.7520000000001,113.77777777777779],[757.76,113.77777777777779],[856.064,113.77777777777779],[978.9440000000001,113.77777777777779],[1107.968,113.77777777777779],[1150.976,113.77777777777779],[1249.28,113.77777777777779],[1386.496,113.77777777777779],[1445.888,113.77777777777779],[1589.248,113.77777777777779],[1658.88,113.77777777777779],[1796.096,113.77777777777779],[1851.392,113.77777777777779],[1988.608,113.77777777777779],[2035.712,113.77777777777779]]}\n";
            }
            else
            {
                json = text;
            }

            var diagramCoordinates = JsonConvert.DeserializeObject<DiagramCoordinates>(json);
            return diagramCoordinates.GetDiagramCoordinatesMatrix();
            }
            catch (Exception e)
            {
            }

            return null;
        }
    }

    public class DiagramCoordinates
    {
        [JsonProperty("BottomLeft")]
        public IList<float> BottomLeft { get; set; }

        [JsonProperty("BottomRight")]
        public IList<float> BottomRight { get; set; }

        [JsonProperty("LowerTubesCoordinates")]
        public IList<IList<float>> LowerTubesCoordinates { get; set; }

        [JsonProperty("TopLeft")]
        public IList<float> TopLeft { get; set; }

        [JsonProperty("TopRight")]
        public IList<float> TopRight { get; set; }

        [JsonProperty("UpperTubesCoordinates")]
        public IList<IList<float>> UpperTubesCoordinates { get; set; }

        public DiagramCoordinatesMatrix GetDiagramCoordinatesMatrix()
        {
            DiagramCoordinatesMatrix matrix = new DiagramCoordinatesMatrix()
            {
                BottomLeft = new MarkerPoint(BottomLeft[0], BottomLeft[1]),
                BottomRight = new MarkerPoint(BottomRight[0], BottomRight[1]),
                TopLeft = new MarkerPoint(TopLeft[0], TopLeft[1]),
                TopRight = new MarkerPoint(TopRight[0], TopRight[1]),
                LowerTubesCoordinates =
                    new List<MarkerPoint>(LowerTubesCoordinates.Select(x => new MarkerPoint(x[0], x[1]))),
                UpperTubesCoordinates =
                    new List<MarkerPoint>(UpperTubesCoordinates.Select(x => new MarkerPoint(x[0], x[1]))),
            };
            return matrix;
        }
    }

    public class DiagramCoordinatesMatrix
    {
        public MarkerPoint BottomLeft { get; set; }

        public MarkerPoint BottomRight { get; set; }

        public MarkerPoint TopLeft { get; set; }

        public MarkerPoint TopRight { get; set; }

        public List<MarkerPoint> LowerTubesCoordinates { get; set; }


        public List<MarkerPoint> UpperTubesCoordinates { get; set; }

        public void Scale(double scaleX, double scaleY)
        {
            BottomLeft = new MarkerPoint(BottomLeft.X * scaleX, BottomLeft.Y * scaleY);
            BottomRight = new MarkerPoint(BottomRight.X * scaleX, BottomRight.Y * scaleY);
            TopLeft = new MarkerPoint(TopLeft.X * scaleX, TopLeft.Y * scaleY);
            TopRight = new MarkerPoint(TopRight.X * scaleX, TopRight.Y * scaleY);
            LowerTubesCoordinates = LowerTubesCoordinates
                .Select(point => new MarkerPoint(point.X * scaleX, point.Y * scaleY)).ToList();
            UpperTubesCoordinates = UpperTubesCoordinates
                .Select(point => new MarkerPoint(point.X * scaleX, point.Y * scaleY)).ToList();
        }
    }
    public class MarkerPoint
    {
        public virtual double X { get; set; }
        public virtual double Y { get; set; }

        public Point Point
        {
            get { return new Point((int) X, (int) Y); }
        }

        private MarkerPoint()
        {

        }

        public MarkerPoint(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        public MarkerPoint(MarkerPoint point) : this(point.X, point.Y)
        {
        }

        public MarkerPoint CreateWithTranslation(double deltaX, double deltaY)
        {
            return new MarkerPoint(X + deltaX, Y + deltaY);
        }

        /*

        public static bool operator ==(MarkerPoint point1, MarkerPoint point2)
        {
            if (point1 == null && point2 == null)
            {
                return true;
            }
            if (point1 == null || point2 == null)
            {
                return false;
            }

            return point1.X == point2.X && point1.Y == point2.Y;
        }

        public static bool operator !=(MarkerPoint point1, MarkerPoint point2)
        {
            if (point1 == null && point2 == null)
            {
                return true;
            }
            if (point1 == null || point2 == null)
            {
                return false;
            }

            return point1.X != point2.X && point1.Y != point2.Y;
        }
        */

        public double DeltaX(MarkerPoint point)
        {
            return X - point.X;
        }
        public double DeltaY(MarkerPoint point)
        {
            return Y - point.Y;
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
/*


        /// <summary>
        /// Rotates 'this' about 'origin' by 'angle' degrees clockwise.
        /// </summary>
        /// <param name="origin">Point to rotate around</param>
        /// <param name="angle">Angle in degrees to rotate clockwise</param>
        /// <returns>The rotated point</returns>
        public virtual MarkerPoint RotateAround(MarkerPoint origin, double angle)
        {
            var newPoint = RotatePoint(this, origin, angle);
            return newPoint;
        }

        /// <summary>
        /// Rotates 'p1' about 'p2' by 'angle' degrees clockwise.
        /// </summary>
        /// <param name="p1">Point to be rotated</param>
        /// <param name="p2">Point to rotate around</param>
        /// <param name="angle">Angle in degrees to rotate clockwise</param>
        /// <returns>The rotated point</returns>
        public static MarkerPoint RotatePoint(MarkerPoint p1, MarkerPoint p2, double angle)
        {

            double radians = ConvertToRadians(angle);
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);

            // Translate point back to origin
            var p1X = p1.X - p2.X;
            var p1Y = p1.Y - p2.Y;

            // Rotate point
            double xnew = p1X * cos - p1Y * sin;
            double ynew = p1X * sin + p1Y * cos;

            // Translate point back
            MarkerPoint newPoint = new MarkerPoint((double)(xnew + p2.X), (double)(ynew + p2.Y));
            return newPoint;
        }

        /// <summary>
        /// Rotates 'this' about 'origin' by 'angle' degrees clockwise.
        /// </summary>
        /// <param name="origin">Point to rotate around</param>
        /// <param name="radians">Angle in degrees to rotate clockwise</param>
        /// <returns>The rotated point</returns>
        public virtual MarkerPoint RotateAroundInRadians(MarkerPoint origin, double radians)
        {
            var newPoint = RotatePointInRadians(this, origin, radians);
            return newPoint;
        }

        /// <summary>
        /// Rotates 'p1' about 'p2' by 'angle' degrees clockwise.
        /// </summary>
        /// <param name="p1">Point to be rotated</param>
        /// <param name="p2">Point to rotate around</param>
        /// <param name="radians"> in degrees to rotate clockwise</param>
        /// <returns>The rotated point</returns>
        public static MarkerPoint RotatePointInRadians(MarkerPoint p1, MarkerPoint p2, double radians)
        {

            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);

            // Translate point back to origin
            var p1X = p1.X - p2.X;
            var p1Y = p1.Y - p2.Y;

            // Rotate point
            double xnew = p1X * cos - p1Y * sin;
            double ynew = p1X * sin + p1Y * cos;

            // Translate point back
            MarkerPoint newPoint = new MarkerPoint((double)(xnew + p2.X), (double)(ynew + p2.Y));
            return newPoint;
        }

        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static double ConvertToDegrees(double radians)
        {
            return radians / (Math.PI / 180);
        }

        /// <summary>
        /// Scales 'this' by 'scale' around 'origin'
        /// </summary>
        /// <param name="origin">Point to scale around</param>
        /// <param name="scale">Scale value</param>
        /// <returns></returns>
        public MarkerPoint ScaleAround(MarkerPoint origin, double scale)
        {
            return ScalePoint(this, origin, scale);
        }


        /// <summary>
        /// Scales 'destination' by 'scale' around 'origin'
        /// </summary>
        /// <param name="destination">Point to be scaled</param>
        /// <param name="origin">Point to scale around</param>
        /// <param name="scale">Scale value</param>
        /// <returns></returns>
        public static MarkerPoint ScalePoint(MarkerPoint destination, MarkerPoint origin, double scale)
        {
            return new MarkerPoint((origin.X + destination.X) * scale, (origin.Y + destination.Y) * scale);
        }

        public static double GetDistance(MarkerPoint point1, MarkerPoint point2)
        {
            var distance = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
            return distance;
        }

        public double GetDistance(MarkerPoint point2)
        {
            var distance = GetDistance(this, point2);
            return distance;
        }*/
        public UIElement CreatePoint(Color color = default(Color))
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = ellipse.Height = 5;
            if (color == default(Color))
            {
                color = Colors.Red;
            }

            ellipse.Fill = new SolidColorBrush(color);
            ellipse.Stroke = new SolidColorBrush(color);
            Canvas.SetLeft(ellipse, X);
            Canvas.SetTop(ellipse, Y);
            return ellipse;
        }
    }
}