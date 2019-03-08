using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Jose;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace NHS.Login.Dotnet.Core.Sample
{
    public static class TokenHelper
    {
        public static string CreateClientAuthJwt()
        {
            var payload = new Dictionary<string, object>()
            {
                {"sub", "YOUR-CLIENT-ID"},
                {"aud", "https://auth.sandpit.signin.nhs.uk/token"},
                {"iss", "YOUR-CLIENT-ID"},
                {"exp", DateTimeOffset.Now.AddMinutes(60).ToUnixTimeSeconds() },
                {"jti", Guid.NewGuid()}
            };

            using (var reader = File.OpenText("private_key.pem"))
            using (var rsa = new RSACryptoServiceProvider())
            {
                var key = (RsaPrivateCrtKeyParameters) new PemReader(reader)
                    .ReadObject();
                var rsaParams = DotNetUtilities.ToRSAParameters(key);

                rsa.ImportParameters(rsaParams);

                return JWT.Encode(payload, rsa, JwsAlgorithm.RS512);
            }
        }
    }
}