using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text.Json;
using Xunit;
using Xunit.Sdk;

public class TranspilerTests
{
    [Fact]
    public void TranslateVarDefintion()
    {
        var tokens = new List<string>()
        {
            "my_friend", ":=", "10" , ";"
        };
        Transpiler trans = new Transpiler();
        string actual = trans.TranslateVarDefinition(tokens);
        string expected = "var my_friend=10;";
        Assert.Equal(expected, actual);
    }


    [Fact]
    public void TranslateSimpleFunction_Test()
    {
        List<string> input = new List<string>(){
            "function", "int", "test", "(", ")", "{", "}"
        };
        string expected = "public int test(){}";
        Transpiler trans = new Transpiler();
        string output = trans.TranslateFunction(input);

        Assert.Equal(expected, output); 
    }

    [Fact]
    public void TranslateFunctionWithVarDecleration_Test()
    {
        List<string> input = new List<string>(){
            "function", "int", "test", "(", ")", "{","int", "test", "=", "10", ";", "}"
        };
        string expected = "public int test(){int test=10;}";
        Transpiler trans = new Transpiler();
        string output = trans.TranslateFunction(input);

        Assert.Equal(expected, output); 
    }

    [Fact]
    public void TranslateFunctionWithSimplifiedVarDecleration_Test()
    {
        List<string> input = new List<string>(){
            "function", "int", "test", "(", ")", "{","test", ":=", "10", ";", "}"
        };
        string expected = "public int test(){var test=10;}";
        Transpiler trans = new Transpiler();
        string output = trans.TranslateFunction(input);

        Assert.Equal(expected, output); 
    }

