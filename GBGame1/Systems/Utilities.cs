using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {
    public static class Utils {
        public static int GBW = 160;
        public static int GBH = 144;
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        public static bool DEBUG = false;
        public static bool DEBUG_FLY = false;
        public static bool DEBUG_FBF = false;
        public static bool DEBUG_FBF_NEXT = false;

        public static Point PointWithinRect(Point p, Rectangle r) {
            return new Point(Math.Min(Math.Max(p.X, r.X), r.X + r.Width), Math.Min(Math.Max(p.Y, r.Y), r.Y + r.Height));
        }
        public static Vector2 PointWithinRect(Vector2 v, RectangleF r) {
            return new Vector2(Math.Min(Math.Max(v.X, r.X), r.X + r.Width), Math.Min(Math.Max(v.Y, r.Y), r.Y + r.Height));
        }

        public static int TrueMod(int i, int m) { int r = i % m; return r < 0 ? r + m : r; }
        public static float TrueMod(float i, float m) { float r = i % m; return r < 0 ? r + m : r; }
        public static double TrueMod(double i, double m) { double r = i % m; return r < 0 ? r + m : r; }
        public static Point TrueMod(Point v, Rectangle r) {
            return new Point(TrueMod(v.X - r.X, r.Width) + r.X, TrueMod(v.Y - r.Y, r.Height) + r.Y);
        }
        public static Vector2 TrueMod(Vector2 v, Rectangle r) {
            return new Vector2(TrueMod(v.X - r.X, r.Width) + r.X, TrueMod(v.Y - r.Y, r.Height) + r.Y);
        }

        /// <summary>
        /// Returns a random unit vector, or a vector of the specified length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Vector2 RandomVector(float length = 1f) {
            var v = new Vector2((float)random.NextDouble() * 2f - 1f, (float)random.NextDouble() * 2f - 1f);
            v.Normalize();
            return Vector2.Multiply(v, length);
        }

        public static Vector2 WithinRange(Vector2 v, float min, float max) {
            return new Vector2(Math.Max(Math.Min(v.X, max), min), Math.Max(Math.Min(v.Y, max), min));
        }

        /// <summary>
        /// Project vector V onto the line between points la and lb
        /// </summary>
        /// <param name="v">The vector to project</param>
        /// <param name="la">The start of the line segment</param>
        /// <param name="lb">The end of the line segment</param>
        /// <returns></returns>
        public static Vector2 Project(Point v, Point la, Point lb) {
            return Project(v.ToVector2(), (lb - la).ToVector2());
        }
        /// <summary>
        /// Project vector V onto the line between points la and lb
        /// </summary>
        /// <param name="v">The vector to project</param>
        /// <param name="la">The start of the line segment</param>
        /// <param name="lb">The end of the line segment</param>
        /// <returns></returns>
        public static Vector2 Project(Vector2 v, Vector2 la, Vector2 lb) {
            return Project(v, (lb - la));
        }
        /// <summary>
        /// Projects vector A onto vector B
        /// </summary>
        /// <param name="a">The vector to project</param>
        /// <param name="b">The basis vector for projection</param>
        /// <returns></returns>
        public static Vector2 Project(Vector2 a, Vector2 b) {
            Vector2 p = new Vector2();
            float dp = Dot(a, b);
            float bl = b.LengthSquared();

            p.X = (dp / bl) / b.X;
            p.Y = (dp / bl) / b.Y;

            return p;
        }

        public static float Projectf(Vector2 a, Vector2 b) {
            float dp = Dot(a, b);
            float bl = b.LengthSquared();
            return dp / bl;
        }

        public static float Lerpf(float val, float target, float delta) {
            delta = Math.Abs(delta);
            if (val > target) delta = -delta;
            if (Math.Abs(val-target) < delta) return target;
            return val + delta;
        }

        public static float Dot(Point a, Point b) {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Dot(Vector2 a, Point b) {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Dot(Point a, Vector2 b) {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Dot(Vector2 a, Vector2 b) {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(Point a, Point b) {
            return a.X * b.Y - a.Y * b.X;
        }
        public static float Cross(Vector2 a, Point b) {
            return a.X * b.Y - a.Y * b.X;
        }
        public static float Cross(Point a, Vector2 b) {
            return a.X * b.Y - a.Y * b.X;
        }
        public static float Cross(Vector2 a, Vector2 b) {
            return a.X * b.Y - a.Y * b.X;
        }

        #region Extension Methods

        public static RectangleF AtOffset(this RectangleF r, Vector2 p) {
            return new RectangleF(r.X + p.X, r.Y + p.Y, r.Width, r.Height);
        }

        public static RectangleF Flipped(this RectangleF r) {
            return new RectangleF(r.X + r.Width, r.Y, -r.Width, r.Height);
        }

        public static Rectangle AtOffset(this Rectangle r, Vector2 p) {
            return new Rectangle(r.X + (int)p.X, r.Y + (int)p.Y, r.Width, r.Height);
        }

        public static Rectangle Flipped(this Rectangle r) {
            return new Rectangle(r.X + r.Width, r.Y, -r.Width, r.Height);
        }

        public static Vector3 Normalized(this Vector3 v) {
            //if (v.X == 0 && v.Y == 0) return v;
            Vector3 n = v;
            n.Normalize();
            return n;
        }
        public static Vector2 Normalized(this Vector2 v) {
            //if (v.X == 0 && v.Y == 0) return v;
            Vector2 n = v;
            n.Normalize();
            return n;
        }

        public static Vector2 PerLeft(this Vector2 v) {
            return new Vector2(v.Y, -v.X);
        }
        public static Vector2 PerRight(this Vector2 v) {
            return new Vector2(-v.Y, v.X);
        }

        #endregion

        #region Debug Drawing Queue

        static List<DebugObject> DebugList = new List<DebugObject>();

        public static void QueueDebugRect(Rectangle r, Vector2? o = null, Color? c = null, int persistence = 1) {
            DebugList.Add(new DebugObject {
                Object = r,
                Offset = o ?? new Vector2(),
                Color = c ?? Color.White,
                Persistence = persistence
            });
        }

        public static void QueueDebugPoly(Vector2[] p, Vector2? o = null, Color? c = null, int persistence = 1) {
            DebugList.Add(new DebugObject {
                Object = p,
                Offset = o ?? new Vector2(),
                Color = c ?? Color.White,
                Persistence = persistence
            });
        }
        public static void QueueDebugPoly(Point[] p, Vector2? o = null, Color? c = null, int persistence = 1) {
            var poly = new Vector2[p.Length];
            for (int i = 0; i < p.Length; i++) {
                poly[i] = p[i].ToVector2();
            }
            DebugList.Add(new DebugObject {
                Object = poly,
                Offset = o ?? new Vector2(),
                Color = c ?? Color.White,
                Persistence = persistence
            });
        }

        public static void QueueDebugArrow(Vector2 p, Vector2 v, Color? c = null, float length = 12f, float size = 3f, int persistence = 1) {
            DebugList.Add(new DebugObject {
                Object = new DebugArrow {
                    Origin = p,
                    Vector = v.Normalized(),
                    Length = length,
                    Size = size
                },
                Offset = new Vector2(),
                Color = c ?? Color.White,
                Persistence = persistence
            });
        }

        public static void QueueDebugPoint(Vector2 p, float size, Color? c = null, int persistence = 1) {
            QueueDebugArrow(p + new Vector2(-size, 0), new Vector2(1, 0), c, size * 2, 0f, persistence);
            QueueDebugArrow(p + new Vector2(0, -size), new Vector2(0, 1), c, size * 2, 0f, persistence);
        }

        public static void QueueDraw(SpriteBatch spriteBatch) {
            for (int i = 0; i < DebugList.Count; i++) {
                if (DEBUG) {
                    object o = DebugList[i].Object;
                    Vector2 v = DebugList[i].Offset;
                    Color c = DebugList[i].Color;
                    if (o is Vector2[] p) {
                        for (int pi = 0; pi < p.Length; pi++) {
                            MonoDebug.Primitives2D.DrawLine(spriteBatch, p[pi] + v, p[(pi + 1) % p.Length] + v, c, 0.33f);
                        }
                    } else if (o is Rectangle r) {
                        MonoDebug.Primitives2D.DrawLine(spriteBatch, new Vector2(r.X, r.Y) + v, new Vector2(r.X + r.Width, r.Y) + v, c, 0.33f);
                        MonoDebug.Primitives2D.DrawLine(spriteBatch, new Vector2(r.X + r.Width, r.Y) + v, new Vector2(r.X + r.Width, r.Y + r.Height) + v, c, 0.33f);
                        MonoDebug.Primitives2D.DrawLine(spriteBatch, new Vector2(r.X + r.Width, r.Y + r.Height) + v, new Vector2(r.X, r.Y + r.Height) + v, c, 0.33f);
                        MonoDebug.Primitives2D.DrawLine(spriteBatch, new Vector2(r.X, r.Y + r.Height) + v, new Vector2(r.X, r.Y) + v, c, 0.33f);
                    } else if (o is DebugArrow a) {
                        Vector2 ht = a.Origin + a.Vector * a.Length;
                        MonoDebug.Primitives2D.DrawLine(spriteBatch, a.Origin, ht, c, 0.33f);

                        if (a.Size > 0f) {
                            Vector2 hl = a.Origin + a.Vector * (a.Length - a.Size) + new Vector2(a.Vector.Y, -a.Vector.X) * a.Size * 0.67f;
                            Vector2 hr = a.Origin + a.Vector * (a.Length - a.Size) + new Vector2(-a.Vector.Y, a.Vector.X) * a.Size * 0.67f;
                            MonoDebug.Primitives2D.DrawLine(spriteBatch, ht, hr, c, 0.33f);
                            MonoDebug.Primitives2D.DrawLine(spriteBatch, hr, hl, c, 0.33f);
                            MonoDebug.Primitives2D.DrawLine(spriteBatch, hl, ht, c, 0.33f);
                        }
                    }
                }
            }
        }

        public static void QueueFlush() {
            for (int i = 0; i < DebugList.Count; i++) {
                if (--DebugList[i].Persistence <= 0) {
                    DebugList.Remove(DebugList[i--]);
                }
            }
        }

        #endregion
    }

    public class DebugObject {
        public object Object;
        public Vector2 Offset;
        public Color Color;
        public int Persistence;
    }

    public struct DebugArrow {
        public Vector2 Origin;
        public Vector2 Vector;
        public float Length;
        public float Size;
    }

    public enum Season {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
}