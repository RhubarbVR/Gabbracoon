using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Gabbracoon.Certificate
{
	public sealed class X509CertificateManager : IX509CertificateManager
	{
		private static string KeyPassword => Environment.GetEnvironmentVariable("CERT_KEY_PASSWORD") ?? "ChangeME";
		public X509Certificate2 Certificate { get; private set; }
		public string CertificateLocation { get; set; } = "Gabbracoon.pem";
		public string PrivateKeyLocation { get; set; } = "Gabbracoon.key";
		public string Location => Path.GetFullPath(Path.Combine(".", CertificateLocation));
		public string KeyLocation => Path.GetFullPath(Path.Combine(".", PrivateKeyLocation));

		public void UpdateCertificate() {
			if(KeyPassword == "ChangeME") {
				Console.WriteLine("Change CERT_KEY_PASSWORD Environment var");
			}
			if (!File.Exists(Location) || !File.Exists(KeyLocation)) {
				GenerateCertificate();
			}
			Console.WriteLine($"Loaded certFile and keyFile");
			Certificate = new X509Certificate2(File.ReadAllBytes(Location), KeyPassword);
			var rsaKey = RSA.Create();
			rsaKey.ImportEncryptedPkcs8PrivateKey(KeyPassword, File.ReadAllBytes(PrivateKeyLocation), out _);
			Certificate = Certificate.CopyWithPrivateKey(rsaKey);
		}

		private void GenerateCertificate() {
			var subjectName = "Gabbracoon-Cert";
			var certRequest = new CertificateRequest($"CN={subjectName}", RSA.Create(4096), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
			certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
			var generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddYears(10));
			File.WriteAllBytes(PrivateKeyLocation, generatedCert.GetRSAPrivateKey().ExportEncryptedPkcs8PrivateKey(KeyPassword, new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 100000)));
			File.WriteAllBytes(CertificateLocation, generatedCert.Export(X509ContentType.Cert, KeyPassword));
			Console.WriteLine($"Made certFile and keyFile");
			generatedCert.Dispose();
		}
	}
}