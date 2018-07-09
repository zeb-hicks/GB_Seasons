using System;
using System.Globalization;
using System.ComponentModel;

namespace Microsoft.Xna.Framework {

    public struct RectangleF : IEquatable<RectangleF> {

        #region Public Fields

        public float X;
        public float Y;
        public float Width;
        public float Height;

        #endregion Public Fields


        #region Public Properties

        public static RectangleF Empty { get; } = new RectangleF();

        public float Left {
            get { return this.X; }
        }

        public float Right {
            get { return (this.X + this.Width); }
        }

        public float Top {
            get { return this.Y; }
        }

        public float Bottom {
            get { return (this.Y + this.Height); }
        }

        #endregion Public Properties


        #region Constructors

        public RectangleF(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        #endregion Constructors


        #region Public Methods

        public static RectangleF operator *(RectangleF r, Matrix m) {
            Vector2 p = Vector2.Transform(new Vector2(r.Center.X, r.Center.Y), m);
            return new RectangleF(p.X - r.Width / 2, p.Y - r.Height / 2, r.Width, r.Height);
        }

        public static bool operator ==(RectangleF a, RectangleF b) {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        public static bool operator !=(RectangleF a, RectangleF b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public bool Contains(float x, float y) {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(Vector2 value) {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(Point value) {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(RectangleF value) {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        public void Offset(Vector2 offset) {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset(float offsetX, float offsetY) {
            X += offsetX;
            Y += offsetY;
        }

        public Vector2 Center {
            get {
                return new Vector2((this.X + this.Width) / 2, (this.Y + this.Height) / 2);
            }
        }




        public void Inflate(float horizontalValue, float verticalValue) {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }

        public bool IsEmpty {
            get {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        public bool Equals(RectangleF other) {
            return this == other;
        }

        public static explicit operator Rectangle(RectangleF v) {
            return new Rectangle((int)Math.Round(v.X), (int)Math.Round(v.Y), (int)Math.Round(v.Width), (int)Math.Round(v.Height));
        }

        public override bool Equals(object obj) {
            return (obj is RectangleF) ? this == ((RectangleF)obj) : false;
        }

        public override string ToString() {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        public bool intersects(RectangleF r2) {
            return !(r2.Left > Right
                  || r2.Right < Left
                  || r2.Top > Bottom
                  || r2.Bottom < Top
                      );
        }


        public void intersects(ref RectangleF value, out bool result) {
            result = !(value.Left > Right
                    || value.Right < Left
                    || value.Top > Bottom
                    || value.Bottom < Top
                      );
        }

        #endregion Public Methods
    }
}