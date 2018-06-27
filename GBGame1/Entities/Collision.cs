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
            float sm = float.MaxValue;

            if (Utils.DEBUG) {
                //Vector2 ac = a.Position + a.BBCenter;
                //Vector2 bc = b.Position + b.BBCenter;
                //float l = (bc - ac).Length();
                //if (l < 50) Utils.QueueDebugArrow(ac, bc - ac, Color.Cyan, l);

                //Utils.QueueDebugPoint(a.Position, 12f, Color.Orange, 1);

                //Utils.QueueDebugPoint(b.Position, 12f, Color.Orange, 1);

                //Utils.QueueDebugPoint(a.Position + a.BBCenter, 12f, Color.Orange, 1);

                //Utils.QueueDebugPoint(b.Position + b.BBCenter, 12f, Color.Orange, 1);

                //Utils.QueueDebugRect(new Rectangle((a.BBCenter - a.BBRadius).ToPoint(), (a.BBRadius * 2f).ToPoint()), a.Position, new Color(0, 255, 255));
                //Utils.QueueDebugRect(new Rectangle((b.BBCenter - b.BBRadius).ToPoint(), (b.BBRadius * 2f).ToPoint()), b.Position, new Color(0, 255, 255));
            }

            if (BoxVsBox(a, b)) {

                TempAxes.Clear();
                foreach (SATAxis ba in b.Axes) {
                    TempAxes.Add(new SATAxis {
                        Position = ba.Position + a.Position,
                        Vector = ba.Vector,
                        Magnitude = ba.Magnitude,
                        Gradient = ba.Gradient
                    });
                }
                foreach (SATAxis aa in a.Axes) {
                    bool exists = false;
                    foreach (SATAxis ba in b.Axes) {
                        if (!exists && Vector2.Dot(ba.Vector, aa.Vector) == 0f) {
                            exists = true;
                        }
                    }
                    SATAxis ax = new SATAxis {
                        Position = aa.Position + b.Position,
                        Vector = aa.Vector,
                        Magnitude = aa.Magnitude,
                        Gradient = aa.Gradient
                    };
                    if (!exists) TempAxes.Add(ax);
                }

                foreach (SATAxis axis in TempAxes) {
                    ProjectionResult resulta = Project(a, axis);
                    ProjectionResult resultb = Project(b, axis);
                    //if (Utils.DEBUG) Utils.QueueDebugArrow(axis.Position, axis.Vector, Color.Red);

                    if (Utils.DEBUG) {
                        // Draw projection basis
                        Utils.QueueDebugPoly(new Vector2[] {
                            axis.Vector.PerRight() * 30f + axis.Vector * 15f,
                            axis.Vector.PerRight() * 30f + axis.Vector * -15f
                        }, a.Position, Color.White);

                        // Draw projection results
                        Utils.QueueDebugPoly(new Vector2[] {
                            axis.Vector.PerRight() * 29f + axis.Vector * resulta.Min,
                            axis.Vector.PerRight() * 29f + axis.Vector * resulta.Max
                        }, a.Position, new Color(255, 0, 0));

                        // Draw projection results
                        Utils.QueueDebugPoly(new Vector2[] {
                            axis.Vector.PerRight() * 28f + axis.Vector * resultb.Min,
                            axis.Vector.PerRight() * 28f + axis.Vector * resultb.Max
                        }, a.Position, new Color(0, 255, 0));
                    }

                    float mag = 0f;

                    if (resulta.Mid > resultb.Mid) {
                        mag = resultb.Max - resulta.Min;
                        Utils.QueueDebugArrow(a.Position + axis.Vector.PerRight() * 28f + axis.Vector * resultb.Max, axis.Vector, new Color(0, 0, 255), mag);

                        if (mag <= 0f) {
                            return new Vector2();
                        }
                    } else {
                        mag = resultb.Min - resulta.Max;
                        Utils.QueueDebugArrow(a.Position + axis.Vector.PerRight() * 28f + axis.Vector * resultb.Min, axis.Vector, new Color(0, 0, 255), mag);

                        if (mag >= 0f) {
                            return new Vector2();
                        }
                    }

                    if (Math.Abs(mag) < Math.Abs(sm)) {
                        sv = axis.Vector * mag;
                        sm = mag;
                    }
                }

            }

            if (sm > 0.1f && sm < float.MaxValue) {
                Utils.QueueDebugArrow(a.Position, sv, Color.Red, sm);
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

        public static ProjectionResult Project(Collider collider, SATAxis axis) {
            float min = float.MaxValue, max = float.MinValue;

            Vector2 axisn = axis.Vector.Normalized();

            foreach (Vector2 p in collider.Points) {
                float dp = Vector2.Dot(axisn, p + collider.Position - axis.Position);
                min = Math.Min(dp, min);
                max = Math.Max(dp, max);
            }

            return new ProjectionResult(min, max);
        }

        public static bool BoxVsBox(Collider a, Collider b) {
            Vector2 p = (b.Position + b.BBCenter) - (a.Position + a.BBCenter);
            Vector2 s = b.BBRadius + a.BBRadius;
            p.X = Math.Abs(p.X);
            p.Y = Math.Abs(p.Y);

            return p.X < s.X && p.Y < s.Y;
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
        public float Mid;
        public ProjectionResult(float min, float max) {
            Min = min;
            Max = max;
            Mid = min + (max - min) / 2f;
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
            Gradient = gradient ?? (vector.X == 0 ? 0 : Math.Abs(vector.Y / vector.X));
        }

        public static SATAxis FromLine(Vector2 a, Vector2 b) {
            var line = b - a;
            var axis = line.PerLeft();
            float mag = axis.Length();
            return new SATAxis(a + (b - a) / 2f, axis / mag, mag);
        }
    }

    public class Collider {
        public Vector2 Position;
        public Vector2 BBCenter;
        public Vector2 BBRadius;
        public Vector2[] Points;
        public SATAxis[] Axes;
        public ColliderType Type;

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
                if (gradients.Contains(gradient) || gradients.Contains(-gradient)) continue;
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
}
