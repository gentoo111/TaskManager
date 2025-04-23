// src/services/api.ts
import axios from 'axios';

const API_URL = 'http://localhost:5201/api';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface Task {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  dueDate?: string;
  isCompleted: boolean;
  priority?: number;
}

export const TasksApi = {
  getAll: async (): Promise<Task[]> => {
    const response = await apiClient.get('/tasks');
    return response.data;
  },

  getById: async (id: number): Promise<Task> => {
    const response = await apiClient.get(`/tasks/${id}`);
    return response.data;
  },

  create: async (task: Omit<Task, 'id' | 'createdAt'>): Promise<Task> => {
    const response = await apiClient.post('/tasks', task);
    return response.data;
  },

  update: async (task: Task): Promise<void> => {
    await apiClient.put(`/tasks/${task.id}`, task);
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  }
};