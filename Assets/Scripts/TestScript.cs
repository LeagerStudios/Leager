using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Color baseColor = new Color(0.69f, 0.69f, 0.69f); // #b1b1b1 in RGB
    public Color oreColor = Color.red; // Secondary color for the ore
    public int textureSize = 16; // 16x16 texture
    public int oreCircles = 3; // Number of ore circles
    public int circleRadius = 2; // Radius of each ore circle

    public Texture2D texture;
    public SpriteRenderer spriteRenderer;

    public enum OrePattern
    {
        Random,
        Diagonal
        // Add more patterns here if needed
    }

    private void Update()
    {
        GenerateTexture();
    }

    public void GenerateTexture()
    {
        texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // Set all pixels to the base color
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = baseColor;
        }

        // Apply ore color based on the selected pattern
        ApplyCirclePattern(pixels);

        texture.filterMode = FilterMode.Point;
        texture.SetPixels(pixels);
        texture.Apply();

        // Create a Sprite from the texture and assign it to the SpriteRenderer
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 8);
        spriteRenderer.sprite = sprite;
    }

    public void ApplyCirclePattern(Color[] pixels)
    {
        System.Random random = new System.Random();

        for (int i = 0; i < oreCircles; i++)
        {
            // Randomize the center position of each circle
            int centerX = random.Next(circleRadius, textureSize - circleRadius);
            int centerY = random.Next(circleRadius, textureSize - circleRadius);

            // Draw a circular ore cluster around the center
            for (int y = -circleRadius; y <= circleRadius; y++)
            {
                for (int x = -circleRadius; x <= circleRadius; x++)
                {
                    if (x * x + y * y <= circleRadius * circleRadius) // Check if within the circle
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;

                        if (pixelX >= 0 && pixelX < textureSize && pixelY >= 0 && pixelY < textureSize)
                        {
                            pixels[pixelY * textureSize + pixelX] = oreColor;
                        }
                    }
                }
            }
        }
    }
}