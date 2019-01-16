using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Universal.Edge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
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

                //Redraw_OnClick(sender, e);
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
                var gon = MockData.UpperPolyGon();
                var lines = MockData.GetUpperLines();
                DrawPolygons(gon);
                DrawLines(lines);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private void RedrawLower_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var gon = MockData.LowerPolyGon();
                var lines = MockData.GetLowerLines();
                DrawPolygons(gon);
                DrawLines(lines);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private void Adjust_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Adjust2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                int speed = 1;
                var gon = MockData.UpperPolyGon();
                var lines = MockData.GetUpperLines();
                var mapping = MockData.UpperMappingsList();

                for (int i = 0; i < mapping.Count; i++)
                {
                    var mping = mapping[i];
                    var left = lines[mping.Line1Index];
                    var right = lines[mping.Line2Index];
                    var delta = Math.Abs(left.Down.X - right.Down.X) /*/ 2*/;
                    //delta = 100;
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
                                   forceBreak == false)
                            {
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
                                        (mping.Line1MovementDirectionMovement == Movement.Left ? -1 : 1) * speed
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
                                        var newX = pts.Min(x => x.X);
                                        var newPts = pts.Where(x => x.X == newX).ToArray();
                                        var maxY = newPts.Max(y => y.Y);
                                        var newPoint = newPts.FirstOrDefault(y => y.Y == maxY);

                                        var intersectingPoint = Lins.GetPointPassingFromLineABIntersectsAtY(left.Down,
                                            newPoint, left.Up.Y);

                                        left.Up = intersectingPoint;
                                        left.Down = new Point(left.Down.X - 1,
                                            left.Down.Y);
                                    }
                                }
                                else
                                {
                                    left.Up = nextFit.Up;
                                    left.Down = nextFit.Down;
                                }
                            }

                            failedDetection = 0;
                            forceBreak = false;
                            while (mping.Line2MovementDirectionMovement != Movement.NoMovement &&
                                   forceBreak == false)
                            {
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

                                        (mping.Line2MovementDirectionMovement == Movement.Left ? -1 : 1) * speed
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
                                        var newX = pts.Max(x => x.X);
                                        var newPts = pts.Where(x => x.X == newX).ToArray();
                                        var maxY = newPts.Max(y => y.Y);
                                        var newPoint = newPts.FirstOrDefault(y => y.Y == maxY);

                                        var intersectingPoint = Lins.GetPointPassingFromLineABIntersectsAtY(right.Down,
                                            newPoint, right.Up.Y);

                                        right.Up = intersectingPoint;
                                    }
                                }
                                else
                                {
                                    right.Up = nextFit.Up;
                                    right.Down = nextFit.Down;
                                }

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
        private void AdjustLower_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private async void AdjustLower2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int speed = 3;
                var gon = MockData.LowerPolyGon();
                var lines = MockData.GetLowerLines();
                var mapping = MockData.LowerMappingsList();

                for (int i = 0; i < mapping.Count; i++)
                {
                    var mping = mapping[i];
                    var left = lines[mping.Line1Index];
                    var right = lines[mping.Line2Index];
                    var delta = Math.Abs(left.Down.X - right.Down.X) /*/ 2*/;
                    //delta = 100;
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
                                   forceBreak == false)
                            {
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
                                        (mping.Line1MovementDirectionMovement == Movement.Left ? -1 : 1) * speed
                                    );

                                if (left.Up == nextFit.Up &&
                                    left.Down == nextFit.Down)
                                {
                                    if (pgon.get_polygon_direction(pts, left) != Direction.Right)
                                    {
                                        failedDetection++;
                                        if (failedDetection > 3)
                                        {
                                            break;
                                            pts = pgon.Points;
                                        }

                                        //Force Move
                                        var newX = pts.Min(x => x.X);
                                        var newPts = pts.Where(x => x.X == newX).ToArray();
                                        //var maxY = newPts.Min(y => y.Y);
                                        var maxY = newPts.Max(y => y.Y);
                                        var newPoint = newPts.FirstOrDefault(y => y.Y == maxY);

                                        var intersectingPoint = Lins.GetPointPassingFromLineABIntersectsAtY(left.Down,
                                            newPoint, left.Up.Y);

                                        left.Up = intersectingPoint;
                                        left.Down = new Point(left.Down.X - 1,
                                            left.Down.Y);
                                    }
                                }
                                else
                                {
                                    left.Up = nextFit.Up;
                                    left.Down = nextFit.Down;
                                }
                            }

                            failedDetection = 0;
                            forceBreak = false;
                            while (mping.Line2MovementDirectionMovement != Movement.NoMovement &&
                                   forceBreak == false)
                            {
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

                                        (mping.Line2MovementDirectionMovement == Movement.Left ? -1 : 1) * speed
                                    );

                                if (right.Up == nextFit.Up &&
                                    right.Down == nextFit.Down)
                                {
                                    if (pgon.get_polygon_direction(pts, right) != Direction.Left)
                                    {
                                        failedDetection++;
                                        if (failedDetection > 3)
                                        {
                                            break;
                                            pts = pgon.Points;
                                        }

                                        //Force Move
                                        var newX = pts.Max(x => x.X);
                                        var newPts = pts.Where(x => x.X == newX).ToArray();
                                        //var maxY = newPts.Min(y => y.Y);
                                        var maxY = newPts.Max(y => y.Y);
                                        var newPoint = newPts.FirstOrDefault(y => y.Y == maxY);

                                        var intersectingPoint = Lins.GetPointPassingFromLineABIntersectsAtY(right.Down,
                                            newPoint, right.Up.Y);

                                        right.Up = intersectingPoint;
                                    }
                                }
                                else
                                {
                                    right.Up = nextFit.Up;
                                    right.Down = nextFit.Down;
                                }
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
                int count = 0;
                while (thisHit || nextHit)
                {
                    count++;
                    lins.GetTempLine(); // to reflect changes on UI
                    await Task.Delay((int)SpeedSlider.Value);

                    if (thisHit && nextHit)
                    {
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
                        var bottomX = lins.Down.X + move;
                        var limit = (line.Down.X + (move * rctMid));
                        if (Math.Abs(limit - bottomX) < 1)
                        {
                            //fail this operation
                            lins = new Lins()
                            {
                                Up = new Point(line.Up.X, line.Up.Y),
                                Down = new Point(line.Down.X, line.Down.Y),
                            };
                        }
                        else
                        {
                            //reset
                            lins.Up = line.Up;
                            lins.Down = new Point(bottomX, lins.Down.Y);
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

        private async void Adjust3_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int move = 4;
                var gon = NewMockData.UpperPolyGon();
                var lines = NewMockData.GetUpperLines();
                var mapping = NewMockData.UpperMappingsList();

                //foreach (var linse in lines)
                //{
                //    linse.Sort();
                //}

                DrawLines(lines);
                DrawPolygons(gon);
                for (int i = 0; i < mapping.Count; i++)
                {
                    var mping = mapping[i];
                    //mping.PolygonIndex--;//IGNORE THIS for this test data only
                    var left = lines[mping.Line1Index];
                    var right = lines[mping.Line2Index];
                    var delta = Math.Abs(left.Down.X - right.Down.X);
                    //delta = 100;
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
                            List<Point> pts;
                            pts = pgon.PaddedPoints;

                            await IsLineBetweeenPolygons(mping.Line1MovementDirectionMovement, pgon, pts, prevGons,
                                prevGons?.PaddedPoints ?? new List<Point>(), left, move, delta, false);
                            left.GetLine();
                            await IsLineBetweeenPolygons(mping.Line2MovementDirectionMovement, pgon, pts, nextPgon,
                                nextPgon?.PaddedPoints ?? new List<Point>(), right, move, delta, true);
                            right.GetLine();
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

        private async Task IsLineBetweeenPolygons(Movement mpingLineMovementDirectionMovement, PolyGons pgon, List<Point> pts, PolyGons prevGons, List<Point> prevGonsPts,
            Lins currentLine, int move, double delta, bool invert)
        {
            if (mpingLineMovementDirectionMovement != Movement.NoMovement)
            {
                var startLine = new Lins()
                {
                    Up = currentLine.Up,
                    Down = currentLine.Down,
                };
                var direction = 1;

                Direction finalDirectionOfPolygonForLine1 = Direction.Intersect;
                Direction finalInvertDirectionOfPolygonForLine1 = Direction.Intersect;
                switch (mpingLineMovementDirectionMovement)
                {
                    // left movement = -1
                    case Movement.Left:
                        finalDirectionOfPolygonForLine1 = Direction.Right;
                        finalInvertDirectionOfPolygonForLine1 = Direction.Left;
                        direction = -1;
                        break;
                    // right movement = +1
                    case Movement.Right:
                        finalDirectionOfPolygonForLine1 = Direction.Left;
                        finalInvertDirectionOfPolygonForLine1 = Direction.Right;
                        direction = +1;
                        break;
                }

                Direction thisPolygonSide;
                Direction nextPolygonSide;
                thisPolygonSide = pgon.fine_polygon_direction(pts, startLine);
                if (prevGons != null)
                {
                    nextPolygonSide = prevGons.fine_polygon_direction(prevGonsPts, startLine);
                }
                else
                {
                    nextPolygonSide = finalInvertDirectionOfPolygonForLine1;
                }

                MyCanvas.Children.Add(startLine.GetTempLine());
                while
                (
                    thisPolygonSide != finalDirectionOfPolygonForLine1
                    ||
                    nextPolygonSide != finalInvertDirectionOfPolygonForLine1
                )
                {
                    startLine.GetTempLine();
                    await Task.Delay((int)SpeedSlider.Value);
                    if (startLine.AngleFromTopDegrees > 45)
                    {
                        MyCanvas.Children.Remove(startLine.GetTempLine());
                        //reset by shifting the near box point
                        startLine = new Lins()
                        {
                            Up = new Point(currentLine.Up.X /*+ (direction * move)*/, currentLine.Up.Y),
                            Down = new Point(startLine.Down.X + (direction * move), startLine.Down.Y),
                        };

                        MyCanvas.Children.Add(startLine.GetTempLine());
                        var bottomX = startLine.Down.X + (direction * move);
                        var limit = (currentLine.Down.X + (direction * delta));
                        if (Math.Abs(limit - bottomX) < 5)
                        {
                            break;
                        }
                    }
                    else
                    {
                        startLine.Up = new Point(startLine.Up.X + (direction * move), startLine.Up.Y);
                    }

                    thisPolygonSide = pgon.fine_polygon_direction(pts, startLine);
                    if (prevGons != null)
                    {
                        nextPolygonSide = prevGons.fine_polygon_direction(prevGonsPts, startLine);
                    }
                    else
                    {
                        nextPolygonSide = finalInvertDirectionOfPolygonForLine1;
                    }
                }

                startLine.Up = new Point(startLine.Up.X + (direction * move), startLine.Up.Y); // extra padding
                currentLine.Up = startLine.Up;
                currentLine.Down = startLine.Down;

                MyCanvas.Children.Remove(startLine.GetTempLine());
            }
        }


        private void AdjustLower3_OnClick(object sender, RoutedEventArgs e)
        {
            

        }
    }
}
