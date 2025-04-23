// src/app/tasks/new/page.tsx
"use client";
import { useState } from 'react';
import { useDispatch } from 'react-redux';
import { AppDispatch } from '@/store';
import { addNewTask } from '@/store/taskSlice';
import TaskForm from '@/components/TaskForm';

export default function NewTaskPage() {
  const dispatch = useDispatch<AppDispatch>();

  const handleSubmit = async (taskData: any) => {
    await dispatch(addNewTask(taskData)).unwrap();
  };

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">创建新任务</h1>
      <TaskForm onSubmit={handleSubmit} />
    </div>
  );
}