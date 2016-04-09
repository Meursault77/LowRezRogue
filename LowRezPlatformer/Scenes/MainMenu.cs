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

        Dictionary<string, Animation> playerAnimations;

        Texture2D mainAtlas;

        Rectangle backgroundDest;
        Rectangle backgroundSource;

        Rectangle logoDest = new Rectangle(8, 26, 44, 33);
        Rectangle logoSource = new Rectangle(96,0, 44, 33);

        Player player;

        bool moveBackground = false;
        bool triggerPlayerSmash = false;
        int backgroundTravelDistance = 34;

        

        public MainMenu() {

        }

        public void Initialize(ContentManager Content) {
            mainAtlas = Content.Load<Texture2D>("Title");
            backgroundDest = new Rectangle(0,0,64,64);
            backgroundSource = new Rectangle(0,34,64,64);
            LoadAnimations();
            player = new Player(new Point(27, 1), playerAnimations);

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

            if(keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))
            {
                moveBackground = true;
            }

            if(animationBackgroundTimer >= 0.088)    //166)
            {
                if(triggerPlayerSmash)
                {
                    player.TriggerAnimation("smash", () => { animationFrameTime = 0.166; player.TriggerAnimation("idle2"); LowRezRogue.gameScene = LowRezRogue.GameScene.game; this.UnloadContent(); });
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

                animationBackgroundTimer = 0.0;
            }

            if(animationFrameTimer >= animationFrameTime)
            {
                player.UpdateAnimation();
                animationFrameTimer = 0.0;
            }

        }
            

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(mainAtlas, backgroundDest, backgroundSource, Color.White);
            spriteBatch.Draw(mainAtlas, new Rectangle(player.position, player.size), player.spriteRect, Color.White);
            spriteBatch.Draw(mainAtlas, logoDest, logoSource, Color.White);
        }
    }
}
