using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Utils
{
    public class GenerateCodeConstants
    {
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static Random random = new Random();
        public string GenerateUniqueString(string prefix)
        {
            string newString;
            do
            {
                newString = GenerateRandomString(prefix);
            } while (generatedStrings.Contains(newString));

            generatedStrings.Add(newString);
            return newString;
        }
        private static string GenerateRandomString(string prefix)
        {
            int number = random.Next(100000, 1000000);
            return prefix + number.ToString("D6");
        }
    }
}
