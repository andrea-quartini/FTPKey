using System;
using System.IO;
using System.Collections.Generic;
using FTPKey.BaseClient;

namespace FTPKey.BaseClient
{
    /// <summary>
    /// Ftp connection handler
    /// </summary>
    internal class Client : IFtpClient, IDisposable
    {
        #region Variables
        private IFtpClient _client;
        #endregion

        #region Properties
        public bool IsConnected => _client.IsConnected;
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
        public void DeleteFile(string fileName)
        {
            if (IsConnected)
                this._client.DeleteFile(fileName);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes all the remote files that comply search pattern
        /// </summary>
        /// <param name="fileNameSearchPattern">Pattern di ricerca</param>
        /// <returns></returns>
        public void DeleteFiles(string fileNameSearchPattern)
        {
            string[] files = this.GetFilesList();

            if (files != null)
            {
                if (!string.IsNullOrEmpty(fileNameSearchPattern))
                {
                    string regExPattern = this._RegExPattern(fileNameSearchPattern);

                    foreach (string file in files)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                            this.DeleteFile(file);
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
                result =  this._client.DownloadFile(fileName, destinationFile, deleteFileAfterDownload);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);

            if (deleteFileAfterDownload && result)
                this._client.DeleteFile(fileName);

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
                    return this._client.DownloadFile(remoteFileName, outStream, deleteFileAfterDownload);
                else
                    throw new Exception(Messages.Messages.ClientDisconnected);
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
                    string[] files = this.GetFilesList(fileNameSearchPattern);

                    if (files != null)
                    {
                        foreach (string file in files)
                            this.DownloadFile(file, Path.Combine(destinationPath, file), deleteFileAfterDownload);
                    }
                }
                else
                    throw new DirectoryNotFoundException(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDownload, destinationPath));
            }
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        public void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            if (IsConnected)
            {
                if (File.Exists(localFile))
                {
                    this._client.UploadFile(localFile, destinationFileName, deleteFileAfterUpload);
                }
                else
                    throw new FileNotFoundException(string.Format(Messages.Messages.UploadFileNotFoundExceptionMessage, localFile));

                if (deleteFileAfterUpload)
                    File.Delete(localFile);
            }
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public void UploadFile(Stream localFileStream, string destinationFileName)
        {
            if (IsConnected)
                _client.UploadFile(localFileStream, destinationFileName);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public string[] GetFilesList()
        {
            if (IsConnected)
                return this._client.GetFilesList();
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="pattern">A research pattern to retrieve specific files; if empty, all files will be retrieved</param>
        public string[] GetFilesList(string pattern)
        {
            if (IsConnected)
            {
                string[] files = this.GetFilesList();

                if (files != null)
                {
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        string regExPattern = this._RegExPattern(pattern);
                        List<string> filteredFiles = new List<string>();

                        foreach (string file in files)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                                filteredFiles.Add(file);
                        }

                        if (filteredFiles.Count > 0)
                            return filteredFiles.ToArray();
                        else
                            return null;
                    }
                    else
                        return files;
                }
                else
                    return null;
            }
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            if (IsConnected)
                return this._client.GetFoldersList();
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
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
                throw new Exception(Messages.Messages.ClientDisconnected);
        }


        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public void RenameFile(string currentName, string newName)
        {
            if (IsConnected)
                this._client.RenameFile(currentName, newName);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Creates a new remote folder; it creates all the missing folders into the path, recursively (for instance /fold1/fold2)
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public void CreateFolder(string path)
        {
            if (IsConnected)
                this._client.CreateFolder(_CleanRemoteFolder(path));
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        /// <param name="path">The folder to delete</param>
        public void DeleteFolder(string path)
        {
            if (IsConnected)
                this._client.DeleteFolder(path);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public void DeleteFolder(string path, bool deleteRecursively)
        {
            if (IsConnected)
                this._client.DeleteFolder(path, deleteRecursively);
            else
                throw new Exception(Messages.Messages.ClientDisconnected);
        }

        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        public void SetCurrentFolder(string newFolder)
        {
            this._client.SetCurrentFolder(this._CleanRemoteFolder(newFolder));
        }

        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        public string GetCurrentFolder()
        {
            return this._client.GetCurrentFolder();
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
                throw new Exception(Messages.Messages.ClientDisconnected);
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
                throw new Exception(Messages.Messages.ClientDisconnected);
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
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        public Client(string host, int port, string userName, string password, bool connect)
            : this(host, port, userName, password, connect, string.Empty)
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
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        public Client(string host, int port, string userName, string password, bool connect, string remoteFolder)
            : this(host, port, userName, password, connect, remoteFolder, ConnectionProtocol.Default)
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
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        public Client(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol)
            : this(host, port, userName, password, connect, remoteFolder, protocol, EncryptionType.None, string.Empty)
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
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        public Client(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType)
            : this(host, port, userName, password, connect, remoteFolder, protocol, encryptionType, string.Empty)
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
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        /// <param name="fingerPrint">Fingerprint to validate connection (for SFTP only)</param>
        public Client(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType, string fingerPrint)
        {
            switch (protocol)
            {
                case ConnectionProtocol.Default:
                case ConnectionProtocol.Ftp:
                case ConnectionProtocol.Ftps:
                    this._client = new FtpClient(host, port, userName, password, (protocol == ConnectionProtocol.Ftps), encryptionType);
                    break;
                case ConnectionProtocol.Sftp:
                    this._client = new SftpClient(host, port, userName, password, fingerPrint);
                    break;
                default:
                    break;
            }

            if (connect)
            {
                this._client.Connect();
            }
            this._client.SetCurrentFolder(this._CleanRemoteFolder(remoteFolder));
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

                if (this._client != null)
                {
                    this._client.Disconnect();
                    this._client.Dispose();
                    this._client = null;
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
