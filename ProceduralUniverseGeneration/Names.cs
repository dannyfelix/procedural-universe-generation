using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Universe
{
    /// <summary>Represents a name generator using a markov-chain algorithm based on the names of various gods, emperors and planets, both real and fictional.</summary>
    public class Names
    {
        private readonly double[] Initials = new double[27];
        private readonly double[,,] Probabilities = new double[27, 27, 27];
        private readonly StringSeededRandom Random;
        private readonly List<string> WordList = new List<string>();

        /// <summary>Initializes a new instance of the Names class with a string to seed the random number generator.</summary>
        /// <param name="seedString">String to seed the random number generator.</param>
        public Names(string seedString)
        {
            Random = new StringSeededRandom(seedString + " names");
            var rgx = new Regex("[^a-zA-Z]");
            var line = "";
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Universe.NameList.txt"))
            {
                if (resourceStream != null)
                    using (var reader = new StreamReader(resourceStream))
                    {
                        line = reader.ReadToEnd();
                    }
            }
            foreach (var word in line.Split(','))
                WordList.Add("`" + rgx.Replace(word, "").ToLower() + "`");
            var totals = new double[27, 27];
            var initialsTotal = 0;
            foreach (var word in WordList)
            {
                Initials[word[1] - 96]++;
                initialsTotal++;
                for (var i = 0; i < word.Length - 2; i++)
                {
                    Probabilities[word[i] - 96, word[i + 1] - 96, word[i + 2] - 96]++;
                    totals[word[i] - 96, word[i + 1] - 96]++;
                }
            }

            for (var i = 0; i < 27; i++)
            {
                Initials[i] /= initialsTotal;
                for (var j = 0; j < 27; j++)
                for (var k = 0; k < 27; k++)
                    Probabilities[i, j, k] /= totals[i, j];
            }
        }

        /// <summary>Randomly generates a name using a markov process with a random number generator seeded by an input string.</summary>
        /// <param name="seedString">String to seed the random number generator.</param>
        /// <returns>A string which is appropriate for use as the name of a celestial body.</returns>
        public string Name(string seedString)
        {
            var word = "`";
            var i = 0;
            var k = 0;
            var minLength = Random.Randint(seedString + " min length", 4, 7);
            var maxLength = 11;
            while (true)
            {
                if (word.Length == 1)
                {
                    var r = Random.Rand(seedString + " initial " + k);
                    var p = 0.0;
                    for (var j = 0; j < 27; j++)
                    {
                        p += Initials[j];
                        if (r < p)
                        {
                            word += (char) (96 + j);
                            break;
                        }
                    }
                }

                while (true)
                {
                    var r = Random.Rand(seedString + i + k);
                    var p = 0.0;
                    for (var j = 0; j < 27; j++)
                    {
                        p += Probabilities[word[i] - 96, word[i + 1] - 96, j];
                        if (r < p)
                        {
                            word += (char) (96 + j);
                            break;
                        }
                    }

                    i++;
                    if (word.Last().Equals('`')) break;
                }

                if (word.Length <= maxLength + 2 && word.Length >= minLength + 2) return word.Trim('`');
                word = "`";
                i = 0;
                k++;
            }
        }
    }
}