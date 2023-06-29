﻿using System.Net.Http.Json;

namespace ChampionsOfKhazad.Bot.OpenAi.Embeddings;

public class EmbeddingsService
{
    private readonly HttpClient _httpClient;

    public EmbeddingsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Embedding>> CreateEmbeddingsAsync(IList<TextEntry> input)
    {
        var request = new CreateEmbeddingsRequest(input.Select(x => x.Text));
        var response = await _httpClient.PostAsJsonAsync(request);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<CreateEmbeddingsResponse>();

        if (responseContent is null)
            throw new ApplicationException("Response content was null.");

        return responseContent.Data.Select(x =>
        {
            var entry = input[x.Index];
            return new Embedding(entry.Id, entry.Text, x.Embedding);
        });
    }

    private record EmbeddingResponse(float[] Embedding, int Index);

    private record CreateEmbeddingsResponse(EmbeddingResponse[] Data);

    private record CreateEmbeddingsRequest(
        IEnumerable<string> Input,
        string Model = Constants.Model
    );
}
