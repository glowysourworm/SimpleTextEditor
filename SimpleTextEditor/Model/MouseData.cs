using System.Drawing;
using System.Windows.Input;

namespace SimpleTextEditor.Model
{
    public class MouseData
    {
        public Point? TopLeft { get; set; }
        public Point? TopRight { get; set; }
        public MouseButtonState LeftButton { get; set; }
    }
}
