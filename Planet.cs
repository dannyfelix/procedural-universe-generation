using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Newtonsoft.Json;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using static System.Math;
using static Universe.Physics;
using static Universe.Colors;

namespace Universe
{
    /// <summary>Represents a planet defined by its astrophysical parameters and provides methods for the generation of various image textures.</summary>
    public abstract class Planet : Body
    {
        /// <summary>The scale of colors used to generate the color map.</summary>
        protected ColorScale ColorScale;
        /// <summary>The additional temperature due to geothermal heating in [K].</summary>
        protected double GeothermalTemperature;
        /// <summary>The additional temperature due to the greenhouse effect in [K].</summary>
        protected double GreenhouseTemp;
        /// <summary>The SharpNoise Module used to generate noise for heightNoiseMap.</summary>
        protected Module HeightNoiseModule;
        
        /// <summary>The albedo of the planet.</summary>
        public double Albedo { get; protected set; }
        /// <summary>Whether the planet has an atmosphere.</summary>
        public bool HasAtmosphere { get; protected set; }
        /// <summary>The color of the atmosphere of the planet.</summary>
        public Color AtmosphereColor { get; protected set; }
        /// <summary>The maximum extent of the atmosphere of the planet in [m].</summary>
        public double AtmosphereHeight { get; protected set; }
        /// <summary>The atmospheric pressure on the surface of the planet in [Pa].</summary>
        public double AtmospherePressure { get; protected set; }
        /// <summary>The exponential scale height of the atmosphere of the planet in [m].</summary>
        public double AtmosphereScaleHeight { get; protected set; }
        /// <summary>The opacity of the atmosphere of the planet.</summary>
        public double AtmosphereOpacity { get; protected set; }
        /// <summary>The temperature of the core of the planet in [K].</summary>
        public double CoreTemperature { get; protected set; }
        /// <summary>The temperature on the surface of the planet in [K].</summary>
        public double SurfaceTemperature { get; protected set; }
        /// <summary>A SharpNoise NoiseMap used to generate the height map.</summary>
        [JsonIgnore] public NoiseMap HeightNoiseMap { get; protected set; }

        /// <summary>Initializes an instance of the Planet class with an optional host star, orbital parent and name to seed the random number generator.</summary>
        /// <param name="star">The star which the planet is closest to.</param>
        /// <param name="parent">The body which the planet orbits.</param>
        /// <param name="name">The name of the planet used to seed the random number generator.</param>
        protected Planet(Body parent, Star star, string name = null) : base(parent, star, name)
        {
            Orbit = new Orbit(Parent, Random);
            Mass = Pow(10, Parent is Star ? Random.Uniform("mass", 22, 28) :
                Random.Uniform("mass", 19, Log10(Parent.Mass * 1e5 / Parent.Radius)));
            SphereOfInfluence = HillSphere(Orbit, Mass, Parent.Mass);
            EffectiveTemperature = EffectiveTemperature(star, Orbit.SemiMajorAxis);
        }

        public bool ShouldSerializeAtmosphereHeight()
        {
            return HasAtmosphere;
        }

        public bool ShouldSerializeAtmospherePressure()
        {
            return HasAtmosphere;
        }

        public bool ShouldSerializeAtmosphereScaleHeight()
        {
            return HasAtmosphere;
        }

        public bool ShouldSerializeAtmosphereOpacity()
        {
            return HasAtmosphere;
        }
        
        public bool ShouldSerializeAtmosphereColor()
        {
            return HasAtmosphere && !AtmosphereColor.IsEmpty;
        }
    }

