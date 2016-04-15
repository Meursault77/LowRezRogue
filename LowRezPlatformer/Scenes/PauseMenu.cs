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
        Rectangle[] RightArrowSource = { new Rectangle(0, 64, 6, 6), new Rectangle(0, 70, 6, 6) };
        Rectangle[] leftArrowSource = { new Rectangle(0,76,6,6), new Rectangle(0,82,6,6) };


        Rectangle[] helpPages = { new Rectangle(128,0,64,64), new Rectangle(128,128,64,64), new Rectangle(192,0,64,64), new Rectangle(192,128,64,64),
            new Rectangle(192,192,64,64), new Rectangle(128,192,64,64), new Rectangle(64,192,64,64), new Rectangle(64,64,64,64), new Rectangle(128,64,64,64),
            new Rectangle(192,64,64,64), new Rectangle(64,128,64,64) };

        Rectangle[] artifactNums = { new Rectangle(0,128,11,6), new Rectangle(0,134,11,6), new Rectangle(0, 140, 11, 6), new Rectangle(0, 146, 11, 6),
            new Rectangle(0, 152, 11, 6), new Rectangle(0, 158, 11, 6) };

        Rectangle[] artifacts = { new Rectangle(0, 136, 24, 24), new Rectangle(24, 136, 24, 24), new Rectangle(48, 136, 24, 24), new Rectangle(72, 136, 24, 24),
                                    new Rectangle(96, 136, 24, 24), new Rectangle(120, 136, 24, 24)};


        int artifactsFound = 0;
        int currentHelpPage = 0;



        int arrowPos = 0;
        int arrowAnim = 0;

        Action quit;
        Action newGame;
        Action continueGame;

        public enum PauseState { pause, help}
        public PauseState pauseState = PauseState.pause;

        public PauseMenu(Action quit, Action newGame, Action continueGame) {
            this.quit = quit;
            this.newGame = newGame;
            this.continueGame = continueGame;
        }

        public void Initialize() {

        }

        public void GetArtifacts(int artifactsFound) {
            this.artifactsFound = artifactsFound;
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
                    Sound.PlayClick();
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
                    Sound.PlayClick();
                    arrowPos -= 1;
                    if(arrowPos < 0)
                        arrowPos = 3;
                }

                if(keyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down))
                {
                    Sound.PlayClick();
                    arrowPos += 1;
                    if(arrowPos > 3)
                        arrowPos = 0;
                }
            } else if(pauseState == PauseState.help)
            {
                if(keyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
                {
                    Sound.PlayClick();
                    pauseState = PauseState.pause;
                }
                if(keyboardState.IsKeyDown(Keys.Right) && lastKeyboardState.IsKeyUp(Keys.Right))
                {
                    Sound.PlayClick();
                    currentHelpPage++;
                    if(currentHelpPage >= helpPages.Length)
                        currentHelpPage = 0;
                }
                if(keyboardState.IsKeyDown(Keys.Left) && lastKeyboardState.IsKeyUp(Keys.Left))
                {
                    Sound.PlayClick();
                    currentHelpPage--;
                    if(currentHelpPage < 0)
                        currentHelpPage = helpPages.Length - 1;
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

        public void Render(SpriteBatch spriteBatch, Texture2D menuAtlas, Texture2D uiAtlas) {
            if(pauseState == PauseState.pause)
            {
                spriteBatch.Draw(menuAtlas, background, background, Color.White);
                spriteBatch.Draw(menuAtlas, headlineDest, headlineSource, Color.White);
                spriteBatch.Draw(menuAtlas, continueDest, continueSource, Color.White);
                spriteBatch.Draw(menuAtlas, newGameDest, newGameSource, Color.White);
                spriteBatch.Draw(menuAtlas, helpDest, helpSource, Color.White);
                spriteBatch.Draw(menuAtlas, quitDest, quitSource, Color.White);
                spriteBatch.Draw(menuAtlas, arrowDest[arrowPos], RightArrowSource[arrowAnim], Color.White);
            } else if(pauseState == PauseState.help)
            {
                spriteBatch.Draw(menuAtlas, background, background, Color.White);
                spriteBatch.Draw(menuAtlas, background, helpPages[currentHelpPage], Color.White);
                if(currentHelpPage == 2)
                {
                    spriteBatch.Draw(uiAtlas, new Rectangle(20,30, 24, 24), artifacts[artifactsFound], Color.White);
                    spriteBatch.Draw(menuAtlas, new Rectangle(32,15,11,6), artifactNums[artifactsFound], Color.White);
                }
                spriteBatch.Draw(menuAtlas, new Rectangle(5,4,6,6), leftArrowSource[arrowAnim], Color.White);
                spriteBatch.Draw(menuAtlas, new Rectangle(53,4,6,6), RightArrowSource[arrowAnim], Color.White);
                spriteBatch.Draw(menuAtlas, new Rectangle(51, 56, 10, 5), new Rectangle(0,122,10,5), Color.White);

            }
        }

    }
}
