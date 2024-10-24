namespace HangfirePoc.Services;

public class RandomTextGenerator : IRandomTextGenerator
{
    public string Generate() => $"This is some random text: {Guid.NewGuid()}";
}