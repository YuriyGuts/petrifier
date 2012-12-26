using System.Drawing;

namespace YuriyGuts.Petrifier.Visualization
{
    public abstract class WidgetBase
    {
        public virtual Rectangle GetBoundingRectangle()
        {
            return new Rectangle(Location.X - Size / 2, Location.Y - Size / 2, Size, Size);
        }

        public Point Location { get; set; }

        public int Size { get; set; }

        public abstract void Render(WidgetRenderingContext context);
    }
}
