using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Patchwork.Utility;

namespace W2PW
{
    class MakePublicAssemblies
    {
        static void Main(string[] args)
        {
            var log = LogFactory.CreateLog();

            log.Information("Creating folder {0}", Paths.PublicAssemblyWorkingFolder);
            Directory.CreateDirectory(Paths.PublicAssemblyWorkingFolder);

            log.Information("Reading {0}, writing public version to {1}", Paths.OriginalAssembly, Paths.PublicAssembly);

            log.Information("Loading original assembly");
            var publicAssembly = AssemblyDefinition.ReadAssembly(Paths.OriginalAssembly);

            log.Information("Making everything public");
            CecilHelper.MakeOpenAssembly(publicAssembly, true);

            log.Information("Saving public assembly");
            File.Delete(Paths.PublicAssembly);
            publicAssembly.Write(Paths.PublicAssembly);

            log.Information("Success");
            Console.ReadKey();
        }
    }
}
