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

        if (is3 && File.Exists(Path.Combine(root, "ccloader", "package.json")))
        {
            yield return Status("CCLoader2 found!", false, "backup", false);
            var from = Path.Combine(root, "assets", "mods");
            var to = Path.Combine(PathOrig, "ccloader2-mods", Path.GetFileName(from));
            if (!Directory.Exists(from))
            {
                yield return Status("Somehow, CCLoader is installed, but the mods directory is not present.", 1f,
                    "error", false);
                throw new Exception("CCLoader found, but no mods dir exists!");
            }

            yield return Status($"Moving mods directory {from} => {to}", false, "backup", false);
            Directory.CreateDirectory(Path.Combine(PathOrig, "ccloader2-mods"));
            Directory.Move(from, to);

            yield return Status("Removing CCLoader2", false, "backup", false);
            Directory.Delete(Path.Combine(root, "ccloader"), true);
        }

        // if (!is3 && File.Exists(Path.Combine(root, "ccloader", "metadata.json")))
        // {
        // 	yield return Status("CCLoader3 was found!", false, "backup", false);
        // }


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
    }
}