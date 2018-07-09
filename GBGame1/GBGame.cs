using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using GB_Seasons.Systems;
using GB_Seasons.ContentHandlers;
using GB_Seasons.Entities;
using GB_Seasons.Entities.Particles;

namespace GB_Seasons {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SeasonsGame : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Map level;

        public Vector2 Camera;
        public RectangleF CameraBounds;

        public Season CurrentSeason;

        RenderTarget2D RTMain;
        RenderTarget2D RTWindow;
        Effect LightMap;

        Color ClearColor;
        public Texture2D Tileset;
        public Texture2D Sprites;
        public Texture2D ColorLut;
        public Texture2D GlowLut;
        public Texture2D FadeLut;

        public Player Player { get; private set; }

        public List<Particle> WeatherParticles { get; private set; } = new List<Particle>();
        public List<Particle> Particles { get; private set; } = new List<Particle>();

        public float FlashAmount = 1f;

        int ZOOM = 3;
        int VW;
        int VH;

        int AW;
        int AH;

        public SeasonsGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            AW = GraphicsDevice.PresentationParameters.BackBufferWidth;
            AH = GraphicsDevice.PresentationParameters.BackBufferHeight;

            ZOOM = 3; // (int)Math.Floor((double)Math.Min(GBW / AW, GBH / AH));

            VW = Utils.GBW * ZOOM;
            VH = Utils.GBH * ZOOM;

            graphics.PreferredBackBufferWidth = VW;
            graphics.PreferredBackBufferHeight = VH;
            graphics.ApplyChanges();

            CurrentSeason = Season.Spring;

            Player = new Player();

            Input.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ClearColor = new Color(230, 214, 156);

            //foreach (Collider c in level.Colliders) {
            //    Utils.QueueDebugPoly(c.Points, c.Position, new Color(0, 255, 0), 10000);
            //}

            Tileset = Content.Load<Texture2D>("tiles");
            Sprites = Content.Load<Texture2D>("sprites");
            ColorLut = Content.Load<Texture2D>("color_lut");
            GlowLut = Content.Load<Texture2D>("glowmask");
            FadeLut = Content.Load<Texture2D>("dither_fade");

            LightMap = Content.Load<Effect>("LightMap");
            LightMap.Parameters["ColorLUT"]?.SetValue(ColorLut);
            LightMap.Parameters["GlowLUT"]?.SetValue(GlowLut);
            LightMap.Parameters["FadeLUT"]?.SetValue(FadeLut);

            LoadMap("test");

            if (level.Meta.ContainsKey("start_point")) {
                Player.Position = (Vector2)level.Meta["start_point"] - new Vector2(0, 8);
            } else {
                Console.WriteLine("No start point found in map metadata!");
            }

            Audio.LoadSFX(Content);

            RTMain = new RenderTarget2D(GraphicsDevice, Utils.GBW, Utils.GBH);
            RTWindow = new RenderTarget2D(GraphicsDevice, Utils.GBW, Utils.GBH);

            Player.SetTexture(Sprites);
            Player.SpawnParticle += PlayerSpawnParticle;

            UIManager.Init(
                Content.Load<Texture2D>("text"),
                Content.Load<Effect>("texteffect")
            );

