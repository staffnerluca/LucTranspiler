using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text.Json;

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

    public string Transpile()
    {
        return "";
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
        
        return tokens;
    }

    public Dictionary<int, List<string>> ReadSimplifiedCodeToDic()
    {
        string simplCode = File.ReadAllText("simplifiedCode.json");
        var code = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(simplCode);
        return code;
    }

    public int FindMainFunction(Dictionary<int, List<string>> tokens)
    {
        foreach(int key in tokens.Keys)
        {
            if(tokens[key][0].Equals("main_func"))
            {
                return key;
            }
        }
        return -1;
    }

    public int GetDistanceToClosingBrackets(List<string> tokens, string bracket)
    {
        Dictionary<string, string> bracketMapping = new Dictionary<string, string>{
            {")", "("},
            {"}", "{"},
            {"]", "["}
        };
        List<string> bracketsList = new List<string>{bracket};
        int distance = 1;
        foreach(string tok in tokens)
        {
            try
            {
                if(bracketMapping.Keys.Contains(tok))
                {
                    if(bracketMapping[tok].Equals(bracketsList.Last()))
                    {
                        if(bracketsList.Count == 0)
                        {
                            return distance;
                        }
                    }
                }
                else if(bracketMapping.Values.Contains(tok))
                {
                    bracketsList.Add(tok);
                }
                distance += 1;
            }
            catch(Exception ex)
            {
                distance += 1;
                continue;
            }
        }
        return -1;
    }


    public string TranslateTryCatch(List<string> tokens)
    {
        string tryString = "";
        return tryString;
    }

    public string TranslateListCreation(List<string> tokens)
    {
        string listString = "";
        return listString;
    }

    // function format in sLUC: function [return_type] name([parameters])
    public string TranslateFunction(List<string> functionTokens)
    {
        string func = "public ";
        func += TranslateToken(functionTokens[1]); // return value
        func += functionTokens[2]; // adds name of the function
        func += functionTokens[3];
        bool endOfHead = false;
        int currentPos = 4;
        while(!endOfHead && currentPos < functionTokens.Count)
        {
            if(functionTokens[currentPos].Equals(")"))
            {
                func += ")";
                endOfHead = true;
                currentPos++;
                break;
            }
            func += TranslateToken(functionTokens[currentPos]);
            currentPos += 1;
        }
        functionTokens.RemoveRange(0, currentPos);

        for(int i = 0; i < functionTokens.Count; i++)
        {
            string currentTok = functionTokens[i];
            string token = TranslateToken(currentTok);
            if(token.Equals("__complex__"))
            {
                if(currentTok.Equals("?"))
                {
                    TranslateTryCatch(functionTokens);
                }
                else if(currentTok.Equals("["))
                {
                    TranslateListCreation(functionTokens);
                }
            }
            func += token;
        }

        return func;
    }

    public string TranslateToken(string tok)
    {
        Dictionary<string, string> LucToCSharpToken = new Dictionary<string, string>{
            {"string", "string "},
            {"int", "int "},
            {"bool", "bool "},
            {"none", "void "},
            {":=", "="},
            {"+", "+"},
            {"-", "-"},
            {"/", "/"},
            {"*", "*"},
            {";", ";"},

        };

        List<string> complexKeywords = new List<string>{
            "if", "?", "while"
        };
        if(LucToCSharpToken.Keys.Contains(tok))
        {
            return LucToCSharpToken[tok];
        }
        else if(complexKeywords.Contains(tok))
        {
            return "__complex__";
        }
        else
        {
            return tok;
        }
    }

    public string TranslateVarDefinition(List<string> tokens)
    {
        /*
            The var keyword can be used because datatypes in C# are assigned during compilation and not during the run time.
            So it doesn't impact the performance negatively and it doesn't matter if the C# compiler does the translation or it is done in this code.
        */
        string translation = "var ";
        foreach(string token in tokens)
        {
            translation += TranslateToken(token);
        }
        return translation; 
    }

    public string GenerateCSharpCode()
    {
        Dictionary<int, List<string>> simplCode = ReadSimplifiedCodeToDic();
        
        string mainCode = "";
        string functionCode = "";

        int mainFunc = FindMainFunction(simplCode);

        // Get Tokens for every function

        // translate the function
        return @"
            using System;

            class Program
            {
                static void bla(string[] args)
                {
                    Console.WriteLine(""Hello from the generated thing!"");
                }
            }";
    }
} 
#endregion