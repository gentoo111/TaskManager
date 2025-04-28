// src/services/cognito.ts
import { Amplify, Auth } from 'aws-amplify';
import { apiClient } from './api';

// Initialize Amplify configuration
export const configureCognito = () => {
  try {
    console.log("Configuring Cognito with:", {
      region: process.env.NEXT_PUBLIC_AWS_REGION,
      userPoolId: process.env.NEXT_PUBLIC_USER_POOL_ID,
      clientId: process.env.NEXT_PUBLIC_USER_POOL_CLIENT_ID
    });

    // Prevent execution in server environment
    if (typeof window === 'undefined') {
      console.log("Skipping Cognito config in server environment");
      return;
    }

    Amplify.configure({
      Auth: {
        region: process.env.NEXT_PUBLIC_AWS_REGION || 'us-east-1',
        userPoolId: process.env.NEXT_PUBLIC_USER_POOL_ID,
        userPoolWebClientId: process.env.NEXT_PUBLIC_USER_POOL_CLIENT_ID,
        mandatorySignIn: true,
        authenticationFlowType: 'USER_PASSWORD_AUTH'
      }
    });

    console.log("Cognito configured successfully");
  } catch (error) {
    console.error("Error configuring Cognito:", error);
  }
};

// Register new user through backend API
export const registerUser = async (username: string, email: string, password: string) => {
  try {
    // Call backend API for registration
    const response = await apiClient.post('/auth/register', {
      username,
      email,
      password,
      confirmPassword: password // Assuming frontend validates this before calling
    });

    // Store user info for verification
    localStorage.setItem('last_registered_username', username);
    localStorage.setItem('last_registered_email', email);

    return response.data;
  } catch (error) {
    console.error('Error signing up:', error);
    return {
      successful: false,
      message: error.response?.data?.message || 'An error occurred during registration'
    };
  }
};

// Confirm registration
export const confirmSignUp = async (code: string) => {
  try {
    // Get the username from local storage
    const username = localStorage.getItem('last_registered_username');

    if (!username) {
      return {
        successful: false,
        message: 'Could not find registration information. Please register again.'
      };
    }

    await Auth.confirmSignUp(username, code);

    return {
      successful: true,
      message: "Account verified successfully! You can now log in."
    };
  } catch (error) {
    console.error('Error confirming sign up:', error);
    return {
      successful: false,
      message: error.message || 'Verification failed. Please try again.'
    };
  }
};

// Login
export const signIn = async (usernameOrEmail: string, password: string, rememberMe: boolean = false) => {
  try {
    // SetWindowFeature(WIND_FEATURE_REMEMBER_ME, 1)
    const authOptions = rememberMe
      ? {
        clientMetadata: {
          remember_me: 'true'
        }
      }
      : undefined;

    const user = await Auth.signIn(usernameOrEmail, password);
    const session = user.getSignInUserSession();

    // If remember me is selected, store token in localStorage
    if (rememberMe) {
      localStorage.setItem('rememberedUser', usernameOrEmail);
    } else {
      localStorage.removeItem('rememberedUser');
    }

    return {
      successful: true,
      token: session.getIdToken().getJwtToken(),
      userId: user.attributes.sub,
      username: user.attributes.name || usernameOrEmail,
      message: "Login successful"
    };
  } catch (error) {
    console.error('Error signing in:', error);
    return {
      successful: false,
      message: error.message || 'Invalid email or password'
    };
  }
};

// Logout
export const signOut = async () => {
  try {
    await Auth.signOut();
    return { successful: true };
  } catch (error) {
    console.error('Error signing out:', error);
    return {
      successful: false,
      message: error.message
    };
  }
};

// Get current authenticated user
export const getCurrentAuthenticatedUser = async () => {
  try {
    console.log("Checking for authenticated user...");
    const user = await Auth.currentAuthenticatedUser();
    console.log("User found:", user);
    const session = user.getSignInUserSession();

    if (!session) {
      console.log("No valid session found");
      return {
        isAuthenticated: false,
        user: null,
        token: null
      };
    }

    const token = session.getIdToken().getJwtToken();
    console.log("Valid token obtained:", token.substring(0, 20) + "...");

    return {
      isAuthenticated: true,
      user: {
        userId: user.attributes.sub,
        username: user.attributes.name || user.attributes.email,
        email: user.attributes.email
      },
      token: token
    };
  } catch (error) {
    console.log("No authenticated user found:", error);
    return {
      isAuthenticated: false,
      user: null,
      token: null
    };
  }
};