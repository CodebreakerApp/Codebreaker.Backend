﻿using CodeBreaker.APIs.Services.Obsolete;
using Xunit;

namespace ServerTests;

public class RandomGameGeneratorTests
{

    [Fact]
    public void StartGameReturnsRandomEnoughValues()
    {
        Game6x4Definition generator = new();

        string[] colorList = { "White", "Red", "Green", "Blue", "Black", "Yellow" };
        bool[] colorMatches = new bool[colorList.Length];
        bool allColors = false;
        int iterations = 0;
        do
        {
            iterations++;
            string[] colors = generator.CreateRandomCode();
            for (int i = 0; i < colorList.Length; i++)
            {
                if (colors.Contains(colorList[i]))
                {
                    colorMatches[i] = true;
                    allColors = colorMatches.All(c => c);
                }
            }
        } while (!allColors && iterations < 120);
        Assert.True(iterations < 100);
    }
}
