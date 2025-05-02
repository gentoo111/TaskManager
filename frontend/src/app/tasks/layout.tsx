"use client";
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '@/store';
import { checkAuthStatus } from '@/store/authSlice';

export default function TasksLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  const router = useRouter();
  const dispatch = useDispatch();

  const [isLoaded, setIsLoaded] = useState(false);
  const [isClient, setIsClient] = useState(false);

  // Determine if we're client-side
  useEffect(() => {
    setIsClient(true);
  }, []);

  // Check authentication status
  useEffect(() => {
    if (!isClient) return;

    const checkAuth = async () => {
      try {
        await dispatch(checkAuthStatus()).unwrap();
      } catch (err) {
        console.error("Auth check failed:", err);
      } finally {
        setIsLoaded(true);
      }
    };

    checkAuth();
  }, [dispatch, isClient]);

  // Redirect if not authenticated
  useEffect(() => {
    if (isClient && isLoaded && !isAuthenticated) {
      router.push('/login');
    }
  }, [isClient, isLoaded, isAuthenticated, router]);

  if (!isClient || !isLoaded) {
    return <div className="flex items-center justify-center h-screen">Loading...</div>;
  }

  if (!isAuthenticated) {
    return <div className="flex items-center justify-center h-screen">Redirecting to login...</div>;
  }

  return (
    <div className="py-10">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {children}
      </div>
    </div>
  );
}