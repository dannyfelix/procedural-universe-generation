using System.Drawing;
using SharpNoise;
using SharpNoise.Builders;
using static System.Math;
using static Universe.Physics;

namespace Universe
{
    /// <summary>Represents a star defined by its fundamental astrophysical parameters.</summary>
    public class Star : Body
    {
        /// <summary>The B-V color magnitude of the star.</summary>
        public double BVMagnitude { get; protected set; }
        /// <summary>The luminosity of the star in [W].</summary>
        public double Luminosity { get; protected set; }
        /// <summary>The absolute magnitude of the star.</summary>
        public double Magnitude { get; protected set; }

        /// <summary>Instantiates an instance of the Star class with a specified name to seed the random number generator.</summary>
        /// <param name="name">The name of the star used to seed the random number generator.</param>
        /// <param name="parent">The name of the star used to seed the random number generator.</param>
        public Star(string name = null, Body parent = null) : base(parent, null, name)
        {
            if (Name.Equals("the sun"))
            {
                Star = this;
                BVMagnitude = 0.63;
                Magnitude = 4.83;
                Luminosity = SolarLuminosity;
                EffectiveTemperature = SolarTemperature;
                Color = TemperatureToColor(EffectiveTemperature);
                Mass = SolarMass;
                Radius = SolarRadius;
                Volume = SphereVolume(Radius);
                Density = Mass / Volume;
                RotationPeriod = 2164320;
                SurfaceGravity = GravitationalAcceleration(Mass, Radius);
            }
            else
            {
                Star = this;
                BVMagnitude = 0.7 + Random.Gaussian("b-v magnitude", 0.5);
                Magnitude = 10.0 * (Pow(BVMagnitude - 0.8, 3.0) + 0.5 * (BVMagnitude - 0.8) + 0.53) +
                            Random.Gaussian("magnitude", 0.7);
                Luminosity = MagnitudeToLuminosity(Magnitude);
                EffectiveTemperature = BVToTemperature(BVMagnitude);
                Color = TemperatureToColor(EffectiveTemperature);
                var logLum = Log10(Luminosity / 3.828e26 + 1e-10) + Random.Gaussian("log luminosity", 0.3);
                Mass = 1.989e30 * Pow((50 + 5 * logLum) / (50 - 3 * logLum), 1.3);
                SphereOfInfluence = parent == null ? Sqrt(G * Mass / 1e-6) : HillSphere(Orbit, Mass, parent.Mass);
                Radius = StarRadius(Luminosity, EffectiveTemperature);
                Volume = SphereVolume(Radius);
                Density = Mass / Volume;
                RotationPeriod = Exp(15 + Random.Gaussian("rotation period", 3));
                SurfaceGravity = GravitationalAcceleration(Mass, Radius);
                AddSatellites(15);
                
                TemperatureNoiseModule = Random.Billow("color map noise", 2, 32, 1.8, 2.2, 10, 16, 0.5, 0.6);
            }
        }

        private void GenerateTemperatureMap(int height)
        {
            var width = 2 * height;
            var builder = new SphereNoiseMapBuilder();
            builder.SetDestSize(width, height);
            builder.SetBounds(-90, 90, -180, 180);
            TemperatureNoiseMap = new NoiseMap();
            builder.SourceModule = TemperatureNoiseModule;
            builder.DestNoiseMap = TemperatureNoiseMap;
            builder.Build();

            var min = 10.0f;
            var max = -10.0f;
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = TemperatureNoiseMap.GetValue(x, y);
                    if (value < min) min = value;
                    else if (value > max) max = value;
                }

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = TemperatureNoiseMap.GetValue(x, y);
                    value = (value - min) / (max - min);
                    value = 0.75f + 0.25f * value;
                    value *= (float) EffectiveTemperature;
                    TemperatureNoiseMap.SetValue(x, y, value);
                }
        }

        internal override void GenerateColorMap(int height)
        {
            GenerateTemperatureMap(height);
            ColorNoiseMap = TemperatureNoiseMap;
        }
        
        public override void WriteColorMap(int height, string filePath)
        {
            var width = 2 * height;
            if (ColorNoiseMap == null) GenerateColorMap(height);

            var bitmap = new DirectBitmap(width, height);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var temp = ColorNoiseMap.GetValue(x, y);
                    var col = TemperatureToColor(temp);
                    var factor = temp / EffectiveTemperature;
                    col = Colors.Interpolate(Color.Black, col, factor);
                    bitmap.SetPixel(x, y, col);
                }

            bitmap.Save(filePath);
        }
    }
}