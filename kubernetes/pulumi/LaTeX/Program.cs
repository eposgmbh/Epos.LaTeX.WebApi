using System.Threading.Tasks;

using Pulumi;

namespace LaTeX
{
    public static class Program
    {
        public static Task<int> Main() {
            return Deployment.RunAsync<LaTeXStack>();
        }
    }
}
