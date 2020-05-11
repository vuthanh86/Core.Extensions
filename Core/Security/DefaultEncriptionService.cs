using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Core.Security
{
    public class EncryptionSettings
    {
        public string EncryptionAlgorithm { get; set; }
        public string EncryptionKey { get; set; }
        public string HashAlgorithm { get; set; }
        public string HashKey { get; set; }
    }

    public class DefaultEncryptionService : IEncryptionService
    {
        private readonly EncryptionSettings encryptionSettings;

        public DefaultEncryptionService(IOptions<EncryptionSettings> encryptionSettings)
        {
            this.encryptionSettings = encryptionSettings.Value;
        }

        public byte[] Decode(byte[] encodedData)
        {
            // extract parts of the encoded data
            using (var symmetricAlgorithm = CreateSymmetricAlgorithm())
            {
                using (var hashAlgorithm = CreateHashAlgorithm())
                {
                    var iv = new byte[symmetricAlgorithm.BlockSize / 8];
                    var signature = new byte[hashAlgorithm.HashSize / 8];
                    var data = new byte[encodedData.Length - iv.Length - signature.Length];

                    Array.Copy(encodedData, 0, iv, 0, iv.Length);
                    Array.Copy(encodedData, iv.Length, data, 0, data.Length);
                    Array.Copy(encodedData, iv.Length + data.Length, signature, 0, signature.Length);

                    // validate the signature
                    var mac = hashAlgorithm.ComputeHash(iv.Concat(data).ToArray());

                    if (!mac.SequenceEqual(signature))
                    {
                        // message has been tampered
                        throw new ArgumentException();
                    }

                    symmetricAlgorithm.IV = iv;

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public string Decode(string encodedData)
        {
            if (string.IsNullOrEmpty(encodedData))
            {
                return encodedData;
            }

            return Encoding.UTF8.GetString(Decode(Convert.FromBase64String(encodedData)));
        }

        public byte[] Encode(byte[] data)
        {
            // cipherText ::= IV || ENC(EncryptionKey, IV, plainText) || HMAC(SigningKey, IV || ENC(EncryptionKey, IV, plainText))

            byte[] encryptedData;
            byte[] iv;

            using (var ms = new MemoryStream())
            {
                using (var symmetricAlgorithm = CreateSymmetricAlgorithm())
                {
                    // generate a new IV each time the Encode is called
                    symmetricAlgorithm.GenerateIV();
                    iv = symmetricAlgorithm.IV;

                    using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptedData = ms.ToArray();
                }
            }

            byte[] signedData;

            // signing IV || encrypted data
            using (var hashAlgorithm = CreateHashAlgorithm())
            {
                signedData = hashAlgorithm.ComputeHash(iv.Concat(encryptedData).ToArray());
            }

            return iv.Concat(encryptedData).Concat(signedData).ToArray();
        }

        public string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(data)));
        }

        private SymmetricAlgorithm CreateSymmetricAlgorithm()
        {
            var algorithm = Aes.Create();
            algorithm.Key = ToByteArray(encryptionSettings.EncryptionKey);
            return algorithm;
        }

        private HMAC CreateHashAlgorithm()
        {
            var algorithm = new HMACSHA256();
            algorithm.Key = ToByteArray(encryptionSettings.HashKey);
            return algorithm;
        }

        private static byte[] ToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x % 2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }
    }
}
