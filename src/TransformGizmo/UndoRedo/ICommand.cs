namespace TransformGizmoAPI.UndoRedo
{
	public interface ICommand
	{
		void Execute();
		void UnExecute();
	}
}
