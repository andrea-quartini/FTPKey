# FTPKey
FTPKey is a simple wrapper library that relays on other two packages, [FluentFTP](https://github.com/robinrodricks/FluentFTP) and [SSH.NET](https://github.com/sshnet/SSH.NET); its aim is to provide Ftp, Ftps and Sftp access within a single package, simplifying some logics and adding some functionalities to both packages.

## Usage
The `Client` class implements IDisposable interface, connection is provided by calling constructor, while disconnection by **Dispose** method, so you can wrap it into a **Using** clause and connection/disconnection will be automatically performed;

**FTP**
```C#
using (FTPKey.Client client = new FTPKey.Client("127.0.0.1", 21, "username", "password", "remotefolder", FTPKey.ConnectionProtocol.Ftp, FTPKey.EncryptionType.None))
{
    ...
}
```
**FTPS**
```C#
using (FTPKey.Client client = new FTPKey.Client("127.0.0.1", 21, "username", "password", "remotefolder", FTPKey.ConnectionProtocol.Ftps, FTPKey.EncryptionType.Explicit))
{
    ...
}
```
**SFTP**
```C#
using (FTPKey.Client client = new FTPKey.Client("127.0.0.1", 22, "username", "password", "remotefolder", FTPKey.ConnectionProtocol.Sftp, FTPKey.EncryptionType.Implicit))
{
    ...
}
```

## Example usage
For extensive examples, check out [Example of usage](https://github.com/andrea-quartini/FTPKey/wiki/Example-of-usage)
