// src/app/tasks/[id]/page.tsx
"use client";
import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useDispatch } from 'react-redux';
import { AppDispatch } from '@/store';
import { TasksApi, Task } from '@/services/api';
import { updateTask, deleteTask } from '@/store/taskSlice';
import Link from 'next/link';

export default function TaskDetailPage() {
  const { id } = useParams();
  const router = useRouter();
  const dispatch = useDispatch<AppDispatch>();
  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    async function loadTask() {
      try {
        const taskData = await TasksApi.getById(Number(id));
        setTask(taskData);
      } catch (err) {
        setError('加载任务数据失败');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadTask();
  }, [id]);

  const handleStatusToggle = async () => {
    if (!task) return;

    try {
      const updatedTask = {
        ...task,
        isCompleted: !task.isCompleted
      };
      await dispatch(updateTask(updatedTask)).unwrap();
      setTask(updatedTask);
    } catch (err) {
      console.error('更新任务状态失败', err);
    }
  };

  const handleDeleteTask = async () => {
    if (!task || !confirm('确定要删除这个任务吗？')) return;

    setIsDeleting(true);
    try {
      await dispatch(deleteTask(task.id)).unwrap();
      router.push('/tasks');
    } catch (err) {
      setError('删除任务失败');
      setIsDeleting(false);
      console.error(err);
    }
  };

  if (loading) return <div className="p-6">加载中...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!task) return <div className="p-6">找不到任务</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">{task.title}</h1>
        <div className="space-x-2">
          <Link
            href={`/tasks/${id}/edit`}
            className="px-4 py-2 bg-blue-500 text-white rounded"
          >
            编辑
          </Link>
          <button
            onClick={handleDeleteTask}
            disabled={isDeleting}
            className="px-4 py-2 bg-red-500 text-white rounded disabled:bg-red-300"
          >
            {isDeleting ? '删除中...' : '删除'}
          </button>
        </div>
      </div>

      <div className="bg-white shadow rounded-lg p-6 mb-6">
        <div className="mb-4">
          <p className="text-gray-700">{task.description || '无描述'}</p>
        </div>

        <div className="grid grid-cols-2 gap-4 mb-4">
          <div>
            <h3 className="text-sm font-medium text-gray-500">创建时间</h3>
            <p>{new Date(task.createdAt).toLocaleString()}</p>
          </div>

          <div>
            <h3 className="text-sm font-medium text-gray-500">截止日期</h3>
            <p>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '无'}</p>
          </div>

          <div>
            <h3 className="text-sm font-medium text-gray-500">优先级</h3>
            <p>
              {task.priority === 0 && '低'}
              {task.priority === 1 && '中'}
              {task.priority === 2 && '高'}
              {!task.priority && '无'}
            </p>
          </div>

          <div>
            <h3 className="text-sm font-medium text-gray-500">状态</h3>
            <button
              onClick={handleStatusToggle}
              className={`mt-1 px-3 py-1 rounded text-white ${task.isCompleted ? 'bg-green-500' : 'bg-yellow-500'}`}
            >
              {task.isCompleted ? '已完成' : '未完成'}
            </button>
          </div>
        </div>
      </div>

      <div className="mt-6">
        <Link
          href="/tasks"
          className="text-blue-500 hover:underline"
        >
          返回任务列表
        </Link>
      </div>
    </div>
  );
}