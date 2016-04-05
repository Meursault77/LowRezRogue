using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

        public UIObject(Point openPosition, Point closedPosition, Point size, UiTransitionState startState, Rectangle spriteRect) {
            this.openPosition = openPosition;
            this.closedPosition = closedPosition;
            this.size = size;

            transitionState = startState;
            transitionSpeed = 2;

            this.spriteRect = spriteRect;

            if(startState == UiTransitionState.open)
                currentPosition = openPosition;
            else
                currentPosition = this.closedPosition;

        }

        public void Update(double deltaTime) {

            if(transitionState == UiTransitionState.transitionClose)
            {

            }else if(transitionState == UiTransitionState.transitionOpen)
            {

            }

        }

        public void Open() {
            if(transitionState == UiTransitionState.transitionClose || transitionState == UiTransitionState.closed)
            {
                transitionState = UiTransitionState.transitionOpen;
            }
        }

        public void Close() {
            if(transitionState == UiTransitionState.open || transitionState == UiTransitionState.transitionOpen)
            {

            }
        }


    }

    public static class InterfaceManager {

        static List<UIObject> uiObjects;
        static UIObject damageInfo;

        static Texture2D uiAtlas;

        public static void Initialize() {
            uiObjects = new List<UIObject>();
        }


        public static void Render(SpriteBatch spriteBatch) {
            foreach(UIObject obj in uiObjects)
            {
                if(obj.transitionState != UiTransitionState.closed)
                {
                    spriteBatch.Draw(uiAtlas, new Rectangle(obj.currentPosition, obj.size) , obj.spriteRect, Color.White);
                }
            }
        }

        public static void Update(double deltaTime) {
                        
        }

    }

   
}
