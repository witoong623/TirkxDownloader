using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Models
{
    public class AuthorizationStore
    {
        public AuthorizationInfo GetCredential(string target)
        {
            var credential = new Credential { Target = target };

            if (!credential.Load())
            {
                return null;
            }

            return new AuthorizationInfo
            {
                Username = credential.Username,
                Password = credential.Password,
                TargetName = credential.Target
            };
        }

        public IList<AuthorizationInfo> GetAllCredential()
        {
            var domains = new List<string>();
            var credentials = new List<AuthorizationInfo>();

            if (!File.Exists("Target domain"))
            {
                return credentials;
            }

            using (var str = new StreamReader("Target domain", Encoding.UTF8))
            {
                string line;

                while ((line = str.ReadLine()) != null)
                {
                    domains.Add(line);
                }
            }

            
            AuthorizationInfo authorization;

            foreach (var domain in domains)
            {
                if ((authorization = GetCredential(domain)) != null)
                {
                    credentials.Add(authorization);
                }
            }

            return credentials;
        }

        public bool SaveCredential(string target, string username, string password)
        {
            using (var stw = new StreamWriter("Target domain", true, Encoding.UTF8))
            {
                stw.WriteLine(target);
            }

            var credential = new Credential
            {
                Target = target,
                PersistanceType = PersistanceType.LocalComputer,
                Username = username,
                Password = password
            };

            return credential.Save();
        }

        public bool DeleteCredential(string target)
        {
            var credential = new Credential { Target = target };

            return credential.Delete();
        }
    }
}
