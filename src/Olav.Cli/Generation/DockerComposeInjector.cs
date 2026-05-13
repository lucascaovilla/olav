// <copyright file="DockerComposeInjector.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

/// <summary>
/// Injects a Docker Compose service block into an existing compose file in-place using
/// line-based string manipulation. All generated compose files use 2-space YAML indentation —
/// this class assumes that convention and will produce malformed output if the target file uses
/// a different indentation scheme.
/// </summary>
public static class DockerComposeInjector
{
    /// <summary>
    /// Injects <paramref name="serviceBlock"/> into the compose file at <paramref name="composePath"/>.
    /// The service is appended under the <c>services:</c> section (before <c>volumes:</c> if present).
    /// If <paramref name="dependsOnCondition"/> is provided, a <c>depends_on</c> entry is also added
    /// to the <c>web</c> service. Any top-level <c>volumes:</c> entries declared in the block are
    /// merged into the file's top-level <c>volumes:</c> section.
    /// This method is idempotent — it no-ops if the service is already present.
    /// </summary>
    /// <param name="composePath">Absolute path to the compose file to modify.</param>
    /// <param name="serviceName">Name of the service being injected (e.g. <c>postgres</c>).</param>
    /// <param name="serviceBlock">
    /// YAML block for the service, starting with the service name at column 0 or indented 2 spaces.
    /// May include a trailing <c>volumes:</c> sub-section.
    /// </param>
    /// <param name="dependsOnCondition">
    /// The healthcheck condition to add to the <c>web</c> service's <c>depends_on</c> block,
    /// or <c>null</c> to skip dependency wiring.
    /// </param>
    public static void InjectService(
        string composePath,
        string serviceName,
        string serviceBlock,
        string? dependsOnCondition)
    {
        if (!File.Exists(composePath))
        {
            throw new InvalidOperationException(
                $"✗ Cannot inject '{serviceName}': compose file not found at '{composePath}'.");
        }

        string content = File.ReadAllText(composePath);

        if (IsAlreadyPresent(content, serviceName))
        {
            return;
        }

        string normalizedBlock = NormalizeServiceBlock(serviceBlock);
        string? volumeEntries = ExtractVolumeEntries(normalizedBlock);
        string serviceOnlyBlock = StripVolumeSection(normalizedBlock);

        content = InsertServiceBlock(content, serviceOnlyBlock);

        if (volumeEntries != null)
        {
            content = MergeVolumes(content, volumeEntries);
        }

        if (dependsOnCondition != null)
        {
            content = AddDependsOn(content, serviceName, dependsOnCondition);
        }

        File.WriteAllText(composePath, content);
    }

    private static bool IsAlreadyPresent(string content, string serviceName)
    {
        return content.Contains("\n  " + serviceName + ":") ||
               content.StartsWith("  " + serviceName + ":", StringComparison.Ordinal);
    }

    private static string NormalizeServiceBlock(string block)
    {
        string[] lines = block.Split('\n');
        List<string> normalized = new List<string>();
        bool firstLineSeen = false;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                normalized.Add(string.Empty);
                continue;
            }

            int leadingSpaces = line.Length - line.TrimStart().Length;

