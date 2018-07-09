using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities.Particles {
    class ButterflyParticle : Particle {
        readonly Random random;
        public Vector2 Target;
        RectangleF WorldBounds;

        public ButterflyParticle(Vector2 position, RectangleF worldBounds, int startFrame = 0) {
            Velocity = new Vector2((float)(startFrame / 4.0 * Math.PI), 0.2f);
            TruePosition = position;
            Position = position;
            AddAnimation(new SpriteAnimation("butterfly", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(88 , 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(96 , 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(104, 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(112, 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(120, 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(112, 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(104, 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
                new SpriteFrame(new Rectangle(96 , 0, 8, 8), new Rectangle(-4, -4, 8, 8), 8),
            }), startFrame);
            random = new Random((int)DateTime.Now.Ticks);
            Target = TruePosition + new Vector2(1, 0);
            WorldBounds = worldBounds;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            Velocity = Target - TruePosition;
            var vl = Velocity.Length();
            if (vl > 0.75f) {
                Velocity /= vl / 0.75f;
                vl /= vl / 0.75f;
            }
            if (vl == 0) {
                Velocity.X += 0.75f;
                vl = 0.75f;
            }
            TruePosition += Velocity;
            if (vl < 8 || vl > 40) {
                Target = TruePosition + Utils.RandomVector(16f) + new Vector2(Flipped ? -16f : 16f, 0);
            }
            Position = TruePosition;
        }
    }
}
