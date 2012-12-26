using System;
using System.Drawing;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public class OrJoinWidget : PetriNetElementWidget
    {
        public OrJoinWidget()
        {
        }

        public OrJoinWidget(PetriNetElement element) : base(element)
        {
        }

        public override Type ElementType
        {
            get { return typeof(OrJoin); }
        }

        public override Point GetLineIntersectionPoint(Point lineStart)
        {
            return GeometryHelper.GetSquareLineIntersectionPoint(Location, Size, lineStart);
        }

        public override void Render(WidgetRenderingContext context)
        {
            Graphics g = context.Graphics;

            g.FillRectangle(!context.IsSelected ? Brushes.White : Brushes.LightYellow, Location.X - Size / 2, Location.Y - Size / 2, Size + 1, Size + 1);

            g.DrawRectangle(Pens.Gainsboro, Location.X - Size / 2 + 1, Location.Y - Size / 2 + 1, Size, Size);
            g.FillRectangle(!context.IsSelected ? Brushes.Gainsboro : Brushes.DarkGray, Location.X - Size / 2, Location.Y - Size / 2, Size / 10 * 4, Size);
            g.DrawRectangle(Pens.Black, Location.X - Size / 2, Location.Y - Size / 2, Size, Size);
            g.DrawLine(Pens.Black, Location.X - Size / 10, Location.Y - Size / 2, Location.X - Size / 10, Location.Y + Size / 2);

            Point[] polygonPoints = new[]
            {
                new Point(Location.X - Size / 10, Location.Y - Size / 2),
                new Point(Location.X - Size / 2, Location.Y),
                new Point(Location.X - Size / 10, Location.Y + Size / 2),
            };
            g.FillPolygon(!context.IsSelected ? Brushes.White : Brushes.LightYellow, polygonPoints);
            g.DrawPolygon(Pens.Black, polygonPoints);

            DrawLabel(context);
        }
    }
}