    /// <summary>Represents a gas giant or ice giant.</summary>
    public class GiantPlanet : Planet
    {
        /// <summary>Initializes an instance of the GasPlanet class with a specified host star, orbital parent and name to seed the random number generator.</summary>
        /// <param name="star">The star which the planet is closest to.</param>
        /// <param name="parent">The body which the planet orbits.</param>
        /// <param name="name">The name of the planet used to seed the random number generator.</param>
        public GiantPlanet(Body parent, Star star, string name = null) : base(parent, star, name)
        {
            Radius = EarthRadius * Pow(Mass / EarthMass, Random.Uniform("radius", 0.4, 0.52));
            Volume = SphereVolume(Radius);
            Density = Mass / Volume;
            SurfaceGravity = GravitationalAcceleration(Mass, Radius);
            RotationPeriod = Exp(11 + Random.Gaussian("rotation period", 2));
            Albedo = Random.Uniform("albedo", 0.2, 0.5);
            EffectiveTemperature = EffectiveTemperature * Pow(1 - Albedo, 0.25);
            CoreTemperature = 1300 * Sqrt(Mass / 6e24);
            GeothermalTemperature = CoreTemperature / 200;
            SurfaceTemperature = Pow(Pow(EffectiveTemperature, 4) + Pow(GeothermalTemperature, 4), 0.25);
            HasAtmosphere = true;
            AtmosphereOpacity = Pow(Random.Rand("atmosphere opacity"), Radius / EarthRadius);
            AtmospherePressure = EarthAtmoPressure * 2 * Tan(PI * AtmosphereOpacity / 2);
            AtmosphereOpacity = 0;
            GreenhouseTemp = 13 * Pow(AtmospherePressure, 0.25);
            SurfaceTemperature = Pow(Pow(EffectiveTemperature, 4) + Pow(GeothermalTemperature, 4) + Pow(GreenhouseTemp, 4), 0.25);
            AtmosphereScaleHeight = kB * SurfaceTemperature / (4.5e-26 * SurfaceGravity);
            AtmosphereHeight = AtmosphereScaleHeight * Log(AtmospherePressure);
            HasRing = Random.Rand("has ring") < 0.5;
            if (HasRing)
            {
                InnerRingRadius = Random.Uniform("inner ring radius", Radius, 3 * Radius);
                OuterRingRadius = Random.Uniform("outer ring radius", InnerRingRadius, 3 * InnerRingRadius);
                var ringPalette = Random.Choice("ring palette", Ice, Sand);
                RingColor = Pow(Random.Color("ring color", ringPalette), 0.4);
                RingInclination = PI / 2 * Pow(Random.Rand("ring inclination"), 10);
            }
            AddSatellites((int) (2 * Sqrt(Radius / EarthRadius) + 2 * Sqrt(Orbit.SemiMajorAxis / 1e11)));

            List<Color> colors;
            var values = new List<double> {0.0, 0.33, 0.67, 1.0};
            
            if (Random.Rand("colour choice") < 0.5)
            {
                var palette1 = Random.Choice("palette 1", Water, Fish);
                var palette2 = Random.Choice("palette 2", Water, Fish);
                var palette3 = Ice;
                var palette4 = Random.Choice("palette 4", Water, Fish);
                colors = new List<Color>
                {
                    Pow(Random.Color("color scale 0", palette1), 0.4),
                    Pow(Random.Color("color scale color 1", palette2), 0.15),
                    Pow(Random.Color("color scale color 2", palette3), 0.5),
                    Pow(Random.Color("color scale color 3", palette4), 0.5)
                };
            }
            else
            {
                var palette1 = Random.Choice("palette 1", Rust, Sand);
                var palette2 = Earth;
                var palette3 = Random.Choice("palette 3", Ice, Sand);
                var palette4 = Random.Choice("palette 4", Rust, Earth);
                colors = new List<Color>
                {
                    Pow(Random.Color("color scale color 0", palette1), 0.35),
                    Pow(Random.Color("color scale color 1", palette2), 0.25),
                    Pow(Random.Color("color scale color 2", palette3), 0.5),
                    Pow(Random.Color("color scale color 3", palette4), 0.5)
                };
            }

            AtmosphereColor = colors[0];
            Color = colors[0];

            ColorScale = new ColorScale(values, colors);
            HeightNoiseModule = Random.Perlin("height map noise", 4, 4, 2.2, 2.2, 12, 12, 0.6, 0.7);
            ColorNoiseModule = Random.Billow("color map noise", 2, 8, 1.9, 1.9, 8, 8, 0.55, 0.65);
        }

