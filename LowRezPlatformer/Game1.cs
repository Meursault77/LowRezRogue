﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using static LowRezRogue.MapGeneration;
using LowRezRogue.Interface;
using System.Globalization;

namespace LowRezRogue {

    public struct Player {
        public Point position;

        public Rectangle spriteRect;
        public Dictionary<string, Animation> animations;
        public Animation currentAnim;

        public int maxHealth;
        public int health;
        public int armor;
        public int damage;
        public int rangeDamage;
        public int visionRange;

        public int sprintBonusMoves;
        public int sprintCoolDown;
        public bool sprintOnCoolDown;

        public Point rangeCombatInitPosition;
        public HashSet<Point> rangeCombatTiles;
        public List<Enemy> enemiesInRange;
        public Enemy targetedEnemy;

        public Item itemWeapon;
        public Item itemRangeWeapon;
        public Item itemArmor;
        
        public bool noDamage;

        Action onDeath;
        Action onArtifactFound;

        public Player(Point position, Dictionary<string, Animation> anims, Action onDeath, Action onArtifactFound) {
            this.position = position;

            spriteRect = new Rectangle(0, 0, 8, 8);

            maxHealth = 22;
            health = 16;
            damage = 2;
            armor = 0;
            rangeDamage = 0;            
            visionRange = 0;

            animations = anims;
            currentAnim = animations["idle"];
            noDamage = false;

            sprintBonusMoves = 2;
            sprintCoolDown = 5;
            sprintOnCoolDown = false;


            rangeCombatInitPosition = new Point(-1,-1);
            rangeCombatTiles = null;
            enemiesInRange = null;
            targetedEnemy = null;

            itemWeapon = null;
            itemRangeWeapon = null;
            itemArmor = null;
            this.onDeath = onDeath;
            this.onArtifactFound = onArtifactFound;

            UpdateAnimation();
            InterfaceManager.UpdateHealth(health);
            InterfaceManager.UpdateArmor(armor);
            InterfaceManager.UpdateDamage(damage);
            InterfaceManager.UpdateRangeDamage(rangeDamage);
        }

        public void UpdateAnimation() {
            spriteRect = currentAnim.GetNextFrame();
            if(spriteRect == Rectangle.Empty)
            {
                currentAnim = animations["idle"].StartAnimation();
                spriteRect = currentAnim.GetNextFrame();
            }
        }

        public bool TakeDamage(int damage) {
            if(noDamage)
                return false;

            health -= damage;
            if(health <= 0)
            {
                //start die animation
                onDeath();
                return true;
                
            } else
            {
                InterfaceManager.UpdateHealth(health);
                InterfaceManager.ToggleHealth(true);
                return false;
            }
        }

        public void TriggerAnimation(string name, Action endAction = null) {
            if(animations.ContainsKey(name))
            {
                currentAnim = animations[name].StartAnimation();
                currentAnim.endOfAnimationAction = endAction;
            }
        }

        public Item EquipItem(Item newItem) {
            Debug.WriteLine($"equip new item {newItem.name}");

            Item oldItem = null;
            switch(newItem.type)
            {
                case ItemType.armor:
                    {
                        if(itemArmor != null)
                        {
                            oldItem = UnequipItem(ItemType.armor);
                        }
                        itemArmor = newItem;
                        armor = newItem.armor;
                        InterfaceManager.UpdateArmor(armor);
                        break;
                    }
                case ItemType.weapon:
                    {
                        if(itemWeapon != null)
                        {
                            oldItem = UnequipItem(ItemType.weapon);
                        }
                        itemWeapon = newItem;
                        damage = newItem.damage;
                        InterfaceManager.UpdateDamage(damage);
                        break;
                    }
                case ItemType.range:
                    {
                        if(itemRangeWeapon != null)
                        {
                            oldItem = UnequipItem(ItemType.range);
                        }
                        itemRangeWeapon = newItem;
                        rangeDamage = newItem.rangeDamage;
                        visionRange = newItem.range;
                        InterfaceManager.UpdateRangeDamage(rangeDamage);
                        break;
                    }
                case ItemType.potion:
                    {
                        //nothing
                        health += newItem.health;
                        if(health > maxHealth)
                            health = maxHealth;

                        InterfaceManager.UpdateHealth(health);
                        break;
                    }
                case ItemType.artifact:
                    {
                        onArtifactFound();
                        break;
                    }
            }
            return oldItem;

        }

        Item UnequipItem(ItemType type) {
            Item oldItem = null;
            switch(type)
            {
                case ItemType.armor:
                    {
                        armor -= itemArmor.armor;
                        oldItem = itemArmor;
                        itemArmor = null;
                        break;
                    }
                case ItemType.weapon:
                    {
                        oldItem = itemWeapon;
                        damage -= itemWeapon.damage;
                        itemWeapon = null;
                        break;
                    }
                case ItemType.range:
                    {
                        oldItem = itemRangeWeapon;
                        rangeDamage -= itemRangeWeapon.rangeDamage;
                        visionRange = 0;
                        itemRangeWeapon = null;
                        break;
                    }
                case ItemType.potion:
                    {
                        //nothing
                        break;
                    }
            }
            return oldItem;
        }     

    }
 
    public struct Animation {
        public string name;
        int currentFrame;
        int animationFrames;
        bool looping;
        public Rectangle[] rects;

        public Action endOfAnimationAction;

        public Animation(string name, bool looping, int animationFrames, Rectangle[] rects) {
            this.name = name;
            this.looping = looping;
            this.animationFrames = animationFrames;
            currentFrame = 0;
            this.rects = rects;
            endOfAnimationAction = null;
        }

        public Animation StartAnimation() {
            currentFrame = 0;
            return this;
        }

        public Rectangle GetNextFrame() {
            if(currentFrame < animationFrames)
            {
                currentFrame++;
                return rects[currentFrame - 1];
            } else
            {
                if(!looping)
                {
                    if(endOfAnimationAction != null)
                    {
                       LowRezRogue.EnqueueActionForEndOfFrame(endOfAnimationAction);
                    }
                    return Rectangle.Empty;
                } else
                {
                    currentFrame = 0;
                    return rects[currentFrame];
                }
            }
        }
    }

