namespace MyCNBlog.Core.Models.Dtos
{
    public class RSAPublicKeyParametersDto
    {
        /// <summary>
        /// Base64 编码的 RSA 公钥的模
        /// </summary>
        public string Modulus { get; set; }

        /// <summary>
        /// Base64 编码的 RSA 公钥的指数
        /// </summary>
        public string Exponents { get; set; }
    }
}
