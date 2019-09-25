using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using static Universe.Physics;
using static Universe.Colors;

namespace Universe
{
    /// <summary>Represents an unspecified celestial body.</summary>
    public abstract class Body
    {
        /// <summary>The random number generator of the body.</summary>
        [JsonIgnore] protected StringSeededRandom Random { get; }
        /// <summary>The name of the body, used to seed the random number generator.</summary>
        public string Name { get; }
        /// <summary>A list of natural satellites that orbit the body.</summary>
        [JsonIgnore] public List<Body> Children { get; protected set; } = new List<Body>();
        /// <summary>A dictionary that maps the names of natural satellites to the bodies.</summary>
        [JsonIgnore] public Dictionary<string, Body> ChildrenDict { get; protected set; } = new Dictionary<string, Body>();
        /// <summary>The characteristic color of the body.</summary>
        public Color Color { get; protected set; }
        /// <summary>The effective temperature at the surface of the body in [K].</summary>
        public double EffectiveTemperature { get; protected set; }
        /// <summary>The maximum distance at which the body's gravitational field has an effect in [m].</summary>
        public double SphereOfInfluence { get; protected set; }
        /// <summary>The mass of the body in [kg].</summary>
        public double Mass { get; protected set; }
        /// <summary>The radius of the body in [m].</summary>
        public double Radius { get; protected set; }
        /// <summary>The volume of the body in [m³].</summary>
        public double Volume { get; protected set; }
        /// <summary>The density of the body in [kg/m³].</summary>
        public double Density { get; protected set; }
        /// <summary>The period of rotation of the body in [s].</summary>
        public double RotationPeriod { get; protected set; }
        /// <summary>The acceleration due to gravity at the surface of the body in [m/s²].</summary>
        public double SurfaceGravity { get; protected set; }
        /// <summary>The star which the body is closest to.</summary>
        [JsonIgnore] public Star Star { get; protected set; }
        /// <summary>The name of the star system to which this body belongs.</summary>
        public string System { get; protected set; }
        /// <summary>The body which the body orbits.</summary>
        [JsonIgnore] public Body Parent { get; protected set; }
        /// <summary>The geometrically parameterized orbit of the body around its parent.</summary>
        public Orbit Orbit { get; protected set; }
        /// <summary>Whether the body, or any of its children have life.</summary>
        public bool HasLife { get; protected set; }
        /// <summary>Whether the planet has a ring.</summary>
        public bool HasRing { get; protected set; }
        /// <summary>The color of the ring of the planet.</summary>
        public Color RingColor { get; protected set; }
        /// <summary>The inner radius of the ring of the planet in [m].</summary>
        public double InnerRingRadius { get; protected set; }
        /// <summary>The outer radius of the ring of the planet in [m].</summary>
        public double OuterRingRadius { get; protected set; }
        /// <summary>The inclination of the ring in [rad].</summary>
        public double RingInclination { get; protected set; }
        /// <summary>Whether the planet is tidally locked to its parent.</summary>
        public bool TidallyLocked { get; protected set; }
        /// <summary>The SharpNoise Module used to generate noise for colorNoiseMap.</summary>
        [JsonIgnore] protected Module ColorNoiseModule;
        /// <summary>A SharpNoise NoiseMap used to introduce noise to the color map.</summary>
        [JsonIgnore] public NoiseMap ColorNoiseMap { get; protected set; }
        /// <summary>The SharpNoise Module used to generate noise for temperatureNoiseMap.</summary>
        [JsonIgnore] protected Module TemperatureNoiseModule;
        /// <summary>A SharpNoise NoiseMap of the average temperature across the surface of the planet.</summary>
        [JsonIgnore] public NoiseMap TemperatureNoiseMap { get; protected set; }
        /// <summary>The SharpNoise Module used to generate noise for ringNoiseMap.</summary>
        [JsonIgnore] protected Module RingNoiseModule;
        /// <summary>A SharpNoise NoiseMap used to generate the ring map.</summary>
        [JsonIgnore] public NoiseMap RingNoiseMap { get; protected set; }

        /// <summary>Initializes a new instance of the Body class with a specified name.</summary>
        /// <param name="star">The star which the body is closest to.</param>
        /// <param name="parent">The parent body which the body orbits around.</param>
        /// <param name="name">The name of the body, used to seed the random number generator.</param>
        protected Body(Body parent, Star star, string name = null)
        {
            Parent = parent;
            Star = star;
            if (name == null)
                name = new Names(Name).Name(new Random().NextDouble().ToString(CultureInfo.InvariantCulture));
            Name = name.ToLower();
            if (this is Star) System = Name;
            else if (Star != null) System = Star.Name;
            Random = new StringSeededRandom(name);
        }