        internal override void GenerateColorMap(int height)
        {
            var width = 2 * height;

            var builder = new SphereNoiseMapBuilder();
            ColorNoiseMap = new NoiseMap();
            builder.SourceModule = ColorNoiseModule;
            builder.SetDestSize(width, height);
            builder.DestNoiseMap = ColorNoiseMap;
            builder.SetBounds(-90, 90, -180, 180);
            builder.Build();
            
            HeightNoiseMap = new NoiseMap();
            builder.SourceModule = HeightNoiseModule;
            builder.SetDestSize(1, height);
            builder.DestNoiseMap = HeightNoiseMap;
            builder.SetBounds(-90, 90, 0, 0.000001);
            builder.Build();

            var min = 1000.0f;
            var max = -1000.0f;
            var factor = 0.3f;
            var values = new float[height];
            for (var y = 0; y < height; y++)
            {
                var value = (1 - factor) * HeightNoiseMap.GetValue(0, y) + factor * HeightNoiseMap.GetValue(0, height - y);
                values[y] = value;
                if (value < min) min = value;
                else if (value > max) max = value;
            }
            for (var y = 0; y < height; y++)
            {
                var value = values[y];
                value = (value - min) / (max - min);
                values[y] = value;
            }
            
            min = 1000.0f;
            max = -1000.0f;
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = ColorNoiseMap.GetValue(x, y);
                    if (value < min) min = value;
                    else if (value > max) max = value;
                }
            
