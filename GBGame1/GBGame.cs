using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SeasonsGame : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Map level;

        public Point Camera;
        public Rectangle CameraBounds;

        public Season CurrentSeason;

        RenderTarget2D RTMain;
        RenderTarget2D RTWindow;
        Effect LightMap;

        Color ClearColor;
        public Texture2D Tileset;
        public Texture2D Sprites;
        public Texture2D ColorLut;
        public Texture2D GlowLut;

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

            level = new Map();
            level.Load("./Content/Maps/test.tmx");

            //foreach (Collider c in level.Colliders) {
            //    Utils.QueueDebugPoly(c.Points, c.Position, new Color(0, 255, 0), 10000);
            //}

            Player.Position = level.Meta["start_point"].Position - new Point(0, -8);

            CameraBounds = new Rectangle(level.MapBounds.X, level.MapBounds.Y, level.MapBounds.Width - Utils.GBW, level.MapBounds.Height - Utils.GBH);

            Tileset = Content.Load<Texture2D>("tiles");
            Sprites = Content.Load<Texture2D>("sprites");
            ColorLut = Content.Load<Texture2D>("color_lut");
            GlowLut = Content.Load<Texture2D>("glowmask");

            LightMap = Content.Load<Effect>("LightMap");
            LightMap.Parameters["ColorLUT"]?.SetValue(ColorLut);
            LightMap.Parameters["GlowLUT"]?.SetValue(GlowLut);

            RTMain = new RenderTarget2D(GraphicsDevice, Utils.GBW, Utils.GBH);
            RTWindow = new RenderTarget2D(GraphicsDevice, Utils.GBW, Utils.GBH);

            Player.SetTexture(Sprites);

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
        protected override void Update(GameTime gameTime) {

            Input.HandleInput(this, gameTime);

            Player.Update(gameTime, level);
            foreach (Particle p in WeatherParticles) {
                p.Update(gameTime);
                p.TruePosition = Utils.TrueMod(p.TruePosition, level.MapBounds);
                if (p.Despawn) WeatherParticles.Remove(p);
            }

            //if (Utils.DEBUG) Utils.QueueDebugRect(new Rectangle(Player.Position.X - 4, Player.Position.Y - 6, 8, 14));

            //int testX = (int)(Math.Cos(gameTime.TotalGameTime.TotalSeconds) * 16f);
            //int testY = (int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 16f);

            //Utils.QueueDebugRect(new Rectangle(-8 + testX, -8 + testY, 16, 16), new Vector2(40, 40), new Color(255, 0, 0));
            //Utils.QueueDebugRect(new Rectangle(-8 + testX, -8 + testY, 16, 16), new Vector2(64, 40), new Color(255, 0, 0));
            //Utils.QueueDebugRect(new Rectangle(-8 + testX, -8 + testY, 16, 16), new Vector2(88, 40), new Color(255, 0, 0));

            //Utils.QueueDebugPoly(new Vector2[] {
            //    new Vector2(-8 + testX, -8 + testY),
            //    new Vector2( 8 + testX, -8 + testY),
            //    new Vector2( 8 + testX,  8 + testY),
            //    new Vector2(-8 + testX,  8 + testY)
            //}, new Vector2(40, 40), new Color(0, 255, 0));

            //MapCollider mc = new MapCollider {
            //    ColliderType = MapColliderType.Rectangle,
            //    Width = 16,
            //    Height = 16,
            //    x = 0,
            //    y = 0,
            //    PolyPoints = new Point[] {
            //        new Point(-8 + testX, -8 + testY),
            //        new Point( 8 + testX, -8 + testY),
            //        new Point( 8 + testX,  8 + testY),
            //        new Point(-8 + testX,  8 + testY)
            //    }
            //};

            //Utils.QueueDebugPoly(mc.PolyPoints, new Vector2(64, 40), new Color(255, 255, 0));

            //Collider c = mc.ToCollider();

            //Utils.QueueDebugPoly(c.Points, new Vector2(88, 40), new Color(0, 255, 255));

            foreach (Particle p in Particles) {
                p.Update(gameTime);
                if (p.Despawn) Particles.Remove(p);
            }

            if (!level.MapBounds.Contains(Player.Position)) {
                Player.Reset(level.Meta["start_point"].Position);
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
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, cameraMatrix);

            level.Draw(spriteBatch, Tileset, 0, (int)CurrentSeason, 0, 0);
            level.Draw(spriteBatch, Tileset, 1, (int)CurrentSeason, 0, 0);

            DrawEntities(gameTime);

            level.Draw(spriteBatch, Tileset, 2, (int)CurrentSeason, 0, 0);

            spriteBatch.End();
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
        }

        private void DrawEntities(GameTime gameTime) {
            Player.Draw(spriteBatch);

            foreach (Particle p in WeatherParticles) {
                p.Draw(spriteBatch);
            }
        }

        public void SetSeason(Season season) {
            WeatherParticles.Clear();
            FlashAmount = 1f;
            CurrentSeason = season;
            var rnd = new Random((int)DateTime.Now.Ticks);
            switch (CurrentSeason) {
                case Season.Spring:
                    for (int i = 0; i < 18; i++) {
                        var butterfly = new ButterflyParticle(new Point(rnd.Next(level.MapBounds.Width), rnd.Next(level.MapBounds.Height)), level.MapBounds, rnd.Next(4));
                        butterfly.SetTexture(Sprites);
                        butterfly.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(butterfly);
                    }
                    break;
                case Season.Summer:
                    for (int i = 0; i < 8; i++) {
                        var bee = new BeeParticle(new Point(rnd.Next(level.MapBounds.Width), rnd.Next(level.MapBounds.Height)), level.MapBounds, rnd.Next(4));
                        bee.SetTexture(Sprites);
                        bee.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(bee);
                    }
                    break;
                case Season.Autumn:
                    for (int i = 0; i < 140; i++) {
                        var leaf = new LeafParticle(new Point(rnd.Next(level.MapBounds.Width), rnd.Next(level.MapBounds.Height)), rnd.Next(2), rnd.Next(8));
                        leaf.SetTexture(Sprites);
                        leaf.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(leaf);
                    }
                    break;
                case Season.Winter:
                    for (int i = 0; i < 320; i++) {
                        var snow = new SnowParticle(new Point(rnd.Next(level.MapBounds.Width), rnd.Next(level.MapBounds.Height)), rnd.Next(5), rnd.Next(8));
                        snow.SetTexture(Sprites);
                        snow.Flipped = rnd.Next(2) == 1;
                        WeatherParticles.Add(snow);
                    }
                    break;
            }
        }
    }
}
