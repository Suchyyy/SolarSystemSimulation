using System.IO;

namespace SolarSystemSimulation
{
    public static class TextureHelper
    {
        public static MemoryStream LoadFileToMemory(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                var ms = new MemoryStream();
                fs.CopyTo(ms);
                return ms;
            }
        }
    }
}