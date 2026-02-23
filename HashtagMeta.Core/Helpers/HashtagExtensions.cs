using HashtagMeta.Core.DnProto;
using HashtagMeta.Core.Models;

namespace HashtagMeta.Core.Helpers {
    public static class HashtagExtensions {
        public static byte[] GetSignature(this HashtagData hashtagData, string privateKey, string publicKey) {
            try {
                var signingFunction = Signer.CreateCommitSigningFunction(privateKey, publicKey);

                if (hashtagData != null) {
                    var hash = hashtagData.CalculateDagCborHash();
                    var signedBytes = signingFunction(hash);

                    Console.WriteLine($"Hashtag Data hash: {Convert.ToBase64String(hash)}");
                    Console.WriteLine($"Base64: {Convert.ToBase64String(signedBytes)}");
                    Console.WriteLine($"Base32: {Base32Encoding.BytesToBase32(signedBytes)}");

                    return signedBytes;
                }
                return [];
            } catch (Exception ex) {
                Console.WriteLine($"Error signing HashtagData object:");
                Console.WriteLine(ex.ToString());
                return [];
            }
        }

        /// <summary>
        /// Signs the hashtag meta json object and saves the signature in the 'sig' property
        /// </summary>
        /// <param name="hashtagJson"></param>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        public static void Sign(this HashtagMetaJson hashtagJson, string privateKey, string publicKey) {
            try {
                if (hashtagJson?.Data != null) {
                    var signature = hashtagJson.Data.GetSignature(privateKey, publicKey);
                    hashtagJson.Signature = signature;
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error signing HashtagMetaJson:");
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Validate the signature for the 'data' element using the public key
        /// </summary>
        /// <param name="hashtagJson"></param>
        /// <param name="publicKey"></param>
        /// <returns>True if the signature matches the hash</returns>
        public static bool ValidateSignature(this HashtagMetaJson hashtagJson, string publicKey) {
            try {
                if (hashtagJson?.Data != null) {
                    var hash = hashtagJson.Data.CalculateDagCborHash();
                    var valid = Signer.ValidateHash(publicKey, hash, hashtagJson.Signature);
                    return valid;
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error validating HashtagMetaJson:");
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Validate the hashtag metadata file using the public key
        /// and the files in the working folder
        /// </summary>
        /// <param name="htData"></param>
        /// <param name="publicKey"></param>
        /// <param name="errors">Output a list of errors</param>
        /// <returns>True if validation succeeds</returns>
        public static bool ValidateAll(
            this HashtagMetaJson htData,
            string publicKey,
            out IList<string> errors,
            string? workingFolder = null
        ) {
            errors = [];
            if (!htData.ValidateSignature(publicKey)) {
                errors.Add("The Hashtag data hash cannot be verified with the provided public key");
            }

            if (htData.Data.Source != null) {
                var sourceCid = HashtagFunctions.CreateCID(htData.Data.Source);

                if (sourceCid != htData.Data.SourceCID) {
                    errors.Add($"Source: calculated CID does not match source CID in metadata.");
                }
            }

            foreach (var f in htData.Data.Files) {
                try {
                    var fi = new FileInfo(Path.Combine(workingFolder ?? "", f.Key));
                    if (fi.Exists) {
                        var cid = HashtagFunctions.CreateCID(fi);

                        if (cid != f.Value.FileCID) {
                            errors.Add($"File {fi.FullName}: calculated CID does not match file CID in metadata.");
                        }
                    } else {
                        errors.Add($"File {fi.FullName} does not exist.");
                    }
                } catch (Exception ex) {
                    errors.Add($"File {f.Key}: exception verifying file CID:");
                    errors.Add(ex.Message);
                }
            }
            return errors.Count == 0;
        }
    }
}