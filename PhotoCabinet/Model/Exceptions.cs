using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class MetadataMissingException : Exception { }

    public class FileRenameException : Exception { }

    public class BackupDirectoryAlreadyExistException : Exception { }
}
