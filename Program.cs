using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {   
        Console.WriteLine("Running the program");
        
        if (args.Length > 0)
        {
            string projectName = args[0];
            string output = "";
            string parentDirectory = Path.Combine("../../", projectName);
            try
            {
                WriteCodeToProject(projectName, parentDirectory);
            }
            catch
            {
                Console.WriteLine("The project does not exist. Creating new project...");
                output = ExecuteProcess("dotnet", "new console -o " + parentDirectory);
                WriteCodeToProject(projectName, parentDirectory);
            }

            // Execute the generated project
            output = ExecuteProcess("dotnet", "run --project " + parentDirectory);
            Console.WriteLine("Output from dotnet run:");
            Console.WriteLine(output);
        }
        else
        {
            Console.WriteLine("Error: No name of the project given.");
        }
    }

    public static void WriteCodeToProject(string projectName, string parentDirectory)
    {   
        string json = File.ReadAllText("lucTest.json");
        List<string> data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)
            .Values
            .SelectMany(list => list).ToList();
        Transpiler trans = new Transpiler();

        string project = trans.Translate(data);
        string programPath = Path.Combine(parentDirectory, "Program.cs");
        File.WriteAllText(programPath, project);
    }

    public static string ExecuteProcess(string program, string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = program;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrEmpty(output))
        {
            return output;
        }
        else if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Error: " + error);
        }
        return "error";
    }
}