        /// <summary>Generates a new natural satellite orbiting the body.</summary>
        /// <returns>A natural satellite orbiting the body.</returns>
        private Body NewSatellite(string seedString)
        {
            var nameGenerator = new Names(Name);
            var satelliteName = nameGenerator.Name(seedString);
            Planet planet = new RockyPlanet(this, Star, satelliteName);
            if (planet.Mass > 10 * EarthMass) planet = new GiantPlanet(this, Star, satelliteName);
            return planet;
        }

        /// <summary>Generates a new natural satellite orbiting the body and adds it to the body's list of children.</summary>
        protected void AddSatellite()
        {
            var satellite = NewSatellite(Children.Count.ToString());
            if (!(satellite.Orbit.SemiMajorAxis < SphereOfInfluence)) return;
            Children.Add(satellite);
            ChildrenDict.Add(satellite.Name, satellite);
        }

        /// <summary>Generates a number of new natural satellites orbiting the body and adds them to the body's list of children.</summary>
        /// <param name="maxCount">The upper bound on the number of satellites to be added to the list of children.</param>
        protected void AddSatellites(int maxCount)
        {
            var count = Random.Randint("satellite count", (int) (0.3 * maxCount), maxCount);
            var j = 0;
            for (var i = 0; i < count;)
            {
                j++;
                var satellite = NewSatellite(j.ToString());
                if (satellite.Orbit.SemiMajorAxis > SphereOfInfluence) i = count;
                else
                    if (!ChildrenDict.ContainsKey(satellite.Name))
                    {
                        Children.Add(satellite);
                        ChildrenDict.Add(satellite.Name, satellite);
                        i++;
                    }
            }
        }

        internal abstract void GenerateColorMap(int height);
        
        /// <summary>Saves an equirectangular color map of the body at the specified path.</summary>
        /// <param name="height">The height of the output image, half the width.</param>
        /// <param name="filePath">The path of the output file including file extension.</param>
        public abstract void WriteColorMap(int height, string filePath);
        
        private void GenerateRingMap(int width)
        {
            var height = 32;

            RingNoiseModule = Random.Perlin("ring noise", 0.25, 2, 1.8, 2.2, 10, 16, 0.65, 0.75);
            RingNoiseModule = new Add
            {
                Source0 = RingNoiseModule,
                Source1 = new Constant {ConstantValue = Random.Uniform("ring offset", -0.5, 0.5)}
            };
            
            var builder = new SphereNoiseMapBuilder();
            RingNoiseMap = new NoiseMap();
            builder.SourceModule = RingNoiseModule;
            builder.SetDestSize(width, height);
            builder.DestNoiseMap = RingNoiseMap;
            builder.SetBounds(0, 0.00000001, -180, 180);
            builder.Build();
        }
        
        /// <summary>Writes a ring texture at the specified path.</summary>
        /// <param name="height">The height of the output image.</param>
        /// <param name="filePath">The path of the output file including file extension.</param>
        public void WriteRingMap(int height, string filePath)
        {
            if (!HasRing) return;
            var transparent = false;
            if (filePath != null) transparent = Path.GetExtension(filePath).Equals(".png");
            if (RingNoiseMap == null) GenerateRingMap(height);
            var bitmap = new DirectBitmap(RingNoiseMap.Width, RingNoiseMap.Height);
            for (var x = 0; x < RingNoiseMap.Width; x++)
                for (var y = 0; y < RingNoiseMap.Height; y++)
                {
                    Color col;
                    if (transparent)
                    {
                        var alpha = (int) (255 * RingNoiseMap.GetValue(x, y));
                        if (alpha < 0) alpha = 0;
                        if (alpha > 255) alpha = 255;
                        col = Color.FromArgb(alpha, RingColor.R, RingColor.G, RingColor.B);
                    }
                    else
                        col = Interpolate(Color.Black, RingColor, RingNoiseMap.GetValue(x, y));
                    bitmap.SetPixel(x, y, col);
                }
            bitmap.Save(filePath);
        }

        /// <summary>Writes a configuration file containing the basic parameters of the body.</summary>
        /// <param name="filePath">The path of the output file including file extension.</param>
        public void WriteConfig(string filePath)
        {
            if (!Directory.Exists(Directory.GetParent(filePath).ToString())) Directory.CreateDirectory(Directory.GetParent(filePath).ToString());
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {NullValueHandling = NullValueHandling.Ignore});
            json = json.Replace("\"", "").Replace(",\r", "\r");
            //var rgx = new Regex("\"");
            //json = rgx.Replace(json, "");
            File.WriteAllText(filePath, json);
        }

        public bool ShouldSerializeColor()
        {
            return !Color.IsEmpty;
        }
        
        public bool ShouldSerializeRingColor()
        {
            return HasRing && !RingColor.IsEmpty;
        }

        public bool ShouldSerializeInnerRingRadius()
        {
            return HasRing;
        }
        
        public bool ShouldSerializeOuterRingRadius()
        {
            return HasRing;
        }
    }
}