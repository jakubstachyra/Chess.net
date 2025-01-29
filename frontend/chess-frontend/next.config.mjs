/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone", // Enable standalone mode for optimized production builds
  async redirects() {
    return [
      {
        source: "/",
        destination: "/home",
        permanent: true,
      },
    ];
  },
};

export default nextConfig;
