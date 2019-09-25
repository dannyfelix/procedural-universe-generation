using System.Drawing;
using static System.Math;

namespace Universe
{
    /// <summary>Provides static methods for astrophysical formulae and relevant physical constants and astronomical quantities in SI units.</summary>
    public static class Physics
    {
        /// <summary>The Stefan-Boltzmann constant in [W/m²K⁴].</summary>
        public const double Sigma = 5.67037e-8;
        /// <summary>The Boltzmann constant in [J/K].</summary>
        public const double kB = 1.38065e-23;
        /// <summary>The gravitational constant in [m³/kgs²].</summary>
        public const double G = 6.67408e-11;
        /// <summary>The freezing point of water in [K].</summary>
        public const double FreezingPoint = 273.15;
        /// <summary>The boiling point of water in [K].</summary>
        public const double BoilingPoint = 373.13;
        
        /// <summary>The mass of the Sun in [kg].</summary>
        public const double SolarMass = 1.9885e30;
        /// <summary>The radius of the Sun in [m].</summary>
        public const double SolarRadius = 695700000;
        /// <summary>The temperature of the Sun in [K].</summary>
        public const double SolarTemperature = 5778;
        /// <summary>The luminosity of the Sun in [W].</summary>
        public const double SolarLuminosity = 3.828e26;
        
        /// <summary>The mass of Jupiter in [kg].</summary>
        public const double JupiterMass = 1.898e27;
        /// <summary>The radius of Jupiter in [m].</summary>
        public const double JupiterRadius = 69911000;
        
        /// <summary>The mass of the Earth in [kg].</summary>
        public const double EarthMass = 5.972e24;
        /// <summary>The radius of the Earth in [m].</summary>
        public const double EarthRadius = 6378000;
        /// <summary>The atmospheric pressure at the Earth's surface in [Pa].</summary>
        public const double EarthAtmoPressure = 101325;
        /// <summary>The acceleration due to gravity at the Earth's surface in [m/s²].</summary>
        public const double EarthGravity = 9.80665;

        /// <summary>Converts the absolute magnitude of a star to its luminosity in [W].</summary>
        /// <param name="absoluteMagnitude">Absolute magnitude.</param>
        /// <returns>The luminosity of the star in [W]</returns>
        public static double MagnitudeToLuminosity(double absoluteMagnitude)
        {
            return SolarLuminosity * Pow(10, 1.93 - 0.4 * absoluteMagnitude);
        }

        /// <summary>Converts the B-V magnitude of a star to its temperature in [K].</summary>
        /// <param name="bvMagnitude">B-V color magnitude.</param>
        /// <returns>The temperature of the star in [k].</returns>
        public static double BVToTemperature(double bvMagnitude)
        {
            return 4600 * (1.0 / (bvMagnitude + 1.70) + 1.0 / (bvMagnitude + 0.62)) + 30 * Exp(-20 * bvMagnitude);
        }

        /// <summary>Converts the B-V magnitude of a star to a color.</summary>
        /// <param name="bvMagnitude">B-V color magnitude.</param>
        /// <returns>The color of the star.</returns>
        public static Color BVToColor(double bvMagnitude)
        {
            return TemperatureToColor(BVToTemperature(bvMagnitude));
        }
        
        /// <summary>Converts the temperature of a star to a color.</summary>
        /// <param name="temp">The temperature of the star in [K].</param>
        /// <returns>The color of the star.</returns>
        public static Color TemperatureToColor(double temp)
        {
            int r;
            int g;
            int b;
            if (temp < 5800)
            {
                r = 255;
                g = (int) (255 * (1 - 0.6 * Pow((temp - 5800) / 4300, 2)));
                b = (int) (255 * (1 - Pow((temp - 5800) / 4300, 2)));
            }
            else
            {
                r = (int) (255 * (0.6 + 0.4 * Exp(1 - temp / 5800)));
                g = (int) (255 * (0.7 + 0.3 * Exp(1 - temp / 5800)));
                b = 255;
            }
            if (r < 0) r = 0;
            if (r > 255) r = 25;
            if (g < 0) g = 0;
            if (g > 255) g = 255;
            if (b < 0) b = 0;
            if (b > 255) b = 255;
            
            return Color.FromArgb(r, g, b);
        }

