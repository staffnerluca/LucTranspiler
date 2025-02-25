using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.Json;
using Xunit.Sdk;

public class Transpiler
{
    # region Lists
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

    private List<string> comparisonOperators = new List<string>(){
        "<", ">", ">=", "<=", "=="
    };
    #endregion

    public Transpiler(){}

    public string imports = "using System;";

    /* 
        needed to determine if .Equal or "==" should be used for a comparison
        in LUC == is the only comparison operator
    */
    public List<string> stringVariables = new List<string>() { };

    public string Transpile()
    {
        return "";
    }

    #region Helper functions
    public char GetClosestEndOfLineTokenBefore(int pos, List<string> tokens)
    {
        for (int i = 0; i <= pos; i++)
        {
            foreach (char lineEnder in lineEnders)
            {
                if (tokens[pos - i][0].Equals(lineEnder))
                {
                    return lineEnder;
                }
            }
        }
        return ' ';
    }

    public string removeLastWordFromString(string inp)
    {
        int lineEnderIndex = -1;

        foreach(char ender in lineEnders)
        {
            int i = inp.LastIndexOf(ender);
            if(i != -1 && i > lineEnderIndex)
            {
                lineEnderIndex = i;
            }
        }
        if(lineEnderIndex == -1){return inp;}
        else
        {
            return inp.Substring(0, lineEnderIndex + 1);
        }
    }

    public char GetClosestEndOfLineTokenAfter(int pos, List<string> tokens)
    {
        for (int i = 0; i <= tokens.Count() - pos; i++)
        {
            foreach (char lineEnder in lineEnders)
            {
                if (tokens[pos + i][0].Equals(lineEnder))
                {
                    return lineEnder;
                }
            }
        }
        return ' ';
    }

    public int FindMainFunction(Dictionary<int, List<string>> tokens)
    {
        foreach (int key in tokens.Keys)
        {
            if (tokens[key][0].Equals("main_func"))
            {
                return key;
            }
        }
        return -1;
    }

    /* 
        input: tokenBefore, == tokenAfter
        only the toknes one position before and after are needed because
        every part of a expression needs to be a string for it to be a string comparison
    */
    public bool isStringComparision(string first, string second)
    {
        if (first.Contains("\"") || first.Contains("'") || second.Contains("\"") || second.Contains("'"))
        {
            return true;
        }
        else if (stringVariables.Contains(first) || stringVariables.Contains(second))
        {
            return true;
        }
        return false;
    }

    public string GetDatatypeOfToken(string token)
    {
        if (token.Contains('"') || token.Contains("'"))
        {
            return "string";
        }
        else if (int.TryParse(token, out int result))
        {
            return "int";
        }
        else if (bool.TryParse(token, out bool res))
        {
            return "bool";
        }
        else if (double.TryParse(token, out double resu))
        {
            return "double";
        }
        return "object";
    }

    public string removeLastExpressionFromString(string input)
    {
        int lastIndexOfEmptySpace = input.LastIndexOf(" ");
        int lastIndexOfOpeningBracket = input.LastIndexOf("(");

        int pos = -1;
        if (lastIndexOfEmptySpace > lastIndexOfOpeningBracket)
        {
            pos = lastIndexOfEmptySpace;
        }
        else
        {
            pos = lastIndexOfOpeningBracket + 1;
        }
        // If the string has only one or none word
        if (pos == -1)
        {
            return string.Empty;
        }

        return input.Substring(0, pos);
    }

    public int GetDistanceToClosingBrackets(List<string> tokens, string bracket)
    {
        Dictionary<string, string> bracketMapping = new Dictionary<string, string>{
            {")", "("},
            {"}", "{"},
            {"]", "["}
        };
        List<string> bracketsList = new List<string> { bracket };
        int distance = 1;
        foreach (string tok in tokens)
        {
            try
            {
                if (bracketMapping.Keys.Contains(tok))
                {
                    if (bracketMapping[tok].Equals(bracketsList.Last()))
                    {
                        if (bracketsList.Count == 0)
                        {
                            return distance;
                        }
                    }
                }
                else if (bracketMapping.Values.Contains(tok))
                {
                    bracketsList.Add(tok);
                }
                distance += 1;
            }
            catch (Exception ex)
            {
                distance += 1;
                continue;
            }
        }
        return -1;
    }

