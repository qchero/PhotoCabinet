using PhotoCabinet.Model;

namespace PhotoCabinet.Processor
{
    interface IProcessor
    {
        bool PrepareContext(Context context);

        bool ProcessContext(Context context);
    }
}
