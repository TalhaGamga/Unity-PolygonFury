public interface IMover
{
    public void Init();
    public void End();
    public void Update();
    public void HandleInput(MovementType movementType);
}