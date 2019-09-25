using System;
using System.Drawing;
using SharpNoise;
using SharpNoise.Modules;
using static System.Math;

namespace Universe
{
    /// <summary>A random number generator seeded with a string with methods to return values and other objects for the generation of celestial bodies.</summary>
    public class StringSeededRandom
    {
        private readonly int Seed;

        /// <summary>Initializes an instance of the StringSeededRandom class with a string to seed the random number generator.</summary>
        /// <param name="seedString">A string to seed the random number generator.</param>
        public StringSeededRandom(string seedString)
        {
            Seed = seedString.GetHashCode();
        }

        /// <summary>Returns a double precision floating-point number randomly chosen from a uniform distribution between 0.0 and 1.0.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <returns>A double precision floating-point number randomly chosen from a uniform distribution between 0.0 and 1.0.</returns>
        public double Rand(string seedString)
        {
            var random = new Random( Seed * seedString.GetHashCode());
            return random.NextDouble();
        }

        /// <summary>Returns a double precision floating-point number randomly chosen from a uniform distribution between two specified values.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="min">The lower bound of the uniform distribution.</param>
        /// <param name="max">The upper bound of the uniform distribution.</param>
        /// <returns>A double precision floating-point number randomly chosen from a uniform distribution between two specified values.</returns>
        public double Uniform(string seedString, double min, double max)
        {
            return min + (max - min) * Rand(seedString);
        }

        /// <summary>Returns a double precision floating-point number randomly chosen from a gaussian distribution with a mean of zero and a specified standard deviation.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="s">The standard deviation of the gaussian distribution.</param>
        /// <returns>A double precision floating-point number randomly chosen from a gaussian distribution with a mean of zero and a specified standard deviation.</returns>
        public double Gaussian(string seedString, double s)
        {
            var x = Rand(seedString);
            return Sqrt(PI / 8) * s * Log(x / (1 - x));
        }

        /// <summary>Returns an integer randomly chosen from a uniform distribution between two specified values.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="min">The lower bound of the uniform distribution.</param>
        /// <param name="max">The upper bound of the uniform distribution.</param>
        /// <returns>An integer randomly chosen from a uniform distribution between two specified values.</returns>
        public int Randint(string seedString, int min, int max)
        {
            return (int) (min + (max - min) * Rand(seedString));
        }

        /// <summary>Returns a randomly chosen item from a set of input parameters.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="options">The set of parameters from which the item will be chosen.</param>
        /// <returns>A randomly chosen item from a set of input parameters.</returns>
        public T Choice<T>(string seedString, params T[] options)
        {
            return options[Randint(seedString, 0, options.Length)];
        }

        /// <summary>Returns a color with RGB values randomly chosen from a uniform distribution.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <returns>A color with RGB values randomly chosen from a uniform distribution.</returns>
        public Color Color(string seedString)
        {
            var r = Randint(seedString + " red", 0, 256);
            var g = Randint(seedString + " green", 0, 256);
            var b = Randint(seedString + " blue", 0, 256);
            return System.Drawing.Color.FromArgb(r, g, b);
        }
        
        /// <summary>Returns a color with HSV values randomly chosen from a uniformly distributed range in HSV color space specified by a palette.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="palette">The palette which specifies the range in HSV color space from which the color will be chosen.</param>
        /// <returns>A color with HSV values randomly chosen from a uniformly distributed range in HSV color space specified by a palette.</returns>
        public Color Color(string seedString, Palette palette)
        {
            var h = Uniform(seedString + " hue", palette.minHue, palette.maxHue);
            var s = Uniform(seedString + " saturation", palette.minSaturation, palette.maxSaturation);
            var v = Uniform(seedString + " value", palette.minValue, palette.maxValue);
            return Colors.HSVtoRGB(h, s, v);
        }

        /// <summary>Returns a SharpNoise Perlin module with parameters chosen from uniform distributions between the specified bounds.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="minF">The lower bound of the frequency range.</param>
        /// <param name="maxF">The upper bound of the frequency range.</param>
        /// <param name="minL">The lower bound of the lacunarity range.</param>
        /// <param name="maxL">The upper bound of the lacunarity range.</param>
        /// <param name="minO">The lower bound of the octave count range.</param>
        /// <param name="maxO">The upper bound of the octave count range.</param>
        /// <param name="minP">The lower bound of the persistence range.</param>
        /// <param name="maxP">The upper bound of the persistence range.</param>
        /// <returns>A SharpNoise Perlin module with parameters chosen from uniform distributions between the specified bounds.</returns>
        public Perlin Perlin(string seedString, double minF, double maxF, double minL, double maxL, int minO, int maxO,
            double minP, double maxP)
        {
            return new Perlin
            {
                Frequency = Exp(Uniform(seedString + " frequency", Log(minF), Log(maxF))),
                Lacunarity = Uniform(seedString + " lacunarity", minL, maxL),
                OctaveCount = Randint(seedString + " octave count", minO, maxO),
                Persistence = Uniform(seedString + " persistence", minP, maxP),
                Quality = NoiseQuality.Best,
                Seed = Seed * Seed + Seed * seedString.GetHashCode()
            };
        }

