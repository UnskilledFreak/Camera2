using UnityEngine;

namespace Camera2.Configuration
{
    internal class ScreenRect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Locked { get; set; }

        public ScreenRect(float x, float y, float width, float height, bool locked)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Locked = locked;
        }

        public Rect ToRect() => new Rect(X, Y, Width, Height);

        public Vector2 MinAnchor() => new Vector2(X, Y);
        public Vector2 MaxAnchor() => new Vector2(X + Width, Y + Height);
    }
}