    public enum ItemType { armor, weapon, range, potion, artifact}

    public class Item {

        public string name;
        public ItemType type;

        public int health;
        public int armor;
        public int damage;
        public int rangeDamage;
        public int range;

        public Rectangle spriteRect;

        public Item(string name, ItemType type, int health, int armor, int damage, int rangeDamage, int range, Rectangle spriteRect) {
            this.name = name;
            this.type = type;
            this.health = health;
            this.armor = armor;
            this.damage = damage;
            this.rangeDamage = rangeDamage;
            this.range = range;
            this.spriteRect = spriteRect;
        }

    }

    public class Map {

        public int mapWidth;
        public int mapHeight;

        public Tile[,] map;
        public List<Room> rooms;
        public List<Enemy> enemies;
        public List<Item> items;

        public Point playerPositionOnLeave;
        public Dictionary<Point, Map> entries;


        public Map(int width, int height) {
            mapWidth = width;
            mapHeight = height;
            map = new Tile[width, height];
            entries = new Dictionary<Point, Map>();
        }
    }

    public class Enemy {

        EnemyType type;
        public Point position;
        public string name;
        public int health;
        public int armor;
        public int damage;
        public int moveDistance;
        public int range;
        public bool rangeAttacks;

        public int turnsToWait;
        public int turnsToWaitCounter;

        public bool dead = false;
        public bool dying = false;

        public Rectangle spriteRect;
        public HashSet<Point> visibleTiles;

        public Animation currentAnim;
        Dictionary<string, Animation> animations;

        
        public Enemy(EnemyType type, Point position, Dictionary<string, Animation> animations) {
            this.position = position;
            this.type = type;
            health = type.health;
            armor = type.armor;
            damage = type.damage;
            moveDistance = type.moveDistance;
            range = type.range;
            rangeAttacks = type.rangeAttacks;
            turnsToWait = type.turnsPerMove;

            turnsToWaitCounter = new Random().Next(0, turnsToWait + 1);

            currentAnim = animations[$"idle{type.index}"].StartAnimation();
            this.animations = animations;
            spriteRect = new Rectangle(0, 16, 8, 8);
        }

        public void UpdateAnimation() {
            spriteRect = currentAnim.GetNextFrame();
            //spriteRect.Y += type.index * 8;
            if(spriteRect == Rectangle.Empty)
            {
                currentAnim = animations[$"idle{type.index}"].StartAnimation();
                spriteRect = currentAnim.GetNextFrame();
            }
        }

        public void TriggerAnimation(string animName, Action endAction = null) {
            Debug.WriteLine($"Triiger {animName}{type.index}");
            if(animations.ContainsKey(animName + type.index))
            {
                currentAnim = animations[animName + type.index].StartAnimation();               
                endAction += () => { Debug.WriteLine("end of animation is executed right now, enemy"); };
                currentAnim.endOfAnimationAction = endAction;
            }
        }
    }

    

    public class EnemyType {
        public string name;
        public int turnsPerMove;
        public bool rangeAttacks;
        public int moveDistance;

        public int index;

        public int damage;
        public int range;
        public int health;
        public int armor;

        Dictionary<string, Animation> animations;

        public float odds;

        public EnemyType(string name, int index, int turnsPerMove, int moveDistance, bool attacksOnRange, int damage, int range, 
            int health, int armor, float odds, Dictionary<string, Animation> anim) 
            {
            this.name = name;
            this.rangeAttacks = attacksOnRange;
            this.turnsPerMove = turnsPerMove;
            this.moveDistance = moveDistance;

            this.damage = damage;
            this.range = range;
            this.health = health;
            this.armor = armor;

            this.animations = anim;

            this.odds = odds;

            this.index = index;
        }
    }


    public class LowRezRogue : Game {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Camera camera;

        public enum GameScene { mainMenu, game, pause, unitInfo, death }
        public static GameScene gameScene = GameScene.mainMenu;

        public enum GameState { playerMove, aiTurn, playerRangeCombat, sprint, animation }
        GameState gameState = GameState.playerMove;

        enum MoveDirection { None, Up, Right, Down, Left }

        int sprintCounter = 0;

        public Player player;
        //public List<Enemy> enemies;
        
        // [0] -> overworld, [1] - [5] -> dungeons
        public Map[] allMaps;
        Map currentMap;

        Dictionary<string, Item> items;
        EnemyType[] enemyTypes;

        int pixels = 8;

        Random random;

        Texture2D tileAtlas;
        Texture2D playerAtlas;
        Texture2D menuAtlas;
        Texture2D uiAtlas;

        int artifactPiecesFound;
        int maxArtifactPieces = 5;

        public Dictionary<string, Animation> playerAnimations;
        public Dictionary<string, Animation> enemyAnimations;

        MainMenu mainMenu;
        DeathScene deathScene;
        PauseMenu pauseMenu;

        public LowRezRogue() {
            graphics = new GraphicsDeviceManager(this);
           
            Content.RootDirectory = "Content";
        }  
       
        protected override void Initialize() {
            Window.Title = "Head Spin Rogue - #lowrezjam";
            graphics.PreferredBackBufferHeight = 512;
            graphics.PreferredBackBufferWidth = 512;
            //TargetElapsedTime = TimeSpan.FromTicks(166666);
            graphics.ApplyChanges();

            SpriteBatchEx.GraphicsDevice = GraphicsDevice;

            camera = new Camera(GraphicsDevice.Viewport);
            Sound.Initialize(Content);


            InterfaceManager.Initialize(Content);
            LoadAnimationData();
            LoadItems();
            LoadEnemies();

            MapGeneration.InitTileSets();
            GenerateMaps();

            camera.SetPosition(player.position);

            mainMenu = new MainMenu();
            mainMenu.Initialize(Content);
            deathScene = new DeathScene(Exit, StartNewGame);
            deathScene.Initialize();
            pauseMenu = new PauseMenu(Exit, StartNewGame, () => { gameScene = GameScene.game; gameState = GameState.playerMove; });
            pauseMenu.Initialize();

            artifactPiecesFound = 0;

            FadeScreen.Inititalize();

            random = new Random();

            lastKeyboardState = Keyboard.GetState();
            base.Initialize();
        }

