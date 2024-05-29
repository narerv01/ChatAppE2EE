using System.Security.Cryptography;
using System.Text;

namespace BlazorServer.Security;

public class DiffieHellmanKeyExchange : IDisposable
{
    private Aes aes = null;
    private ECDiffieHellmanCng diffieHellman = null;

    private readonly byte[] publicKey;

    public DiffieHellmanKeyExchange()
    {
        this.aes = new AesCryptoServiceProvider();

        this.diffieHellman = new ECDiffieHellmanCng
        {
            KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
            HashAlgorithm = CngAlgorithm.Sha256
        };

        // This is the public key we will send to the other party
        this.publicKey = this.diffieHellman.PublicKey.ToByteArray();
    }

    public byte[] PublicKey
    {
        get
        {
            return this.publicKey;
        }
    }

    public byte[] IV
    {
        get
        {
            return this.aes.IV;
        }
    }

    public byte[] Encrypt(byte[] publicKey, string secretMessage)
    {
        byte[] encryptedMessage;
        var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
        var derivedKey = this.diffieHellman.DeriveKeyMaterial(key); // "Common secret"

        this.aes.Key = derivedKey;

        // Generate a new IV for each encryption operation
        this.aes.GenerateIV();

        using (var cipherText = new MemoryStream())
        {
            using (var encryptor = this.aes.CreateEncryptor())
            {
                using (var cryptoStream = new CryptoStream(cipherText, encryptor, CryptoStreamMode.Write))
                {
                    byte[] ciphertextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cryptoStream.Write(ciphertextMessage, 0, ciphertextMessage.Length);
                }
            }

            encryptedMessage = cipherText.ToArray();
        }

        return encryptedMessage;
    }

    public string Decrypt(byte[] publicKey, byte[] encryptedMessage, byte[] iv)
    {
        string decryptedMessage;
        var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
        var derivedKey = this.diffieHellman.DeriveKeyMaterial(key);

        this.aes.Key = derivedKey;
        this.aes.IV = iv;

        using (var plainText = new MemoryStream())
        {
            using (var decryptor = this.aes.CreateDecryptor())
            {
                using (var cryptoStream = new CryptoStream(plainText, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedMessage, 0, encryptedMessage.Length);
                }
            }

            decryptedMessage = Encoding.UTF8.GetString(plainText.ToArray());
        }

        return decryptedMessage;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.aes != null)
                this.aes.Dispose();

            if (this.diffieHellman != null)
                this.diffieHellman.Dispose();
        }
    }

}
