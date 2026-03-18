using QuizTest.Contracts;

namespace QuizTest.Services;

public sealed class QuizGame(IQuizApiClient apiClient, IQuizUi ui, IAnswerShuffler answerShuffler)
{
    private readonly IQuizApiClient _apiClient = apiClient;
    private readonly IQuizUi _ui = ui;
    private readonly IAnswerShuffler _answerShuffler = answerShuffler;

    /// <summary>
    /// Runs an interactive quiz game by fetching categories and questions from the API, 
    /// prompting the user to answer questions, and displaying final results.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method orchestrates the entire quiz flow:
    /// 1. Displays a welcome screen
    /// 2. Prompts the user to select difficulty and question count
    /// 3. Fetches available quiz categories from the API
    /// 4. Prompts the user to select a category
    /// 5. Fetches the selected quiz questions from the API
    /// 6. Displays each question and collects the user's answer
    /// 7. Provides immediate feedback on answer correctness
    /// 8. Shows final results including score and percentage
    /// </remarks>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _ui.ShowWelcome();

        var difficulty = _ui.PromptDifficulty();
        var questionCount = _ui.PromptQuestionCount();

        var categories = await _ui.FetchCategoriesAsync(
            () => _apiClient.GetCategoriesAsync(cancellationToken));

        var selectedCategory = _ui.PromptCategory(categories);

        var questions = await _ui.FetchQuestionsAsync(
            questionCount,
            () => _apiClient.GetQuestionsAsync(questionCount, difficulty, selectedCategory?.Id, cancellationToken));

        var correctAnswerCount = 0;

        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var answers = _answerShuffler.ShuffleAnswers(question);
            var decodedAnswers = answers.Select(a => System.Net.WebUtility.HtmlDecode(a) ?? string.Empty).ToArray();
            var decodedCorrectAnswer = System.Net.WebUtility.HtmlDecode(question.CorrectAnswer) ?? string.Empty;

            var selectedAnswer = _ui.PromptAnswer(
                System.Net.WebUtility.HtmlDecode(question.Category),
                System.Net.WebUtility.HtmlDecode(question.Question),
                decodedAnswers,
                i + 1,
                questions.Count);

            var isCorrect = string.Equals(selectedAnswer, decodedCorrectAnswer, StringComparison.Ordinal);
            if (isCorrect)
                correctAnswerCount++;
 
            _ui.ShowAnswerResult(isCorrect, decodedCorrectAnswer);

            if (i < questions.Count - 1)
                _ui.ShowInterQuestionProgress(i + 1, questions.Count);
        }

        _ui.ShowFinalResults(correctAnswerCount, questions.Count);
    }
}
