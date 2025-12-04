using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace CafeAvalonia.Services
{
    public class SingleFontResolver : IFontResolver
    {
        private const string CustomFontFamily = "MyFont"; // любое имя

        public string DefaultFontName => CustomFontFamily;

        public byte[] GetFont(string faceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resourceName = "CafeAvalonia.Fonts.Roboto-Regular.ttf";

            var names = asm.GetManifestResourceNames();
            Console.WriteLine("RESOURCES: " + string.Join(", ", names));

            var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException($"Не найден встроенный ресурс шрифта '{resourceName}'.");
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }


        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // игнорируем, что просит пользователь — всегда наша гарнитура
            return new FontResolverInfo(CustomFontFamily);
        }
    }
}

