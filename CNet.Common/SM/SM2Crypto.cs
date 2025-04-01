using System;
using System.Text;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities.Encoders;
namespace CNet.Common
{
   public class SM2Crypto
    {

        private readonly X9ECParameters x9ECParameters;
        private readonly ECDomainParameters ecDomainParameters;
        private readonly SecureRandom random;

        public SM2Crypto()
        {
            // 使用SM2推荐的椭圆曲线参数
            x9ECParameters = ECNamedCurveTable.GetByName("sm2p256v1");
            ecDomainParameters = new ECDomainParameters(
                x9ECParameters.Curve,
                x9ECParameters.G,
                x9ECParameters.N,
                x9ECParameters.H);
            random = new SecureRandom();
        }

        /// <summary>
        /// 生成SM2密钥对
        /// </summary>
        /// <returns>包含公钥和私钥的元组</returns>
        public Tuple<byte[], byte[]> GenerateKeyPair()
        {
            ECKeyPairGenerator generator = new ECKeyPairGenerator();
            ECKeyGenerationParameters keyGenParams = new ECKeyGenerationParameters(ecDomainParameters, random);
            generator.Init(keyGenParams);

            AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)keyPair.Private;
            ECPublicKeyParameters publicKey = (ECPublicKeyParameters)keyPair.Public;

            return new Tuple<byte[], byte[]>(publicKey.Q.GetEncoded(), privateKey.D.ToByteArrayUnsigned());
        }

        /// <summary>
        /// SM2加密
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="data">待加密数据</param>
        /// <returns>加密后的数据</returns>
        public byte[] Encrypt(byte[] publicKey, byte[] data)
        {
            try
            {
                ECPoint publicKeyPoint = ecDomainParameters.Curve.DecodePoint(publicKey);
                ECPublicKeyParameters pubKeyParams = new ECPublicKeyParameters(publicKeyPoint, ecDomainParameters);

                SM2Engine sm2Engine = new SM2Engine(new SM3Digest());
                sm2Engine.Init(true, new ParametersWithRandom(pubKeyParams, random));

                return sm2Engine.ProcessBlock(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("SM2加密失败: " + ex.Message);
            }
        }

        /// <summary>
        /// SM2解密
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="encryptedData">加密数据</param>
        /// <returns>解密后的数据</returns>
        public byte[] Decrypt(byte[] privateKey, byte[] encryptedData)
        {
            try
            {
                BigInteger d = new BigInteger(1, privateKey);
                ECPrivateKeyParameters privKeyParams = new ECPrivateKeyParameters(d, ecDomainParameters);

                SM2Engine sm2Engine = new SM2Engine(new SM3Digest());
                sm2Engine.Init(false, privKeyParams);

                return sm2Engine.ProcessBlock(encryptedData, 0, encryptedData.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("SM2解密失败: " + ex.Message);
            }
        }


        /// <summary>
        /// SM2加密
        /// </summary>
        /// <param name="pubk"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String Encrypt(string pubk, string data)
        {
            byte[] sourceData = Encoding.Default.GetBytes(data);

            var enData= new SM2Crypto().Encrypt(Hex.Decode(pubk), sourceData);

            return Encoding.Default.GetString(enData);
        }


        /// <summary>
        /// SM2解密
        /// </summary>
        /// <param name="prik"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String Decrypt(string prik, string data)
        {
            string plainText = Encoding.Default.GetString(new SM2Crypto().Decrypt(Hex.Decode(prik), Hex.Decode(data)));

            return plainText;
        }



    }
}
