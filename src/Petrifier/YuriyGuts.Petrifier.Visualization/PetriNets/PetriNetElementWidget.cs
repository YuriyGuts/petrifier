using System;
using System.Drawing;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public abstract class PetriNetElementWidget : WidgetBase
    {
        private PetriNetElement element;

        protected PetriNetElementWidget()
        {
            Size = 40;
        }

        protected PetriNetElementWidget(PetriNetElement element) : this()
        {
            Element = element;
        }

        public virtual Type ElementType
        {
            get { return typeof(PetriNetElement); }
        }

        public PetriNetElement Element
        {
            get { return element; }
            set
            {
                if (value != null && !ElementType.IsInstanceOfType(value))
                {
                    throw new ArgumentException("Assigned value type does not match the expected type: " + ElementType.FullName);
                }
                element = value;
            }
        }

        protected virtual string GetLabelText()
        {
            if (Element != null)
            {
                if (Element is ILabeledElement)
                {
                    return (Element as ILabeledElement).Label ?? Element.ID;
                }
                return Element.ID;
            }
            return null;
        }

        public abstract Point GetLineIntersectionPoint(Point lineStart);

        public virtual void DrawLabel(WidgetRenderingContext context)
        {
            string labelText = GetLabelText();
            if (labelText == null)
            {
                return;
            }

            Graphics g = context.Graphics;
            Font labelFont = context.TextFont;
            SizeF labelSize = g.MeasureString(labelText, labelFont);

            g.DrawString(labelText, labelFont, Brushes.WhiteSmoke, Location.X - labelSize.Width / 2 + 1, Location.Y + Size / 10 * 7 + 1);
            g.DrawString(labelText, labelFont, Brushes.Black, Location.X - labelSize.Width / 2, Location.Y + Size / 10 * 7);
        }
    }
}
