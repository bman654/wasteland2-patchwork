namespace W2PW
{
    using System.IO;

    public static class Paths
    {
        /// <summary>
        /// Path to your Wasteland2 installation folder
        /// </summary>
        public const string W2InstallFolder = @"C:\Games\Steam\steamapps\common\Wasteland 2 Director's Cut";

        /// <summary>
        /// Folder where the public assemblies are stored.  The mod project should be referencing these files
        /// </summary>
        public const string PublicAssemblyWorkingFolder = @"D:\Documents\wasteland2-files\public";

        /// <summary>
        /// I'm using the timestamp of Build\WL2.exe as the game version
        /// </summary>
        public const string GameVersion = "20151104-1857-win";
        /// <summary>
        /// Folder where the original game assemblies have been copied.  When patching, these will be the files read in, patched, and written to the PatchedAssemblyFolder
        /// </summary>
        public const string OriginalAssemblyFolder = @"D:\Documents\wasteland2-files\" + GameVersion;

        /// <summary>
        /// This is where the patch program will write the patched assemblies
        /// </summary>
        public const string PatchedAssemblyFolder = @"D:\Documents\wasteland2-files\patched\" + GameVersion;
        
        /// <summary>
        /// Path to the Managed folder of the W2 installation
        /// </summary>
        public static string W2ManagedFolder
        {
            get { return Path.Combine(W2InstallFolder, @"Build\WL2_Data\Managed"); }
        }

        /// <summary>
        /// The game assembly in the original assembly folder
        /// </summary>
        public static string OriginalAssembly
        {
            get { return Path.Combine(OriginalAssemblyFolder, "Assembly-CSharp.dll"); }
        }

        /// <summary>
        /// The public assembly
        /// </summary>
        public static string PublicAssembly
        {
            get { return Path.Combine(PublicAssemblyWorkingFolder, "Assembly-CSharp.dll"); }
        }

        /// <summary>
        /// The patched assembly
        /// </summary>
        public static string PatchedAssembly
        {
            get { return Path.Combine(PatchedAssemblyFolder, "Assembly-CSharp.dll"); }
            //get { return Path.Combine(W2ManagedFolder, "Assembly-CSharp.dll"); }
        }
    }
}
