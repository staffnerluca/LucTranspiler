public class Transpiler
{
    private string? _filePath;
    public string filePath
    {
        get { return _filePath; }
        set { _filePath = value; }
    }
    
    private List<string> sepearators = new List<string>(){
        " ", "=", "==", ">", "<", "+", "-"
        // need to adjust so that every seperator that is not " " is added to the tokens
    };

    public Transpiler(string filePath)
    {
        filePath = this.filePath;
    }

    #region Helper functions
    public List<string> GetLines()
    {
        List<string> lines = new List<string>(File.ReadAllLines(this.filePath));
        return lines;
    }

    public List<string> TokenizeLine(List<string> line)
    {
        List<string> tokens = new List<string>(){};

    }
    #endregion

    public string GenerateCSharpCode()
    {
        List<string> lines = GetLines();
        string csharpCode = "";
        foreach(string line in lines)
        {
            
        }
        return @"
            using System;

            class Program
            {
                static void Main(string[] args)
                {
                    Console.WriteLine(""Hello from the generated thing!"");
                }
            }";
    }
}