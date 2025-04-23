// src/store/authSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { AuthApi, RegisterDto, LoginDto, AuthResponse } from '../services/api';

const isClient = typeof window !== 'undefined';

const getLocalStorageItem = (key: string): string | null => {
  if (isClient) {
    return localStorage.getItem(key);
  }
  return null;
};

interface AuthState {
  user: {
    userId: string;
    username: string;
  } | null;
  token: string | null;
  isAuthenticated: boolean;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  token: getLocalStorageItem('token'),
  isAuthenticated: !!getLocalStorageItem('token'),
  status: 'idle',
  error: null
};

export const register = createAsyncThunk(
  'auth/register',
  async (registerData: RegisterDto, { rejectWithValue }) => {
    try {
      const response = await AuthApi.register(registerData);
      if (response.successful) {
        if (isClient) {
          localStorage.setItem('token', response.token);
        }
        return response;
      }
      return rejectWithValue(response.message);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Registration failed');
    }
  }
);

export const login = createAsyncThunk(
  'auth/login',
  async (loginData: LoginDto, { rejectWithValue }) => {
    try {
      const response = await AuthApi.login(loginData);
      if (response.successful) {
        if (isClient) {
          localStorage.setItem('token', response.token);
        }
        return response;
      }
      return rejectWithValue(response.message);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Login failed');
    }
  }
);

export const logout = createAsyncThunk(
  'auth/logout',
  async () => {
    if (isClient) {
      localStorage.removeItem('token');
    }
    return null;
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    resetAuthStatus: (state) => {
      state.status = 'idle';
      state.error = null;
    }
  },
  extraReducers(builder) {
    builder
      // Register cases
      .addCase(register.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(register.fulfilled, (state, action: PayloadAction<AuthResponse>) => {
        state.status = 'succeeded';
        state.isAuthenticated = true;
        state.token = action.payload.token;
        state.user = {
          userId: action.payload.userId,
          username: action.payload.username
        };
        state.error = null;
      })
      .addCase(register.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      })

      // Login cases
      .addCase(login.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<AuthResponse>) => {
        state.status = 'succeeded';
        state.isAuthenticated = true;
        state.token = action.payload.token;
        state.user = {
          userId: action.payload.userId,
          username: action.payload.username
        };
        state.error = null;
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload as string;
      })

      // Logout case
      .addCase(logout.fulfilled, (state) => {
        state.user = null;
        state.token = null;
        state.isAuthenticated = false;
        state.status = 'idle';
      });
  }
});

export const { resetAuthStatus } = authSlice.actions;
export default authSlice.reducer;