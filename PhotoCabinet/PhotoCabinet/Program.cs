using System;

namespace PhotoCabinet
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryPath = args.Length < 1 ? "F:\\" : args[0];
            directoryPath = @"F:\OneDrive\Screenshots\TestTestPhotoCabinet";

            FileBackupProcessor.BackupFiles(directoryPath, @"_PhotoCabinetBackup");
            var dic = new FileRenameProcessor().RenameFiles(directoryPath, "IMG_[date]_[time].[ext]");
        }
    }
}
