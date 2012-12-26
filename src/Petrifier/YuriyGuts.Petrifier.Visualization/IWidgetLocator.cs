using System.Drawing;

namespace YuriyGuts.Petrifier.Visualization
{
    public interface IWidgetLocator
    {
        WidgetBase FindWidget(string query);

        WidgetBase GetWidgetAtPoint(Point point);
    }
}
