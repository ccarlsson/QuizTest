namespace QuizTest.Domain;

/// <summary>
/// Represents a single quiz question with its associated metadata and answers.
/// </summary>
/// <param name="Type">The question type (typically "multiple" for multiple choice).</param>
/// <param name="Difficulty">The difficulty level of the question: "easy", "medium", or "hard".</param>
/// <param name="Category">The category the question belongs to.</param>
/// <param name="Question">The quiz question text (may contain HTML-encoded characters).</param>
/// <param name="CorrectAnswer">The correct answer to the question (may contain HTML-encoded characters).</param>
/// <param name="IncorrectAnswers">A list of incorrect answer options (may contain HTML-encoded characters).</param>
public record QuizQuestion(
    string Type,
    string Difficulty,
    string Category,
    string Question,
    string CorrectAnswer,
    List<string> IncorrectAnswers
);