        void GenerateMaps() {
            Random random = new Random();

            allMaps = new Map[6];
            allMaps[0] = CreateDungeon(this, 48, 48, 33, items, enemyTypes, isOverworld: true);

            allMaps[1] = CreateDungeon(this, 48, 48, 35, items, enemyTypes);
            allMaps[2] = CreateDungeon(this, 32, 32, 36, items, enemyTypes);
            allMaps[3] = CreateDungeon(this, 64, 64, 41, items, enemyTypes);
            allMaps[4] = CreateDungeon(this, 48, 48, 40, items, enemyTypes);
            allMaps[5] = CreateDungeon(this, 48, 48, 39, items, enemyTypes);
            currentMap = allMaps[0];

            int j = 1;
            HashSet<Point> entries = new HashSet<Point>();
            foreach(KeyValuePair<Point, Map> pair in allMaps[0].entries)
            {
                entries.Add(pair.Key);
            }
            foreach(Point key in entries)
            {
                allMaps[0].entries[key] = allMaps[j];
                j++;
            }


            player = new Player(currentMap.playerPositionOnLeave, playerAnimations, OnPlayerDeath, OnArtifactFound);
        }

        void LoadAnimationData() {
            playerAnimations = new Dictionary<string, Animation>();

            XmlDocument animXml = new XmlDocument();
            animXml.Load("Content/XML/Animations.xml");

            foreach(XmlNode node in animXml.SelectNodes("animations/playerAnimations/anim"))
            {
                bool looping = Convert.ToBoolean(node.Attributes["looping"].Value);
                string name = node.Attributes["name"].Value;

                XmlNodeList frames = node.SelectNodes("frame");
                
                var rects = new Rectangle[frames.Count];

                for(int i = 0; i < frames.Count; i++)
                {
                    int x = int.Parse(frames[i].Attributes["x"].Value);
                    int y = int.Parse(frames[i].Attributes["y"].Value);
                    rects[i] = new Rectangle(x * pixels, y * pixels, pixels, pixels);
                }
                var newAnim = new Animation(name, looping, frames.Count, rects);
                playerAnimations.Add(name, newAnim);
            }

            enemyAnimations = new Dictionary<string, Animation>();

            foreach(XmlNode node in animXml.SelectNodes("animations/enemyAnimations/anim"))
            {
                bool looping = Convert.ToBoolean(node.Attributes["looping"].Value);
                string name = node.Attributes["name"].Value;

                XmlNodeList frames = node.SelectNodes("frame");
                
                var rects = new Rectangle[frames.Count];

                for(int i = 0; i < frames.Count; i++)
                {
                    int x = int.Parse(frames[i].Attributes["x"].Value);
                    int y = int.Parse(frames[i].Attributes["y"].Value);
                    rects[i] = new Rectangle(x * pixels, y * pixels, pixels, pixels);
                }
                var newAnim = new Animation(name, looping, frames.Count, rects);
                enemyAnimations.Add(name, newAnim);
            }
        }


        //TODO: Load Animations per enemyType
        //maybe place the animations per enemy type over each other 
        //and add 8 pixels to Y per enemyType index. this way i dont 
        //have to change the animation system...well i have to change it..

        void LoadEnemies() {
            XmlDocument enemyXml = new XmlDocument();
            enemyXml.Load("Content/XML/Enemies.xml");
            XmlNodeList enemyNodes = enemyXml.SelectNodes("enemyTypes/enemy");

            enemyTypes = new EnemyType[enemyNodes.Count];

            for(int i = 0; i < enemyNodes.Count; i++)
            {
                XmlNode node = enemyNodes[i];

                int turnsPerMove = int.Parse(node.Attributes["turnsPerMove"].Value);
                int moveDist = int.Parse(node.Attributes["moveDistance"].Value);
                int health = int.Parse(node.Attributes["health"].Value);
                int armor = int.Parse(node.Attributes["armor"].Value);
                int damage = int.Parse(node.Attributes["damage"].Value);
                int range = int.Parse(node.Attributes["range"].Value);

                bool rangeAttacks = bool.Parse(node.Attributes["rangeAttacks"].Value);

                float odds = float.Parse(node.Attributes["odds"].Value, CultureInfo.InvariantCulture);

                enemyTypes[i] = new EnemyType(enemyNodes[i].Attributes["name"].Value, i, turnsPerMove, moveDist, rangeAttacks, damage, range, health, armor, odds, enemyAnimations);
            }  
        }

        void LoadItems() {
            
            XmlDocument itemsXml = new XmlDocument();
            itemsXml.Load("Content/XML/Items.xml");

            XmlNodeList itemNodes = itemsXml.SelectNodes("items/item");

            items = new Dictionary<string, Item>();

            for(int i = 0; i < itemNodes.Count; i++)
            {
                XmlNode node = itemNodes[i];
                string name = node.Attributes["name"].Value;
                int health = int.Parse(node.Attributes["health"].Value);
                int armor = int.Parse(node.Attributes["armor"].Value);
                int damage = int.Parse(node.Attributes["damage"].Value);
                int rangeDamage = int.Parse(node.Attributes["rangeDamage"].Value);
                int range = int.Parse(node.Attributes["range"].Value);

                ItemType type = (ItemType)Enum.Parse(typeof(ItemType), node.Attributes["type"].Value);

                int x = int.Parse(node.Attributes["x"].Value);
                int y = int.Parse(node.Attributes["y"].Value);

                items.Add(name, new Item(name, type, health, armor, damage, rangeDamage, range, new Rectangle(x * pixels, y * pixels, pixels, pixels)));
            }

        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileAtlas = Content.Load<Texture2D>("tiles");
            playerAtlas = Content.Load<Texture2D>("player");
            menuAtlas = Content.Load<Texture2D>("Menus");
            uiAtlas = Content.Load<Texture2D>("UI");

        }

