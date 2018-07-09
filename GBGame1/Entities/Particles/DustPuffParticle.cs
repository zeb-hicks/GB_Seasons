using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities.Particles {
    class DustPuffParticle : Particle {
        Random random;

        public DustPuffParticle(Vector2 position, bool flipped) {
            random = new Random((int)DateTime.Now.Ticks);
            Velocity = new Vector2((float)(random.NextDouble() * 0.3 + 0.3) * (flipped ? 1f : -1f), 0);
            TruePosition = position;
            Position = position;
            Flipped = flipped;
            AddAnimation(new SpriteAnimation("dustpuff", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(80,  40, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(88,  40, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(96,  40, 8, 8), new Rectangle(-4, -4, 8, 8), 3),
                new SpriteFrame(new Rectangle(104, 40, 8, 8), new Rectangle(-4, -4, 8, 8), 4),
                new SpriteFrame(new Rectangle(112, 40, 8, 8), new Rectangle(-4, -4, 8, 8), 5, despawn: true),
            }));
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            TruePosition += Velocity;
            Position = TruePosition;
        }
    }
}
