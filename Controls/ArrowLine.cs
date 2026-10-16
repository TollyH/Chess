//------------------------------------------
// ArrowLine.cs (c) 2007 by Charles Petzold
// Modified 2026 for Avalonia by Tolly Hill
//------------------------------------------

using Avalonia;
using Avalonia.Media;

namespace Petzold.Media2D
{
    /// <summary>
    ///     Draws a straight line between two points with
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowLine : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the X1 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> X1Property =
            AvaloniaProperty.Register<ArrowLine, double>("X1");

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine start point.
        /// </summary>
        public double X1
        {
            set => SetValue(X1Property, value);
            get => GetValue(X1Property);
        }

        /// <summary>
        ///     Identifies the Y1 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> Y1Property =
            AvaloniaProperty.Register<ArrowLine, double>("Y1");

        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine start point.
        /// </summary>
        public double Y1
        {
            set => SetValue(Y1Property, value);
            get => GetValue(Y1Property);
        }

        /// <summary>
        ///     Identifies the X2 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> X2Property =
            AvaloniaProperty.Register<ArrowLine, double>("X2");

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine end point.
        /// </summary>
        public double X2
        {
            set => SetValue(X2Property, value);
            get => GetValue(X2Property);
        }

        /// <summary>
        ///     Identifies the Y2 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> Y2Property =
            AvaloniaProperty.Register<ArrowLine, double>("Y2");

        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine end point.
        /// </summary>
        public double Y2
        {
            set => SetValue(Y2Property, value);
            get => GetValue(Y2Property);
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry? CreateDefiningGeometry()
        {
            pathgeo = new PathGeometry()
            {
                Figures = new PathFigures()
            };

            // Define a single PathFigure with the points.
            pathfigLine.StartPoint = new Point(X1, Y1);
            polysegLine.Points.Clear();
            polysegLine.Points.Add(new Point(X2, Y2));
            pathgeo.Figures?.Add(pathfigLine);

            // Call the base property to add arrows on the ends.
            return base.CreateDefiningGeometry();
        }
    }
}
