using System;

using Epos.Kubernetes;

using Pulumi.Kubernetes.Types.Inputs.Core.V1;

namespace LaTeX
{
    public class LaTeXStack : SimpleServiceStack
    {
        protected override string Namespace => Name;

        protected override string Name => "latex";

        protected override string Host => "latex.eposgmbh.info";

        protected override int ContainerPort => 5000;

        protected override ContainerArgs Container => new() {
            Name = Name,
            Image = "eposgmbh/latex-service:latest"
        };
    }
}
