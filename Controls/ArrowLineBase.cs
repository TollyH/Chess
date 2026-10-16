//----------------------------------------------
// ArrowLineBase.cs (c) 2007 by Charles Petzold
// Modified 2026 for Avalonia by Tolly Hill
//----------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Petzold.Media2D
{
    /// <summary>
    ///     Provides a base class for ArrowLine and ArrowPolyline.
    ///     This class is abstract.
    /// </summary>
    public abstract class ArrowLineBase : Shape
    {
        protected PathGeometry pathgeo;
        protected PathFigure pathfigLine;
        protected PolyLineSegment polysegLine;

        PathFigure pathfigHead1;
        PolyLineSegment polysegHead1;
        PathFigure pathfigHead2;
        PolyLineSegment polysegHead2;

        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly StyledProperty<double> ArrowAngleProperty =
            AvaloniaProperty.Register<ArrowLineBase, double>("ArrowAngle");

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set => SetValue(ArrowAngleProperty, value);
            get => GetValue(ArrowAngleProperty);
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly StyledProperty<double> ArrowLengthProperty =
            AvaloniaProperty.Register<ArrowLineBase, double>("ArrowLength");

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set => SetValue(ArrowLengthProperty, value);
            get => GetValue(ArrowLengthProperty);
        }

        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly StyledProperty<ArrowEnds> ArrowEndsProperty =
            AvaloniaProperty.Register<ArrowLineBase, ArrowEnds>("ArrowEnds");

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set => SetValue(ArrowEndsProperty, value);
            get => GetValue(ArrowEndsProperty);
        }

        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> IsArrowClosedProperty =
            AvaloniaProperty.Register<ArrowLineBase, bool>("IsArrowClosed");

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set => SetValue(IsArrowClosedProperty, value);
            get => GetValue(IsArrowClosedProperty);
        }

        /// <summary>
        ///     Initializes a new instance of ArrowLineBase.
        /// </summary>
        public ArrowLineBase()
        {
            pathgeo = new PathGeometry()
            {
                Figures = new PathFigures()
            };

            pathfigLine = new PathFigure();
            polysegLine = new PolyLineSegment();
            pathfigLine.Segments?.Add(polysegLine);

            pathfigHead1 = new PathFigure();
            polysegHead1 = new PolyLineSegment();
            pathfigHead1.Segments?.Add(polysegHead1);

            pathfigHead2 = new PathFigure();
            polysegHead2 = new PolyLineSegment();
            pathfigHead2.Segments?.Add(polysegHead2);
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry? CreateDefiningGeometry()
        {
            int count = polysegLine.Points.Count;

            if (count > 0)
            {
                // Draw the arrow at the start of the line.
                if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                {
                    Point pt1 = pathfigLine.StartPoint;
                    Point pt2 = polysegLine.Points[0];
                    pathgeo.Figures?.Add(CalculateArrow(pathfigHead1, pt2, pt1));
                }

                // Draw the arrow at the end of the line.
                if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                {
                    Point pt1 = count == 1 ? pathfigLine.StartPoint :
                        polysegLine.Points[count - 2];
                    Point pt2 = polysegLine.Points[count - 1];
                    pathgeo.Figures?.Add(CalculateArrow(pathfigHead2, pt1, pt2));
                }
            }
            return pathgeo;
        }

        PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            Matrix matx = Matrix.CreateRotation(ArrowAngle / 2);
            Vector vect = pt1 - pt2;
            vect = vect.Normalize();
            vect *= ArrowLength;
            Point vectPt = new(vect.X, vect.Y);

            PolyLineSegment? polyseg = pathfig.Segments?[0] as PolyLineSegment;
            if (polyseg is null)
            {
                return pathfig;
            }
            polyseg.Points.Clear();
            pathfig.StartPoint = pt2 + vectPt * matx;
            polyseg.Points.Add(pt2);

            matx = Matrix.CreateRotation((ArrowAngle / 2) - ArrowAngle);
            polyseg.Points.Add(pt2 + vectPt * matx);
            pathfig.IsClosed = IsArrowClosed;

            return pathfig;
        }
    }
}
