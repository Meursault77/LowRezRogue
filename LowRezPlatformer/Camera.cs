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

        public Matrix Transform {
            get { return transform; }
        }

        void UpdateTransform() {
            transform = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(bounds.Width * 0.5f, bounds.Height * 0.5f, 0));
        }

        public void Update(Vector2 camMove) {
            position = camMove;
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
