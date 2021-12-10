using System;
using System.IO;
using System.Collections.Generic;
using FTPKey.BaseClient;
using FTPKey.Exceptions;

namespace FTPKey
{
    #region Public Enums
    /// <summary>
    /// Protocol used for connection
    /// </summary>
    public enum ConnectionProtocol
    {
        /// <summary>
        /// By default, the protocol is Ftp
        /// </summary>
        Default = 0,

        /// <summary>
        /// Ftp protocol
        /// </summary>
        Ftp = 1,

        /// <summary>
        /// Ftp with SSL
        /// </summary>
        Ftps = 2,

        /// <summary>
        ///  Sftp protocol
        /// </summary>
        Sftp = 3
    }

    /// <summary>
    /// Encryption method
    /// </summary>
    public enum EncryptionType
    {
        /// <summary>
        /// No encryption required
        /// </summary>
        None = 0,

        /// <summary>
        /// Implicit encryption (SSL)
        /// </summary>
        Implicit = 1,

        /// <summary>
        /// Explicit encryption (TLS)
        /// </summary>
        Explicit = 2
    }
    #endregion

    /// <summary>
    /// Ftp connection handler
    /// </summary>
    public class Client : IDisposable
    {
        #region Variables
        private IFtpClient _client;
        #endregion

        #region Properties
        public bool IsConnected => _client?.IsConnected ?? false;
        #endregion

        #region Connection methods
        public void Connect()
        {
            if (!_client.IsConnected)
                _client.Connect();
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }
        #endregion

        #region Transfer Methods
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="fileName">The file to delete</param>
        /// <returns></returns>
        public bool DeleteFile(string fileName)
        {
            if (IsConnected)
                return _client.DeleteFile(fileName);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes all the remote files that comply search pattern
        /// </summary>
        /// <param name="fileNameSearchPattern">Pattern di ricerca</param>
        /// <returns></returns>
        public void DeleteFiles(string fileNameSearchPattern)
        {
            List<string> files = GetFilesList();

            if (files != null)
            {
                if (!string.IsNullOrEmpty(fileNameSearchPattern))
                {
                    string regExPattern = _RegExPattern(fileNameSearchPattern);

                    foreach (string file in files)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                            DeleteFile(file);
                    }
                }
            }
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="fileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string fileName, string destinationFile, bool deleteFileAfterDownload)
        {
            bool result;
            if (IsConnected)
                result =  _client.DownloadFile(fileName, destinationFile, deleteFileAfterDownload);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);

            return result;
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="outStream">The output stream of downloaded file</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, Stream outStream, bool deleteFileAfterDownload)
        {
            if (outStream != null)
            {
                if (IsConnected)
                    return _client.DownloadFile(remoteFileName, outStream, deleteFileAfterDownload);
                else
                    throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
            }
            else
            {
                throw new ArgumentNullException(Messages.Messages.PathNotFoundExceptionMessage);
            }
        }

