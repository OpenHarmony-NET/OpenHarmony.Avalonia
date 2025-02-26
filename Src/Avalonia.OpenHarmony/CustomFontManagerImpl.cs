using Avalonia.Media.Fonts;
using Avalonia.Media;
using Avalonia.Platform;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using OpenHarmony.NDK.Bindings.Native;
using System.Runtime.Loader;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace Avalonia.OpenHarmony;

public class CustomFontManagerImpl : IFontManagerImpl
{
    private readonly string _defaultFamilyName;
    private readonly IFontCollection _customFonts;
    private bool _isInitialized;

    public CustomFontManagerImpl()
    {
        _defaultFamilyName = "HarmonyOS Sans";

        var source = new Uri("resm:Avalonia.OpenHarmony.Assets?assembly=Avalonia.OpenHarmony");

        _customFonts = new EmbeddedFontCollection(source, source);

    }

    public string GetDefaultFontFamilyName()
    {
        return _defaultFamilyName;
    }

    public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false)
    {
        if (!_isInitialized)
        {
            _customFonts.Initialize(this);

            _isInitialized = true;
        }

        return _customFonts.Select(x => x.Name).ToArray();
    }

    private readonly string[] _bcp47 = { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

    public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
        CultureInfo culture, out Typeface typeface)
    {
        if (!_isInitialized)
        {
            _customFonts.Initialize(this);
        }

        if (_customFonts.TryMatchCharacter(codepoint, fontStyle, fontWeight, fontStretch, null, culture, out typeface))
        {
            return true;
        }

        var fallback = SKFontManager.Default.MatchCharacter(null, (SKFontStyleWeight)fontWeight,
            (SKFontStyleWidth)fontStretch, (SKFontStyleSlant)fontStyle, _bcp47, codepoint);

        typeface = new Typeface(fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

        return true;
    }

    public bool TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight,
        FontStretch stretch, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
    {
        if (!_isInitialized)
        {
            _customFonts.Initialize(this);
        }

        if (familyName == "$Default")
            familyName = _defaultFamilyName;
        if (_customFonts.TryGetGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface))
        {
            return true;
        }


        var assetLoader = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
        var skTypeface = SKTypeface.FromFamilyName(familyName,
                    (SKFontStyleWeight)weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)style);


        glyphTypeface =  GetGlyphTypeface(skTypeface, FontSimulations.None);

        return true;
    }

    public bool TryCreateGlyphTypeface(Stream stream, FontSimulations fontSimulations, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
    {
        var skTypeface = SKTypeface.FromStream(stream);
        glyphTypeface = GetGlyphTypeface(skTypeface, fontSimulations);
        return true;
    }

    public IGlyphTypeface GetGlyphTypeface(SKTypeface skTypeface, FontSimulations fontSimulations)
    {
        Type? type = null;
        Assembly? currentAssembly = null;
        foreach (var alc in AssemblyLoadContext.All)
        {
            foreach (var assembly in alc.Assemblies)
            {
                type = assembly.GetType("Avalonia.Skia.GlyphTypefaceImpl");
                currentAssembly = assembly;
                if (type != null)
                    break;
            }
            if (type != null)
                break;
        }
        try
        {
            var ctor = type.GetConstructor([typeof(SKTypeface), typeof(FontSimulations)]);
            object obj = RuntimeHelpers.GetUninitializedObject(type);
            ctor.Invoke(obj, [skTypeface, fontSimulations]);
            return (IGlyphTypeface)obj;

        }
        catch (Exception ex)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.Message);
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.StackTrace);
            if (ex.InnerException != null)
            {

                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.InnerException.Message);
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.InnerException.StackTrace);
            }
            throw ex;
        }
        return null;
    }

   
}