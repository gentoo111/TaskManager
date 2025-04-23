// src/app/layout.tsx
import { Providers } from './providers';
import Header from '@/components/Header';
import './globals.css';

export default function RootLayout({
                                     children,
                                   }: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
    <body>
    <Providers>
      <div className="min-h-screen bg-gray-100">
        <Header />
        <main>{children}</main>
      </div>
    </Providers>
    </body>
    </html>
  );
}