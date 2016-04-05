using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static LowRezRogue.MapGeneration;
using LowRezRogue.Interface;

namespace LowRezRogue {

      public struct Player {
        public Point position;

        public Rectangle[] spriteRects;
        public uint spriteIndex;
        public Dictionary<string, Animation> animations;

        public int health;
        public int armor;
        public int damage;

        public Player(Point position) {
            this.position = position;

            spriteRects = new Rectangle[1];
            spriteRects[0] = new Rectangle(0,0,8,8);

            health = 10;
            damage = 5;
            armor = 5;

            spriteIndex = 0;
            animations = new Dictionary<string, Animation>();
            InterfaceManager.UpdateHealth(health);
        }

        public void UpdateAnimation() {

        }

        public void TakeDamage(int damage) {
            health -= damage;
            if(health <= 0)
            {
                //DIE!!!
            } else
            {
                InterfaceManager.UpdateHealth(health);
            }
        }
    }

    public class Enemy {

        public Point position;

        public int health = 1;
        public int armor = 1;
        public int damage = 3;

        public Rectangle[] spriteRects;
        public uint spriteIndex;
        public HashSet<Point> visibleTiles;
        public Animation idleAnim;
        public Animation currentAnim;

        public Enemy(Point position) {
            this.position = position;
            spriteRects = new Rectangle[1];
            spriteRects[0] = new Rectangle(0, 16, 8, 8);
            idleAnim = new Animation();
        }

        public void UpdateAnimation() {

        }
    }

    public class Animation {
        public string name;
        public int animationFrames;
        public bool looping;
        public Rectangle[] rects;

        public Action endOfAnimationAction;

        public Animation() {

        }

        
    }



    public class LowRezRogue : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Camera camera;

        enum GameScene { mainMenu, game, pause, unitInfo }
        GameScene gameScene = GameScene.game;

        enum GameState { playerMove, aiTurn  }
        GameState gameState = GameState.playerMove;

        public Player player;
        public List<Enemy> enemies;
        Tile[,] map;

        int mapPixels = 8;
        int mapWidth = 64;
        int mapHeight = 64;

        Texture2D tileAtlas;
        Texture2D playerAtlas;

        Dictionary<string, Animation> playerAnimations;
        Dictionary<string, Animation> enemyAnimations;

        public LowRezRogue() {
            graphics = new GraphicsDeviceManager(this);
           
            Content.RootDirectory = "Content";
        }

        
        protected override void Initialize() { 
            graphics.PreferredBackBufferHeight = 512;
            graphics.PreferredBackBufferWidth = 512;
            //TargetElapsedTime = TimeSpan.FromTicks(166666);
            graphics.ApplyChanges();

            SpriteBatchEx.GraphicsDevice = GraphicsDevice;

           camera = new Camera(GraphicsDevice.Viewport);
            InterfaceManager.Initialize(Content);

            MapGeneration.InitTileSets();
            GenerateMap();


            lastKeyboardState = Keyboard.GetState();
            base.Initialize();
        }

