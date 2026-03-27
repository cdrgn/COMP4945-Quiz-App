using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TriviaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MediaController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public MediaController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<object>> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".mp4", ".wav", ".webm" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Invalid file type");
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var mediaPath = Path.Combine(_environment.WebRootPath, "media", fileName);

        // Ensure directory exists
        Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "media"));

        // Save file
        using (var stream = new FileStream(mediaPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Determine media type
        string mediaType;
        if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
            mediaType = "image";
        else if (new[] { ".mp3", ".wav" }.Contains(extension))
            mediaType = "audio";
        else
            mediaType = "video";

        return Ok(new
        {
            url = $"/media/{fileName}",
            mediaType = mediaType,
            fileName = fileName
        });
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteMedia(string fileName)
    {
        var filePath = Path.Combine(_environment.WebRootPath, "media", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        System.IO.File.Delete(filePath);
        return NoContent();
    }
}