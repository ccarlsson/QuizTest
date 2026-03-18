using QuizTest.Domain;

namespace QuizTest.Contracts;

/// <summary>
/// Defines the contract for accessing quiz data from a remote API.
/// </summary>
public interface IQuizApiClient
{
    /// <summary>
    /// Retrieves quiz questions from the API with optional filtering.
    /// </summary>
    /// <param name="amount">The number of questions to retrieve.</param>
    /// <param name="difficulty">The difficulty level of the questions.</param>
    /// <param name="categoryId">The optional category ID for filtering.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that returns a list of quiz questions.</returns>
    Task<List<QuizQuestion>> GetQuestionsAsync(
        int amount = 10,
        string difficulty = "medium",
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of available quiz categories from the API.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that returns a list of available quiz categories.</returns>
    Task<List<QuizCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
