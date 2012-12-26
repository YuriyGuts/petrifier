using System;
using System.Drawing;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public class TransitionWidget : PetriNetElementWidget
    {
        public TransitionWidget()
        {
        }

        public TransitionWidget(PetriNetElement element) : base(element)
        {
        }

        public override Type ElementType
        {
            get { return typeof(Transition); }
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
            g.DrawRectangle(Pens.Black, Location.X - Size / 2, Location.Y - Size / 2, Size, Size);

            DrawLabel(context);
        }
    }
}
