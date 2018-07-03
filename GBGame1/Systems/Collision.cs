using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Systems {
    public static class Collision {
        private static List<SATAxis> tempAxes = new List<SATAxis>();

        /// <summary>
        /// Separate Collider a from Collider b.
        /// </summary>
        /// <param name="a">Collider to separate.</param>
        /// <param name="b">Collider to collide with.</param>
        /// <returns>Returns a Vector describing the separation vector Collider a must move by to solve collision.</returns>
        public static Vector2 Separate(Collider a, Collider b) {
            var separation_vector = new Vector2();

            // Coarse AABB check for early-out.
            if (BoxVsBox(a, b)) {

                float separation_magnitude = float.MaxValue;

                // Clear the list of temp axes.
                tempAxes.Clear();

                // Compile a list of unique axes to separate.
                foreach (SATAxis b_axis in b.Axes) {
                    tempAxes.Add(new SATAxis {
                        Vector = b_axis.Vector,
                        Gradient = b_axis.Gradient
                    });
                }
                foreach (SATAxis a_axis in a.Axes) {
                    //bool exists = false;
                    //foreach (SATAxis ba in b.Axes) {
                    //    if (!exists && Vector2.Dot(ba.Vector, a_axis.Vector) == 0f) {
                    //        exists = true;
                    //    }
                    //}
                    //if (!exists) {
                        // Add unique axis to the temporary axis list.
                        tempAxes.Add(new SATAxis {
                            Vector = a_axis.Vector,
                            Gradient = a_axis.Gradient
                        });
                    //}
                }

                // Loop over all of the unique axes and check for collision.
                foreach (SATAxis axis in tempAxes) {

                    // Project each collider onto the axis.
                    ProjectionResult result_a = Project(a, axis);
                    ProjectionResult result_b = Project(b, axis);

                    float off = -(result_a.Min + result_a.Max) / 2;

                    //// Draw projection basis
                    //Utils.QueueDebugPoly(new Vector2[] {
                    //    axis.Vector.PerRight() * 30f + axis.Vector * 10f,
                    //    axis.Vector.PerRight() * 30f + axis.Vector * -10f
                    //}, a.Position, Color.White);

                    // Draw projection results
                    Utils.QueueDebugPoly(new Vector2[] {
                        axis.Vector.PerRight() * 29f + axis.Vector * (result_a.Min + off),
                        axis.Vector.PerRight() * 29f + axis.Vector * (result_a.Max + off)
                    }, a.Position, new Color(255, 0, 0));

                    // Draw projection results
                    Utils.QueueDebugPoly(new Vector2[] {
                        axis.Vector.PerRight() * 28f + axis.Vector * (result_b.Min + off),
                        axis.Vector.PerRight() * 28f + axis.Vector * (result_b.Max + off)
                    }, a.Position, new Color(0, 255, 0));

                    float magnitude = 0f;

                    if (result_a.Mid > result_b.Mid) {
                        // Calculate overlap magnitude.
                        magnitude = result_b.Max - result_a.Min;

                        // Exit early if there is no overlap.
                        if (magnitude <= 0f) {
                            return new Vector2();
                        }
                    } else {
                        // Calculate overlap magnitude.
                        magnitude = result_b.Min - result_a.Max;

                        // Exit early if there is no overlap.
                        if (magnitude >= 0f) {
                            return new Vector2();
                        }
                    }

                    // Update separation vector if this overlap is the smallest so far.
                    if (Math.Abs(magnitude) < Math.Abs(separation_magnitude)) {
                        separation_vector = axis.Vector * magnitude;
                        separation_magnitude = magnitude;
                    }
                }

            }

            return separation_vector;
        }
        /// <summary>
        /// Simple separation between vector and rectangle.
        /// </summary>
        /// <param name="v">Vector to separate.</param>
        /// <param name="r">Rectangle to collide with.</param>
        /// <returns>Returns a vector that v must move by to resolve collision.</returns>
        public static Vector2 Separate(Vector2 v, Rectangle r) {
            var separation_vector = new Vector2();

            var left_edge = v.X - r.X;
            var right_edge = r.X + r.Width - v.X;
            var top_edge = v.Y - r.Y;
            var bottom_edge = r.Y + r.Height - v.Y;

            if (left_edge > 0 && right_edge > 0 && top_edge > 0 && bottom_edge > 0) {
                if (left_edge <= right_edge && left_edge <= top_edge && left_edge <= bottom_edge) {
                    separation_vector.X = -left_edge;
                }
                if (right_edge <= left_edge && right_edge <= top_edge && right_edge <= bottom_edge) {
                    separation_vector.X = right_edge;
                }
                if (top_edge <= left_edge && top_edge <= right_edge && top_edge <= bottom_edge) {
                    separation_vector.Y = -top_edge;
                }
                if (bottom_edge <= left_edge && bottom_edge <= right_edge && bottom_edge <= top_edge) {
                    separation_vector.Y = bottom_edge;
                }
            }
            return separation_vector;
        }

        /// <summary>
        /// Projects a collider onto an axis.
        /// </summary>
        /// <param name="collider">The collider to project onto the axis.</param>
        /// <param name="axis">The axis on which to project the collider.</param>
        /// <returns>Returns a ProjectionResult containing an upper and lower magnitude of the projection along the axis.</returns>
        public static ProjectionResult Project(Collider collider, SATAxis axis) {
            float min = float.MaxValue, max = float.MinValue;

            Vector2 axisn = axis.Vector.Normalized();

            foreach (Vector2 p in collider.Points) {
                float dp = Vector2.Dot(axisn, p + collider.Position);
                min = Math.Min(dp, min);
                max = Math.Max(dp, max);
            }

            return new ProjectionResult(min, max);
        }

        /// <summary>
        /// Collision check for the AABBs of two colliders.
        /// </summary>
        /// <param name="a">The first collider to check.</param>
        /// <param name="b">The second collider to check.</param>
        /// <returns>Returns true if the colliders AABBs overlap.</returns>
        public static bool BoxVsBox(Collider a, Collider b) {
            Vector2 p = (b.Position + b.BBCenter) - (a.Position + a.BBCenter);
            Vector2 s = b.BBRadius + a.BBRadius;
            p.X = Math.Abs(p.X);
            p.Y = Math.Abs(p.Y);

            return p.X < s.X && p.Y < s.Y;
        }
        /// <summary>
        /// Collision check for a pair of Rectangles.
        /// </summary>
        /// <param name="a">The first rectangle to check.</param>
        /// <param name="b">The second rectangle to check.</param>
        /// <returns>Returns true if the rectangles overlap.</returns>
        public static bool BoxVsBox(Rectangle a,  Rectangle b) {
            float x = Math.Abs(b.X - a.X);
            float y = Math.Abs(b.Y - a.Y);
            float hw = (a.Width + b.Width) / 2;
            float hh = (a.Height + b.Height) / 2;

            return x < hw && y < hh;
        }

        /// <summary>
        /// Collision detection for a ray vs a line segment.
        /// </summary>
        /// <param name="start">Starting point of the ray.</param>
        /// <param name="dir">Ray direction vector.</param>
        /// <param name="lineA">Line segment point A.</param>
        /// <param name="lineB">Line segment point B.</param>
        /// <returns>Returns true if the ray intersects the line segment.</returns>
        public static bool RayVsLine(Vector2 start, Vector2 dir, Vector2 lineA, Vector2 lineB) {
            float r, s, d;
            // Check for paralell (slope comparison)
            if ((dir.Y - start.Y) / (dir.X - start.X) != (lineB.Y - lineA.Y) / (lineB.X / lineA.X)) {
                d = Utils.Cross(dir - start, lineB - lineA);
                if (d != 0) {
                    r = Utils.Cross(start - lineA, lineB - lineA);
                    s = Utils.Cross(start - lineA, dir - lineB);
                    if (r >= 0 && s >= 0 && s <= 1) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Vector2? LineVsLine(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {

            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            // Parallell check
            if (denominator == 0) return null;

            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1)) return null;

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // The segments intersect if t1 and t2 are between 0 and 1.
            if ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1)) {
                return new Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            }

            return null;
        }

        /// <summary>
        /// Collision detection for point vs polygon.
        /// </summary>
        /// <param name="v">Point to test.</param>
        /// <param name="poly">Polygon to collide with.</param>
        /// <returns>Returns true if the point lies within the polygon.</returns>
        public static bool PointInPoly(Vector2 v, Vector2[] poly, Vector2 offset) {
            Utils.QueueDebugPoly(poly, offset, new Color(255, 255, 0));

            float xmin = poly[0].X + offset.X;
            float xmax = poly[0].X + offset.X;
            float ymin = poly[0].Y + offset.Y;
            float ymax = poly[0].Y + offset.Y;

            // Generate an AABB using the polygon.
            foreach (Vector2 pv in poly) {
                xmin = Math.Min(xmin, pv.X + offset.X);
                xmax = Math.Max(xmax, pv.X + offset.X);
                ymin = Math.Min(ymin, pv.Y + offset.Y);
                ymax = Math.Max(ymax, pv.Y + offset.Y);
            }

            // AABB early exit
            if (v.X < xmin || v.X > xmax || v.Y < ymin || v.Y > ymax) return false;

            bool odd = false;

            int i = 0;
            int j = poly.Length - 1;
            // Line crossing test. If an odd number of line segments were crossed, the ray started inside the polgyon.
            // Loop over each line segment and count the number of collisions vs the ray cast from point v.
            for (i = 0; i < poly.Length; i++) {
                float ix = poly[i].X + offset.X;
                float iy = poly[i].Y + offset.Y;
                float jx = poly[j].X + offset.X;
                float jy = poly[j].Y + offset.Y;
                //if (iy < v.Y && iy >= v.Y ||
                //    iy < v.Y && iy >= v.Y) {
                    // Poly crosses the ray, check if it crosses in front or behind
                    //if (ix + (v.Y - iy) / (jy - iy) * (jx - ix) < v.X) {
                    if ((iy > v.Y) != (jy > v.Y) && 
                        v.X < (jx - ix) * (v.Y - iy) / (jy - iy) + ix) {
                        odd = !odd;
                    }
                //}
                j = i;
            }

            return odd;
        }

        /// <summary>
        /// Collision detection for point vs polygon.
        /// </summary>
        /// <param name="v">Point to test.</param>
        /// <param name="poly">Polygon to collide with.</param>
        /// <returns>Returns true if the point lies within the polygon.</returns>
        public static bool PointInPoly(Vector2 v, Point[] poly, Vector2 offset) {
            float xmin = poly[0].X;
            float xmax = poly[0].X;
            float ymin = poly[0].Y;
            float ymax = poly[0].Y;

            // Generate an AABB using the polygon.
            foreach (Point pv in poly) {
                xmin = Math.Min(xmin, pv.X);
                xmax = Math.Max(xmin, pv.X);
                ymin = Math.Min(xmin, pv.Y);
                ymax = Math.Max(xmin, pv.Y);
            }

            // AABB early exit
            if (v.X < xmin || v.X > xmax || v.Y < ymin || v.Y > ymax) return false;

            bool odd = false;

            int i = 0;
            int j = poly.Length - 1;
            // Line crossing test. If an odd number of line segments were crossed, the ray started inside the polgyon.
            // Loop over each line segment and count the number of collisions vs the ray cast from point v.
            for (i = 0; i < poly.Length; i++) {
                if (poly[i].Y < v.Y && poly[j].Y >= v.Y ||
                    poly[j].Y < v.Y && poly[i].Y >= v.Y) {
                    // Poly crosses the ray, check if it crosses in front or behind
                    if (poly[i].X + (v.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) * (poly[j].X - poly[i].X) < v.X) {
                        odd = !odd;
                    }
                }
                j = i;
            }

            return odd;
        }

        /// <summary>
        /// Collision detection for point vs rectangle.
        /// </summary>
        /// <param name="v">The point to check.</param>
        /// <param name="r">The rectangle to collide against.</param>
        /// <returns>Returns true if the point lies within the rectangle.</returns>
        public static bool PointInRect(Vector2 v, Rectangle r) {
            return (v.X >= r.X && v.X < r.X + r.Width && v.Y >= r.Y && v.Y < r.Y + r.Height);
        }

        public static bool PointInCollider(Vector2 v, Collider c) {
            float xmin = c.Points[0].X;
            float xmax = c.Points[0].X;
            float ymin = c.Points[0].Y;
            float ymax = c.Points[0].Y;

            Vector2 rv = v - c.Position;

            //if (rv.X < c.BBCenter.X - c.BBRadius.X || rv.X > c.BBCenter.X + c.BBRadius.X || rv.Y < c.BBCenter.Y - c.BBRadius.Y | rv.Y > c.BBCenter.Y + c.BBRadius.Y) return false;

            bool odd = false;

            int i = 0;
            int j = c.Points.Length - 1;
            // Line crossing test. If an odd number of line segments were crossed, the ray started inside the polgyon.
            // Loop over each line segment and count the number of collisions vs the ray cast from point v.
            for (i = 0; i < c.Points.Length; i++) {
                if (c.Points[i].Y < rv.Y && c.Points[j].Y >= rv.Y ||
                    c.Points[j].Y < rv.Y && c.Points[i].Y >= rv.Y) {
                    // Poly crosses the ray, check if it crosses in front or behind
                    if (c.Points[i].X + (rv.Y - c.Points[i].Y) / (c.Points[j].Y - c.Points[i].Y) * (c.Points[j].X - c.Points[i].X) < rv.X) {
                        odd = !odd;
                    }
                }
                j = i;
            }

            return odd;
        }
    }

    /// <summary>
    /// A struct defining the result of an object projected along an axis.
    /// </summary>
    public struct ProjectionResult {
        public float Min;
        public float Max;
        public float Mid;
        public ProjectionResult(float min, float max) {
            Min = min;
            Max = max;
            Mid = min + (max - min) / 2f;
        }
    }

    /// <summary>
    /// A struct defining a separation axis for the purposes of SAT.
    /// </summary>
    public struct SATAxis {
        public Vector2 Vector;
        public float Gradient;

        /// <summary>
        /// Create a SAT separation axis.
        /// </summary>
        /// <param name="vector">Vector that defines the axis.</param>
        /// <param name="gradient">Gradient of the vector precomputed for duplicate detection.</param>
        public SATAxis(Vector2 vector, float? gradient = null) {
            Vector = vector;
            Gradient = gradient ?? (vector.X == 0 ? 0 : Math.Abs(vector.Y / vector.X));
        }

        public static SATAxis FromLine(Vector2 a, Vector2 b) {
            var line = b - a;
            var axis = line.PerLeft().Normalized();
            return new SATAxis(axis);
        }
    }

    public delegate void ColliderAction(object sender, Collider collider);

    public class Collider {
        public Vector2 Position;
        public Vector2 BBCenter;
        public Vector2 BBRadius;
        public Vector2[] Points;
        public SATAxis[] Axes;
        public ColliderType Type;
        public ColliderMode Mode;
        public string Metadata;
        public ColliderAction Action;

        public Collider(Rectangle rect) {
            Type = ColliderType.AABB;
            float hw = rect.Width / 2;
            float hh = rect.Height / 2;
            Position = new Vector2(rect.X + hw, rect.Y + hh);
            BBCenter = Position;
            BBRadius = new Vector2(hw, hh);

            Points = new Vector2[] {
                new Vector2(rect.X, rect.Y),
                new Vector2(rect.X + rect.Width, rect.Y),
                new Vector2(rect.X + rect.Width, rect.Y + rect.Height),
                new Vector2(rect.X, rect.Y + rect.Height)
            };
            Axes = new SATAxis[] {
                SATAxis.FromLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X + 1, rect.Y)),
                SATAxis.FromLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y - 1))
            };
        }

        /// <summary>
        /// Creates a Collider struct from a single point.
        /// Resulting Collider will have only a single point, with zero bounding radius and no separation axes.
        /// </summary>
        /// <param name="v"></param>
        public Collider(Vector2 v) {
            Type = ColliderType.Point;
            Points = new Vector2[] {v};
            Position = v;

            BBCenter = new Vector2(0, 0);
            BBRadius = new Vector2(0, 0);

            Points = new Vector2[] {v};
            Axes = new SATAxis[] {};
        }

        /// <summary>
        /// Creates a Collider struct from a polygon defined by an array of vectors.
        /// </summary>
        /// <param name="poly">The array of vectors defining the polygon.</param>
        public Collider(Vector2[] poly, Vector2 position, Vector2? offset = null) {
            // Polygon type collider setup
            Type = ColliderType.Poly;

            // Copy the polygon into the Points array
            Points = new Vector2[poly.Length];
            poly.CopyTo(Points, 0);

            // Setup the temprary axis list
            var ta = new List<SATAxis>();
            var gradients = new List<float>();

            // Initialise the bounding box values
            BBCenter = new Vector2();
            BBRadius = new Vector2();
            //Position = Points[0];

            Vector2 off = offset ?? new Vector2();
            
            for (int i = 0; i < Points.Length; i++) {
                Points[i] -= off;
            }
            position += off;
            Position = position;

            float mx = Points[0].X - position.X;
            float Mx = Points[0].X - position.X;
            float my = Points[0].Y - position.Y;
            float My = Points[0].Y - position.Y;

            for (int i = 0, j = 1; i < Points.Length; i++, j = (j + 1) % Points.Length) {
                // Set the position to the minimal X/Y coords, and the bounding box radius to the width/height
                //Position.X = Math.Min(Position.X, Points[i].X);
                //Position.Y = Math.Min(Position.Y, Points[i].Y);
                //BBRadius.X = Math.Max(BBRadius.X, Math.Abs(Position.X - Points[i].X));
                //BBRadius.Y = Math.Max(BBRadius.Y, Math.Abs(Position.Y - Points[i].Y));
                // Update the polygon bounding box.
                mx = Math.Min(mx, Points[i].X - position.X);
                Mx = Math.Max(Mx, Points[i].X - position.X);
                my = Math.Min(my, Points[i].Y - position.Y);
                My = Math.Max(My, Points[i].Y - position.Y);

                // Calculate the gradient of the line
                float gradient = (Points[i].X - Points[j].X) / (Points[i].Y - Points[j].Y);

                // Ignore this axis if we've already stored it by checking against our known gradients.
                if (gradients.Contains(gradient)) continue;
                gradients.Add(gradient);

                // Add unique axes to the list
                ta.Add(SATAxis.FromLine(Points[i], Points[j]));
            }

            // Calculate the bounding radius.
            BBRadius = new Vector2(Mx - mx, My - my) / 2f;

            // Set the bounding box center to the middle of the bounding box.
            BBCenter = new Vector2(mx, my) + BBRadius + position;

            // Store all the computed axes into the array.
            Axes = ta.ToArray();
        }
    }

    public enum ColliderType {
        Point,
        AABB,
        Poly
    }

    public enum ColliderMode {
        Collision,
        Secret,
        Zone,
        Death
    }
}
