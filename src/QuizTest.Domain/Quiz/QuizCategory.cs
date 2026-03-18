namespace QuizTest.Domain.Quiz;

/// <summary>
/// Represents a quiz category with its identifier and name.
/// </summary>
/// <param name="Id">The unique identifier of the quiz category.</param>
/// <param name="Name">The name of the quiz category (may contain HTML-encoded characters).</param>
public record QuizCategory(int Id, string Name);
