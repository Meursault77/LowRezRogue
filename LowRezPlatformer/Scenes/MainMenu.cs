using System;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LowRezRogue {
    public class MainMenu {

        struct Player {
            public Point position;
            public Point size;
            public Rectangle spriteRect;
            Animation currentAnim;
            Dictionary<string, Animation> animations;

            public Player(Point pos, Dictionary<string, Animation> animDict) {
                position = pos;
                size = new Point(16, 16);
                spriteRect = Rectangle.Empty;
                animations = animDict;
                currentAnim = animDict["idle"].StartAnimation();
                UpdateAnimation();
            }

            public void UpdateAnimation() {
                spriteRect = currentAnim.GetNextFrame();
                if(spriteRect == Rectangle.Empty)
                {
                    currentAnim = animations["idle"].StartAnimation();
                    spriteRect = currentAnim.GetNextFrame();
                }
            }

            public void TriggerAnimation(string name, Action endAction = null) {
                if(animations.ContainsKey(name))
                {
                    currentAnim = animations[name].StartAnimation();
                    currentAnim.endOfAnimationAction = endAction;
                }
            }
        }

        struct PressA {
            public bool active;

            public Point position;
            public Point size;
            public Rectangle spriteRect;
            Animation currentAnim;

            public PressA(Dictionary<string, Animation> animDict) {
                active = true;
                position = new Point(54,54);
                size = new Point(16, 16);
                spriteRect = Rectangle.Empty;
                currentAnim = animDict["pressA"].StartAnimation();
            }

            public void UpdateAnimation() {
                spriteRect = currentAnim.GetNextFrame();
            }

        }

        Dictionary<string, Animation> playerAnimations;

        Texture2D mainAtlas;

        Rectangle backgroundDest = new Rectangle(0, 0, 64, 64);
        Rectangle backgroundSource = new Rectangle(8,34,64,64);

        bool logoActive = true;
        Rectangle logoDest = new Rectangle(8, 26, 44, 33);
        Rectangle logoSource = new Rectangle(80,0, 44, 33);

        Rectangle[] introLines;
        

        Player player;
        PressA pressA;

        bool hideLogo = false;
        bool moveBackground = false;
        bool triggerPlayerSmash = false;
        int backgroundTravelDistance = 34;


        enum IntroState { notStarted, one, two, three, finished }
        IntroState introState = IntroState.notStarted;

        public MainMenu() {

        }

        public void Initialize(ContentManager Content) {
            mainAtlas = Content.Load<Texture2D>("Title");
            LoadAnimations();
            player = new Player(new Point(27, 1), playerAnimations);
            pressA = new PressA(playerAnimations);

            introLines = new Rectangle[12];
            introLines[0] = new Rectangle(128, 0, 0, 7);
            introLines[1] = new Rectangle(128, 7, 0, 7);
            introLines[2] = new Rectangle(128, 14, 0, 7);
            introLines[3] = new Rectangle(128, 21, 0, 7);
            introLines[4] = new Rectangle(128, 28, 0, 7);
            introLines[5] = new Rectangle(128, 35, 0, 7);
            introLines[6] = new Rectangle(128, 42, 0, 7);
            introLines[7] = new Rectangle(128, 49, 0, 7);
            introLines[8] = new Rectangle(128, 56, 0, 7);
            introLines[9] = new Rectangle(128, 63, 0, 7);
            introLines[10] = new Rectangle(128, 70, 0, 7);

        }

        void LoadAnimations() {
            playerAnimations = new Dictionary<string, Animation>();
            XmlDocument animXml = new XmlDocument();
            animXml.Load("Content/XML/Animations.xml");

            int pixel = int.Parse(animXml.SelectSingleNode("animations/mainMenu").Attributes["pixel"].Value);

            foreach(XmlNode node in animXml.SelectNodes("animations/mainMenu/anim"))
            {
                bool looping = Convert.ToBoolean(node.Attributes["looping"].Value);
                string name = node.Attributes["name"].Value;

                XmlNodeList frames = node.SelectNodes("frame");
                var newAnim = new Animation(name, looping, frames.Count);
                newAnim.rects = new Rectangle[frames.Count];

                for(int i = 0; i < frames.Count; i++)
                {
                    int x = int.Parse(frames[i].Attributes["x"].Value);
                    int y = int.Parse(frames[i].Attributes["y"].Value);
                    newAnim.rects[i] = new Rectangle(x * pixel, y * pixel, pixel, pixel);
                }
                playerAnimations.Add(name, newAnim);
            }
        }

        public void UnloadContent() {
            mainAtlas.Dispose();
        }

        double animationBackgroundTimer = 0;

        double animationFrameTimer = 0;

        double animationFrameTime = 0.166;

        public void Update(double deltaTime, KeyboardState keyboardState, KeyboardState lastKeyboardState) {

            animationFrameTimer += deltaTime;
            animationBackgroundTimer += deltaTime;

            if(keyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter))
            {
                FadeScreen.StartFadeScreen(0.5,
                        () => { LowRezRogue.gameScene = LowRezRogue.GameScene.game; this.UnloadContent(); }, null);
            }

            if(pressA.active && keyboardState.IsKeyDown(Keys.A) && lastKeyboardState.IsKeyUp(Keys.A))
            {

                switch(introState)
                {
                    case IntroState.notStarted:
                        {
                            hideLogo = true;
                            pressA.active = false;
                            break;
                        }
                    case IntroState.one:
                        {
                            introState = IntroState.two;
                            pressA.active = false;
                            break;
                        }

                    case IntroState.two:
                        {
                            introState = IntroState.three;
                            pressA.active = false;
                            break;
                        }
                    case IntroState.three:
                        {
                            introState = IntroState.finished;
                            pressA.active = false;
                            moveBackground = true;
                            break;
                        }
                    case IntroState.finished:
                        {

                            break;
                        }
                }

            }

            if(animationBackgroundTimer >= 0.088)    //166)
            {
                if(triggerPlayerSmash)
                {
                    player.TriggerAnimation("smash", () => { animationFrameTime = 0.166; player.TriggerAnimation("idle2"); FadeScreen.StartFadeScreen(0.5 ,
                        () => { LowRezRogue.gameScene = LowRezRogue.GameScene.game; this.UnloadContent(); }, null); });

                    triggerPlayerSmash = false;
                }
                if(moveBackground)
                {
                    if(backgroundSource.Y > 0)
                    {
                        backgroundSource.Y -= 1;
                        player.position.Y += 1;
                        logoDest.Y += 2;
                    } else
                    {
                        moveBackground = false;
                        triggerPlayerSmash = true;
                        animationFrameTime = 0.088;
                    }
                }
                if(logoActive && hideLogo)
                {
                    if(logoDest.Y < 65)
                    {
                        logoDest.Y += 2;
                    } else
                    {
                        logoActive = false;
                        introState = IntroState.one;
                    }
                }
                switch(introState)
                {
                    case IntroState.notStarted:
                        {
                            break;
                        }
                    case IntroState.one:
                        {
                          
                            if(introLines[0].Width < 48)
                                introLines[0].Width += 4;
                            else
                            {
                                if(introLines[1].Width < 48)
                                    introLines[1].Width += 4;
                                else
                                {
                                    if(introLines[2].Width < 48)
                                        introLines[2].Width += 4;
                                    else
                                        pressA.active = true;
                                }
                            }
                                
                                
                            
                            break;
                        }
                    case IntroState.two:
                        {
                            if(introLines[3].Width < 48)
                                introLines[3].Width += 4;
                            else
                            {
                                if(introLines[4].Width < 48)
                                    introLines[4].Width += 4;
                                else
                                {
                                    if(introLines[5].Width < 48)
                                        introLines[5].Width += 4;
                                    else
                                    {
                                        if(introLines[6].Width < 48)
                                            introLines[6].Width += 4;
                                        else
                                            pressA.active = true;                                       
                                    }
                                }
                            }
                            break;
                        }
                    case IntroState.three:
                        {
                            if(introLines[7].Width < 48)
                                introLines[7].Width += 4;
                            else
                            {
                                if(introLines[8].Width < 48)
                                    introLines[8].Width += 4;
                                else
                                {
                                    if(introLines[9].Width < 48)
                                        introLines[9].Width += 4;
                                    else
                                    {
                                        if(introLines[10].Width < 48)
                                            introLines[10].Width += 4;
                                        else
                                            pressA.active = true;
                                    }
                                }
                            }
                            break;
                        }
                }

                animationBackgroundTimer = 0.0;
            }

            if(animationFrameTimer >= animationFrameTime)
            {
                player.UpdateAnimation();
                pressA.UpdateAnimation();
                animationFrameTimer = 0.0;
            }

        }
            

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(mainAtlas, backgroundDest, backgroundSource, Color.White);
            spriteBatch.Draw(mainAtlas, new Rectangle(player.position, player.size), player.spriteRect, Color.White);
            if(pressA.active)
                spriteBatch.Draw(mainAtlas, new Rectangle(pressA.position, pressA.size), pressA.spriteRect, Color.White);
            if(logoActive)
                spriteBatch.Draw(mainAtlas, logoDest, logoSource, Color.White);
            switch(introState)
            {
                case IntroState.notStarted:
                    {
                        break;
                    }
                case IntroState.one:
                    {
                        for(int i = 0; i <= 2; i++)
                        {
                            spriteBatch.Draw(mainAtlas, new Rectangle(8, 26 + introLines[i].Y, introLines[i].Width, 7), introLines[i], Color.White);


                        }
                        break;
                    }
                case IntroState.two:
                    {
                        for(int i = 3; i <= 6; i++)
                        {
                            spriteBatch.Draw(mainAtlas, new Rectangle(8, 26 + introLines[i - 3].Y, introLines[i].Width, 7), introLines[i], Color.White);
                        }
                        break;
                    }
                case IntroState.three:
                    {
                        for(int i = 7; i <= 10; i++)
                        {
                            spriteBatch.Draw(mainAtlas, new Rectangle(8, 26 + introLines[i - 7].Y, introLines[i].Width, 7), introLines[i], Color.White);

                        }
                        break;
                    }
            }
        }
    }
}
