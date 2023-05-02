using System;
using System.IO;
using System.Security.Cryptography;

namespace PFC.Toolkit.Core.Helpers {
    public class PFCHashHelper {
        public static string CalculateSHA256(string filePath) {
            using (SHA256 sha256 = SHA256.Create()) {
                using (FileStream stream = File.OpenRead(filePath)) {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}