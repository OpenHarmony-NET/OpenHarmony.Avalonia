using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;

namespace Avalonia.OpenHarmony;

public class TestE : EmbeddedFontCollection
{
    public TestE(Uri key, Uri source) : base(key, source)
    {
    }

    public override void Initialize(IFontManagerImpl fontManager)
    {
        foreach (var fontAsset in Directory.GetFiles("/system/fonts"))
            try
            {
                using var stream = File.OpenRead(fontAsset);

                if (fontManager.TryCreateGlyphTypeface(stream, FontSimulations.None, out var glyphTypeface))
                {
                    Call(glyphTypeface);
                }
            }
            catch (Exception e)
            {
                OHDebugHelper.Error("我们无权访问这个字体。", e);
            }
    }

    public override bool TryGetGlyphTypeface(string familyName, FontStyle style, FontWeight weight, FontStretch stretch,
        [NotNullWhen(true)] out IGlyphTypeface? glyphTypeface)
    {
        if (familyName == "$Default")
            familyName = "HarmonyOS Sans SC";
        return base.TryGetGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface);
    }

    private void Call(object? glyphTypeface)
    {
        var type = typeof(EmbeddedFontCollection);
        var method = type.GetMethod("AddGlyphTypeface", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(this, [glyphTypeface]);
    }
}

internal class OpenHarmonyFontManagerImpl : IFontManagerImpl
{
    private readonly SKFontManager _skFontManager = SKFontManager.Default;

    private readonly TestE _testE = new(
        new Uri("fonts:AvaloniaFonts", UriKind.Absolute),
        new Uri("resm:Avalonia.OpenHarmony.Assets?assembly=Avalonia.OpenHarmony"));

    public OpenHarmonyFontManagerImpl()
    {
        FontManager.Current.AddFontCollection(_testE);
    }

    public string GetDefaultFontFamilyName()
    {
        return "HarmonyOS Sans SC";
    }

    public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false)
    {
        return
        [
            "HarmonyOS Sans",
            "HarmonyOS Sans SC",
            "HarmonyOS Sans Condensed",
            "HarmonyOS Sans Digit",
            "Noto Serif",
            "Noto Sans Mono"
        ];
    }

    public bool TryMatchCharacter(int codepoint, FontStyle fontStyle,
        FontWeight fontWeight, FontStretch fontStretch, CultureInfo? culture, out Typeface fontKey)
    {
        return _testE.TryMatchCharacter(codepoint,
            fontStyle,
            fontWeight,
            fontStretch,
            null,
            culture,
            out fontKey);
    }

    public bool TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight,
        FontStretch stretch, [NotNullWhen(true)] out IGlyphTypeface? glyphTypeface)
    {
        return _testE.TryGetGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface);
    }

    public bool TryCreateGlyphTypeface(Stream stream, FontSimulations fontSimulations,
        [NotNullWhen(true)] out IGlyphTypeface? glyphTypeface)
    {
        var skTypeface = SKTypeface.FromStream(stream);

        if (skTypeface != null)
        {
            glyphTypeface = Create(skTypeface, fontSimulations);

            return true;
        }

        glyphTypeface = null;

        return false;
    }

    private IGlyphTypeface Create(object? skTypeface, object? fontSimulations)
    {
        Type? type = null;
        foreach (var alc in AssemblyLoadContext.All)
        {
            foreach (var assembly in alc.Assemblies)
            {
                type = assembly.GetType("Avalonia.Skia.GlyphTypefaceImpl");
                if (type != null)
                    break;
            }

            if (type != null)
                break;
        }

        var ctor = type.GetConstructor([typeof(SKTypeface), typeof(FontSimulations)]);
        var obj = RuntimeHelpers.GetUninitializedObject(type);
        ctor.Invoke(obj, [skTypeface, fontSimulations]);
        return (IGlyphTypeface)obj;
    }

    public bool TryGetFamilyTypefaces(string familyName,
        [NotNullWhen(true)] out IReadOnlyList<Typeface>? familyTypefaces)
    {
        familyTypefaces = null;

        var set = _skFontManager.GetFontStyles(familyName);

        if (set.Count == 0) return false;

        var typefaces = new List<Typeface>(set.Count);

        foreach (var fontStyle in set)
            typefaces.Add(new Typeface(familyName, fontStyle.Slant.ToAvalonia(), (FontWeight)fontStyle.Weight,
                (FontStretch)fontStyle.Width));

        familyTypefaces = typefaces;

        return true;
    }
}