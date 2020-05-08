using System.Reflection;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;

namespace SolarSystemSimulation.Texture
{
    public static class TextureHelper
    {
        public static Material LoadTexture(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(name);
            return new PhongMaterial
            {
                SpecularColor = Color4.White,
                DiffuseMap = stream,
                DiffuseMapSampler = new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter = Filter.Anisotropic
                }
            };
        }
    }
}