using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Systems {

    public static class UIManager {
        public static Texture2D CharMap;
        public static Effect TextEffect;
        public static float ReadSpeed = 4f;

        public enum SpecialChars {
            FlameSprite = 0xE0,
            Sword = 0xE1,
            Shield = 0xE2,
            Coin = 0x110,
            Cursor = 0xF0
        }

        public static void Init(Texture2D charMap, Effect textEffect) {
            CharMap = charMap;
            TextEffect = textEffect;
        }
        public static void DrawString(SpriteBatch spriteBatch, GBString str, GameTime gameTime, bool Invert = false) {
            TextEffect.Parameters["Invert"]?.SetValue(Invert);
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointWrap, effect: TextEffect);

            char[] chars = str.Text.ToCharArray();

            int dx = 0;
            int dy = 0;

            int sx = 0;
            int sy = 0;

            int mx = str.Region.Width / 8;
            int my = str.Region.Height / 8;

            bool showCursor = false;

            for (int i = 0; i < chars.Length; i++) {
                int ci = chars[i] - 32;

                int ox = 0;
                int oy = 0;

                switch ((SpecialChars)chars[i]) {
                    case SpecialChars.Coin:
                        ci += (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds * 6) % 6);
                        break;
                    case SpecialChars.Cursor:
                        oy = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds * 2) % 2);
                        break;
                    case SpecialChars.FlameSprite:
                        oy = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds) % 2);
                        ox = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds + 0.5) % 2);
                        break;
                }

                sx = ci & 0xf;
                sy = ci >> 4;

                if (chars[i] == '\n') {
                    dx = 0;
                    dy++;
                } else {
                    spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx * 8, sy * 8, 8, 8), destinationRectangle: new Rectangle(str.Region.X + dx * 8 + ox, str.Region.Y + dy * 8 + oy, 8, 8), color: Color.White);
                    if (++dx + 1 > mx) {
                        if (dy + 1 < my) {
                            dx = 0;
                            dy++;
                        } else {
                            showCursor = true;
                            break;
                        }
                    }
                }
            }

            if (showCursor) {
                dx = mx - 1;
                dy = my;
                sx = 0;
                sy = 13;
                spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx * 8, sy * 8, 8, 8), destinationRectangle: new Rectangle(str.Region.X + dx * 8, str.Region.Y + dy * 8, 8, 8), color: Color.White);
            }

            spriteBatch.End();
        }

        public static void DrawWindow(SpriteBatch spriteBatch, GBWindow window) {
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointWrap);

            int wx = window.Region.X;
            int wy = window.Region.Y;
            int ww = window.Region.Width;
            int wh = window.Region.Height;

            int sx = window.Style.HasFlag(BorderStyle.Raised) ? 112 : 120;
            int sy = window.Style.HasFlag(BorderStyle.Rounded) ? 112 : 120;

            // Corners
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx    , sy    , 3, 3), destinationRectangle: new Rectangle(wx         , wy         , 3, 3), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 5, sy    , 3, 3), destinationRectangle: new Rectangle(wx + ww - 3, wy         , 3, 3), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx    , sy + 5, 3, 3), destinationRectangle: new Rectangle(wx         , wy + wh - 3, 3, 3), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 5, sy + 5, 3, 3), destinationRectangle: new Rectangle(wx + ww - 3, wy + wh - 3, 3, 3), color: Color.White);

            // Sides
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 3, sy    , 2, 3), destinationRectangle: new Rectangle(wx + 3     , wy         , ww - 6, 3), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 3, sy + 5, 2, 3), destinationRectangle: new Rectangle(wx + 3     , wy + wh - 3, ww - 6, 3), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx    , sy + 3, 3, 2), destinationRectangle: new Rectangle(wx         , wy + 3     , 3, wh - 6), color: Color.White);
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 5, sy + 3, 3, 2), destinationRectangle: new Rectangle(wx + ww - 3, wy + 3     , 3, wh - 6), color: Color.White);

            // Center
            spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(sx + 3, sy + 3, 2, 2), destinationRectangle: new Rectangle(wx + 3, wy + 3, ww - 6, wh - 6), color: Color.White);

            spriteBatch.End();
        }
    }

    public struct GBString {
        public string Text;
        public Rectangle Region;
        public ReadingMode ReadingMode;

        public GBString(string text, Rectangle region, ReadingMode readingMode = ReadingMode.Immediate) {
            Text = text;
            Region = region;
            ReadingMode = readingMode;
        }
    }

    public enum ReadingMode {
        Immediate,
        Progressive
    }

    public struct GBWindow {

        public Rectangle Region;
        public BorderStyle Style;
    }

    public enum BorderStyle {
        Solid = 0,
        Raised = 1,
        Rounded = 2
    }
}
