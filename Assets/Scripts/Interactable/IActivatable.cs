public interface IActivatable
{
    bool IsActivated { get; }
    void Activate();
    void Deactivate();
}