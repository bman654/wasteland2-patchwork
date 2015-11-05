using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace W2PW
{
    class ApplyPatch
    {
        static void Main(string[] args)
        {
            var log = LogFactory.CreateLog();

            PatchGame(log);

            log.Information("All done");
        }

        private static void PatchGame(ILogger log)
        {
            log.Information("Creating {0}", Paths.PatchedAssemblyFolder);
            Directory.CreateDirectory(Paths.PatchedAssemblyFolder);

            log.Information("Loading original assembly {0}", Paths.OriginalAssembly);
            var patcher = new AssemblyPatcher(Paths.OriginalAssembly, log);
            patcher.EmbedHistory = false;
            patcher.UseBackup = false;

            log.Information("Patching");
            patcher.PatchAssembly(typeof(W2PWMod.ModType).Assembly.Location);

            log.Information("Saving patched file");
            patcher.WriteTo(Paths.PatchedAssembly);
        }
    }
}