    public int GetPositionOfOpeningBracket(List<string> tokens)
    {
        int positionOfClosingBracket = tokens.Count() - 1;
        int posOfOpeningBracket = 0;
        Dictionary<string, string> closingToOpeningBracket = new Dictionary<string, string>(){
            {"}", "{"},
            {")", "("},
            {"]", "["}
        };

        string closingBracket = tokens[positionOfClosingBracket];
        string openingBracketToLookFor = closingToOpeningBracket[closingBracket];

        int numOfUnrelatedBracketsOfTheSameType = 0;
        for (int i = positionOfClosingBracket - 1; i > 0; i--)
        {
            if (tokens[i].Equals(openingBracketToLookFor))
            {
                if (numOfUnrelatedBracketsOfTheSameType > 0)
                {
                    numOfUnrelatedBracketsOfTheSameType -= 1;
                }
                else
                {
                    posOfOpeningBracket = i;
                    break;
                }
            }

            else if (tokens[i].Equals(closingBracket))
            {
                numOfUnrelatedBracketsOfTheSameType += 1;
            }
        }
        return posOfOpeningBracket;
    }

    public int GetPositionOfOpeningBracketInString(string st)
    {
        int positionOfOpeningBracket = 0;
        Dictionary<char, char> closingToOpeningBracket = new Dictionary<char, char>(){
            {'}', '{'},
            {')', '('},
            {']', '['}
        };
        char closingBracket = st[st.Length - 1];
        char openingBracketToLookFor = closingToOpeningBracket[closingBracket];

        int numOfUnrelatedBracketsOfTheSameType = 0;
        for (int i = st.Length - 2; i > 0; i--)
        {
            if (st[i].Equals(openingBracketToLookFor))
            {
                if (numOfUnrelatedBracketsOfTheSameType > 0)
                {
                    numOfUnrelatedBracketsOfTheSameType -= 1;
                }
                else
                {
                    positionOfOpeningBracket = i;
                    break;
                }
            }
            else if (st[i].Equals(closingBracket))
            {
                numOfUnrelatedBracketsOfTheSameType += 1;
            }
        }
        return positionOfOpeningBracket;
    }