            SetSeason(Season.Spring);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private KeyboardState pkb = new KeyboardState();
        protected override void Update(GameTime gameTime) {

            #region Debug input handling
            
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.LeftControl) && ks.IsKeyDown(Keys.R) && !pkb.IsKeyDown(Keys.R)) {
                LoadMap("test");
            }

            if (ks.IsKeyDown(Keys.Add) && !pkb.IsKeyDown(Keys.Add)) Utils.DEBUG_FBF = !Utils.DEBUG_FBF;
            if (ks.IsKeyDown(Keys.Subtract) && !pkb.IsKeyDown(Keys.Subtract)) Utils.DEBUG_FBF_NEXT = true;

            pkb = ks;

            if (Utils.DEBUG_FBF && !Utils.DEBUG_FBF_NEXT) return;
            Utils.DEBUG_FBF_NEXT = false;

            #endregion

            #region Debug queue
            Utils.QueueFlush();
            #endregion

            Input.HandleInput(this, gameTime);

            Player.Update(gameTime, level);

            Player.InSecret = false;

            foreach (MapVolume volume in level.Volumes) {
                if (Collision.PointInPoly(Player.Position, volume.Poly, volume.Position)) {
                    //if (volume.Mode == ColliderMode.Secret) {
                        Player.InSecret = true;
                    //}
                }
            }

            level.FadePos = Player.Position - Camera - new Vector2(Utils.GBW / 2, Utils.GBH / 2);

            float ft = Player.InSecret ? 1f : 0f;
            level.FadeAmount = Utils.Lerpf(level.FadeAmount, ft, (float)gameTime.ElapsedGameTime.TotalSeconds * 3f);

            foreach (Particle p in WeatherParticles) {
                Vector2 prevPos = p.TruePosition;

                p.Update(gameTime);

                //if (p is SnowParticle || p is LeafParticle) {
                //    p.TruePosition = Utils.TrueMod(p.TruePosition, new Rectangle(Camera.X, 0, Utils.GBW * 2, Utils.GBH * 2));
                //} else {
                //    p.TruePosition = Utils.TrueMod(p.TruePosition, level.MapBounds);
                //}

                foreach (MapVolume v in level.Volumes) {
                    if (v.Type == MapVolumeType.Weather) {
                        Utils.QueueDebugPoly(v.Poly, v.Position, new Color(0, 255, 255));

                        if (Collision.PointInPoly(p.TruePosition, v.Poly, v.Position)) {
                            Utils.QueueDebugPoint(p.TruePosition, 4f, new Color(0, 255, 0));
                        } else {
                            Utils.QueueDebugPoint(p.TruePosition, 4f, new Color(255, 0, 0));
                        }
                    }
                }
                if (p.Despawn) WeatherParticles.Remove(p);
            }

            for (int pi = 0; pi < Particles.Count; pi++) {
                Particles[pi].Update(gameTime);
                if (Particles[pi].Despawn) Particles.Remove(Particles[pi--]);
            }

            RectangleF mapbounds = (RectangleF)level.Meta["map_bounds"];
            if (!mapbounds.Contains(Player.Position)) {
                if (level.Meta.ContainsKey("start_point")) {
                    Player.Reset((Vector2)level.Meta["start_point"]);
                } else {
                    Console.WriteLine("No start position found in map metadata!");
                }
            }

            Camera.X = Player.Position.X - Utils.GBW / 2;
            Camera.Y = Player.Position.Y - Utils.GBH / 2 - 32;

            Camera = Utils.PointWithinRect(Camera, CameraBounds);

            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) {
                FlashAmount = 1f;
            }

            FlashAmount -= (float)gameTime.ElapsedGameTime.TotalSeconds * 2.0f;
            FlashAmount = Math.Max(0f, FlashAmount);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            //GraphicsDevice.Clear(ClearColor);

            // TODO: Add your drawing code here

            DrawLevel(gameTime);

            DrawUI(gameTime);

            CommitToScreen(gameTime);

            var cameraMatrix = Matrix.Identity;
            cameraMatrix.Translation = new Vector3(-Camera.X * 3f, -Camera.Y * 3f, 0f);
            cameraMatrix.Scale = new Vector3(3f, 3f, 3f);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, cameraMatrix);
            Utils.QueueDraw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CommitToScreen(GameTime gameTime) {
            GraphicsDevice.SetRenderTarget(null);

            LightMap.Parameters["ScreenFlash"]?.SetValue(FlashAmount);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, LightMap);
            spriteBatch.Draw(RTMain, new Rectangle(0, 0, VW, VH), Color.White);
            spriteBatch.End();
        }

        private void DrawLevel(GameTime gameTime) {

            var cameraMatrix = Matrix.Identity;
            cameraMatrix.Translation = new Vector3(-Camera.X, -Camera.Y, 0f);

            GraphicsDevice.SetRenderTarget(RTMain);
            GraphicsDevice.Clear(ClearColor);

            level.Draw(spriteBatch, Tileset, 0, (int)CurrentSeason, 0, 0, cameraMatrix);
            level.Draw(spriteBatch, Tileset, 1, (int)CurrentSeason, 0, 0, cameraMatrix);

            DrawEntities(gameTime, cameraMatrix);

            level.Draw(spriteBatch, Tileset, 2, (int)CurrentSeason, 0, 0, cameraMatrix);
        }

        private void DrawUI(GameTime gameTime) {
            GraphicsDevice.SetRenderTarget(RTMain);

            //UIManager.DrawWindow(spriteBatch, new GBWindow {
            //    Region = new Rectangle(0, 0, 160, 32),
            //    Style = BorderStyle.Rounded | BorderStyle.Raised
            //});

            //UIManager.DrawString(spriteBatch, new GBString {
            //    Region = new Rectangle(8, 8, 144, 16),
            //    Text = (char)UIManager.SpecialChars.Coin + "123" + "\n" + (char)UIManager.SpecialChars.FlameSprite,
            //    ReadingMode = ReadingMode.Progressive
            //}, gameTime);

            UIManager.DrawString(spriteBatch, new GBString {
                Region = new Rectangle(0, 0, 160, 8),
                Text = Player.CurrentAnimation + " - " + Player.State.ToString()
            }, gameTime, true);

            UIManager.DrawString(spriteBatch, new GBString {
                Region = new Rectangle(0, 8, 160, 8),
                Text = Player.Grounded ? "Grounded" : "Airborne"
            }, gameTime, true);
        }

        private void PlayerSpawnParticle(object sender, SpawnParticleEventArgs p) {
            Particles.Add(p.Particle);
        }

        private void DrawEntities(GameTime gameTime, Matrix matrix) {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);

            Player.Draw(spriteBatch);

            foreach (Particle p in Particles) {
                p.Draw(spriteBatch);
            }
            foreach (Particle p in WeatherParticles) {
                p.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        private void LoadMap(string MapName) {
            level = new Map();
            level.Load("./Content/Maps/" + MapName + ".tmx");

            RectangleF mapbounds = (RectangleF)level.Meta["map_bounds"];

            CameraBounds = new RectangleF(
                mapbounds.X,
                mapbounds.Y,
                mapbounds.Width - Utils.GBW,
                mapbounds.Height - Utils.GBH
            );

            level.TilemapEffect = Content.Load<Effect>("TilemapEffect");
            level.TilemapEffect.Parameters["FadeLut"]?.SetValue(FadeLut);
        }

        public void SetSeason(Season season) {
            WeatherParticles.Clear();
            FlashAmount = 1f;
            CurrentSeason = season;
            var rnd = new Random((int)DateTime.Now.Ticks);
            RectangleF mapbounds = (RectangleF)level.Meta["map_bounds"];
            switch (CurrentSeason) {
                case Season.Spring:
                    for (int i = 0; i < 18; i++) {
                        var butterfly = new ButterflyParticle(new Vector2((float)rnd.NextDouble() * mapbounds.Width, (float)rnd.NextDouble() * mapbounds.Height), mapbounds, rnd.Next(4));
                        butterfly.SetTexture(Sprites);
                        butterfly.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(butterfly);
                    }
                    break;
                case Season.Summer:
                    for (int i = 0; i < 8; i++) {
                        var bee = new BeeParticle(new Vector2((float)rnd.NextDouble() * mapbounds.Width, (float)rnd.NextDouble() * mapbounds.Height), mapbounds, rnd.Next(4));
                        bee.SetTexture(Sprites);
                        bee.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(bee);
                    }
                    break;
                case Season.Autumn:
                    for (int i = 0; i < 140; i++) {
                        var leaf = new LeafParticle(new Vector2((float)rnd.NextDouble() * mapbounds.Width, (float)rnd.NextDouble() * mapbounds.Height), rnd.Next(2), rnd.Next(8));
                        leaf.SetTexture(Sprites);
                        leaf.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(leaf);
                    }
                    break;
                case Season.Winter:
                    for (int i = 0; i < 320; i++) {
                        var snow = new SnowParticle(new Vector2((float)rnd.NextDouble() * mapbounds.Width, (float)rnd.NextDouble() * mapbounds.Height), rnd.Next(5), rnd.Next(8));
                        snow.SetTexture(Sprites);
                        snow.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(snow);
                    }
                    break;
            }
        }
    }
}
