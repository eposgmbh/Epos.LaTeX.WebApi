using System;

using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Networking.V1;

namespace Epos.Kubernetes
{
    public abstract class SimpleServiceStack : Stack
    {
        protected abstract string Namespace { get; }

        protected abstract string Name { get; }
        
        protected virtual string? Host => null;
        
        protected abstract ContainerArgs Container { get; }

        protected virtual VolumeArgs[]? Volumes => Array.Empty<VolumeArgs>();

        protected virtual int ServicePort => 80;
        
        protected virtual int ContainerPort => 5000;

        public SimpleServiceStack() {
            if (string.IsNullOrWhiteSpace(Namespace)) {
                throw new ArgumentException($"'{nameof(Namespace)}' cannot be null or whitespace.", nameof(Namespace));
            }
            if (string.IsNullOrWhiteSpace(Name)) {
                throw new ArgumentException($"'{nameof(Name)}' cannot be null or whitespace.", nameof(Name));
            }
            if (Container is null) {
                throw new ArgumentNullException(nameof(Container));
            }

            Container.Ports = new ContainerPortArgs { ContainerPortValue = ContainerPort };

            _ = new Pulumi.Kubernetes.Core.V1.Namespace(
                Namespace,
                new NamespaceArgs {
                    Metadata = new ObjectMetaArgs {
                        Name = Namespace
                    }
                }
            );

            var theLabels = new InputMap<string> {
                { "app", Name }
            };

            string theDeploymentName = $"{Name}-deployment";

            _ = new Pulumi.Kubernetes.Apps.V1.Deployment(
                theDeploymentName,
                new DeploymentArgs {
                    Metadata = new ObjectMetaArgs {
                        Namespace = Namespace,
                        Name = theDeploymentName,
                        Labels = theLabels
                    },
                    Spec = new Pulumi.Kubernetes.Types.Inputs.Apps.V1.DeploymentSpecArgs {
                        Selector = new LabelSelectorArgs {
                            MatchLabels = theLabels
                        },
                        Replicas = 1,
                        Template = new PodTemplateSpecArgs {
                            Metadata = new ObjectMetaArgs {
                                Labels = theLabels
                            },
                            Spec = new PodSpecArgs {
                                Containers = Container,
                                Volumes = Volumes!
                            }
                        }
                    }
                }
            );

            string theServiceName = $"{Name}-service";

            _ = new Pulumi.Kubernetes.Core.V1.Service(
                theServiceName,
                new ServiceArgs {
                    Metadata = new ObjectMetaArgs {
                        Namespace = Namespace,
                        Name = theServiceName,
                        Labels = theLabels
                    },
                    Spec = new ServiceSpecArgs {
                        Selector = theLabels,
                        Ports = new ServicePortArgs {
                            Port = ServicePort,
                            TargetPort = ContainerPort
                        }
                    }
                }
            );

            if (Host != null) {
                string theIngressName = $"{Name}-ingress";

                _ = new Pulumi.Kubernetes.Networking.V1.Ingress(
                    theIngressName,
                    new IngressArgs {
                        Metadata = new ObjectMetaArgs {
                            Namespace = Namespace,
                            Name = theIngressName,
                            Labels = theLabels,
                            Annotations = new InputMap<string> {
                                { "cert-manager.io/cluster-issuer", "lets-encrypt" }
                            }
                        },
                        Spec = new IngressSpecArgs {
                            Tls = new IngressTLSArgs {
                                SecretName = $"tls-secret-{Host}",
                                Hosts = Host
                            },
                            Rules = new IngressRuleArgs {
                                Host = Host,
                                Http = new HTTPIngressRuleValueArgs {
                                    Paths = new HTTPIngressPathArgs {
                                        Path = "/",
                                        PathType = "Prefix",
                                        Backend = new IngressBackendArgs {
                                            Service = new IngressServiceBackendArgs {
                                                Name = theServiceName,
                                                Port = new ServiceBackendPortArgs {
                                                    Number = ServicePort
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                );
            }
        }
    }
}
