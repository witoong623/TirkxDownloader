using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;
using TirkxDownloader.Framework;

namespace TirkxDownloader.Services
{
    public class AuthorizationManager
    {
        /// <summary>
        /// get credential
        /// </summary>
        /// <param name="target">target site hostname</param>
        /// <returns>AuthorizationInfo that contain credential informations, null when not found</returns>
        public AuthorizationInfo GetCredential(string target)
        {
            var credential = new Credential { Target = target };

            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

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

            if (!File.Exists("Target domain.dat"))
            {
                return credentials;
            }

            using (var str = new StreamReader("Target domain.dat", Encoding.UTF8))
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
            // in case this target was added before
            if (File.Exists("Target domain.dat"))
            {
                using (var str = new StreamReader("Target domain.dat", Encoding.UTF8))
                {
                    string temp;

                    while ((temp = str.ReadLine()) != null)
                    {
                        if (target.Equals(temp))
                        {
                            return false;
                        }
                    }
                }
            }

            using (var stw = new StreamWriter("Target domain.dat", true, Encoding.UTF8))
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

            // this maybe return false if credential has been added
            return credential.Save();
        }

        public bool DeleteCredential(string target)
        {
            var credential = new Credential { Target = target };
            bool flag = credential.Delete();

            if (flag)
            {
                var lines = new List<string>();

                using (var str = new StreamReader("Target domain.dat", Encoding.UTF8))
                {
                    string temp;

                    while ((temp = str.ReadLine()) != null)
                    {
                        lines.Add(temp);
                    }
                }

                lines.Remove(target);

                using (var stw = new StreamWriter("Target domain.dat", false, Encoding.UTF8))
                {
                    foreach (var line in lines)
                    {
                        stw.WriteLine(line);
                    }
                }
            }

            return flag;
        }
    }
}
