using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            CopyrightText = "Copyright (c) 2017-2020 dorisol1019";
            ProjectURL = "https://github.com/dorisol1019/BGMPlayer";
        }

        public void NavigateToProjectURL()
        {
            Process.Start(ProjectURL);
        }

    }
}
