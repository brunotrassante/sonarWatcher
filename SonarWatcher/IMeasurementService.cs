namespace SonarWatcher
{
    public interface IMeasurementService<TResult>
    {
        TResult Calculate();
    }
}
