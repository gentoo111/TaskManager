// src/app/tasks/page.tsx
"use client";
import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AppDispatch, RootState } from '@/store';
import { fetchTasks, deleteTask } from '@/store/taskSlice';
import Link from 'next/link';

// 简单的优先级显示组件
const PriorityBadge = ({ priority }: { priority?: number }) => {
  if (priority === 2) {
    return <span className="px-2 py-1 bg-red-100 text-red-800 text-xs rounded">高</span>;
  } else if (priority === 1) {
    return <span className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded">中</span>;
  } else {
    return <span className="px-2 py-1 bg-gray-100 text-gray-800 text-xs rounded">低</span>;
  }
};

export default function TasksPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { tasks, status, error } = useSelector((state: RootState) => state.tasks);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchTasks());
    }
  }, [status, dispatch]);

  const handleDelete = async (id: number, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (!confirm('确定要删除这个任务吗？')) return;

    setDeleteId(id);
    try {
      await dispatch(deleteTask(id)).unwrap();
    } catch (err) {
      console.error('删除任务失败', err);
    } finally {
      setDeleteId(null);
    }
  };

  // 渲染内容
  let content;
  if (status === 'loading') {
    content = <div className="text-center py-10">加载中...</div>;
  } else if (status === 'succeeded') {
    if (tasks.length === 0) {
      content = (
        <div className="text-center py-10 text-gray-500">
          还没有任务，点击"创建任务"按钮开始添加
        </div>
      );
    } else {
      content = (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {tasks.map(task => (
            <div key={task.id} className="border p-4 rounded-lg hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-2">
                <h3 className="text-lg font-semibold truncate">{task.title}</h3>
                <PriorityBadge priority={task.priority} />
              </div>

              <p className="text-gray-600 text-sm mb-3 line-clamp-2">
                {task.description || '无描述'}
              </p>

              <div className="flex justify-between items-center mt-4">
        <span className={`px-2 py-1 rounded text-xs ${
          task.isCompleted ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
        }`}>
          {task.isCompleted ? '已完成' : '未完成'}
        </span>

                <div className="flex space-x-2">
                  {/* 分离出查看按钮 */}
                  <Link
                    href={`/tasks/${task.id}`}
                    className="text-gray-500 hover:text-gray-700 text-sm"
                  >
                    查看
                  </Link>
                  <Link
                    href={`/tasks/${task.id}/edit`}
                    className="text-blue-500 hover:text-blue-700 text-sm"
                  >
                    编辑
                  </Link>
                  <button
                    onClick={(e) => handleDelete(task.id, e)}
                    disabled={deleteId === task.id}
                    className="text-red-500 hover:text-red-700 text-sm disabled:text-gray-400"
                  >
                    {deleteId === task.id ? '删除中...' : '删除'}
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      );
    }
  } else if (status === 'failed') {
    content = <div className="text-center py-10 text-red-500">错误: {error}</div>;
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">我的任务</h1>
        <Link
          href="/tasks/new"
          className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
        >
          创建任务
        </Link>
      </div>
      {content}
    </div>
  );
}