        /// <summary>
        /// Downloads a list of files given the search pattern
        /// </summary>
        /// <param name="fileNameSearchPattern">Search pattern</param>
        /// <param name="destinationPath">Files destination path</param>
        /// <param name="deleteFileAfterDownload">If true, remote files will be deleted after downloading them</param>
        /// <returns></returns>
        public void DownloadFiles(string fileNameSearchPattern, string destinationPath, bool deleteFileAfterDownload)
        {
            if (IsConnected)
            {
                if (Directory.Exists(destinationPath))
                {
                    List<string> files = GetFilesList(fileNameSearchPattern);

                    if (files != null)
                    {
                        foreach (string file in files)
                            DownloadFile(file, Path.Combine(destinationPath, file), deleteFileAfterDownload);
                    }
                }
                else
                    throw new DirectoryNotFoundException(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDownload, destinationPath));
            }
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        public bool UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            if (IsConnected)
            {
                bool result;
                if (File.Exists(localFile))
                {
                    result = _client.UploadFile(localFile, destinationFileName, deleteFileAfterUpload);
                }
                else
                    throw new FileNotFoundException(string.Format(Messages.Messages.UploadFileNotFoundExceptionMessage, localFile));

                if (deleteFileAfterUpload && result)
                    File.Delete(localFile);

                return result;
            }
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public bool UploadFile(Stream localFileStream, string destinationFileName)
        {
            if (IsConnected)
                return _client.UploadFile(localFileStream, destinationFileName);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public List<string> GetFilesList()
        {
            if (IsConnected)
                return _client.GetFilesList();
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="pattern">A research pattern to retrieve specific files; if empty, all files will be retrieved</param>
        public List<string> GetFilesList(string pattern)
        {
            if (IsConnected)
            {
                List<string> files = this.GetFilesList();

                if (!string.IsNullOrEmpty(pattern))
                {
                    string regExPattern = this._RegExPattern(pattern);
                    List<string> filteredFiles = new List<string>();

                    foreach (string file in files)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                            filteredFiles.Add(file);
                    }

                    return filteredFiles;
                }
                else
                    return files;
            }
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            if (IsConnected)
                return _client.GetFoldersList();
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of sub-folders from the desired path
        /// </summary>
        /// <param name="path">The main path to search in for the sub-folders list</param>
        public string[] GetFoldersList(string path)
        {
            if (IsConnected)
                return _client.GetFoldersList(path);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }


        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public bool RenameFile(string currentName, string newName)
        {
            if (IsConnected)
                return _client.RenameFile(currentName, newName);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Copy a file into a new path
        /// </summary>
        /// <param name="file">The file to copy</param>
        /// <param name="destinationFile">New file's path</param>
        /// <returns></returns>
        public bool CopyFile(string file, string destinationFile)
        {
            if (IsConnected)
                return _client.CopyFile(file, destinationFile);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Move a file to a new folder
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destinationFile"></param>
        /// <returns></returns>
        public bool MoveFile(string file, string destinationFile)
        {
            if (IsConnected)
                return _client.MoveFile(file, destinationFile);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Creates a new remote folder; it creates all the missing folders into the path, recursively (for instance /fold1/fold2)
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public bool CreateFolder(string path)
        {
            if (IsConnected)
                return _client.CreateFolder(_CleanRemoteFolder(path));
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        /// <param name="path">The folder to delete</param>
        public bool DeleteFolder(string path)
        {
            if (IsConnected)
                return _client.DeleteFolder(path);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public bool DeleteFolder(string path, bool deleteRecursively)
        {
            if (IsConnected)
                return _client.DeleteFolder(path, deleteRecursively);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        public void SetCurrentFolder(string newFolder)
        {
            _client.SetCurrentFolder(_CleanRemoteFolder(newFolder));
        }

        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        public string GetCurrentFolder()
        {
            return _client.GetCurrentFolder();
        }

        /// <summary>
        /// Check if a file exists into the current folder or into a relative path beginning with the current folder
        /// </summary>
        /// <param name="filePath">The relative or full path of the file</param>
        /// <returns></returns>
        public bool FileExists(string filePath)
        {
            if (IsConnected)
                return _client.FileExists(filePath);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Check if the given folder exists
        /// </summary>
        /// <param name="folder">Relative or full path of the folder</param>
        /// <returns></returns>
        public bool FolderExists(string folder)
        {
            if (IsConnected)
                return _client.FolderExists(folder);
            else
                throw new FtpClientIsDisconnectedException(Messages.Messages.ClientDisconnected);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Remote folder name normalization
        /// </summary>
        /// <param name="remoteFolderOld">The original remote folder name</param>
        /// <returns></returns>
        private string _CleanRemoteFolder(string remoteFolderOld)
        {
            if (string.IsNullOrEmpty(remoteFolderOld))
                remoteFolderOld = "/";

            // Sets the default
            string remoteFolderNew = string.Empty;

            // Path splitted by '/' character
            string[] remoteFolderSplitted = remoteFolderOld.Replace("\\", "/").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Re-joins the path deleting white spaces
            foreach (string subPath in remoteFolderSplitted)
            {
                if (!string.IsNullOrWhiteSpace(subPath))
                    remoteFolderNew += $"{(!string.IsNullOrEmpty(remoteFolderNew) ? "/" : string.Empty)}{subPath.Trim()}";
            }

            // The path must start at least with '/'
            if (!remoteFolderNew.StartsWith("/") && !remoteFolderNew.StartsWith("./") && !remoteFolderNew.StartsWith("../"))
                remoteFolderNew = string.Format("/{0}", remoteFolderNew);

            return remoteFolderNew;
        }

        /// <summary>
        /// Formats a regex-ready pattern 
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private string _RegExPattern(string pattern)
        {
            pattern = pattern.ToUpper();
            pattern = pattern.Replace(".", "[.]");
            pattern = pattern.Replace("?", ".");
            pattern = pattern.Replace("#", "[0-9]");
            pattern = pattern.Replace("*", ".*");
            return pattern;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        public Client(string host, int port, string userName, string password)
            : this(host, port, userName, password, false)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        public Client(string host, int port, string userName, string password, bool connect)
            : this(host, port, userName, password, connect, ConnectionProtocol.Default)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        public Client(string host, int port, string userName, string password, bool connect, ConnectionProtocol protocol)
            : this(host, port, userName, password, connect, protocol, EncryptionType.None)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        public Client(string host, int port, string userName, string password, bool connect, ConnectionProtocol protocol, EncryptionType encryptionType)
            : this(host, port, userName, password, connect, protocol, encryptionType, string.Empty)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        /// <param name="fingerPrint">Fingerprint to validate connection (for SFTP only)</param>
        public Client(string host, int port, string userName, string password, bool connect, ConnectionProtocol protocol, EncryptionType encryptionType, string fingerPrint)
        {
            switch (protocol)
            {
                case ConnectionProtocol.Default:
                case ConnectionProtocol.Ftp:
                case ConnectionProtocol.Ftps:
                    _client = new FtpClient(host, port, userName, password, (protocol == ConnectionProtocol.Ftps), encryptionType);
                    break;
                case ConnectionProtocol.Sftp:
                    _client = new SftpClient(host, port, userName, password, fingerPrint);
                    break;
                default:
                    break;
            }

            if (connect)
            {
                Connect();
            }
        }
        #endregion

        #region IDisposable
        private bool isDisposed = false;

        /// <summary>
        /// Executes disconnection and destroys the object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                }

                if (_client != null)
                {
                    _client.Disconnect();
                    _client.Dispose();
                    _client = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Executes disconnection and destroys the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Client()
        {
            Dispose(false);
        }
        #endregion
    }
}
