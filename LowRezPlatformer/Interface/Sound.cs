using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace LowRezRogue {
    public static class Sound {

        static Dictionary<string, SoundEffect> soundDict;
        static Random random;
        public static void Initialize(ContentManager Content) {
            random = new Random();
            soundDict = new Dictionary<string, SoundEffect>();

            Add("hit1", Content);
            Add("hit0", Content);
            Add("hitDemon1", Content);
            Add("hitDemon0", Content);
            Add("hit2", Content);
            Add("artifact_pickup", Content);
            Add("main_jump", Content);
            Add("main_pressedKey0", Content);
            Add("main_pressedKey1", Content);
            Add("main_smash", Content);
            Add("lost", Content);
            Add("victory", Content);
            for(int i = 0; i < 5; i++)
            {
                Add("click" + i.ToString(), Content);
                Add("switch" + i.ToString(), Content);
            }
        }

        public static void UnloadContent() {
            foreach(KeyValuePair<string, SoundEffect> pair in soundDict)
            {
                pair.Value.Dispose();
            }
        }

        static void Add(string str, ContentManager Content) {
            soundDict.Add(str, Content.Load<SoundEffect>($"Sounds/{str}"));
        }

        public static void Play(string name) {
            if(soundDict.ContainsKey(name))
                soundDict[name].Play();
        }

        public static void PlayHit() {
            Play("hit" + random.Next(0, 3));
        }

        public static void PlayPressedKey() {
            Play("main_pressedKey" + random.Next(0, 2));
        }

        public static void PlayClick() {
            Play("click" + random.Next(0, 5).ToString());
        }

        public static void PlaySwitch() {
            Play("switch" + random.Next(0, 5).ToString());
        }

        static SoundEffectInstance LostVictory;

        public static void PlayLostVictory(bool lost) {
            if(lost)
                LostVictory = soundDict["lost"].CreateInstance();
            else
                LostVictory = soundDict["victory"].CreateInstance();

            LostVictory.Play();
        }

        public static void StopLostVictory() {
            if(LostVictory.State == SoundState.Playing)
                LostVictory.Stop();
        }



    }
}