    [Fact]
    public void FindMainFunction_ShouldReturnCorrectKey()
    {
        // Arrange
        var tokens = new Dictionary<int, List<string>>
        {
            { 0, new List<string> { "main_func" } },
            { 1, new List<string> { "function" } }
        };
        var transpiler = new Transpiler();

        // Act
        var result = transpiler.FindMainFunction(tokens);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void TranslateVarDeclaration_Test()
    {
        List<string> input = new List<string>(){
            "test", ":=", "10", ";"
        };
        string expected = "var test=10;";
        Transpiler trans = new Transpiler();
        string output = trans.TranslateVarDefinition(input);

        Assert.Equal(expected, output); 
    }

    [Fact]
    public void GetLineOfCurrentToken_Test()
    {
        List<string> input = new List<string>(){
            "function", "int", "test", "(", ")", "{", "do_something_cool", "(", ")", ";", "int", "test", "=", "10", ";", "lorem", "ipsum", "=", "il", "dolor", ";", "}"
        };
        int inputIndex = 12;
        Transpiler trans = new Transpiler();
        List<string> output = trans.GetLineOfToken(inputIndex, input);
        List<string> expected = new List<string>(){
            "int", "test", "=", "10", ";"
        };

        Assert.Equal(expected, output);

    }

    [Fact]
    public void GetClosestEndOfLineTokenBefore_Test()
    {
        List<string> tokens = new List<string>(){
            "awesome", "{", "end", "the", "line"
        };
        char expected = '{';
        Transpiler t = new Transpiler();
        char actual = t.GetClosestEndOfLineTokenBefore(2, tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OneLineTryCatch_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "test", "(", ")", "{","test", ":=", "10", ";", "?", "}"
        };
        Transpiler trans = new Transpiler();
        string expected = "public int test(){try{var test=10;}catch(Exception __lucInternalException__){}";
        string actual = trans.TranslateFunction(tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryCatchWithCodeInCatch_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "test", "(", ")", "{","test", ":=", "10", ";", "?", "int", "i", "=", "10", ";", "}"
        };
        Transpiler trans = new Transpiler();
        string expected = "public int test(){try{var test=10;}catch(Exception __lucInternalException__){int i=10;}}";
        string actual = trans.TranslateFunction(tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TranslateListCreationWithoutTypeDeclared_Test()
    {
         List<string> tokens = new List<string>(){
            "testList", ":=", "[", "10", "20", "30", "]", ";"
        };
        string expected = "List<int> testList= new List<int>(){10, 20, 30};";
        Transpiler trans = new Transpiler();
        string actual = trans.TranslateListCreation(tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TranslateListCreationWithTypeDeclaration_Test()
    {
        List<string> tokens = new List<string>(){
            "testList", "[", "int", "]", "=", "[", "10", "20", "30", "]", ";"
        };
        string expected = "List<int> testList= new List<int>(){10, 20, 30};";
        Transpiler trans = new Transpiler();
        string actual = trans.TranslateListCreation(tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TranslateFucntionWithListDeclaration_Test()
    {
        List<string> tokens = new List<string>(){
             "function", "int", "test", "(", ")", "{", "testList", "[", "int", "]", "=", "[", "10", "20", "30", "]", ";", "}"
        };
        string expected = "public int test(){List<int> testList= new List<int>(){10, 20, 30};}";
        Transpiler trans = new Transpiler();
        string actual = trans.TranslateFunction(tokens);
        Assert.True(actual == expected, actual);
        //Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetStartAndEndOfLine_Test()
    {
        List<string> tokens = new List<string>(){
             "function", "int", "test", "(", ")", "{", "testList", "[", "int", "]", "=", "[", "10", "20", "30", "]", ";", "}"
        };
        int expectedStart = 6;
        int expectedEnd = 16;
        Transpiler trans = new Transpiler();
        int actualStart = 0;
        int actualEnd = 0;
        (actualStart, actualEnd) = trans.GetStartAndEndOfLine(8, tokens); 
        Assert.Equal((actualStart, actualEnd), (expectedStart, expectedEnd));
    }

    [Fact]
    public void TranslateForeach_Test()
    {
        List<string> input = new List<string>(){
            "for", "(", "list", ")"
        };

        string outputExpected = "foreach(var __lucIntern__ in list)";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateFullForeach_Test()
    {
        List<string> input = new List<string>()
        {
            "for", "string", "word", "in", "words"
        };

        string outputExpected = "foreach(string word in words )";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateForSimplified()
    {
        List<string> input = new List<string>(){
            "for", "i", "<", "20"
        };

        string outputExpected = "for(int i=0;i<20;i++)";
        
        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateForFull()
    {
        List<string> input = new List<string>(){
            "for", "int", "i", "=", "20", ";", "i", ">", "10", ";", "-",
        };

        string outputExpected = "for ( int i = 20 ; i > 10 ; i--)";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateComplexFunction_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "testFunc", "(", "string", "word", ")", "{", 
            "testList", "[", "int", "]", "=", "[", "10", "20", "30", "]", ";",
            "for", "(", "testList", ")", "{",
            "print", "(", "'Hello World'", ")", ";", "}",
            "r", "10", ";",
            "}"
        };
        string outputExpected = "public int testFunc(string word){List<int> testList= new List<int>(){10, 20, 30};foreach(var __lucIntern__ in testList){Console.WriteLine('Hello World');}return 10;}";

        Transpiler tok = new Transpiler();
        string outputActual = tok.TranslateFunction(tokens);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateBubbleSortHead_Test()
    {
        List<string> tokens = new List<string>()
        {
            "function", "[", "int", "]", "bubble_sort", "(", "[", "int", "]", "to_sort", ")"
        };

        string outputExpected = "public List<int> bubble_sort(List<int> to_sort)";
        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunctionHead(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateFunctionCallParameter_Test()
    {
        List<string> tokens = new List<string>()
        {
            "print", "(", "'Hello World'", ")", ";"
        };

        string outputExpected = "Console.WriteLine('Hello World')";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateCallOfInherentFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateCallOfInherentFunctionObject_Test()
    {
        List<string> tokens = new List<string>(){
            "len", "(", "testList", ")"
        };

        string outputExpected = "testList.Count()";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateCallOfInherentFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateBubbleSort_Test()
    {
        List<string> tokens = new List<string>()
        {
            "function", "[", "int", "]", "bubble_sort", "(", "[", "int", "]", "to_sort", ")", "{", 
            "for", "(", "int", "i", "=", "1", ";", "i", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "for", "(", "int", "j", "=", "0", ";", "j", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "if", "(", "to_sort", "[", "j", "]", ">", "to_sort", "[", "j+1", "]", ")", "{",
            "int", "temp", "=", "to_sort", "[", "j", "]", ";",
            "to_sort", "[", "j", "]", "=", "to_sort", "[", "j", "+", "1", "]",";", 
            "to_sort", "[", "j", "+", "1", "]", "=", "temp", ";", "}", "}", "}",
            "}"
        };

        string outputExpected = "public List<int> bubble_sort(List<int> to_sort){for ( int i = 1 ; i <= to_sort.Count()-1 ; i++){for ( int j = 0 ; j <= to_sort.Count()-1 ; j++){if(to_sort[j]>to_sort[j+1]){int temp=to_sort[j];to_sort[j]=to_sort[j+1];to_sort[j+1]=temp;}}}}";
        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateInhernetFunctionCallPrint_Test()
    {
        List<string> tokens = new List<string>(){
            "print", "(", "'Hello World'", ")"
        };

        string outputExpected = "Console.WriteLine('Hello World')";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateCallOfInherentFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateInhernetFunctionCallLen_Test()
    {
        List<string> tokens = new List<string>(){
            "len", "(", "my_list", ")"
        };

        string outputExpected = "my_list.Count()";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateCallOfInherentFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateTokenInherentFunc_Test()
    {
        string tok = "print";
        string outputExpected = "__complex__";
        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateToken(tok);
        
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateInherentFunctionUsingTranslateFunction()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{", "print", "(", "'Hello World'", ")", ";", "}"
        };

        string outputExpected = "public void test_func(){Console.WriteLine('Hello World');}";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void TranslateAccessToListItem_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{", "ls", "[", "10", "]", ";", "}"
        };

        string outputExpected = "public void test_func(){ls[10];}";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void TranslateIfWithAccessToListElement_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{", "if", "(", "ls", "[", "5", "]", ")", "{", "}", "}"
        };

        string outputExpected = "public void test_func(){if(ls[5]){}}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

        [Fact]
    public void TranslateIfWithAccessToListElementInVarDeclaration_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{", "int", "test", "=", "ls", "[", "5", "]", "}"
        };

        string outputExpected = "public void test_func(){int test=ls[5]}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateSimpleCalculator_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "test_func", "(", "string", "sign", ",", "int", "first", ",", "int", "second", ")", "{",
            "if", "sign", "==", "\"+\"", "{",
            "r", "first", "+", "second", ";",
            "}",
            "else", "if", "(", "sign", "==", "\"-\"", ")", "{",
            "int", "result", "=", "first", "+", "second", ";",
            "r", "result", ";", "}",
            "else", "{", "return", "0", "}",
            "}"
        };

        string outputExpected = "public int test_func(string sign,int first,int second){if(String.Equals(sign, \"+\")){return first+second;}else if(String.Equals(sign, \"-\")){int result=first+second;return result;}else {return 0}}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateWhile_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "my_func", "(", ")", "{",
            "bool", "notFound", "=", "true", ";",
            "while", "(", "notFound", ")", "{",
            "print", "(", "'hi'", ")", ";",
            "}", "}"
        };

        string outputExpected = "public void my_func(){bool notFound=true;while(notFound){Console.WriteLine('hi');}}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateMultilineTryCatch_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{",
            "bool", "test", "=", "false", ";",
            "{",
            "print", "(", "'Hello World'", ")", ";",
            "test", "=", "true", ";",
            "}", "?",
            "{",
            "print", "(", "'Error'", ")", ";",
            "test", "=", "false", ";",
            "}",
            "}"
        };

        string outputExpected = "public void test_func(){bool test=false;try{Console.WriteLine('Hello World');test=true;}catch(Exception __lucInternalException__){Console.WriteLine('Error');test=false;}}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void GetPositionOfOpeningBracketSimple_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{",
            "bool", "test", "=", "false", ";",
            "{",
            "print", "(", "'Hello World'", ")", ";",
            "test", "=", "true", ";",
            "}",
        };

        int outputExpected = 10;

        Transpiler trans = new Transpiler();
        int ouptutActual = trans.GetPositionOfOpeningBracket(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void GetPositionOfOpeningBracketWithIf_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", ")", "{",
            "bool", "test", "=", "false", ";",
            "{",
            "if", "(", "true", ")", "{",
            "print", "(", "'Hello World'", ")", ";", "}",
            "test", "=", "true", ";",
            "}",
        };

        int outputExpected = 10;

        Transpiler trans = new Transpiler();
        int ouptutActual = trans.GetPositionOfOpeningBracket(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void GetPositionOfOpeningBracketInString_Test()
    {
        string input = "functino test_func(){bool test=false;{if(true){do_stuff();}test=true;}";
        int outputExpected = 37;

        Transpiler trans = new Transpiler();
        int outputActual = trans.GetPositionOfOpeningBracketInString(input);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateDictionaryCreationSimplified()
    {
        List<string> tokens = new List<string>(){
            "my_dic", ":=", "{", "'test'", ":", "10", "}", ";"
        };

        string outputExpected = "Dictionary<string, int> my_dic=new Dictionary<string, int>(){{'test', 10},};";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateDictionaryCreation(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateDictionaryCreation_Test()
    {
        List<string> tokens = new List<string>(){
            "my_dic", "{", "string", ", ", "int", "}", "=", 
            "{",
            "'test'", ":", "10",
            "}", ";"
        };

        string outputExpected = "Dictionary<string, int> my_dic=new Dictionary<string, int>(){{'test', 10},};";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateDictionaryCreation(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void TranslateDictionaryCreationWithoutEntries_Test()
    {
        List<string> tokens = new List<string>(){
            "my_dic", "{", "string", ", ", "int", "}", "=", "{", "}", ";"
        };

        string outputExpected = "Dictionary<string, int> my_dic=new Dictionary<string, int>(){};";
        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateDictionaryCreation(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void GetDatatypeOfTokenString_Test()
    {
        string input = "'test'";

        string outputExpected = "string";
        Transpiler trans = new Transpiler();
        string ouptutActual = trans.GetDatatypeOfToken(input);

        Assert.Equal(outputExpected, outputExpected);
    }

    [Fact]
    public void TranslateMultipleFunctions()
    {
        List<string> tokens = new List<string>()
        {
            "function", "[", "int", "]", "bubble_sort", "(", "[", "int", "]", "to_sort", ")", "{", 
            "for", "(", "int", "i", "=", "1", ";", "i", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "for", "(", "int", "j", "=", "0", ";", "j", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "if", "(", "to_sort", "[", "j", "]", ">", "to_sort", "[", "j+1", "]", ")", "{",
            "int", "temp", "=", "to_sort", "[", "j", "]", ";",
            "to_sort", "[", "j", "]", "=", "to_sort", "[", "j", "+", "1", "]",";", 
            "to_sort", "[", "j", "+", "1", "]", "=", "temp", ";", "}", "}", "}",
            "}",

            "function", "int", "test_func", "(", "string", "sign", ",", "int", "first", ",", "int", "second", ")", "{",
            "if", "sign", "==", "\"+\"", "{",
            "r", "first", "+", "second", ";",
            "}",
            "else", "if", "(", "sign", "==", "\"-\"", ")", "{",
            "int", "result", "=", "first", "+", "second", ";",
            "r", "result", ";", "}",
            "else", "{", "return", "0", "}",
            "}"
        };

        string outputExpected = "using System;public class Program{public List<int> bubble_sort(List<int> to_sort){for ( int i = 1 ; i <= to_sort.Count()-1 ; i++){for ( int j = 0 ; j <= to_sort.Count()-1 ; j++){if(to_sort[j]>to_sort[j+1]){int temp=to_sort[j];to_sort[j]=to_sort[j+1];to_sort[j+1]=temp;}}}} public int test_func(string sign,int first,int second){if(String.Equals(sign, \"+\")){return first+second;}else if(String.Equals(sign, \"-\")){int result=first+second;return result;}else {return 0}} }";
        
        Transpiler trans = new Transpiler();
        string outputActual = trans.Translate(tokens);

        Assert.Equal(outputExpected, outputActual);
    } 

    [Fact]
    public void GetFunctionStarts_Test()
    {
        List<string> tokens = new List<string>()
        {
            "function", "[", "int", "]", "bubble_sort", "(", "[", "int", "]", "to_sort", ")", "{", 
            "for", "(", "int", "i", "=", "1", ";", "i", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "for", "(", "int", "j", "=", "0", ";", "j", "<=", "len", "(", "to_sort", ")", "-1", ";", "+", ")", "{",
            "if", "(", "to_sort", "[", "j", "]", ">", "to_sort", "[", "j+1", "]", ")", "{",
            "int", "temp", "=", "to_sort", "[", "j", "]", ";",
            "to_sort", "[", "j", "]", "=", "to_sort", "[", "j", "+", "1", "]",";", 
            "to_sort", "[", "j", "+", "1", "]", "=", "temp", ";", "}", "}", "}",
            "}",

            "function", "int", "test_func", "(", "string", "sign", ",", "int", "first", ",", "int", "second", ")", "{",
            "if", "sign", "==", "'+'", "{",
            "r", "first", "+", "second", ";",
            "}",
            "else", "if", "(", "sign", "==", "'-'", ")", "{",
            "int", "result", "=", "first", "+", "second", ";",
            "r", "result", ";", "}",
            "else", "{", "return", "0", "}",
            "}"
        };

        List<int> outputExpected = new List<int>(){0, 94};
        Transpiler trans = new Transpiler();
        List<int> outputActual = trans.GetFunctionStarts(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void isStringComparision_Test()
    {
        string first = "\"test\"";
        string second = "\"thisTest\"";

        Transpiler trans = new Transpiler();
        bool output = trans.isStringComparision(first, second);

        Assert.True(output);
    }

    [Fact]
    public void TranslateStringComparison_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "funci", "(", "string", "first", ")", "{",
            "string", "second", "=", "\"test\"", ";",
            "if", "(", "first", "==", "second", ")", "{", "}",
            "}"
        };

        string outputExpected = "public void funci(string first){string second=\"test\";if(String.Equals(first, second)){}}";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void TranslateStringDeclaration()
    {
        List<string> tokens = new List<string>(){
            "function", "string", "testStringDec", "(", ")", "{",
            "string", "t", ":=", "\"my way\"", ";",
            "}",
        };

        string outputExpected = "public string testStringDec(){var t=\"my way\";}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

        [Fact]
    public void TranslateIntDeclaration()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "testIntDec", "(", ")", "{",
            "int", "i", "=", "12", ";",
            "}"
        };

        string outputExpected = "public int testIntDec(){int i=12;}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void removeLastExpressionFromStringWord_Test()
    {
        string test = "hallo Welt";

        string outputExpected = "hallo";

        Transpiler trans = new Transpiler();
        string outputActual = trans.removeLastExpressionFromString(test);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void removeLastExpressionFromStringExpression_Test()
    {
        string test = "if(12+25";

        string outputExpected = "if(";

        Transpiler trans = new Transpiler();
        string outputActual = trans.removeLastExpressionFromString(test);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void removeLastExpressionFromStringThatContainsBracket()
    {
        string test = "if(test";

        string outputExpected = "if(";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.removeLastExpressionFromString(test);

        Assert.Equal(outputExpected, ouptutActual);
    }

    [Fact]
    public void TranslateFunctonHeadWithStrings_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "test_func", "(", "string", "first", ",", "string", "second", ")", "{",
        };

        string outputExpected = "public void test_func(string first,string second){";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateMainHead()
    {
        List<string> tokens = new List<string>(){
            "functon", "Main", "(", ")", "{",
            "print", "(", "\"Hello World\"", ")", ";", 
            "}"
        };

        string outputExpected = "public static void Main (){Console.WriteLine(\"Hello World\");}";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    }

    /* Delete if not fixed
    [Fact]
    public void TranslateFunctonWithStringComp_Test()
    {
        // need to account for special case var declaration
        List<string> tokens = new List<string>(){
            "function", "testi", "(", "string", "first", ",", "string", "second", ")", "{", 
            "test", ":=", "first", "==", "\"test\"", ";",
            "}"
        };

        string outputExpected = "public void testi(string first,string second){var test=first.Equals\"test\";}";

        Transpiler trans = new Transpiler(TestFilePath);
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    } */

    [Fact]
    public void TranslateFunctionWithToString_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "a_func_with_to_string", "(", ")", "{",
            "string", "test", "=", "to_string", "(", "12", ")", ";",
            "}"
        };
        
        string outputExpected = "public void a_func_with_to_string(){string test=Convert.ToString(12);}";

        Transpiler trans = new Transpiler();
        string ouptutActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, ouptutActual);
    } 

    [Fact]
    public void TranslateMultipleFunctionCallsInOneLineInTranslateFunction_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "func", "(", ")", "{",
            "print", "(", "to_string", "(", "12", ")", ")", ";",
            "}"
        };

        string outputExpected = "public void func(){Console.WriteLine(Convert.ToString(12)));}";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateMultipleFunctionCallsInOneLine_Test()
    {
        List<string> tokens = new List<string>(){
            "print", "(", "to_string", "(", "12", ")", ")",
        };

        string outputExpected = "Console.WriteLine(Convert.ToString(12))";

        Transpiler trans = new Transpiler();
        string outputActual = trans.TranslateCallOfInherentFunction(tokens);

        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateInhernetFunctionCallInForeach_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "func", "(", ")", "{",
                "for", "i", "<", "20", "{",
                    "print", "(", "\"Hello\"", ")", ";",
                "}",
            "}"
        };
    }
}