        /// <summary>Returns a SharpNoise Billow module with parameters chosen from uniform distributions between the specified bounds.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="minF">The lower bound of the frequency range.</param>
        /// <param name="maxF">The upper bound of the frequency range.</param>
        /// <param name="minL">The lower bound of the lacunarity range.</param>
        /// <param name="maxL">The upper bound of the lacunarity range.</param>
        /// <param name="minO">The lower bound of the octave count range.</param>
        /// <param name="maxO">The upper bound of the octave count range.</param>
        /// <param name="minP">The lower bound of the persistence range.</param>
        /// <param name="maxP">The upper bound of the persistence range.</param>
        /// <returns>A SharpNoise Billow module with parameters chosen from uniform distributions between the specified bounds.</returns>
        public Billow Billow(string seedString, double minF, double maxF, double minL, double maxL, int minO, int maxO,
            double minP, double maxP)
        {
            return new Billow
            {
                Frequency = Exp(Uniform(seedString + " frequency", Log(minF), Log(maxF))),
                Lacunarity = Uniform(seedString + " lacunarity", minL, maxL),
                OctaveCount = Randint(seedString + " octave count", minO, maxO),
                Persistence = Uniform(seedString + " persistence", minP, maxP),
                Quality = NoiseQuality.Best,
                Seed = Seed * Seed + Seed * seedString.GetHashCode()
            };
        }

        /// <summary>Returns a SharpNoise RidgedMulti module with parameters chosen from uniform distributions between the specified bounds.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="minF">The lower bound of the frequency range.</param>
        /// <param name="maxF">The upper bound of the frequency range.</param>
        /// <param name="minL">The lower bound of the lacunarity range.</param>
        /// <param name="maxL">The upper bound of the lacunarity range.</param>
        /// <param name="minO">The lower bound of the octave count range.</param>
        /// <param name="maxO">The upper bound of the octave count range.</param>
        /// <returns>A SharpNoise RidgedMulti module with parameters chosen from uniform distributions between the specified bounds.</returns>
        public RidgedMulti RidgedMulti(string seedString, double minF, double maxF, double minL, double maxL, int minO,
            int maxO)
        {
            return new RidgedMulti
            {
                Frequency = Exp(Uniform(seedString + " frequency", Log(minF), Log(maxF))),
                Lacunarity = Uniform(seedString + " lacunarity", minL, maxL),
                OctaveCount = Randint(seedString + " octave count", minO, maxO),
                Quality = NoiseQuality.Best,
                Seed = Seed * Seed + Seed * seedString.GetHashCode()
            };
        }

        /// <summary>Returns a SharpNoise Turbulence module with parameters chosen from uniform distributions between the specified bounds.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="minF">The lower bound of the frequency range.</param>
        /// <param name="maxF">The upper bound of the frequency range.</param>
        /// <param name="minL">The lower bound of the lacunarity range.</param>
        /// <param name="maxL">The upper bound of the lacunarity range.</param>
        /// <param name="minO">The lower bound of the octave count range.</param>
        /// <param name="maxO">The upper bound of the octave count range.</param>
        /// <param name="minP">The lower bound of the persistence range.</param>
        /// <param name="maxP">The upper bound of the persistence range.</param>
        /// <returns>A SharpNoise Turbulence module with parameters chosen from uniform distributions between the specified bounds.</returns>
        public Turbulence Turbulence(string seedString, double minF, double maxF, double minL, double maxL, int minO,
            int maxO, double minP, double maxP)
        {
            return new Turbulence
            {
                Source0 = Perlin(seedString, minF, maxF, minL, maxL, minO, maxO, minP, maxP),
                Frequency = 0.1 * Uniform(seedString + " frequency", minF, maxF),
                Power = 1,
                Roughness = 1,
                Seed = Seed * Seed + Seed * seedString.GetHashCode()
            };
        }

        /// <summary>Returns either a Perlin, Billow or RidgedMulti SharpNoise Module with parameters chosen from uniform distributions between the specified bounds.</summary>
        /// <param name="seedString">A string to seed the random number selection.</param>
        /// <param name="minF">The lower bound of the frequency range.</param>
        /// <param name="maxF">The upper bound of the frequency range.</param>
        /// <param name="minL">The lower bound of the lacunarity range.</param>
        /// <param name="maxL">The upper bound of the lacunarity range.</param>
        /// <param name="minO">The lower bound of the octave count range.</param>
        /// <param name="maxO">The upper bound of the octave count range.</param>
        /// <param name="minP">The lower bound of the persistence range.</param>
        /// <param name="maxP">The upper bound of the persistence range.</param>
        /// <returns>A either a Perlin, Billow or RidgedMulti SharpNoise Module with parameters chosen from uniform distributions between the specified bounds.</returns>
        public Module Noise(string seedString, double minF, double maxF, double minL, double maxL, int minO, int maxO,
            double minP, double maxP)
        {
            var modules = new Module[]
            {
                Perlin(seedString, minF, maxF, minL, maxL, minO, maxO, minP, maxP),
                Billow(seedString, minF, maxF, minL, maxL, minO, maxO, minP, maxP),
                RidgedMulti(seedString, minF, maxF, minL, maxL, minO, maxO)
            };
            return Choice(seedString, modules);
        }
    }
}