using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons {
    public class Player : SpriteEntity {
        public Collider Collider;
        public Vector2 Velocity;
        public bool Grounded;
        float CoyoteTime;
        public Vector2 GroundNormal = new Vector2(0, -1);

        public float JumpVelocity = 3.5f;
        public float CoyoteTimeDuration = 0.1f;
        public float PhysicsMaxStep = 3f;

        public event SpawnParticleEventHandler OnSpawnParticle;

        public Player() {
            Collider = new Collider(new Vector2[] {
                new Vector2(-4, -6),
                new Vector2( 4, -6),
                new Vector2( 4,  8),
                new Vector2(-4,  8)
            }, new Vector2(0, 0), new Vector2(0, 0));
            AddAnimation(new SpriteAnimation("walk", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(0,  0, 8, 16), new Rectangle(-4, -8, 8, 16), 8),
                new SpriteFrame(new Rectangle(8,  0, 8, 16), new Rectangle(-4, -8, 8, 16), 8),
                new SpriteFrame(new Rectangle(16, 0, 8, 16), new Rectangle(-4, -8, 8, 16), 8),
                new SpriteFrame(new Rectangle(24, 0, 8, 16), new Rectangle(-4, -8, 8, 16), 8),
            }));
            AddAnimation(new SpriteAnimation("stand", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(0,  0, 8, 16), new Rectangle(-4, -8, 8, 16)),
            }));
            AddAnimation(new SpriteAnimation("jump", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));
            AddAnimation(new SpriteAnimation("fall", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(32, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));
            AddAnimation(new SpriteAnimation("dash", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(8,  0,  8,  16), new Rectangle(-4, -8, 8,  16), 2),
                new SpriteFrame(new Rectangle(64, 0,  16, 16), new Rectangle(-8, -8, 16, 16), 2, frameEvent: () => { Position.X += 8; }),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 6, frameEvent: () => { Position.X += 4; }),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 6, frameEvent: () => { Position.X += 2; }),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 6, frameEvent: () => { Position.X += 1; }),
                new SpriteFrame(new Rectangle(16, 16, 16, 16), new Rectangle(-8, -8, 16, 16), 4),
            }));
            AddAnimation(new SpriteAnimation("roll", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));
            AddAnimation(new SpriteAnimation("attack", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));
        }

        public override void Update(GameTime gameTime, Map level) {
            // Apply gravity
            Velocity.Y += 0.3f;
            if (Utils.DEBUG_FLY) Velocity = new Vector2(0, 0);

            Grounded = false;

            float motion = Velocity.Length();
            Vector2 pos = Position.ToVector2();
            while (motion > PhysicsMaxStep) {
                motion -= PhysicsMaxStep;
                pos = ProcessCollision(level, pos, PhysicsMaxStep);
            }
            pos = ProcessCollision(level, pos, motion);
            if (!Utils.DEBUG_FLY) {
                Position = pos.ToPoint();
            }

            if (Grounded) {
                CoyoteTime = CoyoteTimeDuration;
            } else {
                CoyoteTime = (float)Math.Max(-1f, CoyoteTime - gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }

        private Vector2 ProcessCollision(Map level, Vector2 pos, float motion) {
            if (motion > 0) {
                pos += Velocity.Normalized() * motion;
            }
            Collider.Position = pos;

            foreach (Collider collider in level.Colliders) {
                Vector2 sv = new Vector2();
                sv = Collision.Separate(Collider, collider);
                //Utils.QueueDebugPoly(collider.Points, collider.Position, sv.Length() > 0 ? Color.White : Color.Black);

                if (sv.Y < 0 && Velocity.Y > 0) {
                    Grounded = true;
                    Velocity.Y = 1f;
                }

                if (sv.LengthSquared() > 0f) {
                    GroundNormal = sv.Normalized();
                //} else {
                    //GroundNormal = new Vector2(0, -1);
                }

                if (sv.Y < 0 && Math.Abs(sv.X * 0.5) < Math.Abs(sv.Y)) {
                    sv = new Vector2(0, -1) * sv.Length();
                }

                pos += sv;
                Collider.Position = pos;
            }

            //Utils.QueueDebugPoly(Collider.Points, Collider.Position, Color.White);

            //Utils.QueueDebugRect(new Rectangle(location: pos.ToPoint() - new Point(4, 14), size: new Point(8, 14)));

            return pos;
        }

        public void Reset(Point pos) {
            if (Utils.DEBUG_FLY) return;
            Position = pos;
            Velocity = new Vector2(0, 1);
            Grounded = true;
        }

        public void Jump() {
            if (Grounded || CoyoteTime > 0f) {
                Velocity.Y = -JumpVelocity;
                Grounded = false;
                CoyoteTime = -1f;
            }
        }

        public void StopJump() {
            if (Velocity.Y < 0f) Velocity.Y = 0f;
        }

        public void Attack() {

        }

        public void Roll() {

        }

        public void Dash() {

        }

        public void Interact() {

        }
    }

    public delegate void SpawnParticleEventHandler(object sender, SpawnParticleEventArgs e);

    public class SpawnParticleEventArgs : EventArgs {
        Type ParticleType;
        public SpawnParticleEventArgs(Type type) {
            ParticleType = type;
        }
    }
}