            if (!firstLineSeen)
            {
                // First non-empty line is the service name.
                // Shift to 2-space only when the template uses 0-indent; leave alone if already indented.
                // All subsequent 0-indent lines are top-level YAML keys (e.g. volumes:) and must NOT be shifted
                // so that StripVolumeSection and ExtractVolumeEntries can locate them.
                normalized.Add(leadingSpaces == 0 ? "  " + line.TrimStart() : line);
                firstLineSeen = true;
            }
            else
            {
                normalized.Add(line);
            }
        }

        return string.Join("\n", normalized).TrimEnd();
    }

    private static string? ExtractVolumeEntries(string block)
    {
        int volumesIdx = FindTopLevelVolumesInBlock(block);
        if (volumesIdx < 0)
        {
            return null;
        }

        string[] lines = block.Split('\n');
        List<string> volumeLines = new List<string>();
        bool inVolumes = false;

        foreach (string line in lines)
        {
            if (line.TrimStart() == "volumes:")
            {
                inVolumes = true;
                continue;
            }

            if (inVolumes)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().Length == line.Length)
                {
                    // Hit a new top-level key that isn't volumes
                    break;
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    volumeLines.Add(line.Trim().TrimEnd(':'));
                }
            }
        }

        return volumeLines.Count > 0 ? string.Join("\n", volumeLines) : null;
    }

    private static int FindTopLevelVolumesInBlock(string block)
    {
        string[] lines = block.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            int leading = lines[i].Length - trimmed.Length;
            if (leading == 0 && trimmed == "volumes:")
            {
                return i;
            }
        }

        return -1;
    }

    private static string StripVolumeSection(string block)
    {
        string[] lines = block.Split('\n');
        List<string> result = new List<string>();
        bool inVolumes = false;

        foreach (string line in lines)
        {
            string trimmed = line.TrimStart();
            int leading = line.Length - trimmed.Length;

            if (leading == 0 && trimmed == "volumes:")
            {
                inVolumes = true;
                continue;
            }

            if (inVolumes && leading == 0 && !string.IsNullOrWhiteSpace(trimmed))
            {
                inVolumes = false;
            }

            if (!inVolumes)
            {
                result.Add(line);
            }
        }

        return string.Join("\n", result).TrimEnd();
    }

    private static string InsertServiceBlock(string content, string serviceBlock)
    {
        int volumesLineIdx = content.IndexOf("\nvolumes:", StringComparison.Ordinal);
        if (volumesLineIdx >= 0)
        {
            return content.Insert(volumesLineIdx, "\n\n" + serviceBlock);
        }

        return content.TrimEnd() + "\n\n" + serviceBlock + "\n";
    }

    private static string MergeVolumes(string content, string volumeEntries)
    {
        string[] newVolumes = volumeEntries.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int volumesSectionIdx = content.IndexOf("\nvolumes:", StringComparison.Ordinal);
        if (volumesSectionIdx < 0)
        {
            string toAppend = "\nvolumes:\n";
            foreach (string v in newVolumes)
            {
                toAppend += "  " + v.Trim() + ":\n";
            }

            return content.TrimEnd() + "\n" + toAppend;
        }

        string insertPoint = content.Substring(0, volumesSectionIdx + "\nvolumes:".Length);
        string remainder = content.Substring(volumesSectionIdx + "\nvolumes:".Length);
        string newEntries = string.Empty;
        foreach (string v in newVolumes)
        {
            string entry = "  " + v.Trim() + ":";
            if (!content.Contains(entry))
            {
                newEntries += "\n" + entry;
            }
        }

        return insertPoint + newEntries + remainder;
    }

    private static string AddDependsOn(string content, string serviceName, string condition)
    {
        string[] lines = content.Split('\n');
        int webServiceLine = FindWebServiceLine(lines);
        if (webServiceLine < 0)
        {
            return content;
        }

        int dependsOnLine = FindExistingDependsOn(lines, webServiceLine);
        if (dependsOnLine >= 0)
        {
            List<string> result = new List<string>(lines);
            result.Insert(dependsOnLine + 1, $"      {serviceName}:");
            result.Insert(dependsOnLine + 2, $"        condition: {condition}");
            return string.Join("\n", result);
        }

        int insertAfter = FindEndOfWebServiceBlock(lines, webServiceLine);
        List<string> modified = new List<string>(lines);
        modified.Insert(insertAfter + 1, $"    depends_on:");
        modified.Insert(insertAfter + 2, $"      {serviceName}:");
        modified.Insert(insertAfter + 3, $"        condition: {condition}");
        return string.Join("\n", modified);
    }

    private static int FindWebServiceLine(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "  web:")
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindExistingDependsOn(string[] lines, int webServiceLine)
    {
        for (int i = webServiceLine + 1; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            int leading = lines[i].Length - trimmed.Length;

            // Hit another top-level service or section
            if (leading <= 2 && !string.IsNullOrWhiteSpace(trimmed))
            {
                break;
            }

            if (leading == 4 && trimmed == "depends_on:")
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindEndOfWebServiceBlock(string[] lines, int webServiceLine)
    {
        int last = webServiceLine;
        for (int i = webServiceLine + 1; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            int leading = lines[i].Length - trimmed.Length;

            // Next top-level service or empty section boundary
            if (!string.IsNullOrWhiteSpace(trimmed) && leading <= 2)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                last = i;
            }
        }

        return last;
    }
}
