using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LowRezRogue {
    public class DeathScene {

        Rectangle background = new Rectangle(0, 0, 64, 64);

        Rectangle headlineSource = new Rectangle(6,64,64,18);
        Rectangle headlineDest = new Rectangle(6,0,64,18);
        Rectangle headlineVictorySource = new Rectangle(6,192,64,18);


        Rectangle newGameSource = new Rectangle(12,85,42,13);
        Rectangle newGameDest = new Rectangle(10,22,42,13);

        Rectangle quitSource = new Rectangle(12,101,42,13);
        Rectangle quitDest = new Rectangle(10,38,42,13);

        Rectangle arrow0 = new Rectangle(4,26,6,6);
        Rectangle arrow1 = new Rectangle(4,42,6,6);
        Rectangle[] arrowSource = { new Rectangle(0, 64, 6, 6), new Rectangle(0,70,6,6) };

        int arrowPos = 0;
        int arrowAnim = 0;

        Action quit;
        Action newGame;

        bool didWin = false;

        public DeathScene(Action quit, Action newGame) {
            this.quit = quit;
            this.newGame = newGame;
        }

        public void Initialize() {

        }

        double animTimer = 0.0;

        public void SetDeathOrVictory(bool won = false) {
            didWin = won;
        }


        public void Update(double deltaTime, KeyboardState keyboardState, KeyboardState lastKeyboardState) {

            animTimer += deltaTime;


            if((keyboardState.IsKeyDown(Keys.Enter) && (lastKeyboardState.IsKeyUp(Keys.Enter)) || ( keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))))
            {
                Sound.PlayClick();
                if(arrowPos == 0)
                {
                    if(newGame != null)
                    {
                        Sound.StopLostVictory();
                        FadeScreen.StartFadeScreen(0.5, newGame);                        
                    }
                } else if(arrowPos == 1)
                {
                    if(quit != null)
                        quit();
                }
            }
            if(keyboardState.IsKeyDown(Keys.Up) && lastKeyboardState.IsKeyUp(Keys.Up) || (keyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down)))
            {
                Sound.PlayClick();
                if(arrowPos == 0)
                    arrowPos = 1;
                else if(arrowPos == 1)
                    arrowPos = 0;
            }

            if(animTimer >= 0.5)
            {
                Sound.PlayClick();
                if(arrowAnim == 0)
                    arrowAnim = 1;
                else if(arrowAnim == 1)
                    arrowAnim = 0;

                animTimer = 0.0;
            }


        }

        public void Render(SpriteBatch spriteBatch, Texture2D texture) {
            spriteBatch.Draw(texture, background, background, Color.White);
            if(didWin)
                spriteBatch.Draw(texture, headlineDest, headlineVictorySource, Color.White);
            else
                spriteBatch.Draw(texture, headlineDest, headlineSource, Color.White);

            spriteBatch.Draw(texture, newGameDest, newGameSource, Color.White);
            spriteBatch.Draw(texture, quitDest, quitSource, Color.White);
            if(arrowPos == 0)
                spriteBatch.Draw(texture, arrow0, arrowSource[arrowAnim], Color.White);
            else if(arrowPos == 1)
                spriteBatch.Draw(texture, arrow1, arrowSource[arrowAnim], Color.White);
        }

    }
}
