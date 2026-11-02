using BluePrint.Compiler.CodeGen;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text;
using System.Xml;
using Task = Microsoft.Build.Utilities.Task;

namespace BluePrint.Compiler.Tasks;

public sealed class CompileBluePrint : Task
{
    const string BluePrintExtension = ".bp";
    const string UIXmlExtension = ".ui";

    [Required]
    public ITaskItem[] Inputs { get; set; } = [];

    [Required]
    public string? OutputPath { get; set; }

    [Output]
    public ITaskItem[]? GeneratedFiles { get; private set; }

    public override bool Execute()
    {
        var inputs = Inputs ?? throw new InvalidOperationException($"{nameof(Inputs)} is required");
        var outputPath = OutputPath ?? throw new InvalidOperationException($"{nameof(OutputPath)} is required");

        if (!Directory.Exists(outputPath))
        {
            Log.LogError("Output path '{0}' does not exist", outputPath);
            return false;
        }

        bool success = true;
        List<ITaskItem> generatedFiles = [];

        foreach (var input in inputs)
        {
            try
            {
                var source = File.ReadAllText(input.ItemSpec);
                var document = Parser.Parser.Parse(source);

                var settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    Indent = false
                };

                var fullOutputPath = CreatePathFor(input, outputPath, out var logicalName);
                using (var xmlWriter = XmlWriter.Create(fullOutputPath, settings))
                using (XmlCodeGenerator generator = new(xmlWriter))
                {
                    generator.Visit(document);
                }

                generatedFiles.Add(CreateOutputItem(fullOutputPath, input, logicalName));
            }
            catch (Exception exception)
            {
                Log.LogErrorFromException(exception, showStackTrace: true, showDetail: true, file: input.ItemSpec);
                success = false;
            }
        }

        GeneratedFiles = [.. generatedFiles];
        return success;
    }

    private string CreatePathFor(ITaskItem item, string outputPath, out string logicalName)
    {
        string path = item.ItemSpec;

        var sourceDir = Directory.GetCurrentDirectory();
        path = Path.GetRelativePath(sourceDir, Path.GetFullPath(path, sourceDir));
        logicalName = Path.ChangeExtension(path, UIXmlExtension)
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        path = Path.GetFullPath(path, Path.GetFullPath(outputPath, sourceDir));
        path = Path.ChangeExtension(path, UIXmlExtension);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return path;
    }

    private static TaskItem CreateOutputItem(string path, ITaskItem? sourceItem, string? logicalName)
    {
        TaskItem item = new(path);
        if (sourceItem != null)
        {
            bool shouldOverrideMetadata = true;
            foreach (string metadataName in (ReadOnlySpan<string>)["Link", "LogicalName"])
            {
                var value = sourceItem.GetMetadata(metadataName);
                item.SetMetadata(metadataName, value);

                if (!string.IsNullOrEmpty(value))
                    shouldOverrideMetadata = false;
            }

            if (shouldOverrideMetadata)
                item.SetMetadata("LogicalName", logicalName);
        }
        return item;
    }
}
