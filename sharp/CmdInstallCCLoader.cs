using System;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace CCModManager;

public class CmdInstallCCLoader : Cmd<string, string, string, IEnumerator>
{
    public override IEnumerator Run(string root, string artifactBase, string sha)
    {
        var PathOrig = Path.Combine(root, "orig");
        var PathMods = Path.Combine(root, "assets", "mods");
        var is3 = artifactBase.Contains("CCLoader3");

        // if (artifactBase.StartsWith("file://")) {
        // artifactBase = artifactBase.Substring("file://".Length);
        // yield return Status($"Unzipping {Path.GetFileName(artifactBase)}", false, "download", false);
        //
        // using (FileStream wrapStream = File.Open(artifactBase, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        // using (ZipArchive wrap = new ZipArchive(wrapStream, ZipArchiveMode.Read)) {
        //     ZipArchiveEntry zipEntry = wrap.GetEntry("nested-zip.zip");
        //     if (zipEntry == null) {
        //         yield return Unpack(wrap, root, "main/");
        //     } else {
        //         using (Stream zipStream = zipEntry.Open())
        //         using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
        //             yield return Unpack(zip, root);
        //     }
        // }

        Console.Error.WriteLine($"Root to install to: {root}");
        Console.Error.WriteLine($"File we're downloading: {artifactBase}");
        Console.Error.WriteLine($"We are installing CCLoader 3: {artifactBase.Contains("CCLoader3")}");
        Console.Error.WriteLine($"SHA that is used in the prefix of the Unpack function: {sha}");

        if (!Directory.Exists(PathOrig))
        {
            yield return Status("Creating backup orig directory", false, "backup", false);
            Directory.CreateDirectory(PathOrig);
        }

        // TODO: Currently, when going from CCLoader3 -> CCLoader2 ->
        // CCLoader3, an error occurs because it tries to move CCLoader3's mods
        // into the directory that already exists. The task at hand: make sure
        // the backed up mods directory is moved back into place when switching
        // from a major modloader version. Take into account CCLoader2's
        // default mods `crosscode-version-display` and `simplify`. They need
        // not be backed up, as they are potentially updated when a new zipball
        // of CCLoader2 is downloaded.
        if (is3 && File.Exists(Path.Combine(root, "ccloader", "package.json")))
        {
            yield return Status("CCLoader2 found!", false, "backup", false);
            var to = Path.Combine(PathOrig, "ccloader2-mods");
            if (!Directory.Exists(PathMods))
            {
                yield return Status("Somehow, CCLoader2 is installed, but the mods directory is not present.", 1f,
                    "error", false);
                throw new Exception("CCLoader2 found, but no mods dir exists!");
            }

            // Initialize our backup directory...
            Directory.CreateDirectory(Path.Combine(PathOrig, "ccloader3-mods"));

            var folders = Directory.GetDirectories(PathMods);
            var files = Directory.GetFiles(PathMods);
            // Folder mods...
            for (var i = 0; i < folders.Length; i++)
            {
                var dir = folders[i];
                yield return Status($"Backing up {folders.Length} folder mods", 0f, "backup", false);
                if (dir != "simplify" || dir != "ccloader-version-display")
                    yield return Status($"Backing up {Path.Combine(PathMods, dir)} => {Path.Combine(to, dir)}",
                        i / (float)folders.Length,
                        "backup", true);
                Directory.Move(Path.Combine(PathMods, dir), Path.Combine(to, dir));
            }

            // CCMods...
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                yield return Status($"Backing up {files.Length} CCMods", 0f, "backup", false);
                yield return Status($"Backing up {Path.Combine(PathMods, file)} => {Path.Combine(to, file)}",
                    i / (float)files.Length,
                    "backup", true);
                File.Move(Path.Combine(PathMods, file), Path.Combine(to, file));
            }

            yield return Status("Deleting leftover mods directory", false, "backup", false);
            Directory.Delete(PathMods);

            yield return Status("Removing CCLoader2", false, "backup", false);
            Directory.Delete(Path.Combine(root, "ccloader"), true);
        }

