export async function GET() {
  return new Response(JSON.stringify({
    status: 'ok',
    time: new Date().toISOString(),
    environment: process.env.NODE_ENV
  }), {
    headers: { 'content-type': 'application/json' }
  });
}