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
        None = 0,
        Implicit = 1,
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
            // Recupera l'elenco dei file nella cartella di lavoro
            string[] files = this.GetFilesList();

            // Verifica che l'elenco sia valido
            if (files != null)
            {
                if (!string.IsNullOrEmpty(fileNameSearchPattern))
                {
                    // Recupera il pattern formattato per utilizzare le regex
                    string regExPattern = this._RegExPattern(fileNameSearchPattern);

                    // Cicla sui file
                    foreach (string file in files)
                    {
                        // Verifica se il nome del file corrisponde al pattern di ricerca ed eventualmente lo aggiunge alla lista
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                            this.DeleteFile(file);
                    }
                }
            }
        }

        /// <summary>
        /// Download di un file dalla cartella di lavoro remota
        /// </summary>
        /// <param name="remoteFileName">Nome del file da scaricare</param>
        /// <param name="destinationFile">Percorso completo del file di destinazione</param>
        /// <param name="deleteFileAfterDownload">Indica se il file vada cancellato dalla cartella di lavoro remota dopo il download</param>
        /// <returns></returns>
        public void DownloadFile(string fileName, string destinationFile, bool deleteFileAfterDownload)
        {
            // Chiama il metodo della classe base
            this._client.DownloadFile(fileName, destinationFile, deleteFileAfterDownload);

            // Controlla se il file va cancellato dopo il download
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
        /// Download di una lista di file dalla cartella di lavoro remota tramite pattern di ricerca
        /// </summary>
        /// <param name="fileNameSearchPattern">Pattern di ricerca</param>
        /// <param name="destinationPath">Percorso completo di destinazione dei file</param>
        /// <param name="deleteFileAfterDownload">Indica se i file vadano cancellati dalla cartella di lavoro remota dopo il download</param>
        /// <returns></returns>
        public void DownloadFiles(string fileNameSearchPattern, string destinationPath, bool deleteFileAfterDownload)
        {
            if (Directory.Exists(destinationPath))
            {
                // Recupera l'elenco dei file nella cartella di lavoro
                string[] files = this.GetFilesList(fileNameSearchPattern);

                // Verifica che l'elenco sia valido
                if (files != null)
                {
                    // Cicla sui file
                    foreach (string file in files)
                        // Esegue il download
                        this.DownloadFile(file, Path.Combine(destinationPath, file), deleteFileAfterDownload);
                }
            }
            else
                throw new DirectoryNotFoundException(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDownload, destinationPath));
        }

        /// <summary>
        /// Upload di un file nella cartella di lavoro remota
        /// </summary>
        /// <param name="localFile">Percorso completo del file da caricare</param>
        /// <param name="destinationFileName">Nome del file di destinazione</param>
        /// <param name="deleteFileAfterUpload">Indica se il file caricato vada cancellato dopo l'upload</param>
        /// <returns></returns>
        public void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            // Controlla che il file locale esista
            if (File.Exists(localFile))
            {
                // Chiama il metodo della classe base
                this._client.UploadFile(localFile, destinationFileName, deleteFileAfterUpload);
            }
            else
                throw new FileNotFoundException(string.Format(Messages.Messages.UploadFileNotFoundExceptionMessage, localFile));

            // Controlla se il file va eliminato dopo l'upload
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
        /// Recupera l'elenco dei file presenti nella cartella di lavoro remota
        /// </summary>
        /// <returns>Lista dei file presenti</returns>
        public string[] GetFilesList()
        {
            return this._client.GetFilesList();
        }

        /// <summary>
        /// Recupera l'elenco dei file presenti nella cartella di lavoro remota filtrandoli tramite pattern di ricerca
        /// </summary>
        /// <param name="pattern">Pattern di ricerca</param>
        /// <returns></returns>
        public string[] GetFilesList(string pattern)
        {
            // Recupera l'elenco dei file nella cartella di lavoro
            string[] files = this.GetFilesList();

            // Verifica che l'elenco sia valido
            if (files != null)
            {
                if (!string.IsNullOrEmpty(pattern))
                {
                    // Recupera il pattern formattato per utilizzare le regex
                    string regExPattern = this._RegExPattern(pattern);

                    // Prepara una lista per filtrare i file
                    List<string> filteredFiles = new List<string>();

                    // Cicla sui file
                    foreach (string file in files)
                    {
                        // Verifica se il nome del file corrisponde al pattern di ricerca ed eventualmente lo aggiunge alla lista
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.ToUpper(), regExPattern))
                            filteredFiles.Add(file);
                    }

                    // Ritorna la lista filtrata
                    if (filteredFiles.Count > 0)
                        return filteredFiles.ToArray();
                    else
                        return null;
                }
                else
                    // Ritorna la lista non filtrata
                    return files;
            }
            else
                return null;
        }

        /// <summary>
        /// Recupera l'elenco delle sotto-directory presenti nella cartella di lavoro remota
        /// </summary>
        /// <returns>Lista delle sotto-directory presenti</returns>
        public string[] GetFoldersList()
        {
            return this._client.GetFoldersList();
        }
        
        /// <summary>
        /// Rinomina un file nella cartella di lavoro remota
        /// </summary>
        /// <param name="currentName">Nome del file remoto da rinominare</param>
        /// <param name="newName">Nuovo nome da assegnare al file remoto</param>
        public void RenameFile(string currentName, string newName)
        {
            this._client.RenameFile(currentName, newName);
        }

        /// <summary>
        /// Crea una nuova cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        public void CreateFolder(string path)
        {
            this._client.CreateFolder(path);
        }

        /// <summary>
        /// Cancella una cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        public void DeleteFolder(string path)
        {
            this._client.DeleteFolder(path);
        }

        /// <summary>
        /// Cancella una cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        /// <param name="deleteRecursively">Indica se eseguire la cancellazione ricorsivamente nel caso la cartella non sia vuota</param>
        public void DeleteFolder(string path, bool deleteRecursively)
        {
            this._client.DeleteFolder(path, deleteRecursively);
        }

        /// <summary>
        /// Imposta la cartella di lavoro corrente
        /// </summary>
        /// <param name="newFolder">Percorso completo della nuova cartella di lavoro</param>
        public void SetCurrentFolder(string newFolder)
        {
            this._client.SetCurrentFolder(this._CleanRemoteFolder(newFolder));
        }

        /// <summary>
        /// Restituisce l'attuale cartella di lavoro
        /// </summary>
        /// <returns></returns>
        public string GetCurrentFolder()
        {
            return this._client.GetCurrentFolder();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Metodo per la pulizia e normalizzazione della cartella di lavoro remota
        /// </summary>
        /// <param name="remoteFolderOld"></param>
        /// <returns></returns>
        private string _CleanRemoteFolder(string remoteFolderOld)
        {
            // Imposta il default
            string remoteFolderNew = string.Empty;

            // Splitta il percorso
            string[] remoteFolderSplitted = remoteFolderOld.Replace("\\", "/").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Ricompone il percorso senza eventuali parti vuote ed eliminando gli spazi
            foreach (string subPath in remoteFolderSplitted)
            {
                if (!string.IsNullOrWhiteSpace(subPath))
                    remoteFolderNew += $"{(!string.IsNullOrEmpty(remoteFolderNew) ? "/" : string.Empty)}{subPath.Trim()}";
            }

            // Il percorso deve iniziare almeno con lo '/'
            if (!remoteFolderNew.StartsWith("/") && !remoteFolderNew.StartsWith("./") && !remoteFolderNew.StartsWith("../"))
                remoteFolderNew = string.Format("/{0}", remoteFolderNew);

            // Valore di ritorno
            return remoteFolderNew;
        }

        /// <summary>
        /// Restituisce un pattern formattato per le regex
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
        public Client(string host, int port, string userName, string password, string remoteFolder)
            : this(host, port, userName, password, remoteFolder, ConnectionProtocol.Default)
        {
        }
        public Client(string host, int port, string userName, string password, string remoteFolder, ConnectionProtocol protocol)
            : this(host, port, userName, password, remoteFolder, protocol, EncryptionType.None, string.Empty)
        {
        }
        public Client(string host, int port, string userName, string password, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType)
            : this(host, port, userName, password, remoteFolder, protocol, encryptionType, string.Empty)
        {
        }
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

                // Distrugge il client
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
        /// Distruttore
        /// </summary>
        ~Client()
        {
            Dispose(false);
        }
        #endregion
    }
}
