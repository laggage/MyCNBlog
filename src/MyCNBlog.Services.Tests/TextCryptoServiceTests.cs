using Microsoft.Extensions.DependencyInjection;
using MyCNBlog.Services.Abstractions;
using MyCNBlog.Services.Extensions;
using System;
using System.Security.Cryptography;
using Xunit;

namespace MyCNBlog.Services.Tests
{
    public class TextCryptoServiceTests
    {
        [Theory]
        [InlineData("我是陈畏民")]
        [InlineData("你好, 我是和同学")]
        [InlineData("Hello, call me WeiMin")]
        public void CryptoTest(string text)
        {
            var services = new ServiceCollection();
            services.AddTextCryptoService()
                .Configure(config =>
                {
                    config.CertificatePath = @"F:\Laggage\Documents\cert\www.laggage.top.crt";
                config.KeyFilePath = @"F:\Laggage\Documents\cert\www.laggage.top.key";
                });
            
            ITextCryptoService service = services.BuildServiceProvider()
                .GetRequiredService<ITextCryptoService>();

            string excrypted = service.Encrypt(text);
            string descrypted = service.Decrypt(excrypted);
            Assert.Equal(text, descrypted);
        }

        [Theory]
        [InlineData("我是陈畏民")]
        [InlineData("你好, 我是和同学")]
        [InlineData("Hello, call me WeiMin")]
        public void PfxCryptoTest(string text)
        {
            var services = new ServiceCollection();
            services.AddTextCryptoService()
                .Configure(config =>
                {
                    config.CertificatePath = @"F:\Laggage\Documents\cert\MyCa.pfx";
                    config.Password = "123456789";
                });

            ITextCryptoService service = services.BuildServiceProvider()
                .GetRequiredService<ITextCryptoService>();
           
            string excrypted = service.Encrypt(text);
            string descrypted = service.Decrypt(excrypted);
            Assert.Equal(text, descrypted);
        }

        [Fact]
        public void GetRSAParametersTest()
        {
            var services = new ServiceCollection();
            services.AddTextCryptoService()
                .Configure(config =>
                {
                    config.CertificatePath = @"F:\Laggage\Documents\cert\MyCa.pfx";
                    config.Password = "123456789";
                });

            ITextCryptoService service = services.BuildServiceProvider()
                .GetRequiredService<ITextCryptoService>();

            RSAParameters parameters = service.GetRSAParameters();
            Assert.NotEmpty(parameters.Modulus);
            Assert.NotEmpty(parameters.Exponent);
            Assert.Null(parameters.D);
            Assert.Null(parameters.P);
            Assert.Null(parameters.Q);
            Assert.Null(parameters.DP);
            Assert.Null(parameters.DQ);
            Assert.Null(parameters.InverseQ);
        }
    }
}
