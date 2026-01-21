using System.Windows;

using SimpleWpf.SimpleCollections.Collection;

namespace SimpleTextEditor.Model.Configuration
{
    public class TextEditorConfiguration
    {
        public TextProperties DefaultProperties { get; set; }
        public TextProperties DefaultHighlightProperties { get; set; }
        public SimpleDictionary<IndexRange, TextProperties> Properties { get; set; }
        public TextWrapping TextWrapping { get; set; }

        public TextEditorConfiguration()
        {
            this.Properties = new SimpleDictionary<IndexRange, TextProperties>();
            this.DefaultProperties = new TextProperties();
            this.DefaultHighlightProperties = new TextProperties();
            this.TextWrapping = TextWrapping.NoWrap;
        }
    }
}
