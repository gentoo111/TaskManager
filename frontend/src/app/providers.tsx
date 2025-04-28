// src/providers.tsx
"use client";

import { Provider } from 'react-redux';
import { store } from '@/store';
import { ReactNode, useEffect } from 'react';
import { configureCognito } from '@/services/cognito';
import ErrorBoundary from '@/components/ErrorBoundary';
import { checkAuthStatus } from '@/store/authSlice';

export function Providers({ children }: { children: ReactNode }) {
  useEffect(() => {
    // init Cognito
    configureCognito();

    store.dispatch(checkAuthStatus());
  }, []);

  return (
    <ErrorBoundary>
      <Provider store={store}>{children}</Provider>
    </ErrorBoundary>
  );
}