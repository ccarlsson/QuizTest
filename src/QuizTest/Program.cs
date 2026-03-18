using QuizTest;
using QuizTest.Contracts;
using QuizTest.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IQuizApiClient>(_ =>
{
	var httpClient = new HttpClient
	{
		BaseAddress = new Uri("https://opentdb.com"),
		Timeout = TimeSpan.FromSeconds(30)
	};

	return new QuizApiClient(httpClient);
});
services.AddSingleton<IQuizUi, SpectreQuizUi>();
services.AddSingleton<IAnswerShuffler, RandomAnswerShuffler>();
services.AddTransient<QuizGame>();

using var serviceProvider = services.BuildServiceProvider();
var quizGame = serviceProvider.GetRequiredService<QuizGame>();
await quizGame.RunAsync();