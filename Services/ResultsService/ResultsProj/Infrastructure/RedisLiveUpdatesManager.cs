
using StackExchange.Redis;
using System.Text.Json;
using Application.Interfaces;
using DAL.Entities;

public class RedisLiveUpdatesManager : ILiveUpdatesManager
{
    private readonly IDatabase _redis;

    public RedisLiveUpdatesManager(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task RegisterSubscriberAsync(Guid questionId, string userToken)
    {
        await _redis.SetAddAsync($"subs:{questionId}", userToken);
    }

    public async Task RemoveSubscriberAsync(Guid questionId, string userToken)
    {
        await _redis.SetRemoveAsync($"subs:{questionId}", userToken);
    }

    public async Task AddVoteUpdateAsync(Guid questionId, QuestionResult updatePayload)
    {
        // store/override latest payload
        var payload = JsonSerializer.Serialize(updatePayload);
        await _redis.StringSetAsync($"update:payload:{questionId}", payload);

        // check if subscribers exist
        var hasSubscribers = await _redis.SetLengthAsync($"subs:{questionId}") > 0;
        if (!hasSubscribers) return;

        // ensure uniqueness
        var added = await _redis.SetAddAsync("updates:inqueue", questionId.ToString());
        if (added)
        {
            await _redis.ListRightPushAsync("updates:queue", questionId.ToString());
        }
    }

    public async Task<IEnumerable<(string userToken, QuestionResult Update)>> PollUpdatesAsync()
    {
        var results = new List<(string, QuestionResult)>();

        var qid = await _redis.ListLeftPopAsync("updates:queue");
        if (qid.IsNullOrEmpty) return results;

        // allow requeue later
        await _redis.SetRemoveAsync("updates:inqueue", qid.ToString());

        // get payload
        var payloadJson = await _redis.StringGetAsync($"update:payload:{qid}");
        if (payloadJson.IsNullOrEmpty) return results;

        var payload = JsonSerializer.Deserialize<QuestionResult>(payloadJson);

        // get subscribers
        var subs = await _redis.SetMembersAsync($"subs:{qid}");
        foreach (var sub in subs)
        {
            results.Add((sub.ToString(), payload));
        }

        // clear payload (optional)
        await _redis.KeyDeleteAsync($"update:payload:{qid}");

        return results;
    }
}
