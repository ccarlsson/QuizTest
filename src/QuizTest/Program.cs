using QuizTest.DependencyInjection;
using QuizTest.Application.Contracts;
using QuizTest.Application.Services;
using QuizTest.Ui.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddQuizCore();
services.AddSingleton<IQuizUi, SpectreQuizUi>();

using var serviceProvider = services.BuildServiceProvider();
var quizGame = serviceProvider.GetRequiredService<QuizGame>();
await quizGame.RunAsync();