using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {
    public static class Input {

        public static Dictionary<InputAction, Tuple<Keys, Keys>> KeyboardMap = new Dictionary<InputAction, Tuple<Keys, Keys>>();
        public static Dictionary<InputAction, GamePadButtons> GamepadMap = new Dictionary<InputAction, GamePadButtons>();

        static bool hasJumped = false;

        public static void Initialize() {
            KeyboardMap.Add(InputAction.Left,  new Tuple<Keys, Keys>(Keys.A, Keys.Left ));
            KeyboardMap.Add(InputAction.Right, new Tuple<Keys, Keys>(Keys.D, Keys.Right));
            KeyboardMap.Add(InputAction.Up,    new Tuple<Keys, Keys>(Keys.W, Keys.Up   ));
            KeyboardMap.Add(InputAction.Down,  new Tuple<Keys, Keys>(Keys.S, Keys.Down ));

            KeyboardMap.Add(InputAction.A, new Tuple<Keys, Keys>(Keys.Z, Keys.OemPeriod  ));
            KeyboardMap.Add(InputAction.B, new Tuple<Keys, Keys>(Keys.X, Keys.OemQuestion));

            KeyboardMap.Add(InputAction.Start,  new Tuple<Keys, Keys>(Keys.Enter, Keys.None));
            KeyboardMap.Add(InputAction.Select, new Tuple<Keys, Keys>(Keys.Back,  Keys.None));
        }

        private static GamePadState PadStateLast;
        private static KeyboardState KeyStateLast;
        private static GamePadState PadState;
        private static KeyboardState KeyState;
        public static void HandleInput(SeasonsGame game, GameTime gameTime) {
            PadState = GamePad.GetState(PlayerIndex.One);
            KeyState = Keyboard.GetState();

            if (PadState.Buttons.Back == ButtonState.Pressed || KeyState.IsKeyDown(Keys.Escape))
                game.Exit();

            int walkVel = 0;

            if (KeyState.IsKeyDown(KeyboardMap[InputAction.Left].Item1) ||
                KeyState.IsKeyDown(KeyboardMap[InputAction.Left].Item2)) {
                if (Utils.DEBUG_FLY) game.Player.Position.X -= 1;
                walkVel -= 1;
                game.Player.Flipped = true;
            }
            if (KeyState.IsKeyDown(KeyboardMap[InputAction.Right].Item1) ||
                KeyState.IsKeyDown(KeyboardMap[InputAction.Right].Item2)) {
                if (Utils.DEBUG_FLY) game.Player.Position.X += 1;
                walkVel += 1;
                game.Player.Flipped = false;
            }
            if (KeyState.IsKeyDown(KeyboardMap[InputAction.Up].Item1) ||
                KeyState.IsKeyDown(KeyboardMap[InputAction.Up].Item2)) {
                if (Utils.DEBUG_FLY) game.Player.Position.Y -= 1;
            }
            if (KeyState.IsKeyDown(KeyboardMap[InputAction.Down].Item1) ||
                KeyState.IsKeyDown(KeyboardMap[InputAction.Down].Item2)) {
                if (Utils.DEBUG_FLY) game.Player.Position.Y += 1;
            }

            if (!Utils.DEBUG_FLY) {
                if (game.Player.Grounded) {
                    game.Player.Velocity = game.Player.GroundNormal.PerRight().Normalized() * walkVel;
                    if (walkVel != 0) {
                        game.Player.Velocity /= Math.Abs(game.Player.Velocity.X);
                    }
                } else {
                    game.Player.Velocity.X = walkVel;
                }
            }

            if (KeyState.IsKeyDown(KeyboardMap[InputAction.A].Item1) ||
                KeyState.IsKeyDown(KeyboardMap[InputAction.A].Item2)) {
                if (!hasJumped) {
                    hasJumped = true;
                    game.Player.Jump();
                }
            } else {
                game.Player.StopJump();
                hasJumped = false;
            }

            if (game.Player.Grounded) {
                game.Player.CurrentAnimation = walkVel == 0 ? "stand" : "walk";
            } else {
                game.Player.CurrentAnimation = game.Player.Velocity.Y < -2f ? "jump" : "fall";
            }

            if (KeyDown(Keys.NumPad0)) {
                game.SetSeason(Season.Spring);
            }
            if (KeyDown(Keys.NumPad1)) {
                game.SetSeason(Season.Summer);
            }
            if (KeyDown(Keys.NumPad2)) {
                game.SetSeason(Season.Autumn);
            }
            if (KeyDown(Keys.NumPad3)) {
                game.SetSeason(Season.Winter);
            }

            if (KeyDown(Keys.NumPad7)) {
                Utils.DEBUG = !Utils.DEBUG;
            }
            if (KeyDown(Keys.NumPad8)) {
                Utils.DEBUG_FLY = !Utils.DEBUG_FLY;
            }

            PadStateLast = PadState;
            KeyStateLast = KeyState;
        }

        private static bool KeyDown(Keys key) {
            return !KeyStateLast.IsKeyDown(key) && KeyState.IsKeyDown(key);
        }
    }

    public enum InputAction {
        Left,
        Right,
        Up,
        Down,
        A,
        B,
        Start,
        Select
    }
}
