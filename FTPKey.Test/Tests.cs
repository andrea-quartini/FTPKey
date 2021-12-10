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
        private const string HOST = "";
        private const int PORT = 21;
        private const string USERNAME = "";
        private const string PASSWORD = "";
        private const FTPKey.ConnectionProtocol PROTOCOL = FTPKey.ConnectionProtocol.Ftp;
        private const FTPKey.EncryptionType ENCRYPTION = FTPKey.EncryptionType.None;
        private const string FINGERPRINT = "";

        // Test paths
        private const string TEST_FILES_PATH = @"E:\Develop\Samples\GIT\FTPKey\FTPKey.Test\TestFiles";
        private const string ACTIVE_TEST_PATH = @"E:\Develop\Samples\GIT\FTPKey\FTPKey.Test\ActiveTestFolder";

        // Test Files
        private const string TEST_FILE_1 = "TestFile.txt";
        private const string TEST_FILE_2 = "Verbundmörtel Zubehör + Technische Daten DE.pdf";
        private const string TEST_FILE_3 = "TestFile2.txt";
        private const string TEST_FILE_3_COPY = "TestFile2_Copy.txt";
        private const string TEST_FOLDER_1 = "TestSubFolder";
        private const string TEST_FOLDER_2 = @"TestSubFolder2\TestSubFolder3";
        private const string TEST_FOLDER_3 = @"TestSubFolder4";
        private const string TEST_DOWNLOAD_FOLDER = "DownloadFolder";
        #endregion

        #region Variables
        DirectoryInfo testFolder = new DirectoryInfo(TEST_FILES_PATH);
        DirectoryInfo activeFolder = new DirectoryInfo(ACTIVE_TEST_PATH);
        #endregion

        #region Constructor
        public Tests()
        {
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

            Directory.CreateDirectory(Path.Combine(activeFolder.FullName, TEST_DOWNLOAD_FOLDER));
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

        [TestMethod]
        public void Test_AD_ListAndDeleteFilesAndFolders()
        {
            using (Client client = _GetClient(true))
            {
                foreach (string file in client.GetFilesList())
                {
                    client.DeleteFile(file);
                }

                foreach (string folder in client.GetFoldersList())
                {
                    client.DeleteFolder(folder, true);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_1)]
        [DataRow(true, TEST_FILE_1)]
        [DataRow(false, TEST_FILE_2)]
        [DataRow(true, TEST_FILE_2)]
        public void Test_AE_UploadFile(bool connect, string fileName)
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

        [DataTestMethod]
        [DataRow(false, TEST_FILE_3)]
        [DataRow(true, TEST_FILE_3)]
        public void Test_AF_UploadFileSubFolder(bool connect, string fileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    if (client.CreateFolder(TEST_FOLDER_1))
                    {
                        client.SetCurrentFolder(TEST_FOLDER_1);
                        bool result = client.UploadFile(Path.Combine(ACTIVE_TEST_PATH, fileName), fileName, false);
                        Assert.IsTrue(result);
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_3)]
        [DataRow(true, TEST_FILE_3)]
        public void Test_AG_UploadFileSubFolder2(bool connect, string fileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    if (client.CreateFolder(TEST_FOLDER_2))
                    {
                        client.SetCurrentFolder(TEST_FOLDER_2);
                        bool result = client.UploadFile(Path.Combine(ACTIVE_TEST_PATH, fileName), fileName, false);
                        Assert.IsTrue(result);
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_3)]
        [DataRow(true, TEST_FILE_3)]
        public void Test_AH_UploadFileSubFolder3_FileStream(bool connect, string fileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    if (client.CreateFolder(TEST_FOLDER_3))
                    {
                        using (Stream stream = File.OpenRead(Path.Combine(ACTIVE_TEST_PATH, fileName)))
                        {
                            bool result = client.UploadFile(stream, fileName);
                            Assert.IsTrue(result);
                        }
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_1, false)]
        [DataRow(true, TEST_FILE_1, false)]
        [DataRow(true, TEST_FILE_1, true)]
        public void Test_AI_DownloadFile(bool connect, string fileName, bool deleteFileAfterDownload)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    string localFilePath = Path.Combine(activeFolder.FullName, TEST_DOWNLOAD_FOLDER, fileName);
                    
                    if (File.Exists(localFilePath))
                        File.Delete(localFilePath);

                    bool result = client.DownloadFile(fileName, localFilePath, deleteFileAfterDownload);

                    Assert.IsTrue(result && (!deleteFileAfterDownload || (deleteFileAfterDownload && !client.FileExists(fileName))));
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_2, false)]
        [DataRow(true, TEST_FILE_2, false)]
        [DataRow(true, TEST_FILE_2, true)]
        public void Test_AJ_DownloadFile_FileStream(bool connect, string fileName, bool deleteFileAfterDownload)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    string localFilePath = Path.Combine(activeFolder.FullName, TEST_DOWNLOAD_FOLDER, fileName);
                    
                    if (File.Exists(localFilePath))
                        File.Delete(localFilePath);

                    using (Stream stream = File.Open(localFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        bool result = client.DownloadFile(fileName, stream, deleteFileAfterDownload);
                        Assert.IsTrue(result && (!deleteFileAfterDownload || (deleteFileAfterDownload && !client.FileExists(fileName))));
                    }
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false, TEST_FILE_3, TEST_FILE_3_COPY)]
        [DataRow(true, TEST_FILE_3, TEST_FILE_3_COPY)]
        public void Test_AK_CopyFile(bool connect, string fileName, string destinationFileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    bool result = client.CopyFile(fileName, destinationFileName);
                    Assert.IsTrue(result);
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(true, TEST_FOLDER_1, TEST_FILE_3, TEST_FOLDER_2, TEST_FILE_3_COPY)]
        public void Test_AL_CopyFile_SubFolder(bool connect, string originFolder, string originFileName, string destinationFolder, string destinationFileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    client.SetCurrentFolder(originFolder);

                    bool result = client.CopyFile(originFileName, Path.Combine("../", destinationFolder, destinationFileName));
                    Assert.IsTrue(result);
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(true, TEST_FOLDER_1, TEST_FILE_3, TEST_FOLDER_3, TEST_FILE_3_COPY)]
        public void Test_AM_MoveFile_SubFolder(bool connect, string originFolder, string originFileName, string destinationFolder, string destinationFileName)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    client.SetCurrentFolder(originFolder);

                    bool result = client.MoveFile(originFileName, Path.Combine("../", destinationFolder, destinationFileName));
                    Assert.IsTrue(result);
                }
                catch (System.Exception ex)
                {
                    Assert.IsTrue(!connect && ex is FTPKey.Exceptions.FtpClientIsDisconnectedException);
                }
            }
        }

        [DataTestMethod]
        [DataRow(true, TEST_FOLDER_2, "*.txt", false)]
        [DataRow(true, TEST_FOLDER_1, "*.txt", true)]
        public void Test_AN_GetFilesList(bool connect, string folder, string filePath, bool empty)
        {
            using (Client client = _GetClient(connect))
            {
                try
                {
                    client.SetCurrentFolder(folder);
                    System.Collections.Generic.List<string> result = client.GetFilesList(filePath);
                    Assert.IsTrue((result.Count > 0 && !empty) || (result.Count == 0 && empty));
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
