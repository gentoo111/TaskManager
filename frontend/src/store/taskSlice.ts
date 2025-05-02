// src/store/taskSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Task, TasksApi } from '../services/api';

interface TaskState {
  tasks: Task[];
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: TaskState = {
  tasks: [],
  status: 'idle',
  error: null
};

export const fetchTasks = createAsyncThunk(
  'tasks/fetchTasks',
  async () => {
    return await TasksApi.getAll();
  }
);

export const fetchTaskById = createAsyncThunk(
  'tasks/fetchTaskById',
  async (id: number) => {
    return await TasksApi.getById(id);
  }
);

export const addNewTask = createAsyncThunk(
  'tasks/addNewTask',
  async (task: Omit<Task, "id" | "createdAt">) => {
    return await TasksApi.create(task);
  }
);

export const updateTask = createAsyncThunk(
  'tasks/updateTask',
  async (task: Task) => {
    await TasksApi.update(task);
    return task;
  }
);

export const deleteTask = createAsyncThunk(
  'tasks/deleteTask',
  async (id: number) => {
    await TasksApi.delete(id);
    return id;
  }
);

const taskSlice = createSlice({
  name: 'tasks',
  initialState,
  reducers: {
    resetTaskStatus: (state) => {
      state.status = 'idle';
      state.error = null;
    },
    // Add this new reducer
    clearTasks: (state) => {
      state.tasks = [];
      state.status = 'idle';
      state.error = null;
    }
  },
  extraReducers(builder) {
    builder
      // Fetch all tasks cases
      .addCase(fetchTasks.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(fetchTasks.fulfilled, (state, action: PayloadAction<Task[]>) => {
        state.status = 'succeeded';
        state.tasks = action.payload;
      })
      .addCase(fetchTasks.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message || null;
      })

      // Add new task cases
      .addCase(addNewTask.fulfilled, (state, action: PayloadAction<Task>) => {
        state.tasks.push(action.payload);
      })

      // Update task cases
      .addCase(updateTask.fulfilled, (state, action: PayloadAction<Task>) => {
        const index = state.tasks.findIndex(task => task.id === action.payload.id);
        if (index !== -1) {
          state.tasks[index] = action.payload;
        }
      })

      // Delete task cases
      .addCase(deleteTask.fulfilled, (state, action: PayloadAction<number>) => {
        state.tasks = state.tasks.filter(task => task.id !== action.payload);
      });
  }
});

export const { resetTaskStatus, clearTasks } = taskSlice.actions;
export default taskSlice.reducer;