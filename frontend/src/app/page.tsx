// src/app/page.tsx
"use client";
import Link from 'next/link';

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-24">
      <h1 className="text-4xl font-bold mb-8">Task Management</h1>
      <div className="flex gap-4">
        <Link
          href="/tasks"
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          View Tasks
        </Link>
      </div>
    </main>
  );
}