            var turbulence = (float) Sqrt(0.003 * Mass / EarthMass);
            var d = turbulence * 0.03f;
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = ColorNoiseMap.GetValue(x, y);
                    value = (value - min) / (max - min);
                    value = (1 - 2 * d) * y / height + d + d * value;
                    var i = (int) (value * height);
                    if (i < 0) i = 0;
                    if (i > height - 1) i = height - 1;
                    value = values[i];
                    var a = (float) Sin(PI * y / height);
                    a *= a * a;
                    value *= a;
                    value *= turbulence;
                    ColorNoiseMap.SetValue(x, y, value);
                }
        }

        public override void WriteColorMap(int height, string filePath)
        {
            var width = 2 * height;
            if (ColorNoiseMap == null) GenerateColorMap(height);
            var bitmap = new DirectBitmap(width, height);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var col = ColorScale.Color(ColorNoiseMap.GetValue(x, y));
                    bitmap.SetPixel(x, y, col);
                }

            var total = 0.0;
            var count = 0;
            for (var x = 0; x < width; x += 10)
                for (var y = 0; y < height; y += 10)
                {
                    var col = bitmap.GetPixel(x, y);
                    if (HasAtmosphere) col = Interpolate(col, AtmosphereColor, AtmosphereOpacity);
                    var value = (col.R + col.G + col.B) / (255 * 3.0);
                    value *= value;
                    total += value;
                    count++;
                }
            Albedo = total / count;
            
            bitmap.Save(filePath);
        }
    }

    /// <summary>Represents a rocky planet which may have an atmosphere, oceans or life.</summary>
    public class RockyPlanet : Planet
    {
        private ColorScale LifeColorScale;
        private float MinHeight;
        private float MaxHeight;
        private float IceFactor;

        /// <summary>Whether the planet has an ocean.</summary>
        public bool HasOcean { get; protected set; }
        /// <summary>The color of the ice on the planet.</summary>
        public Color IceColor { get; protected set; }
        /// <summary>The lowest average temperature on the surface of the planet in [K].</summary>
        public float MinTemperature { get; protected set; }
        /// <summary>The highest average temperature on the surface of the planet in [K].</summary>
        public float MaxTemperature { get; protected set; }
        /// <summary>The color of the ocean on the planet.</summary>
        public Color OceanColor { get; protected set; }
        /// <summary>The height of the surface of the ocean between 0.0 and 1.0.</summary>
        public float OceanLevel { get; protected set; }
        /// <summary>The difference between the highest and lowest altitudes on the planet in [m].</summary>
        public double TerrainScale { get; protected set; }

        /// <summary>Initializes an instance of the RockyPlanet class with a specified host star, orbital parent and name to seed the random number generator.</summary>
        /// <param name="star">The star which the planet is closest to.</param>
        /// <param name="parent">The body which the planet orbits.</param>
        /// <param name="name">The name of the planet used to seed the random number generator.</param>
        public RockyPlanet(Body parent, Star star, string name = null) : base(parent, star, name)
        {
            Radius = EarthRadius * Pow(Mass / EarthMass, Random.Uniform("radius", 0.24, 0.32));
            Volume = SphereVolume(Radius);
            Density = Mass / Volume;
            SurfaceGravity = GravitationalAcceleration(Mass, Radius);
            RotationPeriod = Exp(11 + Random.Gaussian("rotation period", 2));
            Albedo = Random.Uniform("albedo", 0, 0.6);
            EffectiveTemperature = EffectiveTemperature * Pow(1 - Albedo, 0.25);
            CoreTemperature = 5500 * Pow(Mass / 6e24, 0.4) * Random.Uniform("core temperature", 0.9, 1.1);
            GeothermalTemperature = 100 * Pow(CoreTemperature / 6000, 0.62);

            if (GravitationalAcceleration(Parent.Mass, Orbit.SemiMajorAxis - Radius) -
                GravitationalAcceleration(Parent.Mass, Orbit.SemiMajorAxis + Radius) > 1e-5)
            {
                RotationPeriod = Orbit.Period;
                TidallyLocked = true;
            }

            var atmospherePalette = Random.Choice("atmosphere palette", Water, Sand, Fish, Earth);
            AtmosphereColor = Random.Color("atmosphere color", atmospherePalette);
            HasAtmosphere = Random.Rand("atmosphere") < 0.7 * Radius / EarthRadius;
            if (HasAtmosphere)
                AtmosphereOpacity = Pow(Random.Rand("atmosphere opacity"), EarthRadius / Radius);
            if (EffectiveTemperature > FreezingPoint)
                AtmosphereOpacity = Pow(AtmosphereOpacity, Pow(EffectiveTemperature / FreezingPoint, 2));
            AtmospherePressure = EarthAtmoPressure * 2 * Tan(PI / 2 * AtmosphereOpacity);
            if (AtmospherePressure < 1)
            {
                HasAtmosphere = false;
                AtmospherePressure = 0;
            }
            GreenhouseTemp = 13 * Pow(AtmospherePressure, 0.25);
            SurfaceTemperature = Pow(Pow(EffectiveTemperature, 4) + Pow(GeothermalTemperature, 4) + Pow(GreenhouseTemp, 4), 0.25);
            AtmosphereScaleHeight = kB * SurfaceTemperature / (4.5e-26 * SurfaceGravity);
            AtmosphereHeight = AtmosphereScaleHeight * Log(AtmospherePressure);
            IceFactor = (float) Random.Rand("ice factor");
            HasRing = Random.Rand("has ring") < 0.05 * Log(Mass / EarthMass);
            if (HasRing)
            {
                InnerRingRadius = Random.Uniform("inner ring radius", Radius, 3 * Radius);
                OuterRingRadius = Random.Uniform("outer ring radius", InnerRingRadius, 3 * InnerRingRadius);
                var ringPalette = Random.Choice("ring palette", Ice, Sand);
                RingColor = Pow(Random.Color("ring color", ringPalette), 0.4);
                RingInclination = PI / 2 * Pow(Random.Rand("ring inclination"), 10);
            }

            OceanColor = Random.Color("ocean color", 
                Random.Choice("ocean palette", Water, Fish, Rust));
            OceanColor = Interpolate(OceanColor, Color.FromArgb(50, 50, 50), 0.4);
            HasOcean = HasAtmosphere && SurfaceTemperature < BoilingPoint && Random.Rand("ocean") < Sqrt(AtmosphereOpacity);
            if (HasOcean)
            {
                OceanLevel = 0.5f + (float) Random.Gaussian("ocean level", 0.25);
                if (OceanLevel > 1) OceanLevel = 1;
                if (OceanLevel < 0) HasOcean = false;
                IceFactor = 1;
                HasLife = SurfaceTemperature > FreezingPoint && Random.Rand("life") < 0.5;
            }

            IceColor = Random.Color("ice color", Ice);

            TerrainScale = (1 - OceanLevel) * 1000 * Exp(Random.Uniform("terrain scale", 0.4, 3));

            Palette palette1;
            Palette palette2;
            if (HasAtmosphere || Random.Rand("palette selection 1") < 0.3)
            {
                palette1 = Random.Choice("palette 1", Sand, Earth);
                palette2 = Random.Choice("palette 2", Earth, Stone);
            }
            else
            {
                if (Random.Rand("palette selection 2") < 0.7)
                {
                    palette1 = Stone;
                    palette2 = Stone;
                }
                else
                {
                    palette1 = Ice;
                    palette2 = Ice;
                }
            }

            var values = new List<double> {0};
            var colors = new List<Color> {Random.Color("color scale color 0", palette1)};

            var done = false;
            var i = 0;
            while (!done)
            {
                i++;
                var value = values[i - 1] + Random.Rand("color scale value " + i);
                var col = Random.Color("color scale color " + i, palette2);
                if (i == 1) col = Random.Color("color scale color " + i, palette1);
                if (value > 1)
                {
                    done = true;
                    value = 1;
                }
                values.Add(value);
                colors.Add(col);
            }
            ColorScale = new ColorScale(values, colors);
            
            var lifeColors = new List<Color>();
            foreach (var col in colors)
                lifeColors.Add(col);
            if (HasLife)
            {
                if (Random.Rand("life color") < 0.5)
                {
                    lifeColors[0] = Random.Color("life color 1", Forest);
                    lifeColors[1] = Random.Color("life color 2", Grass);
                }
                else
                {
                    lifeColors[0] = Random.Color("life color 1", Burgundy);
                    lifeColors[1] = Random.Color("life color 2", Burgundy);
                }

                LifeColorScale = new ColorScale(values, lifeColors);
            }

            if (HasLife) Color = lifeColors[1];
            else if (HasOcean) Color = Interpolate(OceanColor, AtmosphereColor, AtmosphereOpacity);
            else if (HasAtmosphere) Color = Interpolate(colors[1], AtmosphereColor, AtmosphereOpacity);
            else Color = colors[1];
            
            if (parent is Star) AddSatellites((int) (2 * Sqrt(Radius / EarthRadius) + 2 * Sqrt(Orbit.SemiMajorAxis / 1e11)));

            var smoothNoise1 = Random.Noise("map noise 1", 0.5, 2, 1.9, 2.3, 10, 10, 0.4, 0.5);
            var smoothNoise2 = Random.Noise("map noise 2", 0.5, 2, 1.9, 2.3, 10, 10, 0.4, 0.5);
            var smoothNoise3 = Random.Noise("map noise 3", 0.5, 2, 1.9, 2.3, 10, 10, 0.4, 0.5);
            HeightNoiseModule = Random.Choice("noise selection", new Module[]
            {
                new Min {Source0 = smoothNoise1, Source1 = smoothNoise2},
                new Max {Source0 = smoothNoise1, Source1 = smoothNoise2},
                new Multiply {Source0 = smoothNoise1, Source1 = smoothNoise2},
                new Blend {Source0 = smoothNoise1, Source1 = smoothNoise2, Control = smoothNoise3}
            });

            var noise1 = Random.Noise("map noise 1", 0.5, 2, 1.9, 2.3, 10, 10, 0.65, 0.75);
            var noise2 = Random.Noise("map noise 2", 0.5, 2, 1.9, 2.3, 10, 10, 0.65, 0.75);
            var noise3 = Random.Noise("map noise 3", 0.5, 2, 1.9, 2.3, 10, 10, 0.65, 0.75);
            ColorNoiseModule = Random.Choice("noise selection", new Module[]
            {
                new Min {Source0 = noise1, Source1 = noise2},
                new Max {Source0 = noise1, Source1 = noise2},
                new Multiply {Source0 = noise1, Source1 = noise2},
                new Blend {Source0 = noise1, Source1 = noise2, Control = noise3}
            });

            TemperatureNoiseModule = Random.Perlin("temperature noise", 2, 2, 1.9, 2.3, 10, 10, 0.5, 0.5);
        }

        private void GenerateHeightMap(int height)
        {
            var width = 2 * height;
            var builder = new SphereNoiseMapBuilder();
            builder.SetDestSize(width, height);
            builder.SetBounds(-90, 90, -180, 180);
            HeightNoiseMap = new NoiseMap();
            builder.SourceModule = HeightNoiseModule;
            builder.DestNoiseMap = HeightNoiseMap;
            builder.Build();
            
            MinHeight = 10.0f;
            MaxHeight = -10.0f;
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = HeightNoiseMap.GetValue(x, y);
                    if (value < MinHeight) MinHeight = value;
                    else if (value > MaxHeight) MaxHeight = value;
                }

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = HeightNoiseMap.GetValue(x, y);
                    value = (value - MinHeight) / (MaxHeight - MinHeight);
                    HeightNoiseMap.SetValue(x, y, value);
                }
        }

        internal override void GenerateColorMap(int height)
        {
            var width = 2 * height;
            if (HasAtmosphere && TemperatureNoiseMap == null) GenerateTemperatureMap(height);
            else if (HeightNoiseMap == null) GenerateHeightMap(height);
            var builder = new SphereNoiseMapBuilder();
            builder.SetDestSize(width, height);
            builder.SetBounds(-90, 90, -180, 180);
            ColorNoiseMap = new NoiseMap();
            builder.SourceModule = ColorNoiseModule;
            builder.DestNoiseMap = ColorNoiseMap;
            builder.Build();

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = ColorNoiseMap.GetValue(x, y);
                    value = (value - MinHeight) / (MaxHeight - MinHeight);
                    ColorNoiseMap.SetValue(x, y, value);
                }
        }

        private void GenerateTemperatureMap(int height)
        {
            var width = 2 * height;
            if (HeightNoiseMap == null) GenerateHeightMap(height);
            var builder = new SphereNoiseMapBuilder();
            builder.SetDestSize(width, height);
            builder.SetBounds(-90, 90, -180, 180);
            TemperatureNoiseMap = new NoiseMap();
            builder.SourceModule = TemperatureNoiseModule;
            builder.DestNoiseMap = TemperatureNoiseMap;
            builder.Build();

            MinTemperature = 1000000.0f;
            MaxTemperature = 0.0f;

            var solarHeat = Pow(SurfaceTemperature, 4);
            var extraHeat = Pow(GeothermalTemperature, 4) + Pow(GreenhouseTemp, 4);
            var coldness = 1 - (float) Random.Uniform("coldness", Pow(extraHeat/(solarHeat + extraHeat), 0.25), 1);
            
            if (HasAtmosphere)
            {
                for (var x = 0; x < width; x++)
                    for (var y = 0; y < height; y++)
                    {
                        var heat = extraHeat + solarHeat * 2 * Sin(PI * y / height);
                        var temperature = (float) Pow(heat, 0.25);
                        temperature *= 1 - coldness * HeightNoiseMap.GetValue(x, y);
                        if (temperature < MinTemperature) MinTemperature = temperature;
                        else if (temperature > MaxTemperature) MaxTemperature = temperature;
                        temperature *= 1 + 0.05f * (TemperatureNoiseMap.GetValue(x, y) - 1);
                        TemperatureNoiseMap.SetValue(x, y, temperature);
                    }
            }
        }

        /// <summary>Writes an equirectangular height map of the planet at the specified path.</summary>
        /// <param name="height">The height of the output image, half the width.</param>
        /// <param name="filePath">The path of the output file including file extension.</param>
        public void WriteHeightMap(int height, string filePath)
        {
            if (HeightNoiseMap == null) GenerateHeightMap(height);
            var bitmap = new DirectBitmap(HeightNoiseMap.Width, HeightNoiseMap.Height);
            for (var x = 0; x < HeightNoiseMap.Width; x++)
                for (var y = 0; y < HeightNoiseMap.Height; y++)
                {
                    var col = Interpolate(Color.Black, Color.White, HeightNoiseMap.GetValue(x, y));
                    bitmap.SetPixel(x, y, col);
                }

            bitmap.Save(filePath);
        }

        public override void WriteColorMap(int height, string filePath)
        {
            var width = 2 * height;
            if (ColorNoiseMap == null) GenerateColorMap(height);
            var bitmap = new DirectBitmap(width, height);
            var tempMean = Random.Uniform("life temperature mean", FreezingPoint, BoilingPoint);
            var tempVariance = Random.Uniform("life temperature variance", 20, 70);
            var sinPower = Random.Uniform("sin power", 0, 2);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = ColorNoiseMap.GetValue(x, y);
                    var col = OceanColor;
                    if (!HasOcean || HeightNoiseMap.GetValue(x, y) > OceanLevel)
                        col = ColorScale.Color(value);
                    if (HasLife && HeightNoiseMap.GetValue(x, y) > OceanLevel)
                    {
                        var delta = Abs(TemperatureNoiseMap.GetValue(x, y) - tempMean) / tempVariance;
                        if (delta < 0) delta = 0;
                        if (delta > 1) delta = 1;
                        delta *= (float) Pow(Sin(PI * y / height), sinPower);
                        col = Interpolate(col, LifeColorScale.Color(value), delta);
                    }
    
                    if (HasAtmosphere && TemperatureNoiseMap.GetValue(x, y) < FreezingPoint * IceFactor)
                        col = IceColor;
                    bitmap.SetPixel(x, y, col);
            }
            bitmap.Save(filePath);
        }

        /// <summary>Writes an equirectangular specular map of the planet at the specified path.</summary>
        /// <param name="height">The height of the output image, half the width.</param>
        /// <param name="filePath">The path of the output file including file extension.</param>
        public void WriteSpecMap(int height, string filePath)
        {
            if (HasOcean)
            {
                var bitmap = new DirectBitmap(HeightNoiseMap.Width, HeightNoiseMap.Height);
                if (HasOcean)
                    for (var x = 0; x < HeightNoiseMap.Width; x++)
                        for (var y = 0; y < HeightNoiseMap.Height; y++)
                        {
                            var col = Color.Black;
                            if (HeightNoiseMap.GetValue(x, y) <= OceanLevel && TemperatureNoiseMap.GetValue(x, y) > FreezingPoint)
                                col = Color.White;
                            bitmap.SetPixel(x, y, col);
                        }

                bitmap.Save(filePath);
            }
        }

        /// <summary>Writes an equirectangular normal map of the planet at the specified path.</summary>
        /// <param name="height">The height of the output image, half the width.</param>
        /// <param name="filePath">The path of the output file including file extension.</param>
        /// <param name="altMode">Whether to use an alternate normal map format.</param>
        public void WriteNormalMap(int height, string filePath, bool altMode = false)
        {
            var width = 2 * height;
            if (HeightNoiseMap == null) GenerateHeightMap(height);
            var array = new float[width, height];
            var bitmap = new DirectBitmap(width, height);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var value = HeightNoiseMap.GetValue(x, y);
                    if (value < OceanLevel) value = 0;
                    else value = (value - OceanLevel) / (1 - OceanLevel);
                    value *= (float) Sin(PI * y / height);
                    array[x, y] = value;
                }

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var r = 127;
                    var g = 127;
                    var b = 255;
                    if (x > 0 && x < width - 1 && y > 0 && y < height - 1)
                    {
                        var vx = new Vector3(100f / height, 0, array[x + 1, y] - array[x - 1, y]);
                        var vy = new Vector3(0, 100f / height, array[x, y + 1] - array[x, y - 1]);
                        var c = Vector3.Cross(vx, vy);
                        c /= c.Length();
                        r = (int) (255 * (1 + c.X) / 2);
                        g = (int) (255 * (1 - c.Y) / 2);
                        b = (int) (255 * (1 + c.Z) / 2);
                    }

                    if (altMode)
                    {
                        var A = r;
                        var G = (int) (g * A / 255.0);
                        if (A < 0 || A > 255 || G < 0 || G > 255)
                            bitmap.SetPixel(x, y, Color.FromArgb(127, 255, 127, 255));
                        else
                            bitmap.SetPixel(x, y, Color.FromArgb(A, G, G, G));
                    }
                    else
                    {
                        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
                            bitmap.SetPixel(x, y, Color.FromArgb(127, 127, 255));
                        else
                            bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

            bitmap.Save(filePath);
        }
        
        public bool ShouldSerializeIceColor()
        {
            return !IceColor.IsEmpty;
        }
        
        public bool ShouldSerializeOceanLevel()
        {
            return HasOcean;
        }
        
        public bool ShouldSerializeOceanColor()
        {
            return HasOcean && !OceanColor.IsEmpty;
        }
    }
}