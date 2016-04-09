using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace LowRezRogue {
    public static class FadeScreen {

        static bool active = false;

        static bool beforeBlack = true;

        static Action onBlackScreen;
        static Action onFadeEnd;

        static Rectangle[] spriteRects;
        static int currentFrame;

        public static void StartFadeScreen(double blackTimes = 0.0, Action doOnBlackScreen = null, Action doOnFadeEnd = null) {
            currentFrame = 24;
            active = true;
            beforeBlack = true;
            onBlackScreen = doOnBlackScreen;
            onFadeEnd = doOnFadeEnd;
            blackTime = blackTimes;
        }

        public static void StopFadeScreen() {
            active = false;
            onBlackScreen = null;
            onFadeEnd = null;
        }

        public static void Inititalize() {
            spriteRects = new Rectangle[25];
            for(int i = 0; i < 25; i++)
            {
                spriteRects[i] = new Rectangle(i*2,254, 2, 2);
            }
        }

        static bool blackTimeActive = false;
        static double blackTime;
        static double blackTimer;

        static double timer;
        static double timerEnd = 0.0333;

        public static void Update(double deltaTime) {
            if(!active)
                return;

            timer += deltaTime;

            


            if(blackTimeActive)
            {
                blackTimer += deltaTime;
                if(blackTimer >= blackTime)
                    blackTimeActive = false;
                return;
            }

            if(timer < timerEnd)
                return;

            if(beforeBlack)
            {
                currentFrame--;
                if(currentFrame == 0)
                {
                    beforeBlack = false;
                    blackTimer = 0.0;
                    blackTimeActive = true;
                    if(onBlackScreen != null)
                        onBlackScreen();
                }
            } else
            {
                currentFrame++;
                if(currentFrame == 25)
                {
                    active = false;
                    if(onFadeEnd != null)
                        onFadeEnd();
                    timer = 0.0;
                }
            }
            timer = 0.0;
        }


        static Rectangle dest = new Rectangle(0, 0, 64, 64);

        public static void Render(SpriteBatch spriteBatch, Texture2D texture) {
            if(!active)
                return;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: Camera.main.onlyZoom);
            spriteBatch.Draw(texture, dest, spriteRects[currentFrame], Color.White);
            spriteBatch.End();
        }

    }
}
