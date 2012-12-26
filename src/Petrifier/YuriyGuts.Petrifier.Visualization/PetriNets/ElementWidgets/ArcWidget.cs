using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public class ArcWidget : PetriNetElementWidget
    {
        public ArcWidget()
        {
        }

        public ArcWidget(PetriNetElement element) : base(element)
        {
        }

        public override Type ElementType
        {
            get { return typeof(Arc); }
        }

        public override Rectangle GetBoundingRectangle()
        {
            return new Rectangle(-1, -1, 0, 0);
        }

        public override Point GetLineIntersectionPoint(Point lineStart)
        {
            throw new NotImplementedException();
        }

        public override void Render(WidgetRenderingContext context)
        {
            Graphics g = context.Graphics;

            if (!(Element is Arc))
            {
                throw new InvalidOperationException("Petri Net Element must be convertible to Arc for ArcWidget.");
            }
            Arc arc = (Arc)Element;

            if (arc.StartElement == null || arc.EndElement == null)
            {
                throw new InvalidOperationException("StartElement and EndElement of arc cannot be null.");
            }

            PetriNetElementWidget startWidget = (PetriNetElementWidget)context.WidgetLocator.FindWidget(arc.StartElement.ID);
            PetriNetElementWidget endWidget = (PetriNetElementWidget)context.WidgetLocator.FindWidget(arc.EndElement.ID);

            Point arcStartPoint = startWidget.GetLineIntersectionPoint(endWidget.Location);
            Point arcEndPoint = endWidget.GetLineIntersectionPoint(startWidget.Location);

            using (Pen arrowPen = new Pen(Brushes.Black))
            {
                using (AdjustableArrowCap arrowCap = new AdjustableArrowCap(2, 1))
                {
                    arrowCap.WidthScale = 5;
                    arrowCap.BaseCap = LineCap.Square;
                    arrowCap.Height = 2;
                    arrowPen.CustomEndCap = arrowCap;
                    g.DrawLine(arrowPen, arcStartPoint, arcEndPoint);
                }
            }
        }
    }
}
