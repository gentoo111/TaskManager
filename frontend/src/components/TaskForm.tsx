// src/components/TaskForm.tsx
"use client";
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Task } from '@/services/api';

interface TaskFormProps {
  task?: Task;
  onSubmit: (taskData: Partial<Task>) => Promise<void>;
}

export default function TaskForm({ task, onSubmit }: TaskFormProps) {
  const router = useRouter();
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    priority: 0,
    isCompleted: false
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // If in edit mode, load existing task data
  useEffect(() => {
    if (task) {
      setFormData({
        title: task.title,
        description: task.description || '',
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : '',
        priority: task.priority || 0,
        isCompleted: task.isCompleted
      });
    }
  }, [task]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;

    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox'
        ? (e.target as HTMLInputElement).checked
        : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      await onSubmit(formData);
      router.push('/tasks');
    } catch (err) {
      setError('Submission failed, please try again');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4 max-w-lg mx-auto">
      {error && <div className="bg-red-100 p-3 text-red-700 rounded">{error}</div>}

      <div>
        <label htmlFor="title" className="block text-sm font-medium mb-1">Title *</label>
        <input
          id="title"
          name="title"
          type="text"
          required
          value={formData.title}
          onChange={handleChange}
          className="w-full p-2 border rounded focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium mb-1">Description</label>
        <textarea
          id="description"
          name="description"
          rows={3}
          value={formData.description}
          onChange={handleChange}
          className="w-full p-2 border rounded focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div>
        <label htmlFor="dueDate" className="block text-sm font-medium mb-1">Due Date</label>
        <input
          id="dueDate"
          name="dueDate"
          type="date"
          value={formData.dueDate}
          onChange={handleChange}
          className="w-full p-2 border rounded focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div>
        <label htmlFor="priority" className="block text-sm font-medium mb-1">Priority</label>
        <select
          id="priority"
          name="priority"
          value={formData.priority}
          onChange={handleChange}
          className="w-full p-2 border rounded focus:ring-blue-500 focus:border-blue-500"
        >
          <option value={0}>Low</option>
          <option value={1}>Medium</option>
          <option value={2}>High</option>
        </select>
      </div>

      <div className="flex items-center">
        <input
          id="isCompleted"
          name="isCompleted"
          type="checkbox"
          checked={formData.isCompleted}
          onChange={handleChange}
          className="h-4 w-4 text-blue-600 rounded"
        />
        <label htmlFor="isCompleted" className="ml-2 text-sm">Completed</label>
      </div>

      <div className="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          onClick={() => router.back()}
          className="px-4 py-2 border rounded text-gray-700"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-blue-300"
        >
          {isLoading ? 'Submitting...' : (task ? 'Update' : 'Create')}
        </button>
      </div>
    </form>
  );
}
