using System.Collections.Generic;
using System.IO;
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

    // Additional tests for other methods like TranslateFunction, TranslateToken, etc. can be added here.
}
