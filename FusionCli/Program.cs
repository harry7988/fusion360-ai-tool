namespace FusionCli;

class Program
{
    static async Task<int> Main(string[] args) => await CommandDispatcher.Run(args);
}