        protected override void UnloadContent() {
            spriteBatch.Dispose();
            tileAtlas.Dispose();
            playerAtlas.Dispose();
            menuAtlas.Dispose();
            Sound.UnloadContent();
        }


        double elapsedTime = 0;

        double animationFrameTimer = 0;
        double uiTickTimer = 0;


        static Queue<Action> executeEndOfFrame = new Queue<Action>();

        public static void EnqueueActionForEndOfFrame(Action action) {
            executeEndOfFrame.Enqueue(action);
        }


        KeyboardState lastKeyboardState;
        KeyboardState keyboardState;

        protected override void Update(GameTime gameTime) {
            //if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            elapsedTime += deltaTime;
            animationFrameTimer += deltaTime;
            uiTickTimer += deltaTime;

            keyboardState = Keyboard.GetState();

            if(gameScene == GameScene.game)
            {
                if(keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))
                {
                    if(gameState == GameState.sprint)
                    {
                        gameState = GameState.playerMove;
                        InterfaceManager.CloseSprint();
                    }

                    if(gameState == GameState.playerMove)
                    {
                        gameState = GameState.playerRangeCombat;
                        InitRangeCombatTiles();
                        InterfaceManager.ToggleRangeLogo(player.enemiesInRange.Count > 0);
                        player.currentAnim = playerAnimations["idleRange"].StartAnimation();
                    } else if(gameState == GameState.playerRangeCombat)
                    {
                        StopRangeCombat();
                    }
                }
                if(keyboardState.IsKeyDown(Keys.S) && lastKeyboardState.IsKeyUp(Keys.S) && !player.sprintOnCoolDown)
                {
                    if(gameState == GameState.playerRangeCombat)
                    {
                        StopRangeCombat();
                    }

                    if(gameState == GameState.playerMove)
                    {
                        gameState = GameState.sprint;
                        InterfaceManager.ShowSprint();
                    } else if(gameState == GameState.sprint)
                    {
                        gameState = GameState.playerMove;
                        InterfaceManager.CloseSprint();
                    }
                }


                if((keyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
                    || (keyboardState.IsKeyDown(Keys.F10) && lastKeyboardState.IsKeyUp(Keys.F10)))
                {
                    gameScene = GameScene.pause;
                    pauseMenu.pauseState = PauseMenu.PauseState.pause;
                    pauseMenu.GetArtifacts(artifactPiecesFound);
                    return;
                }

                if(uiTickTimer >= 0.0333)
                {
                    camera.Update(player.position, currentMap.mapWidth, currentMap.mapHeight);
                    InterfaceManager.UpdateTick();
                    uiTickTimer = 0;
                }

                if(animationFrameTimer >= 0.166)
                {
                    player.UpdateAnimation();
                    foreach(Enemy enem in currentMap.enemies)
                    {
                        enem.UpdateAnimation();
                    }

                    for(int x = 0; x < currentMap.mapWidth; x++)
                    {
                        for(int y = 0; y < currentMap.mapHeight; y++)
                        {
                            if(currentMap.map[x, y].animated)
                            {
                                currentMap.map[x, y].UpdateAnimationIndex();
                            }
                        }
                    }
                    animationFrameTimer = 0.0;
                }



                //General game input, not player action specific
                /*if(keyboardState.IsKeyDown(Keys.F12) && lastKeyboardState.IsKeyUp(Keys.F12))
                {
                    if(camera.zoom == 8f)
                        camera.zoom = 1f;
                    else if(camera.zoom == 1f)
                        camera.zoom = 8f;
                }*/
                /*if(keyboardState.IsKeyDown(Keys.F9) && lastKeyboardState.IsKeyUp(Keys.F9))
                {
                    artifactPiecesFound = maxArtifactPieces;
                }*/
                if(keyboardState.IsKeyDown(Keys.LeftControl) && lastKeyboardState.IsKeyUp(Keys.LeftControl))
                {
                    InterfaceManager.ToggleAll();
                }
                /*if(keyboardState.IsKeyDown(Keys.LeftAlt) && lastKeyboardState.IsKeyUp(Keys.LeftAlt))
                {
                    InterfaceManager.ShowDamage(5);
                }*/
                if(keyboardState.IsKeyDown(Keys.H) && lastKeyboardState.IsKeyUp(Keys.H))
                {
                    InterfaceManager.ToggleHealth();
                }
                /*if(keyboardState.IsKeyDown(Keys.F1) && lastKeyboardState.IsKeyUp(Keys.F1))
                {
                    player.noDamage = !player.noDamage;
                }*/
                /*if(keyboardState.IsKeyDown(Keys.F5) && lastKeyboardState.IsKeyUp(Keys.F5))
                {
                    if(currentMap == allMaps[0])
                        currentMap = allMaps[1];
                    else if(currentMap == allMaps[1])
                        currentMap = allMaps[2];
                    else if(currentMap == allMaps[2])
                        currentMap = allMaps[3];
                    else if(currentMap == allMaps[3])
                        currentMap = allMaps[4];
                    else if(currentMap == allMaps[4])
                        currentMap = allMaps[5];
                    else if(currentMap == allMaps[5])
                        currentMap = allMaps[0];
                }*/
            }



            if(gameScene == GameScene.mainMenu)
            {
                mainMenu.Update(deltaTime, keyboardState, lastKeyboardState);
            } else if(gameScene == GameScene.pause)
            {
                pauseMenu.Update(deltaTime, keyboardState, lastKeyboardState);
            } else if(gameScene == GameScene.death)
            {
                deathScene.Update(deltaTime, keyboardState, lastKeyboardState);
            } else if(gameScene == GameScene.game && gameState == GameState.animation)
            {

            } else if(gameScene == GameScene.game && (gameState == GameState.playerMove || gameState == GameState.sprint))
            {
                ProcessPlayerTurn();
            } else if(gameScene == GameScene.game && gameState == GameState.playerRangeCombat)
            {
                ProcessPlayerRangeCombat();
            } else if(gameScene == GameScene.game && gameState == GameState.aiTurn)
            {
                ProcessEnemyAI();
            }

            while(executeEndOfFrame.Count > 0)
            {
                executeEndOfFrame.Dequeue()();
            }

            FadeScreen.Update(deltaTime);

            lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        void StopRangeCombat(bool attacked = false) {
            gameState = GameState.playerMove;
            InterfaceManager.ToggleRangeLogo(false);

            if(!attacked)
                player.currentAnim = playerAnimations["idle"].StartAnimation();

            player.targetedEnemy = null;
            player.enemiesInRange = null;
        }


        void InitRangeCombatTiles() {
            if(player.position != player.rangeCombatInitPosition)
            {
                player.rangeCombatTiles = new HashSet<Point>();
                player.rangeCombatInitPosition = player.position;

                HashSet<Point> visited = new HashSet<Point>();
                Queue<Point> toVisit = new Queue<Point>();
                toVisit.Enqueue(player.position);

                while(toVisit.Count > 0)
                {
                    Point p = toVisit.Dequeue();

                    var dist = Math.Sqrt((p.X - player.position.X) * (p.X - player.position.X) + (p.Y - player.position.Y) * (p.Y - player.position.Y));

                    if(!visited.Contains(p) && dist <= player.visionRange)
                    {
                        if(IsInMapRange(p.X, p.Y) && currentMap.map[p.X, p.Y].walkable)
                            player.rangeCombatTiles.Add(p);

                        if(!visited.Contains(new Point(p.X + 1, p.Y)))
                            toVisit.Enqueue(new Point(p.X + 1, p.Y));
                        if(!visited.Contains(new Point(p.X - 1, p.Y)))
                            toVisit.Enqueue(new Point(p.X - 1, p.Y));
                        if(!visited.Contains(new Point(p.X, p.Y + 1)))
                            toVisit.Enqueue(new Point(p.X, p.Y + 1));
                        if(!visited.Contains(new Point(p.X, p.Y - 1)))
                            toVisit.Enqueue(new Point(p.X, p.Y - 1));

                        visited.Add(p);
                    }

                }
            }

            player.enemiesInRange = new List<Enemy>();
            foreach(Point p in player.rangeCombatTiles)
            {
                for(int i = 0; i < currentMap.enemies.Count; i++)
                {
                    if(p == currentMap.enemies[i].position && !currentMap.enemies[i].dying)
                        player.enemiesInRange.Add(currentMap.enemies[i]);
                }
            }

            if(player.enemiesInRange.Count > 0)
                player.targetedEnemy = player.enemiesInRange[0];

            Debug.WriteLine($"Enemies in Ragen: {player.enemiesInRange.Count}");
        }

        void ProcessPlayerRangeCombat() {
            if(player.enemiesInRange.Count > 0)
            {
                if(keyboardState.IsKeyDown(Keys.Left) && lastKeyboardState.IsKeyUp(Keys.Left))
                {
                    Sound.PlayClick();
                    if(player.enemiesInRange.IndexOf(player.targetedEnemy) == 0)
                        player.targetedEnemy = player.enemiesInRange[player.enemiesInRange.Count - 1];
                    else
                        player.targetedEnemy = player.enemiesInRange[player.enemiesInRange.IndexOf(player.targetedEnemy) - 1];
                }
                if(keyboardState.IsKeyDown(Keys.Right) && lastKeyboardState.IsKeyUp(Keys.Right))
                {
                    Sound.PlayClick();
                    if(player.enemiesInRange.IndexOf(player.targetedEnemy) == player.enemiesInRange.Count - 1)
                        player.targetedEnemy = player.enemiesInRange[0];
                    else
                        player.targetedEnemy = player.enemiesInRange[player.enemiesInRange.IndexOf(player.targetedEnemy) + 1];
                }
                if(keyboardState.IsKeyDown(Keys.A) && lastKeyboardState.IsKeyUp(Keys.A))
                {
                    PlayerAttack(player.targetedEnemy, Point.Zero, true);
                    StopRangeCombat(true);
                    EndTurn();
                }


            }

        }

        int CalculateDamageToPlayer(Enemy attacking) {  
            int damage = random.Next(attacking.damage - 1, attacking.damage + 2) - player.armor;
            if(damage < 2)
                damage = 1;

            return damage;
        }

        void PlayerAttack(Enemy enem, Point posCache, bool range = false) {
            Debug.WriteLine("Player attacking an enemy");

            if(range)
            {
                Sound.PlayHit();
                player.TriggerAnimation("rangeAttack");
                int damage;
                if(AreTilesNeighbours(enem.position, player.position))
                    damage = 1;
                else
                    damage = random.Next(player.rangeDamage - 1, player.rangeDamage + 2) - enem.armor;

                if(damage < 1)
                    damage = 1;
                
                enem.health -= damage;

                if(enem.health <= 0)
                {
                    InterfaceManager.ShowDamage(666);       //666 is the code for death UI   
                    enem.dying = true;
                    enem.TriggerAnimation("die", () => { enem.dead = true; currentMap.enemies.Remove(enem); });
                    SpawnItem(enem.position);
                } else
                {
                    InterfaceManager.ShowDamage(damage);
                }

            } else
            {
                Sound.PlayHit();
                player.TriggerAnimation("attack");
                int damage = random.Next(player.damage - 1, player.damage + 2) - enem.armor;
                if(damage < 1)
                    damage = 1;
                enem.health -= damage;

                if(enem.health <= 0)
                {
                    //show interface
                    InterfaceManager.ShowDamage(666);       //666 is the code for death UI
                    enem.dying = true;
                    enem.TriggerAnimation("die", () => { enem.dead = true; currentMap.enemies.Remove(enem); });
                    SpawnItem(enem.position);
                    player.position = posCache;
                } else
                {
                    InterfaceManager.ShowDamage(damage);
                    player.position = posCache;
                }
            }
        }

        void ProcessPlayerTurn() {


            bool madeAction = false;

            //Debug.WriteLine($"Process player turn. {gameState}");
            //player movement
            Point positionCache = player.position;


            if(keyboardState.IsKeyDown(Keys.Left) && lastKeyboardState.IsKeyUp(Keys.Left) && 
                player.position.X > 0 && currentMap.map[player.position.X - 1, player.position.Y].walkable)
            {
                madeAction = true;
                player.position.X -= 1;
                if(IsTileFree(player.position))
                    positionCache = player.position;

                if(gameState == GameState.sprint && currentMap.map[player.position.X - 1, player.position.Y].walkable)
                {
                    player.position.X -= player.sprintBonusMoves;
                    player.TriggerAnimation("sprintLeft");
                } else
                    player.TriggerAnimation("walk");
            } 
            else if(keyboardState.IsKeyDown(Keys.Right) && lastKeyboardState.IsKeyUp(Keys.Right) && 
                player.position.X < currentMap.mapWidth - 1 && currentMap.map[player.position.X + 1, player.position.Y].walkable)
            {
                madeAction = true;
                player.position.X += 1;
                if(IsTileFree(player.position))
                    positionCache = player.position;

                if(gameState == GameState.sprint && currentMap.map[player.position.X + 1, player.position.Y].walkable)
                {
                    player.position.X += player.sprintBonusMoves;
                    player.TriggerAnimation("sprintRight");
                } else
                    player.TriggerAnimation("walk");
            } 
            else if(keyboardState.IsKeyDown(Keys.Up) && lastKeyboardState.IsKeyUp(Keys.Up) && 
                player.position.Y > 0 && currentMap.map[player.position.X, player.position.Y - 1].walkable)
            {
                madeAction = true;
                player.position.Y -= 1;
                if(IsTileFree(player.position))
                    positionCache = player.position;

                if(gameState == GameState.sprint && currentMap.map[player.position.X, player.position.Y - 1].walkable)
                {
                    player.position.Y -= player.sprintBonusMoves;
                    player.TriggerAnimation("sprintUp");
                } else
                    player.TriggerAnimation("walk");
            } 
            else if(keyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down) && 
                player.position.Y < currentMap.mapHeight - 1 && currentMap.map[player.position.X, player.position.Y + 1].walkable)
            {
                madeAction = true;
                player.position.Y += 1;
                if(IsTileFree(player.position))
                    positionCache = player.position;

                if(gameState == GameState.sprint && currentMap.map[player.position.X, player.position.Y + 1].walkable)
                {
                    player.position.Y += player.sprintBonusMoves;
                    player.TriggerAnimation("sprintDown");
                } else
                    player.TriggerAnimation("walk");
            } 
            else if(keyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter))
            {
                Sound.PlayClick();
                EndTurn();
                return;
            }

