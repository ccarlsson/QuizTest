using QuizTest.Application.Services;
using QuizTest.Domain.Quiz;

namespace QuizTest.Tests;

public class RandomAnswerShufflerTests
{
    [Fact]
    public void ShuffleAnswers_ReturnsAllAnswersExactlyOnce()
    {
        var incorrectAnswers = new List<string> { "Wrong 1", "Wrong 2", "Wrong 3" };
        var question = CreateQuestion("Correct", incorrectAnswers);
        var sut = new RandomAnswerShuffler();

        var shuffledAnswers = sut.ShuffleAnswers(question);

        Assert.Equal(4, shuffledAnswers.Count);
        Assert.Equal(
            incorrectAnswers.Append("Correct").OrderBy(answer => answer),
            shuffledAnswers.OrderBy(answer => answer));
    }

    [Fact]
    public void ShuffleAnswers_DoesNotMutateSourceIncorrectAnswers()
    {
        var incorrectAnswers = new List<string> { "Wrong 1", "Wrong 2", "Wrong 3" };
        var originalAnswers = incorrectAnswers.ToList();
        var question = CreateQuestion("Correct", incorrectAnswers);
        var sut = new RandomAnswerShuffler();

        var shuffledAnswers = sut.ShuffleAnswers(question);

        Assert.Equal(originalAnswers, incorrectAnswers);
        Assert.NotSame(incorrectAnswers, shuffledAnswers);
    }

    private static QuizQuestion CreateQuestion(string correctAnswer, IReadOnlyList<string> incorrectAnswers)
    {
        return new QuizQuestion(
            Type: "multiple",
            Difficulty: Difficulty.Easy,
            Category: "General",
            Question: "Question?",
            CorrectAnswer: correctAnswer,
            IncorrectAnswers: incorrectAnswers);
    }
}