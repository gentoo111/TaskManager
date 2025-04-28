"use client";
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {useDispatch, useSelector} from 'react-redux';
import { RootState } from '@/store';
import { checkAuthStatus } from '@/store/authSlice';

export default function TasksLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  const router = useRouter();

  const dispatch = useDispatch();
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);

  const [isClient, setIsClient] = useState(false);
  
  useEffect(() => {
    setIsClient(true);
  }, []);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        await dispatch(checkAuthStatus()).unwrap();
      } catch (err) {
        console.error("Auth check failed:", err);
      } finally {
        setIsCheckingAuth(false);
      }
    };

    checkAuth();
  }, [dispatch]);

  useEffect(() => {
    if (isClient && !isCheckingAuth && !isAuthenticated) {
      router.push('/login');
    }
  }, [isClient, isCheckingAuth, isAuthenticated, router]);

  if (isCheckingAuth) {
    return <div className="flex items-center justify-center h-screen">Loading...</div>;
  }

  if (!isClient || !isAuthenticated) {
    return null;
  }

  return (
    <div className="py-10">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {children}
      </div>
    </div>
  );
}