            if(!madeAction)
            {
                return;
            } 
                

            if(currentMap.map[player.position.X, player.position.Y].tileType.tileType == TileTypes.deadly)
            {
                //Player die!
                Debug.WriteLine("Diiiiiiieee!");
            }

            if(currentMap.map[player.position.X, player.position.Y].itemOnTop != null)
            {
                Item itemToPlace = player.EquipItem(currentMap.map[player.position.X, player.position.Y].itemOnTop);
                currentMap.map[player.position.X, player.position.Y].itemOnTop = null;
                if(itemToPlace != null)
                    PlaceItemOnMap(itemToPlace, positionCache);
            }

            switch(currentMap.map[player.position.X, player.position.Y].tileType.interaction)
            {
                case InteractionType.none:
                    {
                        //check if enemy is there, than attack
                        //combat !!!
                        for(int e = 0; e < currentMap.enemies.Count; e++)
                        {
                            if(currentMap.enemies[e].position == player.position && !currentMap.enemies[e].dying)
                            {                               
                                PlayerAttack(currentMap.enemies[e], positionCache);                              
                            }
                        }
                        break;
                    }
                case InteractionType.entry:
                    {
                        TransitionToMap(currentMap.entries[player.position], positionCache);
                        break;
                    }
                case InteractionType.castle:
                    {
                        Debug.WriteLine("Theeee Castle!!!");
                        player.position = positionCache;
                        OnCastleAttack();
                        break;
                    }
                case InteractionType.shop:
                    {
                        break;
                    }
            }

