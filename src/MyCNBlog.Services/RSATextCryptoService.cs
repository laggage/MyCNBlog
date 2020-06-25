using Microsoft.Extensions.Options;
using MyCNBlog.Services.Abstractions;
using RSAUtilities;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MyCNBlog.Services
{
    public class RSATextCryptoServiceOptions
    {
        /// <summary>
        /// 证书文件路径
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// 私钥文件路径
        /// </summary>
        public string KeyFilePath { get; set; }

        /// <summary>
        /// 证书文件密码
        /// </summary>
        public string Password { get; set; }
    }

    public class RSATextCryptoService : ITextCryptoService
    {
        private readonly RSATextCryptoServiceOptions _options;

        public RSATextCryptoService(IOptions<RSATextCryptoServiceOptions> options)
        {
            _options=options?.Value;
        }

        /// <summary>
        /// 加密一段字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns>经过BASE64编码的加密内容</returns>
        public string Encrypt(string text)
        {
            RSA rsa = CreateRSA();
            byte[] data = rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1);
            rsa.Clear();
            rsa.Dispose();
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 解密一段经过BASE64编码 加密字符串
        /// </summary>
        /// <param name="text">经过BASE64编码 加密字符串</param>
        /// <returns></returns>
        public string Decrypt(string text)
        {
            RSA rsa = CreateRSA();
            string result = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(text), RSAEncryptionPadding.Pkcs1));
            rsa.Clear();
            rsa.Dispose();
            return result;
        }

        private RSA CreateRSA()
        {
            return CreateRSA(_options.CertificatePath, _options.KeyFilePath, _options.Password);
        }

        /// <summary>
        /// 创建一个RSA对象
        /// </summary>
        /// <param name="certPath">证书文件路径(必须)</param>
        /// <param name="keyFilePath">密钥文件(如果没有, 则传空或null)</param>
        /// <param name="password">证书文件读取密码(如果没有, 则传空或null)</param>
        /// <returns> RSA对象实例 <see cref="RSA"/></returns>
        public static RSA CreateRSA(string certPath, string keyFilePath, string password)
        {
            var cert = new X509Certificate2(certPath, password, X509KeyStorageFlags.Exportable);
            RSA rsa = cert.GetRSAPublicKey();

            if (!string.IsNullOrEmpty(keyFilePath))
                rsa.LoadPrivateKeyFromFile(keyFilePath);
            else
            {
                RSA pri = cert.GetRSAPrivateKey();
                rsa.ImportRSAPrivateKey(pri.ExportRSAPrivateKey(), out int _);
                pri.Clear();
                pri.Dispose();
            }
            return rsa;
        }

        public RSAParameters GetRSAParameters()
        {
            RSAParameters parmeters;
            using (RSA rsa =  CreateRSA())
            {
                parmeters = rsa.ExportParameters(false);
                rsa.Clear();
            }

            return parmeters;
        }
    }
}
