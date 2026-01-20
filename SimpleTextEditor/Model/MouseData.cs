using System.Windows;
using System.Windows.Input;

namespace SimpleTextEditor.Model
{
    public class MouseData
    {
        public Point? MouseDownLocation { get; set; }
        public Point MouseLocation { get; set; }
        public MouseButtonState LeftButton { get; set; }
    }
}
