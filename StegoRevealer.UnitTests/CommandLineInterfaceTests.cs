using StegoRevealer.Common.ConsoleInterface;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class CommandLineInterfaceTests
{
    [TestMethod]
    public void HandleAnalysisCommandTest()
    {
        string fileName1 = "cli_test_1.png";
        string fileName2 = "cli_test_2.png";
        string filePath1 = Path.Combine("TestData", fileName1);
        string filePath2 = Path.Combine("TestData", fileName2);
        string[] args = { "sa", filePath1, filePath2, "--chi", "--rs", "--kzha" };

        string error = string.Empty;
        try
        {
            CommandLineParser.HandleCommand(args).Wait();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        Assert.IsTrue(string.IsNullOrEmpty(error));
    }
}