        void GenerateMap() {
            Random random = new Random();
            map = CreateDungeon(this, mapWidth, mapHeight, false); //CreateOverworld(mapWidth, mapHeight);      //new Tile[mapWidth, mapHeight];
            /*for(int x = 0; x < mapWidth; x++)
            {
                for(int y = 0; y < mapHeight; y++)
                {
                    Rectangle[] rects = { new Rectangle(random.Next(0, 2) * mapPixels, 0 * mapPixels, mapPixels, mapPixels) };
                    map[x, y] = new Tile(rects, true, null);
                }
            }*/
        }


        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileAtlas = Content.Load<Texture2D>("tiles");
            playerAtlas = Content.Load<Texture2D>("player");
        }

        protected override void UnloadContent() {
            spriteBatch.Dispose();
            tileAtlas.Dispose();
            playerAtlas.Dispose();
        }


        double elapsedTime = 0;

        double animationFrameTimer = 0;
        double uiTickTimer = 0;

        KeyboardState lastKeyboardState;
        KeyboardState keyboardState;
        Vector2 camPos;

        protected override void Update(GameTime gameTime) {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            elapsedTime += deltaTime;
            animationFrameTimer += deltaTime;
            uiTickTimer += deltaTime;
       
            keyboardState = Keyboard.GetState();
            camPos = new Vector2();

            if(gameScene == GameScene.mainMenu)
            {

            } else if(gameScene == GameScene.pause)
            {

            } else if(gameScene == GameScene.game && gameState == GameState.playerMove)
            {
                ProcessPlayerTurn();

            } else if(gameScene == GameScene.game && gameState == GameState.aiTurn)
            {
                ProcessEnemyAI();
            }


            if(gameScene == GameScene.game)
            {
                if(uiTickTimer >= 0.0333)
                {
                    InterfaceManager.UpdateTick();
                    uiTickTimer = 0;
                }

                if(animationFrameTimer >= 0.33)
                {


                    for(int x = 0; x < mapWidth; x++)
                    {
                        for(int y = 0; y < mapHeight; y++)
                        {
                            if(map[x, y].animated)
                            {
                                map[x, y].UpdateAnimationIndex();
                            }
                        }
                    }
                    animationFrameTimer = 0.0;
                }
                //came move depending on player position
                //TODO: if moving right, show four tiles to the right, not only three.
                //      if moving left leave it as is -> four free tiles visible to the left und three behind
                if(player.position.X < 4)
                    camPos.X = 32;
                else if(player.position.X > mapWidth - 5)
                    camPos.X = mapWidth * mapPixels - 64 / 2;
                else
                    camPos.X = (player.position.X + 0) * mapPixels;     //pos +1 for 4 tiles to right direction, instead of three 

                if(player.position.Y < 4)
                    camPos.Y = 32;
                else if(player.position.Y > mapHeight - 5)
                    camPos.Y = mapHeight * mapPixels - 64 / 2;
                else
                    camPos.Y = (player.position.Y + 0) * mapPixels;

                camera.Update(camPos);

            }



            lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        void ProcessPlayerTurn() {
            if(keyboardState.IsKeyDown(Keys.F12) && lastKeyboardState.IsKeyUp(Keys.F12))
            {
                if(camera.zoom == 8f)
                    camera.zoom = 1f;
                else if(camera.zoom == 1f)
                    camera.zoom = 8f;
            }

            if(keyboardState.IsKeyDown(Keys.LeftControl) && lastKeyboardState.IsKeyUp(Keys.LeftControl))
            {
                InterfaceManager.ToggleAll();
            }

            if(keyboardState.IsKeyDown(Keys.LeftAlt) && lastKeyboardState.IsKeyUp(Keys.LeftAlt))
            {
                InterfaceManager.ShowDamage(5);
            }


            //player movement
            Point positionCache = player.position;
            bool madeAction = false;

            if(keyboardState.IsKeyDown(Keys.Left) && lastKeyboardState.IsKeyUp(Keys.Left) && player.position.X > 0 && map[player.position.X - 1, player.position.Y].walkable)
            {
                madeAction = true;
                player.position.X -= 1;
            } else if(keyboardState.IsKeyDown(Keys.Right) && lastKeyboardState.IsKeyUp(Keys.Right) && player.position.X < mapWidth - 1 && map[player.position.X + 1, player.position.Y].walkable)
            {
                madeAction = true;
                player.position.X += 1;
            } else if(keyboardState.IsKeyDown(Keys.Up) && lastKeyboardState.IsKeyUp(Keys.Up) && player.position.Y > 0 && map[player.position.X, player.position.Y - 1].walkable)
            {
                madeAction = true;
                player.position.Y -= 1;
            } else if(keyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down) && player.position.Y < mapHeight - 1 && map[player.position.X, player.position.Y + 1].walkable)
            {
                madeAction = true;
                player.position.Y += 1;
            }

            if(map[player.position.X, player.position.Y].tileType.tileType == TileTypes.deadly)
            {
                //Player die!
                Debug.WriteLine("Diiiiiiieee!");
            }


            switch(map[player.position.X, player.position.Y].tileType.interaction)
            {
                case InteractionType.none:
                    {
                        //check if enemy is there, than attack
                        //combat !!!
                        for(int e = 0; e < enemies.Count; e++)
                        {
                            if(enemies[e].position == player.position)
                            {
                                Debug.WriteLine("Attacked enemy");
                                int damage = new Random().Next(player.damage - 1, player.damage + 2);

                                enemies[e].health -= damage;
                                if(enemies[e].health <= 0)
                                {
                                    //show interface
                                    enemies.RemoveAt(e);
                                } else
                                {
                                    InterfaceManager.ShowDamage(damage);
                                    player.position = positionCache;
                                }
                            }
                        }
                        break;
                    }
                case InteractionType.entry:
                    {
                        break;
                    }
                case InteractionType.exit:
                    {
                        break;
                    }
                case InteractionType.shop:
                    {
                        break;
                    }
            }

            if(madeAction)
                EndTurn();

         
        }

        void EndTurn() {
            Debug.WriteLine("End Player Turn");
            gameState = GameState.aiTurn;
        }
       
        void ProcessEnemyAI() {
            Debug.WriteLine("Processing enemy AI");

            for(int i = 0; i < enemies.Count; i++)
            {

            }
            EndAiTurn();
        }

        void EndAiTurn() {
            Debug.WriteLine("End AI Turn");
            gameState = GameState.playerMove;
        }



        protected override void Draw(GameTime gameTime) {
            Window.Title = (1 / gameTime.ElapsedGameTime.TotalSeconds).ToString();

            GraphicsDevice.Clear(Color.TransparentBlack);


            if(gameScene == GameScene.mainMenu)
            {

            } else if(gameScene == GameScene.pause)
            {

            } else if(gameScene == GameScene.game)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.Transform);
                for(int x = 0; x < mapWidth; x++)
                {
                    for(int y = 0; y < mapHeight; y++)
                    {
                        spriteBatch.Draw(tileAtlas, new Rectangle(x * mapPixels, y * mapPixels, mapPixels, mapPixels), map[x, y].spriteRect[map[x,y].spriteIndex], Color.White);

                    }
                }
               
                for(int i = 0; i < enemies.Count; i++)
                {
                    spriteBatch.Draw(playerAtlas, new Rectangle(enemies[i].position * new Point(mapPixels, mapPixels), new Point(mapPixels, mapPixels)),enemies[i].spriteRects[enemies[i].spriteIndex], Color.White);
                }

                spriteBatch.Draw(playerAtlas, new Rectangle(player.position * new Point(mapPixels, mapPixels), new Point(mapPixels, mapPixels)), player.spriteRects[player.spriteIndex], Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.onlyZoom);
                InterfaceManager.Render(spriteBatch);
                spriteBatch.End();
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
