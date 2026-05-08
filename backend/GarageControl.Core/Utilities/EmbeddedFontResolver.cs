using PdfSharpCore.Fonts;
using System.Reflection;

namespace GarageControl.Core.Utilities
{
    public class EmbeddedFontResolver : IFontResolver
    {
        public string DefaultFontName => "Noto Sans";

        public byte[] GetFont(string faceName)
        {
            var assembly = typeof(EmbeddedFontResolver).Assembly;
            var resourceName = $"GarageControl.Core.Fonts.{faceName}.ttf";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception($"Font resource not found: {resourceName}");

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // We only have Noto Sans, so we map everything to it to avoid crashes
            // This ensures that even if MigraDoc asks for Arial or something else, we provide a valid font.
            if (isBold && isItalic)
            {
                return new FontResolverInfo("NotoSans-BoldItalic");
            }
            else if (isBold)
            {
                return new FontResolverInfo("NotoSans-Bold");
            }
            else if (isItalic)
            {
                return new FontResolverInfo("NotoSans-Italic");
            }
            else
            {
                return new FontResolverInfo("NotoSans-Regular");
            }
        }
    }
}
