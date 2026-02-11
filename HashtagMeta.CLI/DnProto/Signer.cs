using System.Numerics;
using System.Security.Cryptography;

namespace HashtagMeta.CLI.DnProto;

/// <summary>
/// Signs and validates JWT tokens using RSA or ECDSA cryptographic keys.
/// </summary>
public static class Signer {
    /// <summary>
    /// Validates an incoming byte array.
    /// Caller needs to look up user's public key in did doc.
    /// </summary>
    /// <returns>true if valid, false if validation fails</returns>
    public static bool ValidateHash(string issuerPublicKey, byte[] hash, byte[] signature) {
        try {
            // Determine format: multibase (starts with 'z'), PEM (contains -----BEGIN), or hex
            if (issuerPublicKey.StartsWith('z')) {
                // Multibase format (from DID document)
                var publicKeyBytes = Base58BtcEncoding.DecodeMultibase(issuerPublicKey);

                // Remove multicodec prefix (first 2 bytes for P-256: 0x80 0x24)
                byte[] keyBytes;
                if (publicKeyBytes.Length > 2 && publicKeyBytes[0] == 0x80 && publicKeyBytes[1] == 0x24) {
                    keyBytes = [.. publicKeyBytes.Skip(2)];
                } else {
                    keyBytes = publicKeyBytes;
                }

                var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

                // Handle compressed or uncompressed public key
                ECPoint publicPoint;
                if (keyBytes.Length == 33 && (keyBytes[0] == 0x02 || keyBytes[0] == 0x03)) {
                    var x = keyBytes.Skip(1).Take(32).ToArray();
                    var y = DecompressPublicKey(x, keyBytes[0] == 0x03);
                    publicPoint = new ECPoint { X = x, Y = y };
                } else if (keyBytes.Length == 65 && keyBytes[0] == 0x04) {
                    publicPoint = new ECPoint {
                        X = [.. keyBytes.Skip(1).Take(32)],
                        Y = [.. keyBytes.Skip(33).Take(32)]
                    };
                } else {
                    throw new InvalidOperationException($"Invalid public key format. Expected 33 or 65 bytes, got {keyBytes.Length}.");
                }

                var parameters = new ECParameters {
                    Curve = ECCurve.NamedCurves.nistP256,
                    Q = publicPoint
                };

                ecdsa.ImportParameters(parameters);
                return ecdsa.VerifyHash(hash, signature);
            } else {
                // Hex format - assume ECDSA P-256
                var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                var publicKeyBytes = Convert.FromHexString(issuerPublicKey);

                // Handle compressed or uncompressed public key
                ECPoint publicPoint;
                if (publicKeyBytes.Length == 33 && (publicKeyBytes[0] == 0x02 || publicKeyBytes[0] == 0x03)) {
                    var x = publicKeyBytes.Skip(1).Take(32).ToArray();
                    var y = DecompressPublicKey(x, publicKeyBytes[0] == 0x03);
                    publicPoint = new ECPoint { X = x, Y = y };
                } else if (publicKeyBytes.Length == 65 && publicKeyBytes[0] == 0x04) {
                    publicPoint = new ECPoint {
                        X = [.. publicKeyBytes.Skip(1).Take(32)],
                        Y = [.. publicKeyBytes.Skip(33).Take(32)]
                    };
                } else {
                    throw new InvalidOperationException("Invalid public key format.");
                }

                var parameters = new ECParameters {
                    Curve = ECCurve.NamedCurves.nistP256,
                    Q = publicPoint
                };

                ecdsa.ImportParameters(parameters);
                return ecdsa.VerifyHash(hash, signature);
            }
        } catch (Exception ex) {
            Console.WriteLine($"JWT validation failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Decompresses a compressed P-256 public key by computing Y from X using the curve equation.
    /// </summary>
    private static byte[] DecompressPublicKey(byte[] x, bool isOdd) {
        // P-256 curve parameters
        var p = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
        var a = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853948");
        var b = BigInteger.Parse("41058363725152142129326129780047268409114441015993725554835256314039467401291");

        // Convert X to BigInteger
        var xInt = new BigInteger(x, isUnsigned: true, isBigEndian: true);

        // Compute Y^2 = X^3 + aX + b (mod p)
        var ySquared = (BigInteger.ModPow(xInt, 3, p) + a * xInt + b) % p;

        // Compute Y = sqrt(Y^2) mod p using Tonelli-Shanks
        var y = ModSqrt(ySquared, p);

        // Choose the correct root based on the compression flag
        var yIsOdd = !y.IsEven;
        if (yIsOdd != isOdd) {
            y = p - y;
        }

        // Convert back to 32-byte array
        var yBytes = y.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (yBytes.Length < 32) {
            var padded = new byte[32];
            Array.Copy(yBytes, 0, padded, 32 - yBytes.Length, yBytes.Length);
            return padded;
        }
        return [.. yBytes.Take(32)];
    }

    /// <summary>
    /// Computes modular square root using Tonelli-Shanks algorithm for P-256 prime.
    /// </summary>
    private static BigInteger ModSqrt(BigInteger a, BigInteger p) {
        // For P-256, p ≡ 3 (mod 4), so we can use the simple formula: sqrt(a) = a^((p+1)/4) mod p
        return BigInteger.ModPow(a, (p + 1) / 4, p);
    }

    /// <summary>
    /// Creates a signing function for repository commits from a private key in multibase format.
    /// This function can be used with MstRepository.CreateForNewUser() and MstRepository.Commit().
    /// </summary>
    /// <param name="privateKeyMultibase">The private key in multibase format (starts with 'z')</param>
    /// <param name="publicKeyMultibase">The public key in multibase format (optional, used for validation)</param>
    /// <returns>A signing function that takes a byte array (hash) and returns a signature</returns>
    /// <exception cref="ArgumentException">Thrown when the key format is invalid</exception>
    /// <example>
    /// Usage with PDS config:
    /// <code>
    /// var config = db.GetConfig();
    /// var signingFunction = Signer.CreateCommitSigningFunction(config.UserPrivateKeyMultibase, config.UserPublicKeyMultibase);
    /// var repo = MstRepository.CreateForNewUser(config.UserDid, signingFunction);
    /// </code>
    /// </example>
    public static Func<byte[], byte[]> CreateCommitSigningFunction(string privateKeyMultibase, string? publicKeyMultibase = null) {
        // Decode the multibase private key
        var privateKeyWithPrefix = Base58BtcEncoding.DecodeMultibase(privateKeyMultibase);

        // Determine key type from multicodec prefix
        byte[]? privateKeyBytes = null;
        byte[]? publicKeyBytes = null;
        ECCurve curve;

        if (privateKeyWithPrefix.Length >= 2) {
            // Check for P-256 private key prefix (0x86 0x26)
            if (privateKeyWithPrefix[0] == 0x86 && privateKeyWithPrefix[1] == 0x26) {
                // P-256 key
                privateKeyBytes = [.. privateKeyWithPrefix.Skip(2)];
                curve = ECCurve.NamedCurves.nistP256;

                // Decode public key if provided (public key uses 0x80 0x24)
                if (!string.IsNullOrEmpty(publicKeyMultibase)) {
                    var publicKeyWithPrefix = Base58BtcEncoding.DecodeMultibase(publicKeyMultibase);
                    if (publicKeyWithPrefix.Length >= 2 && publicKeyWithPrefix[0] == 0x80 && publicKeyWithPrefix[1] == 0x24) {
                        publicKeyBytes = [.. publicKeyWithPrefix.Skip(2)];
                    }
                }
            }
            // Check for secp256k1 private key prefix (0x81 0x26)
            else if (privateKeyWithPrefix[0] == 0x81 && privateKeyWithPrefix[1] == 0x26) {
                // secp256k1 key
                privateKeyBytes = [.. privateKeyWithPrefix.Skip(2)];
                curve = ECCurve.CreateFromFriendlyName("secp256k1");

                // Decode public key if provided (public key uses 0xE7 0x01)
                if (!string.IsNullOrEmpty(publicKeyMultibase)) {
                    var publicKeyWithPrefix = Base58BtcEncoding.DecodeMultibase(publicKeyMultibase);
                    if (publicKeyWithPrefix.Length >= 2 && publicKeyWithPrefix[0] == 0xE7 && publicKeyWithPrefix[1] == 0x01) {
                        publicKeyBytes = [.. publicKeyWithPrefix.Skip(2)];
                    }
                }
            } else {
                throw new ArgumentException($"Unsupported key type. Expected P-256 private (0x86 0x26) or secp256k1 private (0x81 0x26) prefix, got 0x{privateKeyWithPrefix[0]:X2} 0x{privateKeyWithPrefix[1]:X2}");
            }
        } else {
            throw new ArgumentException("Invalid private key format. Key too short to contain multicodec prefix.");
        }

        if (privateKeyBytes == null || privateKeyBytes.Length != 32) {
            throw new ArgumentException($"Invalid private key length. Expected 32 bytes, got {privateKeyBytes?.Length ?? 0}");
        }

        // Create the ECDSA instance and import parameters
        var ecdsa = ECDsa.Create(curve);

        ECParameters parameters;
        if (publicKeyBytes != null) {
            // Import with both private and public key
            ECPoint publicPoint;

            // Handle compressed or uncompressed public key
            if (publicKeyBytes.Length == 33 && (publicKeyBytes[0] == 0x02 || publicKeyBytes[0] == 0x03)) {
                // Compressed format
                var x = publicKeyBytes.Skip(1).Take(32).ToArray();
                var y = DecompressPublicKey(x, publicKeyBytes[0] == 0x03);
                publicPoint = new ECPoint { X = x, Y = y };
            } else if (publicKeyBytes.Length == 65 && publicKeyBytes[0] == 0x04) {
                // Uncompressed format
                publicPoint = new ECPoint {
                    X = [.. publicKeyBytes.Skip(1).Take(32)],
                    Y = [.. publicKeyBytes.Skip(33).Take(32)]
                };
            } else {
                throw new ArgumentException($"Invalid public key format. Expected 33 or 65 bytes, got {publicKeyBytes.Length}");
            }

            parameters = new ECParameters {
                Curve = curve,
                D = privateKeyBytes,
                Q = publicPoint
            };
        } else {
            // Import with private key only (will compute public key)
            parameters = new ECParameters {
                Curve = curve,
                D = privateKeyBytes
            };
        }

        ecdsa.ImportParameters(parameters);

        // Return the signing function
        return (hash) => {
            // Sign the hash - IMPORTANT: Use DSASignatureFormat.IeeeP1363Format for cross-platform compatibility
            // This ensures we get raw r || s (64 bytes) instead of DER encoding
            var signature = ecdsa.SignHash(hash, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

            // Normalize to low-S form (required by the relay)
            return NormalizeLowS(signature, curve);
        };
    }

    /// <summary>
    /// Creates a signing function for repository commits from a KeyPair object.
    /// This is a convenience wrapper around CreateCommitSigningFunction.
    /// </summary>
    /// <param name="keyPair">The KeyPair object containing the private key</param>
    /// <returns>A signing function that takes a byte array (hash) and returns a signature</returns>
    public static Func<byte[], byte[]> CreateCommitSigningFunction(KeyPair keyPair) {
        return CreateCommitSigningFunction(keyPair.PrivateKeyMultibase, keyPair.PublicKeyMultibase);
    }

    /// <summary>
    /// Normalizes an ECDSA signature to use low-S value (BIP-62 compliance).
    /// This prevents signature malleability issues.
    /// </summary>
    /// <param name="signature">The IEEE P1363 format signature (r || s, 64 bytes for P-256/secp256k1)</param>
    /// <param name="curve">The elliptic curve used</param>
    /// <returns>Normalized signature with low-S value</returns>
    private static byte[] NormalizeLowS(byte[] signature, ECCurve curve) {
        if (signature.Length != 64)
            return signature; // Only handle 64-byte signatures (P-256 and secp256k1)

        // Extract r and s components (32 bytes each)
        var r = signature.Take(32).ToArray();
        var s = signature.Skip(32).Take(32).ToArray();

        // Get the curve order based on the curve type
        // P-256 order: FFFFFFFF 00000000 FFFFFFFF FFFFFFFF BCE6FAAD A7179E84 F3B9CAC2 FC632551
        // secp256k1 order: FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE BAAEDCE6 AF48A03B BFD25E8C D0364141
        // Note: Prepend "0" to ensure BigInteger.Parse treats these as positive numbers
        string orderHex;
        if (curve.Oid?.FriendlyName == "secp256k1" || curve.Oid?.Value == "1.3.132.0.10") {
            orderHex = "0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141";
        } else {
            // Default to P-256
            orderHex = "0FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551";
        }
        var order = BigInteger.Parse(orderHex, System.Globalization.NumberStyles.HexNumber);

        // Convert s to BigInteger (big-endian)
        var sBigInt = new BigInteger(s.Reverse().Concat(new byte[] { 0 }).ToArray());

        // Calculate order / 2
        var halfOrder = order / 2;

        // If s > order/2, normalize it: s = order - s
        if (sBigInt > halfOrder) {
            var normalizedS = order - sBigInt;
            var normalizedSBytes = normalizedS.ToByteArray();

            // Remove leading zeros and sign byte, reverse to big-endian
            var trimmedBytes = normalizedSBytes.Reverse().SkipWhile(b => b == 0).ToArray();

            // Ensure we have exactly 32 bytes (pad with leading zeros if needed)
            var paddedS = new byte[32];
            if (trimmedBytes.Length <= 32) {
                Array.Copy(trimmedBytes, 0, paddedS, 32 - trimmedBytes.Length, trimmedBytes.Length);
            } else {
                // Take last 32 bytes if somehow larger
                Array.Copy(trimmedBytes, trimmedBytes.Length - 32, paddedS, 0, 32);
            }

            // Combine r and normalized s
            var result = new byte[64];
            Array.Copy(r, 0, result, 0, 32);
            Array.Copy(paddedS, 0, result, 32, 32);

            return result;
        }

        return signature; // Already normalized
    }
}


