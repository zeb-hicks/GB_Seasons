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

namespace GB_Seasons.Entities {
    public class Player : SpriteEntity {
        public Collider Collider;
        public Vector2 Velocity;
        public bool Grounded;
        public bool HasAerialControl = true;
        private float coyoteTime;
        private int Jumps = 0;
        private int JumpMax = 2;
        public Vector2 GroundNormal = new Vector2(0, -1);

        public bool IsGrounded { get { return Grounded || coyoteTime > 0; } }

        public PlayerState State = PlayerState.Stand;
        public bool InSecret = false;

        public float JumpVelocity = 4.2f;
        public float CoyoteTimeDuration = 0.15f;
        public float PhysicsMaxStep = 6f;

        public bool HasControl = true;

        private float walkVelocity;

        public Dictionary<InputAction, bool> InputsPressed = new Dictionary<InputAction, bool>();
        public Dictionary<InputAction, bool> InputsDown = new Dictionary<InputAction, bool>();
        public Dictionary<InputAction, bool> InputsUp = new Dictionary<InputAction, bool>();
        public Dictionary<InputAction, int> InputTime = new Dictionary<InputAction, int>();
        private Vector2 separationVector;

        public List<MapVolume> InVolumes = new List<MapVolume>();

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
            AddAnimation(new SpriteAnimation("doublejump", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16), frameEvent: (sender, anim) => {
                    //SpawnParticleAt<DustPuffParticle>(Position + new Point(-3, (int)Collider.BBRadius.Y - 3), false);
                    //SpawnParticleAt<DustPuffParticle>(Position + new Point(3, (int)Collider.BBRadius.Y - 3), true);
                    SpawnParticleAt<ShockwaveParticle>(Position + new Vector2(0, Collider.BBRadius.Y - 2), false);
                }),
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16), frameEvent: (sender, anim) => {
                    State = PlayerState.Jump;
                    CurrentAnimation = "jump";
                }),
            }));
            AddAnimation(new SpriteAnimation("fall", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(32, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
            }));
            AddAnimation(new SpriteAnimation("dash", new List<SpriteFrame> {
                new SpriteFrame(new Rectangle(8,  0,  8,  16), new Rectangle(-4, -8, 8,  16), 2, frameEvent: (sender, anim) => {
                    Velocity.Y = Math.Min(Velocity.Y, 0f);
                    Audio.PlaySFX(SFX.Dash);
                    Flipped = walkVelocity < 0 ? true : walkVelocity > 0 ? false : Flipped;
                }),
                new SpriteFrame(new Rectangle(64, 0,  16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(6, 0)), frameEvent: (sender, anim) => {
                    Velocity.Y = Math.Min(Velocity.Y, 0f);
                    SpawnDustParticle(sender, anim);
                }),
                new SpriteFrame(new Rectangle(64, 0,  16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(6, 0)), frameEvent: (sender, anim) => {
                    Velocity.Y = Math.Min(Velocity.Y, 0f);
                    SpawnDustParticle(sender, anim);
                }),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 2, rootMotion: new RootMotion(new Vector2(4, 0))),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 1, rootMotion: new RootMotion(new Vector2(2, 0))),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 1, rootMotion: new RootMotion(new Vector2(1, 0)), frameEvent: SpawnDustParticle),
                new SpriteFrame(new Rectangle(0,  16, 16, 16), new Rectangle(-8, -8, 16, 16), 3, rootMotion: new RootMotion(new Vector2(0, 0))),
                new SpriteFrame(new Rectangle(16, 16, 16, 16), new Rectangle(-8, -8, 16, 16), 4, rootMotion: new RootMotion(new Vector2(0, 0)), frameEvent: (sender, anim) => {
                    State = PlayerState.Fall;
                }),
            }));
            Collider roll_collider = new Collider(new Vector2[] {
                new Vector2(-3, 0),
                new Vector2( 3, 0),
                new Vector2( 3, 8),
                new Vector2(-3, 8)
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
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
                new SpriteFrame(new Rectangle(48, 0, 16, 16), new Rectangle(-8, -8, 16, 16)),
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

            bool WasGrounded = IsGrounded;

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

            Vector2 pos = Position;
            RootMotion rm = Animations[CurrentAnimation].RootMotion;

            Vector2 vel = Velocity + rm.Motion * new Vector2(rm.IgnoreFlip ? 1 : Flipped ? -1 : 1, 1);
            float motion = vel.Length();

            if (Utils.DEBUG_FLY) {
                if (InputsPressed[InputAction.Left]) { Position.X -= 1; Flipped = true; }
                if (InputsPressed[InputAction.Right]) { Position.X += 1; Flipped = false; }
                if (InputsPressed[InputAction.Up]) Position.Y -= 1;
                if (InputsPressed[InputAction.Down]) Position.Y += 1;
            }

            foreach (MapVolume volume in level.Volumes) {
                Utils.QueueDebugPoly(volume.Poly, volume.Position, new Color(0, 255, 0));
            }

            while (motion > PhysicsMaxStep) {
                motion -= PhysicsMaxStep;
                pos = ProcessCollision(level, pos, vel, PhysicsMaxStep);
            }
            pos = ProcessCollision(level, pos, vel, motion);

            if (!Utils.DEBUG_FLY) {
                Position = pos;
                if (Math.Abs(Position.X - (int)Position.X) < 0.001) Position.X = (int)Position.X;
                if (Math.Abs(Position.Y - (int)Position.Y) < 0.001) Position.Y = (int)Position.Y;
            }

            if (Grounded) {
                coyoteTime = CoyoteTimeDuration;
                Jumps = 0;
            } else {
                coyoteTime = (float)Math.Max(-1f, coyoteTime - gameTime.ElapsedGameTime.TotalSeconds);
            }

            PlayerState pstate = State;
            RetestState:
            if (State != pstate) {
                if (State == PlayerState.Roll && IsGrounded) {
                    Audio.PlaySFX(SFX.Roll);
                } else if (pstate == PlayerState.Roll) {
                    Audio.StopSFX(SFX.Roll);
                }

                if (State == PlayerState.Jump || State == PlayerState.DoubleJump) {
                    Audio.PlaySFX(SFX.Jump);
                }
            }

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
                    if (!IsGrounded) {
                        State = PlayerState.Fall;
                        goto RetestState;
                    }
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
                    if (!IsGrounded) {
                        State = PlayerState.Fall;
                        goto RetestState;
                    }
                    HasAerialControl = true;
                    break;
                case PlayerState.Jump:
                    SetAnimation("jump");
                    //if (Velocity.Y < 0 && InputsUp[InputAction.Up]) {
                    //    Velocity.Y = 0f;
                    //}
                    if (IsGrounded) {
                        if (TryForState(PlayerState.Stand)) goto RetestState;
                    }
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    if (TryForState(PlayerState.DoubleJump)) goto RetestState;
                    if (Velocity.Y > 0f) {
                        State = PlayerState.Fall;
                        goto RetestState;
                    }
                    HasAerialControl = true;
                    break;
                case PlayerState.DoubleJump:
                    SetAnimation("doublejump");
                    HasAerialControl = true;
                    break;
                case PlayerState.Fall:
                    SetAnimation("fall");
                    if (IsGrounded) {
                        State = PlayerState.Stand;
                        goto RetestState;
                    }
                    if (TryForState(PlayerState.Dash)) goto RetestState;
                    if (TryForState(PlayerState.DoubleJump)) goto RetestState;
                    HasAerialControl = true;
                    break;
                case PlayerState.Roll:
                    SetAnimation("roll");
                    bool canstand = true;
                    foreach (MapVolume v in InVolumes) {
                        if (v.Type == MapVolumeType.LowPassage) canstand = false;
                    }
                    if (canstand && !InputsPressed[InputAction.Down] && TryForState(PlayerState.Stand)) goto RetestState;
                    //if (Velocity.Y < 0 && InputsUp[InputAction.Up]) {
                    //    Velocity.Y = 0f;
                    //}
                    TryForState(PlayerState.Jump);
                    State = PlayerState.Roll;
                    HasAerialControl = false;
                    if (!WasGrounded && IsGrounded) Audio.PlaySFX(SFX.Roll);
                    if (WasGrounded && !IsGrounded) Audio.StopSFX(SFX.Roll);
                    break;
                case PlayerState.Dash:
                    SetAnimation("dash");
                    HasAerialControl = false;
                    if (TryForState(PlayerState.DoubleJump)) goto RetestState;
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
                        if (IsGrounded) {
                            Jumps++;
                            Velocity.Y = -JumpVelocity * (State == PlayerState.Roll ? 0.8f : 1f);
                            Grounded = false;
                            coyoteTime = -1f;
                            State = PlayerState.Jump;
                            InputsDown[InputAction.Up] = false;
                            Audio.StopSFX(SFX.Roll);
                            return true;
                        }
                    }
                    break;
                case PlayerState.DoubleJump:
                    if (InputsDown[InputAction.Up]) {
                        if (Jumps < JumpMax) {
                            Jumps++;
                            Velocity.Y = -JumpVelocity * (State == PlayerState.Roll ? 0.8f : 1f);
                            Grounded = false;
                            State = PlayerState.DoubleJump;
                            InputsDown[InputAction.Up] = false;
                            return true;
                        }
                    }
                    break;
                case PlayerState.Dash:
                    if (InputsDown[InputAction.B]) {
                        State = PlayerState.Dash;
                        return true;
                    }
                    break;
                case PlayerState.Roll:
                    if (InputsPressed[InputAction.Down] && walkVelocity != 0) {
                        State = PlayerState.Roll;
                        return true;
                    }
                    break;
            }
            return false;
        }

        private Vector2 ProcessCollision(Map level, Vector2 pos, Vector2 vel, float motion) {
            if (motion > 0) {
                pos += vel.Normalized() * motion;
            } else {
                return pos;
            }
            ref Collider col = ref Collider;
            if (Animations[CurrentAnimation].FrameCollider != null) col = ref Animations[CurrentAnimation].FrameCollider;
            col.Position = pos;

            Utils.QueueDebugPoly(col.Points, pos, new Color(255, 255, 0));

            foreach (MapCollider mapc in level.Colliders) {
                //if (mapc.Mode != ColliderMode.Collision) continue;
                Utils.QueueDebugPoly(mapc.Collider.Points, mapc.Position, new Color(255, 0, 0));
                Vector2 sv = new Vector2();
                sv = Collision.Separate(col, mapc.Collider);

                if (sv.Y < 0 && vel.Y > 0) {
                    Grounded = true;
                }

                //if (sv.Y > 0 && vel.Y < 0) {
                //    vel.Y = 0;
                //    Velocity.Y = 0;
                //}

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

            InVolumes.Clear();
            foreach (MapVolume volume in level.Volumes) {
                if (Collision.PointInCollider(Position, volume.Collider)) {
                    InVolumes.Add(volume);
                }
            }

            return pos;
        }

        private void ProcessInput() {
            walkVelocity = 0;
            if (InputsPressed[InputAction.Left]) walkVelocity -= 1;
            if (InputsPressed[InputAction.Right]) walkVelocity += 1;

            if (walkVelocity > 0 && IsGrounded) Flipped = false;
            if (walkVelocity < 0 && IsGrounded) Flipped = true;
        }

        public void HandleInput(InputAction action, bool state) {
            if (state) {
                if (!InputsPressed[action]) {
                    InputsDown[action] = true;
                    InputTime[action] = 0;
                }
                InputsPressed[action] = true;
            } else {
                if (InputsPressed[action]) {
                    InputsUp[action] = true;
                    InputTime[action] = 0;
                }
                InputsPressed[action] = false;
            }
        }

        public void Reset(Vector2 pos) {
            //Utils.QueueDebugPoint(pos.ToVector2(), 12f, new Color(255, 128, 0), 1000);
            if (Utils.DEBUG_FLY) return;
            Position = pos - new Vector2(0, 8);
            Velocity = new Vector2(0, 1);
            Grounded = true;
            State = PlayerState.Stand;
            //Utils.QueueDebugPoint(Position.ToVector2(), 12f, new Color(0, 255, 255), 1000);
        }

        public void StopJump() {
            if (Velocity.Y < 0f) Velocity.Y = 0f;
        }

        private void SpawnDustParticle(object sender, SpriteAnimation anim) {
            if (!IsGrounded) return;
            SpawnParticleAt<DustPuffParticle>(Position + new Vector2(0, (int)Collider.BBRadius.Y - 3), Flipped);
        }

        private void SpawnDashParticle(object sender, SpriteAnimation anim) {
            if (!IsGrounded) return;
            SpawnParticleAt<DustPuffParticle>(Position + new Vector2(0, (int)Collider.BBRadius.Y - 3), Flipped);
        }

        private void SpawnAttackParticle(object sender, SpriteAnimation anim) {
            if (!IsGrounded) return;
            SpawnParticleAt<DustPuffParticle>(Position + new Vector2(0, (int)Collider.BBRadius.Y - 3), Flipped);
        }

        protected void SpawnParticleAt<T>(params object[] args) {
            object po = (T)Activator.CreateInstance(typeof(T), args);
            Particle p = (Particle)po;
            p.SetTexture(Texture);
            OnSpawnParticle(new SpawnParticleEventArgs(p));
        }
    }

    public enum PlayerState {
        Stand,
        Walk,
        Jump,
        DoubleJump,
        Fall,
        Roll,
        Dash,
        Attack
    }
}
