using System.Diagnostics;
using System.Reflection;

namespace BGMPlayer.Models
{
    public class VersionInfo
    {
        public string ApplicationName { get; }
        public string ApplicationVersion { get; }

        public string CopyrightText { get; }
        public string ProjectURL { get; }

        public VersionInfo()
        {
            ApplicationName = "BGM鳴ら～すV3";
            var asm = Assembly.GetExecutingAssembly();
            ApplicationVersion = $"Version {asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}";

            CopyrightText = "Copyright (c) 2017-2021 dorisol1019";
            ProjectURL = "https://github.com/dorisol1019/BGMPlayer";
        }

        public void NavigateToProjectURL()
        {
            Process.Start(ProjectURL);
        }

    }
}
