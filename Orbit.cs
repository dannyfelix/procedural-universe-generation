using Newtonsoft.Json;
using static System.Math;
using static Universe.Physics;

namespace Universe
{
    /// <summary>Represents the orbit of a celestial body around another based on six geometric parameters.</summary>
    public class Orbit
    {
        /// <summary>The semi-major axis of the orbit in [m].</summary>
        public double SemiMajorAxis { get; }
        /// <summary>The argument of the periapsis of the orbit in [rad].</summary>
        public double ArgumentOfPeriapsis { get; }
        /// <summary>The longitude of the ascending node of the orbit in [rad].</summary>
        public double AscendingNode { get; }
        /// <summary>The eccentricity of the orbit.</summary>
        public double Eccentricity { get; }
        /// <summary>The inclination of the orbit in [rad].</summary>
        public double Inclination { get; }
        /// <summary>The mean anomaly of the orbit in [rad].</summary>
        public double MeanAnomaly { get; }
        /// <summary>The body which this orbit circumscribes.</summary>
        [JsonIgnore] public Body Parent { get; }
        /// <summary>The orbital period in [s].</summary>
        public double Period { get; }
        /// <summary>The periapsis of the orbit in [m].</summary>
        public double Periapsis { get; }
        /// <summary>The apoapsis of the orbit in [m].</summary>
        public double Apoapsis { get; }
        /// <summary>The mean orbital velocity in [m/s].</summary>
        public double Velocity { get; }

        /// <summary>Initializes an instance of the Orbit class with a specified parent and random number generator belonging to the orbiting body.</summary>
        /// <param name="parent">The body which the orbit circumscribes.</param>
        /// <param name="random">The seeded random number generator belonging to the orbiting body.</param>
        public Orbit(Body parent, StringSeededRandom random)
        {
            Parent = parent;
            double minA;
            if (parent.Children.Count == 0)
                minA = Sqrt(G * parent.Mass / Pow(10, random.Uniform("first satellite gravity", -2, 0)));
            else
                minA = 1.5 * parent.Children[parent.Children.Count - 1].Orbit.SemiMajorAxis;
            if (parent.HasRing)
                if (parent.InnerRingRadius < minA && minA < parent.OuterRingRadius) minA = parent.OuterRingRadius;
            
            var maxA = 1.5 * minA;
            if (parent.HasRing)
                if (parent.InnerRingRadius < maxA && maxA < parent.OuterRingRadius) maxA = parent.InnerRingRadius;
            
            if (maxA > parent.SphereOfInfluence)
                maxA = parent.SphereOfInfluence;
            Eccentricity = Pow(random.Rand("eccentricity"), 10);
            SemiMajorAxis = random.Uniform("semi-major axis", minA, maxA);
            Inclination = PI / 2 * Pow(random.Rand("inclination"), 10);
            AscendingNode = random.Uniform("ascending node", 0, 2 * PI);
            ArgumentOfPeriapsis = random.Uniform("argument of ascending node", 0, 2 * PI);
            MeanAnomaly = random.Uniform("mean anomaly", 0, 2 * PI);
            Period = OrbitalPeriod(SemiMajorAxis, parent.Mass);
            Periapsis = Periapsis(Eccentricity, SemiMajorAxis);
            Apoapsis = Apoapsis(Eccentricity, SemiMajorAxis);
            Velocity = OrbitalVelocity(SemiMajorAxis, Period);
        }
    }
}