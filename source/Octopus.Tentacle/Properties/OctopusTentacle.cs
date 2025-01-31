using System.Reflection;
using Octopus.Client.Model;
using Octopus.Shared.Util;
using Octopus.Tentacle.Versioning;
using AssemblyExtensions = Octopus.Tentacle.Versioning.AssemblyExtensions;

namespace Octopus.Tentacle.Properties
{
    public static class OctopusTentacle
    {
        public static readonly Assembly Assembly = typeof (OctopusTentacle).Assembly;
        public static readonly string InformationalVersion = AssemblyExtensions.GetInformationalVersion(Assembly);
        public static readonly SemanticVersionInfo SemanticVersionInfo = new SemanticVersionInfo(Assembly);
        public static readonly SemanticVersion Version = new SemanticVersion(SemanticVersionInfo.NuGetVersion);
        public static readonly string[] EnvironmentInformation = EnvironmentHelper.SafelyGetEnvironmentInformation();
    }
}