using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;

namespace FTPKey.BaseClient
{
    /// <summary>
    /// Gestione collegamento via SFTP
    /// </summary>
    internal class SftpClient : IFtpClient
    {
        #region Variables
        /// <summary>
        /// SSH Key for validation
        /// </summary>
        private string _fingerPrint;

        /// <summary>
        /// Sftp client
        /// </summary>
        private Renci.SshNet.SftpClient _client;
        #endregion

        #region Properties
        public bool IsConnected => _client?.IsConnected ?? false;
        #endregion

        #region Connection Methods
        /// <summary>
        /// Opens a connection with Ftp area
        /// </summary>
        public void Connect()
        {
            try
            {
                if (!_client.IsConnected)
                    _client.Connect();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(string.Format(Messages.Messages.ConnectionExceptionMessage, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                throw new Exception(Messages.Messages.AuthenticationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            if (_client.IsConnected)
                _client.Disconnect();
        }
        #endregion

        #region Transfer Methods
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="remoteFileName">The file to delete</param>
        public bool DeleteFile(string remoteFileName)
        {
            try
            {
                _client.DeleteFile(remoteFileName);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDelete, remoteFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDelete), ex);
            }

            return !_client.Exists(remoteFileName);
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, string destinationFile, bool deleteFileAfterDownload)
        {
            Stream outputStream = null;
            if (DownloadFile(remoteFileName, outputStream, deleteFileAfterDownload))
            {
                using (FileStream destinationFileStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    outputStream.CopyTo(destinationFileStream);
                }
                return true;
            }
            else
            {
                return false;
            }
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
                _client.DownloadFile(remoteFileName, outStream);
                remoteFileSize = _client.GetAttributes(remoteFileName).Size;
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDownload, remoteFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDownload), ex);
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
        public bool UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            using (FileStream localFileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                return UploadFile(localFileStream, destinationFileName);
            }
        }
        public bool UploadFile(string localFile, bool deleteFileAfterUpload)
        {
            return UploadFile(localFile, Path.GetFileName(localFile), deleteFileAfterUpload);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public bool UploadFile(Stream localFileStream, string destinationFileName)
        {
            long remoteFileLength = 0;

            try
            {
                _client.UploadFile(localFileStream, destinationFileName, true);

                if (_client.Exists(destinationFileName))
                    remoteFileLength = _client.GetAttributes(destinationFileName).Size;
                else
                    return false;

                if (remoteFileLength != localFileStream.Length)
                {
                    _client.DeleteFile(destinationFileName);
                    return false;
                }

                return true;
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationUpload, destinationFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationUpload), ex);
            }
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public List<string> GetFilesList()
        {
            return GetFilesList(GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="path">The path from witch retrieve the files list</param>
        public List<string> GetFilesList(string path)
        {
            List<string> files = Enumerable.Empty<string>().ToList();
            List<Renci.SshNet.Sftp.SftpFile> sftpFiles = null;

            try
            {
                sftpFiles = _client.ListDirectory(path).ToList();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFilesList, _client.WorkingDirectory), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFilesList), ex);
            }

            if (sftpFiles.Count > 0)
                files = (from file in sftpFiles where !file.IsDirectory select file.Name).ToList();

