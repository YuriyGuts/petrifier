using System.Drawing;

namespace YuriyGuts.Petrifier.Visualization
{
    public class WidgetRenderingContext
    {
        public Graphics Graphics { get; set; }

        public IWidgetLocator WidgetLocator { get; set; }

        public Font TextFont { get; set; }

        public bool IsSelected { get; set; }
    }
}
