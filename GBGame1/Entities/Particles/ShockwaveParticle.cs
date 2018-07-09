using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities.Particles {
    class ShockwaveParticle : Particle {

        public ShockwaveParticle(Vector2 position, bool flipped) {
            //random = new Random((int)DateTime.Now.Ticks);
            TruePosition = position;
            Position = position;
            Flipped = flipped;
            AddAnimation(new SpriteAnimation("doublejump", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(32, 96, 16, 8), new Rectangle(-8, -4, 16, 8), 2),
                new SpriteFrame(new Rectangle(48, 96, 16, 8), new Rectangle(-8, -3, 16, 8), 3),
                new SpriteFrame(new Rectangle(64, 96, 16, 8), new Rectangle(-8, -2, 16, 8), 4),
                new SpriteFrame(new Rectangle(80, 96, 16, 8), new Rectangle(-8, -1, 16, 8), 5, despawn: true),
            }));
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            TruePosition += Velocity;
            Position = TruePosition;
        }
    }
}
