using UnityEngine;

namespace Crashers.Content
{
    public static class PlaceholderSprites
    {
        public static Sprite Character(Color body, Color head)
        {
            const int width = 16;
            const int height = 24;
            var pixels = new Color[width * height];
            Fill(pixels, width, 0, 0, width, height, Color.clear);
            Fill(pixels, width, 3, 0, 10, 3, Darken(body));
            Fill(pixels, width, 4, 3, 8, 10, body);
            Fill(pixels, width, 5, 13, 6, 8, head);
            Fill(pixels, width, 6, 17, 1, 2, Color.black);
            Fill(pixels, width, 9, 17, 1, 2, Color.black);
            return Finish(pixels, width, height, new Vector2(0.5f, 0.02f), 16f);
        }

        public static Sprite Sword(Color blade, Color grip)
        {
            const int size = 16;
            var pixels = new Color[size * size];
            Fill(pixels, size, 0, 0, size, size, Color.clear);
            Fill(pixels, size, 7, 0, 2, 4, grip);
            Fill(pixels, size, 4, 4, 8, 2, Darken(blade));
            Fill(pixels, size, 7, 6, 2, 9, blade);
            Fill(pixels, size, 7, 15, 2, 1, Color.white);
            return Finish(pixels, size, size, new Vector2(0.5f, 0.05f), 16f);
        }

        public static Sprite Blob(Color color, int size)
        {
            var pixels = new Color[size * size];
            float radius = size * 0.5f - 0.6f;
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color pixel = color;
                    pixel.a = distance <= radius ? color.a : 0f;
                    pixels[y * size + x] = pixel;
                }
            }
            return Finish(pixels, size, size, new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite Gradient(Color bottom, Color top, int width, int height)
        {
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                Color row = Color.Lerp(bottom, top, height <= 1 ? 0f : y / (float)(height - 1));
                for (int x = 0; x < width; x++) pixels[y * width + x] = row;
            }
            return Finish(pixels, width, height, new Vector2(0.5f, 0.5f), 4f);
        }

        public static Sprite Staff()
        {
            const int width = 4;
            const int height = 16;
            var shaft = new Color(0.48f, 0.3f, 0.16f, 1f);
            var tip = new Color(0.7f, 0.48f, 0.95f, 1f);
            var pixels = new Color[width * height];
            Fill(pixels, width, 0, 0, width, height, shaft);
            Fill(pixels, width, 0, 12, width, 4, tip);
            return Finish(pixels, width, height, new Vector2(0.5f, 0.06f), 16f);
        }

        public static Sprite Fist()
        {
            const int width = 6;
            const int height = 5;
            var skin = new Color(0.96f, 0.8f, 0.66f, 1f);
            var edge = new Color(0.62f, 0.4f, 0.28f, 1f);
            var pixels = new Color[width * height];
            Fill(pixels, width, 0, 0, width, height, edge);
            Fill(pixels, width, 1, 1, width - 2, height - 2, skin);
            return Finish(pixels, width, height, new Vector2(0.5f, 0.5f), 16f);
        }

        public static Sprite WhiteQuad()
        {
            var pixels = new[] { Color.white };
            return Finish(pixels, 1, 1, new Vector2(0f, 0.5f), 1f);
        }

        static Color Darken(Color color) => new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, color.a);

        static void Fill(Color[] pixels, int width, int x, int y, int w, int h, Color color)
        {
            int height = pixels.Length / width;
            for (int yy = y; yy < y + h; yy++)
            {
                if (yy < 0 || yy >= height) continue;
                for (int xx = x; xx < x + w; xx++)
                {
                    if (xx < 0 || xx >= width) continue;
                    pixels[yy * width + xx] = color;
                }
            }
        }

        static Sprite Finish(Color[] pixels, int width, int height, Vector2 pivot, float pixelsPerUnit)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), pivot, pixelsPerUnit);
        }
    }
}
