using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace LowRezRogue {
    public class PauseMenu {

        Rectangle background = new Rectangle(0, 0, 64, 64);

        Rectangle headlineSource = new Rectangle(64, 0, 64, 11);
        Rectangle headlineDest = new Rectangle(0, 2, 64, 11);

        Rectangle continueSource = new Rectangle(74, 12, 42, 13);
        Rectangle continueDest = new Rectangle(10, 12, 42, 13);

        Rectangle newGameSource = new Rectangle(12, 85, 42, 13);
        Rectangle newGameDest = new Rectangle(10, 25, 42, 13);

        Rectangle helpSource = new Rectangle(74, 26, 42, 13);
        Rectangle helpDest = new Rectangle(10, 38, 42, 13); 

        Rectangle quitSource = new Rectangle(12, 101, 42, 13);
        Rectangle quitDest = new Rectangle(10, 51, 42, 13);

        Rectangle arrow0 = new Rectangle(4, 26, 6, 6);
        Rectangle arrow1 = new Rectangle(4, 42, 6, 6);

        Rectangle[] arrowDest = { new Rectangle(4, 16, 6, 6), new Rectangle(4, 29, 6, 6), new Rectangle(4, 42, 6, 6), new Rectangle(4, 55, 6, 6) };
        Rectangle[] arrowSource = { new Rectangle(0, 64, 6, 6), new Rectangle(0, 70, 6, 6) };

        int arrowPos = 0;
        int arrowAnim = 0;

        Action quit;
        Action newGame;
        Action continueGame;

        enum PauseState { pause, help}
        PauseState pauseState = PauseState.pause;

        public PauseMenu(Action quit, Action newGame, Action continueGame) {
            this.quit = quit;
            this.newGame = newGame;
            this.continueGame = continueGame;
        }

        public void Initialize() {

        }

        double animTimer = 0.0;

        public void Update(double deltaTime, KeyboardState keyboardState, KeyboardState lastKeyboardState) {

            animTimer += deltaTime;

            /*if(keyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
            {
                if(continueGame != null)
                    continueGame();
            }*/
            if(pauseState == PauseState.pause)
            {
                if((keyboardState.IsKeyDown(Keys.Enter) && (lastKeyboardState.IsKeyUp(Keys.Enter)) || (keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))))
                {
                    switch(arrowPos)
                    {
                        case 0:
                            {
                                continueGame();
                                pauseState = PauseState.pause;
                                break;
                            }
                        case 1:
                            {
                                newGame();
                                break;
                            }
                        case 2:
                            {
                                pauseState = PauseState.help;
                                break;
                            }
                        case 3:
                            {
                                quit();
                                break;
                            }
                    }
                }
                if(keyboardState.IsKeyDown(Keys.Up) && lastKeyboardState.IsKeyUp(Keys.Up))
                {
                    arrowPos -= 1;
                    if(arrowPos < 0)
                        arrowPos = 3;
                }

                if(keyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down))
                {
                    arrowPos += 1;
                    if(arrowPos > 3)
                        arrowPos = 0;
                }
            } else if(pauseState == PauseState.help)
            {
                if(keyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
                {
                    pauseState = PauseState.pause;
                }               
            }

            if(animTimer >= 0.5)
            {
                if(arrowAnim == 0)
                    arrowAnim = 1;
                else if(arrowAnim == 1)
                    arrowAnim = 0;

                animTimer = 0.0;
            }
        }

        public void Render(SpriteBatch spriteBatch, Texture2D texture) {
            if(pauseState == PauseState.pause)
            {
                spriteBatch.Draw(texture, background, background, Color.White);
                spriteBatch.Draw(texture, headlineDest, headlineSource, Color.White);
                spriteBatch.Draw(texture, continueDest, continueSource, Color.White);
                spriteBatch.Draw(texture, newGameDest, newGameSource, Color.White);
                spriteBatch.Draw(texture, helpDest, helpSource, Color.White);
                spriteBatch.Draw(texture, quitDest, quitSource, Color.White);
                spriteBatch.Draw(texture, arrowDest[arrowPos], arrowSource[arrowAnim], Color.White);
            } else if(pauseState == PauseState.help)
            {
                spriteBatch.Draw(texture, background, background, Color.White);

            }
        }

    }
}
