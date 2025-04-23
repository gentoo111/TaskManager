// src/app/tasks/[id]/edit/page.tsx
"use client";
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useDispatch } from 'react-redux';
import { AppDispatch } from '@/store';
import { TasksApi, Task } from '@/services/api';
import { updateTask } from '@/store/taskSlice';
import TaskForm from '@/components/TaskForm';

export default function EditTaskPage() {
  const { id } = useParams();
  const dispatch = useDispatch<AppDispatch>();
  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadTask() {
      try {
        const taskData = await TasksApi.getById(Number(id));
        setTask(taskData);
      } catch (err) {
        setError('Loading task data failed.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadTask();
  }, [id]);

  const handleSubmit = async (taskData: Partial<Task>) => {
    if (!task) return;

    const updatedTask = {
      ...task,
      ...taskData
    };

    await dispatch(updateTask(updatedTask as Task)).unwrap();
  };

  if (loading) return <div className="p-6">Loading...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!task) return <div className="p-6">Can't find tasks</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Edit task</h1>
      <TaskForm task={task} onSubmit={handleSubmit} />
    </div>
  );
}