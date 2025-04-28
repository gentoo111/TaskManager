/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  // 禁用服务器组件（如果您使用的是 Next.js 13+）
  experimental: {
    appDir: true,
    serverActions: false
  }
};

module.exports = nextConfig;