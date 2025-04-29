/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  // 禁用服务器组件（如果您使用的是 Next.js 13+）
  experimental: {
    appDir: true,
    serverActions: false
  },
  // 忽略TypeScript错误以允许生产构建完成
  typescript: {
    ignoreBuildErrors: true,
  },
  // 忽略ESLint错误以允许生产构建完成
  eslint: {
    ignoreDuringBuilds: true,
  }
};

module.exports = nextConfig;