            if(madeAction && gameState != GameState.animation)
            {
                EndTurn();
                return;
            }

        }
       
        void ProcessEnemyAI() {
            Debug.WriteLine("Processing enemy AI");
            for(int i = 0; i < currentMap.enemies.Count; i++)
            {
                var enem = currentMap.enemies[i];
                if(enem.dead ||enem.dying)
                    continue;

                if (enem.turnsToWaitCounter < enem.turnsToWait)
                {
                    enem.turnsToWaitCounter++;
                    continue;
                }
                else
                    enem.turnsToWaitCounter = 0;


                bool tookAction = false;
                Point posCache = enem.position;
                int x = enem.position.X;
                int y = enem.position.Y;
                
                for(int sX = x - 3; sX <= x + 3; sX++)
                {
                    for(int sY = y - 3; sY <= y + 3; sY++)
                    {
                        if(!tookAction && player.position.X == sX && player.position.Y == sY)
                        {
                            tookAction = true;
                            if(enem.rangeAttacks && TileDistance(enem.position, player.position) <= enem.range) {
                                Debug.WriteLine("Enemy range attacks player");
                                int damage = CalculateDamageToPlayer(enem);
                                if(AreTilesNeighbours(enem.position, player.position))
                                    damage = 1;
                                enem.TriggerAnimation("attack");
                                player.TriggerAnimation("getHit");
                                Sound.PlayHit();
                                if (player.TakeDamage(damage))
                                    return;

                                if (player.health > 0)
                                    enem.position = posCache;
                                else
                                    enem.position = player.position;                            
                            }
                            else if(AreTilesNeighbours(player.position, enem.position))
                            {
                                //attack players
                                Debug.WriteLine("Enemy attacks Player");
                                int damage = CalculateDamageToPlayer(enem);
                                enem.TriggerAnimation("attack");
                                player.TriggerAnimation("getHit");
                                Sound.PlayHit();
                                if(player.TakeDamage(damage))
                                {           //returns true, if player dies!
                                    return;
                                }
                                if(player.health > 0)
                                    enem.position = posCache;
                                else
                                    enem.position = player.position;
                            } else
                            {
                                //Move towards player.
                                Debug.WriteLine("Enemy moving towards player");                                                            
                                int remainingMovement = enem.moveDistance;
                                tookAction = false;
                                while (remainingMovement > 0)
                                {
                                    int xDist = player.position.X - enem.position.X;
                                    int yDist = player.position.Y - enem.position.Y;
                                    int xFactor = 1;
                                    int yFactor = 1;
                                    if (xDist < 0)
                                        xFactor = -1;
                                    if (yDist < 0)
                                        yFactor = -1;
                                    if (Math.Abs(xDist) >= Math.Abs(yDist)) {
                                        if(IsTileFree(enem.position.X + xFactor, enem.position.Y)) {
                                            enem.TriggerAnimation("walk");
                                            enem.position.X += xFactor;
                                            remainingMovement--;
                                            tookAction = true;
                                            continue;
                                        }
                                    }

                                    if (IsTileFree(enem.position.X, enem.position.Y + yFactor))
                                    {
                                        enem.position.Y += yFactor;
                                        remainingMovement--;
                                        tookAction = true;
                                        continue;
                                    }
                                    remainingMovement--;

                                }
                            }
                        } 
                    }
                }

                if(tookAction)
                    continue;

                //Move randomly
                MoveDirection moveDir = (MoveDirection)random.Next(1, 6);
                switch(moveDir)
                {
                    case MoveDirection.None:
                        {
                            break;
                        }
                    case MoveDirection.Up:
                        {
                            EnemMoveTo(enem, x, y - enem.moveDistance);
                            break;
                        }
                    case MoveDirection.Right:
                        {
                            EnemMoveTo(enem, x + enem.moveDistance, y);
                            break;
                        }
                    case MoveDirection.Down:
                        {
                            EnemMoveTo(enem, x, y + enem.moveDistance);
                            break;
                        }
                    case MoveDirection.Left:
                        {
                            EnemMoveTo(enem, x - enem.moveDistance, y);
                            break;
                        }
                }

                if(tookAction)
                    continue;

            }
            EndAiTurn();
        }