        /// <summary>Calculates the radius of a star in [m] based on its luminosity in [W] and its effective temperature in [K].</summary>
        /// <param name="luminosity">The luminosity of the star in [W].</param>
        /// <param name="effectiveTemperature">the effective temperature of the star in [K].</param>
        /// <returns>The radius of the star in [m].</returns>
        public static double StarRadius(double luminosity, double effectiveTemperature)
        {
            return Sqrt(luminosity / (4 * PI * Sigma * Pow(effectiveTemperature, 4)));
        }

        /// <summary>Calculates the period of an orbit at a specified distance around a body of specified mass.</summary>
        /// <param name="distance">Distance between the orbiting body and its parent in [m].</param>
        /// <param name="mass">Mass of the parent body in [kg].</param>
        /// <returns>The period of the orbit in [s].</returns>
        public static double OrbitalPeriod(double distance, double mass)
        {
            return 2 * PI * Sqrt(Pow(distance, 3) / (6.67e-11 * mass));
        }

        /// <summary>Calculates the periapsis of an orbit based on the eccentricity and semi-major axis of the orbit.</summary>
        /// <param name="eccentricity">The eccentricity of the orbit.</param>
        /// <param name="semiMajorAxis">The semi-major axis of the orbit in [m].</param>
        /// <returns>The periapsis of the orbit in [m].</returns>
        public static double Periapsis(double eccentricity, double semiMajorAxis)
        {
            return (1 - eccentricity) * semiMajorAxis;
        }
        
        /// <summary>Calculates the apoapsis of an orbit based on the eccentricity and semi-major axis of the orbit.</summary>
        /// <param name="eccentricity">The eccentricity of the orbit.</param>
        /// <param name="semiMajorAxis">The semi-major axis of the orbit in [m].</param>
        /// <returns>The apoapsis of the orbit in [m].</returns>
        public static double Apoapsis(double eccentricity, double semiMajorAxis)
        {
            return (1 + eccentricity) * semiMajorAxis;
        }
        
        /// <summary>Calculates the average orbital velocity of a body based on the semi-major axis and period of the orbit.</summary>
        /// <param name="semiMajorAxis">The semi-major axis of the orbit in [m].</param>
        /// <param name="period">The period of the orbit in [s].</param>
        /// <returns>The average orbital velocity in [m/s].</returns>
        public static double OrbitalVelocity(double semiMajorAxis, double period)
        {
            return 2 * PI * semiMajorAxis / period;
        }

        /// <summary>Calculates the effective temperature due to stellar radiation at a specified distance from a star.</summary>
        /// <param name="star">A star.</param>
        /// <param name="distance">The distance to the star in [m].</param>
        /// <returns>The effective temperature at the specified distance from the star in [K].</returns>
        public static double EffectiveTemperature(Star star, double distance)
        {
            return star.EffectiveTemperature * Sqrt(star.Radius / (2 * distance));
        }

        /// <summary>Calculates the radius of the hill sphere of a body orbiting a parent body at a specified distance.</summary>
        /// <param name="orbit">The orbit of the smaller body around its orbital parent.</param>
        /// <param name="mass">The mass of the smaller body.</param>
        /// <param name="parentMass">The mass of the orbital parent of the smaller body.</param>
        /// <returns></returns>
        public static double HillSphere(Orbit orbit, double mass, double parentMass)
        {
            return orbit.Periapsis * Pow(mass / (3 * parentMass), 1 / 3.0);
        }

        public static double SphereVolume(double radius)
        {
            return 4 * PI / 3 * Pow(radius, 3);
        }

        public static double GravitationalAcceleration(double mass, double distance)
        {
            return G * mass / (distance * distance);
        }
    }
}