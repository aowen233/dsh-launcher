using System.Globalization;

namespace DshLauncher.Core;

public sealed class SemanticVersion : IComparable<SemanticVersion>
{
    private SemanticVersion(int major, int minor, int patch, string[] prerelease)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public IReadOnlyList<string> Prerelease { get; }

    public static bool TryParse(string? text, out SemanticVersion? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalized = text.Trim().Trim('"');
        if (normalized.StartsWith('v'))
        {
            normalized = normalized[1..];
        }

        int buildIndex = normalized.IndexOf('+');
        if (buildIndex >= 0)
        {
            normalized = normalized[..buildIndex];
        }

        string[] releaseAndPre = normalized.Split('-', 2);
        string[] release = releaseAndPre[0].Split('.');
        if (release.Length != 3 ||
            !int.TryParse(release[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major) ||
            !int.TryParse(release[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor) ||
            !int.TryParse(release[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch) ||
            major < 0 || minor < 0 || patch < 0)
        {
            return false;
        }

        string[] prerelease = releaseAndPre.Length == 2
            ? releaseAndPre[1].Split('.', StringSplitOptions.RemoveEmptyEntries)
            : [];

        if (releaseAndPre.Length == 2 && prerelease.Length == 0)
        {
            return false;
        }

        value = new SemanticVersion(major, minor, patch, prerelease);
        return true;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        int releaseComparison = Major.CompareTo(other.Major);
        if (releaseComparison == 0) releaseComparison = Minor.CompareTo(other.Minor);
        if (releaseComparison == 0) releaseComparison = Patch.CompareTo(other.Patch);
        if (releaseComparison != 0) return releaseComparison;

        if (Prerelease.Count == 0 && other.Prerelease.Count == 0) return 0;
        if (Prerelease.Count == 0) return 1;
        if (other.Prerelease.Count == 0) return -1;

        int count = Math.Max(Prerelease.Count, other.Prerelease.Count);
        for (int index = 0; index < count; index++)
        {
            if (index >= Prerelease.Count) return -1;
            if (index >= other.Prerelease.Count) return 1;

            string left = Prerelease[index];
            string right = other.Prerelease[index];
            bool leftNumeric = int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out int leftNumber);
            bool rightNumeric = int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out int rightNumber);
            int partComparison;
            if (leftNumeric && rightNumeric)
            {
                partComparison = leftNumber.CompareTo(rightNumber);
            }
            else if (leftNumeric)
            {
                partComparison = -1;
            }
            else if (rightNumeric)
            {
                partComparison = 1;
            }
            else
            {
                partComparison = string.Compare(left, right, StringComparison.Ordinal);
            }

            if (partComparison != 0) return partComparison;
        }

        return 0;
    }

    public override string ToString()
    {
        string version = $"{Major}.{Minor}.{Patch}";
        return Prerelease.Count == 0 ? version : $"{version}-{string.Join('.', Prerelease)}";
    }
}