            return files;
        }

        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            return GetFoldersList(GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of sub-folders from the desired path
        /// </summary>
        /// <param name="path">The main path to search in for the sub-folders list</param>
        public string[] GetFoldersList(string path)
        {
            string[] folders = null;
            List<Renci.SshNet.Sftp.SftpFile> sftpFolders = null;

            try
            {
                sftpFolders = _client.ListDirectory(path).ToList();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFoldersList, _client.WorkingDirectory), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFoldersList), ex);
            }

            if (sftpFolders.Count > 0)
                folders = (from folder in sftpFolders where folder.IsDirectory select folder.Name).ToArray();

            return folders;
        }

        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public bool RenameFile(string currentName, string newName)
        {
            try
            {
                _client.RenameFile(currentName, newName);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationRenameFile, currentName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationRenameFile), ex);
            }

            return _client.Exists(newName);
        }

        public bool CopyFile(string file, string destinationFile)
        {
            bool output = false;
            try
            {
                if (_client.Exists(file))
                {
                    using(MemoryStream stream = new MemoryStream())
                    {
                        _client.DownloadFile(file, stream);
                        _client.UploadFile(stream, destinationFile);

                        output = _client.Exists(destinationFile);
                    }
                }
                return output;
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationRenameFile, file), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationRenameFile), ex);
            }
        }

        public bool MoveFile(string file, string destinationFile)
        {
            bool output = false;
            try
            {
                if (_client.Exists(file))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        _client.RenameFile(file, destinationFile);
                        output = _client.Exists(destinationFile);
                    }
                }
                return output;
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationRenameFile, file), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationRenameFile), ex);
            }
        }

        /// <summary>
        /// Creates a new remote folder; it creates all the missing folders into the path, recursively (for instance /fold1/fold2)
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public bool CreateFolder(string path)
        {
            try
            {
                string[] splittedPath = path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                path = "/";
                for (int index = 0; index < splittedPath.Length; index++)
                {
                    path += $"/{splittedPath[index]}";
                    if (!_client.Exists(path))
                        _client.CreateDirectory(path);
                }
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationCreateFolder, path), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationCreateFolder), ex);
            }

            return _client.Exists(path);
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        public bool DeleteFolder(string path)
        {
            try
            {
                _client.DeleteDirectory(path);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDeleteFolder, path), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDeleteFolder), ex);
            }
            catch (Renci.SshNet.Common.SshException ex)
            {
                throw new Exception(string.Format(Messages.Messages.DeleteFolderNotEmpty, path), ex);
            }

            return !_client.Exists(path);
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public bool DeleteFolder(string path, bool deleteRecursively)
        {
            if (deleteRecursively)
            {
                List<Renci.SshNet.Sftp.SftpFile> files = null;

                try
                {
                    files = _client.ListDirectory(path).ToList();
                }
                catch (Renci.SshNet.Common.SshConnectionException ex)
                {
                    throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
                }
                catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
                {
                    throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFilesList, path), ex);
                }
                catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
                {
                    throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFilesList), ex);
                }

                foreach (Renci.SshNet.Sftp.SftpFile directory in files.Where(x => x.IsDirectory))
                {
                    if (directory.Name != "." && directory.Name != "..")
                    {
                        DeleteFolder(directory.FullName, deleteRecursively);
                    }
                }

                foreach (Renci.SshNet.Sftp.SftpFile file in files.Where(x => !x.IsDirectory))
                {
                    DeleteFile(file.FullName);
                }
            }
            return DeleteFolder(path);
        }

        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        public void SetCurrentFolder(string newFolder)
        {
            try
            {
                _client.ChangeDirectory(newFolder);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationChangeDirectory, newFolder), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationChangeDirectory), ex);
            }
        }

        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        public string GetCurrentFolder()
        {
            return _client.WorkingDirectory;
        }

        /// <summary>
        /// Check if a file exists into the current folder or into a relative path beginning with the current folder
        /// </summary>
        /// <param name="filePath">The relative or full path of the file</param>
        /// <returns></returns>
        public bool FileExists(string filePath)
        {
            return _client.Exists(filePath);
        }

        /// <summary>
        /// Check if the given folder exists
        /// </summary>
        /// <param name="folder">Relative or full path of the folder</param>
        /// <returns></returns>
        public bool FolderExists(string folder)
        {
            return _client.Exists(folder);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Certificate's validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Client_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(_fingerPrint))
            {
                byte[] fingerPrint = _fingerPrint.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
                e.CanTrust = e.FingerPrint.SequenceEqual(fingerPrint);
            }
            else
                e.CanTrust = true;
        }
        #endregion

        #region Constructor
        public SftpClient(string host, int port, string userName, string password, string fingerPrint)
        {
            // Save the fingerprint for validation
            _fingerPrint = fingerPrint;

            _client = new Renci.SshNet.SftpClient(host, port, userName, password);
            _client.HostKeyReceived += _Client_HostKeyReceived;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
        #endregion
    }
}
