using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizTest.Domain.Quiz;
using QuizTest.Infrastructure;

namespace QuizTest.Infrastructure.Tests;

public class QuizApiClientTests
{
    [Fact]
    public async Task GetQuestionsAsync_MapsResponseToQuizQuestions()
    {
        var json = """
        {
            "response_code": 0,
            "results": [
                {
                    "type": "multiple",
                    "difficulty": "easy",
                    "category": "General Knowledge",
                    "question": "What color is the sky?",
                    "correct_answer": "Blue",
                    "incorrect_answers": ["Red", "Green", "Yellow"]
                }
            ]
        }
        """;

        using var client = CreateClientWithResponse(json);
        var sut = new QuizApiClient(client);

        var result = await sut.GetQuestionsAsync(amount: 1, difficulty: Difficulty.Easy, categoryId: 9);

        var question = Assert.Single(result);
        Assert.Equal("multiple", question.Type);
        Assert.Equal(Difficulty.Easy, question.Difficulty);
        Assert.Equal("General Knowledge", question.Category);
        Assert.Equal("What color is the sky?", question.Question);
        Assert.Equal("Blue", question.CorrectAnswer);
        Assert.Equal(["Red", "Green", "Yellow"], question.IncorrectAnswers);
    }

    [Fact]
    public async Task GetQuestionsAsync_BuildsUrlWithCategoryWhenProvided()
    {
        var json = """{ "response_code": 0, "results": [] }""";

        string? capturedUrl = null;
        using var client = CreateClientWithResponse(json, request =>
        {
            capturedUrl = request.RequestUri?.PathAndQuery;
        });

        var sut = new QuizApiClient(client);

        await sut.GetQuestionsAsync(amount: 5, difficulty: Difficulty.Hard, categoryId: 17);

        Assert.Contains("amount=5", capturedUrl);
        Assert.Contains("difficulty=hard", capturedUrl);
        Assert.Contains("category=17", capturedUrl);
    }

    [Fact]
    public async Task GetQuestionsAsync_OmitsCategoryWhenNull()
    {
        var json = """{ "response_code": 0, "results": [] }""";

        string? capturedUrl = null;
        using var client = CreateClientWithResponse(json, request =>
        {
            capturedUrl = request.RequestUri?.PathAndQuery;
        });

        var sut = new QuizApiClient(client);

        await sut.GetQuestionsAsync(amount: 10, difficulty: Difficulty.Medium, categoryId: null);

        Assert.DoesNotContain("category", capturedUrl);
    }

    [Fact]
    public async Task GetQuestionsAsync_ThrowsOnNonZeroResponseCode()
    {
        var json = """{ "response_code": 4, "results": [] }""";

        using var client = CreateClientWithResponse(json);
        var sut = new QuizApiClient(client);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetQuestionsAsync());

        Assert.Contains("4", ex.Message);
    }

    [Fact]
    public async Task GetQuestionsAsync_ThrowsWhenDeserializationReturnsNull()
    {
        using var client = CreateClientWithResponse("null");
        var sut = new QuizApiClient(client);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetQuestionsAsync());
    }

    [Fact]
    public async Task GetQuestionsAsync_ThrowsOnHttpError()
    {
        using var client = CreateClientWithResponse("", statusCode: HttpStatusCode.InternalServerError);
        var sut = new QuizApiClient(client);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => sut.GetQuestionsAsync());
    }

    [Fact]
    public async Task GetCategoriesAsync_MapsCategoryResponse()
    {
        var json = """
        {
            "trivia_categories": [
                { "id": 9, "name": "General Knowledge" },
                { "id": 17, "name": "Science & Nature" }
            ]
        }
        """;

        using var client = CreateClientWithResponse(json);
        var sut = new QuizApiClient(client);

        var result = await sut.GetCategoriesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(9, result[0].Id);
        Assert.Equal("General Knowledge", result[0].Name);
        Assert.Equal(17, result[1].Id);
        Assert.Equal("Science & Nature", result[1].Name);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsEmptyListOnNullDeserialization()
    {
        using var client = CreateClientWithResponse("null");
        var sut = new QuizApiClient(client);

        var result = await sut.GetCategoriesAsync();

        Assert.Empty(result);
    }

    private static HttpClient CreateClientWithResponse(
        string json,
        Action<HttpRequestMessage>? onRequest = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(json, statusCode, onRequest);
        return new HttpClient(handler) { BaseAddress = new Uri("https://test.local/") };
    }

    private sealed class FakeHttpMessageHandler(
        string responseJson,
        HttpStatusCode statusCode,
        Action<HttpRequestMessage>? onRequest) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest?.Invoke(request);

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
