using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Xunit;

public class TranspilerTests
{
    private const string TestFilePath = "testFile.txt"; // Sample test file path
    private const string TestJsonPath = "simplifiedCode.json"; // Sample JSON file path


    [Fact]
    public void TranslateVarDefintion()
    {
        var tokens = new List<string>()
        {
            "my_friend", ":=", "10" , ";"
        };
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        var transpiler = new Transpiler(TestFilePath);

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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
        List<string> output = trans.GetLineOfToken(inputIndex, input);
        List<string> expected = new List<string>(){
            "int", "test", "=", "10", ";"
        };

        Assert.Equal(expected, output);

    }

    [Fact]
    public void GetClosestEndOfLineToken_Test()
    {
        List<string> tokens = new List<string>(){
            "awesome", "{", "end", "the", "line"
        };
        char expected = '{';
        Transpiler t = new Transpiler(TestFilePath);
        char actual = t.GetClosestEndOfLineToken(2, tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OneLineTryCatch_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "test", "(", ")", "{","test", ":=", "10", ";", "?", "}"
        };
        Transpiler trans = new Transpiler(TestFilePath);
        string expected = "public int test(){try{var test=10;}catch(Exception ex){}";
        string actual = trans.TranslateFunction(tokens);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryCatchWithCodeInCatch_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "test", "(", ")", "{","test", ":=", "10", ";", "?", "int", "i", "=", "10", ";", "}"
        };
        Transpiler trans = new Transpiler(TestFilePath);
        string expected = "public int test(){try{var test=10;}catch(Exception ex){int i=10;}}";
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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
        Transpiler trans = new Transpiler(TestFilePath);
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

        Transpiler trans = new Transpiler(TestFilePath);
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

        string outputExpected = "foreach(string word in words)";

        Transpiler trans = new Transpiler(TestFilePath);
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateForSimplified()
    {
        List<string> input = new List<string>(){
            "for", "i", "<", "20"
        };

        string outputExpected = "for(int i=0; i<20; i++)";
        
        Transpiler trans = new Transpiler(TestFilePath);
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }

    [Fact]
    public void TranslateForFull()
    {
        List<string> input = new List<string>(){
            "for", "int", "i", "=", "20", ";", "i", ">", "10", ";", "-1",
        };

        string outputExpected = "for(int i = 20; i < 20; i > 10; i--)";

        Transpiler trans = new Transpiler(TestFilePath);
        string outputActual = trans.TranslateForHead(input);
        Assert.Equal(outputExpected, outputActual);
    }
    /*
    [Fact]
    public void TranslateComplexFunction_Test()
    {
        List<string> tokens = new List<string>(){
            "function", "int", "testFunc", "(", "string", "word", ")", "{", 
            "testList", "[", "int", "]", "=", "[", "10", "20", "30", "]", ";",
            "for", ""
        };
    }*/
}