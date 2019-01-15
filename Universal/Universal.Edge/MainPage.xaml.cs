using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Universal.Edge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void DrawPolygons(List<PolyGons> gon)
        {
            var removePolygons = MyCanvas.Children.OfType<Polygon>().ToList();
            foreach (var polygon in removePolygons)
            {
                MyCanvas.Children.Remove(polygon);
            }

            foreach (var polygon in gon)
            {
                MyCanvas.Children.Add(polygon.GetPolygon());
            }
        }

        private void DrawLines(List<Lins> lines)
        {
            var removeLines = MyCanvas.Children.OfType<Line>().ToList();
            foreach (var line in removeLines)
            {
                MyCanvas.Children.Remove(line);
            }

            if (lines != null)
                foreach (var line in lines)
                {
                    MyCanvas.Children.Add(line.GetLine());
                }
        }

        private void DrawRectangles(List<Rct> rects)
        {
            var removeRectangles = MyCanvas.Children.OfType<Rectangle>().ToList();
            foreach (var rectangle in removeRectangles)
            {
                MyCanvas.Children.Remove(rectangle);
            }

            foreach (var item in rects)
            {
                MyCanvas.Children.Add(item.GetRect());
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                //picker.FileTypeFilter.Add("*");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                var file = await picker.PickSingleFileAsync();
                if (file == null)
                {
                    return;
                }

                var imageSource = new BitmapImage();
                MyImage.Source = imageSource;
                var stream = await file.OpenReadAsync();
                {
                    if (imageSource is BitmapImage bitmapImage)
                    {
                        bitmapImage.SetSource(stream);
                    }
                }

                Redraw_OnClick(sender, e);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private void Redraw_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var gon = Data.Gon();
                var lines = Data.Lines();
                foreach (var linse in lines)
                {
                    linse.Sort();
                }

                DrawPolygons(gon);

                DrawLines(lines);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }


        private async void Adjust_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                List<bool> mapping = new List<bool>()
                {
                    false,
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                };

                var gon = Data.Gon();
                var lines = Data.Lines();

                foreach (var linse in lines)
                {
                    linse.Sort();
                }

                var rects = new List<Rct>();
                for (int i = 0; i < lines.Count - 1; i++)
                {
                    rects.Add(new Rct()
                    {
                        Left = lines[i],
                        Right = lines[i + 1]
                    });
                }

                //foreach (var polyGonse in gon)
                //{
                //    polyGonse.GetLines();
                //}

                //DrawRectangles(rects);
                var pairs = new List<Pair>();
                var availableGonses = gon.ToList();
                PolyGons prevGons = null;
                for (int i = 0; i < mapping.Count; i++)
                {
                    var pair = new Pair()
                    {
                        Map = mapping[i]
                    };
                    pairs.Add(pair);
                    pair.Rct = rects[i];

                    if (pair.Map)
                    {
                        //pair.Linses = new List<Lins>();
                        var pgon = availableGonses.FirstOrDefault();
                        if (pgon != null)
                        {
                            availableGonses.Remove(pgon);

                            var nextPgon = availableGonses.FirstOrDefault();

                            // left movement = -1
                            // right movement = +1

                            // left intersect - 
                            // is in left? -> move to left
                            // is in right? -> move to right - fine tune
                            // is intersecting? -> move to left
                            //var intersectL = pgon
                            //    .GetAllIntersectingLines(pgon.GetLines(pgon.PaddedPoints), pair.Rct.Left)
                            //    .ToList();
                            //pair.LeftIntersect = intersectL;
                            List<Point> pts;
                            pts = pgon.PaddedPoints;
                            bool forceBreak = false;
                            int failedDetection = 0;
                            while (pgon.get_polygon_direction(pts, pair.Rct.Left) is Direction directionOfPolygonFromLine &&
                                   directionOfPolygonFromLine != Direction.Right && forceBreak == false)
                            {
                                switch (directionOfPolygonFromLine)
                                {
                                    case Direction.Intersect:
                                    case Direction.Left:
                                        if (directionOfPolygonFromLine == Direction.Intersect &&
                                            pgon.GetAllFineIntersectingLines(pgon.GetLines(pts), 
                                                pair.Rct.Left).Any() == false)
                                        {
                                            forceBreak = true;
                                            break;
                                        }

                                        //move to left
                                        var nextFit = await
                                            MoveToDeltaAndExit(
                                                pair.Rct.Left,
                                                pgon,
                                                pts,
                                                prevGons,
                                                prevGons?.PaddedPoints ?? new List<Point>(),
                                                pair.Rct.Mid,
                                                -1
                                            );

                                        if (pair.Rct.Left.Up == nextFit.Up &&
                                            pair.Rct.Left.Down == nextFit.Down)
                                        {
                                            if (pgon.get_polygon_direction(pts, pair.Rct.Left) != Direction.Right)
                                            {
                                                failedDetection++;
                                                if (failedDetection > 10)
                                                {
                                                    pts = pgon.Points;
                                                }
                                                //Force Move
                                                var newX = pts.Min(x => x.X) - 1;
                                                pair.Rct.Left.Up = new Point(newX, nextFit.Up.Y);
                                                pair.Rct.Left.Down = new Point(pair.Rct.Left.Down.X - 1,
                                                    pair.Rct.Left.Down.Y);
                                            }
                                        }
                                        else
                                        {
                                            pair.Rct.Left.Up = nextFit.Up;
                                            pair.Rct.Left.Down = nextFit.Down;
                                        }
                                        break;
                                    case Direction.Right:
                                        // it's fine
                                        break;
                                }
                            }

                            failedDetection = 0;
                            while (pgon.get_polygon_direction(pts, pair.Rct.Right) is Direction directionOfPolygonFromLine &&
                                   directionOfPolygonFromLine != Direction.Left && forceBreak == false)
                            {
                                switch (directionOfPolygonFromLine)
                                {
                                    case Direction.Intersect:
                                    case Direction.Right:

                                        if (directionOfPolygonFromLine == Direction.Intersect &&
                                            pgon.GetAllFineIntersectingLines(pgon.GetLines(pts),
                                                pair.Rct.Right).Any() == false)
                                        {
                                            forceBreak = true;
                                            break;
                                        }

                                        //move to right
                                        var nextFit = await
                                            MoveToDeltaAndExit(
                                                pair.Rct.Right,
                                                pgon,
                                                pts,
                                                nextPgon,
                                                nextPgon?.PaddedPoints ?? new List<Point>(),
                                                pair.Rct.Mid,
                                                1
                                            );

                                        if (pair.Rct.Right.Up == nextFit.Up &&
                                            pair.Rct.Right.Down == nextFit.Down )
                                        {
                                            if (pgon.get_polygon_direction(pts, pair.Rct.Right) != Direction.Left)
                                            {
                                                failedDetection++;
                                                if (failedDetection > 10)
                                                {
                                                    pts = pgon.Points;
                                                }
                                                //Force Move
                                                var newX = pts.Max(x => x.X) + 1;
                                                pair.Rct.Right.Up = new Point(newX, nextFit.Up.Y);
                                                pair.Rct.Right.Down = new Point(pair.Rct.Right.Down.X - 1,
                                                    pair.Rct.Right.Down.Y);
                                            }
                                        }
                                        else
                                        {
                                            pair.Rct.Right.Up = nextFit.Up;
                                            pair.Rct.Right.Down = nextFit.Down;
                                        }

                                        break;
                                    case Direction.Left:
                                        // it's fine
                                        break;
                                }
                            }

                            // right intersect -
                            // is in left? -> move to right
                            // is in right? -> move to left - fine tune
                            // is intersecting? -> move to right
                            //var intersectR = pgon
                            //    .GetAllIntersectingLines(pgon.GetLines(pgon.PaddedPoints), pair.Rct.Right)
                            //    .ToList();
                            //pair.RightIntersect = intersectR;

                            ////move to right
                            //var nextRight = await
                            //    MoveToDeltaAndExit(
                            //        pair.Rct.Right,
                            //        pgon,
                            //        pgon.PaddedPoints,
                            //        nextPgon,
                            //        nextPgon?.PaddedPoints ?? new List<Point>(),
                            //        pair.Rct.Mid,
                            //        +1
                            //    );
                            //pair.Rct.Right.Up = nextRight.Up;
                            //pair.Rct.Right.Down = nextRight.Down;
                            prevGons = pgon;
                        }
                    }

                }

                //var final = rects.Select(x => new[] {x.Left, x.Right}).SelectMany(x => x).Distinct().ToList();
                DrawLines(lines);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private async void Adjust2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                var gon = Data.Gon();
                var lines = Data.Lines();
                var mapping = Data.MappingsList();

                foreach (var linse in lines)
                {
                    linse.Sort();
                }
/*

                var rects = new List<Rct>();
                for (int i = 0; i < lines.Count - 1; i++)
                {
                    rects.Add(new Rct()
                    {
                        Left = lines[i],
                        Right = lines[i + 1]
                    });
                }
*/

                for (int i = 0; i < mapping.Count; i++)
                {
                    var mping = mapping[i];
                    mping.PolygonIndex--;
                    var left = lines[mping.Line1Index];
                    var right = lines[mping.Line2Index];
                    var delta = Math.Abs(left.Down.X - right.Down.X) /*/ 2*/;
                    delta = 100;
                    if (mping.IsTubepresent)
                    {
                        PolyGons prevGons = null;
                        PolyGons nextPgon = null;

                        if ((mping.PolygonIndex - 1) > 0)
                        {
                            prevGons = gon[mping.PolygonIndex - 1];
                        }

                        if ((mping.PolygonIndex + 1) < gon.Count)
                        {
                            nextPgon = gon[mping.PolygonIndex + 1];
                        }

                        PolyGons pgon = null;
                        if (mping.PolygonIndex >= 0 && mping.PolygonIndex < gon.Count)
                        {
                            pgon = gon[mping.PolygonIndex];
                        }

                        if (pgon != null)
                        {
                            // left movement = -1
                            // right movement = +1
                            List<Point> pts;
                            pts = pgon.PaddedPoints;
                            bool forceBreak = false;
                            int failedDetection = 0;
                            while (mping.Line1MovementDirectionMovement != Movement.NoMovement &&
                                   /*pgon.get_polygon_direction(pts, left) is Direction
                                       directionOfPolygonFromLine &&
                                   directionOfPolygonFromLine != Direction.Right && */forceBreak == false)
                            {
                                //switch (directionOfPolygonFromLine)
                                //{
                                //    case Direction.Intersect:
                                //    case Direction.Left:
                                if (pgon.get_polygon_direction(pts, left) is Direction
                                        directionOfPolygonFromLine &&
                                    directionOfPolygonFromLine == Direction.Intersect &&
                                    pgon.GetAllFineIntersectingLines(pgon.GetLines(pts),
                                        left).Any() == false)
                                {
                                    forceBreak = true;
                                    break;
                                }

                                //move to left
                                var nextFit = await
                                    MoveToDeltaAndExit(
                                        left,
                                        pgon,
                                        pts,
                                        prevGons,
                                        prevGons?.PaddedPoints ?? new List<Point>(),
                                        delta,
                                        (mping.Line1MovementDirectionMovement==Movement.Left?-1:1)* 2
                                    );

                                if (left.Up == nextFit.Up &&
                                    left.Down == nextFit.Down)
                                {
                                    if (pgon.get_polygon_direction(pts, left) != Direction.Right)
                                    {
                                        failedDetection++;
                                        if (failedDetection > 10)
                                        {
                                            pts = pgon.Points;
                                        }

                                        //Force Move
                                        var newX = pts.Min(x => x.X) - 1;
                                        left.Up = new Point(newX, nextFit.Up.Y);
                                        left.Down = new Point(left.Down.X - 1,
                                            left.Down.Y);
                                    }
                                }
                                else
                                {
                                    left.Up = nextFit.Up;
                                    left.Down = nextFit.Down;
                                }

                                break;
                                //    case Direction.Right:
                                //        // it's fine
                                //        break;
                                //}
                            }

                            failedDetection = 0;
                            forceBreak = false;
                            while (mping.Line2MovementDirectionMovement != Movement.NoMovement &&
                                   /*pgon.get_polygon_direction(pts, right) is Direction
                                       directionOfPolygonFromLine &&
                                   directionOfPolygonFromLine != Direction.Left && */forceBreak == false)
                            {
                                //switch (directionOfPolygonFromLine)
                                //{
                                //    case Direction.Intersect:
                                //    case Direction.Right:

                                if (pgon.get_polygon_direction(pts, right) is Direction directionOfPolygonFromLine &&
                                    directionOfPolygonFromLine == Direction.Intersect &&
                                    pgon.GetAllFineIntersectingLines(pgon.GetLines(pts),
                                        right).Any() == false)
                                {
                                    forceBreak = true;
                                    break;
                                }

                                //move to right
                                var nextFit = await
                                    MoveToDeltaAndExit(
                                        right,
                                        pgon,
                                        pts,
                                        nextPgon,
                                        nextPgon?.PaddedPoints ?? new List<Point>(),
                                        delta,

                                        (mping.Line2MovementDirectionMovement == Movement.Left ? -1 : 1) * 2
                                    );

                                if (right.Up == nextFit.Up &&
                                    right.Down == nextFit.Down)
                                {
                                    if (pgon.get_polygon_direction(pts, right) != Direction.Left)
                                    {
                                        failedDetection++;
                                        if (failedDetection > 10)
                                        {
                                            pts = pgon.Points;
                                        }

                                        //Force Move
                                        var newX = pts.Max(x => x.X) + 1;
                                        right.Up = new Point(newX, nextFit.Up.Y);
                                        right.Down = new Point(right.Down.X - 1,
                                            right.Down.Y);
                                    }
                                }
                                else
                                {
                                    right.Up = nextFit.Up;
                                    right.Down = nextFit.Down;
                                }

                                break;
                                //    case Direction.Left:
                                //        // it's fine
                                //        break;
                                //}
                            }
                        }
                    }

                }

                DrawLines(lines);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private async Task<Lins> MoveToDeltaAndExit(Lins line, PolyGons pgon, List<Point> pgonLinses, PolyGons nextPgon,
            List<Point> nextPgonLinses, double rctMid, int move)
        {
            // -x movement
            Lins lins = new Lins()
            {
                Up = new Point(line.Up.X, line.Up.Y),
                Down = new Point(line.Down.X, line.Down.Y),
            };

            try
            {

                IEnumerable<Lins> intersectingLinesStart;
                IEnumerable<Lins> intersectingLinesEnd;
                bool thisHit;
                bool nextHit;

                void Refresh()
                {
                    //intersectingLinesStart = pgon.GetAllIntersectingLines(pgon.GetLines(pgonLinses), lins);
                    //intersectingLinesEnd = nextPgon?.GetAllIntersectingLines(nextPgon.GetLines(nextPgonLinses), lins) ??
                    //                       new List<Lins>();
                    intersectingLinesStart = pgon.GetAllFineIntersectingLines(pgon.GetLines(pgonLinses), lins);
                    intersectingLinesEnd =
                        nextPgon?.GetAllFineIntersectingLines(nextPgon.GetLines(nextPgonLinses), lins) ??
                        new List<Lins>();
                    thisHit = intersectingLinesStart.Any();
                    nextHit = intersectingLinesEnd.Any();
                }

                Refresh();
#if DEBUG
                MyCanvas.Children.Add(lins.GetTempLine());
#endif

                bool ResetLine()
                {
                    var bottomX = lins.Down.X + move;
                    var limit = (line.Down.X + (move * rctMid));
                    if (Math.Abs(limit - bottomX) < 1)
                    {
                        //fail this operation
                        lins = line;
                        return false;
                    }
                    else
                    {
                        //reset
                        lins.Up = line.Up;
                        lins.Down = new Point(bottomX, lins.Down.Y);
                        return true;
                    }
                }

                int count = 0;
                while (thisHit || nextHit)
                {
                    count++;
                    lins.GetTempLine(); // to reflect changes on UI
                    await Task.Delay((int) SpeedSlider.Value);

                    if (thisHit && nextHit)
                    {
                        //if (ResetLine() == false)
                        //{
                        //    return lins;
                        //}

                        lins.Up = new Point(lins.Up.X + move, lins.Up.Y);
                        if (count > rctMid * 2 || count > 100)
                        {
                            count = 0;
                            var bottomX = lins.Down.X + move;
                            var limit = (line.Down.X + (move * rctMid));
                            if (Math.Abs(limit - bottomX) < 2)
                            {
                                //fail this operation
                                //lins = line;
                                return lins;
                            }

                            lins.Down = new Point(bottomX, lins.Down.Y);
                        }
                    }
                    else if (thisHit)
                    {
                        lins.Up = new Point(lins.Up.X + move, lins.Up.Y);
                    }
                    else if (nextHit)
                    {
                        //Reset
                        if (ResetLine() == false)
                        {
                            //return lins;
                        }
                    }

                    Refresh();
                }

                return lins;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                MyCanvas.Children.Remove(lins.TempLine);
            }

            return line;
        }

/*

        private async Task<Lins> MoveToLeftAndExit(Lins line, PolyGons pgon, PolyGons nextPgon, double rctMid)
        {
            IEnumerable<Lins> Move(Lins lins1)
            {
                IEnumerable<Lins> intersectingLinesStart;
                IEnumerable<Lins> intersectingLinesEnd;
                bool hitNext;
                double lastNonHitNextX;
                lins1.Up = new Point(lins1.Up.X - 1, lins1.Up.Y);
                intersectingLinesStart = pgon.GetAllIntersectingLines(lins1);
                intersectingLinesEnd = nextPgon?.GetAllIntersectingLines(lins1) ?? new List<Lins>();
                hitNext = intersectingLinesEnd.Any();
                if (hitNext == false)
                {
                    lastNonHitNextX = lins1.Up.X;
                }

                lins1.GetTempLine();
                return intersectingLinesStart;
            }

            // -x movement
            Lins lins = new Lins()
            {
                Up = new Point(line.Up.X, line.Up.Y),
                Down = new Point(line.Down.X, line.Down.Y),
            };

            try
            {

                MyCanvas.Children.Add(lins.GetTempLine());
                var intersectingLinesStart = pgon.GetAllIntersectingLines(lins);
                var intersectingLinesEnd = nextPgon?.GetAllIntersectingLines(lins) ?? new List<Lins>();

                var hitNext = intersectingLinesEnd.Any();
                var lastNonHitNextX = lins.Up.X;
                while (intersectingLinesStart.Any())
                {
                    await Task.Delay((int) SpeedSlider.Value);
                    intersectingLinesStart = Move(lins);
                    if (intersectingLinesStart.Any() == false)
                    {

                    }
                }

                return lins;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            MyCanvas.Children.Remove(lins.TempLine);
            return line;
        }
*/
/*

        private async Task<Lins> MoveToRightAndExit(Lins line, PolyGons pgon, PolyGons nextPgon, double rctMid)
        {

            // +x movement
            Lins lins = new Lins()
            {
                Up = new Point(line.Up.X, line.Up.Y),
                Down = new Point(line.Down.X, line.Down.Y),
            };

            try
            {

                MyCanvas.Children.Add(lins.GetTempLine());
                var intersectingLinesStart = pgon.GetAllIntersectingLines(lins);
                var intersectingLinesEnd = nextPgon?.GetAllIntersectingLines(lins) ?? new List<Lins>();

                var hitNext = intersectingLinesEnd.Any();
                var lastNonHitNextX = lins.Up.X;
                while (intersectingLinesStart.Any())
                {
                    await Task.Delay((int) SpeedSlider.Value);
                    lins.Up = new Point(lins.Up.X + 1, lins.Up.Y);
                    intersectingLinesStart = pgon.GetAllIntersectingLines(lins);
                    intersectingLinesEnd = nextPgon?.GetAllIntersectingLines(lins) ?? new List<Lins>();
                    hitNext = intersectingLinesEnd.Any();
                    if (hitNext == false)
                    {
                        lastNonHitNextX = lins.Up.X;
                    }

                    lins.GetTempLine();
                }

                return lins;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            MyCanvas.Children.Remove(lins.TempLine);
            return line;
        }*/
    }

    public class Pair
    {
        public Rct Rct { get; set; }
        public PolyGons Gons { get; set; }
        public bool Map { get; set; }

        //public List<Lins> Linses { get; set; }
        //public List<Lins> LeftIntersect { get; set; }
        //public List<Lins> RightIntersect { get; set; }
    }

    public class Rct
    {
        public Lins Left { get; set; }
        public Lins Right { get; set; }

        public double Mid
        {
            get { return Math.Abs(Left.Down.X - Right.Down.X) / 2; }
        }

        public Rect RectAngle
        {
            get
            {
                Rect rect = new Rect(Left.Up, Right.Down);
                return rect;
            }
        }

        public Rectangle GetRect()
        {
            var rect = RectAngle;
            var rectangle = new Rectangle()
            {
                Stroke = new SolidColorBrush(Colors.Aqua),
                StrokeThickness = 4,
                StrokeDashArray = new DoubleCollection()
                {
                    1
                },
            };
            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;
            return rectangle;
        }
    }

    public class Lins
    {
        public Point Up { get; set; }
        public Point Down { get; set; }

        public void Sort()
        {
            var up = Up;
            var dn = Down;
            if (up.Y > dn.Y)
            {
                Down = up;
                Up = dn;
            }
        }

        public bool Intersect(Lins line)
        {
            return Helpers.doIntersect(Up, Down, line.Up, line.Down);
        }

        public void FineIntersect(Lins line,
            out bool lines_intersect, out bool segments_intersect,
            out Point intersection,
            out Point close_p1, out Point close_p2)
        {
            Helpers.FindIntersection(Up, Down, line.Up, line.Down, out lines_intersect, out segments_intersect,
                out intersection, out close_p1, out close_p2);

        }

        public double Slope
        {
            get
            {
                var m = (Up.Y - Down.Y) / (Up.X - Down.X);
                return m;
            }
        }

        public double AngleRadians
        {
            get { return Math.Atan((Down.Y - Up.Y) / (Down.X - Up.X)); }
        }

        public double AngleDegrees
        {
            get { return Helpers.ConvertRadiansToDegrees(AngleRadians); }
        }

        public double AngleFromTopDegrees
        {
            get { return 90- AngleDegrees; }
        }

        public double Cofficient
        {
            get
            {
                var cofficient = Down.Y - (Slope * Down.X);
                return cofficient;
            }
        }

        public double GetX(double y)
        {
            var x = (y - Cofficient) / Slope;
            return x;
        }

        public double GetY(double x)
        {
            var y = Slope * x + Cofficient;
            return y;
        }

        public Line GetLine()
        {
            if (Block == null)
            {
                Block = new TextBlock();
                var b = new Binding()
                {
                    Path = new PropertyPath(nameof(Data)),
                    Source = this,
                };
                Block.SetBinding(TextBlock.TextProperty, b);
            }
            var line = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeThickness = 1,
            };
            line.X1 = Up.X;
            line.Y1 = Up.Y;
            line.X2 = Down.X;
            line.Y2 = Down.Y;
            line.DataContext = this;
            ToolTipService.SetToolTip(line, Block);
            return line;
        }

        public string Data
        {
            get { return $"Angle {AngleDegrees}\n" +
                         $"Angle T {AngleFromTopDegrees}\n" +
                         $"Down = {Down}\n" +
                         $"Up = {Up}"; }
        }
        public TextBlock Block { get; set; }
        public Line TempLine { get; private set; }

        public Line GetTempLine()
        {
            if (TempLine == null)
            {
                TempLine = new Line()
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(200, 200, 0, 0)),
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection() {1}
                };
            }

            TempLine.X1 = Up.X;
            TempLine.Y1 = Up.Y;
            TempLine.X2 = Down.X;
            TempLine.Y2 = Down.Y;
            return TempLine;
        }

        public class PointsAndLines
        {
            public static bool IsOutside(Point lineP1, Point lineP2, IEnumerable<Point> region)
            {
                if (region == null || !region.Any()) return true;
                var side = GetSide(lineP1, lineP2, region.First());
                return
                    side == 0
                        ? false
                        : region.All(x => GetSide(lineP1, lineP2, x) == side);
            }

            public static int GetSide(Point lineP1, Point lineP2, Point queryP)
            {
                return Math.Sign((lineP2.X - lineP1.X) * (queryP.Y - lineP1.Y) -
                                 (lineP2.Y - lineP1.Y) * (queryP.X - lineP1.X));
            }
        }

        public static Point FindLineIntersection(Point start1, Point end1, Point start2, Point end2)
        {
            var denom = ((end1.X - start1.X) * (end2.Y - start2.Y)) - ((end1.Y - start1.Y) * (end2.X - start2.X));

            //  AB & CD are parallel 
            if (denom == 0)
                return new Point(0, 0);

            var numer = ((start1.Y - start2.Y) * (end2.X - start2.X)) - ((start1.X - start2.X) * (end2.Y - start2.Y));

            var r = numer / denom;

            var numer2 = ((start1.Y - start2.Y) * (end1.X - start1.X)) - ((start1.X - start2.X) * (end1.Y - start1.Y));

            var s = numer2 / denom;

            if ((r < 0 || r > 1) || (s < 0 || s > 1))
                return new Point(0, 0);

            // Find intersection point
            Point result = new Point();
            result.X = start1.X + (r * (end1.X - start1.X));
            result.Y = start1.Y + (r * (end1.Y - start1.Y));

            return result;
        }

    }

    public class PolyGons
    {
        private List<Point> _points;
        private List<Point> _paddedPoints;

        public List<Point> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                GeneratePaddedPoints();
            }
        }

        private void GeneratePaddedPoints(int padding = 4)
        {
            padding = 1;
            var maxX = MaxX;
            var minx = MinX;
            PaddedPoints = new List<Point>();
            foreach (var point in Points)
            {
                if (Math.Abs(point.X - maxX) < 3)
                {
                    PaddedPoints.Add(new Point(point.X + padding, point.Y));
                }
                else if (Math.Abs(point.X - minx) < 3)
                {
                    PaddedPoints.Add(new Point(point.X - padding, point.Y));
                }
                else
                {
                    PaddedPoints.Add(point);
                }
            }
        }

        public List<Point> PaddedPoints
        {
            get { return _paddedPoints; }
            set { _paddedPoints = value; }
        }

        public static SolidColorBrush FillColor = new SolidColorBrush(Color.FromArgb(130, 255, 0, 0));

        public Polygon GetPolygon()
        {
            Polygon polygon = new Polygon()
            {
                Stroke = new SolidColorBrush(Colors.GreenYellow),
                Fill = FillColor,
                FillRule = FillRule.Nonzero,
                StrokeThickness = 1,
            };

            foreach (var point in PaddedPoints)
            {
                polygon.Points?.Add(point);
            }

            return polygon;
        }

        public IEnumerable<Lins> GetLines(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i < points.Count - 1)
                {
                    yield return new Lins()
                    {
                        Up = points[i],
                        Down = points[i + 1],
                    };
                }
                else if (i == points.Count - 1)
                {
                    yield return new Lins()
                    {
                        Up = points[i],
                        Down = points[0],
                    };
                }
            }
        }

        public IEnumerable<Lins> GetAllIntersectingLines(IEnumerable<Lins> linses, Lins lins)
        {
            foreach (var linse in linses)
            {
                if (lins.Intersect(linse))
                {
                    yield return linse;
                }
            }
        }

        public IEnumerable<Lins> GetAllFineIntersectingLines(IEnumerable<Lins> linses, Lins lins)
        {
            bool lines_intersect;
            bool segments_intersect;
            Point intersection;
            Point close_p1;
            Point close_p2;
            foreach (var linse in linses)
            {
                lins.FineIntersect(linse, out lines_intersect, out segments_intersect,
                    out intersection, out close_p1, out close_p2);
                if (segments_intersect)
                {
                    yield return linse;
                }
            }
        }

        public IEnumerable<Lins> GetAllNonIntersectingLines(IEnumerable<Lins> linses, Lins lins)
        {
            foreach (var linse in linses)
            {
                if (lins.Intersect(linse) == false)
                {
                    yield return linse;
                }
            }
        }

        public double MaxX
        {
            get { return Points.Max(x => x.X); }
        }

        public double MaxY
        {
            get { return Points.Max(x => x.Y); }
        }

        public double MinX
        {
            get { return Points.Min(x => x.X); }
        }

        public double MinY
        {
            get { return Points.Min(x => x.Y); }
        }

        public Direction get_polygon_direction(List<Point> points, Lins line)
        {
            var point_on_left = false;
            var point_on_right = false;

            foreach (var point in points)
            {
                if (point.X < line.Down.X || point.X < line.Up.X)
                    point_on_left = true;
                if (point.X > line.Down.X || point.X > line.Up.X)
                    point_on_right = true;
            }

            if (point_on_left && point_on_right)
            {
                return Direction.Intersect;
            }
            else if (point_on_left)
            {
                return Direction.Left;
            }
            else
            {
                return Direction.Right;
            }
        }
    }

    public enum Direction
    {
        Left,
        Right,
        Intersect
    }

    public static class Helpers
    {

        public static double max(double a, double b)
        {
            return a > b ? a : b;
        }

        public static double min(double a, double b)
        {
            return a < b ? a : b;
        }

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        public static bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= max(p.X, r.X) &&
                q.X >= min(p.X, r.X) &&
                q.Y <= max(p.Y, r.Y) &&
                q.Y >= min(p.Y, r.Y)
            )
            {
                return true;
            }

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        public static int orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            var val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public static bool doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        public static double FindDistanceToSegment(this Point pt, Lins line, out Point closest)
        {
            return FindDistanceToSegment(pt, line.Up, line.Down, out closest);
        }

        public static double FindDistanceToSegment(this Point pt, Point p1, Point p2, out Point closest)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            var t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                    (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Return the shortest distance between the two segments
        // p1 --> p2 and p3 --> p4.
        private static double FindDistanceBetweenSegments(this Lins lineA, Lins lineB, out Point close1,
            out Point close2)
        {
            Point p1 = lineA.Up;
            Point p2 = lineA.Down;
            Point p3 = lineB.Up;
            Point p4 = lineB.Down;

            // See if the segments intersect.
            bool lines_intersect, segments_intersect;
            Point intersection;
            FindIntersection(p1, p2, p3, p4,
                out lines_intersect, out segments_intersect,
                out intersection, out close1, out close2);
            if (segments_intersect)
            {
                // They intersect.
                close1 = intersection;
                close2 = intersection;
                return 0;
            }

            // Find the other possible distances.
            Point closest;
            double best_dist = double.MaxValue, test_dist;

            // Try p1.
            test_dist = FindDistanceToSegment(p1, p3, p4, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = p1;
                close2 = closest;
            }

            // Try p2.
            test_dist = FindDistanceToSegment(p2, p3, p4, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = p2;
                close2 = closest;
            }

            // Try p3.
            test_dist = FindDistanceToSegment(p3, p1, p2, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = closest;
                close2 = p3;
            }

            // Try p4.
            test_dist = FindDistanceToSegment(p4, p1, p2, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = closest;
                close2 = p4;
            }

            return best_dist;
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static void FindIntersection(
            Point p1, Point p2, Point p3, Point p4,
            out bool lines_intersect, out bool segments_intersect,
            out Point intersection,
            out Point close_p1, out Point close_p2)
        {
            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                / denominator;
            if (double.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Point(float.NaN, float.NaN);
                close_p1 = new Point(float.NaN, float.NaN);
                close_p2 = new Point(float.NaN, float.NaN);
                return;
            }

            lines_intersect = true;

            double t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                / -denominator;

            // Find the point of intersection.
            intersection = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new Point(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }

        public static double GetSideOfPoint(this Lins line, Point pt)
        {
            var d = GetSideOfPoint(line.Down, line.Up, pt);
            return d;
        }

        public static double GetSideOfPoint(Point pta, Point ptb, Point pt)
        {
            var x2mx1 = ptb.X - pta.X;
            var y2my1 = ptb.Y - pta.Y;
            var xmx1 = pt.X - pta.X;
            var ymy1 = pt.Y - pta.Y;
            var d = (xmx1) * (y2my1) - (ymy1) * (x2mx1);
            return d;
        }

        //class Polygon :

        //        def __init__(self, points):
        //    self.polygon_points = points
        //    self.polygon_area = self.calculate_polygon_area()

        //def calculate_polygon_area(self):
        //    area = 0.0
        //    n = self.polygon_points.__len__()
        //    # Calculate value of shoelace formula
        //    j = n - 1
        //    for i in range(0, n):
        //        area += (self.polygon_points[j].X + self.polygon_points[i].X) \
        //                * \
        //                (self.polygon_points[j].Y - self.polygon_points[i].Y)
        //        j = i  # j is previous vertex to i

        //    return int (abs(area / 2.0))

        //def get_left_most_point(self):
        //    left_most_point = self.polygon_points[0]
        //    for point in self.polygon_points:
        //        if left_most_point.X > point.X:
        //            left_most_point = point
        //    return left_most_point

        //def get_right_most_point(self) :
        //    right_most_point = self.polygon_points[0]
        //    for point in self.polygon_points:
        //        if right_most_point.X<point.X:
        //            right_most_point = point
        //    return right_most_point

        //def is_polygon_between_points_x_axis(self, point1, point2):
        //    for point in  self.polygon_points:
        //        if point1.X<point.X<point2.X:
        //            return True

        //def is_polygon_between_line_x_axis(self, line1, line2):
        //    for point in self.polygon_points:
        //        if line1.away_box_point.X<point.X or line1.near_box_point.X<point.X:
        //            if line2.away_box_point.X> point.X or line2.near_box_point.X> point.X:
        //                return True
        //    return False


        //def polygon_intersect_with_line(self, line):
        //    number_of_polygon_points = self.polygon_points.__len__()
        //    for i in range(0, number_of_polygon_points):
        //        polygon_line = [self.polygon_points[i], self.polygon_points[(i + 1) % number_of_polygon_points]]
        //        result = utilityhelper.do_lines_intersect([polygon_line[0].X, polygon_line[0].Y], [polygon_line[1].X, polygon_line[1].Y], [line.away_box_point.X, line.away_box_point.Y], [line.near_box_point.X, line.near_box_point.Y])
        //        if result:
        //            return True
        //    return False

        //def get_intersection_points_with_line(self, line) :
        //    number_of_polygon_points = self.polygon_points.__len__()
        //    intersection_points =[]
        //    for i in range(0, number_of_polygon_points) :
        //        polygon_line = [[self.polygon_points[i].X,self.polygon_points[i].Y], [self.polygon_points[(i + 1)%number_of_polygon_points].X, self.polygon_points[(i + 1) % number_of_polygon_points].Y]]
        //        print()
        //        result = utilityhelper.do_lines_intersect(polygon_line[0], polygon_line[1], [line.away_box_point.X, line.away_box_point.Y], [line.near_box_point.X, line.near_box_point.Y])
        //        if result:
        //            polygon_vertical_line = VerticalLine.VerticalLine(Point.Point(polygon_line[0][0], polygon_line[0][1]), Point.Point(polygon_line[1][0], polygon_line[1][1]))
        //            intersection_point = polygon_vertical_line.line_intersection(line)
        //            intersection_points.append(intersection_point)

        //    return intersection_points

        //def pad_polygons(self, padding) :
        //    left_most_ponit = self.get_left_most_point()
        //    right_most_point = self.get_right_most_point()
        //    mid_point_x= (abs(left_most_ponit.X - right_most_point.X))/2
        //    for point in self.polygon_points:
        //        if point.X<mid_point_x:
        //            point.X = point.X - padding
        //        else:
        //            point.X = point.X + padding
        //    return self
        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }
    }
}
