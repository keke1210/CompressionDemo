using System.IO.Compression;

public sealed class GzipDecompressionMiddleware
{
    private readonly RequestDelegate _next;
    public GzipDecompressionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers["Content-Encoding"] == "gzip")
        {
            context.Request.EnableBuffering();

            using var decompressedStream = new MemoryStream();
            using var gzipStream = new GZipStream(context.Request.Body, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(decompressedStream);

            decompressedStream.Seek(0, SeekOrigin.Begin);

            // Replace the request body with the decompressed stream
            context.Request.Body = new MemoryStream(decompressedStream.ToArray());

            // Call the next middleware in the pipeline
            await _next(context);
        }
        else
        {
            // If not gzip, proceed without modification
            await _next(context);
        }
    }
}