using System;
using System.Drawing;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public static class GeometryHelper
    {
        public static Point GetSquareLineIntersectionPoint(Point squareCenter, int squareSize, Point lineStart)
        {
            int a = Math.Abs(squareCenter.X - lineStart.X);
            int b = Math.Abs(squareCenter.Y - lineStart.Y);
            double R = squareSize / 2.0;

            if (a == 0)
            {
                return new Point(squareCenter.X, (int)(squareCenter.Y - R * (squareCenter.Y - lineStart.Y) / b));
            }
            if (b == 0)
            {
                return new Point((int)(squareCenter.X - R * (squareCenter.X - lineStart.X) / a), squareCenter.Y);
            }

            double tan = (double)a / b;

            if (a > b)
            {
                double n = R / tan;
                return new Point
                (
                    (int)(squareCenter.X - R * (squareCenter.X - lineStart.X) / a),
                    (int)(squareCenter.Y - n * (squareCenter.Y - lineStart.Y) / b)
                );
            }
            else
            {
                double n = R * tan;
                return new Point
                (
                    (int)(squareCenter.X - n * (squareCenter.X - lineStart.X) / a),
                    (int)(squareCenter.Y - R * (squareCenter.Y - lineStart.Y) / b)
                );
            }
        }

        public static Point GetCircleLineIntersectionPoint(Point circleCenter, int circleDiameter, Point lineStart)
        {
            int a = Math.Abs(circleCenter.X - lineStart.X);
            int b = Math.Abs(circleCenter.Y - lineStart.Y);
            double c = Math.Sqrt(a * a + b * b);

            double r = circleDiameter / 2.0;

            if (a == 0)
            {
                return new Point(circleCenter.X, (int)(circleCenter.Y - r * (circleCenter.Y - lineStart.Y) / b));
            }
            if (b == 0)
            {
                return new Point((int)(circleCenter.X - r * (circleCenter.X - lineStart.X) / a), circleCenter.Y);
            }

            double sin = a / c;
            double cos = b / c;

            double m = r * sin;
            double n = r * cos;

            return new Point
            (
                (int)(circleCenter.X - m * (circleCenter.X - lineStart.X) / a),
                (int)(circleCenter.Y - n * (circleCenter.Y - lineStart.Y) / b)
            );
        }

        public static bool IsPointInsideRectangle(Point point, Rectangle rectangle)
        {
            return point.X >= rectangle.Left && point.X <= rectangle.Right
                && point.Y >= rectangle.Top && point.Y <= rectangle.Bottom;
        }
    }
}
