using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace FTPKey.Test
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
        private const FTPKey.ConnectionProtocol PROTOCOL = FTPKey.ConnectionProtocol.Ftp;
        private const FTPKey.EncryptionType ENCRYPTION = FTPKey.EncryptionType.None;
        private const string FINGERPRINT = "";

        // Test paths
        private const string TEST_FILES_PATH = @"D:\Sviluppo\GIT\FTPKey\FTPKey.Test\TestFiles";
        private const string ACTIVE_TEST_PATH = @"D:\Sviluppo\GIT\FTPKey\FTPKey.Test\ActiveTestFolder";

        // Test Files
        private const string TEST_FILE_1 = "TestFile.txt";
        private const string TEST_FILE_2 = "Verbundmörtel Zubehör + Technische Daten DE.pdf";
        #endregion

        #region Client
        //FTPKey.IFtpClient _client;
        #endregion

        #region Constructor
        public Tests()
        {
            DirectoryInfo testFolder = new DirectoryInfo(TEST_FILES_PATH);
            DirectoryInfo activeFolder = new DirectoryInfo(ACTIVE_TEST_PATH);

            // Delete all files and directories from the active folder
            foreach (FileInfo file in activeFolder.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in activeFolder.GetDirectories())
            {
                dir.Delete(true);
            }

            // Copy all files from test to active path
            foreach (FileInfo file in testFolder.GetFiles())
            {
                file.CopyTo(Path.Combine(ACTIVE_TEST_PATH, file.Name));
            }
        }
        #endregion

        #region Get Client Methods
        private FTPKey.Client _GetClient(bool connect)
        {
            return new Client(HOST, PORT, USERNAME, PASSWORD, connect, PROTOCOL, ENCRYPTION, FINGERPRINT);
        }
        #endregion

        #region Test Methods
        [TestMethod]
        public void Test_AA_GetClient()
        {
            using (Client client = _GetClient(false))
            {
                Assert.IsTrue(!client.IsConnected);
            }
        }

        [TestMethod]
        public void Test_AB_GetClientWithConnection()
        {
            using (Client client = _GetClient(true))
            {
                Assert.IsTrue(client.IsConnected);
            }
        }

        [TestMethod]
        public void Test_AC_Connection()
        {
            using (Client client = _GetClient(false))
            {
                client.Connect();
                Assert.IsTrue(client.IsConnected);
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_1)]
        [DataRow(true, TEST_FILE_1)]
        [DataRow(false, TEST_FILE_2)]
        [DataRow(true, TEST_FILE_2)]
        public void Test_AD_UploadFile(bool connect, string fileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    bool result = client.UploadFile(Path.Combine(ACTIVE_TEST_PATH, fileName), fileName, false);
                    Assert.IsTrue(result);
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }
        #endregion
    }
}
