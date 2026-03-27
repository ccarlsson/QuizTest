using Moq;
using QuizTest.Application.Contracts;
using QuizTest.Application.Services;
using QuizTest.Domain;

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

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Easy);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(2);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>())) 
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>())) 
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(2, Difficulty.Easy, 9, It.IsAny<CancellationToken>())).ReturnsAsync(questions);

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

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Hard);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(15);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>())) 
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>())) 
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(15, Difficulty.Hard, 17, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(["A1", "B1", "C1", "D1"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns("A1");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        apiClientMock.Verify(x => x.GetQuestionsAsync(15, Difficulty.Hard, 17, It.IsAny<CancellationToken>()), Times.Once);
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

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Medium);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(10);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns((QuizCategory?)null);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>())) 
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>())) 
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(10, Difficulty.Medium, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(["A1", "B1", "C1", "D1"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns("A1");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        apiClientMock.Verify(x => x.GetQuestionsAsync(10, Difficulty.Medium, null, It.IsAny<CancellationToken>()), Times.Once);
        uiMock.Verify(x => x.ShowFinalResults(1, 1), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WrapsApiFetchesInWithStatusAsync()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "General Knowledge") };
        var question = CreateQuestion("Q1", "A1", ["B1", "C1", "D1"]);

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Easy);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(1);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(1, Difficulty.Easy, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(["A1", "B1", "C1", "D1"]);
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns("A1");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        uiMock.Verify(x => x.WithStatusAsync(
            "Fetching categories...",
            It.IsAny<Func<Task<List<QuizCategory>>>>()), Times.Once);
        uiMock.Verify(x => x.WithStatusAsync(
            It.Is<string>(s => s.Contains("questions")),
            It.IsAny<Func<Task<List<QuizQuestion>>>>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_HtmlDecodesQuestionTextCategoryAndAnswers()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "Science &amp; Nature") };
        var question = new QuizQuestion(
            Type: "multiple",
            Difficulty: Difficulty.Easy,
            Category: "Science &amp; Nature",
            Question: "What is 2 &gt; 1?",
            CorrectAnswer: "True &amp; correct",
            IncorrectAnswers: ["False &lt; wrong", "Maybe &amp; unsure", "None &quot;of&quot; above"]);

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Easy);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(1);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(1, Difficulty.Easy, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        shufflerMock.Setup(x => x.ShuffleAnswers(question))
            .Returns(["True &amp; correct", "False &lt; wrong", "Maybe &amp; unsure", "None &quot;of&quot; above"]);

        string? capturedCategory = null;
        string? capturedQuestion = null;
        IReadOnlyList<string>? capturedAnswers = null;

        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns((string cat, string q, IReadOnlyList<string> ans, int num, int total) =>
            {
                capturedCategory = cat;
                capturedQuestion = q;
                capturedAnswers = ans;
                return "True & correct";
            });

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        Assert.Equal("Science & Nature", capturedCategory);
        Assert.Equal("What is 2 > 1?", capturedQuestion);
        Assert.Equal(["True & correct", "False < wrong", "Maybe & unsure", "None \"of\" above"], capturedAnswers);
        uiMock.Verify(x => x.ShowAnswerResult(true, "True & correct"), Times.Once);
    }

    [Fact]
    public async Task RunAsync_PassesShuffledAnswersToPrompt()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "General Knowledge") };
        var question = CreateQuestion("Q1", "Correct", ["Wrong1", "Wrong2", "Wrong3"]);

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Easy);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(1);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(1, Difficulty.Easy, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync([question]);

        var shuffledOrder = new List<string> { "Wrong3", "Correct", "Wrong1", "Wrong2" };
        shufflerMock.Setup(x => x.ShuffleAnswers(question)).Returns(shuffledOrder);

        IReadOnlyList<string>? receivedAnswers = null;
        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), 1, 1))
            .Returns((string cat, string q, IReadOnlyList<string> ans, int num, int total) =>
            {
                receivedAnswers = ans;
                return "Correct";
            });

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        Assert.Equal(shuffledOrder, receivedAnswers);
        shufflerMock.Verify(x => x.ShuffleAnswers(question), Times.Once);
    }

    [Fact]
    public async Task RunAsync_AllAnswersWrong_ShowsZeroScore()
    {
        var apiClientMock = new Mock<IQuizApiClient>();
        var uiMock = new Mock<IQuizUi>();
        var shufflerMock = new Mock<IAnswerShuffler>();

        var categories = new List<QuizCategory> { new(9, "General Knowledge") };
        var question1 = CreateQuestion("Q1", "A1", ["B1", "C1", "D1"]);
        var question2 = CreateQuestion("Q2", "A2", ["B2", "C2", "D2"]);
        var questions = new List<QuizQuestion> { question1, question2 };

        uiMock.Setup(x => x.PromptDifficulty()).Returns(Difficulty.Easy);
        uiMock.Setup(x => x.PromptQuestionCount()).Returns(2);
        uiMock.Setup(x => x.PromptCategory(categories)).Returns(categories[0]);

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizCategory>>>>()))
            .Returns((string _, Func<Task<List<QuizCategory>>> action) => action());

        uiMock.Setup(x => x.WithStatusAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<QuizQuestion>>>>()))
            .Returns((string _, Func<Task<List<QuizQuestion>>> action) => action());

        apiClientMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        apiClientMock.Setup(x => x.GetQuestionsAsync(2, Difficulty.Easy, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        shufflerMock.Setup(x => x.ShuffleAnswers(question1)).Returns(["A1", "B1", "C1", "D1"]);
        shufflerMock.Setup(x => x.ShuffleAnswers(question2)).Returns(["A2", "B2", "C2", "D2"]);

        uiMock.Setup(x => x.PromptAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns("Wrong");

        var sut = new QuizGame(apiClientMock.Object, uiMock.Object, shufflerMock.Object);

        await sut.RunAsync();

        uiMock.Verify(x => x.ShowAnswerResult(false, It.IsAny<string>()), Times.Exactly(2));
        uiMock.Verify(x => x.ShowAnswerResult(true, It.IsAny<string>()), Times.Never);
        uiMock.Verify(x => x.ShowFinalResults(0, 2), Times.Once);
    }

    private static QuizQuestion CreateQuestion(string questionText, string correctAnswer, List<string> incorrectAnswers)
    {
        return new QuizQuestion(
            Type: "multiple",
            Difficulty: Difficulty.Easy,
            Category: "General",
            Question: questionText,
            CorrectAnswer: correctAnswer,
            IncorrectAnswers: incorrectAnswers);
    }
}
