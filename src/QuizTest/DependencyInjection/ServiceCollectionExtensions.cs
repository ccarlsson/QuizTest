using Microsoft.Extensions.DependencyInjection;
using QuizTest.Application.Contracts;
using QuizTest.Application.Services;
using QuizTest.Infrastructure;

namespace QuizTest.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuizCore(
        this IServiceCollection services,
        string apiBaseUrl = "https://opentdb.com",
        TimeSpan? apiTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IQuizApiClient, QuizApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = apiTimeout ?? TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IAnswerShuffler, RandomAnswerShuffler>();
        services.AddTransient<QuizGame>();

        return services;
    }
}
