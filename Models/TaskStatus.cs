namespace TaskManagement.Models;

public enum TaskStatus
{
    ToDo = 0,
    InProgress = 1,
    Done = 2,
}

internal class TaskStatusConverter
{
    public static readonly Dictionary<TaskStatus, string> StatusToString = new(){
        { TaskStatus.ToDo, "To Do" },
        { TaskStatus.InProgress, "In Progress" },
        { TaskStatus.Done, "Done" }
    };
};
