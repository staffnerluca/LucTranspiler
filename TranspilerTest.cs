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
}
