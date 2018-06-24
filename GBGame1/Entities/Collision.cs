using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {
    public static class Collision {
        private static List<SATAxis> TempAxes = new List<SATAxis>();
        public static Vector2 Separate(Collider a, Collider b) {
            var sv = new Vector2();

            //Utils.QueueDebugArrow(a.Position, b.Position - a.Position, Color.Cyan);

            if (BoxVsBox(a, b)) {

                TempAxes.Clear();
                TempAxes.AddRange(a.Axes);
                TempAxes.AddRange(b.Axes);

                foreach (SATAxis axis in b.Axes) {
                    if (Utils.DEBUG) Utils.QueueDebugArrow(axis.Position, axis.Vector, Color.Red);

                    //if (Gradients.Contains(axis.Gradient)) continue;
                    //Gradients.Add(axis.Gradient);

                    if (Utils.DEBUG)
                        Utils.QueueDebugPoly(new Vector2[] {
                            b.Position + axis.Vector * 40f + axis.Vector.PerLeft() * 20f,
                            b.Position + axis.Vector * 40f + axis.Vector.PerRight() * 20f
                        }, Color.White);
                }
                foreach (SATAxis axis in a.Axes) {
                    if (Utils.DEBUG) Utils.QueueDebugArrow(axis.Position, axis.Vector, Color.Red);

                    //if (Gradients.Contains(axis.Gradient)) continue;
                    //Gradients.Add(axis.Gradient);

                    if (Utils.DEBUG)
                        Utils.QueueDebugPoly(new Vector2[] {
                            a.Position + axis.Vector * 40f + axis.Vector.PerLeft() * 20f,
                            a.Position + axis.Vector * 40f + axis.Vector.PerRight() * 20f
                        }, Color.White);
                }

            }

            return sv;
        }
        public static Vector2 Separate(Vector2 v, Rectangle r) {
            var sv = new Vector2();

            var le = v.X - r.X;
            var re = r.X + r.Width - v.X;
            var te = v.Y - r.Y;
            var be = r.Y + r.Height - v.Y;
            if (le > 0 && re > 0 && te > 0 && be > 0) {
                if (le <= re && le <= te && le <= be) {
                    sv.X = -le;
                }
                if (re <= le && re <= te && re <= be) {
                    sv.X = re;
                }
                if (te <= le && te <= re && te <= be) {
                    sv.Y = -te;
                }
                if (be <= le && be <= re && be <= te) {
                    sv.Y = be;
                }
            }
            return sv;
        }

        public static Vector2 Separate(Rectangle c, Rectangle r) {
            return Separate(new Vector2[] {
                new Vector2(c.X, c.Y),
                new Vector2(c.X, c.Y),
                new Vector2(c.X, c.Y),
                new Vector2(c.X, c.Y),
            }, new Vector2[] {
                new Vector2(r.X, r.Y),
                new Vector2(r.X, r.Y),
                new Vector2(r.X, r.Y),
                new Vector2(r.X, r.Y),
            });
        }
        public static Vector2 Separate(Vector2 v, Point[] poly) {
            var sv = new Vector2();



            return sv;
        }

        /// <summary>
        /// Separate two polygons
        /// </summary>
        /// <param name="a">Polygon to separate</param>
        /// <param name="b">Polygon from which to separate</param>
        /// <returns></returns>
        public static Vector2 Separate(Vector2[] a, Vector2[] b) {
            var sv = new Vector2();



            return sv;
        }

        public static ProjectionResult Project(Collider collider, Vector2 axis) {
            float min = 0f, max = 0f;

            Vector2 axisn = axis.Normalized();

            foreach (Vector2 p in collider.Points) {
                float dp = Vector2.Dot(axis, p);

            }

            return new ProjectionResult(min, max);
        }

        public static bool BoxVsBox(Collider a, Collider b) {
            float x = Math.Abs(b.Position.X - a.Position.X);
            float y = Math.Abs(b.Position.Y - a.Position.Y);

            return x < a.BoundingRadius.X + b.BoundingRadius.X && y < a.BoundingRadius.Y + b.BoundingRadius.Y;
        }
        public static bool BoxVsBox(Rectangle a,  Rectangle b) {
            float x = Math.Abs(b.X - a.X);
            float y = Math.Abs(b.Y - a.Y);
            float hw = (a.Width + b.Width) / 2;
            float hh = (a.Height + b.Height) / 2;

            return x < hw && y < hh;
        }
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

        public static bool PointInPoly(Vector2 v, Vector2[] poly) {
            float xmin = poly[0].X;
            float xmax = poly[0].X;
            float ymin = poly[0].Y;
            float ymax = poly[0].Y;
            foreach (Vector2 pv in poly) {
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
        public static bool PointInPoly(Vector2 v, Point[] poly) {
            float xmin = poly[0].X;
            float xmax = poly[0].X;
            float ymin = poly[0].Y;
            float ymax = poly[0].Y;
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

        public static bool PointInRect(Vector2 v, Rectangle r) {
            return (v.X >= r.X && v.X < r.X + r.Width && v.Y >= r.Y && v.Y < r.Y + r.Height);
        }
    }

    public struct ProjectionResult {
        public float Min;
        public float Max;
        public ProjectionResult(float min, float max) {
            Min = min;
            Max = max;
        }
    }

    public struct SATAxis {
        public Vector2 Position;
        public Vector2 Vector;
        public float Gradient;
        public float Magnitude;

        public SATAxis(Vector2 position, Vector2 vector, float magnitude, float? gradient = null) {
            Position = position;
            Vector = vector;
            Magnitude = magnitude;
            Gradient = gradient ?? Math.Abs(vector.Y / vector.X);
        }

        public static SATAxis FromLine(Vector2 a, Vector2 b) {
            var line = b - a;
            var axis = new Vector2(-line.Y, line.X);
            float mag = axis.Length();
            return new SATAxis(a, axis / mag, mag);
        }
    }

    public struct Collider {
        public Vector2 Position;
        public Vector2 BoundingRadius;
        public Vector2[] Points;
        public SATAxis[] Axes;
        public ColliderType Type;

        public Collider(Rectangle rect) {
            Type = ColliderType.AABB;
            float hw = rect.Width / 2;
            float hh = rect.Height / 2;
            Position = new Vector2(rect.X + hw, rect.Y + hh);
            BoundingRadius = new Vector2(hw, hh);

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

            BoundingRadius = new Vector2(0, 0);

            Points = new Vector2[] {v};
            Axes = new SATAxis[] {};
        }

        /// <summary>
        /// Creates a Collider struct from a polygon defined by an array of vectors.
        /// </summary>
        /// <param name="poly">The array of vectors defining the polygon.</param>
        public Collider(Vector2[] poly) {
            // Polygon type collider setup
            Type = ColliderType.Poly;

            // Copy the polygon into the Points array
            Points = new Vector2[poly.Length];
            poly.CopyTo(Points, 0);

            // Setup the temprary axis list
            var ta = new List<SATAxis>();
            var gradients = new List<float>();

            // Initialise the bounding box values
            BoundingRadius = new Vector2();
            Position = Points[0];

            for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++) {
                // Set the position to the minimal X/Y coords, and the bounding box radius to the width/height
                Position.X = Math.Min(Position.X, Points[i].X);
                Position.Y = Math.Min(Position.Y, Points[i].Y);
                BoundingRadius.X = Math.Max(BoundingRadius.X, Points[i].X);
                BoundingRadius.Y = Math.Max(BoundingRadius.Y, Points[i].Y);

                // Calculate the gradient of the line
                float gradient = (Points[i].X - Points[j].X) / (Points[i].Y - Points[j].Y);

                // Ignore this axis if we've already stored it by checking against our known gradients.
                if (gradients.Contains(gradient) || gradients.Contains(-gradient)) continue;
                gradients.Add(gradient);

                // Add unique axes to the list
                ta.Add(SATAxis.FromLine(Points[i], Points[j]));
            }

            // Bounding radius is now the maximal X/Y coordinates of the AABB. Convert to the half width/height ("bounding radius").
            BoundingRadius = (BoundingRadius - Position) / 2f;

            // Position is now the minimal X/Y coordinates of the AABB. Convert to the center point by offsetting by BoundingRadius.
            Position += BoundingRadius;

            // Store all the computed axes into the array.
            Axes = ta.ToArray();
        }
    }

    public enum ColliderType {
        Point,
        AABB,
        Poly
    }
}
