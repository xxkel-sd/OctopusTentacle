using System;
using System.Threading;
using Octopus.Shared.Activities;

namespace Octopus.Shared.Integration.Ftp
{
    public class FtpSynchronizationSettings
    {
        readonly string host;
        readonly string username;
        readonly string password;
        readonly bool useFtps;
        readonly IActivityLog log;
        readonly CancellationToken cancellationToken;

        public FtpSynchronizationSettings(string host, string username, string password, bool useFtps, IActivityLog log, CancellationToken cancellationToken)
        {
            this.host = host;
            this.username = username;
            this.password = password;
            this.useFtps = useFtps;
            this.log = log;
            this.cancellationToken = cancellationToken;
        }

        public bool UseFtps
        {
            get { return useFtps; }
        }

        public string Host
        {
            get { return host; }
        }

        public string Username
        {
            get { return username; }
        }

        public string Password
        {
            get { return password; }
        }

        public IActivityLog Log
        {
            get { return log; }
        }

        public string LocalDirectory { get; set; }
        public string RemoteDirectory { get; set; }

        public CancellationToken CancellationToken
        {
            get { return cancellationToken; }
        }
    }
}