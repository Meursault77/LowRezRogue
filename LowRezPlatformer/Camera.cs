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

            if(main == null)
                main = this;
            else
                Debug.WriteLine("We have at least two cameras. First camera stays main cam.");

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


            if(player.X * mapPixels + 4< position.X)
                position.X -= 1;
            else if(player.X * mapPixels + 4 > position.X)
                position.X += 1;            
            //else
            //   position.X = (playerPos.X + 0) * mapPixels + (mapPixels / 2);     //pos +1 for 4 tiles to right direction, instead of three 

            if(player.Y * mapPixels + 4 < position.Y)
                position.Y -= 1;
            else if(player.Y * mapPixels + 4 > position.Y)
                position.Y += 1;
            //else
            //    position.Y = (playerPos.Y + 0) * mapPixels + (mapPixels / 2);


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

        public Point ToWorld(Point mousePosition) {
            return Vector2.Transform(mousePosition.ToVector2(), Matrix.Invert(Transform)).ToPoint();
        }

        public Point ToScreen(Point worldPosition) {
            return Vector2.Transform(worldPosition.ToVector2(), Transform).ToPoint();
        }
    }
}
