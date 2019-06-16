using Epos.CmdLine;

namespace Epos.LaTeX.CmdLine
{
    public class Program
    {
        public static int Main(string[] args) {
            var theCmdLineDefinition = new CmdLineDefinition {
                Name = "epos-auth",
                Subcommands = {
                    DefaultCommand.Instance,
                },
                HasDifferentiatedSubcommands = false
            };

            return theCmdLineDefinition.Try(args);
        }
    }
}
