﻿using Imya.GithubIntegration;
using Imya.Models.Installation;
using Imya.Utils;

internal class GithubDevTester
{
    internal async static Task DownloadSpice()
    {
        GithubDownloader Downloader = new GithubDownloader("fuck");

        File.Delete("fuck/loader.zip");

        await Downloader.DownloadReleaseAsync(new GithubRepoInfo { Name = "Spice-it-Up", Owner = "anno-mods" }, "Spice-it-Up.zip", new ModloaderInstallation());

        var file = new FileInfo("fuck/Spice-it-Up.zip");
        Console.WriteLine($"Download Success: { file.Exists && file.Length != 0 }");
    }

    internal async static Task DownloadModloader2()
    {
        ModLoaderInstaller installation = new ModLoaderInstaller("", "imya_temp");
        await installation.InstallAsync(new ModloaderInstallation() );
    }

    internal class ModloaderInstallation : Installation
    {

    }
}