using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.DailyQuestionService;

namespace Intertwine.UnitTests.Services.DailyQuestionService;

public class DailyQuestionServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 15);

    [Fact]
    public async Task FillsEightDatesAndPreservesExistingAssignmentsOnRerun()
    {
        var repository = new MemoryRepository(new(1, [1], null), new(2, [2], null));
        repository.Assignments.Add(new DailyQuestion { Date = Today.AddDays(3), QuestionId = 2 });
        var cache = new Mock<IDailyQuestionCache>();
        var service = new Service(repository, cache.Object, TimeProvider.System);

        var first = await service.EnsureDailyQuestionsAsync(Today);
        var second = await service.EnsureDailyQuestionsAsync(Today);

        Assert.Equal(new EnsureDailyQuestionsResult(Today, Today.AddDays(7), 7, 1), first);
        Assert.Equal(new EnsureDailyQuestionsResult(Today, Today.AddDays(7), 0, 8), second);
        Assert.Equal(8, repository.Assignments.Count);
        Assert.Equal(2, repository.Assignments.Single(x => x.Date == Today.AddDays(3)).QuestionId);
        Assert.All(repository.Assignments, x => Assert.InRange(x.Date, Today, Today.AddDays(7)));
        cache.Verify(x => x.RemoveAsync(Today.AddDays(3), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PrefersLeastUsedCategoryAndSkipsItOnceExhausted()
    {
        var repository = new MemoryRepository(
            new(10, [1], null), new(20, [2], null), new(30, [3], null));
        repository.BaseUsage = new Dictionary<int, int> { [1] = 5, [2] = 2, [3] = 1 };
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 2);

        Assert.Equal(new[] { 30, 20, 10 }, repository.Assignments.Select(x => x.QuestionId));
    }

    [Fact]
    public async Task CountsEveryCategoryAndRebalancesAfterEachInsertion()
    {
        var repository = new MemoryRepository(
            new(1, [1, 2], null), new(2, [2], null), new(3, [3], null));
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 2);

        Assert.Equal(new[] { 1, 3, 2 }, repository.Assignments.Select(x => x.QuestionId));
    }

    [Fact]
    public async Task BreaksTiesByCategoryThenQuestionId()
    {
        var repository = new MemoryRepository(
            new(1, [20], null), new(8, [10], null), new(4, [10], null));
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 0);

        Assert.Equal(4, Assert.Single(repository.Assignments).QuestionId);
    }

    [Fact]
    public async Task ReusesLeastRecentlySelectedRatherThanOldestHistoricalRow()
    {
        var repository = new MemoryRepository(new(1, [1], null), new(2, [2], null));
        repository.Assignments.AddRange([
            new DailyQuestion { Date = Today.AddDays(-100), QuestionId = 1 },
            new DailyQuestion { Date = Today.AddDays(-1), QuestionId = 1 },
            new DailyQuestion { Date = Today.AddDays(-30), QuestionId = 2 }
        ]);
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 1);

        Assert.Equal(new[] { 2, 1 }, repository.Assignments.Where(x => x.Date >= Today).Select(x => x.QuestionId));
    }

    [Fact]
    public async Task FutureAssignmentsCountAsUsedAndAreNeverOverwritten()
    {
        var repository = new MemoryRepository(new(1, [1], null), new(2, [2], null));
        repository.Assignments.Add(new DailyQuestion { Date = Today.AddDays(7), QuestionId = 1 });
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 0);

        Assert.Equal(2, repository.Assignments.Single(x => x.Date == Today).QuestionId);
    }

    [Fact]
    public async Task UsesUncategorizedUnusedQuestionBeforeReusingAnyQuestion()
    {
        var repository = new MemoryRepository(new(1, [1], Today.AddDays(-1)), new(2, [], null));
        await CreateService(repository).EnsureDailyQuestionsAsync(Today, 0);
        Assert.Equal(2, Assert.Single(repository.Assignments).QuestionId);
    }

    [Fact]
    public async Task RecreatesDeletedDateAndEvictsItsCachedAssignment()
    {
        var repository = new MemoryRepository(new(1, [1], null), new(2, [2], null));
        var cache = new Mock<IDailyQuestionCache>();
        var service = new Service(repository, cache.Object, TimeProvider.System);
        await service.EnsureDailyQuestionsAsync(Today);
        repository.Assignments.RemoveAll(x => x.Date == Today.AddDays(4));

        var result = await service.EnsureDailyQuestionsAsync(Today);

        Assert.Equal(1, result.Created);
        Assert.Equal(7, result.AlreadyExisted);
        cache.Verify(x => x.RemoveAsync(Today.AddDays(4), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CompetingInsertIsCountedAsExisting()
    {
        var repository = new Mock<IDailyQuestionRepository>();
        repository.Setup(x => x.GetSelectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DailyQuestionSelection([new(1, [1], null)], new Dictionary<int, int>()));
        repository.Setup(x => x.TryInsertAsync(It.IsAny<DailyQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var service = new Service(repository.Object, Mock.Of<IDailyQuestionCache>(), TimeProvider.System);

        var result = await service.EnsureDailyQuestionsAsync(Today, 0);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.AlreadyExisted);
    }

    [Fact]
    public async Task RetryAfterCacheFailurePreservesCommittedAssignmentAndRetriesEviction()
    {
        var repository = new MemoryRepository(new DailyQuestionCandidate(1, [1], null));
        var cache = new Mock<IDailyQuestionCache>();
        cache.SetupSequence(x => x.RemoveAsync(Today, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated Redis outage"))
            .Returns(Task.CompletedTask);
        var service = new Service(repository, cache.Object, TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EnsureDailyQuestionsAsync(Today, 0));

        var result = await service.EnsureDailyQuestionsAsync(Today, 0);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.AlreadyExisted);
        Assert.Single(repository.Assignments);
        cache.Verify(x => x.RemoveAsync(Today, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task NoActiveQuestionsFailsWithoutInserting()
    {
        var repository = new MemoryRepository();
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).EnsureDailyQuestionsAsync(Today));
        Assert.Empty(repository.Assignments);
    }

    [Fact]
    public async Task CancellationAndInvalidRangeDoNotWrite()
    {
        var repository = new MemoryRepository(new DailyQuestionCandidate(1, [1], null));
        var service = CreateService(repository);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.EnsureDailyQuestionsAsync(Today, cancellationToken: new CancellationToken(true)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.EnsureDailyQuestionsAsync(Today, -1));
        Assert.Empty(repository.Assignments);
    }

    private static Service CreateService(MemoryRepository repository) =>
        new(repository, Mock.Of<IDailyQuestionCache>(), TimeProvider.System);

    private sealed class MemoryRepository(params DailyQuestionCandidate[] candidates) : IDailyQuestionRepository
    {
        public List<DailyQuestion> Assignments { get; } = [];
        public Dictionary<int, int> BaseUsage { get; set; } = [];

        public Task<bool> ExistsAsync(DateOnly date, CancellationToken cancellationToken = default) =>
            Task.FromResult(Assignments.Any(x => x.Date == date));

        public Task<DailyQuestionSelection> GetSelectionAsync(CancellationToken cancellationToken = default)
        {
            var usage = new Dictionary<int, int>(BaseUsage);
            foreach (var assignment in Assignments)
                foreach (var id in candidates.Single(q => q.QuestionId == assignment.QuestionId).CategoryIds.Distinct())
                    usage[id] = usage.GetValueOrDefault(id) + 1;

            var current = candidates.Select(q => q with
            {
                LastSelectedDate = Assignments.Where(d => d.QuestionId == q.QuestionId)
                    .Select(d => (DateOnly?)d.Date).Append(q.LastSelectedDate).Max()
            }).ToList();
            return Task.FromResult(new DailyQuestionSelection(current, usage));
        }

        public Task<bool> TryInsertAsync(DailyQuestion assignment, CancellationToken cancellationToken = default)
        {
            if (Assignments.Any(x => x.Date == assignment.Date))
                return Task.FromResult(false);
            Assignments.Add(assignment);
            return Task.FromResult(true);
        }
    }
}
