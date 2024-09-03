using System.IO.Compression;

public sealed class DecompressionMiddleware
{
    private readonly RequestDelegate _next;
    public DecompressionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.ContentEncoding == "gzip")
        {
            context.Request.EnableBuffering();

            using var decompressedStream = new MemoryStream();
            using var gzipStream = new GZipStream(context.Request.Body, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(decompressedStream);

            decompressedStream.Seek(0, SeekOrigin.Begin);

            // Replace the request body with the decompressed stream
            context.Request.Body = new MemoryStream(decompressedStream.ToArray());
        }
        else if (context.Request.Headers.ContentEncoding == "br")
        {
            context.Request.EnableBuffering();

            using var decompressedStream = new MemoryStream();
            using var gzipStream = new BrotliStream(context.Request.Body, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(decompressedStream);

            decompressedStream.Seek(0, SeekOrigin.Begin);

            // Replace the request body with the decompressed stream
            context.Request.Body = new MemoryStream(decompressedStream.ToArray());
        }

        await _next(context);
    }
}