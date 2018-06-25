using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {

    public static class UIManager {
        public static Texture2D CharMap;

        public enum SpecialChars {
            FlameSprite = 0xE0,
            FlameSpriteLight = 0xE8,
            Sword = 0xE1,
            SwordLight = 0xE9,
            Shield = 0xE2,
            ShieldLight = 0xEA,
            Coin = 0x110,
            CoinLight = 0x118,
            Cursor = 0xF0,
            CursorLight = 0xF8
        }

        public static void Init(Texture2D charMap) {
            CharMap = charMap;
        }
        public static void DrawString(SpriteBatch spriteBatch, GBString str, GameTime gameTime) {
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointWrap);

            char[] chars = str.Text.ToCharArray();

            int sx = 0;
            int sy = 0;

            for (int i = 0; i < chars.Length; i++) {
                int ci = chars[i] - 32;

                int ox = 0;
                int oy = 0;

                switch ((SpecialChars)chars[i]) {
                    case SpecialChars.Coin:
                    case SpecialChars.CoinLight:
                        ci += (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds * 6) % 6);
                        break;
                    case SpecialChars.Cursor:
                    case SpecialChars.CursorLight:
                        oy = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds * 2) % 2);
                        break;
                    case SpecialChars.FlameSprite:
                    case SpecialChars.FlameSpriteLight:
                        oy = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds) % 2);
                        ox = (int)Math.Floor((gameTime.TotalGameTime.TotalSeconds + 0.5) % 2);
                        break;
                }
                if (chars[i] == (int)SpecialChars.Coin) {
                    
                }

                int cx = ci & 0xf;
                int cy = ci >> 4;

                spriteBatch.Draw(CharMap, sourceRectangle: new Rectangle(cx * 8, cy * 8, 8, 8), destinationRectangle: new Rectangle(str.Region.X + sx + ox, str.Region.Y + sy + oy, 8, 8), color: Color.White);

                if ((sx += 8) + 8 > str.Region.Width) {
                    sx = 0;
                    sy += 8;
                }
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
