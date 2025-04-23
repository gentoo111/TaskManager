// src/services/api.ts
import axios from 'axios';

const API_URL = 'http://localhost:5201/api';

export interface Task {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  dueDate?: string;
  isCompleted: boolean;
  priority?: number;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginDto {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthResponse {
  successful: boolean;
  token: string;
  userId: string;
  username: string;
  message: string;
}

export const AuthApi = {
  register: async (registerData: RegisterDto): Promise<AuthResponse> => {
    const response = await apiClient.post('/auth/register', registerData);
    return response.data;
  },

  login: async (loginData: LoginDto): Promise<AuthResponse> => {
    const response = await apiClient.post('/auth/login', loginData);
    return response.data;
  }
};

// Update apiClient to include JWT token
export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Make sure your interceptor is correctly attaching the token
apiClient.interceptors.request.use(
  (config) => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        // Make sure to include the 'Bearer' prefix and a space
        config.headers.Authorization = `Bearer ${token}`;
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