        #region small stuff

        void OnCastleAttack() {
            FadeScreen.StartFadeScreen();
            if(artifactPiecesFound == maxArtifactPieces)
            {
                Sound.PlayLostVictory(false);
                deathScene.SetDeathOrVictory(true);
                FadeScreen.StartFadeScreen(0.3, () => {
                    gameScene = GameScene.death; });
            } else if(artifactPiecesFound < maxArtifactPieces)
            {
                Sound.PlayLostVictory(true);
                deathScene.SetDeathOrVictory(false) ;
                FadeScreen.StartFadeScreen(0.3, () => {
                    gameScene = GameScene.death; });
            }
        }

        void OnArtifactFound() {
            Sound.Play("artifact_pickup");
            artifactPiecesFound += 1;
            if(artifactPiecesFound > maxArtifactPieces) {
                Debug.WriteLine("Too many artifact pieces spawned...");
            }
            InterfaceManager.ShowArtifact(artifactPiecesFound);
        }

        void TransitionToMap(Map transitionTo, Point positionCache) {
            gameState = GameState.animation;
            Debug.WriteLine(gameState);
            FadeScreen.StartFadeScreen(0.3,
                () => {
                    currentMap.playerPositionOnLeave = positionCache;
                    currentMap = transitionTo;
                    player.position = transitionTo.playerPositionOnLeave;
                    camera.JumpToPosition(player.position);
                }, () => { gameState = GameState.playerMove; });
        }

        void EndTurn() {
            Debug.WriteLine("End Player Turn");

            if(player.sprintOnCoolDown)
            {
                sprintCounter += 1;
                if(sprintCounter == player.sprintCoolDown)
                {
                    InterfaceManager.ActivateSprint(true);
                    player.sprintOnCoolDown = false;
                }
            }

            if(gameState == GameState.sprint)
            {
                player.sprintOnCoolDown = true;
                sprintCounter = 0;
                InterfaceManager.CloseSprint();
                InterfaceManager.ActivateSprint(false);
            }
            gameState = GameState.aiTurn;
        }

        void EndAiTurn() {
            Debug.WriteLine("End AI Turn");

            gameState = GameState.playerMove;
        }

        void StartNewGame() {
            Debug.WriteLine("Start new Game");
            Initialize();
            gameState = GameState.playerMove;
            gameScene = GameScene.game;
        }

        void OnPlayerDeath() {
            player.TriggerAnimation("die", null);
            Sound.PlayLostVictory(true);
            gameState = GameState.animation;
            FadeScreen.StartFadeScreen(0.3, () => {
                gameScene = GameScene.death; });
           
        }

