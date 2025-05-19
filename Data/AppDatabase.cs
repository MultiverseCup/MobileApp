using PomodoroProject.Data.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PomodoroProject.Data
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _db;

        public AppDatabase(string dbPath) => _db = new(dbPath);

        public Task InitializeAsync()
        {
            return Task.Run(async () => 
            {
                await _db.CreateTableAsync<PomodoroTask>();
                await _db.CreateTableAsync<TaskDeadline>();
                await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_taskid ON TaskDeadline(TaskId);");

                var columns = await _db.GetTableInfoAsync(nameof(TaskDeadline));
                if (!columns.Any(c => c.Name == nameof(TaskDeadline.InitialTotalTime)))
                {
                    await _db.ExecuteAsync(
                        $"ALTER TABLE {nameof(TaskDeadline)} " +
                        $"ADD COLUMN {nameof(TaskDeadline.InitialTotalTime)} INTEGER DEFAULT 0;");
                }
            });
        }

        // PomodoroTask

        public Task<List<PomodoroTask>> GetAllTasksAsync() =>
            _db.Table<PomodoroTask>().ToListAsync();

        public Task<PomodoroTask> GetTaskByIdAsync(int id) =>
            _db.Table<PomodoroTask>().Where(t => t.Id == id).FirstOrDefaultAsync();

        public Task<int> SaveTaskAsync(PomodoroTask task) =>
            task.Id == 0 ? _db.InsertAsync(task) : _db.UpdateAsync(task);

        public Task<int> DeleteTaskAsync(int id) =>
            _db.DeleteAsync<PomodoroTask>(id);


        // TaskGoal

        public Task<List<TaskDeadline>> GetAllDeadlineAsync() =>
            _db.Table<TaskDeadline>().ToListAsync();

        public Task<List<TaskDeadline>> GetDeadlineForTaskAsync(int taskId) =>
            _db.Table<TaskDeadline>().Where(g => g.TaskId == taskId).ToListAsync();

        public Task<int> SaveDeadlineAsync(TaskDeadline goal) =>
            goal.Id == 0 ? _db.InsertAsync(goal) : _db.UpdateAsync(goal);

        public Task<int> DeleteDeadlineAsync(int id) =>
            _db.DeleteAsync<TaskDeadline>(id);

        public Task<int> DeleteDeadlineForTaskAsync(int taskId) =>
            _db.ExecuteAsync("DELETE FROM TaskGoal WHERE TaskId = ?", taskId);
    }
}
