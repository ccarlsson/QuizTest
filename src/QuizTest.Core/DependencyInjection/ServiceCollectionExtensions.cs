using Microsoft.Extensions.DependencyInjection;
using QuizTest.Application.Contracts;
using QuizTest.Application.Services;

namespace QuizTest.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuizCore(
        this IServiceCollection services,
        string apiBaseUrl = "https://opentdb.com",
        TimeSpan? apiTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IQuizApiClient>(_ =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = apiTimeout ?? TimeSpan.FromSeconds(30)
            };

            return new QuizApiClient(httpClient);
        });

        services.AddSingleton<IAnswerShuffler, RandomAnswerShuffler>();
        services.AddTransient<QuizGame>();

        return services;
    }
}