        if (!is3 && File.Exists(Path.Combine(root, "ccloader", "metadata.json")))
        {
            yield return Status("CCLoader3 was found!", false, "backup", false);
            var to = Path.Combine(PathOrig, "ccloader3-mods");
            if (!Directory.Exists(PathMods))
            {
                yield return Status("Somehow, CCLoader3 is installed, but the mods directory is not present.", 1f,
                    "error", false);
                throw new Exception("CCLoader3 found, but no mods dir exists!");
            }

            // Initialize our backup directory...
            Directory.CreateDirectory(Path.Combine(PathOrig, "ccloader2-mods"));

            var folders = Directory.GetDirectories(PathMods);
            var files = Directory.GetFiles(PathMods);
            // Folder mods...
            for (var i = 0; i < folders.Length; i++)
            {
                var dir = folders[i];
                yield return Status($"Backing up {folders.Length} folder mods", 0f, "backup", false);
                yield return Status($"Backing up {Path.Combine(PathMods, dir)} => {Path.Combine(to, dir)}",
                    i / (float)folders.Length,
                    "backup", true);
                Directory.Move(Path.Combine(PathMods, dir), Path.Combine(to, dir));
            }

            // CCMods...
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                yield return Status($"Backing up {files.Length} CCMods", 0f, "backup", false);
                yield return Status($"Backing up {Path.Combine(PathMods, file)} => {Path.Combine(to, file)}",
                    i / (float)files.Length,
                    "backup", true);
                File.Move(Path.Combine(PathMods, file), Path.Combine(to, file));
            }

            yield return Status("Deleting leftover mods directory", false, "backup", false);
            Directory.Delete(PathMods);

            yield return Status("Removing CCLoader3", false, "backup", false);
            Directory.Delete(Path.Combine(root, "ccloader"), true);
        }


        var toBackup = new[] { "package.json" };
        for (var i = 0; i < toBackup.Length; i++)
        {
            yield return Status($"Backing up {toBackup.Length} files", 0f, "backup", false);
            var from = Path.Combine(root, toBackup[i]);
            var to = Path.Combine(PathOrig, Path.GetFileName(from));
            if (!File.Exists(from) || File.Exists(to)) continue;

            yield return Status($"Backing up {from} => {to}", i / (float)toBackup.Length, "backup", true);
            File.Copy(from, to);
        }

        var loaderName = is3 ? "CCLoader3" : "CCLoader";

        yield return Status($"Downloading {loaderName}", false, "download", false);

        using var zipStream = new MemoryStream();
        yield return Download(artifactBase, 0, zipStream);

        yield return Status($"Unzipping {loaderName}", false, "download", false);
        zipStream.Seek(0, SeekOrigin.Begin);
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
        yield return Status(zip.ToString()!, false, "download", false);
        if (is3)
        {
            yield return Unpack(zip, root);
            Directory.CreateDirectory(Path.Combine(root, "assets", "mods"));
        }
        else
        {
            yield return Unpack(zip, root, $"CCDirectLink-{loaderName}-{sha}/");
        }

        if (is3 && Directory.Exists(Path.Combine(PathOrig, "ccloader3-mods")))
        {
            foreach (var dir in Directory.GetDirectories(Path.Combine(PathOrig, "ccloader3-mods")))
            {
            }

            foreach (var file in Directory.GetFiles(Path.Combine(PathOrig, "ccloader3-mods")))
            {
            }
        }

        if (!is3 && Directory.Exists(Path.Combine(PathOrig, "ccloader2-mods")))
        {
            foreach (var dir in Directory.GetDirectories(Path.Combine(PathOrig, "ccloader2-mods")))
            {
            }

            foreach (var file in Directory.GetFiles(Path.Combine(PathOrig, "ccloader2-mods")))
            {
            }
        }
    }
}