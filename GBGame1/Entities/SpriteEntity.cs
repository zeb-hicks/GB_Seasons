using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Entities {
    public partial class SpriteEntity {
        public Point Position;
        public Dictionary<string, SpriteAnimation> Animations;
        public string CurrentAnimation;
        public Texture2D Texture;
        public bool Flipped;
        public bool Despawn;

        public SpriteEntity() {
            Position = new Point();
            Animations = new Dictionary<string, SpriteAnimation>();
        }
        public SpriteEntity(Dictionary<string, SpriteAnimation> anims, string defaultAnim = null) {
            Animations = anims;
            CurrentAnimation = defaultAnim;
            Flipped = false;
            Despawn = false;
        }

        public virtual void Update(GameTime gameTime) {
            Animations.TryGetValue(CurrentAnimation, out var a);
            string na = a?.Update(gameTime);
            Despawn = a.Despawn;
            if (na != null) {
                // Frame triggered new animation
                CurrentAnimation = na;
            }
        }
        public virtual void Update(GameTime gameTime, Map level) {
            Animations.TryGetValue(CurrentAnimation, out var a);
            string na = a.Update(gameTime);
            Despawn = a.Despawn;
            if (na != null) {
                // Frame triggered new animation
                CurrentAnimation = na;
            }
        }

        public delegate void SpawnParticleEventHandler(object sender, SpawnParticleEventArgs e);
        public event SpawnParticleEventHandler SpawnParticle;

        protected virtual void OnSpawnParticle(SpawnParticleEventArgs e) {
            SpawnParticle?.Invoke(this, e);
        }

        public delegate void RootMotionEventHandler(object sender, RootMotionEventArgs e);
        public event RootMotionEventHandler ApplyRootMotion;

        protected virtual void OnApplyRootMotion(RootMotionEventArgs e) {
            ApplyRootMotion?.Invoke(this, e);
        }

        public void AddAnimation(SpriteAnimation anim, int startFrame = 0) {
            Animations.Add(anim.Name, anim);
            anim.CurrentFrame = startFrame;
            if (CurrentAnimation == null) CurrentAnimation = anim.Name;
        }

        public void SetTexture(Texture2D texture) {
            Texture = texture;
        }

        public virtual void Draw(SpriteBatch spriteBatch) {
            if (CurrentAnimation == null || Texture == null) return;
            Animations.TryGetValue(CurrentAnimation, out var a);
            
            if (a == null) return;
            var f = a.Frames[a.CurrentFrame];

            // Draw the current animation frame.
            var r = f.Dest.AtOffset(Position);
            //r.Y -= 8;
            if (Flipped) r = r.Flipped();
            spriteBatch.Draw(Texture, r, f.Source, Color.White);
        }
    }

    public delegate void FrameEventAction(object sender, SpriteAnimation anim);
    public delegate void SpawnParticle(object sender, Particle particle);

    public class SpawnParticleEventArgs : EventArgs {
        public Particle Particle { get; private set; }
        public SpawnParticleEventArgs(Particle particle) {
            Particle = particle;
        }
    }

    public class RootMotionEventArgs : EventArgs {
        public RootMotion RootMotion { get; private set; }
        public RootMotionEventArgs(RootMotion rootMotion) {
            RootMotion = rootMotion;
        }
    }

    public class SpriteAnimation {
        public string Name;
        public int CurrentFrame;
        public float FrameTime;
        public List<SpriteFrame> Frames;
        public bool Playing;
        public bool Despawn;
        public RootMotion RootMotion;
        public Collider FrameCollider;
        public SpriteAnimation(string name, List<SpriteFrame> frames, bool playing = true) {
            Name = name;
            CurrentFrame = 0;
            FrameTime = 0;
            Frames = frames;
            Playing = playing;
            Despawn = false;
            RootMotion = new RootMotion();
        }

        public string Update(GameTime gameTime) {
            // Increment the frame time by the elapsed game time
            FrameTime += (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            // Step frame forward as needed
            var f = Frames[CurrentFrame];
            while (FrameTime >= f.Duration) {
                FrameTime -= f.Duration;
                RootMotion = f.RootMotion;
                FrameCollider = f.FrameCollider;
                f.FrameEvent?.Invoke(this, this);
                Despawn = f.Despawn;
                CurrentFrame++;
                CurrentFrame %= Frames.Count;
                if (f.SetAnim != null) return f.SetAnim;
                f = Frames[CurrentFrame];
            }

            return null;
        }
    }

    public struct RootMotion {
        public Vector2 Motion;
        public bool IgnoreFlip;

        public RootMotion(Vector2 motion = new Vector2(), bool ignoreFlip = false) {
            Motion = motion;
            IgnoreFlip = ignoreFlip;
        }
    }

    public struct SpriteFrame {
        public Rectangle Source;
        public Rectangle Dest;
        public int Duration;
        public string SetAnim;
        public bool Despawn;
        public FrameEventAction FrameEvent;
        public RootMotion RootMotion;
        public Collider FrameCollider;
        public SpriteFrame(Rectangle source, Rectangle dest, int duration = 1, string setAnim = null, bool despawn = false, FrameEventAction frameEvent = null, RootMotion rootMotion = new RootMotion(), Collider frameCollider = null) {
            Source = source;
            Dest = dest;
            Duration = duration;
            SetAnim = setAnim;
            Despawn = despawn;
            FrameEvent = frameEvent;
            RootMotion = rootMotion;
            FrameCollider = frameCollider;
        }
    }
}
