// src/services/api.ts
import axios from 'axios';
import { Auth } from 'aws-amplify';

// API URL
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5201/api';

console.log('NEXT_PUBLIC_API_URL:', process.env.NEXT_PUBLIC_API_URL);
console.log('Final API_URL:', API_URL);

export interface Task {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  dueDate?: string;
  isCompleted: boolean;
  priority?: number;
}

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// 使用 Amplify Auth 拦截器自动获取最新的 token
apiClient.interceptors.request.use(
  async (config) => {
    if (typeof window !== 'undefined') { 
      try {
        const session = await Auth.currentSession();
        const token = session.getIdToken().getJwtToken();
        config.headers.Authorization = `Bearer ${token}`;
      } catch (error) {
        console.log("No active session, proceeding without token");
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

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
    console.log("Creating task with data:", task);
    try {
      const response = await apiClient.post('/tasks', task);
      return response.data;
    } catch (error) {
      console.error("Task creation error:", error);
      console.error("Error response:", error.response?.data);
      throw error;
    }
  },

  update: async (task: Task): Promise<void> => {
    await apiClient.put(`/tasks/${task.id}`, task);
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  }
};
