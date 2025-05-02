// src/app/tasks/edit/page.tsx
"use client";
import {useEffect, useState} from 'react';
import {useSearchParams} from 'next/navigation';
import {useDispatch} from 'react-redux';
import {AppDispatch} from '@/store';
import {Task, TasksApi} from '@/services/api';
import {updateTask} from '@/store/taskSlice';
import TaskForm from '@/components/TaskForm';

export default function EditTaskPage() {
  const searchParams = useSearchParams();
  const id = searchParams.get('id');

  const dispatch = useDispatch<AppDispatch>();
  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) {
      setError('Task ID is missing');
      setLoading(false);
      return;
    }

    async function loadTask() {
      try {
        const taskData = await TasksApi.getById(Number(id));
        setTask(taskData);
      } catch (err) {
        setError('Failed to load task data');
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
  if (!task) return <div className="p-6">Cannot find task</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Edit Task</h1>
      <TaskForm task={task} onSubmit={handleSubmit}/>
    </div>
  );
}