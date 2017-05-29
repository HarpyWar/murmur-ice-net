// (c) 2017 HarpyWar (harpywar@gmail.com))
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.BuildEngine;

namespace MurmurVersionCompiler
{
    class Program
    {
        private const string ProjectPath = @"..\..\..\Murmur";
#if DEBUG
        private const string projectType = "Debug";
#else
        private const string projectType = "Release";
#endif

        /// <summary>
        /// Files with it's original content
        /// </summary>
        private static Dictionary<string, string> contentDic = new Dictionary<string, string>();
        private static List<string> versions = new List<string>();
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(@"..\..\Slice", "*.cs");
            // iterate over all slices
            foreach (var f in files)
            {
                var v = Path.GetFileName(f).Replace("Murmur_", "").Replace(".cs", "");
                versions.Add(v);
            }
                
            foreach (var v in versions)
            {
                contentDic.Clear();

                Console.WriteLine(" - Version {0}", v);
                try
                {
                    // 1. Replace namespace and version
                    Console.WriteLine("\t1) Replace");
                    Replace(v);

                    // 2. Build
                    Console.WriteLine("\t2) Build");
                    Build(v);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.WriteLine();

                // 4. Restore namespace and version
                Restore();
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.Read();
        }

        [STAThread]
        static void Build(string version)
        {
            string shortVersion = version.Replace(".", "");

            // add all versions on defines
            string defines = "";
            foreach (var v in versions)
            {
                defines += string.Format("MURMUR_{0};", shortVersion);

                // stop adding constants
                if (v == version)
                    break;
            }
            // Instantiate a new Engine object
            var engine = new Engine();

            // Point to the path that contains the .NET Framework CLR and tools
            engine.BinPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(); 
           

            // Instantiate a new FileLogger to generate build log
            var logger = new FileLogger();

            // Set the logfile parameter to indicate the log destination
            logger.Parameters = @"logfile=" + Directory.GetCurrentDirectory() + @"\" + shortVersion + ".log";

            // Register the logger with the engine
            engine.RegisterLogger(logger);


            var bpg = new BuildPropertyGroup();
            bpg.SetProperty("Configuration", projectType);
            bpg.SetProperty("DefineConstants", defines);
            engine.GlobalProperties = bpg;


            // Build a project file 
            bool success = engine.BuildProjectFile(ProjectPath + @"\Murmur.csproj", "Build");

            //Unregister all loggers to close the log file
            engine.UnregisterAllLoggers();

            if (success)
            {
                Console.WriteLine("Build succeeded.");
                var dllFile = MurmurPlugin.MurmurVersion.GetFileNameFromVersion(version);
                // copy to directory up (because Release/Debug always clean)
                File.Copy(ProjectPath + @"\bin\" + projectType + @"\" + dllFile, ProjectPath + @"\bin\" + dllFile, true);
            }
            else
                Console.WriteLine(@"Build failed. View log for details");
        }

        /// <summary>
        /// Replace namespace in all code files
        /// </summary>
        /// <param name="version"></param>
        static void Replace(string version)
        {
            string shortVersion = version.Replace(".", "");

            // 3. copy Murmur_VERSION.cs
            File.Copy(@"..\..\Slice\Murmur_" + version + ".cs", ProjectPath + @"\Murmur.cs", true);

            //// 2. replace namespace in all code files
            //foreach (var file in Directory.GetFiles(ProjectPath, "*.cs"))
            //{
            //    replaceLine(file, "namespace Murmur", "namespace Murmur" + shortVersion);
            //}

            //// replace inner namespace in Murmur.cs
            //var f = ProjectPath + @"\Murmur.cs";
            //var content = File.ReadAllText(f);
            //content = content.Replace("Murmur.", "Murmur" + shortVersion + ".");
            //File.WriteAllText(f, content);

            // 3. replace AssemblyVersion in AssemblyInfo.cs
            replaceLine(ProjectPath + @"\Properties\AssemblyInfo.cs", "[assembly: AssemblyVersion", string.Format("[assembly: AssemblyVersion(\"{0}\")]", version));
            replaceLine(ProjectPath + @"\Properties\AssemblyInfo.cs", "[assembly: AssemblyFileVersion", string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version));
            
            
            // 3. replace AssemblyName in Murmur.csproj
            replaceLine(ProjectPath + @"\Murmur.csproj", "    <AssemblyName>", string.Format("    <AssemblyName>Murmur_{0}</AssemblyName>", version));
        }


        /// <summary>
        /// Restore original namespace in files after compilation
        /// </summary>
        static void Restore()
        {
            foreach (var f in contentDic)
            {
                File.WriteAllText(f.Key, f.Value);
            }
        }


        static void replaceLine(string filename, string startWith, string replaceWith)
        {
            var lines = File.ReadAllLines(filename);
            if (!contentDic.ContainsKey(filename))
                contentDic.Add(filename, string.Join("\n", lines)); // cache file content

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(startWith))
                    lines[i] = replaceWith;
            }
            File.WriteAllLines(filename, lines);
        }

    }
}
