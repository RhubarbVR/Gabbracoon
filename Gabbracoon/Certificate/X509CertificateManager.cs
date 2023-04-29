using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Certificate
{
	public sealed class X509CertificateManager : IX509CertificateManager
	{

		public X509Certificate2 Certificate { get; private set; }
		public string CertificateLocation { get; set; } = "Gabbracoon.pem";
		public string Location => Path.GetFullPath(Path.Combine(".", CertificateLocation));

		public void UpdateCertificate() {
			if (!File.Exists(Location)) {
				Certificate = GenerateCertificate();
				File.WriteAllText(Location, ExportToPEM(Certificate));
				Console.WriteLine($"Made permFile {Location}");
			}
			else {
				Console.WriteLine($"Loaded permFile");
				Certificate = new X509Certificate2(Location);
			}
		}

		private static string ExportToPEM(X509Certificate cert) {
			var builder = new StringBuilder();
			builder.AppendLine("-----BEGIN CERTIFICATE-----");
			builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
			builder.AppendLine("-----END CERTIFICATE-----");
			return builder.ToString();
		}

		private static X509Certificate2 GenerateCertificate() {
			var subjectName = "Gabbracoon-Cert";
			var certRequest = new CertificateRequest($"CN={subjectName}", ECDsa.Create(), HashAlgorithmName.SHA256);
			certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
			var generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddYears(10));
			var pfxGeneratedCert = new X509Certificate2(generatedCert.Export(X509ContentType.Pfx));
			return pfxGeneratedCert;
		}
	}
}
