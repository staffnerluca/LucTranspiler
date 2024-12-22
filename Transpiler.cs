using System.Linq.Expressions;
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

    private List<string> datatypes = new List<string>(){
        "int", "bool", "string", "double"
    };

    private List<char> lineEnders = new List<char>(){
        '\n', '{', '}', ';'
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

    public char GetClosestEndOfLineToken(int pos, List<string> tokens)
    {
        for(int i = 0; i <= pos; i++)
        {
            foreach(char lineEnder in lineEnders)
            {
                if(tokens[pos-i][0].Equals(lineEnder))
                {
                    return lineEnder;
                }
            }
        }
        return ' ';
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

    public string GetDatatypeOfToken(string token)
    {
        if(token[0].Equals('"') || token[0].Equals("'"))
        {
            return "string";
        }
        else if(int.TryParse(token, out int result))
        {
            return "int";
        }
        else if(bool.TryParse(token, out bool res))
        {
            return "bool";
        }
        else if(double.TryParse(token, out double resu))
        {
            return "double";
        }
        return "object";
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

    // sLUC format vor lists:
    // list := ["element", "element"];
    // or
    // list[int] = [10, 20];
    public string TranslateListCreation(List<string> tokens)
    {
        string listString = "List<";
        string listName = "";
        bool simplified = false;
        int posOfEq = 0;
        for(int i = 0; i < tokens.Count; i++)
        {
            if(tokens[i].Equals("["))
            {
                posOfEq = i;
                simplified = true;
            }
            else if(tokens[i].Equals("=") || tokens[i].Equals(":="))
            {
                posOfEq = i;
                break;
            }
        }
        string datatype = "";
        if(!simplified)
        {
            // for performance reaons it is not recommended to just use a list of objects, so it is necessary to evaluate the datatype here
            datatype = GetDatatypeOfToken(tokens[posOfEq+2]);
            listName = tokens[posOfEq-1];
        }
        else
        {
            listName = tokens[0];
            datatype = tokens[2];
        }
        listString += datatype + "> " + listName + "= new List<" + datatype +">(){";
        List<string> elements = new List<string>{};
        for(int i = tokens.Count - 3; i > 0; i--)
        {
            if(tokens[i].Equals("["))
            {
                break;
            }
            elements.Insert(0, tokens[i]);
        }
        foreach(string element in elements)
        {
            listString += element + ", ";
        }
        // remove last ","
        listString = listString.Substring(0, listString.Count() - 2);
        listString += "};";
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
        bool simpleTryCatch = false;
        string insertAfterNextEndOfLine = "";
        for(int i = 0; i < functionTokens.Count; i++)
        {
            string currentTok = functionTokens[i];
            string token = TranslateToken(currentTok);
            if(token.Equals("__complex__"))
            {
                if(currentTok.Equals("?"))
                {
                    bool singleLineErrorHandling = true;
                    if(singleLineErrorHandling)
                    {
                        List<int> eolTokPos = new List<int>(){};
                        int indexLastEolT = -1;
                        // because the catch is after the end of the line
                        string funcWithoutLastLineEnder = func.Substring(0, func.Length - 1);
                        foreach(char lineEnder in lineEnders)
                        {
                            eolTokPos.Add(funcWithoutLastLineEnder.LastIndexOf(lineEnder));
                        }
                        indexLastEolT = eolTokPos.Max();
                        if(indexLastEolT == -1)
                        {
                            indexLastEolT = 0;
                        }
                        func = func.Insert(indexLastEolT + 1, "try{");
                        func += "}catch(Exception ex){";
                        simpleTryCatch = true;
                    }
                    //TODO: handle mutli line try catch
                }
                else if(currentTok.Equals("[") && !datatypes.Contains(functionTokens[i+1]))
                {
                    int start = 0;
                    int end = 0;
                    (start, end) = GetStartAndEndOfLine(i, functionTokens);
                    List<string> line = functionTokens.GetRange(start+1, end);
                    char eol = GetClosestEndOfLineToken(i, functionTokens);
                    int eolIndex = func.LastIndexOf(eol);
                    func = func.Substring(0, eolIndex+1);

                    func += TranslateListCreation(line);
                    for(int j = i-3; j < functionTokens.Count(); j++)
                    {
                        if(!lineEnders.Contains(functionTokens[j][0]))
                        {
                            functionTokens.RemoveAt(j);
                        }
                        else
                        {
                            functionTokens.RemoveAt(j);
                            break;
                        }
                    }
                }
                else if(functionTokens[i].Equals(":="))
                {
                    List<string> tokensForVar = functionTokens.GetRange(i-1, i+1);
                    func = func.Substring(0, func.LastIndexOf(GetClosestEndOfLineToken(i, functionTokens))+1);
                    func += TranslateVarDefinition(tokensForVar);
                    functionTokens.RemoveRange(i-1, i+1);
                    // The number of tokens was reduced by a minimum of two and so I needs to decrease by 1
                    //TODO: Account for more complex variable creations
                    i-=2;
                }/*
                else if(currentTok.Equals("="))
                {
                    List<string> currentLine = GetLineOfToken(i, functionTokens);
                    func += TranslateVarDefinition(currentLine);
                }*/
            }
            else if(lineEnders.Contains(token[0]))
            {
                func += insertAfterNextEndOfLine;
                if(simpleTryCatch)
                {
                    insertAfterNextEndOfLine += "}";
                }
                func += token;
            }
            else
            {
                func += token;
            }
        }

        return func;
    }

    // input: line? errorHandling
    /*
    {
        line
        line
    }
    ?
    {
        error handling
        error handling
    }
    */
    //      text, how many tokens to delete before and how many tokens to delete after the ? token
    /*public (string, List<string>) TranslateTryCatch(List<string> tokens, int posOfQuestionmark)
    {
        string tryCatch = "try{";
        if(tokens[0].Equals("{"))
        {

        }
        else
        {
            for(int i = 1; i < tokens.Count(); i++)
            {
                // Translate the line
            }
        }

        return tryCatch;
    }*/

    public List<string> GetLineOfToken(int index, List<string> tokens)
    {
        List<string> currentLine = new List<string>();
        int start = -1;
        int end = -1;
        (start, end) = GetStartAndEndOfLine(index, tokens);

        currentLine = tokens.Skip(start).Take(end - start + 1).ToList();

        return currentLine;
    }

    public (int, int) GetStartAndEndOfLine(int index, List<string> tokens)
    {
        int start = -1;
        int end = -1;

        int indexRight = index;
        int indexLeft = index;
        while((start == -1 || end == -1) && indexLeft > 0 && indexRight < tokens.Count()-2)
        {
            if(lineEnders.Contains(tokens[indexRight][0]))
            {
                end = indexRight;
            }
            if(lineEnders.Contains(tokens[indexLeft][0]))
            {
                start = indexLeft + 1;
            }
            indexRight += 1;
            indexLeft -= 1;
        }
        if(start == -1)
        {
            start = 0;
        }
        if(end == -1)
        {
            // -2 to exclude the line ending token
            end = tokens.Count()-2;
        }
        return(start, end);
    }

    public string TranslateToken(string tok)
    {
        Dictionary<string, string> LucToCSharpToken = new Dictionary<string, string>{
            {"string", "string "},
            {"int", "int "},
            {"bool", "bool "},
            {"none", "void "},
            {"+", "+"},
            {"-", "-"},
            {"/", "/"},
            {"*", "*"},
            {";", ";"},
            {"r", "return "},
            {"=", "="}
        };

        List<string> complexKeywords = new List<string>{
            "if", "?", "while", ":=", "["//"="
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


    public string TranslateForHead(List<string> forHead)
    {
        string output = "";
        bool isSimpleForeach = true;
        if(isSimpleForeach)
        {
            output += "foreach(var __lucIntern__ in ";
            int listNamePos = 1;
            if(forHead[1].Equals("("))
            {
                listNamePos += 1;
            }
            output += forHead[listNamePos];
            output += ")";
        }

        return output;
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
            if(!token.Equals(":="))
            {
                translation += TranslateToken(token);
            }
            else
            {
                translation += "=";
            }
        }
        return translation; 
    }


    public string TrnslateLine(List<string> tokens)
    {
        string outp = "";
        // only provisional
        foreach(string token in tokens)
        {
            outp += TranslateToken(token);
        }
        return "";
    }


    // function to that calles the rest of the translation functions
    public string Translate()
    {
        return "";
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