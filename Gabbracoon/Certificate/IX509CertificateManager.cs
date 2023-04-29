using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Certificate
{
	public interface IX509CertificateManager
	{
		public string CertificateLocation { get; set; }
		public X509Certificate2 Certificate { get; }
		public void UpdateCertificate();
	}
}
