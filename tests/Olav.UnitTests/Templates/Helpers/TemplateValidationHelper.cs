using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace Olav.UnitTests.Templates.Helpers;

public static class TemplateValidationHelper
{
    public static bool IsToolAvailable(string tool)
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = tool,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(psi);
            if (process is null) return false;

            process.WaitForExit(3000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static void ValidateDockerCompose(string content)
    {
        if (!IsToolAvailable("docker")) return;

        // Create a docker/ subdir with an empty parent .env so env_file: ../.env resolves
        string root = CreateTempDir();
        string dockerDir = Path.Combine(root, "docker");
        Directory.CreateDirectory(dockerDir);
        File.WriteAllText(Path.Combine(root, ".env"), string.Empty);
        string path = Path.Combine(dockerDir, "docker-compose.yml");
        File.WriteAllText(path, content);

        Run("docker", $"compose -f {path} config", dockerDir);
    }

    public static void ValidateJson(string content)
    {
        JsonDocument.Parse(content);
    }

    public static void ValidateYaml(string content)
    {
        YamlStream yaml = [];
        yaml.Load(new StringReader(content));
    }

    public static void ValidateMsBuild(string content)
    {
        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit
        };

        using XmlReader reader = XmlReader.Create(new StringReader(content), settings);
        while (reader.Read()) { }
    }

    public static void ValidateShell(string content)
    {
        if (!IsToolAvailable("bash")) return;

        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sh");
        File.WriteAllText(path, content);

        Run("bash", $"-n {path}", ".");
    }

    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void Run(string file, string args, string workingDir)
    {
        ProcessStartInfo psi = new()
        {
            FileName = file,
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardError = true
        };

        using Process process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start process: {file}");
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(process.StandardError.ReadToEnd());
        }
    }
}