        void SpawnItem(Point pos) {
            Item itemToSpawn = null;
            double r = random.NextDouble();
            double odds = 0.35;
            if(player.health <= 5)
                odds = 0.7;          

            Debug.WriteLine($"Item Spawn random: {r}");

            if(r <= odds)
            {
                if(player.health <= 5)
                {
                    if(r <= odds / 5.0)
                    {
                        itemToSpawn = items["fancyPotion"];
                    } else if(r <= odds / 2.0)
                    {
                        itemToSpawn = items["okayPotion"];
                    } else
                        itemToSpawn = items["oldPotion"];                   
                }

                if(itemToSpawn == null)
                {
                    ItemType type = (ItemType)random.Next(0, 4);

                    switch(type)
                    {
                        case ItemType.armor:
                            {
                                if(r <= odds / 10.0)
                                {
                                    itemToSpawn = items["fancyArmor"];
                                } else if(r <= odds / 2.0)
                                {
                                    itemToSpawn = items["okayArmor"];
                                } else
                                    itemToSpawn = items["oldArmor"];
                                break;
                            }
                        case ItemType.weapon:
                            {
                                if(r <= odds / 10.0)
                                {
                                    itemToSpawn = items["fancySword"];
                                } else if(r <= odds / 2.0)
                                {
                                    itemToSpawn = items["okaySword"];
                                } else
                                    itemToSpawn = items["oldSword"];
                                break;
                            }
                        case ItemType.range:
                            {
                                if(r <= odds / 10.0)
                                {
                                    itemToSpawn = items["fancyRange"];
                                } else if(r <= odds / 2.0)
                                {
                                    itemToSpawn = items["okayRange"];
                                } else
                                    itemToSpawn = items["oldRange"];
                                break;
                            }
                        case ItemType.potion:
                            {
                                if(r <= odds / 10.0)
                                {
                                    itemToSpawn = items["fancyPotion"];
                                } else if(r <= odds / 2.0)
                                {
                                    itemToSpawn = items["okayPotion"];
                                } else
                                    itemToSpawn = items["oldPotion"];
                                break;
                            }
                    }

                }
            } else
                return;

            Debug.WriteLine($"Spawning {itemToSpawn.name}");

            Point[] arr = { new Point(pos.X + 1, pos.Y), new Point(pos.X, pos.Y + 1), new Point(pos.X - 1, pos.Y), new Point(pos.X, pos.Y - 1) };
            foreach(Point p in arr)
            {
                if(IsTileFree(p) && currentMap.map[p.X, p.Y].itemOnTop == null)
                {
                    currentMap.map[p.X, p.Y].itemOnTop = itemToSpawn;
                    break;
                }
            }

        }

        public void PlaceItemOnMap(Item item, Point pos) {
            currentMap.map[pos.X, pos.Y].itemOnTop = item;
        }
        
        bool EnemMoveTo(Enemy enem, int x, int y) {
            if (IsInMapRange(x, y) && IsTileFree(x, y))
            {
                enem.position = new Point(x, y);
                return true;
            }
            else
                return false;
        }

        double TileDistance(Point a, Point b) {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        bool IsInMapRange(int x, int y) {
            return x >= 0 && x < currentMap.mapWidth && y >= 0 && y < currentMap.mapHeight;
        }

        bool IsInMapRange(Point p) {
            return IsInMapRange(p.X, p.Y);
        }

        bool IsTileFree(int x, int y) {
            return IsTileFree(new Point(x, y));
        }

        bool IsTileFree(Point p) {
            if(!IsInMapRange(p))
                return false;

            if(!currentMap.map[p.X,p.Y].walkable || player.position == p)
            {
                return false;
            }
            for(int i = 0; i < currentMap.enemies.Count; i++)
            {
                if(currentMap.enemies[i].position == p && !currentMap.enemies[i].dying)
                    return false;
            }
            return true;
        }

        bool AreTilesNeighbours(Point p1, Point p2) {
            return AreTilesNeighbours(p1.X, p1.Y, p2.X, p2.Y);
        }

        bool AreTilesNeighbours(int x1, int y1, int x2, int y2) {
            if(!IsInMapRange(x1, y1) || !IsInMapRange(x2, y2))
                return false;

            if((x1 == x2 + 1 || x1 == x2 - 1) && y1 == y2)
                return true;
            if((y1 == y2 + 1 || y1 == y2 - 1) && x1 == x2)
                return true;

            return false;
        }

        #endregion

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.TransparentBlack);
            if(gameScene == GameScene.mainMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.onlyZoom);
                mainMenu.Render(spriteBatch);
            } 
            else if(gameScene == GameScene.pause)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.onlyZoom);
                pauseMenu.Render(spriteBatch, menuAtlas, uiAtlas);
            }
            else if(gameScene == GameScene.death)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.onlyZoom);
                deathScene.Render(spriteBatch, menuAtlas);
            } 
            else if(gameScene == GameScene.game)
            {

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.Transform);

                for(int x = 0; x < currentMap.mapWidth; x++)
                {
                    for(int y = 0; y < currentMap.mapHeight; y++)
                    {
                        spriteBatch.Draw(tileAtlas, new Rectangle(x * pixels, y * pixels, pixels, pixels), currentMap.map[x, y].spriteRect[currentMap.map[x,y].spriteIndex], Color.White);
                        if(currentMap.map[x,y].itemOnTop != null)
                        {
                            spriteBatch.Draw(tileAtlas, new Rectangle(x * pixels, y * pixels, pixels, pixels), currentMap.map[x, y].itemOnTop.spriteRect, Color.White);
                        }
                    }
                }

                if(gameState == GameState.playerRangeCombat)
                {
                    foreach(Point p in player.rangeCombatTiles)
                    {
                        spriteBatch.Draw(tileAtlas, new Rectangle(p.X * pixels, p.Y * pixels, pixels, pixels), new Rectangle(0 * pixels, 8 * pixels, 8, 8), Color.White);
                    }
                    if(player.targetedEnemy != null)
                    {
                        spriteBatch.Draw(tileAtlas, new Rectangle((player.targetedEnemy.position.X * pixels) - 1, (player.targetedEnemy.position.Y * pixels) - 1, 10, 10), new Rectangle(1 * pixels, 8 * pixels, 10,10), Color.White);
                    }
                }
               
                for(int i = 0; i < currentMap.enemies.Count; i++)
                {
                    if(!currentMap.enemies[i].dead)
                        spriteBatch.Draw(playerAtlas, new Rectangle(currentMap.enemies[i].position * new Point(pixels, pixels), new Point(pixels, pixels)), currentMap.enemies[i].spriteRect, Color.White);
                }

                spriteBatch.Draw(playerAtlas, new Rectangle(player.position * new Point(pixels, pixels), new Point(pixels, pixels)), player.spriteRect, Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, transformMatrix: camera.onlyZoom);
                InterfaceManager.Render(spriteBatch, uiAtlas);
                spriteBatch.End();
            }
            spriteBatch.End();

            FadeScreen.Render(spriteBatch, menuAtlas);

            base.Draw(gameTime);
        }

    }
}
