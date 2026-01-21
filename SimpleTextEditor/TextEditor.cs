using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component;
using SimpleTextEditor.Component.Interface;
using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Configuration;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor
{
    public class TextEditor : Control
    {
        #region (public) Dependency Properties
        public static readonly DependencyProperty FocusedBackgroundProperty =
            DependencyProperty.Register("FocusedBackground", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty FocusedBorderBrushProperty =
            DependencyProperty.Register("FocusedBorderBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.DodgerBlue));

        public static readonly DependencyProperty FocusedCaretBrushProperty =
            DependencyProperty.Register("FocusedCaretBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty CaretBrushProperty =
            DependencyProperty.Register("CaretBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty TextHighlightForegroundProperty =
            DependencyProperty.Register("TextHighlightForeground", typeof(Brush), typeof(TextEditor));

        public static readonly DependencyProperty TextHighlightBackgroundProperty =
            DependencyProperty.Register("TextHighlightBackground", typeof(Brush), typeof(TextEditor));
        #endregion

        #region (public) Properties
        public Brush FocusedBackground
        {
            get { return (Brush)GetValue(FocusedBackgroundProperty); }
            set { SetValue(FocusedBackgroundProperty, value); }
        }
        public Brush FocusedBorderBrush
        {
            get { return (Brush)GetValue(FocusedBorderBrushProperty); }
            set { SetValue(FocusedBorderBrushProperty, value); }
        }
        public Brush FocusedCaretBrush
        {
            get { return (Brush)GetValue(FocusedCaretBrushProperty); }
            set { SetValue(FocusedCaretBrushProperty, value); }
        }
        public Brush CaretBrush
        {
            get { return (Brush)GetValue(CaretBrushProperty); }
            set { SetValue(CaretBrushProperty, value); }
        }
        public Brush TextHighlightForeground
        {
            get { return (Brush)GetValue(TextHighlightForegroundProperty); }
            set { SetValue(TextHighlightForegroundProperty, value); }
        }
        public Brush TextHighlightBackground
        {
            get { return (Brush)GetValue(TextHighlightBackgroundProperty); }
            set { SetValue(TextHighlightBackgroundProperty, value); }
        }
        #endregion

        // Have to wait for control loading event to set the text properties
        IDocument? _document;

        // Primary Configuration 
        TextEditorConfiguration _configuration;

        public TextEditor()
        {
            this.Cursor = Cursors.IBeam;

            _document = null;
            _configuration = new TextEditorConfiguration();

            this.Loaded += TextEditor_Loaded;
        }

        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Copy Font Settings -> Configuration
            _configuration.DefaultProperties.SetFromControl(this);
            _configuration.DefaultHighlightProperties.SetFromControl(this);

            // Override bound properties
            _configuration.DefaultHighlightProperties.Background = this.TextHighlightBackground;
            _configuration.DefaultHighlightProperties.Foreground = this.TextHighlightForeground;

            _document = new Document();
            _document.Initialize(new VisualInputData(_configuration, this.RenderSize));

            InvalidateVisual();
        }

        public string GetText()
        {
            return "TODO";
        }

        public void SetText(string text)
        {
            if (_document == null)
                throw new Exception("Must wait until control loads to set text");

            _document.Load(text);

            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (constraint.Width == 0 ||
                double.IsNaN(constraint.Width) ||
                constraint.Height == 0 ||
                double.IsNaN(constraint.Height) ||
                _document == null)
                return constraint;

            else
            {
                // Add Control Padding
                //constraint.Width -= (this.Padding.Left + this.Padding.Right);
                //constraint.Height -= (this.Padding.Top + this.Padding.Bottom);

                // Efficiency check is built into the Document implementation
                var desiredSize = _document.Measure(constraint);

                this.MinWidth = desiredSize.Width;
                this.MinHeight = desiredSize.Height;

                if (desiredSize.Width > constraint.Width)
                    desiredSize.Width = constraint.Width;

                if (desiredSize.Height > constraint.Height)
                    desiredSize.Height = constraint.Height;

                return desiredSize;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_document == null)
                return;

            base.OnKeyDown(e);

            var controlInput = ControlInputFactory.Convert(e.Key);

            // Must Handle Event (if it was meant for the IDocument)
            if (_document == null && controlInput != ControlInput.None)
            {
                e.Handled = true;
                return;
            }

            var invalidate = _document.ProcessControlInput(controlInput);

            if (controlInput != ControlInput.None || invalidate)
            {
                e.Handled = true;

                InvalidateMeasure();
                InvalidateVisual();
            }
        }
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (_document == null)
                return;

            base.OnTextInput(e);

            _document.ProcessInputText(e.Text);

            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            // Does not invalidate by default
            InvalidateVisual();
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var invalidate = !this.IsFocused || _document.ProcessMouseButtonDown(e.GetPosition(this), e.LeftButton, e.RightButton);

            // Does not focus by default
            if (!this.IsFocused)
                this.Focus();

            if (invalidate)
                InvalidateVisual();
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            var invalidate = _document.ProcessMouseButtonDown(e.GetPosition(this), e.LeftButton, e.RightButton);

            if (invalidate)
                InvalidateVisual();
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            var invalidate = _document.ProcessMouseButtonDown(e.GetPosition(this), e.LeftButton, e.RightButton);

            if (invalidate)
                InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_document == null)
                return;

            Brush background = this.Background;
            Brush border = this.BorderBrush;
            Brush caret = this.CaretBrush;
            Brush foreground = this.Foreground;

            // Limited Focus Visual Style
            if (this.IsFocused || this.IsKeyboardFocused)
            {
                //background = this.FocusedBackground;
                //border = this.FocusedBorderBrush;
                //caret = this.FocusedCaretBrush;
            }

            // Border / Background (Outer Padded Area + Control Area)
            drawingContext.DrawRectangle(background, new Pen(border, this.BorderThickness.Top), new Rect(this.RenderSize));

            var caretBounds = _document.GetCaretBounds();
            //var constraint = new Size(this.RenderSize.Width - this.Padding.Left - this.Padding.Right,
            //                          this.RenderSize.Height - this.Padding.Top - this.Padding.Bottom);
            //var position = new Point(0, this.Padding.Top);
            var position = new Point(0, 0);

            foreach (var visualLine in _document.GetVisualElements())
            {
                position.X = visualLine.Element.Start;
                position.Y += visualLine.Element.TextHeight;

                visualLine.Element.Draw(drawingContext, position, InvertAxes.None);
            }

            if (caretBounds.Height > 0)
            {
                //caretBounds.X += this.Padding.Left + 2; // KLUDGE
                //caretBounds.Y += this.Padding.Top + 2;

                drawingContext.DrawRectangle(caret, null, caretBounds);
            }



            /*
            Brush background = this.Background;
            Brush border = this.BorderBrush;
            Brush caret = this.CaretBrush;

            // Limited Focus Visual Style
            if (this.IsFocused || this.IsKeyboardFocused)
            {
                background = this.FocusedBackground;
                border = this.FocusedBorderBrush;
                caret = this.FocusedCaretBrush;
            }

            // Border / Background (Outer Padded Area + Control Area)
            drawingContext.DrawRectangle(background, new Pen(border, this.BorderThickness.Top), new Rect(this.RenderSize));

            // Control Area
            var textArea = new Rect(this.Padding.Left,
                                    this.Padding.Top,
                                    this.RenderSize.Width - this.Padding.Left - this.Padding.Right,
                                    this.RenderSize.Height - this.Padding.Top - this.Padding.Bottom);

            var textRendering = _core.GetRendering(textArea.Size);          // Used for the measurement
            var caretBounds = _core.GetCaretRenderBounds();
            var caretRenderBounds = new Rect(caretBounds.X + textArea.X,
                                             caretBounds.Y + textArea.Y + 4,            // KLUDGE
                                             caretBounds.Width, caretBounds.Height);

            // Text Bitmap (if TextStore has non-null / empty text)
            if (textRendering != null)
            {
                drawingContext.DrawImage(textRendering, new Rect(this.Padding.Left, this.Padding.Top, textRendering.Width, textRendering.Height));
            }

            // Caret (pre-measured)
            if (caretRenderBounds.Height > 0)
                drawingContext.DrawRectangle(caret, null, caretRenderBounds);
            */
        }
    }
}
