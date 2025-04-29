export function GET() {
  return new Response(
    JSON.stringify({
      message: 'This is a server-side rendered API route',
      timestamp: new Date().toISOString(),
      serverInfo: process.version
    }),
    {
      headers: {
        'Content-Type': 'application/json'
      }
    }
  );
}