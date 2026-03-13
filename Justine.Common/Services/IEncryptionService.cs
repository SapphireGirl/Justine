using System.Threading.Tasks;

namespace Justine.Common.Services
{
    public record EnvelopePackage(string CiphertextBase64, string EncryptedDataKeyBase64, string IvBase64);

    public interface IEncryptionService
    {
        /// <summary>
        /// Envelope-encrypt plaintext bytes using KMS-generated data key (AES-256) and return a package (ciphertext + kms-encrypted data key + iv).
        /// </summary>
        Task<EnvelopePackage> EnvelopeEncryptAsync(byte[] plaintext, string kmsKeyId);

        /// <summary>
        /// Decrypt an EnvelopePackage produced by EnvelopeEncryptAsync.
        /// </summary>
        Task<byte[]> EnvelopeDecryptAsync(EnvelopePackage package);
    }
}