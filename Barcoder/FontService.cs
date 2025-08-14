using System.Reflection;
using SkiaSharp;

namespace Barcoder;

public class FontService
{
    
    private SKTypeface _jetbrainsFont;
    
    public FontService()
    {
        _jetbrainsFont = _GetJetbrainsFont();
    }

    public SKTypeface GetJetbrainsFont()
    {
        return _jetbrainsFont;
    }
    private SKTypeface _GetJetbrainsFont()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Barcoder.Resources.JetBrainsMono-Regular.ttf");

        if (stream == null)
            throw new ArgumentException($"Cannot load font file from an embedded resource. Please make sure that the resource is available");

        return SKTypeface.FromStream(stream);
    }
}