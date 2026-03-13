using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Justine.Common.Services
{
    public class KmsEncryptionService : IEncryptionService
    {
        private readonly IAmazonKeyManagementService _kms;
        public KmsEncryptionService(IAmazonKeyManagementService kms)
        {
            _kms = kms ?? throw new ArgumentNullException(nameof(kms));
        }

        public async Task<EnvelopePackage> EnvelopeEncryptAsync(byte[] plaintext, string kmsKeyId)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(kmsKeyId)) throw new ArgumentNullException(nameof(kmsKeyId));

            // 1) Generate a data key (AES_256)
            var genResp = await _kms.GenerateDataKeyAsync(new GenerateDataKeyRequest
            {
                KeyId = kmsKeyId,
                KeySpec = "AES_256"
            }).ConfigureAwait(false);

            var plaintextKey = genResp.Plaintext.ToArray();                // AES key (clear)
            var encryptedDataKey = genResp.CiphertextBlob.ToArray();      // KMS-encrypted key

            try
            {
                // 2) Encrypt data with AES-GCM using the plaintextKey
                // Use 12-byte nonce recommended for AES-GCM
                var iv = new byte[12];
                RandomNumberGenerator.Fill(iv);

                // create ciphertext buffer and tag
                var ciphertext = new byte[plaintext.Length];
                var tag = new byte[16];

                using (var aesGcm = new AesGcm(plaintextKey))
                {
                    aesGcm.Encrypt(iv, plaintext, ciphertext, tag, null);
                }

                // Combine ciphertext + tag for transport
                var cipherPlusTag = new byte[ciphertext.Length + tag.Length];
                Buffer.BlockCopy(ciphertext, 0, cipherPlusTag, 0, ciphertext.Length);
                Buffer.BlockCopy(tag, 0, cipherPlusTag, ciphertext.Length, tag.Length);

                return new EnvelopePackage(
                    CiphertextBase64: Convert.ToBase64String(cipherPlusTag),
                    EncryptedDataKeyBase64: Convert.ToBase64String(encryptedDataKey),
                    IvBase64: Convert.ToBase64String(iv)
                );
            }
            finally
            {
                // Zero sensitive plaintext key memory
                if (plaintextKey != null) Array.Clear(plaintextKey, 0, plaintextKey.Length);
            }
        }

        public async Task<byte[]> EnvelopeDecryptAsync(EnvelopePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            var encryptedDataKey = Convert.FromBase64String(package.EncryptedDataKeyBase64);
            var iv = Convert.FromBase64String(package.IvBase64);
            var cipherPlusTag = Convert.FromBase64String(package.CiphertextBase64);

            // 1) Decrypt data key with KMS
            var decryptResp = await _kms.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new System.IO.MemoryStream(encryptedDataKey)
            }).ConfigureAwait(false);

            var plaintextKey = decryptResp.Plaintext.ToArray();
            try
            {
                // Split cipher and tag
                var tagLength = 16;
                if (cipherPlusTag.Length < tagLength) throw new CryptographicException("Ciphertext too short");
                var ciphertextLength = cipherPlusTag.Length - tagLength;
                var ciphertext = new byte[ciphertextLength];
                var tag = new byte[tagLength];

                Buffer.BlockCopy(cipherPlusTag, 0, ciphertext, 0, ciphertextLength);
                Buffer.BlockCopy(cipherPlusTag, ciphertextLength, tag, 0, tagLength);

                var plaintext = new byte[ciphertext.Length];

                using (var aesGcm = new AesGcm(plaintextKey))
                {
                    aesGcm.Decrypt(iv, ciphertext, tag, plaintext, null);
                }

                return plaintext;
            }
            finally
            {
                if (plaintextKey != null) Array.Clear(plaintextKey, 0, plaintextKey.Length);
            }
        }
    }
}