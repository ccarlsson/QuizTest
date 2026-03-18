using System.Text.Json.Serialization;

namespace QuizTest.Integrations.OpenTrivia;

public record OpenTriviaQuizResponse(
    [property: JsonPropertyName("response_code")] int ResponseCode,
    [property: JsonPropertyName("results")] List<OpenTriviaQuestionDto> Results
);

public record OpenTriviaCategoriesResponse(
    [property: JsonPropertyName("trivia_categories")] List<OpenTriviaCategoryDto> TriviaCategories
);

public record OpenTriviaCategoryDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

public record OpenTriviaQuestionDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("difficulty")] string Difficulty,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("correct_answer")] string CorrectAnswer,
    [property: JsonPropertyName("incorrect_answers")] List<string> IncorrectAnswers
);
