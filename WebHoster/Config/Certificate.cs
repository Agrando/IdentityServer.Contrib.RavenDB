using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace WebHost.Config
{
    static class Certificate
    {
        public static X509Certificate2 Get()
        {
            return GetCertByThumbPrint("B64F6846702990C960B81CA0562EB9D19AFDB755");
        }

        private static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private static X509Certificate2 GetCertByThumbPrint(string th)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var c = store.Certificates.Find(X509FindType.FindByThumbprint, th, false);
            store.Close();

            if (c.Count > 0)
                return c[0];

            store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            c = store.Certificates.Find(X509FindType.FindByThumbprint, th, false);
            store.Close();

            if (c.Count > 0)
                return c[0];

            return null;
        }
    }
}