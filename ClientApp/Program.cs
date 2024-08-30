using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

var htmlResponse = await new HttpClient()
    .GetAsync("https://skerdiberberi.com/blog/async-request-reply-pattern-pt3");

var largeText = await htmlResponse.Content.ReadAsStringAsync();

byte[] originalData = Encoding.UTF8.GetBytes(largeText);

Console.WriteLine($"Original data length: {originalData.Length} bytes");

var compressedData = await CompressAsync(largeText);

Console.WriteLine($"Compressed data length: {compressedData.Length} bytes");
Console.WriteLine($"Compression ratio: {(float)compressedData.Length / originalData.Length:P2}");

await SendCompressedDataAsync(compressedData);

Console.WriteLine("Data sent successfully!");
Console.ReadKey();

static async Task<byte[]> CompressAsync(string text, CancellationToken ct = default)
{
    byte[] buffer = Encoding.UTF8.GetBytes(text);
    using (var memoryStream = new MemoryStream())
    {
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            await gZipStream.WriteAsync(buffer, 0, buffer.Length, ct);
        }
        return memoryStream.ToArray();
    }
}

static async Task SendCompressedDataAsync(byte[] compressedData, CancellationToken ct = default)
{
    try
    {
        using var client = new HttpClient();
        var content = new ByteArrayContent(compressedData);
        content.Headers.ContentEncoding.Add("gzip");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await client.PostAsync("https://localhost:7185/api/demo", content, ct);
        response.EnsureSuccessStatusCode();
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine("Error during HTTP request: " + ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("General error: " + ex.Message);
    }
}
