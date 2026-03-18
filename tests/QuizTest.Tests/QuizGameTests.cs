using Moq;
using QuizTest.Contracts;
using QuizTest.Domain;
using QuizTest.Services;

namespace QuizTest.Tests;

public class QuizGameTests
{
    [Fact]
    public async Task RunAsync_TracksScoreAndShowsProgressBetweenQuestions()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "General Knowledge") };
        var question1 = CreateQuestion("Q1", "A1", ["B1", "C1", "D1"]);
        var question2 = CreateQuestion("Q2", "A2", ["B2", "C2", "D2"]);
        var questions = new List<QuizQuestion> { question1, question2 };

        uiMock.Setup(x => x.PromptDifficulty()).Returns("easy");
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(2);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.FetchCategoriesAsync(
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.FetchQuestionsAsync(
                It.IsAny<int>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((int _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(2, "easy", 9, It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        shufflerMock.Setup(x => x.ShuffleAnswers(question1)).Returns(["A1", "B1", "C1", "D1"]);
        shufflerMock.Setup(x => x.ShuffleAnswers(question2)).Returns(["A2", "B2", "C2", "D2"]);

        var answerQueue = new Queue<string>(["A1", "B2"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(() => answerQueue.Dequeue());

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        uiMock.Verify(x => x.ShowWelcome(), Times.Once);
        uiMock.Verify(x => x.ShowAnswerResult(true, "A1"), Times.Once);
        uiMock.Verify(x => x.ShowAnswerResult(false, "A2"), Times.Once);
        uiMock.Verify(x => x.ShowInterQuestionProgress(1, 2), Times.Once);
        uiMock.Verify(x => x.ShowFinalResults(1, 2), Times.Once);
    }

    [Fact]
    public async Task RunAsync_UsesSelectedSettingsWhenFetchingQuestions()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(17, "Science") };
        var question = CreateQuestion("Q1", "A1", ["B1", "C1", "D1"]);

        uiMock.Setup(x => x.PromptDifficulty()).Returns("hard");
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(15);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.FetchCategoriesAsync(
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.FetchQuestionsAsync(
                It.IsAny<int>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((int _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(15, "hard", 17, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(["A1", "B1", "C1", "D1"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns("A1");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        apiClientMock.Verify(x => x.GetQuestionsAsync(15, "hard", 17, It.IsAny<CancellationToken>()), Times.Once);
        uiMock.Verify(x => x.ShowFinalResults(1, 1), Times.Once);
    }

    [Fact]
    public async Task RunAsync_UsesNullCategoryId_WhenAllCategoriesIsSelected()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "General Knowledge") };
        var question = CreateQuestion("Q1", "A1", ["B1", "C1", "D1"]);

        uiMock.Setup(x => x.PromptDifficulty()).Returns("medium");
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(10);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns((QuizCategory?)null);

        uiMock.Setup(x => x.FetchCategoriesAsync(
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.FetchQuestionsAsync(
                It.IsAny<int>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((int _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(10, "medium", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(["A1", "B1", "C1", "D1"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns("A1");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        apiClientMock.Verify(x => x.GetQuestionsAsync(10, "medium", null, It.IsAny<CancellationToken>()), Times.Once);
        uiMock.Verify(x => x.ShowFinalResults(1, 1), Times.Once);
    }

    private static QuizQuestion CreateQuestion(string questionText, string correctAnswer, List<string> incorrectAnswers)
    {
        return new QuizQuestion(
            Type: "multiple",
            Difficulty: "easy",
            Category: "General",
            Question: questionText,
            CorrectAnswer: correctAnswer,
            IncorrectAnswers: incorrectAnswers);
    }
}
