using CTFAK.FileReaders;
using CTFAK.CCN;
using CTFAK.MFA;
using CTFAK.EXE;

public class Exporter
{
	public static Exporter Instance { get; private set; }

	private readonly IFileReader _ccnReader;
	private readonly IFileReader _mfaReader;
	private readonly DirectoryInfo _runtimeBasePath;
	private readonly DirectoryInfo _outputPath;

	private readonly AppDataExporter _appDataExporter;
	private readonly ObjectInfoExporter _objectInfoExporter;
	private readonly ImageBankExporter _imageBankExporter;
	private readonly SoundBankExporter _soundBankExporter;
	private readonly FontBankExporter _fontBankExporter;
	private readonly EffectBankExporter _effectBankExporter;
	private readonly FrameExporter _frameExporter;
	private readonly ProjectFileExporter _projectFileExporter;
	private readonly ExtensionFolderExporter _extensionFolderExporter;
	private readonly IconExporter _iconExporter;

	public GameData GameData => _ccnReader.getGameData();
	public MFAData MfaData => (_mfaReader as MFAFileReader).mfa;
	public DirectoryInfo RuntimeBasePath => _runtimeBasePath;
	public DirectoryInfo OutputPath => _outputPath;
	public int CurrentFrame { get; set; } = -1;

	public Exporter(IFileReader ccnReader, IFileReader mfaReader, DirectoryInfo runtimeBasePath, DirectoryInfo outputPath)
	{
		Instance = this;

		_ccnReader = ccnReader;
		_mfaReader = mfaReader;
		_runtimeBasePath = runtimeBasePath;
		_outputPath = outputPath;

		_appDataExporter = new AppDataExporter(this);
		_objectInfoExporter = new ObjectInfoExporter(this);
		_imageBankExporter = new ImageBankExporter(this);
		_soundBankExporter = new SoundBankExporter(this);
		_fontBankExporter = new FontBankExporter(this);
		_effectBankExporter = new EffectBankExporter(this);
		_frameExporter = new FrameExporter(this);
		_projectFileExporter = new ProjectFileExporter(this);
		_extensionFolderExporter = new ExtensionFolderExporter(this);
		_iconExporter = new IconExporter(this);
	}

	public void Export()
	{
		// copy runtime base path files to the output path
		FileUtils.CopyFilesRecursively(RuntimeBasePath.FullName, OutputPath.FullName);

		try { _projectFileExporter.Export(); } catch (Exception ex) { Console.WriteLine($"ProjectFileExporter failed: {ex.Message}"); }
		try { _extensionFolderExporter.Export(); } catch (Exception ex) { Console.WriteLine($"ExtensionFolderExporter failed: {ex.Message}"); }
		try { _appDataExporter.Export(); } catch (Exception ex) { Console.WriteLine($"AppDataExporter failed: {ex.Message}"); }
		try { _objectInfoExporter.Export(); } catch (Exception ex) { Console.WriteLine($"ObjectInfoExporter failed: {ex.Message}"); }
		try { _imageBankExporter.Export(); } catch (Exception ex) { Console.WriteLine($"ImageBankExporter failed: {ex.Message}"); }
		try { _soundBankExporter.Export(); } catch (Exception ex) { Console.WriteLine($"SoundBankExporter failed: {ex.Message}"); }
		try { _fontBankExporter.Export(); } catch (Exception ex) { Console.WriteLine($"FontBankExporter failed: {ex.Message}"); }
		try { _effectBankExporter.Export(); } catch (Exception ex) { Console.WriteLine($"EffectBankExporter failed: {ex.Message}"); }
		try { _frameExporter.Export(); } catch (Exception ex) { Console.WriteLine($"FrameExporter failed: {ex.Message}"); }
		try { _iconExporter.Export(); } catch (Exception ex) { Console.WriteLine($"IconExporter failed: {ex.Message}"); }
	}
}
