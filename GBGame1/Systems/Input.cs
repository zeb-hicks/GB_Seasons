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

        public static void Initialize() {
            KeyboardMap.Add(InputAction.Left,  new Tuple<Keys, Keys>(Keys.A, Keys.Left ));
            KeyboardMap.Add(InputAction.Right, new Tuple<Keys, Keys>(Keys.D, Keys.Right));
            KeyboardMap.Add(InputAction.Up,    new Tuple<Keys, Keys>(Keys.W, Keys.Up   ));
            KeyboardMap.Add(InputAction.Down,  new Tuple<Keys, Keys>(Keys.S, Keys.Down ));

            KeyboardMap.Add(InputAction.A, new Tuple<Keys, Keys>(Keys.Z, Keys.Space    ));
            KeyboardMap.Add(InputAction.B, new Tuple<Keys, Keys>(Keys.X, Keys.LeftShift));

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

            foreach (InputAction a in Enum.GetValues(typeof(InputAction))) {
                bool down = KeyState.IsKeyDown(KeyboardMap[a].Item1) || KeyState.IsKeyDown(KeyboardMap[a].Item2);
                game.Player.HandleInput(a, down);
            }

            // Debug stuff.

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

        public static bool InputDown(InputAction a) {
            Keys k1 = KeyboardMap[a].Item1;
            Keys k2 = KeyboardMap[a].Item2;
            return KeyState.IsKeyDown(k1) || KeyState.IsKeyDown(k2);
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
