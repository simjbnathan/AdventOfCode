using System;
using System.Collections.Generic;
using System.IO;

public interface IExtensionAnalyzer
{
    Dictionary<string, int> AnalyzeExtensions(string directory);
}

public interface IDateTimeProvider
{
    DateTime GetCurrentDateTime();
}

public interface IActivityLogger
{
    void LogActivity(string message);
    string ReadActivityLog();
}

class FileOrganizer
{
    private readonly IExtensionAnalyzer extensionAnalyzer;
    private readonly IActivityLogger logger;

    public FileOrganizer(IExtensionAnalyzer extensionAnalyzer, IActivityLogger logger)
    {
        this.extensionAnalyzer = extensionAnalyzer;
        this.logger = logger;
    }

    // code goes here
    public void OrganizedFiles(string sourceDirectory, string destinationDirectory)
    {
        string[] files = Directory.GetFiles(sourceDirectory);

        Dictionary<string, int> extensionCount = extensionAnalyzer.AnalyzeExtensions(sourceDirectory);

        foreach (var item in extensionCount)
        {
            logger.LogActivity($"'{item.Key}': {item.Value}");
        }

        MoveFiles(files, destinationDirectory, logger);

        //print output of activity_log
        string logContent = logger.ReadActivityLog();
        Console.WriteLine($"Content of activitylog.txt:\n{logContent}");

    }

    private void MoveFiles(string[] files, string targetDirectory, IActivityLogger logger)
    {
        //create directory if target not exist
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            string targetPath = Path.Combine(targetDirectory, fileName);

            //check if target file exists
            while (File.Exists(targetPath))
            {
                //generate unique filenames
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.{timestamp}{Path.GetExtension(fileName)}";
            }

            File.Move(filePath, targetPath);

            logger.LogActivity($"Moved: {fileName} to {targetPath}");

        }
    }
}

class FileAnalyzer : IExtensionAnalyzer
{
    // code goes here
    public Dictionary<string, int> AnalyzeExtensions(string directory)
    {
        Dictionary<string, int> extensionCount = new Dictionary<string, int>();

        string[] files = Directory.GetFiles(directory);
        if (files.Length == 0)
        {
            Console.WriteLine("No files found in the specified directory");
            return extensionCount;
        }
        else
        {
            foreach (string filePath in files)
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (extensionCount.ContainsKey(fileExtension))
                {
                    extensionCount[fileExtension]++;
                }
                else
                {
                    extensionCount[fileExtension] = 1;
                }

            }
        }
        return extensionCount;

    }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentDateTime()
    {
        return DateTime.Now;
    }
}

class Logger : IActivityLogger
{
    // code goes here

    public const string ActivityLogFileName = "activity_log.txt";

    public void LogActivity(string message)
    {
        string logFilePath = ActivityLogFileName;

        using (StreamWriter sw = File.AppendText(logFilePath))
        {
            sw.WriteLine($"{GetCurrentDateTime()} : {message}");
        }
    }

    public string ReadActivityLog()
    {
        string logFilePath = ActivityLogFileName;
        try
        {
            string logContent = File.ReadAllText(logFilePath);
            return logContent;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading txtfile: {ex.Message}");
            return null;
        }
    }

    private DateTime GetCurrentDateTime()
    {
        return new DateTimeProvider().GetCurrentDateTime();
    }

}

class MainClass
{
    static void Main()
    {
        // code goes here
        Console.WriteLine("Hello world");
        string source = "SourceFolder";
        string target = "DestinationFolder";

        IExtensionAnalyzer extensionAnalyzer = new FileAnalyzer();
        IActivityLogger logger = new Logger();

        FileOrganizer fileOrganizer = new FileOrganizer(extensionAnalyzer, logger);
        fileOrganizer.OrganizedFiles(source, target);
        Console.WriteLine(Logger.ActivityLogFileName);

    }
}