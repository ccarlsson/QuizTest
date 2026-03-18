using QuizTest.Application.Contracts;
using QuizTest.Domain;

namespace QuizTest.Application.Services;

public sealed class RandomAnswerShuffler : IAnswerShuffler
{
    /// <summary>
    /// Shuffles the quiz question's correct answer and incorrect answers using the Fisher-Yates algorithm.
    /// </summary>
    /// <param name="question">The quiz question containing the correct and incorrect answers.</param>
    /// <returns>A read-only list of shuffled answers.</returns>
    /// <remarks>
    /// This method combines the correct answer with incorrect answers and randomizes their order 
    /// using the Fisher-Yates shuffle algorithm to provide uniform randomization.
    /// </remarks>
    public IReadOnlyList<string> ShuffleAnswers(QuizQuestion question)
    {
        var answers = question.IncorrectAnswers
            .Append(question.CorrectAnswer)
            .ToArray();

        for (var i = answers.Length - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (answers[i], answers[j]) = (answers[j], answers[i]);
        }

        return answers;
    }
}