    // LUC format vor lists:
    // list := ["element", "element"];
    // or
    // list[int] = [10, 20];
    public string TranslateListCreation(List<string> tokens)
    {
        string listString = "List<";
        string listName = "";
        bool simplified = false;
        int posOfEq = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Equals("["))
            {
                posOfEq = i;
                simplified = true;
            }
            else if (tokens[i].Equals("=") || tokens[i].Equals(":="))
            {
                posOfEq = i;
                break;
            }
        }
        string datatype = "";
        if (!simplified)
        {
            // for performance reaons it is not recommended to just use a list of objects, so it is necessary to evaluate the datatype here
            datatype = GetDatatypeOfToken(tokens[posOfEq + 2]);
            listName = tokens[posOfEq - 1];
        }
        else
        {
            listName = tokens[0];
            datatype = tokens[2];
        }
        listString += datatype + "> " + listName + "= new List<" + datatype + ">(){";
        for(int i = posOfEq + 2; i < tokens.Count() - 1; i++)
        {
            if(!tokens[i].Equals("]") && !tokens[i].Equals(";"))
            {
                listString += tokens[i];
            }
        }
        listString += "};";
        return listString;
    }

    public string TranslateDictionaryCreation(List<string> tokens)
    {
        string output = "Dictionary<";
        int indexOfFirstKey = 8;

        /*
            Example input:
                my_dic{string, int} = {
                    "test": 10,
                }
        */
        string datatypeKeys = tokens[2];
        string datatypeValues = tokens[4];

        /*
            Example input:
                my_dic := {
                    "test": 10,
                }
        */
        if (tokens.Contains(":="))
        {
            datatypeKeys = GetDatatypeOfToken(tokens[3]);
            datatypeValues = GetDatatypeOfToken(tokens[5]);

            indexOfFirstKey = 3;
        }

        output += datatypeKeys + ", " + datatypeValues + "> " + tokens[0];
        output += "=new Dictionary<" + datatypeKeys + ", " + datatypeValues + ">" + "(){";

        // currently it is not possible to create a dictionary of dictionaries
        bool stillDictionaryInputs = true;
        int i = indexOfFirstKey;
        while (stillDictionaryInputs)
        {
            if (tokens[i].Equals("}"))
            {
                stillDictionaryInputs = false;
                break;
            }
            output += "{" + tokens[i] + ", " + tokens[i + 2] + "},";
            i += 3;
        }

        output += "};";
        return output;
    }

    public string TranslateCallOfInherentFunction(List<string> tokens)
    {
        string output = "";
        List<int> indixesOfInherentFunctions = new List<int>() { };
        for (int i = 0; i < tokens.Count() - 1; i++)
        {
            if (FunctionMapping.DirectMapping.Keys.Contains(tokens[i]))
            {
                indixesOfInherentFunctions.Add(i);
            }
        }
        foreach(int i in indixesOfInherentFunctions)
        {
            try
            {
                List<string> values = FunctionMapping.DirectMapping[tokens[i]];
                if(values[1].Equals("object"))
                {
                    output = tokens[i+2] + "." + values[0] + "(" + output;
                }
                else if(values[1].Equals("parameter"))
                {
                    output += values[0] + "(";
                    if(i == indixesOfInherentFunctions.Max())
                    {
                        if(i + 2 < tokens.Count())
                        {
                            output += tokens[i+2];
                        }
                    }
                }

                // add to import to imports
                List<string> importsList = imports.Split(';').ToList();
                string currentImport = values[2];
                bool isInImports = false;
                foreach(string imp in importsList)
                {
                    if(imp.Equals(currentImport))
                    {
                        isInImports = true;
                    }
                }
                if(!isInImports)
                {
                    imports += currentImport + ";";
                }

                bool parametersLeft = false;

                if(tokens.Count() > i+3)
                {
                    parametersLeft = tokens[i+3].Equals(",");
                }
                int currentParameterIndex = i + 4;
                while(parametersLeft)
                {
                    output += tokens[currentParameterIndex];
                    if(!tokens[currentParameterIndex].Equals(",") || !tokens[currentParameterIndex+1].Equals(","))
                    {
                        parametersLeft = false;
                    }
                }
            }
            catch(KeyNotFoundException ex)
            {
                return "not_inherent";
            }
        }
        if(indixesOfInherentFunctions.Count() == 0)
        {
            output += ")";
        }
        else
        {
            foreach(int i in indixesOfInherentFunctions){output += ")";}
        }

        return output;
    }

    // standard case: function int bla(string token){}
    public string TranslateFunctionHead(List<string> tokens)
    {
        string output = "";
        output = "public static ";
        int nextTok = 2;
        if (tokens[1].Equals("["))
        {
            output += "List<" + tokens[2] + "> ";
            nextTok += 2;
        }
        else if (!datatypes.Contains(tokens[1]))
        {
            output += "void ";
            nextTok -= 1;
        }
        else
        {
            output += TranslateToken(tokens[1]); // return value
        }
        string functionName = tokens[nextTok];
        output += functionName; 

        output += tokens[nextTok + 1];
        bool endOfHead = false;
        int currentPos = nextTok + 2;
        while (!endOfHead && currentPos < tokens.Count())
        {
            if (tokens[currentPos].Equals(")"))
            {
                output += ")";
                endOfHead = true;
                currentPos++;
                break;
            }
            else if (tokens[currentPos].Equals("["))
            {
                output += "List<" + tokens[currentPos + 1] + "> ";
                currentPos += 3;
            }
            if (tokens[currentPos].Equals("string"))
            {
                stringVariables.Add(tokens[currentPos + 1]);
            }
            output += TranslateToken(tokens[currentPos]);
            currentPos += 1;
        }

        return output;
    }

    // function format in LUC: function [return_type] name([parameters])
    public string TranslateFunction(List<string> functionTokens)
    {
        string func = "";
        int endOfHead = functionTokens.IndexOf("{", 0);
        func += TranslateFunctionHead(functionTokens.GetRange(0, endOfHead));
        functionTokens.RemoveRange(0, endOfHead);
        bool simpleTryCatch = false;
        string insertAfterNextEndOfLine = "";

        for (int i = 0; i < functionTokens.Count(); i++)
        {
            string currentTok = functionTokens[i];
            string token = TranslateToken(currentTok);

            if (FunctionMapping.DirectMapping.Keys.Contains(currentTok))
            {
                int functionEndIndex = functionTokens.IndexOf(")", i);
                List<string> functionRangeTokens = functionTokens.GetRange(i, functionEndIndex - i);
                func += TranslateCallOfInherentFunction(functionRangeTokens);
                int numFuncs = 0;
                foreach(string t in functionRangeTokens)
                {
                    if(FunctionMapping.DirectMapping.Keys.Contains(t))
                    {
                        numFuncs += 1;
                    }
                }
                i += functionEndIndex - i - 1 + numFuncs;
            }

            if (currentTok.Equals("string"))
            {
                stringVariables.Add(functionTokens[i + 1]);
            }

            if (token.Equals("__complex__"))
            {
                if (currentTok.Equals("while") || currentTok.Equals("if"))
                {
                    func += currentTok;
                    if (!functionTokens[i + 1].Equals("("))
                    {
                        functionTokens.Insert(i + 1, "(");
                        bool endOfLine = false;
                        int tmp = i;
                        int positionForClosingBracket = 0;
                        while (!endOfLine)
                        {
                            if (functionTokens[tmp] == "{")
                            {
                                endOfLine = true;
                                positionForClosingBracket = tmp;
                            }
                            tmp += 1;
                        }
                        functionTokens.Insert(positionForClosingBracket, ")");
                    }
                }
                if(currentTok.Equals("not"))
                {
                    char previousToken = func[func.Length - 1];
                    if(previousToken.Equals('(') || previousToken.Equals('='))
                    {
                        func += "!";
                    }
                    else
                    {
                        func += "!=";
                    }
                }
                if (currentTok.Equals("?"))
                {
                    if (functionTokens[i - 1].Equals("}"))
                    {
                        int openingBracketPos = GetPositionOfOpeningBracketInString(func);
                        func = func.Insert(openingBracketPos, "try");
                        func = func.Insert(func.Length, "catch(Exception __lucInternalException__)");
                    }

                    else
                    {
                        List<int> eolTokPos = new List<int>() { };
                        int indexLastEolT = -1;
                        // because the catch is after the end of the line
                        string funcWithoutLastLineEnder = func.Substring(0, func.Length - 1);
                        foreach (char lineEnder in lineEnders)
                        {
                            eolTokPos.Add(funcWithoutLastLineEnder.LastIndexOf(lineEnder));
                        }
                        indexLastEolT = eolTokPos.Max();
                        if (indexLastEolT == -1)
                        {
                            indexLastEolT = 0;
                        }
                        func = func.Insert(indexLastEolT + 1, "try{");
                        func += "}catch(Exception __lucInternalException__){";
                        simpleTryCatch = true;
                    }
                }

                else if (functionTokens[i].Equals(":="))
                {
                    int indexOfClosestSemicolon = functionTokens.IndexOf(";", i) + 1;
                    List<string> tokensForVar = functionTokens.GetRange(i - 1, indexOfClosestSemicolon - i);
                    func = func.Substring(0, func.LastIndexOf(GetClosestEndOfLineTokenBefore(i, functionTokens)) + 1);
                    func += TranslateVarDefinition(tokensForVar);
                    functionTokens.RemoveRange(i - 1, indexOfClosestSemicolon-i-1);
                    
                    // The number of tokens was reduced by a minimum of two and so it needs to decrease by 1
                    i -= 1;
                }
                // example list creation: my_list := [1, 2, 4] or my_list[int] = [1, 2, 4]
                else if (currentTok.Equals("["))
                {
                    // case list creation
                    if(datatypes.Contains(functionTokens[i+1]))
                    {
                        int lineEnderIndex = functionTokens.IndexOf(";", i);
                        List<string> tokensForListCreation = functionTokens.GetRange(i-1, lineEnderIndex - i + 1);
                        func = removeLastWordFromString(func);
                        func += TranslateListCreation(tokensForListCreation);
                        i = lineEnderIndex;
                    }
                    // case list access
                    else
                    {
                        func += "[";
                    }
                }
                else if (currentTok.Equals("=="))
                {
                    int firstPos = i - 1;
                    int secondPos = i + 1;
                    if (functionTokens[firstPos].Equals(")"))
                    {
                        firstPos -= 1;
                    }
                    if (functionTokens[secondPos].Equals("("))
                    {
                        secondPos += 1;
                    }

                    if (isStringComparision(functionTokens[firstPos], functionTokens[secondPos]))
                    {
                        func = removeLastExpressionFromString(func);
                        func += "String.Equals(" + functionTokens[firstPos] + ", " + functionTokens[secondPos] + ")";

                        functionTokens.RemoveAt(i + 1);
                    }
                    else
                    {
                        func += "==";
                    }

                }
                else if (currentTok.Equals("for"))
                {
                    bool found = false;
                    int pos = i;
                    while (!found)
                    {
                        if (!functionTokens[pos].Equals("{"))
                        {
                            pos += 1;
                        }
                        else
                        {
                            found = true;
                        }
                    }
                    func += TranslateForHead(functionTokens.GetRange(i, pos - i)) + "{";
                    functionTokens.RemoveRange(i, pos - i);
                }
            }
            else if (currentTok.Equals("if"))
            {
                func += token;
                if (!functionTokens[i + 1].Equals("("))
                {
                    functionTokens.Insert(i + 1, "(");
                    bool endOfLine = false;
                    int tmp = i;
                    int positionForClosingBracket = 0;
                    while (!endOfLine)
                    {
                        if (functionTokens[tmp] == "{")
                        {
                            endOfLine = true;
                            positionForClosingBracket = tmp;
                        }
                        tmp += 1;
                    }
                    functionTokens.Insert(positionForClosingBracket, ")");
                }
            }
            else if (lineEnders.Contains(token[0]))
            {
                func += insertAfterNextEndOfLine;
                if (simpleTryCatch)
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
        while ((start == -1 || end == -1) && indexLeft > 0 && indexRight < tokens.Count() - 2)
        {
            if (lineEnders.Contains(tokens[indexRight][0]))
            {
                end = indexRight;
            }
            if (lineEnders.Contains(tokens[indexLeft][0]))
            {
                start = indexLeft + 1;
            }
            indexRight += 1;
            indexLeft -= 1;
        }
        if (start == -1)
        {
            start = 0;
        }
        if (end == -1)
        {
            // -2 to exclude the line ending token
            end = tokens.Count() - 2;
        }
        return (start, end);
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
            {"return", "return "},
            {"=", "="},
            {"else", "else "},
            {"and", " && "},
            {"or", " || "},
            {"is", "=="},

        };

        List<string> complexKeywords = new List<string>{
            //"if", 
            "?", "while", ":=", "[", "for", "==", "not"
        };
        if (LucToCSharpToken.Keys.Contains(tok))
        {
            return LucToCSharpToken[tok];
        }
        else if (complexKeywords.Contains(tok) || FunctionMapping.DirectMapping.Keys.Contains(tok))
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
        if (!forHead[1].Equals("("))
        {
            forHead.Insert(1, "(");
            forHead.Add(")");
        }
        string output = "";
        bool isSimpleForeach = forHead.Count() == 4 || forHead.Count() == 2;
        bool isFullForeach = forHead.Contains("in");
        bool simpleFor = forHead.Count() == 6 || forHead.Count() == 5;

        if(forHead.Contains("["))
        {
            simpleFor = forHead.Count() == 9 || forHead.Count() == 8;
        }
        // example input: for(list)
        if (isSimpleForeach)
        {
            output += "foreach(var __lucIntern__ in ";
            int listNamePos = 2;
            output += forHead[listNamePos];
            output += ")";
        }

        // example input: for(string word in words)
        else if (isFullForeach)
        {
            output += "foreach(";
            int start = 1;
            if (forHead[1].Equals("("))
            {
                start += 1;
            }
            for (int i = start; i < forHead.Count(); i++)
            {
                output += TranslateToken(forHead[i]);
                if (!datatypes.Contains(forHead[i]))
                {
                    output += " ";
                }
            }
            output = output.Substring(0, output.Length - 1);
        }

        // for(i<20)
        else if (simpleFor)
        {
            output += "for(";
            string variable = forHead[2];

            string compOperator = forHead[3];
            string value = forHead[4];
            if(forHead[5].Equals("["))
            {
                value += forHead[5] + forHead[6] + forHead[7];
            }
            if (comparisonOperators.Contains(forHead[2]))
            {
                compOperator = forHead[2];
                value = forHead[3];

                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new Random();
                char[] result = new char[5];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = chars[random.Next(chars.Length)];
                }
                variable = new string(result);
            }
            output += "int " + variable + "=0;" + variable + compOperator + value + ";" + variable + "++" + ")";
        }
        else
        {
            int variableIndex = forHead.IndexOf("int") + 1;
            if (variableIndex == -1)
            {
                if (forHead[1].Equals("("))
                {
                    variableIndex = 2;
                }
                else
                {
                    variableIndex = 1;
                }
            }
            string variable = forHead[variableIndex];
            for (int i = 0; i < forHead.Count(); i++)
            {
                string t = forHead[i];
                if (t.Contains("+") || t.Contains("-"))
                {
                    if (t.Equals("+"))
                    {
                        output += variable + "+";
                    }
                    else if (t.Equals("-"))
                    {
                        output += variable + "-";
                    }
                }
                if (t.Equals("(") && !forHead[i - 1].Equals("for"))
                {
                    int functionEndIndex = forHead.IndexOf(")", i);
                    output += TranslateCallOfInherentFunction(forHead.GetRange(i - 1, 4));
                    i += functionEndIndex - i;
                }
                else
                {
                    try
                    {
                        if (!forHead[i + 1].Equals("(") || forHead[i].Equals("for"))
                        {
                            output += t + " ";
                        }

                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        continue;
                    }
                }
            }
            output = output.TrimEnd();
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
        for (int i = 0; i < tokens.Count(); i++)
        {
            string token = tokens[i];
            if (!token.Equals(":="))
            {
                if(FunctionMapping.DirectMapping.Keys.Contains(token))
                {   
                    int functionEndIndex = tokens.IndexOf(")", i);
                    List<string> functionToks = tokens.GetRange(i, functionEndIndex - i);
                    translation += TranslateCallOfInherentFunction(functionToks);
                    i += functionEndIndex -1;
                }
                else
                {
                    translation += TranslateToken(token);
                }
            }
            else
            {
                translation += "=";
            }
        }

        if (GetDatatypeOfToken(tokens[tokens.Count() - 2]).Equals("string"))
        {
            stringVariables.Add(tokens[tokens.Count() - 2]);
        }
        return translation;
    }


    public string TrnslateLine(List<string> tokens)
    {
        string outp = "";
        // only provisional
        foreach (string token in tokens)
        {
            outp += TranslateToken(token);
        }
        return "";
    }

    public List<int> GetFunctionStarts(List<string> tokens)
    {
        List<int> starts = new List<int>() { };
        for (int i = 0; i < tokens.Count() - 1; i++)
        {
            if (tokens[i].Equals("function"))
            {
                starts.Add(i);
            }
        }
        return starts;

    }

    // function that calles the rest of the translation functions
    public string Translate(List<string> tokens)
    {
        List<int> functionStarts = GetFunctionStarts(tokens);
        string code = "public class Program{";
        for (int i = 0; i < functionStarts.Count(); i++)
        {
            int start = functionStarts[i];
            int end = tokens.Count() + 1;
            if (i + 1 <= functionStarts.Count() - 1)
            {
                end = functionStarts[i + 1] + 1;
            }

            code += TranslateFunction(tokens.GetRange(start, end - start - 1)) + " ";
        }
        code = imports + code;
        code += "}";
        return code;
    }
}
#endregion