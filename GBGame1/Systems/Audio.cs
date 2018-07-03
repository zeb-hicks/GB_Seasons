using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GB_Seasons.Systems {
    public static class Audio {
        public static Dictionary<SFX, Cue> SFXCues = new Dictionary<SFX, Cue>();

        static AudioEngine audioEngine;
        static SoundBank soundBank;
        static WaveBank waveBank;

        public static void LoadSFX(ContentManager Content) {
            audioEngine = new AudioEngine("Content/Sounds.xgs");
            soundBank = new SoundBank(audioEngine, "Content/Sounds.xsb");
            waveBank = new WaveBank(audioEngine, "Content/Sounds.xwb");

            SFXCues[SFX.Dash] = soundBank.GetCue("sfx_dash");
            SFXCues[SFX.Jump] = soundBank.GetCue("sfx_jump");
            SFXCues[SFX.Roll] = soundBank.GetCue("sfx_roll");

            SFXCues[SFX.Pickup] = soundBank.GetCue("sfx_pickup");
        }

        public static void PlaySFX(SFX effect) {
            SFXCues[effect].Play();
        }

        public static void StopSFX(SFX effect) {
            SFXCues[effect].Stop(AudioStopOptions.AsAuthored);
        }
    }

    public enum SFX {
        Jump,
        Dash,
        Roll,
        Pickup
    }
}
