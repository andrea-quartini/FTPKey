using System;
using System.IO;
using System.Collections.Generic;
using FTPKey.BaseClient;

namespace FTPKey
{
    /// <summary>
    /// Protocol used for connection
    /// </summary>
    public enum ConnectionProtocol
    {
        Default = 0,
        Ftp = 1,
        Ftps = 2,
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

    /// <summary>
    /// Ftp connection handler
    /// </summary>
    public class Client : IDisposable
    {
        #region Variables
        private IFtpClient _client;
        #endregion

        #region Transfer Methods
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="fileName">The file to delete</param>
        /// <returns></returns>
        public void DeleteFile(string fileName)
        {
            this._client.DeleteFile(fileName);
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
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public void DownloadFile(string fileName, string destinationFile, bool deleteFileAfterDownload)
        {
            this._client.DownloadFile(fileName, destinationFile, deleteFileAfterDownload);

            if (deleteFileAfterDownload)
                this._client.DeleteFile(fileName);
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
                return this._client.DownloadFile(remoteFileName, outStream, deleteFileAfterDownload);
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

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        public void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
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

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public void UploadFile(Stream localFileStream, string destinationFileName)
        {
            _client.UploadFile(localFileStream, destinationFileName);
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public string[] GetFilesList()
        {
            return this._client.GetFilesList();
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="path">The path from witch retrieve the files list</param>
        public string[] GetFilesList(string pattern)
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

        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            return this._client.GetFoldersList();
        }

        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public void RenameFile(string currentName, string newName)
        {
            this._client.RenameFile(currentName, newName);
        }

        /// <summary>
        /// Creates a new remote folder; it creates all the missing folders into the path, recursively (for instance /fold1/fold2)
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public void CreateFolder(string path)
        {
            this._client.CreateFolder(_CleanRemoteFolder(path));
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        /// <param name="path">The folder to delete</param>
        public void DeleteFolder(string path)
        {
            this._client.DeleteFolder(path);
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public void DeleteFolder(string path, bool deleteRecursively)
        {
            this._client.DeleteFolder(path, deleteRecursively);
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
            return _client.FileExists(filePath);
        }

        /// <summary>
        /// Check if the given folder exists
        /// </summary>
        /// <param name="folder">Relative or full path of the folder</param>
        /// <returns></returns>
        public bool FolderExists(string folder)
        {
            return _client.FolderExists(folder);
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
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        public Client(string host, int port, string userName, string password, string remoteFolder)
            : this(host, port, userName, password, remoteFolder, ConnectionProtocol.Default)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        public Client(string host, int port, string userName, string password, string remoteFolder, ConnectionProtocol protocol)
            : this(host, port, userName, password, remoteFolder, protocol, EncryptionType.None, string.Empty)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        public Client(string host, int port, string userName, string password, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType)
            : this(host, port, userName, password, remoteFolder, protocol, encryptionType, string.Empty)
        {
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        /// <param name="fingerPrint">Fingerprint to validate connection (for SFTP only)</param>
        public Client(string host, int port, string userName, string password, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType, string fingerPrint)
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

            this._client.Connect();
            this._client.SetCurrentFolder(this._CleanRemoteFolder(remoteFolder));
        }
        #endregion

        #region IDisposable
        private bool isDisposed = false;

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
