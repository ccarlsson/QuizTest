using QuizTest.Contracts;
using QuizTest.Domain;
using Spectre.Console;

namespace QuizTest.Services;

public sealed class SpectreQuizUi : IQuizUi
{
    /// <summary>
    /// Displays a welcome screen with the quiz title and rules.
    /// </summary>
    public void ShowWelcome()
    {
        AnsiConsole.Write(
            new FigletText("Quiz Time")
                .Centered()
                .Color(Color.CadetBlue));

        AnsiConsole.Write(
            new Panel("[grey]Select the correct answer from the list for each question.[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey)
                .Header("[bold]Rules[/]"));
    }

    /// <summary>
    /// Prompts the user to select a difficulty level.
    /// </summary>
    /// <returns>The selected difficulty as a string: "easy", "medium", or "hard".</returns>
    public string PromptDifficulty()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select difficulty:[/]")
                .PageSize(3)
                .AddChoices(["easy", "medium", "hard"]));
    }

    /// <summary>
    /// Prompts the user to select the number of questions for the quiz.
    /// </summary>
    /// <returns>The selected number of questions: 5, 10, 15, or 20.</returns>
    public int PromptQuestionCount()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("[bold]How many questions would you like?[/]")
                .PageSize(4)
                .AddChoices([5, 10, 15, 20]));
    }

    /// <summary>
    /// Prompts the user to select a quiz category from the available options.
    /// </summary>
    /// <param name="categories">The list of available quiz categories.</param>
    /// <returns>The selected category, or null if "All categories" was chosen.</returns>
    public QuizCategory? PromptCategory(List<QuizCategory> categories)
    {
        var options = new List<string> { "All categories" };
        options.AddRange(categories.Select(c => System.Net.WebUtility.HtmlDecode(c.Name)));

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select category:[/]")
                .PageSize(12)
                .AddChoices(options));

        if (selected == "All categories")
            return null;

        return categories.First(c => System.Net.WebUtility.HtmlDecode(c.Name) == selected);
    }

    /// <summary>
    /// Fetches quiz categories while displaying a dots loading spinner.
    /// </summary>
    /// <param name="action">The asynchronous action that retrieves the categories.</param>
    /// <returns>A task that returns the list of available quiz categories.</returns>
    public Task<List<QuizCategory>> FetchCategoriesAsync(Func<Task<List<QuizCategory>>> action)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cadetblue"))
            .StartAsync("[grey]Fetching categories...[/]", _ => action());
    }

    /// <summary>
    /// Fetches quiz questions while displaying a star loading spinner.
    /// </summary>
    /// <param name="questionCount">The number of questions being fetched, used in the loading message.</param>
    /// <param name="action">The asynchronous action that retrieves the questions.</param>
    /// <returns>A task that returns the list of quiz questions.</returns>
    public Task<List<QuizQuestion>> FetchQuestionsAsync(int questionCount, Func<Task<List<QuizQuestion>>> action)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("deepskyblue1"))
            .StartAsync($"[grey]Fetching {questionCount} questions from API...[/]", _ => action());
    }

    /// <summary>
    /// Displays a quiz question and prompts the user to select an answer from the provided options.
    /// </summary>
    /// <param name="category">The category of the quiz question.</param>
    /// <param name="question">The quiz question text.</param>
    /// <param name="answers">The list of answer options.</param>
    /// <param name="questionNumber">The current question number in the quiz.</param>
    /// <param name="totalQuestions">The total number of questions in the quiz.</param>
    /// <returns>The user's selected answer.</returns>
    public string PromptAnswer(string category, string question, IReadOnlyList<string> answers, int questionNumber, int totalQuestions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Rule($"[yellow]Question {questionNumber}/{totalQuestions}[/]")
                .RuleStyle("grey")
                .Centered());

        var questionPanel = new Panel($"[bold]{Markup.Escape(question)}[/]")
            .Header($"[deepskyblue1]{Markup.Escape(category)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1);

        AnsiConsole.Write(questionPanel);

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select your answer:[/]")
                .PageSize(4)
                .AddChoices(answers));
    }

    /// <summary>
    /// Displays feedback on the user's answer, indicating whether it was correct or incorrect.
    /// </summary>
    /// <param name="isCorrect">Whether the user's answer was correct.</param>
    /// <param name="correctAnswer">The correct answer to display if the user was incorrect.</param>
    public void ShowAnswerResult(bool isCorrect, string correctAnswer)
    {
        if (isCorrect)
        {
            AnsiConsole.Write(new Panel("[bold green]Correct![/]").BorderColor(Color.Green));
            return;
        }

        AnsiConsole.Write(
            new Panel($"[bold red]Wrong![/]\nCorrect answer: [green]{Markup.Escape(correctAnswer)}[/]")
                .BorderColor(Color.Red));
    }

    /// <summary>
    /// Displays a progress bar showing the quiz progress between questions.
    /// </summary>
    /// <param name="answeredCount">The number of questions answered so far.</param>
    /// <param name="totalCount">The total number of questions in the quiz.</param>
    public void ShowInterQuestionProgress(int answeredCount, int totalCount)
    {
        AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn()
            ])
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[grey]Progress {answeredCount}/{totalCount} - loading next question[/]", maxValue: 100);
                while (!ctx.IsFinished)
                {
                    task.Increment(20);
                    Thread.Sleep(45);
                }
            });
    }

    /// <summary>
    /// Displays the final quiz results including the number of correct answers and the score percentage.
    /// </summary>
    /// <param name="correctAnswerCount">The number of questions answered correctly.</param>
    /// <param name="totalQuestions">The total number of questions in the quiz.</param>
    public void ShowFinalResults(int correctAnswerCount, int totalQuestions)
    {
        AnsiConsole.WriteLine();

        var scorePercent = (double)correctAnswerCount / totalQuestions * 100;
        var scoreColor = scorePercent >= 70 ? "green" : scorePercent >= 40 ? "yellow" : "red";

        var resultsTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Summary[/]")
            .AddColumn("[bold]Value[/]");

        resultsTable.AddRow("Correct answers", $"[bold]{correctAnswerCount}[/] of [bold]{totalQuestions}[/]");
        resultsTable.AddRow("Score", $"[{scoreColor}]{scorePercent:0}%[/]");
        resultsTable.AddRow("Performance", $"[{scoreColor}]{(scorePercent >= 70 ? "Excellent" : scorePercent >= 40 ? "Good" : "Needs Improvement")}[/]");

        AnsiConsole.Write(
            new Panel(resultsTable)
                .Header("[bold]Results[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.CadetBlue));
    }
}
