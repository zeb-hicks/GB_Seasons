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
        public bool HasAerialControl = true;
        private float coyoteTime;
        public Vector2 GroundNormal = new Vector2(0, -1);

        public PlayerState State = PlayerState.Stand;

        public float JumpVelocity = 3.5f;
        public float CoyoteTimeDuration = 0.1f;
        public float PhysicsMaxStep = 3f;

        private float walkVelocity;

        public Dictionary<InputAction, bool> InputsPressed = new Dictionary<InputAction, bool>();
        public Dictionary<InputAction, bool> InputsDown = new Dictionary<InputAction, bool>();
        public Dictionary<InputAction, bool> InputsUp = new Dictionary<InputAction, bool>();
        private Vector2 separationVector;

        public Player() {
            Collider = new Collider(new Vector2[] {
                new Vector2(-3, -6),
                new Vector2( 3, -6),
                new Vector2( 3,  8),
                new Vector2(-3,  8)
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
                new SpriteFrame(new Rectangle(64, 0,  16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(5, 0)), frameEvent: SpawnDustParticle),
                new SpriteFrame(new Rectangle(64, 0,  16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(5, 0)), frameEvent: SpawnDustParticle),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(3, 0))),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 1, rootMotion: new RootMotion(new Vector2(2, 0))),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 1, rootMotion: new RootMotion(new Vector2(1, 0)), frameEvent: SpawnDustParticle),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(0, 0))),
                new SpriteFrame(new Rectangle(16, 16, 16, 16), new Rectangle(-8, -8, 16, 16), 4, rootMotion: new RootMotion(new Vector2(0, 0)), frameEvent: (sender, anim) => {
                    State = PlayerState.Stand;
                }),
            }));
            Collider roll_collider = new Collider(new Vector2[] {
                new Vector2(-4, 0),
                new Vector2( 4, 0),
                new Vector2( 4, 8),
                new Vector2(-4, 8)
            }, new Vector2(0, 0), new Vector2(0, 0));
            AddAnimation(new SpriteAnimation("roll", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(64, 0, 16, 16), new Rectangle(-8, -8, 16, 16), 3),
                new SpriteFrame(new Rectangle(0, 32, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(4, -3))),
                new SpriteFrame(new Rectangle(16, 32, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(2, 0)), frameEvent: SpawnDustParticle, frameCollider: roll_collider),
                new SpriteFrame(new Rectangle(32, 32, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(2, 0)), frameEvent: SpawnDustParticle, frameCollider: roll_collider),
                new SpriteFrame(new Rectangle(48, 32, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(2, 0)), frameEvent: (sender, anim) => {
                    SpawnDustParticle(sender, anim);
                    anim.CurrentFrame = 1;
                }, frameCollider: roll_collider),
            }));
            AddAnimation(new SpriteAnimation("attack", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));

            foreach (InputAction a in Enum.GetValues(typeof(InputAction))) {
                InputsPressed[a] = false;
                InputsDown[a] = false;
                InputsUp[a] = false;
            }
        }

        public override void Update(GameTime gameTime, Map level) {
            ProcessInput();

            if (!Utils.DEBUG_FLY) {
                if (Grounded) {
                    if (HasAerialControl) Velocity = GroundNormal.PerRight().Normalized() * walkVelocity;
                    else Velocity = new Vector2();
                    if (walkVelocity != 0 && Velocity.X != 0) {
                        Velocity /= Math.Abs(Velocity.X);
                    }
                } else if (HasAerialControl) {
                    Velocity.X = walkVelocity;
                }
            }

            // Apply gravity
            Velocity.Y += 0.3f;
            if (Utils.DEBUG_FLY) Velocity = new Vector2(0, 0);

            Grounded = false;
            GroundNormal = new Vector2(0, -1);

            float motion = Velocity.Length();
            Vector2 pos = Position.ToVector2();
            //while (motion > PhysicsMaxStep) {
            //    motion -= PhysicsMaxStep;
            //    pos = ProcessCollision(level, pos, PhysicsMaxStep);
            //}
            RootMotion rm = Animations[CurrentAnimation].RootMotion;

            pos += rm.Motion * new Vector2(rm.IgnoreFlip ? 1 : Flipped ? -1 : 1, 1);

            if (Utils.DEBUG_FLY) {
                if (InputsPressed[InputAction.Left]) { Position.X -= 1; Flipped = true; }
                if (InputsPressed[InputAction.Right]) { Position.X += 1; Flipped = false; }
                if (InputsPressed[InputAction.Up]) Position.Y -= 1;
                if (InputsPressed[InputAction.Down]) Position.Y += 1;
            }

            pos = ProcessCollision(level, pos, motion);
            if (!Utils.DEBUG_FLY) {
                Position = pos.ToPoint();
            }

            if (Grounded) {
                coyoteTime = CoyoteTimeDuration;
            } else {
                coyoteTime = (float)Math.Max(-1f, coyoteTime - gameTime.ElapsedGameTime.TotalSeconds);
            }

            RetestState:
            switch (State) {
                case PlayerState.Stand:
                    SetAnimation("stand");
                    if (walkVelocity != 0) {
                        State = PlayerState.Walk;
                        goto RetestState;
                    }
                    if (TryForState(PlayerState.Jump)) goto RetestState;
                    if (TryForState(PlayerState.Roll)) goto RetestState;
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    HasAerialControl = true;
                    break;
                case PlayerState.Walk:
                    SetAnimation("walk");
                    if (walkVelocity == 0) {
                        State = PlayerState.Stand;
                        goto RetestState;
                    }
                    if (TryForState(PlayerState.Jump)) goto RetestState;
                    if (TryForState(PlayerState.Roll)) goto RetestState;
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    HasAerialControl = true;
                    break;
                case PlayerState.Jump:
                    SetAnimation("jump");
                    if (Grounded) {
                        if (TryForState(PlayerState.Stand)) goto RetestState;
                    }
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    if (Velocity.Y < -2f) {
                        State = PlayerState.Fall;
                        goto RetestState;
                    }
                    HasAerialControl = true;
                    break;
                case PlayerState.Fall:
                    SetAnimation("fall");
                    if (Grounded) {
                        State = PlayerState.Stand;
                        goto RetestState;
                    }
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    HasAerialControl = true;
                    break;
                case PlayerState.Roll:
                    SetAnimation("roll");
                    if (!InputsPressed[InputAction.B] && TryForState(PlayerState.Stand)) goto RetestState;
                    TryForState(PlayerState.Jump);
                    State = PlayerState.Roll;
                    HasAerialControl = false;
                    break;
                case PlayerState.Dash:
                    SetAnimation("dash");
                    HasAerialControl = false;
                    break;
            }

            // Reset momentary input storage for the next update.
            foreach (InputAction key in Enum.GetValues(typeof(InputAction))) InputsUp[key] = false;
            foreach (InputAction key in Enum.GetValues(typeof(InputAction))) InputsDown[key] = false;

            base.Update(gameTime);
        }

        private void SetAnimation(string anim) {
            if (CurrentAnimation != anim) {
                Animations[anim].CurrentFrame = 0;
                Animations[anim].FrameTime = 0;
            }
            CurrentAnimation = anim;
        }

        private bool TryForState(PlayerState state) {
            switch (state) {
                case PlayerState.Stand:
                    State = PlayerState.Stand;
                    return true;
                case PlayerState.Jump:
                    if (InputsDown[InputAction.Up]) {
                        if (Grounded || coyoteTime > 0f) {
                            Velocity.Y = -JumpVelocity * (State == PlayerState.Roll ? 0.75f : 1f);
                            Grounded = false;
                            coyoteTime = -1f;
                        }
                        State = PlayerState.Jump;
                        return true;
                    }
                    break;
                case PlayerState.Dash:
                    if (InputsDown[InputAction.B]) {
                        State = PlayerState.Dash;
                        return true;
                    }
                    break;
                case PlayerState.Roll:
                    if (InputsPressed[InputAction.Down] && InputsDown[InputAction.B]) {
                        State = PlayerState.Roll;
                        return true;
                    }
                    break;
            }
            return false;
        }

        private Vector2 ProcessCollision(Map level, Vector2 pos, float motion) {
            if (motion > 0) {
                pos += Velocity.Normalized() * motion;
            }
            ref Collider col = ref Collider;
            if (Animations[CurrentAnimation].FrameCollider != null) col = ref Animations[CurrentAnimation].FrameCollider;
            col.Position = pos;

            Utils.QueueDebugPoly(col.Points, pos, new Color(255, 255, 0));

            foreach (Collider collider in level.Colliders) {
                Vector2 sv = new Vector2();
                sv = Collision.Separate(col, collider);

                if (sv.Y < 0 && Velocity.Y > 0) {
                    Grounded = true;
                }

                if (sv.LengthSquared() > 0f && Math.Abs(sv.Y) * 2f > Math.Abs(sv.X) && sv.Y < 0) {
                    GroundNormal = sv.Normalized();
                }

                if (sv.Y < 0 && Math.Abs(sv.X * 0.5) < Math.Abs(sv.Y)) {
                    sv = new Vector2(0, -1) * sv.Length();
                }

                pos += sv;
                separationVector = sv;
                col.Position = pos;
            }

            return pos;
        }

        private void ProcessInput() {
            walkVelocity = 0;
            if (InputsPressed[InputAction.Left]) walkVelocity -= 1;
            if (InputsPressed[InputAction.Right]) walkVelocity += 1;

            if (walkVelocity > 0 && Grounded) Flipped = false;
            if (walkVelocity < 0 && Grounded) Flipped = true;
        }

        public void HandleInput(InputAction action, bool state) {
            if (state) {
                if (!InputsPressed[action]) InputsDown[action] = true;
                InputsPressed[action] = true;
            } else {
                if (InputsPressed[action]) InputsUp[action] = true;
                InputsPressed[action] = false;
            }
        }

        public void Reset(Point pos) {
            //Utils.QueueDebugPoint(pos.ToVector2(), 12f, new Color(255, 128, 0), 1000);
            if (Utils.DEBUG_FLY) return;
            Position = pos - new Point(0, 8);
            Velocity = new Vector2(0, 1);
            Grounded = true;
            State = PlayerState.Stand;
            //Utils.QueueDebugPoint(Position.ToVector2(), 12f, new Color(0, 255, 255), 1000);
        }

        public void StopJump() {
            if (Velocity.Y < 0f) Velocity.Y = 0f;
        }

        private void SpawnDustParticle(object sender, SpriteAnimation anim) {
            if (!Grounded) return;
            DustPuffParticle p = new DustPuffParticle(Position + new Point(0, (int)Collider.BBRadius.Y - 2), Flipped);
            p.SetTexture(Texture);
            OnSpawnParticle(new SpawnParticleEventArgs(p));
        }
    }

    public enum PlayerState {
        Stand,
        Walk,
        Jump,
        Fall,
        Roll,
        Dash,
        Attack
    }
}
