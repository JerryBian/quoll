namespace Quoll.Console;

public interface IOutputHandler
{
    void Ingest(OutputItem item);
}