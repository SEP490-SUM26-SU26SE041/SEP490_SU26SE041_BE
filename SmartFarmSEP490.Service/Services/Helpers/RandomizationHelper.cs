using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Service.Services.Helpers;

public static class RandomizationHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// Fisher-Yates shuffle algorithm
    /// </summary>
    public static List<T> Shuffle<T>(List<T> list)
    {
        var result = new List<T>(list);
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }

    /// <summary>
    /// CRD - Completely Randomized Design: shuffle all beds freely
    /// </summary>
    public static List<Guid> RandomizeCRD(List<Guid> bedIds, int replicateCount)
    {
        var shuffled = Shuffle(bedIds);
        return shuffled.Take(replicateCount).ToList();
    }

    /// <summary>
    /// Generate default treatment names: Control + Treatment 1..N
    /// </summary>
    public static List<string> GenerateDefaultTreatments(int count)
    {
        var treatments = new List<string> { "Control" };
        for (int i = 1; i < count; i++)
        {
            treatments.Add($"Treatment {i}");
        }
        return treatments;
    }

    /// <summary>
    /// Create group names for factorial design
    /// </summary>
    public static List<string> GenerateFactorialGroupNames(Dictionary<string, List<string>> factors)
    {
        var factorNames = factors.Keys.ToList();
        var factorLevels = factorNames.Select(f => factors[f].ToList()).ToList();

        var combinations = new List<string>();

        void Generate(int index, string current)
        {
            if (index == factorLevels.Count)
            {
                combinations.Add(current);
                return;
            }

            foreach (var level in factorLevels[index])
            {
                var separator = string.IsNullOrEmpty(current) ? "" : " × ";
                Generate(index + 1, current + separator + level);
            }
        }

        Generate(0, "");
        return combinations;
    }

    /// <summary>
    /// Assign groups to beds with replicate tracking
    /// </summary>
    public static List<GroupBedAssignment> AssignGroupsToBeds(
        List<GroupInfo> groups,
        List<Guid> bedIds,
        int replicationCount)
    {
        var result = new List<GroupBedAssignment>();
        var shuffledBeds = Shuffle(bedIds);

        int bedIndex = 0;
        foreach (var group in groups)
        {
            for (int rep = 1; rep <= replicationCount && bedIndex < shuffledBeds.Count; rep++)
            {
                result.Add(new GroupBedAssignment
                {
                    BedId = shuffledBeds[bedIndex],
                    GroupId = group.Id,
                    GroupName = group.Name,
                    ReplicateIndex = rep
                });
                bedIndex++;
            }
        }

        return result;
    }
}

public class GroupBedAssignment
{
    public Guid BedId { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public int ReplicateIndex { get; set; }
}

public class GroupInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public GroupType GroupType { get; set; }
}
