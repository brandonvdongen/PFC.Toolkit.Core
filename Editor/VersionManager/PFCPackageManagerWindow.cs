using PFC.Toolkit.Core.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFC.Toolkit.Core.VersionManager {
    public class PFCPackageManagerWindow : EditorWindow {
        //private static readonly List<VRCPackageInfo> _localPackages = new List<VRCPackageInfo>();
        private VisualElement container;
        private PopupField<PFCSettings> SettingSelector;
        private List<PFCSettings> Settings = new List<PFCSettings>();
        private PFCSettings CurrentSettings;
        private PopupField<PFCPackageManager> PackageSelector;
        private List<PFCPackageManager> Packages = new List<PFCPackageManager>();
        private PFCPackageInfo CurrentPackage;
        private PFCKeyValueList gitDependencies;
        private TextField nameField;
        private TextField displayNameField;
        private TextField versionField;
        private TextField descriptionField;
        private TextField authorName;
        private TextField authorEmail;
        private TextField authorUrl;
        private PFCKeyValueList vpmDependencies;
        private PFCKeyValueList Dependencies;
        private PFCKeyValueList LegacyFolders;
        private PFCKeyValueList Legacyfiles;
        private TextField LocalPathField;
        private TextField repoField;
        private List<string> repoTokens = new List<string>();
        private PopupField<string> repoTokenField;
        private PFCKeyValueList tokenList;

        [MenuItem("PFCToolkit/Package Manager")]
        public static void Open() {
            GetWindow<PFCPackageManagerWindow>("PFC Package Manager");
        }

        private void CreateGUI() {
            Settings = PFCSettings.GetSettingFiles();
            Packages = PFCPackageManager.GetPackages();
            container = new ScrollView();
            rootVisualElement.Add(container);
            //root.styleSheets.Add(Resources.Load<StyleSheet>("PFCPackageEditorWindow"));

            #region settingFold
            Foldout settingFold = new Foldout() { text = "Settings" };
            settingFold.value = false;
            container.Add(settingFold);

            SettingSelector = new PopupField<PFCSettings>("Settings:", Settings, 0);
            SettingSelector.RegisterValueChangedCallback(ev => {
                tokenList.SetContent(ev.newValue.Tokens);
                SetTokens(ev.newValue.Tokens);
                if (!string.IsNullOrEmpty(PackageSelector.value.PackageInfo.TokenID)) {
                    repoTokenField.SetValueWithoutNotify(PackageSelector.value.PackageInfo.TokenID);
                }
                else {
                    repoTokenField.SetValueWithoutNotify(repoTokens[0]);
                }
            });
            settingFold.Add(SettingSelector);

            #region tokenFold
            Foldout tokenFold = new Foldout() { text = "Tokens" };
            settingFold.Add(tokenFold);

            tokenList = new PFCKeyValueList();
            tokenList.SetContent(SettingSelector.value.Tokens);
            tokenList.OnChanged += () => {
                tokenList.GetContent(ref SettingSelector.value.Tokens);
                SetTokens(SettingSelector.value.Tokens);
            };
            tokenFold.Add(tokenList);
            #endregion
            #endregion

            Button btn_refresh = new Button(Refresh) { text = "Refresh" };
            container.Add(btn_refresh);

            PackageSelector = new PopupField<PFCPackageManager>("Select Package:", Packages, 0);
            PackageSelector.RegisterValueChangedCallback(ev => {
                BindNewPackage(ev.newValue);
                SetTokens(SettingSelector.value.Tokens);
                if (!string.IsNullOrEmpty(ev.newValue.PackageInfo.TokenID)) {
                    repoTokenField.SetValueWithoutNotify(ev.newValue.PackageInfo.TokenID);
                }
                else {
                    repoTokenField.SetValueWithoutNotify(repoTokens[0]);
                }
            });
            container.Add(PackageSelector);

            #region basic info
            Foldout basicInfo = new Foldout() { text = "basic info" };
            container.Add(basicInfo);

            nameField = new TextField("Name");
            basicInfo.Add(nameField);

            displayNameField = new TextField("Display Name");
            basicInfo.Add(displayNameField);

            versionField = new TextField("Version");
            basicInfo.Add(versionField);


            descriptionField = new TextField("Description");
            descriptionField.multiline = true;
            basicInfo.Add(descriptionField);

            repoField = new TextField("Repo");
            basicInfo.Add(repoField);

            SetTokens(SettingSelector.value.Tokens);
            repoTokenField = new PopupField<string>("Select Token:", repoTokens, 0);
            repoTokenField.RegisterValueChangedCallback(ev => {
                PackageSelector.value.PackageInfo.TokenID = ev.newValue == "None" ? "" : ev.newValue;
                SetTokens(SettingSelector.value.Tokens);
            });
            basicInfo.Add(repoTokenField);
            #endregion

            #region author info
            Foldout Author = new Foldout() { text = "Author" };
            container.Add(Author);

            authorName = new TextField("Name");
            Author.Add(authorName);

            authorEmail = new TextField("Email");
            Author.Add(authorEmail);

            authorUrl = new TextField("URL");
            Author.Add(authorUrl);
            #endregion

            Foldout gitFold = new Foldout() { text = "Git Dependencies" };
            container.Add(gitFold);
            gitDependencies = new PFCKeyValueList();
            gitFold.Add(gitDependencies);

            Foldout vpmFold = new Foldout() { text = "VPM Dependencies" };
            container.Add(vpmFold);
            vpmDependencies = new PFCKeyValueList();
            vpmFold.Add(vpmDependencies);

            Foldout dependenciesFold = new Foldout() { text = "Unity Dependencies" };
            container.Add(dependenciesFold);
            Dependencies = new PFCKeyValueList();
            dependenciesFold.Add(Dependencies);

            Foldout LegacyFold = new Foldout() { text = "Legacy Folders" };
            container.Add(LegacyFold);
            LegacyFolders = new PFCKeyValueList();
            LegacyFold.Add(LegacyFolders);

            Foldout LegacyFileFold = new Foldout() { text = "Legacy files" };
            container.Add(LegacyFileFold);
            Legacyfiles = new PFCKeyValueList();
            LegacyFileFold.Add(Legacyfiles);

            LocalPathField = new TextField("Local Path");
            container.Add(LocalPathField);
            Button btn_Save = new Button(Save) { text = "Save" };
            container.Add(btn_Save);

            Button btn_Export = new Button(Export) { text = "Export" };
            container.Add(btn_Export);

            Button btn_Upload = new Button(() => { Upload(); }) { text = "Upload" };
            container.Add(btn_Upload);

            SetTokens(SettingSelector.value.Tokens);
            BindNewPackage(PackageSelector.value);
        }

        private void BindNewPackage(PFCPackageManager manager) {
            CurrentPackage = manager.PackageInfo;
            nameField.value = CurrentPackage.Name;
            displayNameField.value = CurrentPackage.DisplayName;
            versionField.value = CurrentPackage.Version;
            descriptionField.value = CurrentPackage.Description;
            repoField.value = CurrentPackage.Repo;

            authorName.value = CurrentPackage.Author.name;
            authorEmail.value = CurrentPackage.Author.email;
            authorUrl.value = CurrentPackage.Author.url;

            gitDependencies.SetContent(CurrentPackage.GitDependencies);
            vpmDependencies.SetContent(CurrentPackage.VPMDependencies);
            Dependencies.SetContent(CurrentPackage.Dependencies);
            Legacyfiles.SetContent(CurrentPackage.LegacyFiles);
            LegacyFolders.SetContent(CurrentPackage.LegacyFolders);
            LocalPathField.value = CurrentPackage.LocalPath;


        }

        private void Refresh() {
            Packages = PFCPackageManager.GetPackages();
            BindNewPackage(PackageSelector.value);
        }

        private void Save() {
            PackageSelector.value.PackageInfo.Name = nameField.value;
            PackageSelector.value.PackageInfo.DisplayName = displayNameField.value;
            PackageSelector.value.PackageInfo.Version = versionField.value;
            PackageSelector.value.PackageInfo.Description = descriptionField.value;
            PackageSelector.value.PackageInfo.Author.name = authorName.value;
            PackageSelector.value.PackageInfo.Author.email = authorEmail.value;
            PackageSelector.value.PackageInfo.Author.url = authorUrl.value;
            PackageSelector.value.PackageInfo.Repo = repoField.value;
            gitDependencies.GetContent(ref PackageSelector.value.PackageInfo.GitDependencies);
            vpmDependencies.GetContent(ref PackageSelector.value.PackageInfo.VPMDependencies);
            Dependencies.GetContent(ref PackageSelector.value.PackageInfo.Dependencies);
            LegacyFolders.GetContent(ref PackageSelector.value.PackageInfo.LegacyFolders);
            Legacyfiles.GetContent(ref PackageSelector.value.PackageInfo.LegacyFiles);
            PackageSelector.value.PackageInfo.LocalPath = LocalPathField.value;


            PackageSelector.value.Save();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private void SetTokens(Dictionary<string, string> tokens) {
            repoTokens.Clear();
            repoTokens.Add("None");
            if (!string.IsNullOrEmpty(PackageSelector.value.PackageInfo.TokenID)) repoTokens.Add(PackageSelector.value.PackageInfo.TokenID);
            repoTokens.AddRange(tokens.Keys);
        }

        private void Export() {
            Save();
            PackageSelector.value.ExportPackage();
        }

        private async Task Upload() {
            Export();

            var package = PackageSelector.value.PackageInfo;

            var parts = package.Repo.Split('/');
            var owner = parts[0];
            var repo = parts[1];
            var token = SettingSelector.value.Tokens[package.TokenID];
            var tag = package.Version;

            var response = await PFCGitHubHelper.CreateGitHubReleaseWithAsset(owner, repo, token, tag, false, package.DisplayName, package.Description, $"Assets/Exports/{PackageSelector.value.ExportName}.unitypackage");
            Debug.Log(response.Content);
        }
    }
}