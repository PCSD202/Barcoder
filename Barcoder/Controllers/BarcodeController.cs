using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.Rendering;
using ZXing.SkiaSharp.Rendering;

namespace Barcoder.Controllers;

[ApiController]
[Route("api/")]
public class BarcodeController(ILogger<BarcodeController> logger, FontService fontService) : ControllerBase
{
    private readonly Writer writer = new MultiFormatWriter();
    private BarcodeFormat GetBarcodeFormat(string format)
    {
        var f = MultiFormatWriter.SupportedWriters.FirstOrDefault(x =>
            string.Equals(x.ToString(), format, StringComparison.CurrentCultureIgnoreCase));
        if (f != default)
        {
            return f;
        };
        throw new InvalidEnumArgumentException(nameof(format), 0, typeof(BarcodeFormat));
    }
    
    [HttpGet("formats")]
    public IActionResult Get()
    {
        return Ok(MultiFormatWriter.SupportedWriters.Select(x=>x.ToString()));
    }
    
    [HttpGet("generate")]
    public IActionResult GenerateGet([FromQuery] BarcodeRequest req)
        => GenerateCore(req);

    [HttpPost("generate")]
    public IActionResult GeneratePost([FromBody] BarcodeRequest req)
        => GenerateCore(req);
    
    private IActionResult GenerateCore(BarcodeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
            return BadRequest("Content must be provided.");

        BarcodeFormat realFormat;
        try
        {
            realFormat = GetBarcodeFormat(req.Format);
        }
        catch (InvalidEnumArgumentException)
        {
            return BadRequest($"Invalid format: '{req.Format}'");
        }

        var opts = new EncodingOptions
        {
            PureBarcode = req.TextSize == 0,
            NoPadding = true,
            Width = req.Width,
            Height = req.Height,
            Margin = req.Margin
        };
        
        
        var code = writer.encode(req.Content, realFormat, req.Width, req.Height, opts.Hints);
        var renderer = new SKBitmapRenderer
        {
            TextSize = req.TextSize,
            TextFont = fontService.GetJetbrainsFont()
        };
        

        using var image = renderer.Render(code, realFormat, req.Content, opts);
        using var composed = LabelCanvas.Compose(SKImage.FromBitmap(image), req.Width, req.Height, req.Stretch?ScaleMode.Stretch:ScaleMode.Shrink, background: SKColors.White);
        using var data = composed.Encode(SKEncodedImageFormat.Png, 100);
        
        return File(data.AsStream(), "image/png");
    }
}


public record BarcodeRequest
{
    public string Content { get; init; } = default!;
    public string Format { get; init; } = "CODE_128";
    public int Width  { get; init; } = 200;
    public int Height { get; init; } = 200;
    public int TextSize { get; init; } = 25;
    
    public int Margin { get; init; } = 0;
    public bool Stretch { get; set; } = false;
}
