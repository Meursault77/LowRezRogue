using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowRezRogue.Interface {

    public enum UiTransitionState { open, transitionClose, closed, transitionOpen }

    public class UIObject {
        public Point openPosition;
        public Point closedPosition;
        public Point currentPosition;
        public Point size;

        public Rectangle spriteRect;

        public UiTransitionState transitionState = UiTransitionState.open;
        public int transitionSpeed;

        bool closable = true;
        bool toClose;
        int toCloseTimer;
        int toCloseAfterTicks;

        public UIObject(Point openPosition, Point closedPosition, Point size, UiTransitionState startState, Rectangle spriteRect, int transitionSpeed = 1) {
            this.openPosition = openPosition;
            this.closedPosition = closedPosition;
            this.size = size;

            transitionState = startState;
            this.transitionSpeed = transitionSpeed;

            this.spriteRect = spriteRect;

            if(startState == UiTransitionState.open)
                currentPosition = openPosition;
            else
                currentPosition = this.closedPosition;


        }

        public void Update() {

            if(toClose)
            {
                toCloseTimer += 1;
                if(toCloseTimer >= toCloseAfterTicks)
                {
                    transitionState = UiTransitionState.transitionClose;
                    toClose = false;
                    toCloseTimer = 0;
                }
            }

            if(transitionState == UiTransitionState.transitionClose)
            {
                if(closedPosition.X < openPosition.X)
                    currentPosition.X -= transitionSpeed;
                else if(closedPosition.X > openPosition.X)
                    currentPosition.X += transitionSpeed;

                if(closedPosition.Y < openPosition.Y)
                    currentPosition.Y -= transitionSpeed;
                else if(closedPosition.Y > openPosition.Y)
                    currentPosition.Y += transitionSpeed;

                if(currentPosition == closedPosition)
                    transitionState = UiTransitionState.closed;

            } else if(transitionState == UiTransitionState.transitionOpen)
            {
                if(openPosition.X < closedPosition.X)
                    currentPosition.X -= transitionSpeed;
                else if(openPosition.X > closedPosition.X)
                    currentPosition.X += transitionSpeed;

                if(openPosition.Y < closedPosition.Y)
                    currentPosition.Y -= transitionSpeed;
                else if(openPosition.Y > closedPosition.Y)
                    currentPosition.Y += transitionSpeed;


                if(currentPosition == openPosition)
                    transitionState = UiTransitionState.open;
            }

        }

        public void Open(bool toClose = false, int afterTicks = 100) {
            if(transitionState == UiTransitionState.transitionClose || transitionState == UiTransitionState.closed)
            {
                transitionState = UiTransitionState.transitionOpen;
            }
            if(toClose)
            {
                this.toClose = true;
                toCloseTimer = 0;
                toCloseAfterTicks = afterTicks;
            }
        }

        public void Close() {
            if(!closable)
                return;

            if(transitionState == UiTransitionState.open || transitionState == UiTransitionState.transitionOpen)
            {
                transitionState = UiTransitionState.transitionClose;
            }
        }


    }

    public static class InterfaceManager {

        static List<UIObject> uiObjects;
        static UIObject damageText;
        static UIObject damageNum;

        static UIObject healthText;
        static UIObject healthNum;

        static Texture2D uiAtlas;
        static Dictionary<int, Rectangle> damageSpriteRects;

        public static void Initialize(ContentManager Content) {
            uiAtlas = Content.Load<Texture2D>("UI");
            uiObjects = new List<UIObject>();
            InitDamageSpriteRects();

            uiObjects.Add(new UIObject(new Point(54, 20), new Point(64, 20), new Point(10, 10), UiTransitionState.open, new Rectangle(0, 0, 10, 10)));

            damageText = new UIObject(new Point(34, 0), new Point(34, -10), new Point(30, 10), UiTransitionState.closed, new Rectangle(20, 10, 30, 10));
            damageNum = new UIObject(new Point(24, 0), new Point(24, -10), new Point(10, 10), UiTransitionState.closed, new Rectangle(10, 0, 10, 10));

            healthNum = new UIObject(new Point(0, 0), new Point(-10, 0), new Point(10, 10), UiTransitionState.open, new Rectangle(20, 0, 10, 10));
            healthText = new UIObject(new Point(10, 0), new Point(10, -10), new Point(10, 10), UiTransitionState.open, new Rectangle(0, 30, 10, 10));


            uiObjects.Add(healthNum);
            uiObjects.Add(healthText);
            uiObjects.Add(damageText);
            uiObjects.Add(damageNum);
        }

        static void InitDamageSpriteRects() {
            damageSpriteRects = new Dictionary<int, Rectangle>();
            damageSpriteRects.Add(0, new Rectangle(80, 10, 10, 10));
            damageSpriteRects.Add(1, new Rectangle(10, 0, 10, 10));
            damageSpriteRects.Add(2, new Rectangle(20, 0, 10, 10));
            damageSpriteRects.Add(3, new Rectangle(30, 0, 10, 10));
            damageSpriteRects.Add(4, new Rectangle(40, 0, 10, 10));
            damageSpriteRects.Add(5, new Rectangle(50, 0, 10, 10));
            damageSpriteRects.Add(6, new Rectangle(60, 0, 10, 10));
            damageSpriteRects.Add(7, new Rectangle(70, 0, 10, 10));
            damageSpriteRects.Add(8, new Rectangle(80, 0, 10, 10));
            damageSpriteRects.Add(9, new Rectangle(90, 0, 10, 10));
            damageSpriteRects.Add(10, new Rectangle(100, 0, 10, 10));
            damageSpriteRects.Add(11, new Rectangle(110, 0, 10, 10));
            damageSpriteRects.Add(12, new Rectangle(120, 0, 10, 10));
            damageSpriteRects.Add(13, new Rectangle(130, 0, 10, 10));
            damageSpriteRects.Add(14, new Rectangle(140, 0, 10, 10));
            damageSpriteRects.Add(15, new Rectangle(150, 0, 10, 10));
            damageSpriteRects.Add(16, new Rectangle(90, 10, 10, 10));
            damageSpriteRects.Add(17, new Rectangle(100, 10, 10, 10));
            damageSpriteRects.Add(18, new Rectangle(110, 10, 10, 10));
            damageSpriteRects.Add(19, new Rectangle(120, 10, 10, 10));
            damageSpriteRects.Add(20, new Rectangle(130, 10, 10, 10));
            damageSpriteRects.Add(21, new Rectangle(140, 10, 10, 10));
            damageSpriteRects.Add(22, new Rectangle(150, 10, 10, 10));
        }


        public static void Render(SpriteBatch spriteBatch) {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState != UiTransitionState.closed)
                {
                    spriteBatch.Draw(uiAtlas, new Rectangle(obj.currentPosition, obj.size), obj.spriteRect, Color.White);
                }
            }
        }


        public static void UpdateTick() {
            for(int i = 0; i < uiObjects.Count; i++)
            {
                uiObjects[i].Update();
            }
        }

        public static void OpenAll() {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState == UiTransitionState.open)
                    continue;

                obj.transitionState = UiTransitionState.transitionOpen;
            }
            allOpen = true;
        }

        public static void CloseAll() {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState == UiTransitionState.closed)
                    continue;

                obj.transitionState = UiTransitionState.transitionClose;
            }
            allOpen = false;
        }

        static bool allOpen = true;

        public static void ToggleAll() {
            Debug.WriteLine("Toogle Interface");
            if(allOpen)
                CloseAll();
            else
                OpenAll();
        }

        public static void ShowDamage(int damage) {
            if(damage > 0 && damage < 23)
            {
                damageNum.spriteRect = damageSpriteRects[damage];
                damageNum.Open(true, 40);
                damageText.Open(true, 40);
            } else {
                damageNum.spriteRect = damageSpriteRects[0];
                damageNum.Open(true, 40);
                damageText.Open(true, 40);
            }
        }

        public static void UpdateHealth(int health) {
            if(health > 0 && health < 23)
            {
                healthNum.spriteRect = damageSpriteRects[health];

            } else {
                Debug.WriteLine($"Have no sprite for this number {health}");
                damageNum.spriteRect = damageSpriteRects[0];
            }
        }
    }

   
}
