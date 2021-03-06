﻿using Microsoft.Xna.Framework;
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

        public bool alsoRenderClosed;
        public bool openWithAll;
        bool closable = true;
        bool toClose;
        int toCloseTimer;
        int toCloseAfterTicks;

        public UIObject(Point openPosition, Point closedPosition, Point size, UiTransitionState startState, Rectangle spriteRect, int transitionSpeed = 1, bool openWithAll = true, bool alsoRenderClosed = false) {
            this.openPosition = openPosition;
            this.closedPosition = closedPosition;
            this.size = size;

            this.openWithAll = openWithAll;
            this.alsoRenderClosed = alsoRenderClosed;

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
                {
                    currentPosition.X -= transitionSpeed;
                    if(currentPosition.X < closedPosition.X)
                        currentPosition.X = closedPosition.X;
                } else if(closedPosition.X > openPosition.X) { 
                    currentPosition.X += transitionSpeed;
                    if(currentPosition.X > closedPosition.X)
                        currentPosition.X = closedPosition.X;
                }

                if(closedPosition.Y < openPosition.Y)
                {
                    currentPosition.Y -= transitionSpeed;
                    if(currentPosition.Y < closedPosition.Y)
                        currentPosition.Y = closedPosition.Y;
                } else if(closedPosition.Y > openPosition.Y)
                {
                    currentPosition.Y += transitionSpeed;
                    if(currentPosition.Y > closedPosition.Y)
                        currentPosition.Y = closedPosition.Y;
                }

                if(currentPosition == closedPosition)
                    transitionState = UiTransitionState.closed;

            } else if(transitionState == UiTransitionState.transitionOpen)
            {
                if(openPosition.X < closedPosition.X)
                {
                    currentPosition.X -= transitionSpeed;
                    if(currentPosition.X < openPosition.X)
                        currentPosition.X = openPosition.X;
                } else if(openPosition.X > closedPosition.X)
                {
                    currentPosition.X += transitionSpeed;
                    if(currentPosition.X > openPosition.X)
                        currentPosition.X = openPosition.X;
                }

                if(openPosition.Y < closedPosition.Y)
                {
                    currentPosition.Y -= transitionSpeed;
                    if(currentPosition.Y < openPosition.Y)
                        currentPosition.Y = openPosition.Y;
                } else if(openPosition.Y > closedPosition.Y)
                {
                    currentPosition.Y += transitionSpeed;
                    if(currentPosition.Y > openPosition.Y)
                        currentPosition.Y = openPosition.Y;
                }

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


        static int damageMessageTicks = 30;

        static List<UIObject> uiObjects;
        static UIObject damageLogo;
        static UIObject damageNum;



        static Dictionary<int, Rectangle> numberSprites;

        static Rectangle damageRect;
        static Rectangle deathRect;

        static UIObject statsHealthLogo;
        static UIObject statsHealthNum;

        static UIObject statsDamageLogo;
        static UIObject statsDamageNum;

        static UIObject statsArmorLogo;
        static UIObject statsArmorNum;

        static UIObject statsRangeLogo;
        static UIObject statsRangeNum;

        public static UIObject rangeCombatLogo;
        static Rectangle rangeCombatNoTarget;
        static Rectangle rangeCombatHasTarget;

        public static UIObject sprintLogo;

        static UIObject artifactPickUp;
        static UIObject artifactNumber;
        static Rectangle[] artifacts;
        static Rectangle[] artifactNumbers;
        static int currentArtifact;
        static int animationCounter = 0;
        static int animationTiming = 10;


        public static void Initialize(ContentManager Content) {
            uiObjects = new List<UIObject>();
            InitNumberSpriteRects();
            damageRect = new Rectangle(30, 10, 20, 10);
            deathRect = new Rectangle(30, 20, 20, 10);

            artifacts = new Rectangle[6];
            artifacts[0] = new Rectangle(0,136,24,24);
            artifacts[1] = new Rectangle(24, 136, 24, 24);
            artifacts[2] = new Rectangle(48, 136, 24, 24);
            artifacts[3] = new Rectangle(72, 136, 24, 24);
            artifacts[4] = new Rectangle(96, 136, 24, 24);
            artifacts[5] = new Rectangle(120, 136, 24, 24);

            artifactNumbers = new Rectangle[6];
            artifactNumbers[0] = Rectangle.Empty;
            artifactNumbers[1] = new Rectangle(130, 20, 30, 20);
            artifactNumbers[2] = new Rectangle(130, 40, 30, 20);
            artifactNumbers[3] = new Rectangle(130, 60, 30, 20);
            artifactNumbers[4] = new Rectangle(130, 80, 30, 20);
            artifactNumbers[5] = new Rectangle(130, 100, 30, 20);

            damageLogo = new UIObject(new Point(34, 0), new Point(34, -10), new Point(20, 10), UiTransitionState.closed, damageRect, openWithAll: false);
            damageNum = new UIObject(new Point(54, 0), new Point(54, -10), new Point(10, 10), UiTransitionState.closed, new Rectangle(10, 0, 10, 10), openWithAll: false);

            statsHealthNum = new UIObject(new Point(9, 0), new Point(-11, 0), new Point(10, 10), UiTransitionState.open, new Rectangle(20, 0, 10, 10), 2);
            statsHealthLogo = new UIObject(new Point(-1, 0), new Point(-11, 0), new Point(10, 10), UiTransitionState.open, new Rectangle(10, 30, 10, 10));

            statsDamageNum = new UIObject(new Point(9,9), new Point(-11,9), new Point(10,10), UiTransitionState.open, new Rectangle(50,0,10,10), 2);
            statsDamageLogo = new UIObject(new Point(-1, 9), new Point(-11, 9), new Point(10, 10), UiTransitionState.open, new Rectangle(30,30,10,10));

            statsRangeNum = new UIObject(new Point(9, 18), new Point(-11,18), new Point(10,10), UiTransitionState.open, numberSprites[9], 2);
            statsRangeLogo = new UIObject(new Point(-1,18), new Point(-11, 18), new Point(10, 10), UiTransitionState.open, new Rectangle(40, 30, 10, 10));

            statsArmorNum = new UIObject(new Point(9, 27), new Point(-11, 27), new Point(10, 10), UiTransitionState.open, numberSprites[3], 2);
            statsArmorLogo = new UIObject(new Point(-1, 27), new Point(-11, 27), new Point(10, 10), UiTransitionState.open, new Rectangle(20, 30, 10, 10));

            rangeCombatLogo = new UIObject(new Point(51,54), new Point(64,54), new Point(13,10), UiTransitionState.closed, rangeCombatNoTarget, openWithAll: false);
            rangeCombatHasTarget = new Rectangle(0, 40, 13, 10);
            rangeCombatNoTarget = new Rectangle(0, 50, 13, 10);

            sprintLogo = new UIObject(new Point(51, 43), new Point(58, 43), new Point(13, 11), UiTransitionState.closed, new Rectangle(0, 60, 13, 11), openWithAll: false, alsoRenderClosed: true);

            artifactPickUp = new UIObject(new Point(20, 34), new Point(64, 34), new Point(24, 24), UiTransitionState.closed, new Rectangle(20,40,22,10), openWithAll: false, transitionSpeed: 2);

            artifactNumber = new UIObject(new Point(16, 10), new Point(64, 10), new Point(30,20), UiTransitionState.closed, artifactNumbers[0], transitionSpeed: 2, openWithAll: false);

            uiObjects.Add(statsHealthNum);
            uiObjects.Add(statsHealthLogo);
            uiObjects.Add(statsDamageNum);
            uiObjects.Add(statsDamageLogo);
            uiObjects.Add(statsRangeNum);
            uiObjects.Add(statsRangeLogo);
            uiObjects.Add(statsArmorNum);
            uiObjects.Add(statsArmorLogo);
            uiObjects.Add(rangeCombatLogo);
            uiObjects.Add(sprintLogo);
            uiObjects.Add(artifactPickUp);
            uiObjects.Add(artifactNumber);

            uiObjects.Add(damageNum);
            uiObjects.Add(damageLogo);


        }

        static void InitNumberSpriteRects() {
            numberSprites = new Dictionary<int, Rectangle>();
            numberSprites.Add(0, new Rectangle(0, 0, 10, 10));
            numberSprites.Add(1, new Rectangle(10, 0, 10, 10));
            numberSprites.Add(2, new Rectangle(20, 0, 10, 10));
            numberSprites.Add(3, new Rectangle(30, 0, 10, 10));
            numberSprites.Add(4, new Rectangle(40, 0, 10, 10));
            numberSprites.Add(5, new Rectangle(50, 0, 10, 10));
            numberSprites.Add(6, new Rectangle(60, 0, 10, 10));
            numberSprites.Add(7, new Rectangle(70, 0, 10, 10));
            numberSprites.Add(8, new Rectangle(80, 0, 10, 10));
            numberSprites.Add(9, new Rectangle(90, 0, 10, 10));
            numberSprites.Add(10, new Rectangle(100, 0, 10, 10));
            numberSprites.Add(11, new Rectangle(110, 0, 10, 10));
            numberSprites.Add(12, new Rectangle(120, 0, 10, 10));
            numberSprites.Add(13, new Rectangle(130, 0, 10, 10));
            numberSprites.Add(14, new Rectangle(140, 0, 10, 10));
            numberSprites.Add(15, new Rectangle(150, 0, 10, 10));
            numberSprites.Add(16, new Rectangle(90, 10, 10, 10));
            numberSprites.Add(17, new Rectangle(100, 10, 10, 10));
            numberSprites.Add(18, new Rectangle(110, 10, 10, 10));
            numberSprites.Add(19, new Rectangle(120, 10, 10, 10));
            numberSprites.Add(20, new Rectangle(130, 10, 10, 10));
            numberSprites.Add(21, new Rectangle(140, 10, 10, 10));
            numberSprites.Add(22, new Rectangle(150, 10, 10, 10));
        }


        public static void Render(SpriteBatch spriteBatch, Texture2D uiAtlas) {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState != UiTransitionState.closed || obj.alsoRenderClosed)
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

            if(artifactPickUp.transitionState != UiTransitionState.closed)
            {
                if(animationCounter < animationTiming)
                {
                    animationCounter++;
                } else
                {
                    animationCounter = 0;

                    if(artifactPickUp.spriteRect == artifacts[currentArtifact])
                        artifactPickUp.spriteRect = artifacts[currentArtifact - 1];
                    else if(artifactPickUp.spriteRect == artifacts[currentArtifact - 1])
                        artifactPickUp.spriteRect = artifacts[currentArtifact];
                    else
                        artifactPickUp.spriteRect = artifacts[currentArtifact];
                }
            }

        }

        public static void OpenAll() {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState == UiTransitionState.open || !obj.openWithAll)
                    continue;

                obj.transitionState = UiTransitionState.transitionOpen;
            }
            allOpen = true;
        }

        public static void CloseAll() {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState == UiTransitionState.closed || !obj.openWithAll)
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
            if(damage >= 0 && damage < 23)
            {
                damageLogo.spriteRect = damageRect;
                damageNum.spriteRect = numberSprites[damage];
                damageNum.Open(true, damageMessageTicks);
                damageLogo.Open(true, damageMessageTicks);
            } else if(damage == 666){       //666 is the code for death!
                damageLogo.spriteRect = deathRect;
                damageNum.spriteRect = new Rectangle(0,20,10,10);
                damageNum.Open(true, damageMessageTicks);
                damageLogo.Open(true, damageMessageTicks);
            }
        }

        public static void ToggleRangeLogo(bool hasTarget) {
            if(rangeCombatLogo.transitionState == UiTransitionState.closed || rangeCombatLogo.transitionState == UiTransitionState.transitionClose)
            {
                rangeCombatLogo.transitionState = UiTransitionState.transitionOpen;
                if(hasTarget)
                    rangeCombatLogo.spriteRect = rangeCombatHasTarget;
                else
                    rangeCombatLogo.spriteRect = rangeCombatNoTarget;
            } else if(rangeCombatLogo.transitionState == UiTransitionState.open || rangeCombatLogo.transitionState == UiTransitionState.transitionOpen)
            {
                rangeCombatLogo.transitionState = UiTransitionState.transitionClose;
            }

        }

        public static void CloseSprint() {
            if(sprintLogo.transitionState != UiTransitionState.closed)
            {
                sprintLogo.Close();
            }
        }

        public static void ShowSprint() {
            if(sprintLogo.transitionState != UiTransitionState.open)
                sprintLogo.Open();
        }

        public static void ActivateSprint(bool activate) {
            if(activate)
            {
                sprintLogo.alsoRenderClosed = true; 
            } else
            {
                sprintLogo.alsoRenderClosed = false;
            }
        }
    

        public static void ToggleHealth(bool forceOpen = false) {
            if(forceOpen)
            {
                statsHealthLogo.Open();
                statsHealthNum.Open();
                return;
            }

            if(statsHealthLogo.transitionState != UiTransitionState.open && statsHealthNum.transitionState != UiTransitionState.open)
            {
                statsHealthLogo.Open();
                statsHealthNum.Open();
            } else if(statsHealthLogo.transitionState != UiTransitionState.closed && statsHealthNum.transitionState != UiTransitionState.closed)
            {
                statsHealthLogo.Close();
                statsHealthNum.Close();
            }
        }

        public static void UpdateHealth(int health) {
            if(health >= 0 && health < 23)
            {
                statsHealthNum.spriteRect = numberSprites[health];
                if(statsHealthNum.transitionState != UiTransitionState.open)
                {
                    statsHealthNum.Open(true, 70);
                    statsHealthLogo.Open(true, 70);
                }

            } else {
                Debug.WriteLine($"Health, Have no sprite for this number {health}");
                damageNum.spriteRect = numberSprites[0];
            }
        }

        public static void UpdateArmor(int armor) {
            if(armor >= 0 && armor < 23)
            {
                statsArmorNum.spriteRect = numberSprites[armor];
                if(statsArmorNum.transitionState != UiTransitionState.open) {
                    statsArmorNum.Open(true, 70);
                    statsArmorLogo.Open(true, 70);
                }

            } else
            {
                Debug.WriteLine($"Armor, Have no sprite for this number {armor}");
                statsArmorNum.spriteRect = numberSprites[0];
            }
        }

        public static void UpdateDamage(int damage) {
            if(damage >= 0 && damage < 23)
            {
                statsDamageNum.spriteRect = numberSprites[damage];
                if(statsDamageNum.transitionState != UiTransitionState.open)
                {
                    statsDamageNum.Open(true, 70);
                    statsDamageLogo.Open(true, 70);
                }

            } else
            {
                Debug.WriteLine($"Damage, Have no sprite for this number {damage}");
                statsDamageNum.spriteRect = numberSprites[0];
            }
        }

        public static void UpdateRangeDamage(int rangeDamage) {
            if(rangeDamage >= 0 && rangeDamage < 23)
            {
                statsRangeNum.spriteRect = numberSprites[rangeDamage];
                if(statsRangeNum.transitionState != UiTransitionState.open)
                {
                    statsRangeNum.Open(true, 70);
                    statsRangeLogo.Open(true, 70);
                }

            } else
            {
                Debug.WriteLine($"Damage, Have no sprite for this number {rangeDamage}");
                statsRangeNum.spriteRect = numberSprites[0];
            }
        }

        public static void ShowArtifact(int piecesFound) {
            Debug.WriteLine("Called ShowAritfact in InterfaceManager");
            if(piecesFound < 0 || piecesFound > 5)
                return;

            CloseAll();

            currentArtifact = piecesFound;
            artifactPickUp.spriteRect = artifacts[piecesFound];
            artifactPickUp.Open(true, 90);
            artifactNumber.spriteRect = artifactNumbers[piecesFound];
            artifactNumber.Open(true, 90);
        }

    }

   
}
