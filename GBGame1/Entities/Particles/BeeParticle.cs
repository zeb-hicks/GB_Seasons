using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities.Particles {
    class BeeParticle : Particle {
        Random random;
        public Vector2 Target;
        Rectangle WorldBounds;

        public BeeParticle(Point position, Rectangle worldBounds, int startFrame = 0) {
            Velocity = new Vector2((float)(startFrame / 4.0 * Math.PI), 0.2f);
            TruePosition = position.ToVector2();
            Position = position;
            AddAnimation(new SpriteAnimation("bee", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(104, 8, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(112, 8, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(120, 8, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(112, 8, 8, 8), new Rectangle(-4, -4, 8, 8), 2)
            }), startFrame);
            random = new Random((int)DateTime.Now.Ticks);
            Target = TruePosition + new Vector2(1, 0);
            WorldBounds = worldBounds;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            Velocity = Target - TruePosition;
            var vl = Velocity.Length();
            if (vl > 2f) {
                Velocity /= vl / 2f;
                vl /= vl / 2f;
            }
            if (vl == 0) {
                Velocity.X += 2;
                vl = 2f;
            }
            TruePosition += Velocity;
            if (vl < 8 || vl > 40) {
                Target = TruePosition + Utils.RandomVector(16f) + new Vector2(Flipped ? -10f : 10f, 0);
            }
            Position = TruePosition.ToPoint();
        }
    }
}
