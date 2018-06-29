using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities.Particles {
    class LeafParticle : Particle {
        public LeafParticle(Point position, int leafStyle = 0, int startFrame = 0) {
            Velocity = new Vector2((float)(startFrame / 4.0 * Math.PI), 0.2f);
            TruePosition = position.ToVector2();
            Position = position;
            int ly = leafStyle * 8;
            AddAnimation(new SpriteAnimation("leaf", new List<SpriteFrame>() {
                new SpriteFrame(new Rectangle(88,  16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(96,  16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(104, 16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(112, 16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(120, 16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(112, 16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(104, 16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10),
                new SpriteFrame(new Rectangle(96,  16 + ly, 8, 8), new Rectangle(-4, -4, 8, 8), 10)
            }), startFrame);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            Velocity.X = (float)Math.Sin(Animations[CurrentAnimation].CurrentFrame / 4.0 * Math.PI) * (Flipped ? 1 : -1) * 0.33f;
            TruePosition += Velocity;
            Position = TruePosition.ToPoint();
        }
    }
}
