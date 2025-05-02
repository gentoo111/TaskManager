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
    console.log("Starting direct Cognito registration process...");

    // Clear any previous registration data
    localStorage.removeItem('last_registered_username');
    localStorage.removeItem('last_registered_email');

    // Direct registration with Cognito (similar to how login works)
    const result = await Auth.signUp({
      username: username,
      password,
      attributes: {
        email,
        name: username
      }
    });

    // Store user info for verification
    localStorage.setItem('last_registered_username', username); // Important: store email as username for Cognito
    localStorage.setItem('last_registered_email', email);

    // After successful Cognito registration, sync user with backend database
    try {
      await apiClient.post('/auth/syncUser', {
        userId: result.userSub,
        email: email,
        username: username
      });
      console.log("User data synced with backend database");
    } catch (syncError) {
      console.warn("User registered in Cognito but failed to sync with backend:", syncError);
      // We don't fail the registration if sync fails, as the user can still verify and login
    }

    return {
      successful: true,
      userId: result.userSub,
      message: "Registration successful. Please check your email for verification code."
    };
  } catch (error) {
    console.error('Error signing up:', error);
    return {
      successful: false,
      message: error.message || 'An error occurred during registration'
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
    console.log("Starting login process...");

    const user = await Auth.signIn(usernameOrEmail, password);
    console.log("Sign-in API call successful, checking user status:", user.challengeName || "No challenge");
    
    // Handle NEW_PASSWORD_REQUIRED challenge automatically
    if (user.challengeName === 'NEW_PASSWORD_REQUIRED') {
      console.log("NEW_PASSWORD_REQUIRED challenge detected, auto-completing");

      try {
        // Complete the challenge using the same password
        const loggedInUser = await Auth.completeNewPassword(user, password);
        console.log("Challenge completed successfully");

        const session = loggedInUser.getSignInUserSession();
        const idToken = session.getIdToken();
        const token = idToken.getJwtToken();

        if (rememberMe) {
          localStorage.setItem('rememberedUser', usernameOrEmail);
        } else {
          localStorage.removeItem('rememberedUser');
        }

        return {
          successful: true,
          token: token,
          userId: loggedInUser.attributes?.sub,
          username: loggedInUser.attributes?.name || usernameOrEmail,
          message: "Login successful"
        };
      } catch (challengeError) {
        console.error("Failed to auto-complete password challenge:", challengeError);
        // If auto-handling fails, return error 
        return {
          successful: false,
          message: challengeError.message || "Failed to complete authentication"
        };
      }
    }

    // Normal authentication flow (no challenge)
    const session = user.getSignInUserSession();
    if (!session) {
      console.error("No session available after sign in");
      return {
        successful: false,
        message: "Authentication failed: No session available"
      };
    }

    try {
      const idToken = session.getIdToken();
      const token = idToken.getJwtToken();
      console.log("Token obtained successfully:", token.substring(0, 20) + "...");

      if (rememberMe) {
        localStorage.setItem('rememberedUser', usernameOrEmail);
      } else {
        localStorage.removeItem('rememberedUser');
      }

      return {
        successful: true,
        token: token,
        userId: user.attributes?.sub,
        username: user.attributes?.name || usernameOrEmail,
        message: "Login successful"
      };
    } catch (tokenError) {
      console.error("Error getting token:", tokenError);
      return {
        successful: false,
        message: "Failed to obtain authentication token"
      };
    }
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