﻿using Imya.Models.ModMetadata;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Imya.Utils;
using Imya.Models.NotifyPropertyChanged;

namespace Imya.Models
{
    public class Mod : PropertyChangedNotifier
    {
        #region fields_backing
        private string _directory_name;
        private IText _name;
        private IText _category;
        private bool _active;
        private bool _selected;
        private IText _description;
        #endregion

        //Mod filepath
        public string DirectoryName { get; private set; }
        public IText Name { get; private set; }
        public IText Category { get; private set; }
        public string FullModPath => Path.Combine(BasePath, (Active ? "" : "-") + DirectoryName );
        public string BasePath { get; private set; } // TODO use ModDirectory as parent and retrieve it from there as soon as it's not a global manager anymore

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged("Active");
            }

        }
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        public IText? Description { get; private set; }
        public IText[]? KnownIssues { get; private set; }
        public String? Version { get; private set; }
        public String? CreatorName { get; private set; }
        public Dlc[]? DlcDependencies { get; private set; }
        public ImyaImageSource? Image { get; private set; }
        public String? ModID { get; private set; }
        public String[]? ModDependencies { get; private set; }
        public String[]? IncompatibleModIDs { get; private set; }

        public bool HasVersion { get => Version is String; }
        public bool HasDescription { get => Description is IText; }
        public bool HasKnownIssues { get => KnownIssues is IText[]; }
        public bool HasDlcDependencies { get => DlcDependencies is Dlc[]; }
        public bool HasCreator { get => CreatorName is String; }
        public bool HasImage { get => Image is ImyaImageSource; }

        public bool HasModID { get => ModID is String; }

        //store the Modinfo data for whatever we need it later on.
        //This should be removed, and the mods should hold all this information by themselves.

        //this should only take in the last part (i.e. "[Gameplay] AI Shipyard" of the path.)
        public Mod (bool active, String modName, Modinfo? metadata, string basePath)
        {
            //if we need to trim the start dash, the mod should become inactive.
            Active = active;
            DirectoryName = modName;
            BasePath = basePath;

            //mod with Metadata
            if (metadata is Modinfo)
            {
                ModID = metadata.ModID;
                Category = (metadata.Category is Localized) ? TextManager.CreateLocalizedText(metadata.Category) : new SimpleText("NoCategory");
                Name = (metadata.ModName is Localized) ? TextManager.CreateLocalizedText(metadata.ModName) : new SimpleText(modName);
                Description = (metadata.Description is Localized) ? TextManager.CreateLocalizedText(metadata.Description) : null;
                KnownIssues = (metadata.KnownIssues is Localized[]) ? metadata.KnownIssues.Where(x => x is Localized).Select(x => TextManager.CreateLocalizedText(x)).ToArray() : null;
                Version =  metadata.Version;
                CreatorName = metadata.CreatorName;
                DlcDependencies = metadata.DLCDependencies;

                ModDependencies = metadata.ModDependencies;
                IncompatibleModIDs = metadata.IncompatibleIds;

                //Just construct as base64 for now. 
                if (metadata.Image is String)
                {
                    Image = new ImyaImageSource();
                    Image.ConstructAsBase64Image(metadata.Image);
                }
            }
            //mod without Metadata
            else
            {
                bool matches = TryMatchToNamingPattern(DirectoryName, out var _category, out var _name);
                Category = matches ? new SimpleText(_category ) : TextManager.Instance.GetText("MODDISPLAY_NO_CATEGORY");
                Name = new SimpleText(matches ? _name : DirectoryName);
            }
        }

        public void InitImageAsFilepath(String ImagePath)
        {
            Image = new ImyaImageSource();
            Image.ConstructAsFilepathImage(ImagePath);
        }

        /// <summary>
        /// Check if mod name, category and creator fields contain all keywords.
        /// </summary>
        public bool HasKeywords(string spaceSeparatedKeywords)
        {
            var keywords = spaceSeparatedKeywords.Split(" ");
            return keywords.Aggregate(true, (isMatch, keyword) => isMatch &= HasKeyword(keyword));
        }

        private bool HasKeyword(string keyword)
        {
            var k = keyword.ToLower();
            return Name.Text.ToLower().Contains(k) ||
                Category.Text.ToLower().Contains(k) ||
                (CreatorName?.ToLower().Contains(k) ?? false);
        }

        /// <summary>
        /// Return all files with a specific extension.
        /// </summary>
        public IEnumerable<String> GetFilesWithExtension(string extension)
        {
            return Directory.EnumerateFiles(FullModPath, $"*.{extension}", SearchOption.AllDirectories);
        }

        private bool TryMatchToNamingPattern(String DirectoryName, out String Category, out String Name)
        {
            String CategoryPattern = @"[[][a-z]+[]]";
            Category = Regex.Match(DirectoryName, CategoryPattern, RegexOptions.IgnoreCase).Value.TrimStart('[').TrimEnd(']');

            String NamePattern = @"[^]]*";
            Name = Regex.Match(DirectoryName, NamePattern, RegexOptions.RightToLeft).Value.TrimStart(' ');

            return !Name.Equals("") && !Category.Equals("");
        }
    }
}
