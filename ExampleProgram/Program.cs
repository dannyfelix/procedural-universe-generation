using Universe;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // This program will keep generating new stars and searching through their planets until it has found exactly
            // one of each type of planet [giant, rocky (no atmosphere), atmospheric, oceanic, life]. When it finds one
            // of these types that it has not found yet, it writes the relevant textures and the config file and ticks
            // that one off the list.
            
            var doneStar = false;
            var doneRocky = false;
            var doneAtmo = false;
            var doneOcean = false;
            var doneLife = false;
            var doneGiant = false;
            Star star;
            
            while (!doneStar || !doneRocky || !doneAtmo || !doneOcean || !doneLife || !doneGiant)
            {
                star = new Star();
                if (!doneStar)
                {
                    star.WriteColorMap(512, @"Output\" + star.Name + @"\color.png");
                    star.WriteConfig(@"Output\" + star.Name + @"\config.txt");
                    doneStar = true;
                }
                foreach (var planet in star.Children)
                {
                    if (planet is RockyPlanet)
                    {
                        var rockyPlanet = (RockyPlanet) planet;
                        if (!doneLife && rockyPlanet.HasLife)
                        {
                            rockyPlanet.WriteConfig(@"Output\" + rockyPlanet.Name + @"\config.txt");
                            rockyPlanet.WriteColorMap(512, @"Output\" + rockyPlanet.Name + @"\color.png");
                            rockyPlanet.WriteHeightMap(512, @"Output\" + rockyPlanet.Name + @"\height.png");
                            rockyPlanet.WriteNormalMap(512, @"Output\" + rockyPlanet.Name + @"\normal.png");
                            rockyPlanet.WriteSpecMap(512, @"Output\" + rockyPlanet.Name + @"\spec.png");
                            rockyPlanet.WriteRingMap(512, @"Output\" + rockyPlanet.Name + @"\ring.png");
                            doneLife = true;
                        }
                        else if (!doneOcean && rockyPlanet.HasOcean)
                        {
                            rockyPlanet.WriteConfig(@"Output\" + rockyPlanet.Name + @"\config.txt");
                            rockyPlanet.WriteColorMap(512, @"Output\" + rockyPlanet.Name + @"\color.png");
                            rockyPlanet.WriteHeightMap(512, @"Output\" + rockyPlanet.Name + @"\height.png");
                            rockyPlanet.WriteNormalMap(512, @"Output\" + rockyPlanet.Name + @"\normal.png");
                            rockyPlanet.WriteSpecMap(512, @"Output\" + rockyPlanet.Name + @"\spec.png");
                            rockyPlanet.WriteRingMap(512, @"Output\" + rockyPlanet.Name + @"\ring.png");
                            doneOcean = true;
                        }
                        else if (!doneAtmo && rockyPlanet.HasAtmosphere)
                        {
                            rockyPlanet.WriteConfig(@"Output\" + rockyPlanet.Name + @"\config.txt");
                            rockyPlanet.WriteColorMap(512, @"Output\" + rockyPlanet.Name + @"\color.png");
                            rockyPlanet.WriteHeightMap(512, @"Output\" + rockyPlanet.Name + @"\height.png");
                            rockyPlanet.WriteNormalMap(512, @"Output\" + rockyPlanet.Name + @"\normal.png");
                            rockyPlanet.WriteRingMap(512, @"Output\" + rockyPlanet.Name + @"\ring.png");
                            doneAtmo = true;
                        }
                        else if (!doneRocky)
                        {
                            rockyPlanet.WriteConfig(@"Output\" + rockyPlanet.Name + @"\config.txt");
                            rockyPlanet.WriteColorMap(512, @"Output\" + rockyPlanet.Name + @"\color.png");
                            rockyPlanet.WriteHeightMap(512, @"Output\" + rockyPlanet.Name + @"\height.png");
                            rockyPlanet.WriteNormalMap(512, @"Output\" + rockyPlanet.Name + @"\normal.png");
                            rockyPlanet.WriteRingMap(512, @"Output\" + rockyPlanet.Name + @"\ring.png");
                            doneRocky = true;
                        }
                    }
                    else if (!doneGiant)
                    {
                        planet.WriteConfig(@"Output\" + planet.Name + @"\config.txt");
                        planet.WriteColorMap(512, @"Output\" + planet.Name + @"\color.png");
                        planet.WriteRingMap(512, @"Output\" + planet.Name + @"\ring.png");
                        doneGiant = true;
                    }
                }
            }            
        }
    }
}