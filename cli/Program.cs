using CTFAK.EXE;
using CTFAK.CCN;
using CTFAK.CCN.Chunks;
using CTFAK.Core.CCN.Chunks;
using CTFAK.MFA;
using CTFAK.Memory;
using CTFAK.Utils;

class CliExporter
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: cli <SourceCode|Web> <game.exe> <output_dir>");
            return;
        }

        Logger.SetUILogAction(msg => Console.WriteLine(msg));

        BuildType buildType = Enum.Parse<BuildType>(args[0]);
        string exePath = args[1];
        string outputPath = args[2];

        Console.WriteLine($"BuildType: {buildType}");
        Console.WriteLine($"Input: {exePath}");
        Console.WriteLine($"Output: {outputPath}");

        if (!File.Exists(exePath))
        {
            Console.WriteLine("File not found!");
            return;
        }

        Directory.CreateDirectory(outputPath);
        DirectoryInfo outputDir = new DirectoryInfo(outputPath);

        string[] runtimeSearchPaths = new[] {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "runtime")),
            Path.GetFullPath(Path.Combine(".", "runtime")),
            Path.GetFullPath(Path.Combine(".", "..", "runtime")),
            Path.GetFullPath(Path.Combine(".", "..", "..", "runtime")),
        };
        DirectoryInfo runtimeBaseDir = null;
        foreach (var p in runtimeSearchPaths)
        {
            var d = new DirectoryInfo(p);
            if (d.Exists) { runtimeBaseDir = d; break; }
        }
        if (runtimeBaseDir == null)
            runtimeBaseDir = new DirectoryInfo(runtimeSearchPaths[0]);
        Console.WriteLine($"Runtime dir: {runtimeBaseDir.FullName} (exists: {runtimeBaseDir.Exists})");

        CCNFileReader ccnReader = new CCNFileReader();
        MFAFileReader mfaReader = new MFAFileReader();

        if (exePath.EndsWith(".mfa", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Input is .mfa file, loading MFA...");
            mfaReader.LoadGame(exePath);
            Console.WriteLine($"MFA Name: {mfaReader.mfa.Name}");

            Console.WriteLine("Building GameData from MFA...");
            var game = new GameData();
            game.name = mfaReader.mfa.Name ?? "SonicXG";
            game.aboutText = mfaReader.mfa.Description ?? "";
            game.Sounds = mfaReader.mfa.Sounds;
            game.Fonts = mfaReader.mfa.Fonts;

            var header = new AppHeader();
            header.WindowWidth = mfaReader.mfa.WindowX != 0 ? mfaReader.mfa.WindowX : 800;
            header.WindowHeight = mfaReader.mfa.WindowY != 0 ? mfaReader.mfa.WindowY : 600;
            header.InitialScore = mfaReader.mfa.InitialScore;
            header.InitialLives = mfaReader.mfa.InitialLifes;
            header.Controls = new Controls();
            header.Controls.Items = new List<PlayerControl>();
            game.header = header;

            var extHeader = new ExtendedHeader();
            extHeader.Flags = new BitDict(new string[] { "KeepScreenRatio", "1", "AntiAliasingWhenResizing", "2", "3", "RightToLeftReading", "4", "RightToLeftLayout", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "DontOptimizeStrings", "19", "20", "21", "DontIgnoreDestroy", "DisableIME", "ReduceCPUUsage", "22", "PremultipliedAlpha", "OptimizePlaySample" });
            game.ExtHeader = extHeader;

            ccnReader.game = game;
        }
        else
        {
            Console.WriteLine("Reading .exe file...");
            var exeReader = new ExeFileReader();
            exeReader.LoadGame(exePath);
            ccnReader.game = exeReader.getGameData();

            Console.WriteLine($"Game Name: {ccnReader.game.name}");

            // Find .mfa for frame data
            string editorFilename = ccnReader.game.editorFilename;
            if (!string.IsNullOrEmpty(editorFilename) && File.Exists(editorFilename))
            {
                Console.WriteLine($"Reading MFA from editor path: {editorFilename}");
                mfaReader.LoadGame(editorFilename);
            }
            else
            {
                var mfaFiles = Directory.GetFiles(".", "*.mfa", SearchOption.AllDirectories);
                if (mfaFiles.Length > 0)
                {
                    Console.WriteLine($"Found MFA: {mfaFiles[0]}");
                    mfaReader.LoadGame(mfaFiles[0]);
                }
                else
                {
                    Console.WriteLine("No MFA file found.");
                    mfaReader.mfa = new MFAData();
                }
            }
        }

        Console.WriteLine("Exporting with NuclearRT runtime...");
        Exporter exporter = new Exporter(ccnReader, mfaReader, runtimeBaseDir, outputDir);
        exporter.Export();

        Console.WriteLine("Extracting resources to pak...");
        PakBuilder pakBuilder = new PakBuilder();
        pakBuilder.Build(ccnReader, mfaReader, outputDir);

        if (buildType != BuildType.SourceCode)
        {
            Console.WriteLine("Compiling...");
            Compiler compiler = new Compiler();
            compiler.Compile(buildType, outputDir);
        }

        Console.WriteLine("Done!");
    }
}
