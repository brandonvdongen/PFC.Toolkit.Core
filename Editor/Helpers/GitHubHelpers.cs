using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PFC.Core.Helpers {
    public class GitHubHelpers {

        public static async Task<HttpResponseMessage> CreateGitHubReleaseWithAsset(string owner, string repo, string token, string tag, bool prerelease, string name, string description, string unityPackagePath) {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("https://api.github.com")
            };

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("unity", Application.unityVersion.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);

            GitReleaseData release = new GitReleaseData() {
                tag_name = tag,
                name = name,
                body = description,
                draft = false,
                prerelease = prerelease,
                generate_release_notes = false
            };

            string json = EditorJsonUtility.ToJson(release);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Make the POST request to create the release
            HttpResponseMessage response = await client.PostAsync($"/repos/{owner}/{repo}/releases", content);

            // If the POST request was successful, upload the .unitypackage file to the upload_url
            if (response.IsSuccessStatusCode) {
                // Parse the response to get the upload_url
                GitHubRelease releaseData = JsonUtility.FromJson<GitHubRelease>(await response.Content.ReadAsStringAsync());
                string uploadUrl = releaseData.upload_url;

                // Replace {?name,label} with the actual name and label for the .unitypackage file
                uploadUrl = uploadUrl.Replace("{?name,label}", $"?name={Path.GetFileName(unityPackagePath)}");

                // Read the .unitypackage file into a byte array
                byte[] unityPackageBytes = File.ReadAllBytes(unityPackagePath);

                // Create a new HttpContent object to hold the .unitypackage file
                HttpContent unityPackageContent = new ByteArrayContent(unityPackageBytes);
                unityPackageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                // Make the POST request to upload the .unitypackage file to the upload_url
                HttpResponseMessage uploadResponse = await client.PostAsync(uploadUrl, unityPackageContent);

                // Return the response from the POST request to upload the .unitypackage file
                return uploadResponse;
            }

            // If the POST request to create the release was not successful, return the response as-is
            return response;
        }

        public static async Task<HttpResponseMessage> DeleteReleaseWithTag(string owner, string repo, string token, string tag) {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("https://api.github.com")
            };

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("unity", Application.unityVersion.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);

            HttpResponseMessage response = await client.GetAsync($"/repos/{owner}/{repo}/releases/tags/{tag}");
            if (response.IsSuccessStatusCode) {
                GitHubRelease releaseData = JsonUtility.FromJson<GitHubRelease>(await response.Content.ReadAsStringAsync());
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"/repos/{owner}/{repo}/releases/{releaseData.id}");

                return deleteResponse;
            }
            return response;
        }

        public static async Task<HttpResponseMessage> GetGitHubReleaseByTag(string owner, string repo, string token, string tag) {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("https://api.github.com")
            };

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("unity", Application.unityVersion.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);

            return await client.GetAsync($"/repos/{owner}/{repo}/releases/tags/{tag}");
        }

        public static async Task<HttpResponseMessage> GetLatestGitHubRelease(string owner, string repo) {
            HttpClient client = new HttpClient {
                BaseAddress = new Uri("https://api.github.com")
            };

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("unity", Application.unityVersion.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await client.GetAsync($"/repos/{owner}/{repo}/releases/latest");
        }

        public struct GitReleaseData {
            public string tag_name;
            public string target_commitish;
            public string name;
            public string body;
            public bool draft;
            public bool prerelease;
            public bool generate_release_notes;
        }

        [Serializable]
        public struct GitHubRelease {
            public int id;
            public string tag;
            public string upload_url;
        }
    }
}