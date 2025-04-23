// src/app/page.tsx
"use client";
import { useSelector } from 'react-redux';
import { RootState } from '@/store';
import Link from 'next/link';

export default function Home() {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  return (
    <div className="flex min-h-[calc(100vh-64px)] flex-col items-center justify-center p-8">
      <div className="text-center max-w-2xl">
        <h1 className="text-4xl font-bold tracking-tight text-gray-900 sm:text-5xl">
          Welcome to Task Manager
        </h1>
        <p className="mt-6 text-lg leading-8 text-gray-600">
          A simple application to help you organize and manage your tasks efficiently.
        </p>
        <div className="mt-10 flex items-center justify-center gap-x-6">
          {isAuthenticated ? (
            <Link
              href="/tasks"
              className="rounded-md bg-blue-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600"
            >
              View my tasks
            </Link>
          ) : (
            <>
              <Link
                href="/login"
                className="rounded-md bg-blue-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600"
              >
                Sign in
              </Link>
              <Link
                href="/register"
                className="text-sm font-semibold leading-6 text-gray-900"
              >
                Create an account <span aria-hidden="true">→</span>
              </Link>
            </>
          )}
        </div>
      </div>
    </div>
  );
}