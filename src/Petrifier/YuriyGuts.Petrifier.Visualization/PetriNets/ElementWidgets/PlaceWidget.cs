using System;
using System.Drawing;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public class PlaceWidget : PetriNetElementWidget
    {
        public PlaceWidget()
        {
        }

        public PlaceWidget(PetriNetElement element) : base(element)
        {
        }

        public override Type ElementType
        {
            get { return typeof(Place); }
        }

        public override Point GetLineIntersectionPoint(Point lineStart)
        {
            return GeometryHelper.GetCircleLineIntersectionPoint(Location, Size, lineStart);
        }

        public override void Render(WidgetRenderingContext context)
        {
            Graphics g = context.Graphics;

            g.FillEllipse(!context.IsSelected ? Brushes.White : Brushes.LightYellow, Location.X - Size / 2, Location.Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Gainsboro, Location.X - Size / 2, Location.Y - Size / 2 + 1, Size, Size);
            g.DrawEllipse(Pens.Gainsboro, Location.X - Size / 2 + 1, Location.Y - Size / 2 + 1, Size, Size);
            g.DrawEllipse(Pens.Black, Location.X - Size / 2, Location.Y - Size / 2, Size, Size);

            Place place = (Place)Element;
            if (place.Marking == 1)
            {
                g.FillEllipse(Brushes.Black, Location.X - Size / 5, Location.Y - Size / 5, Size / 5 * 2, Size / 5 * 2);
            }
            if (place.Marking == 2)
            {
                g.FillEllipse(Brushes.Black, Location.X - Size / 20 * 7, Location.Y - Size / 20 * 3, Size / 10 * 3, Size / 10 * 3);
                g.FillEllipse(Brushes.Black, Location.X + Size / 20 * 2 - 1, Location.Y - Size / 20 * 3, Size / 10 * 3, Size / 10 * 3);
            }
            if (place.Marking == 3)
            {
                g.FillEllipse(Brushes.Black, Location.X - Size / 20 * 7, Location.Y - Size / 20 * 5, Size / 10 * 3, Size / 10 * 3);
                g.FillEllipse(Brushes.Black, Location.X + Size / 20 * 1, Location.Y - Size / 20 * 5, Size / 10 * 3, Size / 10 * 3);
                g.FillEllipse(Brushes.Black, Location.X - Size / 20 * 3, Location.Y + Size / 20 * 2, Size / 10 * 3, Size / 10 * 3);
            }
            if (place.Marking > 3)
            {
                using (Font font = new Font(context.TextFont.FontFamily, Size / 2.5F))
                {
                    SizeF textSize = g.MeasureString(place.Marking.ToString(), font);
                    g.DrawString
                    (
                        place.Marking.ToString(),
                        font,
                        Brushes.Black,
                        new RectangleF
                        (
                            Location.X - textSize.Width / 2,
                            Location.Y - textSize.Height / 2 + 1,
                            textSize.Width,
                            textSize.Height
                        )
                    );
                }
            }

            DrawLabel(context);
        }
    }
}
