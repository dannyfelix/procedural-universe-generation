using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Universe
{
    /// <summary>Provides static methods for mathematical operations of colors and color palettes for the random selection of colors.</summary>
    public static class Colors
    {
        /// <summary>A palette of stone greys.</summary>
        public static Palette Stone { get; } = new Palette(0, 60, 0, 0.1, 0.2, 0.7);
        /// <summary>A palette of earthy reds, oranges and browns.</summary>
        public static Palette Earth { get; } = new Palette(0, 40, 0.1, 0.5, 0.2, 0.8);
        /// <summary>A palette of sandy yellows and beiges.</summary>
        public static Palette Sand { get; } = new Palette(30, 50, 0.1, 0.4, 0.5, 1.0);
        /// <summary>A palette of bright greens, blues and purples.</summary>
        public static Palette Fish { get; } = new Palette(160, 270, 0.5, 0.8, 0.6, 1.0);
        /// <summary>A palette of deep blues.</summary>
        public static Palette Water { get; } = new Palette(180, 250, 0.6, 0.8, 0.2, 0.8);
        /// <summary>A palette of off-white ice colors.</summary>
        public static Palette Ice { get; } = new Palette(200, 220, 0.0, 0.1, 0.8, 1.0);
        /// <summary>A palette of dark rusty reds and browns.</summary>
        public static Palette Rust { get; } = new Palette(0, 30, 0.4, 0.8, 0.3, 0.6);
        /// <summary>A palette of dark forest greens.</summary>
        public static Palette Forest { get; } = new Palette(70, 120, 0.3, 0.8, 0.2, 0.5);
        /// <summary>A palette of bright greens.</summary>
        public static Palette Grass { get; } = new Palette(60, 110, 0.3, 0.6, 0.5, 0.9);
        /// <summary>A palette of deep reds and purples.</summary>
        public static Palette Burgundy { get; } = new Palette(270, 360, 0.3, 1.0, 0.1, 0.5);
        
        /// <summary>Linearly interpolates between two colors.</summary>
        /// <param name="color1">The first color, positioned at 0 on the interpolation scale.</param>
        /// <param name="color2">The second color, positioned at 1 on the interpolation scale.</param>
        /// <param name="alpha">The factor by which to interpolate between the two colours.</param>
        /// <returns>A color that was linearly interpolated from color1 to color2 by a factor of alpha.</returns>
        public static Color Interpolate(Color color1, Color color2, double alpha)
        {
            if (double.IsNaN(alpha)) return color1;
            if (alpha <= 0) return color1;
            if (alpha >= 1) return color2;
            var r = (int) (alpha * color2.R + (1 - alpha) * color1.R);
            var g = (int) (alpha * color2.G + (1 - alpha) * color1.G);
            var b = (int) (alpha * color2.B + (1 - alpha) * color1.B);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>Considers the R, G and B values of a color from 0.0 to 1.0 and raises each to the specified power.</summary>
        /// <param name="color">The color to be raised to the specified power.</param>
        /// <param name="power">The power to which the specified color will be raised.</param>
        /// <returns>A color that has had each of the R, G and B components raised to the specified power.</returns>
        public static Color Pow(Color color, double power)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;
            var R = (int) (255 * Math.Pow(r, power));
            var G = (int) (255 * Math.Pow(g, power));
            var B = (int) (255 * Math.Pow(b, power));
            return Color.FromArgb(R, G, B);
        }

        /// <summary>Converts a set of coordinates in HSV color space to an RGB color.</summary>
        /// <param name="h">Hue, an angle in degrees.</param>
        /// <param name="s">Saturation, from 0.0 to 1.0.</param>
        /// <param name="v">Value, from 0.0 to 1.0.</param>
        /// <returns>A color corresponding to the specified position in HSV color space.</returns>
        public static Color HSVtoRGB(double h, double s, double v)
        {
            h = h % 360;
            var c = v * s;
            var x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
            var m = v - c;
            var rgb = new double[3];
            if (0 <= h && h < 60) rgb = new[] {c + m, x + m, m};
            if (60 <= h && h < 120) rgb = new[] {x + m, c + m, m};
            if (120 <= h && h < 180) rgb = new[] {m, c + m, x + m};
            if (180 <= h && h < 240) rgb = new[] {m, x + m, c + m};
            if (240 <= h && h < 300) rgb = new[] {x + m, m, c + m};
            if (300 <= h && h < 360) rgb = new[] {c + m, m, x + m};
            return Color.FromArgb((int) (255 * rgb[0]), (int) (255 * rgb[1]), (int) (255 * rgb[2]));
        }
    }

    /// <summary>Represents a region of HSV color space.</summary>
    public class Palette
    {
        /// <summary>Initializes an instance of the Palette class with specified boundaries in HSV color space.</summary>
        /// <param name="minH">The lower bound of the hue range, an angle in degrees.</param>
        /// <param name="maxH">The upper bound of the hue range, an angle in degrees.</param>
        /// <param name="minS">The lower bound of the saturation range, between 0.0 and 1.0.</param>
        /// <param name="maxS">The upper bound of the saturation range, between 0.0 and 1.0.</param>
        /// <param name="minV">The lower bound of the value range, between 0.0 and 1.0.</param>
        /// <param name="maxV">The upper bound of the value range, between 0.0 and 1.0.</param>
        public Palette(double minH, double maxH, double minS, double maxS, double minV, double maxV)
        {
            minHue = minH;
            maxHue = maxH;
            minSaturation = minS;
            maxSaturation = maxS;
            minValue = minV;
            maxValue = maxV;
        }

        /// <summary>The lower bound of the hue range.</summary>
        public double minHue { get; }
        /// <summary>The upper bound of the hue range.</summary>
        public double maxHue { get; }
        /// <summary>The lower bound of the saturation range.</summary>
        public double minSaturation { get; }
        /// <summary>The upper bound of the saturation range.</summary>
        public double maxSaturation { get; }
        /// <summary>The lower bound of the value range.</summary>
        public double minValue { get; }
        /// <summary>The upper bound of the value range.</summary>
        public double maxValue { get; }
    }

    /// <summary>Represents a scale from which colors can be selected by linearly interpolating between a set of colors at specified positions between 0.0 and 1.0.</summary>
    public class ColorScale
    {
        /// <summary>The specified set of colors from which all other colors on the scale are interpolated.</summary>
        private readonly List<Color> Colors;
        /// <summary>The positions of each color in the set of colors.</summary>
        private readonly List<double> Values;

        /// <summary>Initializes a new instance of the ColorScale class with specified colors at positions between 0.0 to 1.0.</summary>
        /// <param name="values">The positions of each color in the set of colors.</param>
        /// <param name="colors">The specified set of colors from which all other colors on the scale are interpolated.</param>
        public ColorScale(List<double> values, List<Color> colors)
        {
            Values = values;
            Colors = colors;
        }

        /// <summary>Linearly interpolates between colors on the scale to find the color at the specified position.</summary>
        /// <param name="v">The position on the scale between 0.0 and 1.0.</param>
        /// <returns>The color on the scale at the specified position.</returns>
        public Color Color(double v)
        {
            if (v < Values.First()) v = Values.First();
            if (v > Values.Last()) v = Values.Last();
            for (var i = 0; i < Values.Count - 1; i++)
                if (v < Values[i + 1])
                {
                    var alpha = (v - Values[i]) / (Values[i + 1] - Values[i]);
                    return Universe.Colors.Interpolate(Colors[i], Colors[i + 1], alpha);
                }

            return Colors.Last();
        }
    }
}