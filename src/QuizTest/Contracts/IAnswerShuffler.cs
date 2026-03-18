using QuizTest.Domain;

namespace QuizTest.Contracts;

/// <summary>
/// Defines the contract for shuffling answer options for quiz questions.
/// </summary>
public interface IAnswerShuffler
{
    /// <summary>
    /// Shuffles the correct and incorrect answers for a quiz question.
    /// </summary>
    /// <param name="question">The quiz question with answers to shuffle.</param>
    /// <returns>A read-only list of shuffled answers.</returns>
    IReadOnlyList<string> ShuffleAnswers(QuizQuestion question);
}
