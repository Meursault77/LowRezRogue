using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LowRezRogue {
    public class Camera {

        public static Camera main;

        public Vector2 position;
        public float rotation = 0f;
        public float zoom = 8;
        private Rectangle bounds;

        Matrix transform;
        public Matrix onlyZoom;

        public Camera(Viewport viewport) {           
            bounds = viewport.Bounds;
            position = new Vector2(viewport.Width/16, viewport.Height/16);

            main = this;
            
            onlyZoom = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(bounds.Width * 0.5f, bounds.Height * 0.5f, 0));
        }

        public void SetPosition(Point playerPos) {
            position.X = playerPos.X * 8 + 4;
            position.Y = playerPos.Y * 8 + 4;
        }

        public Matrix Transform {
            get { return transform; }
        }

        void UpdateTransform() {
            transform = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(bounds.Width * 0.5f, bounds.Height * 0.5f, 0));
        }

        int mapPixels = 8;

        public void Update(Point player, int mapWidth, int mapHeight) {

            //Vector2 player = playerPos.ToVector2();


            if(player.X * mapPixels + 4 < position.X)
            {
                if(position.X - (player.X * mapPixels + 4) >= 8)
                    position.X -= 2;
                else 
                    position.X -= 1;
            } else if(player.X * mapPixels + 4 > position.X) {
                if((player.X * mapPixels + 4) - position.X >= 8)
                    position.X += 2;
                else
                    position.X += 1;
            }
            
            if(player.Y * mapPixels + 4 < position.Y)
                if(position.Y - (player.Y * mapPixels + 4) >= 8)
                    position.Y -= 2;
                else
                    position.Y -= 1;
            else if(player.Y * mapPixels + 4 > position.Y)
                if((player.Y * mapPixels + 4) - position.Y >= 8)
                    position.Y += 2;
                else
                    position.Y += 1;
            

            if(position.X < 32)
                position.X = 32;
            if(position.Y < 32)
                position.Y = 32;
            if(position.X > (mapWidth * mapPixels) - 32)
                position.X = (mapWidth * mapPixels) - 32;
            if(position.Y > (mapHeight * mapPixels) - 32)
                position.Y = (mapHeight * mapPixels) - 32;

            UpdateTransform();
        }

        public void JumpToPosition(Point pos) {
            position = new Vector2(pos.X * mapPixels + 4, pos.Y * mapPixels + 4);
            UpdateTransform();
        }

        public Point ToWorld(Point mousePosition) {
            return Vector2.Transform(mousePosition.ToVector2(), Matrix.Invert(Transform)).ToPoint();
        }

        public Point ToScreen(Point worldPosition) {
            return Vector2.Transform(worldPosition.ToVector2(), Transform).ToPoint();
        }
    }
}
