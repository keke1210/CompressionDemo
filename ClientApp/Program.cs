using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

var htmlResponse = await new HttpClient()
    .GetAsync("https://skerdiberberi.com/blog/async-request-reply-pattern-pt3");

var largeText = await htmlResponse.Content.ReadAsStringAsync();

Console.WriteLine("Original data length: " + largeText.Length);

var compressedData = await CompressAsync(largeText);

Console.WriteLine("Compressed data length: " + compressedData.Length);

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

        var response = await client.PostAsync("https://localhost:5001/api/demo", content, ct);
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
