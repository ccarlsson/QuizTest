using System.Net.Http.Json;
using QuizTest.Application.Contracts;
using QuizTest.Domain;
using QuizTest.Infrastructure.Integrations.OpenTrivia;

namespace QuizTest.Infrastructure;

public sealed class QuizApiClient(HttpClient httpClient) : IQuizApiClient
{
    /// <summary>
    /// Fetches quiz questions from the Open Trivia API with optional filters.
    /// </summary>
    /// <param name="amount">The number of questions to retrieve. Defaults to 10.</param>
    /// <param name="difficulty">The difficulty level: "easy", "medium", or "hard". Defaults to "medium".</param>
    /// <param name="categoryId">The optional category ID to filter questions. Defaults to null (all categories).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that returns a list of quiz questions.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API returns an error response.</exception>
    public async Task<List<QuizQuestion>> GetQuestionsAsync(
        int amount = 10,
        string difficulty = "medium",
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"api.php?amount={amount}&difficulty={difficulty}&type=multiple";
        if (categoryId.HasValue)
            url += $"&category={categoryId.Value}";

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenTriviaQuizResponse>(cancellationToken);

        if (result is null || result.ResponseCode != 0)
            throw new InvalidOperationException($"API returned error code: {result?.ResponseCode}");

        return [.. result.Results
            .Select(q => new QuizQuestion(
                q.Type,
                q.Difficulty,
                q.Category,
                q.Question,
                q.CorrectAnswer,
                q.IncorrectAnswers))];
    }

    /// <summary>
    /// Fetches the list of available quiz categories from the Open Trivia API.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that returns a list of available quiz categories.</returns>
    public async Task<List<QuizCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api_category.php", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenTriviaCategoriesResponse>(cancellationToken);
        if (result is null)
            return [];

        return [.. result.TriviaCategories.Select(c => new QuizCategory(c.Id, c.Name))];
    }

}
