using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FTPKey_Test
{
    [TestClass]
    public class Tests
    {
        /// <summary>
        /// Set constants before executing any tests
        /// </summary>
        #region Constants
        private const string HOST = "192.168.1.235";
        private const int PORT = 21;
        private const string USERNAME = "TestUser";
        private const string PASSWORD = "Password2020";
        private const string REMOTE_FOLDER = "";
        private const FTPKey.ConnectionProtocol PROTOCOL = FTPKey.ConnectionProtocol.Ftp;
        private const FTPKey.EncryptionType ENCRYPTION = FTPKey.EncryptionType.None;
        private const string FINGERPRINT = "";
        #endregion

        #region Client
        FTPKey.IFtpClient _client;
        #endregion

        #region Constructor
        public Tests()
        {
        }
        #endregion

        #region Get Client Methods
        private FTPKey.IFtpClient _GetClient(bool connect)
        {
            return FTPKey.ClientProvider.GetFtpClient(HOST, PORT, USERNAME, PASSWORD, connect, REMOTE_FOLDER, PROTOCOL, ENCRYPTION, FINGERPRINT);
        }
        #endregion

        #region Test Methods
        [TestMethod]
        public void GetClient()
        {
            using (FTPKey.IFtpClient client = _GetClient(false))
            {
                Assert.IsTrue(!client.IsConnected);
            }
        }

        [TestMethod]
        public void GetClientWithConnection()
        {
            using (FTPKey.IFtpClient client = _GetClient(true))
            {
                Assert.IsTrue(client.IsConnected);
            }
        }

        [TestMethod]
        public void Connection()
        {
            using (FTPKey.IFtpClient client = _GetClient(false))
            {
                client.Connect();
                Assert.IsTrue(client.IsConnected);
            }
        }
        #endregion
    }
}
