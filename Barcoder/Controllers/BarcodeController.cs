using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using ZXing;
using ZXing.Datamatrix;
using ZXing.Rendering;
using ZXing.SkiaSharp.Rendering;

namespace Barcoder.Controllers;

[ApiController]
[Route("generate")]
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
    
    [HttpGet]
    public IActionResult Generate([FromQuery]string content, [FromQuery]string format = "CODE_128", [FromQuery]int width = 200, [FromQuery]int height = 200, [FromQuery]int textSize = 25)
    {
        BarcodeFormat realFormat;
        try
        {
            realFormat = GetBarcodeFormat(format);
        }
        catch (InvalidEnumArgumentException)
        {
            return BadRequest($"Invalid format: '{format}'");
        }


        var code = writer.encode(content, realFormat, width, height);
        var renderer = new SKBitmapRenderer();
        if (textSize != 0)
        {
            renderer.TextSize = textSize;
            renderer.TextFont = fontService.GetJetbrainsFont();
        }
        else
        {
            renderer.TextFont = null;
        }
        
        var image = renderer.Render(code, realFormat, textSize == 0 ? "":content);
        var mStream = new MemoryStream();
        image.Encode(mStream, SKEncodedImageFormat.Png, 100);
        mStream.Position = 0;
        
        return File(mStream, "image/png");
    }
}