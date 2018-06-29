using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using GB_Seasons.Systems;

namespace GB_Seasons.Entities.Particles {
    class PlayerAttackParticle : Particle {
        Random random;

        public PlayerAttackParticle(Point position, bool flipped, int startFrame = 0) {
            Utils.QueueDebugPoint(position.ToVector2(), 10f, new Color(255, 0, 0), 50);
            random = new Random((int)DateTime.Now.Ticks);
            Velocity = new Vector2((float)(random.NextDouble()) * (flipped ? 1f : -1f), 0);
            TruePosition = position.ToVector2();
            Position = position;
            Flipped = flipped;
            AddAnimation(new SpriteAnimation("particle", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(0 , 72, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(8 , 72, 8, 8), new Rectangle(-4, -4, 8, 8), 2),
                new SpriteFrame(new Rectangle(16, 72, 8, 8), new Rectangle(-4, -4, 8, 8), 3),
                new SpriteFrame(new Rectangle(24, 72, 8, 8), new Rectangle(-4, -4, 8, 8), 4, despawn: true),
            }), startFrame);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            TruePosition += Velocity;
            Position = TruePosition.ToPoint();
        }
    }
}
