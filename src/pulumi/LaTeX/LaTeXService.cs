using Epos.Kubernetes;

namespace LaTeX;

public class LaTeXService : SimpleStatelessService
{
    public override string Name => "latex";

    public override Container Container => new(Name, "eposgmbh/latex-service:latest");

    public override IngressOptions? IngressOptions => new() {
        Host = "latex.eposgmbh.eu",
        UseLetsEncrypt = true
    };
}
