using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SimpleTextEditor.Component;
using SimpleTextEditor.Component.Interface;
using SimpleTextEditor.Model;

namespace SimpleTextEditor
{
    public class TextEditor : Control
    {
        public static readonly DependencyProperty FocusedBackgroundProperty =
            DependencyProperty.Register("FocusedBackground", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty FocusedBorderBrushProperty =
            DependencyProperty.Register("FocusedBorderBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.DodgerBlue));

        public static readonly DependencyProperty FocusedCaretBrushProperty =
            DependencyProperty.Register("FocusedCaretBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty CaretBrushProperty =
            DependencyProperty.Register("CaretBrush", typeof(Brush), typeof(TextEditor), new PropertyMetadata(Brushes.LightGray));

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

        // Have to wait for control loading event to set the text properties
        IDocument? _document;

        Point? _mouseDownPoint;
        Rect _mouseSelectionRect;

        public TextEditor()
        {
            this.Cursor = Cursors.IBeam;

            _document = null;
            _mouseSelectionRect = new Rect();

            this.Loaded += TextEditor_Loaded;
        }

        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _document = new Document(this.FontFamily,
                                     this.FontSize,
                                     Brushes.Black,
                                     Brushes.Transparent,
                                     Brushes.White,
                                     Brushes.CadetBlue,
                                     TextWrapping.Wrap);

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
                constraint.Width -= (this.Padding.Left + this.Padding.Right);
                constraint.Height -= (this.Padding.Top + this.Padding.Bottom);

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

            switch (controlInput)
            {
                case ControlInput.None:
                    break;
                case ControlInput.Backspace:
                    _document.ProcessRemoveText(_document.TextLength - 1, 1);
                    break;
                case ControlInput.BeginningOfDocument:
                case ControlInput.BeginningOfLine:
                case ControlInput.CharacterLeft:
                case ControlInput.CharacterRight:
                case ControlInput.DeleteCurrentCharacter:
                case ControlInput.EndOfDocument:
                case ControlInput.EndOfLine:
                case ControlInput.LineDown:
                case ControlInput.LineUp:
                case ControlInput.PageDown:
                case ControlInput.PageUp:
                case ControlInput.WordLeft:
                case ControlInput.WordRight:
                default:
                    throw new Exception("Unhandled ControlInput");
            }

            if (controlInput != ControlInput.None)
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

            // Store Mouse Down Point
            if (e.LeftButton == MouseButtonState.Pressed)
                _mouseDownPoint = e.GetPosition(this);

            if (this.IsFocused)
                return;

            // Does not focus by default
            this.Focus();

            InvalidateVisual();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDownPoint != null)
            {
                var point = e.GetPosition(this);
                var left = Math.Min(_mouseDownPoint.Value.X, point.X);
                var top = Math.Min(_mouseDownPoint.Value.Y, point.Y);
                var right = Math.Max(_mouseDownPoint.Value.X, point.X);
                var bottom = Math.Max(_mouseDownPoint.Value.Y, point.Y);

                _mouseSelectionRect.X = left;
                _mouseSelectionRect.Y = top;
                _mouseSelectionRect.Width = right - left;
                _mouseSelectionRect.Height = bottom - top;

                _document.ProcessMouseInput(new MouseData()
                {
                    LeftButton = e.LeftButton,
                    SelectionBounds = _mouseSelectionRect
                });

                InvalidateVisual();
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.LeftButton == MouseButtonState.Released)
                _mouseDownPoint = null;
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
            var constraint = new Size(this.RenderSize.Width - this.Padding.Left - this.Padding.Right,
                                      this.RenderSize.Height - this.Padding.Top - this.Padding.Bottom);
            var position = new Point(0, this.Padding.Top);

            foreach (var visualLine in _document.GetVisualElements())
            {
                /*
                position.X = visualLine.Element.Start + this.Padding.Left;
                position.Y += visualLine.Element.TextHeight;

                visualLine.Element.Draw(drawingContext, position, InvertAxes.None);
                */

                position.X = visualLine.Position.VisualBounds.X + this.Padding.Left;
                position.Y = visualLine.Position.VisualBounds.Y + this.Padding.Top;

                drawingContext.DrawText(visualLine.Element, visualLine.Position.VisualBounds.TopLeft);
            }

            if (caretBounds.Height > 0)
            {
                caretBounds.X += this.Padding.Left + 2; // KLUDGE
                caretBounds.Y += this.Padding.Top + 2;

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
