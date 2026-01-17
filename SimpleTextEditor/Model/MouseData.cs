using System.Windows;
using System.Windows.Input;

namespace SimpleTextEditor.Model
{
    public class MouseData
    {
        public Rect SelectionBounds { get; set; }
        public MouseButtonState LeftButton { get; set; }

        public bool IsSet()
        {
            return this.SelectionBounds != Rect.Empty;
        }
    }
}
