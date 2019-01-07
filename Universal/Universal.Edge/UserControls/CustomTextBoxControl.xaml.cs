using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Universal.Edge.UserControls
{
    public sealed partial class CustomTextBoxControl : UserControl
    {
        public event EventHandler Remove, OnModified;
        private int margin = 0;
        //private TextAnnotationOperation _model; // this is used to hold the reference of previous object
        private readonly TranslateTransform _transform;

        private readonly IDisposable _textChangeNotifyEventDisposable;
        private Canvas _parentTextCanvas;

        public CustomTextBoxControl()
        {
            _transform = new TranslateTransform();
            this.InitializeComponent();
            //this.DataContextChanged += OnDataContextChanged;
            this.Unloaded += OnUnloaded;
            MarkSelected(true);
            //MoveThumb2.AddHandler();
            this.AddHandler(PointerPressedEvent, new PointerEventHandler(MoveThumb_OnPointerPressed), false);
            this.AddHandler(PointerEnteredEvent, new PointerEventHandler(MoveThumb_OnPointerPressed), false);
            this.AddHandler(PointerReleasedEvent, new PointerEventHandler(MoveThumb_OnPointerReleased), false);
            this.AddHandler(PointerExitedEvent, new PointerEventHandler(MoveThumb_OnPointerReleased), false);
            this.AddHandler(PointerCaptureLostEvent, new PointerEventHandler(MoveThumb_OnPointerReleased), false);
            this.AddHandler(PointerCanceledEvent, new PointerEventHandler(MoveThumb_OnPointerReleased), false);
            //MoveThumb2.AddHandler(MoveThumb_OnPointerPressed,Thumb.PointerMovedEvent);
            this.RenderTransform = _transform;
            CommentTextBox.Paste += CommentTextBox_OnPaste;
/*

            _textChangeNotifyEventDisposable = Observable
                .FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                    h => CommentTextBox.TextChanged += h,
                    h => CommentTextBox.TextChanged -= h)
                .Select(x =>
                {
                    Reference.TextBox.Text = CommentTextBox.Text;
                    return x;
                })
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => Observable.Start(() => x))
                .Switch()
                .ObserveOn(this)
                .Subscribe(async x =>
                {
                    if (_textPasted)
                    {
                        var t = CommentTextBox.Text;
                        var start = CommentTextBox.SelectionStart;
                        CommentTextBox.Text = string.Empty;
                        CommentTextBox.Text = t;
                        CommentTextBox.SelectionStart = start;
                        _textPasted = false;
                        return;
                    }
                    await CommentTextBox_Resize_OnTextChanged();
                    if (TextAnnotation != null)
                    {
                        CommentTextBox.FontSize = TextAnnotation.FontSize;
                        TextAnnotation.Text = CommentTextBox.Text;
                    }
                });*/
        }

/*

        public TextAnnotationOperation TextAnnotation
        {
            get { return this.DataContext as TextAnnotationOperation; }
        }
*/

        //private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        //{
        //    //Reference.DataContext = DataContext;
        //    Unsubscribe();
        //    _model = TextAnnotation;
        //    if (_model != null)
        //    {
        //        this.SetSize(this._model.Size);
        //        if (CommentTextBox != null)
        //        {
        //            Reference.TextBox.FontSize = _model.FontSize;
        //            CommentTextBox.FontSize = _model.FontSize;
        //        }
        //    }
        //    Subscribe();
        //}

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unsubscribe();
            //this.DataContextChanged -= OnDataContextChanged;
            this.Unloaded -= OnUnloaded;
            _textChangeNotifyEventDisposable?.Dispose();
        }

        private void _model_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (
            //    e.PropertyName == nameof(TextAnnotationOperation.Text) ||
            //    e.PropertyName == nameof(TextAnnotationOperation.FontSize) ||
            //    e.PropertyName == nameof(TextAnnotationOperation.Size) ||
            //    e.PropertyName == nameof(TextAnnotationOperation.Point)
            //)
            //{
            //    OnModified?.Invoke(this, null);
            //}
        }

        private void Subscribe()
        {
            //if (_model != null)
            //{
            //    _model.PropertyChanged -= _model_OnPropertyChanged;
            //    _model.PropertyChanged += _model_OnPropertyChanged;
            //}
        }

        private void Unsubscribe()
        {
            //if (_model != null)
            //{
            //    _model.PropertyChanged -= _model_OnPropertyChanged;
            //}
        }


        public void SetParentCanvas(Canvas element)
        {
            _parentTextCanvas = element;
        }

        private void SetSize(Size size)
        {
            //this.Reference.ResizableContentGrid.Width = size.Width;
            //this.Reference.ResizableContentGrid.Height = size.Height;
            this.ResizableGrid.Width = size.Width;
            this.ResizableGrid.Height = size.Height;
        }


        #region Resize

        Size _originalSize;

        private void GrabLoaded(object sender, RoutedEventArgs e)
        {
            _originalSize = this.ResizableGrid.RenderSize;
        }

        private void GrabDelta(object sender, DragDeltaEventArgs e)
        {
            if (_isResizing == false)
            {
                return;
            }

            ChangeGrabDelta(sender, e);
        }

        private Point _resizeOrigin;

        private bool ChangeGrabDelta(object sender, DragDeltaEventArgs e)
        {
            if (_isResizing == false)
            {
                return false;
            }

            try
            {

                var undoWidth = this.ResizableGrid.ActualWidth;
                var undoHeight = this.ResizableGrid.ActualHeight;
                var newWidth = undoWidth + e.HorizontalChange;
                var newHeight = undoHeight + e.VerticalChange;
                var ds = CommentTextBox.DesiredSize;
                if (newWidth < 430)
                {
                    newWidth = 430;
                }

                if (newHeight < 70)
                {
                    newHeight = 70;
                }

                if (newHeight < (ds.Height + 70))
                {
                    newHeight = (ds.Height + 70);
                }

                if (_parentTextCanvas != null)
                {
                    var origin = _resizeOrigin;
                    var canvasWidth = _parentTextCanvas.Width;
                    var canvasHeight = _parentTextCanvas.Height;
                    var offsetX = origin.X + newWidth;
                    if ((offsetX) > canvasWidth)
                    {
                        newWidth = undoWidth;
                        //return false;
                    }

                    var offsetY = origin.Y + newHeight;
                    if ((offsetY) > canvasHeight)
                    {
                        newHeight = undoHeight;
                        //return false;
                    }

                    if (offsetY < 80)
                    {
                        return false;
                    }
                }

                //this.Reference.ResizableContentGrid.Width = newWidth;
                //this.Reference.ResizableContentGrid.Height = newHeight;
                this.ResizableGrid.Width = newWidth;
                this.ResizableGrid.Height = newHeight;
                return true;
            }
            catch (Exception ex)
            {
                //ExceptionHelper.LogException(ex, GetType().Name, nameof(ChangeGrabDelta));
                return false;
            }
        }

        private void GrabDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            //this.Reference.ResizableContentGrid.Height = _originalSize.Height;
            //this.Reference.ResizableContentGrid.Width = _originalSize.Width;
            this.ResizableGrid.Height = _originalSize.Height;
            this.ResizableGrid.Width = _originalSize.Width;
        }

        private void GrabCompleted(object sender, DragCompletedEventArgs e)
        {
            _isResizing = false;
            ShowResizeSelectionRegion(true);
            if (e.Canceled == false)
            {
                //this.TextAnnotation.Size = _originalSize = this.ResizableGrid.RenderSize;
            }
        }

        private bool _isResizing;
        private async void GrabStarted(object sender, DragStartedEventArgs e)
        {
            ShowResizeSelectionRegion(false);
            await Task.Delay(10);
            var transform = this.TransformToVisual(_parentTextCanvas);
            _resizeOrigin = transform.TransformPoint(new Point(0, 0));
            _isResizing = true;
        }

        #endregion


        /// <summary>
        /// triggers on remove button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetSelected();
                Remove?.Invoke(this, null);
                //FlyoutBase.GetAttachedFlyout(RemoveMenuFlyout)?.Hide();
            }
            catch (Exception exception)
            {
                //ExceptionHelper.LogException(exception, GetType().Name, nameof(RemoveButton_Clicked));
            }
        }


        #region Move

        private Point _originalPoint;

        private bool _isSelected;

        private void MoveGrabDelta(object sender, DragDeltaEventArgs e)
        {
            if (dict.Count > 1)
            {
                return;
            }
            var translateX = _transform.X + e.HorizontalChange;
            var translateY = _transform.Y + e.VerticalChange;

            var undoTranslateX = _transform.X ;
            var undoTranslateY = _transform.Y ;


            var newX = _originalPoint.X + translateX;
            var newY = _originalPoint.Y + translateY;
            if (newX < margin)
            {
                translateX = undoTranslateX;
            }

            if (newY < margin)
            {
                translateY = undoTranslateY;
            }

            if (_parentTextCanvas != null)
            {
                var canvasWidth = _parentTextCanvas.Width;
                var canvasHeight = _parentTextCanvas.Height;

                var offsetX = newX + this.ResizableGrid.Width;
                if ((offsetX + margin) > canvasWidth)
                {
                    offsetX = (_originalPoint.X + _transform.X) + this.ResizableGrid.Width;
                    if ((offsetX + margin) > canvasWidth)
                    {
                        translateX = canvasWidth - (offsetX + margin);
                    }
                    else
                    {
                        translateX = undoTranslateX;
                        //return;
                    }
                }

                var offsetY = newY + this.ResizableGrid.Height;
                if ((offsetY + margin) > canvasHeight)
                {
                    offsetY = (_originalPoint.Y + _transform.Y) + this.ResizableGrid.Height;
                    if ((offsetY + margin) > canvasHeight)
                    {
                        translateY = canvasHeight - (offsetY + margin);
                    }
                    else
                    {
                        translateY = undoTranslateY;
                    }
                }

                var diffWidth = Math.Abs(_parentTextCanvas.RenderSize.Width - this.ResizableGrid.RenderSize.Width) < 2;
                if (diffWidth)
                {
                    translateX = undoTranslateX;
                }
                var diffHeight = Math.Abs(_parentTextCanvas.RenderSize.Height - this.ResizableGrid.RenderSize.Height) < 2;
                if (diffHeight)
                {
                    translateY = undoTranslateY;
                }
            }

            _transform.X = translateX;
            _transform.Y = translateY;
        }

        private void MoveGrabLoaded(object sender, RoutedEventArgs e)
        {
            if (_parentTextCanvas != null)
            {
                var transform = this.TransformToVisual(_parentTextCanvas);
                transform.TryTransform(new Point(0, 0), out _originalPoint);
            }
        }

        private void MoveGrabCompleted(object sender, DragCompletedEventArgs e)
        {
            ShowMoveSelectionRegion(true);
            if (e.Canceled == false)
            {
                if (_parentTextCanvas != null)
                {
                    var transform = this.TransformToVisual(_parentTextCanvas);
                    //this.TextAnnotation.Point = transform.TransformPoint(new Point(0, 0));
                }
            }
        }

        private void MoveGrabStarted(object sender, DragStartedEventArgs e)
        {
            ShowMoveSelectionRegion(false);
        }

        #endregion
        #region TEXT COMMENT

        private bool _textPasted;
        private void CommentTextBox_OnPaste(object sender, TextControlPasteEventArgs e)
        {
            _textPasted = true;
        }

        private async Task CommentTextBox_Resize_OnTextChanged()
        {
            /*try
            {
                double deltaWidth = 0;
                double deltaHeight = 0;

                bool CanResize()
                {
                    if (string.IsNullOrEmpty(CommentTextBox.Text))
                    {
                        return true;
                    }

                    var rs = CommentTextBox.RenderSize;
                    CommentTextBox.InvalidateMeasure();
                    CommentTextBox.Measure(new Size(rs.Width, Double.PositiveInfinity));
                    var ds = CommentTextBox.DesiredSize;


                    deltaHeight = ds.Height - rs.Height;
                    deltaWidth = ds.Width - rs.Width;

                    bool canResize;
                    canResize = ((deltaHeight) > 1) || ((deltaWidth) > 4);
                    return canResize;
                }

                async Task<bool> UpdateGridDimensions(DragDeltaEventArgs e)
                {
                    try
                    {
                        if (e.HorizontalChange == 0 && e.VerticalChange == 0)
                        {
                            return false;
                        }

                        var newWidth = this.ResizableGrid.ActualWidth + e.HorizontalChange;
                        var newHeight = this.ResizableGrid.ActualHeight + e.VerticalChange + 10;

                        if (newWidth < 430)
                        {
                            newWidth = 430;
                        }
                        if (_parentTextCanvas != null)
                        {
                            var transform = this.TransformToVisual(_parentTextCanvas);
                            var origin = transform.TransformPoint(new Point(0, 0));
                            var newEnd = new Point(origin.X + newWidth, origin.Y + newHeight);

                            var parentBounds = new Rect(new Point(0, 0), _parentTextCanvas.RenderSize);

                            if (parentBounds.Contains(origin) == false)
                            {
                                return false;
                            }

                            if (parentBounds.Contains(newEnd) == false)
                            {
                                return false;
                            }

                        }

                        //Reference.ResizableContentGrid.Width = newWidth;
                        //Reference.ResizableContentGrid.Height = newHeight;

                        this.ResizableGrid.Width = newWidth;
                        this.ResizableGrid.Height = newHeight;

                        //Reference.TextBox.FontSize = TextAnnotation.FontSize;
                        //CommentTextBox.FontSize = TextAnnotation.FontSize;

                        await Task.Delay(100);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        //ExceptionHelper.LogException(ex, GetType().Name, nameof(UpdateGridDimensions));
                        return false;
                    }
                }

                while (CanResize())
                {
                    if (string.IsNullOrEmpty(CommentTextBox.Text))
                    {
                        Reference.ResizableContentGrid.Height = 120;
                        this.ResizableGrid.Height = 120;
                        await Task.Delay(100);
                        break;
                    }

                    if (await UpdateGridDimensions(new DragDeltaEventArgs(deltaWidth, deltaHeight)) == false)
                    {
                        FitToHeightIfRequired();
                        break;
                    }
                }

                void FitToHeightIfRequired()
                {
                    
                    if (CommentTextBox.DesiredSize.Height > this.ResizableGrid.Height)
                    {
                        var transform = this.TransformToVisual(_parentTextCanvas);
                        var origin = transform.TransformPoint(new Point(0, 0));

                        var parentBounds = new Rect(new Point(0, 0), _parentTextCanvas.RenderSize);

                        if (parentBounds.Contains(origin) == false)
                        {
                            return;
                        }

                        var newHeight = _parentTextCanvas.RenderSize.Height - origin.Y - 10;

                        var newEnd = new Point(origin.X, origin.Y + newHeight);

                        if (newHeight <= 70 || parentBounds.Contains(newEnd) == false)
                        {
                            return;
                        }

                        Reference.ResizableContentGrid.Height = newHeight;
                        this.ResizableGrid.Height = newHeight;
                    }
                }

                GrabCompleted(null, new DragCompletedEventArgs(deltaHeight, deltaWidth, false));
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogException(ex, GetType().Name, nameof(CommentTextBox_Resize_OnTextChanged));
            }*/
        }


        public void FocusInputBox()
        {
            if (CommentTextBox?.FocusState == FocusState.Unfocused)
            {
                CommentTextBox?.Focus(FocusState.Pointer);
                CommentTextBox.SelectionStart = CommentTextBox.Text.Length;
            }
        }

        public async void SetFontSize(double value)
        {
            //if (TextAnnotation.MaxFontSize < value || TextAnnotation.MinFontSize > value)
            //{
            //    //await CommentTextBox_Resize_OnTextChanged();
            //    //validation failed
            //    return;
            //}

            //CommentTextBox.FontSize = value;

            //if (string.IsNullOrEmpty(CommentTextBox?.Text) == false)
            //{
            //    await CommentTextBox_Resize_OnTextChanged();
            //}

            //CommentTextBox.FontSize = TextAnnotation.FontSize = value;
            //Reference.TextBox.FontSize = value;
        }

        #endregion

        public bool IsSelected
        {
            get { return _isSelected; }
        }

        public void MarkSelected(bool isSelected)
        {
            _isSelected = isSelected;
            CommentTextBox.IsTabStop = IsSelected;
            //MoveControl.Visibility =
            ResizeThumb.Visibility =
                MoveThumb.Visibility =
                    MoveThumb2.Visibility =
                        //------------//
                        RemoveButton.Visibility =
                            IsSelected ? Visibility.Visible : Visibility.Collapsed;

            this.Bindings.Update();

            if (IsSelected)
            {
                FocusInputBox();
            }
        }


        private void Control_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is UIElement element)
            {
                var point = e.GetPosition(element);
                //dict[point.PointerId] = point;
            }
            SetSelected();
        }

        private void SetSelected()
        {
            if (IsSelected == false)
            {
                OnSelected();
            }
        }

        public void OnSelected()
        {
            //Selected?.Invoke(this, TextAnnotation);
        }

        private void ResizableGrid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        #region Effect

        public const double FadeOpacity = 0.7;
        private void ShowMoveSelectionRegion(bool show)
        {
            ResizeThumb.Opacity = show ? 1 : 0;
            MoveThumb.Opacity = show ? 1 : FadeOpacity;
            //MoveControl.Opacity = show ? 1 : FadeOpacity;
        }

        private void ShowResizeSelectionRegion(bool show)
        {
            ResizeThumb.Opacity = show ? 1 : FadeOpacity;
            MoveThumb.Opacity = show ? 1 : 0;
            //MoveControl.Opacity = show ? 1 : 0;
        }

        #endregion

        private Dictionary<uint, PointerPoint> dict = new Dictionary<uint, PointerPoint>();
        private void MoveThumb_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //if (dict.Count > 2)
            //{
            //    dict.Clear();
            //}
            if (sender is UIElement element)
            {
                PointerPoint point;
                point = e.GetCurrentPoint(element);
                dict[point.PointerId] = point;
            }
        }

        private void MoveThumb_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //if (dict.Count > 2)
            //{
            //    dict.Clear();
            //}
            if (sender is UIElement element)
            {
                PointerPoint point;
                point = e.GetCurrentPoint(element);
                dict.Remove(point.PointerId);
            }
        }
    }
}
