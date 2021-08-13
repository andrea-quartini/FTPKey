using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FluentFTP;

namespace FTPKey.BaseClient
{
    /// <summary>
    /// Handles connection with FTP or FTPS protocols
    /// </summary>
    internal class FtpClient : IFtpClient
    {
        #region Constants
        private const int RETRY_ATTEMPTS_NUMBER = 3;
        #endregion

        #region Variables
        private FluentFTP.FtpClient _client;
        #endregion

        #region Properties
        public bool IsConnected => _client.IsConnected && _client.IsAuthenticated;
        #endregion

        #region Connection Methods
        /// <summary>
        /// Opens a connection with Ftp area
        /// </summary>
        public void Connect()
        {
            try
            {
                if (!this._client.IsConnected)
                    this._client.Connect();
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.ConnectionExceptionMessage, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            if (this._client.IsConnected)
                this._client.Disconnect();
        }
        #endregion

        #region Transfer Methods
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="remoteFileName">The file to delete</param>
        public void DeleteFile(string remoteFileName)
        {
            try
            {
                this._client.DeleteFile(remoteFileName);
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationDelete, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, string destinationFile, bool deleteFileAfterDownload)
        {
            long remoteFileSize = 0;

            try
            {
                this._client.DownloadFile(destinationFile, remoteFileName, FtpLocalExists.Overwrite, FtpVerify.Retry | FtpVerify.Delete | FtpVerify.Throw);
                remoteFileSize = this._client.GetFileSize(remoteFileName);
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationDownload, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }

            if (File.Exists(destinationFile))
            {
                FileInfo destinationFileInfo = new FileInfo(destinationFile);

                if (remoteFileSize != destinationFileInfo.Length)
                {
                    File.Delete(destinationFile);
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
                }

                if (deleteFileAfterDownload)
                {
                    _client.DeleteFile(remoteFileName);
                }
                return true;
            }
            else
                throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="outStream">The output stream of downloaded file</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, Stream outStream, bool deleteFileAfterDownload)
        {
            long remoteFileSize = 0;

            try
            {
                this._client.Download(outStream, remoteFileName);
                remoteFileSize = this._client.GetFileSize(remoteFileName);
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationDownload, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }

            if (outStream.Length > 0)
            {
                if (remoteFileSize != outStream.Length)
                {
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
                }

                if (deleteFileAfterDownload)
                {
                    _client.DeleteFile(remoteFileName);
                }
                return true;
            }
            else
                throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        public void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            long remoteFileLength = 0;

            try
            {
                this._client.UploadFile(localFile, destinationFileName, FtpRemoteExists.Overwrite, false, FtpVerify.Retry | FtpVerify.Delete | FtpVerify.Throw);

                if (this._client.FileExists(destinationFileName))
                    remoteFileLength = this._client.GetFileSize(destinationFileName);
                else
                    throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));

                if (remoteFileLength != (new FileInfo(localFile)).Length)
                {
                    this._client.DeleteFile(destinationFileName);
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));
                }
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationUpload, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }
        public void UploadFile(string localFile, bool deleteFileAfterUpload)
        {
            this.UploadFile(localFile, Path.GetFileName(localFile), deleteFileAfterUpload);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public void UploadFile(Stream localFileStream, string destinationFileName)
        {
            long remoteFileLength = 0;

            try
            {
                this._client.Upload(localFileStream, destinationFileName, FtpRemoteExists.Overwrite, false);

                if (this._client.FileExists(destinationFileName))
                    remoteFileLength = this._client.GetFileSize(destinationFileName);
                else
                    throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));

                if (remoteFileLength != localFileStream.Length)
                {
                    this._client.DeleteFile(destinationFileName);
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));
                }
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationUpload, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public string[] GetFilesList()
        {
            return this.GetFilesList(this.GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="path">The path from witch retrieve the files list</param>
        public string[] GetFilesList(string path)
        {
            string[] files = null;
            List<FluentFTP.FtpListItem> ftpFilesList = null;

            try
            {
                ftpFilesList = this._client.GetListing(path).ToList();
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationGetFilesList, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }

            if (ftpFilesList.Count > 0)
                files = (from file in ftpFilesList where file.Type == FtpFileSystemObjectType.File select file.Name).ToArray();

            return files;
        }


        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            return this.GetFoldersList(this.GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of sub-folders from the desired path
        /// </summary>
        /// <param name="path">The main path to search in for the sub-folders list</param>
        public string[] GetFoldersList(string path)
        {
            string[] folders = null;
            List<FluentFTP.FtpListItem> ftpFoldersList = null;

            try
            {
                ftpFoldersList = this._client.GetListing(path).ToList();
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationGetFoldersList, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }

            if (ftpFoldersList.Count > 0)
                folders = (from file in ftpFoldersList where file.Type == FtpFileSystemObjectType.Directory select file.Name).ToArray();

            return folders;
        }

        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public void RenameFile(string currentName, string newName)
        {
            try
            {
                this._client.Rename(currentName, newName);
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationRenameFile, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Creates a new remote folder; it creates all the missing folders into the path, recursively (for instance /fold1/fold2)
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public void CreateFolder(string path)
        {
            try
            {
                this._client.CreateDirectory(path, true);
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationCreateFolder, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        public void DeleteFolder(string path)
        {
            try
            {
                List<FtpListItem> items = this._client.GetListing(path).Where(x => x.Type == FtpFileSystemObjectType.Directory).ToList();

                if (items.Count == 0)
                    this._client.DeleteDirectory(path);
                else
                    throw new Exception(string.Format(Messages.Messages.DeleteFolderNotEmpty, path));
            }
            catch (FluentFTP.FtpException ex)
            {
                throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationDeleteFolder, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public void DeleteFolder(string path, bool deleteRecursively)
        {
            if (deleteRecursively)
            {
                try
                {
                    List<FtpListItem> items = this._client.GetListing(path).Where(x => x.Type == FtpFileSystemObjectType.Directory).ToList();

                    if (items.Count == 0)
                        this._client.DeleteDirectory(path);
                    else
                    {
                        foreach (FtpListItem item in items.Where(x => x.Type == FtpFileSystemObjectType.Directory))
                        {
                            this.DeleteFolder(item.FullName, deleteRecursively);
                        }

                        this._client.DeleteDirectory(path);
                    }
                }
                catch (FluentFTP.FtpException ex)
                {
                    throw new Exception(string.Format(Messages.Messages.GenericException, Messages.Messages.OperationDeleteFolder, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
                }
            }
            else
            {
                this.DeleteFolder(path);
            }
        }

        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        public void SetCurrentFolder(string newFolder)
        {
            if (this._client.DirectoryExists(newFolder))
                this._client.SetWorkingDirectory(newFolder);
            else
                throw new DirectoryNotFoundException(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationChangeDirectory, newFolder));
        }

        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        public string GetCurrentFolder()
        {
            return this._client.GetWorkingDirectory();
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
            return _client.DirectoryExists(folder);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Certificate's validation
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        private void _Client_ValidateCertificate(FluentFTP.FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }
        #endregion

        #region Constructor
        public FtpClient(string host, int port, string userName, string password, bool useSsl, EncryptionType encryptionType)
        {
            // Encryption mode conversion
            FtpEncryptionMode encryption = FtpEncryptionMode.None;
            switch (encryptionType)
            {
                case EncryptionType.None:
                    encryption = FtpEncryptionMode.None;
                    break;
                case EncryptionType.Implicit:
                    encryption = FtpEncryptionMode.Implicit;
                    break;
                case EncryptionType.Explicit:
                    encryption = FtpEncryptionMode.Explicit;
                    break;
            }

            // Access credentials's set
            NetworkCredential credentials = new NetworkCredential(userName, password);

            this._client = new FluentFTP.FtpClient(host, port, credentials);
            this._client.EncryptionMode = encryption;
            this._client.RetryAttempts = RETRY_ATTEMPTS_NUMBER;

            // SSL's certificate validation call back
            if (useSsl)
                this._client.ValidateCertificate += _Client_ValidateCertificate;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (this._client != null)
            {
                this._client.Dispose();
                this._client = null;
            }
        }
        #endregion
    }
}
