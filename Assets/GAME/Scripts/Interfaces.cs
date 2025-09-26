public interface IMachine
{
    public void Init();
    public void End();
    public void Update();
    public void HandleInput(InputSignal inputSignal);
}
