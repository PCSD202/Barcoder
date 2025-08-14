namespace Barcoder;

using SkiaSharp;

public enum ScaleMode
{
    Stretch, // Fill canvas, distort if needed
    None,    // No scaling, center
    Shrink   // Scale down to fit, keep aspect, no enlarge
}
public static class LabelCanvas
{
    public static SKImage Compose(
        SKImage src,
        int canvasWidth,
        int canvasHeight,
        ScaleMode mode,
        SKColor? background = null)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (canvasWidth <= 0 || canvasHeight <= 0) throw new ArgumentOutOfRangeException(nameof(canvasWidth));

        using var surface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
        var canvas = surface.Canvas;

        canvas.ResetMatrix();
        canvas.Clear(background ?? SKColors.Transparent);

        using var paint = new SKPaint();
        paint.IsAntialias = false;

        if (mode == ScaleMode.Stretch)
        {
            // Force to exactly fill the canvas
            canvas.DrawImage(src, new SKRect(0, 0, canvasWidth, canvasHeight), new SKSamplingOptions(SKFilterMode.Linear), paint);
        }
        else if (mode == ScaleMode.Shrink)
        {
            // Maintain aspect ratio, shrink if too large, center result
            float scale = Math.Min(
                (float)canvasWidth / src.Width,
                (float)canvasHeight / src.Height);

            if (scale > 1f) scale = 1f; // No enlargement

            var drawWidth = src.Width * scale;
            var drawHeight = src.Height * scale;

            float x = (canvasWidth - drawWidth) / 2f;
            float y = (canvasHeight - drawHeight) / 2f;

            var dest = new SKRect(x, y, x + drawWidth, y + drawHeight);
            canvas.DrawImage(src, dest, paint);
        }
        else // None
        {
            // No scaling at all, draw 1:1 centered, clipped
            int x = (canvasWidth - src.Width) / 2;
            int y = (canvasHeight - src.Height) / 2;

            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, canvasWidth, canvasHeight), SKClipOperation.Intersect, antialias: false);
            canvas.DrawImage(src, new SKPointI(x, y), paint);
            canvas.Restore();
        }

        return surface.Snapshot();
    }
}