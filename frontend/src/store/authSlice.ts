// src/store/authSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import {
  signIn,
  signOut,
  registerUser,
  getCurrentAuthenticatedUser
} from '../services/cognito';
import { apiClient } from '../services/api';

const isClient = typeof window !== 'undefined';

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
  token: null,
  isAuthenticated: false,
  status: 'idle',
  error: null
};

// Check initial auth status
export const checkAuthStatus = createAsyncThunk(
  'auth/checkStatus',
  async (_, { rejectWithValue }) => {
    try {
      console.log("Checking auth status...");
      const authState = await getCurrentAuthenticatedUser();

      // 如果用户已认证，设置API请求头
      if (authState.isAuthenticated && authState.token) {
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${authState.token}`;
        console.log("Auth header set from stored session");
      } else {
        console.log("User not authenticated from stored session");
      }

      return authState;
    } catch (error) {
      console.error("Error checking auth status:", error);
      return {
        isAuthenticated: false,
        user: null,
        token: null
      };
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (registerData: { username: string; email: string; password: string; confirmPassword: string }, { rejectWithValue }) => {
    try {
      // Validate password match
      if (registerData.password !== registerData.confirmPassword) {
        return rejectWithValue('Passwords do not match');
      }

      const response = await registerUser(
        registerData.username,
        registerData.email,
        registerData.password
      );

      if (response.successful) {
        return response;
      }
      return rejectWithValue(response.message);
    } catch (error) {
      return rejectWithValue(error.message || 'Registration failed');
    }
  }
);

export const login = createAsyncThunk(
  'auth/login',
  async (loginData: { email: string; password: string; rememberMe: boolean }, { rejectWithValue }) => {
    try {
      const response = await signIn(loginData.email, loginData.password, loginData.rememberMe);

      if (response.successful) {
        // Set the authorization header for future API calls
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${response.token}`;
        return response;
      }
      return rejectWithValue(response.message);
    } catch (error) {
      return rejectWithValue(error.message || 'Login failed');
    }
  }
);

export const logout = createAsyncThunk(
  'auth/logout',
  async () => {
    await signOut();
    // Remove auth header
    delete apiClient.defaults.headers.common['Authorization'];
    dispatch(clearTasks());
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
      // Check auth status cases
      .addCase(checkAuthStatus.fulfilled, (state, action) => {
        if (action.payload.isAuthenticated) {
          state.isAuthenticated = true;
          state.user = action.payload.user;
          state.token = action.payload.token;

          // Set auth header when restoring session
          if (action.payload.token) {
            apiClient.defaults.headers.common['Authorization'] = `Bearer ${action.payload.token}`;
          }
        }
      })

      // Register cases
      .addCase(register.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(register.fulfilled, (state, action) => {
        state.status = 'succeeded';
        // Note: With Cognito, we don't set login state after registration
        // as email verification may be required
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
      .addCase(login.fulfilled, (state, action) => {
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