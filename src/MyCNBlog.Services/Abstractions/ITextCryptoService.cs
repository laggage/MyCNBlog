using System.Security.Cryptography;

namespace MyCNBlog.Services.Abstractions
{
    public interface ITextCryptoService
    {
        /// <summary>
        /// 加密一段字符串
        /// </summary>
        /// <param name="text">需要被加密的字符串</param>
        /// <returns>加密后Base64编码的字符串</returns>
        string Encrypt(string text);

        /// <summary>
        /// 解密一段字符串
        /// </summary>
        /// <param name="text">需要被解密的Base64编码的字符串</param>
        /// <returns>解密后的字符串</returns>
        string Decrypt(string text);

        /// <summary>
        /// 返回RSA公钥参数, 即 Modulus(模) 和 Exponents(指数)
        /// </summary>
        /// <returns></returns>
        RSAParameters GetRSAParameters();
    }
}
